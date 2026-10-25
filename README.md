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

- Real-time calculation of nutrient concentrations in PPM (mg/L)
- Toggle between metric (quantities in g/L) and imperial (quantities in g/gal) units
- The selected unit changes how mix quantities are interpreted: in imperial mode the entered quantity is grams per gallon, so concentrations are divided by 3.78541
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
- Quantities follow the selected unit: enter grams per liter in metric mode or grams per gallon in imperial mode. Saved mixes use whatever unit was active when the quantities were entered.
- Fertilizers marked as liquid are dosed in milliliters. Label percentages are by weight, so the app converts mL to grams using the product's density — check "Liquid" and set the density (g/mL) when adding a liquid fertilizer. If the label gives lb/gal, multiply by 0.12 to get g/mL.
- You can export your data to share with other users or as a backup.

## System Requirements

- Windows operating system
- .NET Framework 4.7.2 or higher

## Project Structure

- `FertCalculator.csproj` — WPF desktop app (.NET Framework 4.7.2)
- `FertCalculatorMaui/` — .NET MAUI mobile app (net9.0-android, net9.0-windows for local builds)
- `FertCalculator.Core/` — shared models, unit conversion, and nutrient math (.NET Standard 2.0) used by both apps
- `FertCalculator.Core.Tests/` — xUnit tests covering calculations and XML serialization compatibility

## Building

```bash
# Desktop app + shared core + tests
dotnet build FertCalculator.sln
dotnet test FertCalculator.Core.Tests/FertCalculator.Core.Tests.csproj

# Mobile app (requires the maui-android workload and an Android SDK)
dotnet workload restore FertCalculatorMaui/FertCalculatorMaui.csproj
dotnet build FertCalculatorMaui/FertCalculatorMaui.csproj -f net9.0-android
```

Data files (`Fertilizers.xml`, `Mixes.xml`) are stored next to the executable. Writes are atomic and keep a `.bak` backup of the previous version; an unreadable file is moved aside to `.corrupt` rather than being overwritten. Exported files are `ExportData` XML documents with a `Version` field — older files without a version still import on both apps.
