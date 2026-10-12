namespace SuperMetroid.Core.Game;

/// <summary>The mutually exclusive blue-spore room palette owner.</summary>
public enum BrinstarBlueSporePaletteOwner
{
    /// <summary>Palette-FX object $8D:F775, native Brinstar 1: the $ED99 loop animates BG palette 7 colors 1 through 3 in ordinary blue-spore rooms, without a boss-death pre-instruction.</summary>
    StandardRooms,
    /// <summary>Palette-FX object $8D:F779, native Brinstar 8: the $EE2D variant uses the same spore colors but installs $EEC5 to delete the object when the current area's mini-boss defeat bit ($0002) is set.</summary>
    SporeSpawnRoom,
}

/// <summary>Immutable control words for the two Brinstar blue-spore palette loops.</summary>
/// <remarks>
/// Palette-FX definitions <c>$F775</c> and <c>$F779</c> contain the same presentation BGR555
/// sequence. The Spore Spawn variant additionally installs the area-mini-boss death
/// pre-instruction. This catalog owns setup, timing, waits, and loop control only.
/// Standard $8D:ED99 selects CGRAM byte $00E2 and enters records at
/// $ED9D + 10*i; Spore Spawn $8D:EE2D first installs pre-instruction
/// $EEC5, selects the same index, and enters records at $EE35 + 10*i.
/// Each valid i=0..13 lasts ten frames, writes three presentation colors, and
/// ends in $C595 wait. Their $C61E gotos at $EE29 and $EEC1 return to
/// the respective first records, giving 140-frame cycles; i=14 reaches
/// loop control. All 66 control words across both programs match the
/// pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class BrinstarBlueSporePaletteFxProgramMechanicsDefinitions
{
    private static readonly BrinstarBlueSporePaletteFxProgramDefinition Standard =
        new(BrinstarBlueSporePaletteOwner.StandardRooms);
    private static readonly BrinstarBlueSporePaletteFxProgramDefinition SporeSpawn =
        new(BrinstarBlueSporePaletteOwner.SporeSpawnRoom);

    private sealed class ProgramOwners : IReadOnlyList<BrinstarBlueSporePaletteFxProgramDefinition>
    {
        public int Count => 2;
        public BrinstarBlueSporePaletteFxProgramDefinition this[int index] => index switch
        {
            0 => Standard,
            1 => SporeSpawn,
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        public IEnumerator<BrinstarBlueSporePaletteFxProgramDefinition> GetEnumerator()
        {
            yield return Standard;
            yield return SporeSpawn;
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Fourteen ten-frame records form one complete spore-color cycle.</summary>
    public const int FrameCount = 14;

    /// <summary>Three BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>Bytes from one duration word through its terminal wait command.</summary>
    public const int FrameByteCount = 10;

    /// <summary>The byte index of the first blue-spore color in CGRAM.</summary>
    public const ushort ColorByteIndex = 0x00e2;

    /// <summary>The standard-room and Spore Spawn variants in definition order.</summary>
    public static IReadOnlyList<BrinstarBlueSporePaletteFxProgramDefinition> All { get; } = new ProgramOwners();

    /// <summary>Resolves one compiled mechanics word across both blue-spore programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        if (Standard.TryReadMechanicsWord(pointer, out value) ||
            SporeSpawn.TryReadMechanicsWord(pointer, out value))
            return true;
        value = 0;
        return false;
    }
}

/// <summary>One room-specific entry into the fourteen-frame blue-spore palette loop.</summary>
public sealed class BrinstarBlueSporePaletteFxProgramDefinition
{
    internal BrinstarBlueSporePaletteFxProgramDefinition(BrinstarBlueSporePaletteOwner owner)
    {
        if (owner is not (BrinstarBlueSporePaletteOwner.StandardRooms or BrinstarBlueSporePaletteOwner.SporeSpawnRoom))
            throw new ArgumentOutOfRangeException(nameof(owner));
        Owner = owner;
    }

    /// <summary>The room family represented by this program.</summary>
    public BrinstarBlueSporePaletteOwner Owner { get; }

    /// <summary>Native blue-spore setup entries $8D:ED99 (standard) and $8D:EE2D (Spore Spawn).</summary>
    public ushort ProgramStart => DeletesWithAreaMiniBoss ? (ushort)0xee2d : (ushort)0xed99;

    /// <summary>$8D:ED9D/$EE35, after color-index setup and the optional death-preinstruction pair.</summary>
    public ushort FirstFramePointer => (ushort)(ProgramStart + (DeletesWithAreaMiniBoss ? 8 : 4));

    /// <summary>$8D:EE29/$EEC1, after fourteen ten-byte timed color records.</summary>
    public ushort LoopInstructionPointer => (ushort)(FirstFramePointer +
        BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameCount *
        BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameByteCount);

    /// <summary>The Spore Spawn room installs the area-mini-boss death callback at $8D:EE2D.</summary>
    public bool DeletesWithAreaMiniBoss => Owner == BrinstarBlueSporePaletteOwner.SporeSpawnRoom;
    /// <summary>Returns one timed-record pointer.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer +
            frame * BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Returns one contiguous presentation-color address within a frame.</summary>
    /// <remarks>
    /// Both native lists contain identical presentation colors for frame f=0..13
    /// and color c=0..2, at FirstFramePointer + 10*f + 2 + 2*c.
    /// Let d=min(f,14-f) and s=(d+2)/2, except s=0 at d=0. With BGR555 word
    /// r + 32*g + 1024*b, the (r,g,b) channels are
    /// c=0: (max(0,2-d), 9-d, 23-d);
    /// c=1: (max(0,3-s), max(0,3-s), 17-s);
    /// c=2: (0, max(0,2-s), 6-s).
    /// These rules match all 84 words in the two pinned NTSC J/U v1.0 ROM
    /// tables. Frame fourteen reaches loop control, not a color record.
    /// Color payload resolution belongs to the installed presentation layer.
    /// </remarks>
    public ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }

    /// <summary>Reads one mechanics word while excluding live BGR555 colors.</summary>
    public bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        int setupOffset = pointer - ProgramStart;
        ushort? setupWord = setupOffset switch
        {
            0 when DeletesWithAreaMiniBoss => (ushort)PaletteFxInstruction.SetPreInstruction,
            2 when DeletesWithAreaMiniBoss =>
                (ushort)PaletteFxPreInstruction.DeleteWhenAreaMiniBossDies,
            4 when DeletesWithAreaMiniBoss => (ushort)PaletteFxInstruction.SetColorIndex,
            6 when DeletesWithAreaMiniBoss =>
                BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.ColorByteIndex,
            0 => (ushort)PaletteFxInstruction.SetColorIndex,
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
            value = (ushort)PaletteFxInstruction.Goto;
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
                value = (ushort)PaletteFxInstruction.Wait;
                return true;
            }
        }

        value = 0;
        return false;
    }
}
