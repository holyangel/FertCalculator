# FertCalculator

## Overview

FertCalculator is a comprehensive tool for managing and calculating nutrient concentrations in hydroponic fertilizer mixes. This application allows users to create, save, and compare different fertilizer mixes while providing real-time nutrient concentration calculations.

## Version

Current Version: 2.0

## Features

### Fertilizer Management

- Create and maintain a library of fertilizers with their nutrient compositions
- Edit existing fertilizers to update nutrient percentages
- Alphabetical sorting of fertilizers for easy navigation
- Support for a wide range of nutrients:
  - Primary: Nitrogen (N), Phosphorus (P), Potassium (K)
  - Secondary: Calcium (Ca), Magnesium (Mg), Sulfur (S)
  - Micronutrients: Boron (B), Copper (Cu), Iron (Fe), Manganese (Mn), Molybdenum (Mo), Zinc (Zn)
  - Additional: Chlorine (Cl), Silica (Si), Humic Acid, Fulvic Acid

### Mix Creation and Management

- Create custom fertilizer mixes by combining multiple fertilizers
- Specify quantities for each fertilizer in the mix
- Save mixes for future reference
- Load saved mixes to continue working on them
- Delete mixes that are no longer needed
- Double-click on a fertilizer name in the mix to remove it

### Nutrient Calculations

- Real-time calculation of nutrient concentrations in PPM
- Toggle between metric (PPM per liter) and imperial (PPM per gallon) units
- Automatic conversion between unit systems
- Total PPM calculation for overall nutrient concentration

### Mix Comparison

- Compare two different saved mixes side by side
- View nutrient profiles of both mixes simultaneously
- Compare ingredient lists between mixes
- Toggle between metric and imperial units in the comparison view

### Data Import/Export

- Export your fertilizers and mixes to share with others
- Import fertilizers and mixes from other users
- Selective import options (fertilizers only, mixes only, or both)
- Duplicate prevention during import
- Validation of mix ingredients during import

## Getting Started

1. **Add Fertilizers**: Start by adding fertilizers to your library using the "Add" button in the Fertilizers section.
2. **Create a Mix**: Select fertilizers from your library and add them to your mix with specified quantities.
3. **View Nutrient Concentrations**: The application automatically calculates and displays nutrient concentrations in real-time.
4. **Save Your Mix**: Give your mix a name and save it for future reference.
5. **Compare Mixes**: Use the "Compare" button to compare different saved mixes.

## Tips

- When entering nutrient percentages for fertilizers, be sure to use dropdown to select the proper form according to your nutrient label. Most US labels will list phosphorous (P) and potassium (K) in their oxide form, which is the preselected drop down.
- Conversion formulas:
  - P₂O₅ to P: multiply by 0.4364
  - K₂O to K: multiply by 0.8301
- The application maintains alphabetical order for fertilizers, making them easier to find.
- You can export your data to share with other users or as a backup.

## System Requirements

- Windows operating system
- .NET Framework 4.7.2 or higher
