namespace FertCalculator.Core.Tests;

// Points at the real data files in the repo so the tests exercise the exact
// payloads the shipped apps read and write.
internal static class TestData
{
    private static readonly string RepoRoot =
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));

    public static readonly string DesktopFertilizersXml = Path.Combine(RepoRoot, "Fertilizers.xml");
    public static readonly string DesktopMixesXml = Path.Combine(RepoRoot, "Mixes.xml");
    public static readonly string MauiFertilizersXml = Path.Combine(RepoRoot, "FertCalculatorMaui", "Fertilizers.xml");
}
