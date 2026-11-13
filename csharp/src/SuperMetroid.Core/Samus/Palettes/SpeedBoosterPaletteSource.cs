using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.Core.Samus.Palettes;

/// <summary>What one active Speed Booster cycle phase copies into Samus's suit colours: one of her suit's own cycle palettes.</summary>
/// <param name="Palette">The cycle palette the phase copies.</param>
internal readonly record struct SpeedBoosterPaletteSource(SamusFullBodyCyclePalette Palette);
