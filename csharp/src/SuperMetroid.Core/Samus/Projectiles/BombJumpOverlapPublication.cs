namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>When a bomb frame publishes Samus's bomb-jump overlap.</summary>
public enum BombJumpOverlapPublication
{
    /// <summary>During the bomb frame.</summary>
    Immediate,

    /// <summary>At the main loop's later overlap pass.</summary>
    Deferred,
}
