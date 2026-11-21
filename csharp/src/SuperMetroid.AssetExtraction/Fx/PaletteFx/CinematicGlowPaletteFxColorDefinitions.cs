using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>The records of the old Mother Brain lights and the gunship glow and the colours each writes, for the import.</summary>
public static class CinematicGlowPaletteFxColorDefinitions
{
    /// <summary>The old Mother Brain fight's three-color, six-frame background light pulse.</summary>
    internal static CinematicGlowPaletteFxColorDefinition OldMotherBrainFightBackgroundLights { get; } = new(ColourEffectId.OldMotherBrainFightBackgroundLights, 3);

    /// <summary>The gunship's one-color, five-frame glow.</summary>
    internal static CinematicGlowPaletteFxColorDefinition GunshipGlow { get; } = new(ColourEffectId.GunshipGlow, 1);
}
