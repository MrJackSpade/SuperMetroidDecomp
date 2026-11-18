namespace SuperMetroid.Core.Samus.Colours;

/// <summary>
/// One of Samus's named shade sequences (#2351): the charge pulse her suit shows while she is charged, in each suit for a
/// charged beam and for the pseudo Screw Attack, the glows her suit shows after a charged shot and a Hyper Beam shot, and
/// the Hyper Beam's own beam colour cycle; the Crystal Flash's pulse and bubble, the drained Samus's rainbow, the visor's
/// X-ray widening and steady cycle, and the stored shine, shinespark, Screw Attack and Speed Booster glows in each suit
/// (#2387). Values are arbitrary.
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

    /// <summary>The stored shine in the Power Suit (#2387).</summary>
    StoredShinePower,

    /// <summary>The stored shine in the Varia Suit (#2387).</summary>
    StoredShineVaria,

    /// <summary>The stored shine in the Gravity Suit (#2387).</summary>
    StoredShineGravity,

    /// <summary>The shinespark's glow in the Power Suit (#2387).</summary>
    ActiveShinesparkPower,

    /// <summary>The shinespark's glow in the Varia Suit (#2387).</summary>
    ActiveShinesparkVaria,

    /// <summary>The shinespark's glow in the Gravity Suit (#2387).</summary>
    ActiveShinesparkGravity,

    /// <summary>The Screw Attack's glow in the Power Suit (#2387).</summary>
    ScrewAttackPower,

    /// <summary>The Screw Attack's glow in the Varia Suit (#2387).</summary>
    ScrewAttackVaria,

    /// <summary>The Screw Attack's glow in the Gravity Suit (#2387).</summary>
    ScrewAttackGravity,

    /// <summary>The Speed Booster's glow rising to its brightest in the Power Suit (#2387).</summary>
    SpeedBoosterRisingPower,

    /// <summary>The Speed Booster's glow rising to its brightest in the Varia Suit (#2387).</summary>
    SpeedBoosterRisingVaria,

    /// <summary>The Speed Booster's glow rising to its brightest in the Gravity Suit (#2387).</summary>
    SpeedBoosterRisingGravity,

    /// <summary>The Speed Booster's brightest glow in the Power Suit, which it rests on (#2387).</summary>
    SpeedBoosterBrightestPower,

    /// <summary>The Speed Booster's brightest glow in the Varia Suit, which it rests on (#2387).</summary>
    SpeedBoosterBrightestVaria,

    /// <summary>The Speed Booster's brightest glow in the Gravity Suit, which it rests on (#2387).</summary>
    SpeedBoosterBrightestGravity,
}
