namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>Oum's animation: idle, rolling forwards or backwards, or attacking, facing left or right.</summary>
public enum MaridiaLargeSnailAnimation
{
    /// <summary>The one-frame, sleeping right-facing idle.</summary>
    FacingRightIdle,
    /// <summary>The one-frame, sleeping left-facing idle.</summary>
    FacingLeftIdle,
    /// <summary>The right-facing rolling loop that changes attack-rotation permission.</summary>
    FacingRightRollingForwards,
    /// <summary>The corresponding left-facing rolling loop.</summary>
    FacingLeftRollingForwards,
    /// <summary>The right-facing attack with its splash sound and completion cue.</summary>
    FacingRightAttacking,
    /// <summary>The left-facing attack with its splash sound and completion cue.</summary>
    FacingLeftAttacking,
    /// <summary>The right-facing backward roll that loops to its own first frame.</summary>
    FacingRightRollingBackwards,
    /// <summary>The left-facing backward roll that loops to its own first frame.</summary>
    FacingLeftRollingBackwards,
}
