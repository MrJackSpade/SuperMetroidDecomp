using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.MotherBrain.Artwork;

/// <summary>The colour runs the <see cref="MotherBrainColours"/> view names (#1405).</summary>
public static class MotherBrainColourDefinitions
{
    /// <summary>Body colours.</summary>
    public static readonly BackgroundColourRun Body = new(BackgroundColourSet.TilesThird, 1, 15);

    /// <summary>Brain colours.</summary>
    public static readonly SpriteColourRun Brain = new(SpriteColourStore.EnemiesFirst, 1, 15);

    /// <summary>Rear-leg colours.</summary>
    public static readonly SpriteColourRun RearLegs = new(SpriteColourStore.EnemiesThird, 1, 15);

    /// <summary>The drain-faded rear-leg colours.</summary>
    public static readonly SpriteColourRun DrainedRearLegs = new(SpriteColourStore.EnemiesThird, 4, 5);

    /// <summary>The fake-death brain colours.</summary>
    public static readonly SpriteColourRun FakeDeathBrain = new(SpriteColourStore.EnemiesFirst, 1, 3);

    /// <summary>First room-flash slice.</summary>
    public static readonly BackgroundColourRun RoomFirst = new(BackgroundColourSet.TilesSecond, 4, 12);

    /// <summary>Second room-flash slice.</summary>
    public static readonly BackgroundColourRun RoomSecond = new(BackgroundColourSet.TilesFourth, 3, 12);

    /// <summary>Mirror of the second room-flash slice.</summary>
    public static readonly BackgroundColourRun RoomSecondMirror = new(BackgroundColourSet.TilesSixth, 3, 12);

    /// <summary>Phase-two attack sprites.</summary>
    public static readonly SpriteColourRun PhaseTwoAttack = new(SpriteColourStore.EnemiesSecond, 1, 15);

    /// <summary>Phase-two rear legs.</summary>
    public static readonly SpriteColourRun PhaseTwoRearLegs = new(SpriteColourStore.EnemiesThird, 1, 15);

    /// <summary>Room-entry glass shards.</summary>
    public static readonly SpriteColourRun GlassShards = new(SpriteColourStore.EnemiesThird, 1, 15);

    /// <summary>Room-entry tube projectiles.</summary>
    public static readonly SpriteColourRun TubeProjectiles = new(SpriteColourStore.EnemiesFourth, 1, 15);

    /// <summary>First room-light slice.</summary>
    public static readonly BackgroundColourRun RecoveryLightsFirst = new(BackgroundColourSet.TilesSecond, 1, 14);

    /// <summary>Second room-light slice.</summary>
    public static readonly BackgroundColourRun RecoveryLightsSecond = new(BackgroundColourSet.TilesFourth, 1, 14);
}
