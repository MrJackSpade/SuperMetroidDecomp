namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's seven mouth shapes: one outer shape per mouth stage and the inner shape of each open stage.</summary>
internal enum KraidMouthHitbox
{
    /// <summary>Closed-mouth vulnerable shape.</summary>
    ClosedVulnerable,
    /// <summary>First opening-stage vulnerable shape.</summary>
    OpeningVulnerable,
    /// <summary>Second opening-stage vulnerable shape.</summary>
    WiderVulnerable,
    /// <summary>Fully open vulnerable shape.</summary>
    OpenVulnerable,
    /// <summary>First opening-stage invulnerable shape.</summary>
    OpeningInvulnerable,
    /// <summary>Second opening-stage invulnerable shape.</summary>
    WiderInvulnerable,
    /// <summary>Fully open invulnerable shape.</summary>
    OpenInvulnerable,
}
