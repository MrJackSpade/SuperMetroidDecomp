namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// One of Samus's named shade sequences (#2351): the charge pulse her suit shows while she is charged, in each suit for a
/// charged beam and for the pseudo Screw Attack, the glows her suit shows after a charged shot and a Hyper Beam shot, and
/// the Hyper Beam's own beam colour cycle. Values are arbitrary.
/// </summary>
public enum SamusGlowSegment
{
    /// <summary>A charged beam's pulse in the Power Suit.</summary>
    ChargedBeamPowerPulse,

    /// <summary>A charged beam's pulse in the Varia Suit.</summary>
    ChargedBeamVariaPulse,

    /// <summary>A charged beam's pulse in the Gravity Suit.</summary>
    ChargedBeamGravityPulse,

    /// <summary>The pseudo Screw Attack's pulse in the Power Suit.</summary>
    PseudoScrewPowerPulse,

    /// <summary>The pseudo Screw Attack's pulse in the Varia Suit.</summary>
    PseudoScrewVariaPulse,

    /// <summary>The pseudo Screw Attack's pulse in the Gravity Suit.</summary>
    PseudoScrewGravityPulse,

    /// <summary>An ordinary charged shot's yellow flash.</summary>
    ChargedShotFlash,

    /// <summary>A Hyper Beam shot's glow, which walks the Hyper Beam's full-body glow backward.</summary>
    HyperShotGlow,

    /// <summary>The Hyper Beam's own colour cycle over the beams.</summary>
    HyperBeamCycle,
}
