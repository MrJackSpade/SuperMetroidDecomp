namespace SuperMetroid.Core.Game;

/// <summary>One room-specific entry into the fourteen-frame blue-spore palette loop.</summary>
public sealed class BrinstarBlueSporePaletteFxProgramDefinition
{
    /// <summary>Restricts entries to the two room families that share the blue-spore colors.</summary>
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
    /// <summary>Locates one of fourteen ten-byte timed records after this owner's setup words.</summary>
    public ushort FramePointer(int frame)
    {
        if ((uint)frame >= BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer +
            frame * BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.FrameByteCount));
    }

    /// <summary>Locates one of three live BGR555 words immediately after a record's duration.</summary>
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
