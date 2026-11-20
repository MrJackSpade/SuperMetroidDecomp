namespace SuperMetroid.Core.Enemies;

/// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has (#2553).</summary>
/// <param name="place">
/// The place, already holding the new identity and, for an occupant placed from a population entry, the entry it is spawned
/// from.
/// </param>
/// <param name="common">The shared participation the entity composes.</param>
internal delegate EnemyOccupant EnemyEntityCreation(RoomEnemyPlace place, EnemyCommonParticipation common);
