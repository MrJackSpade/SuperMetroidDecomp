namespace SuperMetroid.Core.Enemies.Dragon;

/// <summary>
/// Dragon's animations: the idle body, the cosmetic wings and the attacking body, each facing left or right.
/// </summary>
public enum DragonAnimationSelector
{
    /// <summary>The left-facing body's one-frame pose, then sleep.</summary>
    IdleFacingLeft,
    /// <summary>The right-facing body's one-frame pose, then sleep.</summary>
    IdleFacingRight,
    /// <summary>The separate cosmetic wing slot's looping pair of five-update poses, facing left.</summary>
    WingsFacingLeft,
    /// <summary>The cosmetic wing loop facing right.</summary>
    WingsFacingRight,
    /// <summary>The five-pose extension and retraction facing left, ending in the completion cue and sleep.</summary>
    AttackingFacingLeft,
    /// <summary>The mirrored body attack facing right, whose completion lets AI launch a fireball and restart the attack.</summary>
    AttackingFacingRight,
}
