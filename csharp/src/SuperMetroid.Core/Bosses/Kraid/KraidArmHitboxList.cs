namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The sixteen ordered rectangle lists Kraid's arm components collide with.</summary>
internal enum KraidArmHitboxList
{
    /// <summary>Hitbox_KraidArm_0.</summary>
    Articulated0,
    /// <summary>Hitbox_KraidArm_1.</summary>
    Articulated1,
    /// <summary>Hitbox_KraidArm_2.</summary>
    Articulated2,
    /// <summary>Hitbox_KraidArm_3.</summary>
    Articulated3,
    /// <summary>Hitbox_KraidArm_4.</summary>
    Articulated4,
    /// <summary>Hitbox_KraidArm_5.</summary>
    Articulated5,
    /// <summary>Hitbox_KraidArm_6.</summary>
    Articulated6,
    /// <summary>Hitbox_KraidArm_8.</summary>
    ConnectorSecond,
    /// <summary>Hitbox_KraidArm_9.</summary>
    ConnectorThird,
    /// <summary>Hitbox_KraidArm_A.</summary>
    ConnectorFinal,
    /// <summary>Hitbox_KraidArm_F.</summary>
    ConnectorInitial,
    /// <summary>Hitbox_KraidArm_10.</summary>
    BodyFinal,
    /// <summary>Hitbox_KraidArm_11.</summary>
    BodyMiddle,
    /// <summary>Hitbox_KraidArm_12.</summary>
    BodyInitial,
    /// <summary>the first dying/preparing-to-lunge list.</summary>
    FinalPoseFirst,
    /// <summary>the second dying/preparing-to-lunge list.</summary>
    FinalPoseSecond,
}
