using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The codec of the cartridge's Samus pose animation programs (#1473): a table of one same-bank word per pose naming
/// where its program starts, then the programs as delay bytes below <c>$80</c> and instruction bytes <c>$F0-$FF</c> with
/// their operands, each instruction taking the frame numbers of its own byte and its operands. The game holds typed
/// programs; this is the one place their byte layout is known, for checks.
/// </summary>
[EncodingOwner("cartridge Samus pose animation programs")]
internal static class SamusAnimationProgramCodec
{
    /// <summary>The table of words naming each pose's program.</summary>
    internal const int PoseTable = 0x91b010;

    /// <summary>The poses with a word in the table.</summary>
    internal const int PoseCount = 253;

    /// <summary>The bank holding the programs; each table word names a place in it.</summary>
    private const int Bank = 0x910000;

    /// <summary>The window position the programs end at, where the running cadences start.</summary>
    private const int ProgramsEnd = 0xb5d1;

    /// <summary>The lowest instruction byte; every byte below it is a delay.</summary>
    private const int FirstInstruction = 0x80;

    /// <summary>An item and the operand bit that names it.</summary>
    /// <param name="Bit">Its bit in the operand word.</param>
    /// <param name="Item">The item.</param>
    private readonly record struct ItemBit(int Bit, SamusEquipmentFlags Item);

    /// <summary>Each item an item instruction's operand word names, by the bit that names it.</summary>
    private static readonly ItemBit[] ItemBits =
    [
        new(1 << 0, SamusEquipmentFlags.VariaSuit),
        new(1 << 1, SamusEquipmentFlags.SpringBall),
        new(1 << 2, SamusEquipmentFlags.MorphBall),
        new(1 << 3, SamusEquipmentFlags.ScrewAttack),
        new(1 << 4, SamusEquipmentFlags.UnassignedItem),
        new(1 << 5, SamusEquipmentFlags.GravitySuit),
        new(1 << 8, SamusEquipmentFlags.HiJumpBoots),
        new(1 << 9, SamusEquipmentFlags.SpaceJump),
        new(1 << 12, SamusEquipmentFlags.Bombs),
        new(1 << 13, SamusEquipmentFlags.SpeedBooster),
        new(1 << 14, SamusEquipmentFlags.GrappleBeam),
        new(1 << 15, SamusEquipmentFlags.XrayScope),
    ];

    /// <summary>
    /// Decodes <paramref name="pose"/>'s program: every run from where the pose's word points up to the next place any
    /// pose's word points, or the end of the programs.
    /// </summary>
    internal static IReadOnlyList<SamusAnimationSequence> Decode(IImportCartridgeSource cartridge, SamusPoseId pose)
    {
        int start = ProgramStart(cartridge, SamusPoseNumbering.Number(pose));
        int end = ProgramsEnd;
        for (int other = 0; other < PoseCount; other++)
        {
            int otherStart = ProgramStart(cartridge, other);
            if (otherStart > start && otherStart < end)
                end = otherStart;
        }

        var sequences = new List<SamusAnimationSequence>();
        int position = start;
        while (position < end)
        {
            int firstFrame = position - start;
            var delays = new List<int>();
            int value;
            while ((value = Byte(cartridge, position)) < FirstInstruction)
            {
                delays.Add(value);
                position++;
            }
            int frame = position - start;
            SamusAnimationInstruction instruction = Instruction(cartridge, value, position, frame);
            position += 1 + OperandBytes(value);
            // Six transitions write their pose as a word; the interpreter never reads its zero high byte.
            if (instruction is SamusAnimationTransition && position == end - 1 && Byte(cartridge, position) == 0)
                position++;
            sequences.Add(new SamusAnimationSequence(firstFrame, delays, instruction));
        }
        if (position != end)
            throw new InvalidDataException($"Pose {pose}'s program runs past the next program at $91:{end:X4}.");
        return sequences;
    }

    /// <summary>The window position the table's word for pose number <paramref name="pose"/> names.</summary>
    private static int ProgramStart(IImportCartridgeSource cartridge, int pose) =>
        cartridge.ReadCartridgeWord(PoseTable + pose * sizeof(ushort));

    /// <summary>The program byte at window position <paramref name="position"/>.</summary>
    private static int Byte(IImportCartridgeSource cartridge, int position) => cartridge.ReadCartridgeByte(Bank | position);

    /// <summary>The operand bytes that follow instruction byte <paramref name="value"/>.</summary>
    private static int OperandBytes(int value) => value switch
    {
        0xf8 or 0xfd or 0xfe => 1,
        0xfc => 4,
        0xf9 => 6,
        _ => 0,
    };

    /// <summary>Decodes the instruction byte <paramref name="value"/> at frame <paramref name="frame"/>, with its operands.</summary>
    private static SamusAnimationInstruction Instruction(IImportCartridgeSource cartridge, int value, int position, int frame)
    {
        SamusPoseId PoseAt(int operand) => SamusPoseNumbering.FromNumber(Byte(cartridge, position + operand));
        SamusEquipmentFlags Items() => ItemsOf(cartridge.ReadCartridgeWord(Bank | (position + 1)));
        return value switch
        {
            0xf0 => new SamusAnimationHold(),
            0xf6 => new SamusAnimationLoopUnlessLowEnergy(),
            0xf7 => new SamusAnimationInstallDrainedFall(),
            0xf8 => new SamusAnimationTurnTransition(PoseAt(1)),
            0xf9 => new SamusAnimationItemAirborneTransition(Items(), PoseAt(3), PoseAt(4), PoseAt(5), PoseAt(6)),
            0xfb => new SamusAnimationWallJumpSelect(frame + 1, frame + 11, frame + 21),
            0xfc => new SamusAnimationItemTransition(Items(), PoseAt(3), PoseAt(4)),
            0xfd => new SamusAnimationTransition(PoseAt(1)),
            0xfe => new SamusAnimationRepeat(frame - Byte(cartridge, position + 1)),
            0xff => new SamusAnimationRestart(),
            _ => throw new InvalidDataException($"Animation byte ${value:X2} at $91:{position:X4} is no instruction a retail program holds."),
        };
    }

    /// <summary>The items an item instruction's operand word names.</summary>
    private static SamusEquipmentFlags ItemsOf(int word)
    {
        SamusEquipmentFlags items = SamusEquipmentFlags.None;
        int named = 0;
        foreach (ItemBit entry in ItemBits)
        {
            if ((word & entry.Bit) != 0)
                items |= entry.Item;
            named |= entry.Bit;
        }
        if ((word & ~named) != 0)
            throw new InvalidDataException($"Item operand ${word:X4} names bits no item has.");
        return items;
    }
}
