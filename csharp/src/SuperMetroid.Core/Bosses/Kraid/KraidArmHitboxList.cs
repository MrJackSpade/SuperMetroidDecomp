namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The sixteen ordered rectangle lists Kraid's arm components collide with.</summary>
internal enum KraidArmHitboxList
{
    /// <summary>Kraid arm hitbox 0.</summary>
    Articulated0,
    /// <summary>Kraid arm hitbox 1.</summary>
    Articulated1,
    /// <summary>Kraid arm hitbox 2.</summary>
    Articulated2,
    /// <summary>Kraid arm hitbox 3.</summary>
    Articulated3,
    /// <summary>Kraid arm hitbox 4.</summary>
    Articulated4,
    /// <summary>Kraid arm hitbox 5.</summary>
    Articulated5,
    /// <summary>Kraid arm hitbox 6.</summary>
    Articulated6,
    /// <summary>Kraid arm hitbox 8.</summary>
    ConnectorSecond,
    /// <summary>Kraid arm hitbox 9.</summary>
    ConnectorThird,
    /// <summary>Kraid arm hitbox: a.</summary>
    ConnectorFinal,
    /// <summary>Kraid arm hitbox: f.</summary>
    ConnectorInitial,
    /// <summary>Kraid arm hitbox 10.</summary>
    BodyFinal,
    /// <summary>Kraid arm hitbox 11.</summary>
    BodyMiddle,
    /// <summary>Kraid arm hitbox 12.</summary>
    BodyInitial,
    /// <summary>the first dying/preparing-to-lunge list.</summary>
    FinalPoseFirst,
    /// <summary>the second dying/preparing-to-lunge list.</summary>
    FinalPoseSecond,
}
