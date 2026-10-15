namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Where a Samus animation segment repeats or which poses it selects by equipment (#633).</summary>
internal sealed record SamusAnimationTransition
{
    /// <summary>Address of the frame selected by <see cref="SamusAnimationCommand.RepeatFrom"/>.</summary>
    public ushort RepeatTarget { get; init; } = 0;

    /// <summary>Equipment tested by the item-selected transitions.</summary>
    public SamusEquipmentFlags Item { get; init; } = 0;

    /// <summary>Pose for an unequipped item at rest, and the only pose of <c>$F8</c>/<c>$FD</c>.</summary>
    public SamusPoseId UnequippedGrounded { get; init; } = 0;

    /// <summary>Unequipped airborne.</summary>
    public SamusPoseId UnequippedAirborne { get; init; } = 0;

    /// <summary>Equipped grounded.</summary>
    public SamusPoseId EquippedGrounded { get; init; } = 0;

    /// <summary>Equipped airborne.</summary>
    public SamusPoseId EquippedAirborne { get; init; } = 0;

    /// <summary>The source writes this <c>$FD</c> pose as a little-endian word. The interpreter reads only its low byte, so the zero high byte is never executed as a frame.</summary>
    public bool WordPose { get; init; } = false;
}
