namespace SuperMetroid.Core.Enemies;

/// <summary>
/// How one enemy's entity is built where it is set up (#2554): its species' creation with the settings, and the
/// surroundings or encounter, that creation takes, all bound before the build.
/// </summary>
internal interface IEnemyBuild
{
    /// <summary>Builds the entity for <paramref name="occupant"/>, which <paramref name="place"/> now holds.</summary>
    /// <param name="occupant">The occupant just installed.</param>
    /// <param name="place">The place, already holding the occupant and its live values.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    /// <returns>The entity with the setup and main AI it runs.</returns>
    EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common);
}
