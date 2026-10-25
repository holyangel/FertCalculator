# FertCalculator Mobile App

## Overview
FertCalculator Mobile is a comprehensive fertilizer calculator app for Android that helps you manage fertilizers and create custom nutrient mixes. This mobile version brings all the functionality of the desktop application to your smartphone or tablet.

## Features

### Fertilizer Management
- Create and store custom fertilizers with detailed nutrient content
- Edit existing fertilizers to update nutrient percentages
- View a list of all available fertilizers
- Sort fertilizers alphabetically for easy navigation

### Mix Creation
- Add fertilizers to a custom mix with specified quantities
- Calculate nutrient totals based on the mix composition
- Save mixes for future reference
- Load saved mixes to adjust or view them
- Replace or add to the current mix when loading a saved mix

### Nutrient Calculations
- View precise nutrient percentages and PPM values for all nutrients:
  - Nitrogen (N)
  - Phosphorus (P)
  - Potassium (K)
  - Calcium (Ca)
  - Magnesium (Mg)
  - Sulfur (S)
  - Boron (B)
  - Copper (Cu)
  - Iron (Fe)
  - Manganese (Mn)
  - Molybdenum (Mo)
  - Zinc (Zn)
  - Chlorine (Cl)
  - Silica (Si)
  - Humic Acid
  - Fulvic Acid
- Support for both elemental and oxide forms (P₂O₅, K₂O)
- Automatic conversion formulas:
  - P₂O₅ to P: multiply by 0.4364
  - K₂O to K: multiply by 0.8301

### Unit Conversion
- Toggle between metric (quantities in g/L) and imperial (quantities in g/gal) units
- PPM is always reported as mg/L; in imperial mode the same gram quantity is treated as a per-gallon dose, so PPM values are divided by 3.78541 (1 gallon = 3.78541 liters)
- Settings are saved across sessions

### Mix Comparison
- Compare two different mixes side by side
- View nutrient differences between mixes
- Identify nutrient deficiencies or excesses

### Data Import/Export
- Export your fertilizers and mixes to XML files
- Import fertilizers and mixes from XML files (both export files and raw Fertilizers.xml / Mixes.xml database files are supported)
- Options for handling duplicates during import
- Share your data with other users, including the Windows desktop version

## Getting Started

1. Launch the FertCalculator app on your mobile device
2. Add your fertilizers by tapping on "Add New Fertilizer" 
3. Select fertilizers from the list to add them to your current mix
4. Adjust the quantities as needed
5. View the nutrient totals to see the effects of your changes
6. Save your mix for future reference
7. Use the toggle switch to change between metric and imperial units

## System Requirements
- Android 10 (API 29) or higher
- iOS is not currently targeted; the project can be extended to other .NET MAUI platforms

## Support and Feedback
For support or feedback, please contact us at support@fertcalculator.com

## Version History
- v1.0 - Initial release of the mobile app with feature parity to the desktop version
