using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for the old Mother Brain lights and gunship glow.</summary>
/// <remarks>
/// Issue #839 / #625: one parameterized control
/// algorithm for both definitions. After <c>SetColorIndex(index)</c>, repeat fourteen
/// records of <c>duration, colors[width], Wait</c>, then <c>Goto(first record)</c>.
/// The old Mother Brain lights use colour index 40, width 3,
/// duration 6, ten bytes per record, and loop to their first record
/// (84 frames). The gunship glow uses colour index 510, width 1,
/// duration 5, six bytes per record, and loops to its first record
/// (70 frames). All 64 control words match the original programs; the 56 intervening BGR555
/// words remain live presentation data. The compiled mechanics reader supplies only
/// those controls, while the presentation compiler supplies the colors to the
/// palette-FX runtime.
/// </remarks>
public static class CinematicGlowPaletteFxColorDefinitions
{


    /// <summary>
    /// The old-Mother-Brain and gunship glow programs in definition order: the three-color, six-frame light pulse,
    /// then the one-color, five-frame gunship glow.
    /// </summary>
    public static IReadOnlyList<CinematicGlowPaletteFxColorDefinition> All { get; } =
    [
        new()
        {
            Owner = CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights,
            Object = PaletteFxObject.OldMotherBrainFightBackgroundLights,
            ColorsPerFrame = 3,
            FrameDuration = 6,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.OldMotherBrainFightBGLights1, 3),
        },
        new()
        {
            Owner = CinematicGlowPaletteFxProgramOwner.GunshipGlow,
            Object = PaletteFxObject.GunshipGlow,
            ColorsPerFrame = 1,
            FrameDuration = 5,
            ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.GunshipGlow1, 1),
        },
    ];
}
