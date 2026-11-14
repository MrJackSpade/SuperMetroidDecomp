namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The eight compiled Kraid mouth hitboxes.</summary>
internal enum KraidMouthHitbox
{
    /// <summary>Closed-mouth vulnerable shape.</summary>
    ClosedVulnerable = 1,
    /// <summary>First opening-stage vulnerable shape.</summary>
    OpeningVulnerable,
    /// <summary>Second opening-stage vulnerable shape.</summary>
    WiderVulnerable,
    /// <summary>Fully open vulnerable shape.</summary>
    OpenVulnerable,
    /// <summary>Unused zero rectangle.</summary>
    EmptyShape,
    /// <summary>First opening-stage invulnerable shape.</summary>
    OpeningInvulnerable,
    /// <summary>Second opening-stage invulnerable shape.</summary>
    WiderInvulnerable,
    /// <summary>Fully open invulnerable shape.</summary>
    OpenInvulnerable,
}
