namespace SuperMetroid.Core.Enemies;

/// <summary>Whether a lava jumper or Rio follows its leader rather than leads (#1472), the leader its room lists it with.</summary>
/// <param name="Role">Whether it leads or follows its leader.</param>
internal sealed record EnemyFollowerSpawnSettings(EnemyFollowerRole Role) : EnemySpawnSettings;
