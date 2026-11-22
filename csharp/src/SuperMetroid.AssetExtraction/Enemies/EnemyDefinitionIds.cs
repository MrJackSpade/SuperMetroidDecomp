using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Translation between <see cref="EnemyDefinitionId"/> and the bank-$A0 header pointers the cartridge names enemy
/// definitions by (#1412). The identity's values are arbitrary; <see cref="EnemyHeaderWords.Headers"/> is the mapping.
/// </summary>
internal static partial class EnemyDefinitionIds
{

    /// <summary>
    /// The bank-$A0 header pointer of <paramref name="id"/>: its own, or the header it shares with the other actors the
    /// cartridge places by it (<see cref="EnemyHeaderWords.SharedHeaders"/>).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is not a member.</exception>
    internal static ushort HeaderPointer(EnemyDefinitionId id) =>
        EnemyHeaderWords.SharedHeaders.TryGetValue(id, out UnplacedEnemyArtwork shared)
            ? EnemyHeaderWords.UnplacedArtworkHeaders.Word(shared)
            : EnemyHeaderWords.Headers.Word(id);
}
