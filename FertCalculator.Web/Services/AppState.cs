using System.Xml.Serialization;
using FertCalculator.Core;
using Microsoft.JSInterop;

namespace FertCalculator.Web.Services;

/// <summary>
/// Scoped app state shared across pages: the fertilizer library, saved mixes,
/// and the unit preference. Loads once per session; mixes persist to
/// browser localStorage under the same keys Home.razor used previously.
/// </summary>
public class AppState
{
    private const string MixesKey = "fertcalc.mixes";
    private const string ImperialKey = "fertcalc.imperial";
    private const string CustomFertsKey = "fertcalc.customFertilizers";

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
    public List<FertilizerMix> SavedMixes { get; private set; } = new();
    public bool UseImperial { get; private set; }
    public bool IsLoaded => _loadTask?.IsCompletedSuccessfully == true;

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

        var mixesXml = await _js.InvokeAsync<string?>("fertcalc.loadLocal", MixesKey);
        if (!string.IsNullOrEmpty(mixesXml))
        {
            var mixSer = new XmlSerializer(typeof(List<FertilizerMix>));
            using var reader = new StringReader(mixesXml);
            SavedMixes = (List<FertilizerMix>)mixSer.Deserialize(reader)!;
        }
        else
        {
            // First run: seed with the bundled example mixes, if present.
            try
            {
                var mixSer = new XmlSerializer(typeof(List<FertilizerMix>));
                await using var stream = await _http.GetStreamAsync("data/Mixes.xml");
                SavedMixes = (List<FertilizerMix>)mixSer.Deserialize(stream)!;
            }
            catch
            {
                SavedMixes = new List<FertilizerMix>();
            }
        }
    }

    public async Task SetImperial(bool value)
    {
        UseImperial = value;
        await _js.InvokeVoidAsync("fertcalc.saveLocal", ImperialKey, value ? "1" : "0");
    }

    public async Task PersistMixes()
    {
        var serializer = new XmlSerializer(typeof(List<FertilizerMix>));
        using var writer = new StringWriter();
        serializer.Serialize(writer, SavedMixes);
        await _js.InvokeVoidAsync("fertcalc.saveLocal", MixesKey, writer.ToString());
    }

    public bool IsCustomFertilizer(string name) =>
        CustomFertilizers.Any(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Adds or replaces a custom fertilizer and re-sorts the library.</summary>
    public async Task SaveCustomFertilizer(Fertilizer f)
    {
        CustomFertilizers.RemoveAll(x => string.Equals(x.Name, f.Name, StringComparison.OrdinalIgnoreCase));
        Fertilizers.RemoveAll(x => string.Equals(x.Name, f.Name, StringComparison.OrdinalIgnoreCase));
        CustomFertilizers.Add(f);
        Fertilizers.Add(f);
        Fertilizers = Fertilizers.OrderBy(x => x.Name, new FertilizerNameComparer()).ToList();
        await PersistCustomFertilizers();
    }

    public async Task RemoveCustomFertilizer(string name)
    {
        CustomFertilizers.RemoveAll(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
        Fertilizers.RemoveAll(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
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
