namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>How one of Crocomire's collision rectangles reacts when a Samus projectile hits it.</summary>
internal enum CrocomireHitboxShot
{
    /// <summary>The common enemy shot reaction: the upper body's outer rectangles and the broad high body.</summary>
    Common = 1,
    /// <summary>Counts the hit in the fight flags without other effect: the raised, upper and high mouth and the lower central body.</summary>
    CountHit,
    /// <summary>Spawns shot dust: the full-height central body and the corpse.</summary>
    Dust,
    /// <summary>Crocomire's mouth reaction: the first rectangle of each upper-body list.</summary>
    Mouth,
    /// <summary>Spawns shot dust through the alternate entry: the lower and mid-body rectangles and the jaw strip.</summary>
    AlternateDust,
}
