using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The members one enemy graphics set loads, in record order (#1527): the enemies whose sheets the import composes for
/// the rooms that name the set (<see cref="RoomEnemyGraphicsSetDefinition"/>). The game knows no set (#1599).
/// </summary>
/// <param name="set">The set whose members these are.</param>
/// <param name="records">The set's ordered members.</param>
internal sealed class RoomEnemyGraphicsSetMembers(RoomEnemyGraphicsSetDefinition set, RoomEnemyGraphicsSetMember[] records)
{
    /// <summary>The members in the set's order.</summary>
    private readonly RoomEnemyGraphicsSetMember[] members = Ensure.NotNull(records);

    /// <summary>The set whose members these are.</summary>
    internal RoomEnemyGraphicsSetDefinition Set { get; } = Ensure.NotNull(set);

    /// <summary>The members in the set's order; a member listed twice is retained twice.</summary>
    internal ReadOnlyMemory<RoomEnemyGraphicsSetMember> Records => members;
}
