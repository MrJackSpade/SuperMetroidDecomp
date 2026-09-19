namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A3 address.</summary>
internal readonly record struct SkreeMetareeInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>Compiled mechanics words from the parallel Skree and Metaree programs.</summary>
internal static class SkreeMetareeInstructionProgramDefinitions
{
    internal const ushort MetareeIdling = 0x8910;
    internal const ushort MetareePreparingAttack = 0x8924;
    internal const ushort MetareeDiving = 0x8930;
    internal const ushort MetareeStopAnimating = 0x8946;

    internal const ushort SkreeIdling = 0xc65e;
    internal const ushort SkreePreparingAttack = 0xc672;
    internal const ushort SkreeDiving = 0xc67e;
    internal const ushort SkreeStopAnimating = 0xc694;

    private static readonly SkreeMetareeInstructionMechanicsWord[] MetareeWords =
    [
        new(0x8910, 0x000a), new(0x8914, 0x000a),
        new(0x8918, 0x000a), new(0x891c, 0x000a),
        new(0x8920, 0x80ed), new(0x8922, 0x8910),
        new(0x8924, 0x0010), new(0x8928, 0x0008),
        new(0x892c, 0x8956), new(0x892e, 0x812f),
        new(0x8930, 0x8173),
        new(0x8932, 0x0002), new(0x8936, 0x0002),
        new(0x893a, 0x0002), new(0x893e, 0x0002),
        new(0x8942, 0x80ed), new(0x8944, 0x8932),
        new(0x8946, 0x817d), new(0x8948, 0x0001), new(0x894c, 0x812f),
    ];

    private static readonly SkreeMetareeInstructionMechanicsWord[] SkreeWords =
    [
        new(0xc65e, 0x000a), new(0xc662, 0x000a),
        new(0xc666, 0x000a), new(0xc66a, 0x000a),
        new(0xc66e, 0x80ed), new(0xc670, 0xc65e),
        new(0xc672, 0x0010), new(0xc676, 0x0008),
        new(0xc67a, 0xc6a4), new(0xc67c, 0x812f),
        new(0xc67e, 0x8173),
        new(0xc680, 0x0002), new(0xc684, 0x0002),
        new(0xc688, 0x0002), new(0xc68c, 0x0002),
        new(0xc690, 0x80ed), new(0xc692, 0xc680),
        new(0xc694, 0x817d), new(0xc696, 0x0001), new(0xc69a, 0x812f),
    ];

    private static readonly ushort[] MetareePresentationWords =
    [
        0x8912, 0x8916, 0x891a, 0x891e,
        0x8926, 0x892a,
        0x8934, 0x8938, 0x893c, 0x8940,
        0x894a,
    ];

    private static readonly ushort[] SkreePresentationWords =
    [
        0xc660, 0xc664, 0xc668, 0xc66c,
        0xc674, 0xc678,
        0xc682, 0xc686, 0xc68a, 0xc68e,
        0xc698,
    ];

    internal static int MechanicsWordCount(bool metaree) =>
        SelectWords(metaree).Length;

    internal static SkreeMetareeInstructionMechanicsWord MechanicsWord(
        bool metaree,
        int index) => SelectWords(metaree)[index];

    internal static int PresentationWordCount(bool metaree) =>
        SelectPresentationWords(metaree).Length;

    internal static ushort PresentationWordAddress(bool metaree, int index) =>
        SelectPresentationWords(metaree)[index];

    internal static ushort ReadMetareeMechanicsWord(ushort address) =>
        ReadMechanicsWord(MetareeWords, address, "Metaree");

    internal static ushort ReadSkreeMechanicsWord(ushort address) =>
        ReadMechanicsWord(SkreeWords, address, "Skree");

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return ContainsByte(MetareeWords, bankAddress) || ContainsByte(SkreeWords, bankAddress);
    }

    private static SkreeMetareeInstructionMechanicsWord[] SelectWords(bool metaree) =>
        metaree ? MetareeWords : SkreeWords;

    private static ushort[] SelectPresentationWords(bool metaree) =>
        metaree ? MetareePresentationWords : SkreePresentationWords;

    private static ushort ReadMechanicsWord(
        SkreeMetareeInstructionMechanicsWord[] words,
        ushort address,
        string enemyName)
    {
        int low = 0;
        int high = words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            SkreeMetareeInstructionMechanicsWord candidate = words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"{enemyName} instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }

    private static bool ContainsByte(
        SkreeMetareeInstructionMechanicsWord[] words,
        ushort address)
    {
        for (int index = 0; index < words.Length; index++)
        {
            ushort wordAddress = words[index].Address;
            if (address == wordAddress || address == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }
}
