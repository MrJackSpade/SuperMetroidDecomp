namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// One of Samus's named glows (#2351, #2387, #2557): the charge pulse her suit shows while she is charged, for a charged beam
/// and for the pseudo Screw Attack, the glows her suit shows after a charged shot and a Hyper Beam shot, and the Hyper
/// Beam's own beam colour cycle; the Crystal Flash's pulse and bubble, the drained Samus's rainbow, the visor's X-ray
/// widening and steady cycle, and the stored shine, shinespark, Screw Attack and Speed Booster glows. A glow that differs
/// by suit is shown in the suit Samus wears (<see cref="SamusGlowSegment"/>). Values are arbitrary.
/// </summary>
public enum SamusGlow
{
    /// <summary>A charged beam's pulse, in each suit.</summary>
    ChargedBeamPulse,

    /// <summary>The pseudo Screw Attack's pulse, in each suit.</summary>
    PseudoScrewPulse,

    /// <summary>An ordinary charged shot's yellow flash.</summary>
    ChargedShotFlash,

    /// <summary>A Hyper Beam shot's glow, which walks the Hyper Beam's full-body glow backward.</summary>
    HyperShotGlow,

    /// <summary>The Hyper Beam's own colour cycle over the beams.</summary>
    HyperBeamCycle,

    /// <summary>The Crystal Flash's pulse over Samus's body (#2387).</summary>
    CrystalFlashBody,

    /// <summary>The Crystal Flash's bubble turning through its colours around her (#2387).</summary>
    CrystalFlashBubble,

    /// <summary>The drained Samus's rainbow, which walks the Hyper Beam's full-body glow (#2387).</summary>
    HyperBeamRainbow,

    /// <summary>The visor widening with the X-ray beam, up to its widest shade (#2387).</summary>
    XrayVisorWidening,

    /// <summary>The visor's widest X-ray shade, which the widening rests on (#2387).</summary>
    XrayVisorWidest,

    /// <summary>The steady visor cycle the full X-ray beam and the room show (#2387).</summary>
    VisorCycle,

    /// <summary>The stored shine, in each suit (#2387).</summary>
    StoredShine,

    /// <summary>The shinespark's glow, in each suit (#2387).</summary>
    ActiveShinespark,

    /// <summary>The Screw Attack's glow, in each suit (#2387).</summary>
    ScrewAttack,

    /// <summary>The Speed Booster's glow rising to its brightest, in each suit (#2387).</summary>
    SpeedBoosterRising,

    /// <summary>The Speed Booster's brightest glow, which it rests on, in each suit (#2387).</summary>
    SpeedBoosterBrightest,
}
