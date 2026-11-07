using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The members one enemy graphics set loads, in record order (#1527): the enemies whose sheets the import composes for
/// the rooms that name the set. The game knows a set only by its name (<see cref="RoomEnemyGraphicsSetDefinition"/>).
/// </summary>
/// <param name="name">The set's name.</param>
/// <param name="records">The set's ordered members.</param>
internal sealed class RoomEnemyGraphicsSetMembers(string name, RoomEnemyGraphicsSetMember[] records)
{
    /// <summary>The members in the set's order.</summary>
    private readonly RoomEnemyGraphicsSetMember[] members = Ensure.NotNull(records);

    /// <summary>The set's name.</summary>
    internal string Name { get; } = Ensure.NotNull(name);

    /// <summary>The members in the set's order; a member listed twice is retained twice.</summary>
    internal ReadOnlyMemory<RoomEnemyGraphicsSetMember> Records => members;
}
