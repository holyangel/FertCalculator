using FertCalculator.Core;
using Xunit;

namespace FertCalculator.Core.Tests;

public class CalculationTests
{
    [Fact]
    public void P2O5ToP_UsesCorrectAtomicRatio()
    {
        Assert.Equal(43.64, Fertilizer.P2O5ToP(100.0), 4);
    }

    [Fact]
    public void K2OToK_UsesCorrectAtomicRatio()
    {
        Assert.Equal(83.01, Fertilizer.K2OToK(100.0), 4);
    }

    [Fact]
    public void Ppm_PureNutrient_Is1000PerGram()
    {
        var fertilizer = new Fertilizer { NitrogenPercent = 100 };
        // 100% of 1 g is 1000 mg -> the PPM-per-gram scale used by the apps
        Assert.Equal(1000, fertilizer.NitrogenPpm(), 6);
    }

    [Fact]
    public void Ppm_Imperial_ScalesByGallonToLiter()
    {
        var fertilizer = new Fertilizer { NitrogenPercent = 10 };
        double metric = fertilizer.NitrogenPpm();
        double imperial = fertilizer.NitrogenPpm(useImperial: true);
        Assert.Equal(metric / MixCalculator.GallonToLiter, imperial, 9);
    }

    [Fact]
    public void Calculate_SingleIngredient_SumsCorrectly()
    {
        var fertilizer = new Fertilizer { Name = "Test", NitrogenPercent = 10, PotassiumPercent = 20 };
        var mix = new List<FertilizerQuantity>
        {
            new FertilizerQuantity { FertilizerName = "Test", Quantity = 2.0 }
        };

        var totals = MixCalculator.Calculate(mix, new[] { fertilizer });

        Assert.Equal(200, totals.Nitrogen, 6);   // 100 mg/g * 2 g/L
        Assert.Equal(400, totals.Potassium, 6);
        Assert.Equal(600, totals.Total, 6);
    }

    [Fact]
    public void Calculate_Imperial_TreatsQuantityAsPerGallon()
    {
        var fertilizer = new Fertilizer { Name = "Test", NitrogenPercent = 10 };
        var mix = new List<FertilizerQuantity>
        {
            new FertilizerQuantity { FertilizerName = "Test", Quantity = 3.78541 }
        };

        var totals = MixCalculator.Calculate(mix, new[] { fertilizer }, useImperial: true);

        // 3.78541 g/gal == 1 g/L -> same as metric 1 g/L
        Assert.Equal(100, totals.Nitrogen, 4);
    }

    [Fact]
    public void Calculate_MatchesFertilizerNamesCaseInsensitively()
    {
        var fertilizer = new Fertilizer { Name = "Urea", NitrogenPercent = 46 };
        var mix = new List<FertilizerQuantity>
        {
            new FertilizerQuantity { FertilizerName = "urea", Quantity = 1.0 }
        };

        var totals = MixCalculator.Calculate(mix, new[] { fertilizer });

        Assert.Equal(460, totals.Nitrogen, 6);
    }

    [Fact]
    public void Calculate_SkipsMissingFertilizers()
    {
        var mix = new List<FertilizerQuantity>
        {
            new FertilizerQuantity { FertilizerName = "Ghost", Quantity = 5.0 }
        };

        var totals = MixCalculator.Calculate(mix, Array.Empty<Fertilizer>());

        Assert.Equal(0, totals.Total);
    }

    [Fact]
    public void Calculate_Liquid_AppliesDensity()
    {
        // 5% N liquid at SG 1.22, dosed 5 mL/L -> 5 mL * 1.22 g/mL * 50 mg/g = 305 PPM
        var fertilizer = new Fertilizer { Name = "Liquid", NitrogenPercent = 5, IsLiquid = true, Density = 1.22 };
        var mix = new List<FertilizerQuantity>
        {
            new FertilizerQuantity { FertilizerName = "Liquid", Quantity = 5.0 }
        };

        var totals = MixCalculator.Calculate(mix, new[] { fertilizer });

        Assert.Equal(305, totals.Nitrogen, 4);
    }

    [Fact]
    public void Calculate_Solid_IgnoresDensityFields()
    {
        var fertilizer = new Fertilizer { Name = "Solid", NitrogenPercent = 5, IsLiquid = false, Density = 1.5 };
        var mix = new List<FertilizerQuantity>
        {
            new FertilizerQuantity { FertilizerName = "Solid", Quantity = 5.0 }
        };

        var totals = MixCalculator.Calculate(mix, new[] { fertilizer });

        Assert.Equal(250, totals.Nitrogen, 4);
    }

    [Fact]
    public void Calculate_ZeroOrMissingDensity_FallsBackToOne()
    {
        var fertilizer = new Fertilizer { Name = "Liquid", NitrogenPercent = 5, IsLiquid = true, Density = 0 };
        var mix = new List<FertilizerQuantity>
        {
            new FertilizerQuantity { FertilizerName = "Liquid", Quantity = 2.0 }
        };

        var totals = MixCalculator.Calculate(mix, new[] { fertilizer });

        Assert.Equal(100, totals.Nitrogen, 4);
    }

    [Fact]
    public void Calculate_NullOrEmptyInput_ReturnsZeros()
    {
        Assert.Equal(0, MixCalculator.Calculate(null, null).Total);
        Assert.Equal(0, MixCalculator.Calculate(new List<FertilizerQuantity>(), new List<Fertilizer>()).Total);
    }
}
