using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// Sparse fixed visual selectors from compiled instruction catalogs. These are
/// engine definitions, not editable art, callback code, or a reconstructed ROM.
/// A selected target still needs its own renderer and presentation asset.
/// </summary>
internal static partial class CompiledEnemyVisualSelectors
{
    /// <summary>Bank-sorted literal selector operands merged for binary lookup after calculated families.</summary>
    internal static readonly CompiledEnemyVisualSelector[] Entries =
    [
        .. Bank86,
        .. BankA2,
        .. BankA3,
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

    /// <summary>Recognizes operands whose visual target is computed from a compiled instruction family.</summary>
    internal static bool IsCalculatedSelector(int address) =>
        Enum.IsDefined((EnemyInstructionBank)SnesAddressMath.BankNumber(address)) &&
        (EnemyInstructionBank)SnesAddressMath.BankNumber(address) switch
    {
        EnemyInstructionBank.Bank86 => AlcoonFireballInstructionProgramDefinitions.IsPresentationWord((ushort)address) || FuneNamiheFireballInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        EnemyInstructionBank.BankA6 => FakeKraidInstructionProgramDefinitions.IsPresentationWord((ushort)address),
        _ => throw new InvalidOperationException($"Undefined enemy instruction bank ${SnesAddressMath.BankNumber(address):X2}."),
    };
    /// <summary>Resolves special and calculated sprite operands, then searches literal bank entries.</summary>
    internal static bool TryGet(byte bank, ushort operandAddress, out ushort pointer)
    {
        if ((EnemyInstructionBank)bank == EnemyInstructionBank.Bank86 && PolypRockInstructionProgramDefinitions.IsPresentationWord(operandAddress))
        {
            pointer = PolypRockInstructionProgramDefinitions.FrameAt(operandAddress);
            return true;
        }
        int key = ((int)new SnesAddress(bank, operandAddress));
        if (IsCalculatedSelector(key))
        {
            pointer = (EnemyInstructionBank)bank switch
            {
                EnemyInstructionBank.Bank86 => AlcoonFireballInstructionProgramDefinitions.IsPresentationWord(operandAddress)
                    ? EnemyProjectileSpritemapDefinitions.AlcoonFireballFrameAt(operandAddress)
                    : EnemyProjectileSpritemapDefinitions.FuneNamiheFireballFrameAt(operandAddress),
                EnemyInstructionBank.BankA6 => KraidVisualDefinitions.FrameAt(EnemyDefinitionId.MiniKraid, operandAddress),
                _ => throw new InvalidOperationException($"Undefined enemy instruction bank ${bank:X2}."),
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
