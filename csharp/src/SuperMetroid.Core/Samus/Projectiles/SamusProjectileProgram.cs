namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The Samus projectiles' bank-$93 programs (#1301), one per list a header or loop enters.</summary>
public enum SamusProjectileProgram
{
    /// <summary>The list at <c>$93:86DB</c>.</summary>
    PowerUpFacingRight,
    /// <summary>The list at <c>$93:86E7</c>.</summary>
    PowerUpRight,
    /// <summary>The list at <c>$93:86F3</c>.</summary>
    PowerRight,
    /// <summary>The list at <c>$93:86FF</c>.</summary>
    PowerDownRight,
    /// <summary>The list at <c>$93:870B</c>.</summary>
    PowerDownFacingRight,
    /// <summary>The list at <c>$93:8717</c>.</summary>
    PowerDownLeft,
    /// <summary>The list at <c>$93:8723</c>.</summary>
    PowerLeft,
    /// <summary>The list at <c>$93:872F</c>.</summary>
    PowerUpLeft,
    /// <summary>The list at <c>$93:873B</c>.</summary>
    WaveUpFacingRight,
    /// <summary>The list at <c>$93:8743</c>.</summary>
    WaveDownFacingRight,
    /// <summary>The list at <c>$93:87C7</c>.</summary>
    WaveUpRight,
    /// <summary>The list at <c>$93:884B</c>.</summary>
    WaveRight,
    /// <summary>The list at <c>$93:88CF</c>.</summary>
    WaveDownRight,
    /// <summary>The list at <c>$93:8953</c>.</summary>
    IceUpFacingRight,
    /// <summary>The list at <c>$93:8977</c>.</summary>
    SpazerUpFacingRight,
    /// <summary>The list at <c>$93:8987</c>.</summary>
    SpazerUpFacingRightLoop,
    /// <summary>The list at <c>$93:8993</c>.</summary>
    SpazerUpRight,
    /// <summary>The list at <c>$93:89A3</c>.</summary>
    SpazerUpRightLoop,
    /// <summary>The list at <c>$93:89AF</c>.</summary>
    SpazerRight,
    /// <summary>The list at <c>$93:89BF</c>.</summary>
    SpazerRightLoop,
    /// <summary>The list at <c>$93:89CB</c>.</summary>
    SpazerDownRight,
    /// <summary>The list at <c>$93:89DB</c>.</summary>
    SpazerDownRightLoop,
    /// <summary>The list at <c>$93:89E7</c>.</summary>
    SpazerDownFacingRight,
    /// <summary>The list at <c>$93:89F7</c>.</summary>
    SpazerDownFacingRightLoop,
    /// <summary>The list at <c>$93:8A03</c>.</summary>
    SpazerDownLeft,
    /// <summary>The list at <c>$93:8A13</c>.</summary>
    SpazerDownLeftLoop,
    /// <summary>The list at <c>$93:8A1F</c>.</summary>
    SpazerLeft,
    /// <summary>The list at <c>$93:8A2F</c>.</summary>
    SpazerLeftLoop,
    /// <summary>The list at <c>$93:8A3B</c>.</summary>
    SpazerUpLeft,
    /// <summary>The list at <c>$93:8A4B</c>.</summary>
    SpazerUpLeftLoop,
    /// <summary>The list at <c>$93:8A57</c>.</summary>
    SpazerWaveUpFacingRight,
    /// <summary>The list at <c>$93:8AAB</c>.</summary>
    SpazerWaveUpRight,
    /// <summary>The list at <c>$93:8AFF</c>.</summary>
    SpazerWaveRight,
    /// <summary>The list at <c>$93:8B53</c>.</summary>
    SpazerWaveDownRight,
    /// <summary>The list at <c>$93:8BA7</c>.</summary>
    SpazerWaveDownFacingRight,
    /// <summary>The list at <c>$93:8BFB</c>.</summary>
    SpazerWaveDownLeft,
    /// <summary>The list at <c>$93:8C4F</c>.</summary>
    SpazerWaveLeft,
    /// <summary>The list at <c>$93:8CA3</c>.</summary>
    SpazerWaveUpLeft,
    /// <summary>The list at <c>$93:8CF7</c>.</summary>
    PlasmaUpFacingRight,
    /// <summary>The list at <c>$93:8CFF</c>.</summary>
    PlasmaUpFacingRightLoop,
    /// <summary>The list at <c>$93:8D0B</c>.</summary>
    PlasmaUpRight,
    /// <summary>The list at <c>$93:8D13</c>.</summary>
    PlasmaUpRightLoop,
    /// <summary>The list at <c>$93:8D1F</c>.</summary>
    PlasmaRight,
    /// <summary>The list at <c>$93:8D27</c>.</summary>
    PlasmaRightLoop,
    /// <summary>The list at <c>$93:8D33</c>.</summary>
    PlasmaDownRight,
    /// <summary>The list at <c>$93:8D3B</c>.</summary>
    PlasmaDownRightLoop,
    /// <summary>The list at <c>$93:8D47</c>.</summary>
    PlasmaIceWaveUpFacingRight,
    /// <summary>The list at <c>$93:8D4F</c>.</summary>
    PlasmaWaveUpFacingRight,
    /// <summary>The list at <c>$93:8D93</c>.</summary>
    PlasmaIceWaveUpRight,
    /// <summary>The list at <c>$93:8D9B</c>.</summary>
    PlasmaWaveUpRight,
    /// <summary>The list at <c>$93:8DDF</c>.</summary>
    PlasmaWaveRight,
    /// <summary>The list at <c>$93:8DE7</c>.</summary>
    PlasmaWaveRightLoop,
    /// <summary>The list at <c>$93:8E2B</c>.</summary>
    PlasmaIceWaveDownRight,
    /// <summary>The list at <c>$93:8E33</c>.</summary>
    PlasmaWaveDownRight,
    /// <summary>The list at <c>$93:8E77</c>.</summary>
    ChargedPowerUpFacingRight,
    /// <summary>The list at <c>$93:8E8B</c>.</summary>
    ChargedPowerUpRight,
    /// <summary>The list at <c>$93:8E9F</c>.</summary>
    ChargedPowerRight,
    /// <summary>The list at <c>$93:8EB3</c>.</summary>
    ChargedPowerDownRight,
    /// <summary>The list at <c>$93:8EC7</c>.</summary>
    ChargedPowerDownFacingRight,
    /// <summary>The list at <c>$93:8EDB</c>.</summary>
    ChargedPowerDownLeft,
    /// <summary>The list at <c>$93:8EEF</c>.</summary>
    ChargedPowerLeft,
    /// <summary>The list at <c>$93:8F03</c>.</summary>
    ChargedPowerUpLeft,
    /// <summary>The list at <c>$93:8F17</c>.</summary>
    ChargedWaveUpFacingRight,
    /// <summary>The list at <c>$93:8F1F</c>.</summary>
    ChargedWaveDownFacingRight,
    /// <summary>The list at <c>$93:8FA3</c>.</summary>
    ChargedWaveUpRight,
    /// <summary>The list at <c>$93:9027</c>.</summary>
    ChargedWaveRight,
    /// <summary>The list at <c>$93:90AB</c>.</summary>
    ChargedWaveDownRight,
    /// <summary>The list at <c>$93:912F</c>.</summary>
    ChargedIceUpFacingRight,
    /// <summary>The list at <c>$93:9153</c>.</summary>
    ChargedIceWaveUpFacingRight,
    /// <summary>The list at <c>$93:915B</c>.</summary>
    ChargedIceWaveDownFacingRight,
    /// <summary>The list at <c>$93:91DF</c>.</summary>
    ChargedIceWaveUpRight,
    /// <summary>The list at <c>$93:9263</c>.</summary>
    ChargedIceWaveRight,
    /// <summary>The list at <c>$93:92E7</c>.</summary>
    ChargedIceWaveDownRight,
    /// <summary>The list at <c>$93:936B</c>.</summary>
    ChargedSpazerUpFacingRight,
    /// <summary>The list at <c>$93:93AB</c>.</summary>
    ChargedSpazerUpFacingRightLoop,
    /// <summary>The list at <c>$93:93BF</c>.</summary>
    ChargedSpazerUpRight,
    /// <summary>The list at <c>$93:93FF</c>.</summary>
    ChargedSpazerUpRightLoop,
    /// <summary>The list at <c>$93:9413</c>.</summary>
    ChargedSpazerRight,
    /// <summary>The list at <c>$93:9453</c>.</summary>
    ChargedSpazerRightLoop,
    /// <summary>The list at <c>$93:9467</c>.</summary>
    ChargedSpazerDownRight,
    /// <summary>The list at <c>$93:94A7</c>.</summary>
    ChargedSpazerDownRightLoop,
    /// <summary>The list at <c>$93:94BB</c>.</summary>
    ChargedSpazerWaveUpFacingRight,
    /// <summary>The list at <c>$93:94DB</c>.</summary>
    ChargedSpazerWaveUpFacingRightLoop,
    /// <summary>The list at <c>$93:957F</c>.</summary>
    ChargedSpazerWaveUpRight,
    /// <summary>The list at <c>$93:959F</c>.</summary>
    ChargedSpazerWaveUpRightLoop,
    /// <summary>The list at <c>$93:9643</c>.</summary>
    ChargedSpazerWaveRight,
    /// <summary>The list at <c>$93:9663</c>.</summary>
    ChargedSpazerWaveRightLoop,
    /// <summary>The list at <c>$93:9707</c>.</summary>
    ChargedSpazerWaveDownRight,
    /// <summary>The list at <c>$93:9727</c>.</summary>
    ChargedSpazerWaveDownRightLoop,
    /// <summary>The list at <c>$93:97CB</c>.</summary>
    ChargedSpazerWaveDownFacingRight,
    /// <summary>The list at <c>$93:97EB</c>.</summary>
    ChargedSpazerWaveDownFacingRightLoop,
    /// <summary>The list at <c>$93:988F</c>.</summary>
    ChargedSpazerWaveDownLeft,
    /// <summary>The list at <c>$93:98AF</c>.</summary>
    ChargedSpazerWaveDownLeftLoop,
    /// <summary>The list at <c>$93:9953</c>.</summary>
    ChargedSpazerWaveLeft,
    /// <summary>The list at <c>$93:9973</c>.</summary>
    ChargedSpazerWaveLeftLoop,
    /// <summary>The list at <c>$93:9A17</c>.</summary>
    ChargedSpazerWaveUpLeft,
    /// <summary>The list at <c>$93:9A37</c>.</summary>
    ChargedSpazerWaveUpLeftLoop,
    /// <summary>The list at <c>$93:9ADB</c>.</summary>
    ChargedPlasmaUpFacingRight,
    /// <summary>The list at <c>$93:9B0B</c>.</summary>
    ChargedPlasmaUpFacingRightLoop,
    /// <summary>The list at <c>$93:9B1F</c>.</summary>
    ChargedPlasmaUpRight,
    /// <summary>The list at <c>$93:9B4F</c>.</summary>
    ChargedPlasmaUpRightLoop,
    /// <summary>The list at <c>$93:9B63</c>.</summary>
    ChargedPlasmaRight,
    /// <summary>The list at <c>$93:9B93</c>.</summary>
    ChargedPlasmaRightLoop,
    /// <summary>The list at <c>$93:9BA7</c>.</summary>
    ChargedPlasmaDownRight,
    /// <summary>The list at <c>$93:9BD7</c>.</summary>
    ChargedPlasmaDownRightLoop,
    /// <summary>The list at <c>$93:9BEB</c>.</summary>
    ChargedPlasmaWaveUpFacingRight,
    /// <summary>The list at <c>$93:9C1B</c>.</summary>
    ChargedPlasmaWaveUpFacingRightLoop,
    /// <summary>The list at <c>$93:9C9F</c>.</summary>
    ChargedPlasmaWaveUpRight,
    /// <summary>The list at <c>$93:9CCF</c>.</summary>
    ChargedPlasmaWaveUpRightLoop,
    /// <summary>The list at <c>$93:9D53</c>.</summary>
    ChargedPlasmaWaveRight,
    /// <summary>The list at <c>$93:9D83</c>.</summary>
    ChargedPlasmaWaveRightLoop,
    /// <summary>The list at <c>$93:9E07</c>.</summary>
    ChargedPlasmaWaveDownRight,
    /// <summary>The list at <c>$93:9E37</c>.</summary>
    ChargedPlasmaWaveDownRightLoop,
    /// <summary>The list at <c>$93:9EBB</c>.</summary>
    MissileUpFacingRight,
    /// <summary>The list at <c>$93:9EC7</c>.</summary>
    MissileUpRight,
    /// <summary>The list at <c>$93:9ED3</c>.</summary>
    MissileRight,
    /// <summary>The list at <c>$93:9EDF</c>.</summary>
    MissileDownRight,
    /// <summary>The list at <c>$93:9EEB</c>.</summary>
    MissileDownFacingRight,
    /// <summary>The list at <c>$93:9EF7</c>.</summary>
    MissileDownLeft,
    /// <summary>The list at <c>$93:9F03</c>.</summary>
    MissileLeft,
    /// <summary>The list at <c>$93:9F0F</c>.</summary>
    MissileUpLeft,
    /// <summary>The list at <c>$93:9F1B</c>.</summary>
    SuperMissileUpFacingRight,
    /// <summary>The list at <c>$93:9F27</c>.</summary>
    SuperMissileUpRight,
    /// <summary>The list at <c>$93:9F33</c>.</summary>
    SuperMissileRight,
    /// <summary>The list at <c>$93:9F3F</c>.</summary>
    SuperMissileDownRight,
    /// <summary>The list at <c>$93:9F4B</c>.</summary>
    SuperMissileDownFacingRight,
    /// <summary>The list at <c>$93:9F57</c>.</summary>
    SuperMissileDownLeft,
    /// <summary>The list at <c>$93:9F63</c>.</summary>
    SuperMissileLeft,
    /// <summary>The list at <c>$93:9F6F</c>.</summary>
    SuperMissileUpLeft,
    /// <summary>The list at <c>$93:9F7B</c>.</summary>
    SuperMissileLink,
    /// <summary>The list at <c>$93:9F87</c>.</summary>
    PowerBomb,
    /// <summary>The list at <c>$93:9FA3</c>.</summary>
    FastPowerBomb,
    /// <summary>The list at <c>$93:9FBF</c>.</summary>
    Bomb,
    /// <summary>The list at <c>$93:9FE3</c>.</summary>
    FastBomb,
    /// <summary>The list at <c>$93:A007</c>.</summary>
    BeamExplosion,
    /// <summary>The list at <c>$93:A039</c>.</summary>
    MissileExplosion,
    /// <summary>The list at <c>$93:A06B</c>.</summary>
    BombExplosion,
    /// <summary>The list at <c>$93:A095</c>.</summary>
    PlasmaSBA,
    /// <summary>The list at <c>$93:A0C1</c>.</summary>
    SuperMissileExplosion,
    /// <summary>The list at <c>$93:A0F3</c>.</summary>
    Projectile25UpFacingRight,
    /// <summary>The list at <c>$93:A119</c>.</summary>
    ShinesparkEchoUpFacingRight,
    /// <summary>The list at <c>$93:A13D</c>.</summary>
    SpazerSBATrailUpFacingRight,
    /// <summary>The list at <c>$93:A14D</c>.</summary>
    SpazerSBATrailUpFacingRightLoop,
    /// <summary>The list at <c>$93:A159</c>.</summary>
    WaveSBA,
    /// <summary>The list at <c>$93:A16D</c>.</summary>
    Projectile27,
}
