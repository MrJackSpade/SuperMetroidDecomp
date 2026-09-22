namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive blue-spore room palette owner.</summary>
public enum BrinstarBlueSporePaletteOwner
{
    StandardRooms,
    SporeSpawnRoom,
}

/// <summary>Immutable control words for the two Brinstar blue-spore palette loops.</summary>
/// <remarks>
/// Palette-FX definitions <c>$F775</c> and <c>$F779</c> contain the same live BGR555
/// sequence. The Spore Spawn variant additionally installs the area-mini-boss death
/// pre-instruction. This catalog owns setup, timing, waits, and loop control only.
/// </remarks>
public static class BrinstarBlueSporePaletteFxProgramMechanicsDefinitions
{
    private static readonly BrinstarBlueSporePaletteFxProgramDefinition[] Definitions =
    [
        new(BrinstarBlueSporePaletteOwner.StandardRooms, 0xf775, 0xed99, 0xed9d, 0xee29,
            deletesWithAreaMiniBoss: false),
        new(BrinstarBlueSporePaletteOwner.SporeSpawnRoom, 0xf779, 0xee2d, 0xee35, 0xeec1,
            deletesWithAreaMiniBoss: true),
    ];
    private static readonly IReadOnlyList<BrinstarBlueSporePaletteFxProgramDefinition>
        ReadOnlyDefinitions = Array.AsReadOnly(Definitions);

    /// <summary>Fourteen ten-frame records form one complete spore-color cycle.</summary>
    public const int FrameCount = 14;

    /// <summary>Three BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>Bytes from one duration word through its terminal wait command.</summary>
    public const int FrameByteCount = 10;

    /// <summary>The byte index of the first blue-spore color in CGRAM.</summary>
    public const ushort ColorByteIndex = 0x00e2;

    /// <summary>The standard-room and Spore Spawn variants in definition order.</summary>
    public static IReadOnlyList<BrinstarBlueSporePaletteFxProgramDefinition> All =>
        ReadOnlyDefinitions;

    /// <summary>Resolves one compiled mechanics word across both blue-spore programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (BrinstarBlueSporePaletteFxProgramDefinition definition in Definitions)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }
}

/// <summary>One room-specific entry into the fourteen-frame blue-spore palette loop.</summary>
public sealed class BrinstarBlueSporePaletteFxProgramDefinition
{
    internal BrinstarBlueSporePaletteFxProgramDefinition(
        BrinstarBlueSporePaletteOwner owner,
        ushort definitionPointer,
        ushort programStart,
        ushort firstFramePointer,
        ushort loopInstructionPointer,
        bool deletesWithAreaMiniBoss)
    {
        Owner = owner;
        DefinitionPointer = definitionPointer;
        ProgramStart = programStart;
        FirstFramePointer = firstFramePointer;
        LoopInstructionPointer = loopInstructionPointer;
        DeletesWithAreaMiniBoss = deletesWithAreaMiniBoss;
    }

    /// <summary>The room family represented by this program.</summary>
    public BrinstarBlueSporePaletteOwner Owner { get; }

    /// <summary>The palette-FX definition identity that installs this program.</summary>
    public ushort DefinitionPointer { get; }

    /// <summary>The setup entry for this program.</summary>
    public ushort ProgramStart { get; }

    /// <summary>The first timed color record.</summary>
    public ushort FirstFramePointer { get; }

    /// <summary>The terminal <c>goto</c> command after the fourteenth record.</summary>
    public ushort LoopInstructionPointer { get; }

    /// <summary>Whether this owner installs the area-mini-boss death callback.</summary>
    public bool DeletesWithAreaMiniBoss { get; }

    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer +
            frame * BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Reads one mechanics word while excluding live BGR555 colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        int setupOffset = pointer - ProgramStart;
        ushort? setupWord = setupOffset switch
        {
            0 when DeletesWithAreaMiniBoss => PaletteFxInstructionCodes.SetPreInstruction,
            2 when DeletesWithAreaMiniBoss =>
                PaletteFxPreInstructionCodes.DeleteWhenAreaMiniBossDies,
            4 when DeletesWithAreaMiniBoss => PaletteFxInstructionCodes.SetColorIndex,
            6 when DeletesWithAreaMiniBoss =>
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            0 => PaletteFxInstructionCodes.SetColorIndex,
            2 => BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            _ => null,
        };
        if (setupWord.HasValue)
        {
            value = setupWord.Value;
            return true;
        }

        if (pointer == LoopInstructionPointer)
        {
            value = PaletteFxInstructionCodes.Goto;
            return true;
        }
        if (pointer == unchecked((ushort)(LoopInstructionPointer + sizeof(ushort))))
        {
            value = FirstFramePointer;
            return true;
        }

        for (int frame = 0;
             frame < BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameCount;
             frame++)
        {
            ushort framePointer = FramePointer(frame);
            if (pointer == framePointer)
            {
                value = 10;
                return true;
            }
            if (pointer == unchecked((ushort)(framePointer +
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameByteCount -
                sizeof(ushort))))
            {
                value = PaletteFxInstructionCodes.Wait;
                return true;
            }
        }

        value = 0;
        return false;
    }
}
