namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics word in the Ceres Baby's bank-$A6 draw program.</summary>
internal readonly record struct CeresBabyInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Engine-owned control words from the Ceres Baby's mixed draw instruction program.
/// </summary>
/// <remarks>
/// Callback identities, branch targets, and frame durations determine animation cadence
/// and control flow. Fixed spritemap and palette selectors are compiled; their
/// OAM compositions and RGB5 colors live in editable presentation assets.
/// </remarks>
internal static class CeresBabyInstructionProgramDefinitions
{
    /// <summary>Bank $A6 owns the Ceres Baby's private draw program and OAM frames.</summary>
    internal const byte Bank = 0xa6;
    /// <summary>The horizontal-squish Baby OAM frame at $A6:BFFD.</summary>
    internal const ushort HorizontalFrame = 0xbffd;
    /// <summary>The round Baby OAM frame at $A6:C018.</summary>
    internal const ushort RoundFrame = 0xc018;
    /// <summary>The vertical-squish Baby OAM frame at $A6:C033.</summary>
    internal const ushort VerticalFrame = 0xc033;
    /// <summary>
    /// <c>InstList_BabyMetroidCutscene_0</c> at $A6:BF31-$A6:BF58.
    /// The $BFF2 conditional at $BF31 and again at $BF45 targets
    /// <see cref="ExpressiveLoop"/> when Baby's vertical velocity is nonzero.
    /// Each conditional precedes four frames: the duration word at
    /// $BF35 + 4*i or $BF49 + 4*i is exactly $000A for i = 0..3.
    /// Each duration is followed by a compiled spritemap selector. If neither
    /// conditional branches, the eight frames fall through to $BF59.
    /// </summary>
    internal const ushort Initial = 0xbf31;

    /// <summary>
    /// <c>InstList_BabyMetroidCutscene_1</c> at $A6:BF59-$A6:BFC8.
    /// The $BFC9 command at $BF59 queues the cry and may branch to
    /// <see cref="Initial"/> on stationary odd RNG. For frame i = 0..11,
    /// $BF5D + 8*i is palette callback $BFE1 and the duration at
    /// $BF61 + 8*i is exactly 2 + abs(i - 4) ticks. Each is followed by a
    /// palette or spritemap operand. A final $BFE1 at $BFBD updates
    /// the palette; $BFF2 at $BFC1 loops to $BF59 while moving, otherwise
    /// $BFF8 at $BFC5 returns to $BF31.
    /// </summary>
    internal const ushort ExpressiveLoop = 0xbf59;



    /// <summary>
    /// Visual operand layout in the two draw programs.
    /// In <see cref="Initial"/>, each of the two four-frame groups has
    /// spritemap operands at $BF37 + 4*i or $BF4B + 4*i for i = 0..3.
    /// Both groups store the same {$BFFD, $C018, $C033, $C018} pose sequence;
    /// the three distinct pointers advance by $001B. These selectors are
    /// compiled while their editable OAM compositions remain separate assets.
    /// In <see cref="ExpressiveLoop"/>, palette operand i = 0..11 is at
    /// $BF5F + 8*i and points to $E20F + $001E*q, where q follows
    /// {0, 1, 2, 1} repeated three times. Each palette block contains
    /// fifteen colors ($001E bytes). The final operand at $BFBF points to
    /// $E1F1, the preceding palette block, outside that repeated cycle.
    /// The expressive spritemap operand for i = 0..11 is at $BF63 + 8*i;
    /// its pointer is $BFFD + $001B*q using the same four-pose cycle
    /// q = {0, 1, 2, 1} repeated three times.
    /// </summary>
    internal static int MechanicsWordCount => 43;

