namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Creates the part of <paramref name="species"/> that one of its definitions places: the entity the occupant of
/// <paramref name="place"/> becomes, joining the room's <paramref name="common"/> participation. A species holds one for
/// each definition it serves, keyed by that definition, and its definitions are that table's keys.
/// </summary>
/// <typeparam name="TSpecies">The species creating the part.</typeparam>
/// <param name="species">The species creating the part, whose room services the part uses.</param>
/// <param name="occupant">The occupant the definition places.</param>
/// <param name="place">The place it occupies.</param>
/// <param name="common">Its participation in the room's common enemy handling.</param>
internal delegate EnemyOccupant EnemyPartFactory<in TSpecies>(TSpecies species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common);
