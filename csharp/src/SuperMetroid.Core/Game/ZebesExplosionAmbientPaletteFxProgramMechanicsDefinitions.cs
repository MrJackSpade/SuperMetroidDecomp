namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive persistent Zebes-explosion ambient palette loop.</summary>
public enum ZebesExplosionAmbientPaletteFxProgramOwner
{
    /// <summary>The planet's post-explosion afterglow.</summary>
    PlanetAfterglow,
    /// <summary>The exposed lava pulse.</summary>
    Lava,
}

/// <summary>Immutable mechanics for the Zebes explosion's afterglow and lava loops.</summary>
/// <remarks>
/// Definitions <c>$E1D4</c> and <c>$E1D8</c> retain distinct record counts, widths,
/// and duration schedules. Their 58 BGR555 words remain live presentation data; this
/// catalog owns palette placement, timing, waits, and loop control.
/// </remarks>
public static class ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions
{
    private static readonly ushort[] LavaDurations = [9, 8, 7, 6, 5, 5, 6, 7, 8, 9];

    private static readonly ZebesExplosionAmbientPaletteFxProgramDefinition[] Definitions =
    [
        new(
            ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow,
            definitionPointer: 0xe1d4,
            programStart: 0xd3ca,
            colorByteIndex: 0x01c2,
            frameCount: 6,
            colorsPerFrame: 8,
            cycleFrames: 96),
        new(
            ZebesExplosionAmbientPaletteFxProgramOwner.Lava,
            definitionPointer: 0xe1d8,
            programStart: 0xd44a,
            colorByteIndex: 0x0080,
            frameCount: 10,
            colorsPerFrame: 1,
            cycleFrames: 70),
    ];
    private static readonly IReadOnlyList<ZebesExplosionAmbientPaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>The afterglow and lava programs in definition order.</summary>
    public static IReadOnlyList<ZebesExplosionAmbientPaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves one compiled mechanics word across both programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (ZebesExplosionAmbientPaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }

    internal static ushort Duration(
        ZebesExplosionAmbientPaletteFxProgramOwner owner,
        int frame) => owner == ZebesExplosionAmbientPaletteFxProgramOwner.PlanetAfterglow
            ? (ushort)16
            : LavaDurations[frame];
}

/// <summary>One complete persistent Zebes-explosion ambient palette loop.</summary>
public sealed class ZebesExplosionAmbientPaletteFxProgramDefinition
{
    internal ZebesExplosionAmbientPaletteFxProgramDefinition(
        ZebesExplosionAmbientPaletteFxProgramOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort colorByteIndex,
        int frameCount,
        int colorsPerFrame,
        int cycleFrames)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        ColorByteIndex = colorByteIndex;
        FrameCount = frameCount;
        ColorsPerFrame = colorsPerFrame;
        CycleFrames = cycleFrames;
    }

    /// <summary>The mutually exclusive ambient owner.</summary>
    public ZebesExplosionAmbientPaletteFxProgramOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    /// <remarks><c>$8D:E1D4</c> is afterglow and <c>$8D:E1D8</c> is lava.</remarks>
    public ushort DefinitionPointer { get; }

    /// <summary>The native instruction-list entry.</summary>
    /// <remarks><c>$8D:D3CA</c> is afterglow and <c>$8D:D44A</c> is lava.</remarks>
    public ushort ProgramStart { get; }

    /// <summary>The first destination byte in CGRAM.</summary>
    public ushort ColorByteIndex { get; }

    /// <summary>The number of timed records in the loop.</summary>
    public int FrameCount { get; }

    /// <summary>The number of live BGR555 colors written by each timed record.</summary>
    public int ColorsPerFrame { get; }

    /// <summary>The complete loop duration in frames.</summary>
    public int CycleFrames { get; }

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public int FrameByteCount => (ColorsPerFrame + 2) * sizeof(ushort);

    /// <summary>The first timed record after color-index setup.</summary>
    public ushort FirstFramePointer => unchecked((ushort)(ProgramStart + 4));

    /// <summary>The terminal <c>goto</c> command after all timed records.</summary>
    public ushort LoopInstructionPointer => unchecked((ushort)(FirstFramePointer +
        FrameCount * FrameByteCount));

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns the cartridge-authored duration for one timed record.</summary>
    public ushort FrameDuration(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return ZebesExplosionAmbientPaletteFxProgramMechanicsDefinitions.Duration(Owner, frame);
    }

    /// <summary>Resolves one compiled mechanics word while excluding live colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        value = pointer switch
        {
            var item when item == ProgramStart => PaletteFxInstructionCodes.SetColorIndex,
            var item when item == ProgramStart + 2 => ColorByteIndex,
            var item when item == LoopInstructionPointer => PaletteFxInstructionCodes.Goto,
            var item when item == LoopInstructionPointer + 2 => FirstFramePointer,
            _ => 0,
        };
        if (value != 0)
            return true;

        for (int frame = 0; frame < FrameCount; frame++)
        {
            int offset = pointer - FramePointer(frame);
            value = offset switch
            {
                0 => FrameDuration(frame),
                var item when item == FrameByteCount - sizeof(ushort) =>
                    PaletteFxInstructionCodes.Wait,
                _ => 0,
            };
            if (value != 0)
                return true;
        }

        return false;
    }
}
