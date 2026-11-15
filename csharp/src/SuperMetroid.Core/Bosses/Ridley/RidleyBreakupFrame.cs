namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The pose a Ridley death-breakup fragment shows (#641), in the order the breakup authors its
/// fragments; each is also the fragment's typed animation, which shows it for one update and then holds.
/// </summary>
internal enum RidleyBreakupFrame
{
    /// <summary>The first two tail segments.</summary>
    TailSegmentSmall,
    /// <summary>The third and fourth tail segments.</summary>
    TailSegmentMedium,
    /// <summary>The fifth and sixth tail segments.</summary>
    TailSegmentLarge,
    /// <summary>The left-facing wings.</summary>
    WingsFacingLeft,
    /// <summary>The right-facing wings.</summary>
    WingsFacingRight,
    /// <summary>The left-facing legs.</summary>
    LegsFacingLeft,
    /// <summary>The right-facing legs.</summary>
    LegsFacingRight,
    /// <summary>The left-facing open head and neck.</summary>
    OpenHeadAndNeckFacingLeft,
    /// <summary>The right-facing open head and neck.</summary>
    OpenHeadAndNeckFacingRight,
    /// <summary>The left-facing torso.</summary>
    TorsoFacingLeft,
    /// <summary>The right-facing torso.</summary>
    TorsoFacingRight,
    /// <summary>The left-facing claw.</summary>
    ClawFacingLeft,
    /// <summary>The right-facing claw.</summary>
    ClawFacingRight,
    /// <summary>The tail tip pointing straight down, the first of sixteen directions clockwise.</summary>
    TailTipPointingDown,
    /// <summary>The tail tip pointing down, a little left.</summary>
    TailTipPointingDownDownLeft,
    /// <summary>The tail tip pointing down and left.</summary>
    TailTipPointingDownLeft,
    /// <summary>The tail tip pointing left, a little down.</summary>
    TailTipPointingLeftDownLeft,
    /// <summary>The tail tip pointing straight left.</summary>
    TailTipPointingLeft,
    /// <summary>The tail tip pointing left, a little up.</summary>
    TailTipPointingLeftUpLeft,
    /// <summary>The tail tip pointing up and left.</summary>
    TailTipPointingUpLeft,
    /// <summary>The tail tip pointing up, a little left.</summary>
    TailTipPointingUpUpLeft,
    /// <summary>The tail tip pointing straight up.</summary>
    TailTipPointingUp,
    /// <summary>The tail tip pointing up, a little right.</summary>
    TailTipPointingUpUpRight,
    /// <summary>The tail tip pointing up and right.</summary>
    TailTipPointingUpRight,
    /// <summary>The tail tip pointing right, a little up.</summary>
    TailTipPointingRightUpRight,
    /// <summary>The tail tip pointing straight right.</summary>
    TailTipPointingRight,
    /// <summary>The tail tip pointing right, a little down.</summary>
    TailTipPointingRightDownRight,
    /// <summary>The tail tip pointing down and right.</summary>
    TailTipPointingDownRight,
    /// <summary>The tail tip pointing down, a little right.</summary>
    TailTipPointingDownDownRight,
}
