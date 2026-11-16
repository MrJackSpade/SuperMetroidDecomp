namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// The sixteen rectangle lists Kraid's arm components collide with (#2211), each named for the arm's pose its shape
/// follows: a limb and the connector joining it, the part of his body behind the arm, and the dying or lunge-ready
/// arm's two poses.
/// </summary>
internal enum KraidArmHitboxList
{
    /// <summary>A limb raised with its claw closed.</summary>
    LimbRaised,
    /// <summary>A limb raised with its claw opening.</summary>
    LimbRaisedOpen,
    /// <summary>A limb lowering, its claw hooked.</summary>
    LimbLowering,
    /// <summary>A limb held level.</summary>
    LimbLevel,
    /// <summary>A level limb, its claw hanging down.</summary>
    LimbHanging,
    /// <summary>A level limb, its claw tucked.</summary>
    LimbTucked,
    /// <summary>A level limb, its claw folded.</summary>
    LimbFolded,
    /// <summary>The connector of a lowered limb.</summary>
    ConnectorLowered,
    /// <summary>The connector of a limb whose claw hangs down.</summary>
    ConnectorHanging,
    /// <summary>The connector of a limb whose claw is tucked or folded.</summary>
    ConnectorTucked,
    /// <summary>The connector of a raised limb.</summary>
    ConnectorRaised,
    /// <summary>His body behind a lowered arm.</summary>
    BodyBehindLoweredArm,
    /// <summary>His body behind a lowering or level arm.</summary>
    BodyBehindLoweringArm,
    /// <summary>His body behind a raised arm.</summary>
    BodyBehindRaisedArm,
    /// <summary>The dying or lunge-ready arm drooping diagonally.</summary>
    DroopingDiagonal,
    /// <summary>The dying or lunge-ready arm outstretched.</summary>
    Outstretched,
}
