namespace SuperMetroid.Core.Enemies;

/// <summary>Whether a placement leads or follows another lava jumper or Rio (#1472), the one its room lists it with (<see cref="EnemyFollowerListingDefinitions"/>).</summary>
internal enum EnemyFollowerRole
{
    /// <summary>It leads.</summary>
    Leader,

    /// <summary>It follows its leader.</summary>
    Follower,
}
