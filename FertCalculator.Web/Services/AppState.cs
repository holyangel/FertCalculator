using System.Xml.Serialization;
using FertCalculator.Core;
using Microsoft.JSInterop;

namespace FertCalculator.Web.Services;

/// <summary>
/// Scoped app state shared across pages: the fertilizer library, saved mixes,
/// and the unit preference. Loads once per session.
///
/// The server ships a seed Mixes.xml which is always merged in. User mixes
/// live in localStorage and override seed mixes by name; deleting a seed mix
/// hides it via a persisted list so it can be restored by resetting.
/// </summary>
public class AppState
{
    private const string MixesKey = "fertcalc.mixes";
    private const string ImperialKey = "fertcalc.imperial";
    private const string CustomFertsKey = "fertcalc.customFertilizers";
    private const string HiddenMixesKey = "fertcalc.hiddenMixes";

    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private Task? _loadTask;

    public AppState(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public List<Fertilizer> Fertilizers { get; private set; } = new();
    public List<Fertilizer> CustomFertilizers { get; private set; } = new();
    public List<FertilizerMix> SeedMixes { get; private set; } = new();
    public List<FertilizerMix> UserMixes { get; private set; } = new();
    public HashSet<string> HiddenSeeds { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
    public bool UseImperial { get; private set; }
    public bool IsLoaded => _loadTask?.IsCompletedSuccessfully == true;

    /// <summary>
    /// Seed mixes (minus hidden ones), with user mixes of the same name
    /// overriding them, followed by purely local mixes.
    /// </summary>
    public List<FertilizerMix> SavedMixes =>
        SeedMixes
            .Where(s => !HiddenSeeds.Contains(s.Name))
            .Select(s => UserMixes.FirstOrDefault(u => SameName(u.Name, s.Name)) ?? s)
            .Concat(UserMixes.Where(u => !SeedMixes.Any(s => SameName(s.Name, u.Name))))
            .ToList();

    private static bool SameName(string? a, string? b) =>
        string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    public Task EnsureLoaded() => _loadTask ??= LoadAsync();

    private async Task LoadAsync()
    {
        var serializer = new XmlSerializer(typeof(List<Fertilizer>));
        await using (var stream = await _http.GetStreamAsync("data/Fertilizers.xml"))
        {
            Fertilizers = (List<Fertilizer>)serializer.Deserialize(stream)!;
        }

        var customXml = await _js.InvokeAsync<string?>("fertcalc.loadLocal", CustomFertsKey);
        if (!string.IsNullOrEmpty(customXml))
        {
            try
            {
                using var reader = new StringReader(customXml);
                CustomFertilizers = (List<Fertilizer>)serializer.Deserialize(reader)!;
                Fertilizers.AddRange(CustomFertilizers);
            }
            catch { }
        }
        Fertilizers = Fertilizers.OrderBy(f => f.Name, new FertilizerNameComparer()).ToList();

        var imperial = await _js.InvokeAsync<string?>("fertcalc.loadLocal", ImperialKey);
        UseImperial = imperial == "1";

        try
        {
            var mixSer = new XmlSerializer(typeof(List<FertilizerMix>));
            await using var stream = await _http.GetStreamAsync("data/Mixes.xml");
            SeedMixes = (List<FertilizerMix>)mixSer.Deserialize(stream)!;
        }
        catch { }

        var mixesXml = await _js.InvokeAsync<string?>("fertcalc.loadLocal", MixesKey);
        if (!string.IsNullOrEmpty(mixesXml))
        {
            var mixSer = new XmlSerializer(typeof(List<FertilizerMix>));
            using var reader = new StringReader(mixesXml);
            UserMixes = (List<FertilizerMix>)mixSer.Deserialize(reader)!;
        }

        var hidden = await _js.InvokeAsync<string?>("fertcalc.loadLocal", HiddenMixesKey);
        if (!string.IsNullOrEmpty(hidden))
            HiddenSeeds = new HashSet<string>(hidden.Split('\n'), StringComparer.OrdinalIgnoreCase);
    }

    public async Task SetImperial(bool value)
    {
        UseImperial = value;
        await _js.InvokeVoidAsync("fertcalc.saveLocal", ImperialKey, value ? "1" : "0");
    }

    /// <summary>Adds or updates a user mix. Same name as a seed = override.</summary>
    public async Task SaveUserMix(FertilizerMix mix)
    {
        UserMixes.RemoveAll(m => SameName(m.Name, mix.Name));
        UserMixes.Add(mix);
        HiddenSeeds.Remove(mix.Name);
        await PersistMixes();
    }

    /// <summary>
    /// Removes the local copy; if it's a seed mix, hides it so it stays gone
    /// until a reset.
    /// </summary>
    public async Task DeleteUserMix(string name)
    {
        UserMixes.RemoveAll(m => SameName(m.Name, name));
        if (SeedMixes.Any(s => SameName(s.Name, name)))
            HiddenSeeds.Add(name);
        await PersistMixes();
    }

    /// <summary>Drops all local mixes and un-hides seed mixes.</summary>
    public async Task ResetMixes()
    {
        UserMixes.Clear();
        HiddenSeeds.Clear();
        await PersistMixes();
    }

    /// <summary>Drops all custom fertilizers, reverting to the bundled library.</summary>
    public async Task ResetFertilizers()
    {
        CustomFertilizers.Clear();
        var serializer = new XmlSerializer(typeof(List<Fertilizer>));
        await using var stream = await _http.GetStreamAsync("data/Fertilizers.xml");
        Fertilizers = ((List<Fertilizer>)serializer.Deserialize(stream)!)
            .OrderBy(f => f.Name, new FertilizerNameComparer()).ToList();
        await PersistCustomFertilizers();
    }

    private async Task PersistMixes()
    {
        var serializer = new XmlSerializer(typeof(List<FertilizerMix>));
        using var writer = new StringWriter();
        serializer.Serialize(writer, UserMixes);
        await _js.InvokeVoidAsync("fertcalc.saveLocal", MixesKey, writer.ToString());
        await _js.InvokeVoidAsync("fertcalc.saveLocal", HiddenMixesKey, string.Join('\n', HiddenSeeds));
    }

    public bool IsCustomFertilizer(string name) =>
        CustomFertilizers.Any(f => SameName(f.Name, name));

    /// <summary>Adds or replaces a custom fertilizer and re-sorts the library.</summary>
    public async Task SaveCustomFertilizer(Fertilizer f)
    {
        CustomFertilizers.RemoveAll(x => SameName(x.Name, f.Name));
        Fertilizers.RemoveAll(x => SameName(x.Name, f.Name));
        CustomFertilizers.Add(f);
        Fertilizers.Add(f);
        Fertilizers = Fertilizers.OrderBy(x => x.Name, new FertilizerNameComparer()).ToList();
        await PersistCustomFertilizers();
    }

    public async Task RemoveCustomFertilizer(string name)
    {
        CustomFertilizers.RemoveAll(x => SameName(x.Name, name));
        Fertilizers.RemoveAll(x => SameName(x.Name, name));
        await PersistCustomFertilizers();
    }

    private async Task PersistCustomFertilizers()
    {
        var serializer = new XmlSerializer(typeof(List<Fertilizer>));
        using var writer = new StringWriter();
        serializer.Serialize(writer, CustomFertilizers);
        await _js.InvokeVoidAsync("fertcalc.saveLocal", CustomFertsKey, writer.ToString());
    }
}
