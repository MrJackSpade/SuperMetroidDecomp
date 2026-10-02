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

    private static int CalculatedCount => AlcoonInstructionProgramDefinitions.PresentationWordCount +
        AtomicInstructionProgramDefinitions.PresentationWordCount;
    internal static int Count => Entries.Length + CalculatedCount;
    internal static CompiledEnemyVisualSelector At(int index)
    {
        if ((uint)index >= Count) throw new IndexOutOfRangeException();
        int low = 0, high = Entries.Length;
        while (low < high)
        {
            int middle = low + (high - low) / 2;
            if (Entries[middle].Address < 0xa8dbeb) low = middle + 1;
            else high = middle;
        }
        // Alcoon and Atomic are consecutive families in the original sorted catalog;
        // no other selector lies between their two instruction regions.
        int calculatedIndex = index - low;
        if (calculatedIndex < 0) return Entries[index];
        if (calculatedIndex < AlcoonInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = AlcoonInstructionProgramDefinitions.PresentationWordAddress(calculatedIndex);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.AlcoonFrameAt(operand));
        }
        if (calculatedIndex < CalculatedCount)
        {
            ushort operand = AtomicInstructionProgramDefinitions.PresentationWordAddress(
                calculatedIndex - AlcoonInstructionProgramDefinitions.PresentationWordCount);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.AtomicFrameAt(operand));
        }
        return Entries[index - CalculatedCount];
    }

    internal static bool IsCalculatedSelector(int address) =>
        (address >> 16) == EnemySpritemapDefinitions.AlcoonBank &&
        (AlcoonInstructionProgramDefinitions.IsPresentationWord((ushort)address) ||
         AtomicInstructionProgramDefinitions.IsPresentationWord((ushort)address));

    internal static bool TryGet(byte bank, ushort operandAddress, out ushort pointer)
    {
        int key = (bank << 16) | operandAddress;
        if (IsCalculatedSelector(key))
        {
            pointer = AlcoonInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                ? EnemySpritemapDefinitions.AlcoonFrameAt(operandAddress)
                : EnemySpritemapDefinitions.AtomicFrameAt(operandAddress);
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
