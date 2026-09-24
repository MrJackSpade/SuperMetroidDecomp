// Generated from the pinned retail cartridge by --generate-enemy-visual-selectors.
// Edit the producer catalog and regenerate; do not hand-edit individual entries.
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

    internal static int Count => Entries.Length;
    internal static CompiledEnemyVisualSelector At(int index) => Entries[index];

    internal static bool TryGet(byte bank, ushort operandAddress, out ushort pointer)
    {
        int key = (bank << 16) | operandAddress;
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
