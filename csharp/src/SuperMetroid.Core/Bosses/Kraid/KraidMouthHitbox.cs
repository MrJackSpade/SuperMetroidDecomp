namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The eight compiled Kraid mouth hitbox records at $A7:9788..97C7.</summary>
internal enum KraidMouthHitbox : ushort
{
    /// <summary>$A7:9788, Hitbox_KraidMouth_0, closed-mouth vulnerable shape.</summary>
    ClosedVulnerable = 0x9788,
    /// <summary>$A7:9790, Hitbox_KraidMouth_1, first opening-stage vulnerable shape.</summary>
    OpeningVulnerable = 0x9790,
    /// <summary>$A7:9798, Hitbox_KraidMouth_2, second opening-stage vulnerable shape.</summary>
    WiderVulnerable = 0x9798,
    /// <summary>$A7:97A0, Hitbox_KraidMouth_3, fully open vulnerable shape.</summary>
    OpenVulnerable = 0x97a0,
    /// <summary>$A7:97A8, Hitbox_KraidMouth_4, unused zero rectangle.</summary>
    EmptyShape = 0x97a8,
    /// <summary>$A7:97B0, Hitbox_KraidMouth_5, first opening-stage invulnerable shape.</summary>
    OpeningInvulnerable = 0x97b0,
    /// <summary>$A7:97B8, Hitbox_KraidMouth_6, second opening-stage invulnerable shape.</summary>
    WiderInvulnerable = 0x97b8,
    /// <summary>$A7:97C0, Hitbox_KraidMouth_7, fully open invulnerable shape.</summary>
    OpenInvulnerable = 0x97c0,
}
