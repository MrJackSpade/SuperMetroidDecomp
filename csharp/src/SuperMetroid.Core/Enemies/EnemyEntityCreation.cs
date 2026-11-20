namespace SuperMetroid.Core.Enemies;

/// <summary>Creates the entity of <paramref name="occupant"/>, newly installed in <paramref name="place"/>, with the setup it has (#2553).</summary>
/// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
/// <param name="place">The place, already holding the occupant.</param>
/// <param name="common">The shared participation the entity composes.</param>
internal delegate EnemyOccupant EnemyEntityCreation(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common);
