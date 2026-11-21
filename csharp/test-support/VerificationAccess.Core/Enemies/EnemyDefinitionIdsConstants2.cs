using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyDefinitionIds"/> that only verification reads.</summary>
internal static class EnemyDefinitionIdsConstants2
{
    /// <summary>
    /// The species a stored bank-$A0 header pointer names, when it names one. A header several actors share
    /// (<see cref="EnemyHeaderWords.SharedHeaders"/>) names none of them alone: its placement's parameters select the actor.
    /// </summary>
    internal static bool TryFromHeaderPointer(int pointer, out EnemyDefinitionId id) =>
        EnemyHeaderWords.Headers.TryDecode(pointer, out id);
}
