using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FertCalculator.Core
{
    /// <summary>
    /// Custom comparer for fertilizer names that handles numerical values within the names.
    /// Ensures that names like "Jacks 5-15-26" come before "Jacks 10-30-20".
    /// </summary>
    public class FertilizerNameComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null) return y == null ? 0 : -1;
            if (y == null) return 1;

            // Split the names into segments of numbers and non-numbers
            string[] segmentsX = SplitIntoSegments(x);
            string[] segmentsY = SplitIntoSegments(y);

            int minSegments = Math.Min(segmentsX.Length, segmentsY.Length);
            for (int i = 0; i < minSegments; i++)
            {
                string segX = segmentsX[i];
                string segY = segmentsY[i];

                bool isNumX = double.TryParse(segX, out double numX);
                bool isNumY = double.TryParse(segY, out double numY);

                // If both are numbers, compare numerically
                if (isNumX && isNumY)
                {
                    int numCompare = numX.CompareTo(numY);
                    if (numCompare != 0) return numCompare;
                }
                // If only one is a number, numbers come before text
                else if (isNumX) return -1;
                else if (isNumY) return 1;
                // If both are text, compare alphabetically
                else
                {
                    int textCompare = string.Compare(segX, segY, StringComparison.OrdinalIgnoreCase);
                    if (textCompare != 0) return textCompare;
                }
            }

            // If all compared segments are equal, the one with more segments comes later
            return segmentsX.Length.CompareTo(segmentsY.Length);
        }

        private static string[] SplitIntoSegments(string input)
        {
            return Regex.Split(input, @"(?<=\D)(?=\d)|(?<=\d)(?=\D)|[^a-zA-Z0-9]")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
        }
    }
}
