namespace SuperMetroid.Core.Audio;

/// <summary>
/// Every sound of Samus's library (<see cref="SoundEffectLibrary.Samus"/>): Samus's beams, missiles, grapple and spins,
/// the X-ray scope, and the menu and map sounds. The library has its own queue: its sounds interrupt one another
/// and never those of the enemies' or the room's library. The requests that stop its sound and play nothing are
/// <see cref="SamusSoundStop"/>'s (#2580).
/// </summary>
/// <remarks>
/// Members are identities. Their values are arbitrary and nothing reads them; the import pipeline alone knows which
/// installed sound effect each member requests.
/// <para>
/// Each different sound has one member (#2580): beam combinations that play the same sound share its member, which names
/// every combination it serves. The requests merged into a member had its library, priority and length, so the merges
/// keep who interrupts whom.
/// </para>
/// </remarks>
public enum SamusSound
{
    /// <summary>A Power Bomb explosion; it interrupts any sound of the library.</summary>
    PowerBombExplosion,
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
    /// <summary>An uncharged Power Beam shot.</summary>
    UnchargedPowerBeam,
    /// <summary>An uncharged Ice Beam shot, or an Ice and Wave Beam shot.</summary>
    UnchargedIceBeam,
    /// <summary>An uncharged Wave Beam shot.</summary>
    UnchargedWaveBeam,
    /// <summary>An uncharged Spazer shot, or a Spazer and Wave Beam shot.</summary>
    UnchargedSpazer,
    /// <summary>
    /// An uncharged Spazer and Ice Beam shot, or a Spazer, Ice and Wave, a Plasma and Ice, or a Plasma, Ice and Wave Beam
    /// shot.
    /// </summary>
    UnchargedSpazerIce,
    /// <summary>An uncharged Plasma Beam shot, or a Plasma and Wave Beam shot.</summary>
    UnchargedPlasma,
    /// <summary>A charged Power Beam shot.</summary>
    ChargedPowerBeam,
    /// <summary>
    /// A charged Ice Beam shot, or a charged Ice and Wave, Spazer and Ice, Spazer, Ice and Wave, Plasma and Ice, or Plasma,
    /// Ice and Wave Beam shot.
    /// </summary>
    ChargedIceBeam,
    /// <summary>A charged Wave Beam shot.</summary>
    ChargedWaveBeam,
    /// <summary>A charged Spazer shot, or a charged Spazer and Wave Beam shot.</summary>
    ChargedSpazer,
    /// <summary>
    /// A charged Plasma Beam shot, also the Hyper Beam, a charged Plasma and Wave Beam shot, and Samus shooting the screen
    /// after the credits.
    /// </summary>
    ChargedPlasma,
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
    /// <summary>A save file selected on the file-select screen, before the helmet turns.</summary>
    FileSelected,
    /// <summary>The game being saved, at a save station or the gunship.</summary>
    Saving,
    /// <summary>Space Jump under water without the Gravity Suit.</summary>
    UnderwaterSpaceJump,
    /// <summary>A spin jump resumed when a spin or wall jump carries Samus out of water into a dry room.</summary>
    ResumedSpinJump,
    /// <summary>An ordinary spin jump.</summary>
    SpinJump,
    /// <summary>A Screw Attack spin.</summary>
    ScrewAttack,
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
    /// <summary>The map expanding from the area map to the room map.</summary>
    MapExpand,
    /// <summary>The map returning from the room map to the area map.</summary>
    MapReturn,
    /// <summary>A shot absorbed without effect.</summary>
    DudShot,
    /// <summary>A Space Jump.</summary>
    SpaceJump,
    /// <summary>Mother Brain's rainbow beam; a high-priority sound.</summary>
    MotherBrainRainbowBeam,
    /// <summary>The Charge Beam's charging sound resumed.</summary>
    ResumeChargingBeam,
}
