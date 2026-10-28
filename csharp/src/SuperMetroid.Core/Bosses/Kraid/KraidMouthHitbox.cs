namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The eight compiled Kraid mouth hitbox records at $A7:9788..97C7.</summary>
internal enum KraidMouthHitbox
{
    /// <summary>$A7:9788, Hitbox_KraidMouth_0, closed-mouth vulnerable shape.</summary>
    ClosedVulnerable = 1,
    /// <summary>$A7:9790, Hitbox_KraidMouth_1, first opening-stage vulnerable shape.</summary>
    OpeningVulnerable,
    /// <summary>$A7:9798, Hitbox_KraidMouth_2, second opening-stage vulnerable shape.</summary>
    WiderVulnerable,
    /// <summary>$A7:97A0, Hitbox_KraidMouth_3, fully open vulnerable shape.</summary>
    OpenVulnerable,
    /// <summary>$A7:97A8, Hitbox_KraidMouth_4, unused zero rectangle.</summary>
    EmptyShape,
    /// <summary>$A7:97B0, Hitbox_KraidMouth_5, first opening-stage invulnerable shape.</summary>
    OpeningInvulnerable,
    /// <summary>$A7:97B8, Hitbox_KraidMouth_6, second opening-stage invulnerable shape.</summary>
    WiderInvulnerable,
    /// <summary>$A7:97C0, Hitbox_KraidMouth_7, fully open invulnerable shape.</summary>
    OpenInvulnerable,
}
