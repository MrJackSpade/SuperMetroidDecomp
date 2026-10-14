namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The pose a Ridley death-breakup fragment shows (#641), in the authored order of the bank-$A6 one-frame lists
/// <c>$A6:CA47..CAEF</c>; each is also the fragment's typed program, which shows it for one update and then holds.
/// </summary>
internal enum RidleyBreakupFrame
{
    /// <summary>The first two tail segments ($A6:CA47).</summary>
    TailSegmentSmall,
    /// <summary>The third and fourth tail segments ($A6:CA4D).</summary>
    TailSegmentMedium,
    /// <summary>The fifth and sixth tail segments ($A6:CA53).</summary>
    TailSegmentLarge,
    /// <summary>The left-facing wings ($A6:CA59).</summary>
    WingsFacingLeft,
    /// <summary>The right-facing wings ($A6:CA5F).</summary>
    WingsFacingRight,
    /// <summary>The left-facing legs ($A6:CA65).</summary>
    LegsFacingLeft,
    /// <summary>The right-facing legs ($A6:CA6B).</summary>
    LegsFacingRight,
    /// <summary>The left-facing open head and neck ($A6:CA71).</summary>
    OpenHeadAndNeckFacingLeft,
    /// <summary>The right-facing open head and neck ($A6:CA77).</summary>
    OpenHeadAndNeckFacingRight,
    /// <summary>The left-facing torso ($A6:CA7D).</summary>
    TorsoFacingLeft,
    /// <summary>The right-facing torso ($A6:CA83).</summary>
    TorsoFacingRight,
    /// <summary>The left-facing claw ($A6:CA89).</summary>
    ClawFacingLeft,
    /// <summary>The right-facing claw ($A6:CA8F).</summary>
    ClawFacingRight,
    /// <summary>The tail tip pointing down, the first of sixteen orientations clockwise ($A6:CA95).</summary>
    TailTip0,
    /// <summary>Tail tip orientation 1 ($A6:CA9B).</summary>
    TailTip1,
    /// <summary>Tail tip orientation 2 ($A6:CAA1).</summary>
    TailTip2,
    /// <summary>Tail tip orientation 3 ($A6:CAA7).</summary>
    TailTip3,
    /// <summary>Tail tip orientation 4 ($A6:CAAD).</summary>
    TailTip4,
    /// <summary>Tail tip orientation 5 ($A6:CAB3).</summary>
    TailTip5,
    /// <summary>Tail tip orientation 6 ($A6:CAB9).</summary>
    TailTip6,
    /// <summary>Tail tip orientation 7 ($A6:CABF).</summary>
    TailTip7,
    /// <summary>Tail tip orientation 8 ($A6:CAC5).</summary>
    TailTip8,
    /// <summary>Tail tip orientation 9 ($A6:CACB).</summary>
    TailTip9,
    /// <summary>Tail tip orientation 10 ($A6:CAD1).</summary>
    TailTip10,
    /// <summary>Tail tip orientation 11 ($A6:CAD7).</summary>
    TailTip11,
    /// <summary>Tail tip orientation 12 ($A6:CADD).</summary>
    TailTip12,
    /// <summary>Tail tip orientation 13 ($A6:CAE3).</summary>
    TailTip13,
    /// <summary>Tail tip orientation 14 ($A6:CAE9).</summary>
    TailTip14,
    /// <summary>Tail tip orientation 15 ($A6:CAEF).</summary>
    TailTip15,
}
