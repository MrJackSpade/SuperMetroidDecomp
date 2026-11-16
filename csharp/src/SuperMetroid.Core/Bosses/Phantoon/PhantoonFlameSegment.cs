namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's typed flame animations (#641, #1788): one for each animation a flame's spawn or reaction starts, in authored order.</summary>
internal enum PhantoonFlameSegment
{
    /// <summary>The destroyable flame's three-pose idle loop.</summary>
    DestroyableIdle,
    /// <summary>A casual flame's landing pose.</summary>
    CasualHitGround,
    /// <summary>A casual flame's three-pose bouncing loop.</summary>
    CasualBouncing,
    /// <summary>A casual flame's six resting poses, running on into <see cref="Dying"/>.</summary>
    CasualResting,
    /// <summary>A flame's four dying poses, then its deletion.</summary>
    Dying,
    /// <summary>A rain flame's impact pose, then <see cref="Dying"/>.</summary>
    RainImpact,
    /// <summary>A casual flame falling: its kindling pose and nothing, one update each, the first of its three flickers.</summary>
    CasualFallingKindling,
    /// <summary>A falling flame's second flicker: its teardrop pose and nothing.</summary>
    CasualFallingTeardrop,
    /// <summary>A falling flame's third flicker: its sputtering pose and nothing.</summary>
    CasualFallingSputtering,
    /// <summary>A starting flame's three-pose loop.</summary>
    StartingFlame,
    /// <summary>Deletes the flame.</summary>
    Delete,
    /// <summary>A shot flame's four poses, its drop and its deletion.</summary>
    DestroyableShot,
}
