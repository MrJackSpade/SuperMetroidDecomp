using SuperMetroid.Core.Game;

// Bank files contain remaining literal selectors. Calculated families are dispatched
// here and merged into the public enumeration without materializing a lookup cache.
namespace SuperMetroid.Core.Assets;

/// <summary>One immutable cartridge visual-pointer operand and its selected target.</summary>
internal readonly record struct CompiledEnemyVisualSelector(int Address, ushort Pointer);

/// <summary>
/// Sparse fixed visual selectors from compiled instruction catalogs. These are
/// engine definitions, not editable art, callback code, or a reconstructed ROM.
/// A selected target still needs its own renderer and presentation asset.
/// </summary>
internal static partial class CompiledEnemyVisualSelectors
{
    private static readonly CompiledEnemyVisualSelector[] Entries =
    [
        .. Bank86,
        .. BankA2,
        .. BankA3,
        .. BankA4,
        .. BankA5,
        .. BankA6,
        .. BankA7,
        .. BankA8,
        .. BankA9,
        .. BankAA,
        .. BankB2,
        .. BankB3,
        .. BankB4,
    ];

    private static int CalculatedCount => FakeKraidInstructionProgramDefinitions.PresentationWordCount +
        KraidNailInstructionProgramDefinitions.PresentationWordCount +
        AlcoonInstructionProgramDefinitions.PresentationWordCount + AtomicInstructionProgramDefinitions.PresentationWordCount;
    internal static int Count => Entries.Length + CalculatedCount;

    internal static CompiledEnemyVisualSelector At(int index)
    {
        if ((uint)index >= Count) throw new IndexOutOfRangeException();
        // Calculated entries are ordered by native bank/address. Their merged rank
        // is their own ordinal plus the number of literal entries before them.
        int low = 0, high = CalculatedCount;
        while (low < high)
        {
            int middle = low + (high - low) / 2;
            CompiledEnemyVisualSelector candidate = CalculatedAt(middle);
            int literalLow = 0, literalHigh = Entries.Length;
            while (literalLow < literalHigh)
            {
                int literalMiddle = literalLow + (literalHigh - literalLow) / 2;
                if (Entries[literalMiddle].Address < candidate.Address) literalLow = literalMiddle + 1;
                else literalHigh = literalMiddle;
            }
            int rank = literalLow + middle;
            if (rank == index) return candidate;
            if (rank < index) low = middle + 1;
            else high = middle;
        }
        return Entries[index - low];
    }

    private static CompiledEnemyVisualSelector CalculatedAt(int index)
    {
        if (index < FakeKraidInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = FakeKraidInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0xa60000 | operand, KraidVisualDefinitions.FrameAt(RoomEnemySystem.FakeKraidDefinition, operand));
        }
        index -= FakeKraidInstructionProgramDefinitions.PresentationWordCount;
        if (index < KraidNailInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = KraidNailInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0xa70000 | operand, KraidVisualDefinitions.FrameAt(RoomEnemySystem.KraidGoodNailDefinition, operand));
        }
        index -= KraidNailInstructionProgramDefinitions.PresentationWordCount;
        if (index < AlcoonInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = AlcoonInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.AlcoonFrameAt(operand));
        }
        index -= AlcoonInstructionProgramDefinitions.PresentationWordCount;
        ushort atomicOperand = AtomicInstructionProgramDefinitions.PresentationWordAddress(index);
        return new(0xa80000 | atomicOperand, EnemySpritemapDefinitions.AtomicFrameAt(atomicOperand));
    }

    internal static bool IsCalculatedSelector(int address) => (address >> 16) switch
    {
        0xa6 => FakeKraidInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        0xa7 => KraidNailInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        0xa8 => AlcoonInstructionProgramDefinitions.IsPresentationWord((ushort)address) ||
            AtomicInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        _ => false,
    };
    internal static bool TryGet(byte bank, ushort operandAddress, out ushort pointer)
    {
        int key = (bank << 16) | operandAddress;
        if (IsCalculatedSelector(key))
        {
            pointer = bank switch
            {
                0xa6 => KraidVisualDefinitions.FrameAt(RoomEnemySystem.FakeKraidDefinition, operandAddress),
                0xa7 => KraidVisualDefinitions.FrameAt(RoomEnemySystem.KraidGoodNailDefinition, operandAddress),
                _ => AlcoonInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? EnemySpritemapDefinitions.AlcoonFrameAt(operandAddress)
                    : EnemySpritemapDefinitions.AtomicFrameAt(operandAddress),
            };
            return true;
        }
        int low = 0;
        int high = Entries.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CompiledEnemyVisualSelector entry = Entries[middle];
            if (entry.Address == key)
            {
                pointer = entry.Pointer;
                return true;
            }
            if (entry.Address < key)
                low = middle + 1;
            else
                high = middle - 1;
        }
        pointer = 0;
        return false;
    }
}
