namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy holding a room enemy place (#2558): its identity. A place holds its occupant from the binding that installs
/// it until the occupant leaves; a place that holds no enemy holds no occupant, so no identity stands for an empty place.
/// </summary>
/// <param name="definitionId">The occupant's enemy definition.</param>
public sealed class RoomEnemyOccupant(EnemyDefinitionId definitionId)
{
    /// <summary>The occupant's enemy definition, which the game re-reads each time the enemy runs.</summary>
    public EnemyDefinitionId DefinitionId { get; } = definitionId;
}
