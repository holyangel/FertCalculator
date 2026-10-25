using System;
using System.Collections.Generic;

namespace FertCalculator.Core
{
    /// <summary>
    /// Summed nutrient contributions for a mix, in PPM (mg/L). When
    /// <paramref name="useImperial"/> is set, quantities are interpreted as
    /// grams per gallon instead of grams per liter.
    /// </summary>
    public class NutrientTotals
    {
        public double Nitrogen { get; set; }
        public double Phosphorus { get; set; }
        public double Potassium { get; set; }
        public double Calcium { get; set; }
        public double Magnesium { get; set; }
        public double Sulfur { get; set; }
        public double Boron { get; set; }
        public double Copper { get; set; }
        public double Iron { get; set; }
        public double Manganese { get; set; }
        public double Molybdenum { get; set; }
        public double Zinc { get; set; }
        public double Chlorine { get; set; }
        public double Silica { get; set; }
        public double HumicAcid { get; set; }
        public double FulvicAcid { get; set; }
        public double Total { get; set; }
    }

    public static class MixCalculator
    {
        public const double GallonToLiter = 3.78541;

        /// <summary>
        /// Sums the nutrient contribution of every ingredient in <paramref name="mix"/>.
        /// Fertilizer names are matched case-insensitively; ingredients whose
        /// fertilizer is not found are skipped.
        /// </summary>
        public static NutrientTotals Calculate(
            IEnumerable<FertilizerQuantity> mix,
            IEnumerable<Fertilizer> fertilizers,
            bool useImperial = false)
        {
            var totals = new NutrientTotals();
            if (mix == null || fertilizers == null)
                return totals;

            var map = new Dictionary<string, Fertilizer>(StringComparer.OrdinalIgnoreCase);
            foreach (var f in fertilizers)
            {
                if (f?.Name != null && !map.ContainsKey(f.Name))
                    map[f.Name] = f;
            }

            double factor = useImperial ? 1.0 / GallonToLiter : 1.0;

            foreach (var item in mix)
            {
                if (item == null || item.FertilizerName == null)
                    continue;
                if (!map.TryGetValue(item.FertilizerName, out var fertilizer))
                    continue;

                // For liquids the quantity is mL — scale by density to get grams.
                double quantity = item.Quantity * fertilizer.EffectiveDensity * factor;
                totals.Nitrogen += fertilizer.NitrogenPpm() * quantity;
                totals.Phosphorus += fertilizer.PhosphorusPpm() * quantity;
                totals.Potassium += fertilizer.PotassiumPpm() * quantity;
                totals.Calcium += fertilizer.CalciumPpm() * quantity;
                totals.Magnesium += fertilizer.MagnesiumPpm() * quantity;
                totals.Sulfur += fertilizer.SulfurPpm() * quantity;
                totals.Boron += fertilizer.BoronPpm() * quantity;
                totals.Copper += fertilizer.CopperPpm() * quantity;
                totals.Iron += fertilizer.IronPpm() * quantity;
                totals.Manganese += fertilizer.ManganesePpm() * quantity;
                totals.Molybdenum += fertilizer.MolybdenumPpm() * quantity;
                totals.Zinc += fertilizer.ZincPpm() * quantity;
                totals.Chlorine += fertilizer.ChlorinePpm() * quantity;
                totals.Silica += fertilizer.SilicaPpm() * quantity;
                totals.HumicAcid += fertilizer.HumicAcidPpm() * quantity;
                totals.FulvicAcid += fertilizer.FulvicAcidPpm() * quantity;
            }

            totals.Total = totals.Nitrogen + totals.Phosphorus + totals.Potassium +
                           totals.Calcium + totals.Magnesium + totals.Sulfur +
                           totals.Boron + totals.Copper + totals.Iron +
                           totals.Manganese + totals.Molybdenum + totals.Zinc +
                           totals.Chlorine + totals.Silica + totals.HumicAcid + totals.FulvicAcid;

            return totals;
        }
    }
}
