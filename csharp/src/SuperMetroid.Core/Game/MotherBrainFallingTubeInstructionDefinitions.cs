using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Game;

/// <summary>
/// The five fixed $A9:8C69-$8C85 falling-tube instruction lists. Their one-frame
/// sleep cadence is engine behavior; the five spritemap identities remain visual
/// selectors rather than a copied ROM instruction stream.
/// </summary>
public static class MotherBrainFallingTubeInstructionDefinitions
{
    /// <summary>$A9:8C69, the bottom-left tube's first instruction word.</summary>
    public const ushort FirstList = 0x8c69;

    /// <summary>$A9:8C85, the main tube's terminal sleep opcode.</summary>
    public const ushort LastSleep = 0x8c85;

    /// <summary>Native byte stride between the five tube instruction lists.</summary>
    public const ushort ListStride = 6;

    /// <summary>Number of independently selected falling-tube compositions.</summary>
    public const int ListCount = 5;

    /// <summary>Reads a duration or terminal opcode, rejecting visual operands.</summary>
    public static ushort ReadMechanicsWord(ushort address)
    {
        int field = Locate(address).Field;
        return field switch
        {
            0 => 1,
            4 => CommonEnemyInstructionCodes.Sleep,
            _ => throw new InvalidDataException(
                $"Falling-tube visual selector $A9:{address:X4} is not mechanics."),
        };
    }

    /// <summary>Reads the selected native spritemap identity, not its OAM payload.</summary>
    public static ushort ReadVisualSelector(ushort address)
    {
        (int index, int field) = Locate(address);
        if (field != 2)
        {
            throw new InvalidDataException(
                $"Falling-tube word $A9:{address:X4} is not a visual selector.");
        }
        return MotherBrainVisualDefinitions.TubeFrame(index).Pointer;
    }

    private static (int Index, int Field) Locate(ushort address)
    {
        int offset = address - FirstList;
        if (offset < 0 || address > LastSleep || (offset & 1) != 0)
        {
            throw new InvalidDataException(
                $"Falling-tube instruction word $A9:{address:X4} is outside five lists.");
        }
        return (offset / ListStride, offset % ListStride);
    }
}
