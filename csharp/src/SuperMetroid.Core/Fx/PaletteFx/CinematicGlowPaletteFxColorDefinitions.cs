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
    /// <summary>Both glow loops contain fourteen timed records.</summary>
    public const int FrameCount = 14;

    /// <summary>$8D:E1BC: old Mother Brain background-light palette-FX definition.</summary>
    private const ushort MotherBrainDefinition = 0xe1bc;
    /// <summary>$8D:C9BA: old Mother Brain three-color pulse program.</summary>
    private const ushort MotherBrainProgram = 0xc9ba;
    /// <summary>$0028: CGRAM byte destination of the old Mother Brain lights.</summary>
    private const ushort MotherBrainColorByte = 0x0028;
    /// <summary>$8D:E1C0: cinematic gunship-glow palette-FX definition.</summary>
    private const ushort GunshipDefinition = 0xe1c0;
    /// <summary>$8D:CA4E: cinematic gunship one-color pulse program.</summary>
    private const ushort GunshipProgram = 0xca4e;
    /// <summary>$01FE: CGRAM byte destination of the cinematic gunship glow.</summary>
    private const ushort GunshipColorByte = 0x01fe;

    /// <summary>Calculates both glow definitions in native definition order without retaining an array.</summary>
    private sealed class ProgramEntries : IReadOnlyList<CinematicGlowPaletteFxColorDefinition>
    {
        /// <summary>One old Mother Brain light loop and one cinematic gunship loop.</summary>
        public int Count => 2;
        /// <summary>Selects the three-color, six-frame light pulse or one-color, five-frame gunship glow.</summary>
        public CinematicGlowPaletteFxColorDefinition this[int index] =>
            (CinematicGlowPaletteFxProgramOwner)index switch
            {
                CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights => new(
                    CinematicGlowPaletteFxProgramOwner.OldMotherBrainBackgroundLights,
                    MotherBrainDefinition, MotherBrainProgram, MotherBrainColorByte,
                    colorsPerFrame: 3, frameDuration: 6),
                CinematicGlowPaletteFxProgramOwner.GunshipGlow => new(
                    CinematicGlowPaletteFxProgramOwner.GunshipGlow,
                    GunshipDefinition, GunshipProgram, GunshipColorByte,
                    colorsPerFrame: 1, frameDuration: 5),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        /// <summary>Yields the Mother Brain then gunship glow program definitions.</summary>
        public IEnumerator<CinematicGlowPaletteFxColorDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Enumerates the same program pair through the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>The old-Mother-Brain and gunship glow programs in definition order.</summary>
    public static IReadOnlyList<CinematicGlowPaletteFxColorDefinition> All { get; } = new ProgramEntries();
}
