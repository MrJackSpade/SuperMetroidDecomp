namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Builds a group's part in its place, holding the group's head from the head's setup (#2554): the population load builds
/// the part with it when it reaches the part's place, and a respawn of the part builds it again with it.
/// </summary>
/// <param name="place">The part's place, already holding its identity and entry.</param>
/// <param name="common">The shared participation the entity composes.</param>
/// <param name="setup">How the part comes to be set up.</param>
internal delegate IEnemyEntity EnemyPartBuild(RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup);
