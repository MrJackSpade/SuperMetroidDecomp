namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive cinematic glow palette program.</summary>
public enum CinematicGlowPaletteFxProgramOwner
{
    /// <summary>The old Mother Brain fight's background-light pulse.</summary>
    OldMotherBrainBackgroundLights,
    /// <summary>The cinematic gunship glow.</summary>
    GunshipGlow,
}

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
public static class CinematicGlowPaletteFxProgramMechanicsDefinitions
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

    private sealed class ProgramEntries : IReadOnlyList<CinematicGlowPaletteFxProgramDefinition>
    {
        public int Count => 2;
        public CinematicGlowPaletteFxProgramDefinition this[int index] =>
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
        public IEnumerator<CinematicGlowPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>The old-Mother-Brain and gunship glow programs in definition order.</summary>
    public static IReadOnlyList<CinematicGlowPaletteFxProgramDefinition> All { get; } = new ProgramEntries();

    /// <summary>Resolves one compiled mechanics word across both programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (CinematicGlowPaletteFxProgramDefinition definition in All)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }
}

/// <summary>One complete cinematic glow control program.</summary>
public sealed class CinematicGlowPaletteFxProgramDefinition
{
    internal CinematicGlowPaletteFxProgramDefinition(
        CinematicGlowPaletteFxProgramOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex,
        int colorsPerFrame,
        ushort frameDuration)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        ColorsPerFrame = colorsPerFrame;
        FrameDuration = frameDuration;
    }

    /// <summary>The mutually exclusive glow owner.</summary>
    public CinematicGlowPaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    /// <remarks>
    /// <c>$8D:E1BC</c> owns the old Mother Brain lights and <c>$8D:E1C0</c> the gunship.
    /// </remarks>
    public ushort DefinitionPointer { get; }

    /// <summary>The native instruction-list entry.</summary>
    /// <remarks>
    /// <c>$8D:C9BA</c> is the old Mother Brain loop and <c>$8D:CA4E</c> the gunship loop.
    /// </remarks>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The number of live BGR555 colors written by each timed record.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The cartridge-authored duration of each timed record.</summary>
    public ushort FrameDuration { get; }

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public int FrameByteCount => (ColorsPerFrame + 2) * sizeof(ushort);

    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>The terminal <c>goto</c> command after all timed records.</summary>
    public ushort LoopInstructionPointer => unchecked((ushort)(FirstFramePointer +
        CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount * FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one presentation-owned BGR555 word in a timed record.</summary>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => (ushort)PaletteFxInstruction.SetColorIndex,
            var item when item == ProgramStart + 2 => ColorByteIndex,
            var item when item == LoopInstructionPointer => (ushort)PaletteFxInstruction.Goto,
            var item when item == LoopInstructionPointer + 2 => FirstFramePointer,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0;
             frame < CinematicGlowPaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => FrameDuration,
                var item when item == FrameByteCount - sizeof(ushort) =>
                    (ushort)PaletteFxInstruction.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
