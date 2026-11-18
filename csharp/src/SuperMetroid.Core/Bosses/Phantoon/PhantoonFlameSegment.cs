namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's typed flame segments (#641, #1788): one for each animation a flame's spawn or reaction starts, and the runs
/// those animations play on into.
/// </summary>
internal enum PhantoonFlameSegment
{
    /// <summary>A flame's three-pose burning loop: a destroyable flame idles in it and a casual flame bounces in it.</summary>
    Burning,
    /// <summary>A casual flame's landing pose.</summary>
    CasualHitGround,
    /// <summary>A casual flame's six resting poses, running on into <see cref="Dying"/>.</summary>
    CasualResting,
    /// <summary>
    /// A flame's four dying poses: a resting or impacting flame dies and is deleted, and a shot flame dies, leaves its drop
    /// and is deleted.
    /// </summary>
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
    /// <summary>Requests a shot flame's drop as it dies.</summary>
    Drop,
}
