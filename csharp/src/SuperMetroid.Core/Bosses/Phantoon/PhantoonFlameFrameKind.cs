namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The flame program a Phantoon flame frame belongs to.</summary>
internal enum PhantoonFlameFrameKind
{
    /// <summary>The destroyable flame's idle loop.</summary>
    DestroyableIdle,
    /// <summary>A casual flame landing.</summary>
    CasualHitGround,
    /// <summary>A casual flame bouncing.</summary>
    CasualBouncing,
    /// <summary>A casual flame resting.</summary>
    CasualResting,
    /// <summary>A flame dying.</summary>
    Dying,
    /// <summary>A rain flame's impact.</summary>
    RainImpact,
    /// <summary>A casual flame falling: two poses in each of three phases.</summary>
    CasualFalling,
    /// <summary>A starting flame.</summary>
    StartingFlame,
    /// <summary>A shot flame bursting.</summary>
    DestroyableShot,
}