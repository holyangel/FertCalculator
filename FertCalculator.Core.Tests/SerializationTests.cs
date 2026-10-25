using System.Xml.Serialization;
using FertCalculator.Core;
using Xunit;

namespace FertCalculator.Core.Tests;

public class SerializationTests
{
    private static T Deserialize<T>(string path)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StreamReader(path);
        return (T)serializer.Deserialize(reader)!;
    }

    private static string Serialize<T>(T data)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var writer = new StringWriter();
        serializer.Serialize(writer, data);
        return writer.ToString();
    }

    [Fact]
    public void DesktopFertilizersXml_Deserializes()
    {
        var fertilizers = Deserialize<List<Fertilizer>>(TestData.DesktopFertilizersXml);

        Assert.NotEmpty(fertilizers);
        Assert.All(fertilizers, f => Assert.False(string.IsNullOrEmpty(f.Name)));
    }

    [Fact]
    public void DesktopMixesXml_Deserializes()
    {
        var mixes = Deserialize<List<FertilizerMix>>(TestData.DesktopMixesXml);

        Assert.NotNull(mixes);
    }

    [Fact]
    public void MauiExportDataXml_Deserializes()
    {
        var data = Deserialize<ExportData>(TestData.MauiFertilizersXml);

        Assert.NotEmpty(data.Fertilizers);
    }

    [Fact]
    public void ExportData_SerializesWithVersion_AndRoundTrips()
    {
        var data = new ExportData
        {
            Fertilizers = { new Fertilizer { Name = "Urea", NitrogenPercent = 46 } },
            Mixes =
            {
                new FertilizerMix
                {
                    Name = "Mix A",
                    Ingredients = { new FertilizerQuantity { FertilizerName = "Urea", Quantity = 1.5 } }
                }
            }
        };

        string xml = Serialize(data);
        Assert.Contains("<Version>1</Version>", xml);
        Assert.Contains("<Name>Urea</Name>", xml);

        var serializer = new XmlSerializer(typeof(ExportData));
        using var reader = new StringReader(xml);
        var roundTripped = (ExportData)serializer.Deserialize(reader)!;

        Assert.Equal(1, roundTripped.Version);
        Assert.Single(roundTripped.Fertilizers);
        Assert.Equal("Urea", roundTripped.Fertilizers[0].Name);
        Assert.Equal(46, roundTripped.Fertilizers[0].NitrogenPercent);
        Assert.Single(roundTripped.Mixes);
        Assert.Equal(1.5, roundTripped.Mixes[0].Ingredients[0].Quantity);
    }

    [Fact]
    public void ExportData_LegacyFileWithoutVersion_DeserializesWithDefaultVersion()
    {
        const string legacyXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ExportData xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Fertilizers>
    <Fertilizer>
      <IsPhosphorusInOxideForm>true</IsPhosphorusInOxideForm>
      <IsPotassiumInOxideForm>true</IsPotassiumInOxideForm>
      <OriginalPhosphorusValue>52</OriginalPhosphorusValue>
      <OriginalPotassiumValue>34</OriginalPotassiumValue>
      <Name>MKP</Name>
      <NitrogenPercent>0</NitrogenPercent>
      <PhosphorusPercent>22.6928</PhosphorusPercent>
      <PotassiumPercent>28.2234</PotassiumPercent>
    </Fertilizer>
  </Fertilizers>
  <Mixes />
</ExportData>";

        var serializer = new XmlSerializer(typeof(ExportData));
        using var reader = new StringReader(legacyXml);
        var data = (ExportData)serializer.Deserialize(reader)!;

        Assert.Equal(ExportData.CurrentVersion, data.Version);
        Assert.Single(data.Fertilizers);
        Assert.Equal("MKP", data.Fertilizers[0].Name);
        Assert.True(data.Fertilizers[0].IsPhosphorusInOxideForm);
        Assert.Empty(data.Mixes);
    }

    [Fact]
    public void Fertilizer_XmlPropertyNames_AreStable()
    {
        // The serialized element names are the cross-app contract — guard them.
        string xml = Serialize(new List<Fertilizer>
        {
            new Fertilizer { Name = "X", SilicaPercent = 1, HumicAcidPercent = 2, FulvicAcidPercent = 3 }
        });

        foreach (var element in new[]
        {
            "Name", "NitrogenPercent", "PhosphorusPercent", "PotassiumPercent",
            "CalciumPercent", "MagnesiumPercent", "SulfurPercent", "BoronPercent",
            "CopperPercent", "IronPercent", "ManganesePercent", "MolybdenumPercent",
            "ZincPercent", "ChlorinePercent", "SilicaPercent",
            "HumicAcidPercent", "FulvicAcidPercent",
            "IsPhosphorusInOxideForm", "IsPotassiumInOxideForm",
            "OriginalPhosphorusValue", "OriginalPotassiumValue"
        })
        {
            Assert.Contains($"<{element}>", xml);
        }
    }

    [Fact]
    public void Fertilizer_LegacyFileWithoutLiquidFields_DefaultsToSolid()
    {
        const string legacyXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ArrayOfFertilizer xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <Fertilizer>
    <Name>Urea</Name>
    <NitrogenPercent>46</NitrogenPercent>
  </Fertilizer>
</ArrayOfFertilizer>";

        var serializer = new XmlSerializer(typeof(List<Fertilizer>));
        using var reader = new StringReader(legacyXml);
        var fertilizers = (List<Fertilizer>)serializer.Deserialize(reader)!;

        Assert.False(fertilizers[0].IsLiquid);
        Assert.Equal(1.0, fertilizers[0].Density);
        Assert.Equal(1.0, fertilizers[0].EffectiveDensity);
    }

    [Fact]
    public void FertilizerQuantityAndMix_XmlPropertyNames_AreStable()
    {
        string xml = Serialize(new List<FertilizerMix>
        {
            new FertilizerMix
            {
                Name = "M",
                Ingredients = { new FertilizerQuantity { FertilizerName = "Urea", Quantity = 2 } }
            }
        });

        Assert.Contains("<FertilizerName>", xml);
        Assert.Contains("<Quantity>", xml);
        Assert.Contains("<Ingredients>", xml);
        Assert.Contains("<Name>", xml);
    }
}