    internal static CeresBabyInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 12)
        {
            int group = index / 6;
            int word = index % 6;
            int address = Initial + group * 20 + (word < 2 ? word * 2 : 4 + (word - 2) * 4);
            ushort value = word switch
            {
                0 => CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_GotoXIfNotFalling,
                1 => ExpressiveLoop,
                _ => 10,
            };
            return new((ushort)address, value);
        }
        if (index < 14)
            return new((ushort)(ExpressiveLoop + (index - 12) * 2), index == 12
                ? CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_PlayCrySFXOrGotoX : Initial);
        if (index < 38)
        {
            int frame = (index - 14) / 2;
            bool callback = (index & 1) == 0;
            return new((ushort)(ExpressiveLoop + 4 + frame * 8 + (callback ? 0 : 4)),
                callback ? CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_UpdateColors
                    : (ushort)(2 + Math.Abs(frame - 4)));
        }
        int tail = index - 38;
        return new((ushort)(ExpressiveLoop + 100 + (tail == 0 ? 0 : 2 + tail * 2)), tail switch
        {
            0 => CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_UpdateColors,
            1 => CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_GotoXIfNotFalling,
            2 => ExpressiveLoop,
            3 => CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_GotoX,
            _ => Initial,
        });
    }

    internal const int SpritemapOperandCount = 20;
    internal const int PaletteOperandCount = 13;

    /// <summary>Enumerates the twelve expressive and one final palette operands.</summary>
    internal static ushort PaletteOperandAddress(int index) => index switch
    {
        >= 0 and < 12 => unchecked((ushort)(0xbf5f + index * 8)),
        12 => 0xbfbf,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index,
            "Ceres Baby has exactly thirteen authored palette operands."),
    };

    /// <summary>
    /// Returns the authored palette row: rows one, two, three, two repeat in
    /// the expressive cycle, and the final callback restores row zero.
    /// </summary>
    internal static int ReadPaletteRow(ushort address)
    {
        for (int index = 0; index < PaletteOperandCount; index++)
        {
            if (PaletteOperandAddress(index) != address)
                continue;
            return index == 12 ? 0 : (index & 3) switch
            {
                0 => 1,
                1 or 3 => 2,
                _ => 3,
            };
        }
        throw new InvalidDataException(
            $"Ceres Baby palette operand $A6:{address:X4} is not compiled.");
    }

    /// <summary>Enumerates the eight initial and twelve expressive pose operands.</summary>
    internal static ushort SpritemapOperandAddress(int index) => index switch
    {
        >= 0 and < 4 => unchecked((ushort)(0xbf37 + index * 4)),
        >= 4 and < 8 => unchecked((ushort)(0xbf4b + (index - 4) * 4)),
        >= 8 and < SpritemapOperandCount =>
            unchecked((ushort)(0xbf63 + (index - 8) * 8)),
        _ => throw new ArgumentOutOfRangeException(nameof(index), index,
            "Ceres Baby has exactly twenty authored spritemap operands."),
    };

    /// <summary>Returns a fixed visual identity without reading bank-$A6 ROM.</summary>
    internal static ushort ReadSpritemapOperand(ushort address)
    {
        for (int index = 0; index < SpritemapOperandCount; index++)
        {
            if (SpritemapOperandAddress(index) != address)
                continue;
            return (index & 3) switch
            {
                0 => HorizontalFrame,
                1 or 3 => RoundFrame,
                _ => VerticalFrame,
            };
        }
        throw new InvalidDataException(
            $"Ceres Baby spritemap operand $A6:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledSpritemapByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < SpritemapOperandCount; index++)
        {
            ushort operand = SpritemapOperandAddress(index);
            if (bankAddress == operand || bankAddress == unchecked((ushort)(operand + 1)))
                return true;
        }
        return false;
    }

    internal static bool IsCompiledPaletteByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < PaletteOperandCount; index++)
        {
            ushort operand = PaletteOperandAddress(index);
            if (bankAddress == operand || bankAddress == unchecked((ushort)(operand + 1)))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Reads one compiled control word and rejects presentation or adjacent code addresses.
    /// </summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CeresBabyInstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Ceres Baby instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }

}
