using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyDefinitionIds"/> that only verification reads.</summary>
internal static class EnemyDefinitionIdsConstants
{
    /// <summary>The species a stored bank-$A0 header pointer names.</summary>
    /// <exception cref="InvalidDataException">The pointer is not a labelled enemy header.</exception>
    internal static EnemyDefinitionId FromHeaderPointer(int pointer) =>
        EnemyDefinitionIds.TryFromHeaderPointer(pointer, out EnemyDefinitionId id)
            ? id
            : throw new InvalidDataException($"${pointer:X4} is not a bank-$A0 enemy header.");
}
