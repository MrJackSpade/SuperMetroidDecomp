namespace SuperMetroid.Core.Game;

/// <summary>
/// Fixed control words of Mother Brain's $A9:9A42 hand-beam body program.
/// Eight dust/frame records share one native layout; visual selector words are
/// resolved separately so they cannot be mistaken for gameplay operands.
/// </summary>
public static class MotherBrainHandBeamBodyInstructionDefinitions
{
    /// <summary>$A9:9A42, hand-beam body program entry.</summary>
    public const ushort Start = 0x9a42;

    /// <summary>$A9:9AC6, terminal sleep instruction.</summary>
    public const ushort End = 0x9ac6;

    /// <summary>$A9:9A50, first dust/one-frame record.</summary>
    public const ushort FirstDustRecord = 0x9a50;

    /// <summary>Byte stride between eight alternating dust records.</summary>
    public const ushort DustRecordStride = 12;

    /// <summary>Number of dust records preceding the beam attack.</summary>
    public const int DustRecordCount = 8;

    /// <summary>All fifteen visual operands; dust coordinates, durations and opcodes are excluded.</summary>
    internal static IReadOnlyList<ushort> PresentationOperands => VisualOperands;
    private static readonly VisualOperandList VisualOperands = new();

    /// <summary>$A9:9A44-9A4F: three entry duration/spritemap pairs after the pose callback.</summary>
    private const int EntryFrames = 3;
    /// <summary>$A9:9AB0: duration/spritemap pair immediately before the beam-spawn callback.</summary>
    private const ushort BeforeEmissionFrame = FirstDustRecord + DustRecordCount * DustRecordStride;
    /// <summary>$A9:9AB6-9AC1: three duration/spritemap pairs after the beam-spawn callback.</summary>
    private const ushort AfterEmissionFrames = BeforeEmissionFrame + 3 * sizeof(ushort);

    private sealed class VisualOperandList : IReadOnlyList<ushort>
    {
        public int Count => EntryFrames + DustRecordCount + 1 + EntryFrames;
        public ushort this[int index]
        {
            get
            {
                if ((uint)index >= Count) throw new IndexOutOfRangeException();
                if (index < EntryFrames) return (ushort)(Start + 2 * sizeof(ushort) + index * 2 * sizeof(ushort));
                index -= EntryFrames;
                if (index < DustRecordCount) return (ushort)(FirstDustRecord + index * DustRecordStride + DustRecordStride - sizeof(ushort));
                index -= DustRecordCount;
                return index == 0 ? (ushort)(BeforeEmissionFrame + sizeof(ushort)) :
                    (ushort)(AfterEmissionFrames + (index - 1) * 2 * sizeof(ushort) + sizeof(ushort));
            }
        }
        public IEnumerator<ushort> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Whether the aligned word belongs to the compiled hand-beam list.</summary>
    public static bool ContainsWord(ushort address) =>
        address is >= Start and <= End && ((address - Start) & 1) == 0;

    /// <summary>Reads one opcode, timer, or dust parameter in the native list.</summary>
    public static ushort ReadMechanicsWord(ushort address)
    {
        Validate(address);
        if (address is >= FirstDustRecord and <= 0x9aae)
        {
            int offset = address - FirstDustRecord;
            bool evenRecord = (offset / DustRecordStride & 1) == 0;
            return (ushort)((offset % DustRecordStride) switch
            {
                0 => MotherBrainInstructionCodes.Instruction_MotherBrainBody_SpawnDustCloudExplosionProj,
                2 => evenRecord ? (ushort)0x0024 : (ushort)0x0022,
                4 => evenRecord ? (ushort)0xffd8 : (ushort)0xffd6,
                6 => evenRecord ? (ushort)1 : (ushort)2,
                8 => (ushort)1,
                _ => throw NotMechanics(address),
            });
        }

        return address switch
        {
            0x9a42 => MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToDeathBeamMode,
            0x9a44 or 0x9a48 or 0x9a4c or 0x9ab0 or 0x9ab6 or 0x9aba => (ushort)1,
            0x9ab4 => MotherBrainInstructionCodes.Instruction_MotherBrainBody_SpawnDeathBeamProjectile,
            0x9abe => (ushort)0x00f0,
            0x9ac2 => MotherBrainInstructionCodes.Instruction_MotherBrainBody_IncrementDeathBeamAttackPhase,
            0x9ac4 => MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding,
            0x9ac6 => MotherBrainInstructionCodes.Instruction_CommonA9_Sleep,
            _ => throw NotMechanics(address),
        };
    }

    /// <summary>Reads a native visual identity without reading its OAM payload.</summary>
    public static ushort ReadVisualSelector(ushort address)
    {
        Validate(address);
        if (address is >= FirstDustRecord and <= 0x9aae &&
            (address - FirstDustRecord) % DustRecordStride == 10)
        {
            return 0xa3ce;
        }
        return address switch
        {
            0x9a46 or 0x9ac0 => 0x9fa0,
            0x9a4a => 0xa384,
            0x9a4e or 0x9ab2 => 0xa3ce,
            0x9ab8 => 0xa418,
            0x9abc => 0xa462,
            _ => throw new InvalidDataException(
                $"Mother Brain hand-beam word $A9:{address:X4} is not a visual selector."),
        };
    }

    private static void Validate(ushort address)
    {
        if (!ContainsWord(address))
        {
            throw new InvalidDataException(
                $"Mother Brain hand-beam word $A9:{address:X4} is outside the compiled list.");
        }
    }

    private static InvalidDataException NotMechanics(ushort address) => new(
        $"Mother Brain hand-beam visual selector $A9:{address:X4} is not mechanics.");
}
