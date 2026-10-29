namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the old Mother Brain lights and gunship glow.</summary>
/// <remarks>
/// Issue #839 / #625: the pinned NTSC J/U v1.0 ROM gives one parameterized control
/// algorithm for both definitions. After <c>SetColorIndex(index)</c>, repeat fourteen
/// records of <c>duration, colors[width], Wait</c>, then <c>Goto(first record)</c>.
/// At <c>$8D:C9BA</c>, old Mother Brain lights use index <c>$0028</c>, width 3,
/// duration 6, ten bytes per record, and loop from <c>$CA4A</c> to <c>$C9BE</c>
/// (84 frames). At <c>$8D:CA4E</c>, gunship glow uses index <c>$01FE</c>, width 1,
/// duration 5, six bytes per record, and loop from <c>$CAA6</c> to <c>$CA52</c>
/// (70 frames). All 64 control words match the ROM; the 56 intervening BGR555
/// words remain live presentation data. The compiled mechanics reader supplies only
/// those controls, while the presentation compiler supplies the colors to the
/// palette-FX runtime. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
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
            Rows = PaletteFxColorRows.Of(PaletteFxProgram.OldMotherBrainFightBGLights1, 3),
        },
        new()
        {
            Owner = CinematicGlowPaletteFxProgramOwner.GunshipGlow,
            Object = PaletteFxObject.GunshipGlow,
            ColorsPerFrame = 1,
            FrameDuration = 5,
            Rows = PaletteFxColorRows.Of(PaletteFxProgram.GunshipGlow1, 1),
        },
    ];
}
