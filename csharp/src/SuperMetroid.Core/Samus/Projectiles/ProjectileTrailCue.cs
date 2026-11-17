namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// A trail animation's move cue (#2348): it nudges the sparkle it names a pixel down, whichever sparkle's animation raises
/// it, so a pair both running the left ice sparkle's animation moves only its left sparkle, twice.
/// </summary>
public enum ProjectileTrailCue
{
    /// <summary>Moves the trail's left sparkle one pixel down.</summary>
    MoveLeftDown,
    /// <summary>Moves the trail's right sparkle one pixel down.</summary>
    MoveRightDown,
}
