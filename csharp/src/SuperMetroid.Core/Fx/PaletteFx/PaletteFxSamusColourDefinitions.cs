using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// The palette-FX colour groups that recolour Samus's suit in gameplay (#1512): her glow in a heated room and her suit
/// materializing as she loads. She lays the shade of each record they show over her suit; which of her colours each
/// recolours is the import's.
/// </summary>
internal static class PaletteFxSamusColourDefinitions
{
    /// <summary>The groups that recolour her suit.</summary>
    internal static ImmutableHashSet<PaletteFxColours> SuitColours { get; } =
        [PaletteFxColours.HeatSuitGlow, PaletteFxColours.SamusLoadingSuit];
}
