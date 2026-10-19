namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>How one of Crocomire's collision rectangles reacts when Samus touches it.</summary>
internal enum CrocomireHitboxTouch
{
    /// <summary>The common enemy contact reaction: the upper body's rectangles while Crocomire fights.</summary>
    Common = 1,
    /// <summary>Crocomire's claw contact reaction: the body, mouth and lower-body rectangles.</summary>
    Claw,
    /// <summary>No reaction: the corpse's rectangles, whose touch returns at once.</summary>
    Inert,
}
