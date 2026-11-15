namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// The gunship liftoff dust clouds' typed animations (#641): three cloud shapes on each side of the ship, each showing its
/// poses once and deleting itself.
/// </summary>
internal enum GunshipDustSegment
{
    /// <summary>The inner cloud right of the ship.</summary>
    RightInner,
    /// <summary>The middle cloud right of the ship.</summary>
    RightMiddle,
    /// <summary>The outer cloud right of the ship.</summary>
    RightOuter,
    /// <summary>The inner cloud left of the ship.</summary>
    LeftInner,
    /// <summary>The middle cloud left of the ship.</summary>
    LeftMiddle,
    /// <summary>The outer cloud left of the ship.</summary>
    LeftOuter,
}
