namespace SuperMetroid.Core.Enemies;

/// <summary>One member of a room's enemy graphics set, in the set's order: the enemy whose artwork the set loads.</summary>
/// <remarks>
/// The colours a member draws in are its installed sheet's (#1512) and its pictures are its sheet's frames (#1513): the
/// import composes and pre-colours each member's sheet as its graphics set loads it, so a member names nothing else.
/// </remarks>
/// <param name="DefinitionId">The member enemy, which selects its installed sheets and its spawned objects' sheet.</param>
public readonly record struct RoomEnemyGraphicsSetMember(EnemyDefinitionId DefinitionId);
