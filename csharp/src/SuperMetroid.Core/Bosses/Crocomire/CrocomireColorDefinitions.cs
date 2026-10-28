namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The shapes of Crocomire's colour transfers.</summary>
public static class CrocomireColorDefinitions
{
    /// <summary>The body and head colours restored after a hurt flash.</summary>
    public const int FightBodyCount = 8;

    /// <summary>The colours of the initial wall transfer; its last is also the projectile transfer's first.</summary>
    public const int InitialWallCount = 17;

    /// <summary>
    /// The colours of the initial projectile transfer the port carries: sprite set 5's sixteen. The cartridge's seventeenth,
    /// the skeleton arm's first source word, lands on sprite set 6's transparent colour 0 and is not carried (#1436).
    /// </summary>
    public const int InitialProjectileCount = 16;

    /// <summary>The colours of the skeleton arm's palette, loaded as the wall breaks.</summary>
    public const int SkeletonArmCount = 16;

    /// <summary>The colours of the wall spikes' palette, loaded when the rumble ends.</summary>
    public const int WallSpikesCount = 16;
}
