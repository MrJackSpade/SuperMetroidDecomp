namespace SuperMetroid.Core.Audio;

/// <summary>
/// Every sound of the first sound-effect library: Samus's beams, missiles, grapple and spins, the X-ray scope, and the
/// menu and map sounds. The library is its own voice queue: its sounds interrupt one another and never those of the
/// other two libraries.
/// </summary>
/// <remarks>
/// Members are identities. Their values are arbitrary and nothing reads them; the import pipeline alone knows which
/// request each member is in the installed sound program.
/// </remarks>
public enum Library1Sound
{
    /// <summary>A Power Bomb explosion; it interrupts any sound of the library.</summary>
    PowerBombExplosion,
    /// <summary>Silences the library; it interrupts any sound of the library.</summary>
    Silence,
    /// <summary>A missile fired.</summary>
    Missile,
    /// <summary>A Super Missile fired.</summary>
    SuperMissile,
    /// <summary>The grapple beam fired.</summary>
    GrappleStart,
    /// <summary>The grapple beam attached and holding.</summary>
    Grappling,
    /// <summary>The grapple beam released.</summary>
    GrappleEnd,
    /// <summary>The sustained Charge Beam charging sound.</summary>
    ChargingBeam,
    /// <summary>The X-ray scope switching on.</summary>
    XrayStart,
    /// <summary>The X-ray scope switching off; it silences the scope.</summary>
    XrayEnd,
    /// <summary>An uncharged Power Beam shot.</summary>
    UnchargedPowerBeam,
    /// <summary>An uncharged Ice Beam shot.</summary>
    UnchargedIceBeam,
    /// <summary>An uncharged Wave Beam shot.</summary>
    UnchargedWaveBeam,
    /// <summary>An uncharged Ice and Wave Beam shot.</summary>
    UnchargedIceWaveBeam,
    /// <summary>An uncharged Spazer shot.</summary>
    UnchargedSpazer,
    /// <summary>An uncharged Spazer and Ice Beam shot.</summary>
    UnchargedSpazerIce,
    /// <summary>An uncharged Spazer, Ice and Wave Beam shot.</summary>
    UnchargedSpazerIceWave,
    /// <summary>An uncharged Spazer and Wave Beam shot.</summary>
    UnchargedSpazerWave,
    /// <summary>An uncharged Plasma Beam shot.</summary>
    UnchargedPlasma,
    /// <summary>An uncharged Plasma and Ice Beam shot.</summary>
    UnchargedPlasmaIce,
    /// <summary>An uncharged Plasma, Ice and Wave Beam shot.</summary>
    UnchargedPlasmaIceWave,
    /// <summary>An uncharged Plasma and Wave Beam shot.</summary>
    UnchargedPlasmaWave,
    /// <summary>A charged Power Beam shot.</summary>
    ChargedPowerBeam,
    /// <summary>A charged Ice Beam shot.</summary>
    ChargedIceBeam,
    /// <summary>A charged Wave Beam shot.</summary>
    ChargedWaveBeam,
    /// <summary>A charged Ice and Wave Beam shot.</summary>
    ChargedIceWaveBeam,
    /// <summary>A charged Spazer shot.</summary>
    ChargedSpazer,
    /// <summary>A charged Spazer and Ice Beam shot.</summary>
    ChargedSpazerIce,
    /// <summary>A charged Spazer, Ice and Wave Beam shot.</summary>
    ChargedSpazerIceWave,
    /// <summary>A charged Spazer and Wave Beam shot.</summary>
    ChargedSpazerWave,
    /// <summary>A charged Plasma Beam shot, also the Hyper Beam.</summary>
    ChargedPlasma,
    /// <summary>A charged Plasma and Ice Beam shot.</summary>
    ChargedPlasmaIce,
    /// <summary>A charged Plasma, Ice and Wave Beam shot.</summary>
    ChargedPlasmaIceWave,
    /// <summary>A charged Plasma and Wave Beam shot, also Samus shooting the screen after the credits.</summary>
    ChargedPlasmaWave,
    /// <summary>The Ice Beam's special Power Bomb attack.</summary>
    IceSpecialAttack,
    /// <summary>The end of the Ice Beam's special Power Bomb attack.</summary>
    IceSpecialAttackEnd,
    /// <summary>The Spazer's special Power Bomb attack.</summary>
    SpazerSpecialAttack,
    /// <summary>The end of the Spazer's special Power Bomb attack.</summary>
    SpazerSpecialAttackEnd,
    /// <summary>The Plasma Beam's special Power Bomb attack.</summary>
    PlasmaSpecialAttack,
    /// <summary>The Wave Beam's special Power Bomb attack.</summary>
    WaveSpecialAttack,
    /// <summary>The end of the Wave Beam's special Power Bomb attack; it silences the attack.</summary>
    WaveSpecialAttackEnd,
    /// <summary>A save file selected on the file-select screen, before the helmet turns.</summary>
    FileSelected,
    /// <summary>The library's first empty sound; it plays nothing.</summary>
    Empty1,
    /// <summary>The library's second empty sound; it plays nothing.</summary>
    Empty2,
    /// <summary>The library's third empty sound; it plays nothing.</summary>
    Empty3,
    /// <summary>The game being saved, at a save station or the gunship.</summary>
    Saving,
    /// <summary>Space Jump under water without the Gravity Suit.</summary>
    UnderwaterSpaceJump,
    /// <summary>A spin jump resumed when a spin or wall jump carries Samus out of water into a dry room.</summary>
    ResumedSpinJump,
    /// <summary>An ordinary spin jump.</summary>
    SpinJump,
    /// <summary>The end of a spin jump; it silences the spin.</summary>
    SpinJumpEnd,
    /// <summary>A Screw Attack spin.</summary>
    ScrewAttack,
    /// <summary>The end of a Screw Attack; it silences the spin.</summary>
    ScrewAttackEnd,
    /// <summary>Samus damaged, also a shinespark crashing into a wall; a high-priority sound.</summary>
    SamusDamaged,
    /// <summary>The pause map scrolling.</summary>
    MapScroll,
    /// <summary>A menu cursor moved, or the reserve tank mode toggled.</summary>
    MenuCursor,
    /// <summary>A menu or pause-screen entry accepted.</summary>
    MenuConfirm,
    /// <summary>A HUD weapon selected.</summary>
    HudWeaponSelect,
    /// <summary>The library's fourth empty sound; it plays nothing.</summary>
    Empty4,
    /// <summary>The map expanding from the area map to the room map.</summary>
    MapExpand,
    /// <summary>The map returning from the room map to the area map.</summary>
    MapReturn,
    /// <summary>A shot absorbed without effect.</summary>
    DudShot,
    /// <summary>A Space Jump.</summary>
    SpaceJump,
    /// <summary>A resumed Space Jump; no code plays it.</summary>
    ResumedSpaceJump,
    /// <summary>Mother Brain's rainbow beam; a high-priority sound.</summary>
    MotherBrainRainbowBeam,
    /// <summary>The Charge Beam's charging sound resumed.</summary>
    ResumeChargingBeam,
    /// <summary>An unused sound that needs a music data set's instrument.</summary>
    Unused,
}
