using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the Zebes explosion's afterglow and lava and the colours each writes, for the import.</summary>
/// <remarks>
/// The afterglow's colours rise and fall back through the same authored rows; the lava's single red colour dims and
/// brightens again. Both describe the authored colours, not arithmetic the game performs.
/// </remarks>
public static class ZebesExplosionAmbientPaletteFxColorDefinitions
{
    /// <summary>The planet's afterglow.</summary>
    internal static ZebesExplosionAmbientPaletteFxColorDefinition ZebesExplosionPlanetAfterglow { get; } = new(ColourEffectId.ZebesExplosionPlanetAfterglow, 8);

    /// <summary>The exploding planet's lava.</summary>
    internal static ZebesExplosionAmbientPaletteFxColorDefinition ExplodingZebesLava { get; } = new(ColourEffectId.ExplodingZebesLava, 1);

    /// <summary>The afterglow and lava programs in definition order.</summary>
    public static IReadOnlyList<ZebesExplosionAmbientPaletteFxColorDefinition> All { get; } = [ZebesExplosionPlanetAfterglow, ExplodingZebesLava];
}
