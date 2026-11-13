namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The pose a Ridley death-breakup fragment shows (#641), in the order the breakup authors its
/// fragments; each is also the fragment's typed program, which shows it for one update and then holds.
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
    /// <summary>The tail tip pointing down, the first of sixteen orientations clockwise.</summary>
    TailTip0,
    /// <summary>Tail tip orientation 1.</summary>
    TailTip1,
    /// <summary>Tail tip orientation 2.</summary>
    TailTip2,
    /// <summary>Tail tip orientation 3.</summary>
    TailTip3,
    /// <summary>Tail tip orientation 4.</summary>
    TailTip4,
    /// <summary>Tail tip orientation 5.</summary>
    TailTip5,
    /// <summary>Tail tip orientation 6.</summary>
    TailTip6,
    /// <summary>Tail tip orientation 7.</summary>
    TailTip7,
    /// <summary>Tail tip orientation 8.</summary>
    TailTip8,
    /// <summary>Tail tip orientation 9.</summary>
    TailTip9,
    /// <summary>Tail tip orientation 10.</summary>
    TailTip10,
    /// <summary>Tail tip orientation 11.</summary>
    TailTip11,
    /// <summary>Tail tip orientation 12.</summary>
    TailTip12,
    /// <summary>Tail tip orientation 13.</summary>
    TailTip13,
    /// <summary>Tail tip orientation 14.</summary>
    TailTip14,
    /// <summary>Tail tip orientation 15.</summary>
    TailTip15,
}
