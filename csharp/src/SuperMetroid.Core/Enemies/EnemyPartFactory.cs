namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Creates the part of a species that one of its definitions places: the entity the occupant of <paramref name="place"/>
/// becomes, joining the room's <paramref name="common"/> participation.
/// </summary>
/// <param name="occupant">The occupant the definition places.</param>
/// <param name="place">The place it occupies.</param>
/// <param name="common">Its participation in the room's common enemy handling.</param>
internal delegate EnemyOccupant EnemyPartFactory(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common);
