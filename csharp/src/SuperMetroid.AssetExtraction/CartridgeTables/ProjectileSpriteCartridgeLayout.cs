
namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>Where each Samus projectile and charge-flare composition group begins in bank $93.</summary>
internal static class ProjectileSpriteCartridgeLayout
{
    /// <summary>$93:AB6C, ProjectileFlareSpritemaps_Flare_Charge_Hyper_Grapple_0: the first of four seven-byte main flares.</summary>
    internal const ushort FirstMainFlare = 0xab6c;

    /// <summary>$93:A6FD, ProjectileFlareSpritemaps_FlareSlowSparks_FacingRight_0: the first of twelve right-facing sparks.</summary>
    internal const ushort FirstRightSpark = 0xa6fd;

    /// <summary>$93:A8DE, ProjectileFlareSpritemaps_FlareSlowSparks_FacingLeft_0: the first of twelve left-facing sparks.</summary>
    internal const ushort FirstLeftSpark = 0xa8de;

    /// <summary>$93:A117: native Nothing projectile composition group.</summary>
    internal const ushort NothingStart = 0xa117;

    /// <summary>$93:A24D: native Power projectile composition group.</summary>
    internal const ushort PowerStart = 0xa24d;

    /// <summary>$93:A83E: native BombExplosion projectile composition group.</summary>
    internal const ushort BombExplosionStart = 0xa83e;

    /// <summary>$93:AB97: native PowerBomb projectile composition group.</summary>
    internal const ushort PowerBombStart = 0xab97;

    /// <summary>$93:AD45: native Bomb projectile composition group.</summary>
    internal const ushort BombStart = 0xad45;

    /// <summary>$93:AD61: native Missile projectile composition group.</summary>
    internal const ushort MissileStart = 0xad61;

    /// <summary>$93:ADD5: native SuperMissile projectile composition group.</summary>
    internal const ushort SuperMissileStart = 0xadd5;

    /// <summary>$93:AE65: native Wave projectile composition group.</summary>
    internal const ushort WaveStart = 0xae65;

    /// <summary>$93:EC3E: native ChargedPower projectile composition group.</summary>
    internal const ushort ChargedPowerStart = 0xec3e;

    /// <summary>$93:ED9E: native ChargedIce projectile composition group.</summary>
    internal const ushort ChargedIceStart = 0xed9e;

    /// <summary>$93:EDF6: native Ice projectile composition group.</summary>
    internal const ushort IceStart = 0xedf6;

    /// <summary>$93:AF4C/B672, ChargedWave_WaveSBA/ChargedIceWave: two four-quadrant core poses, then paired four-quadrant lobes.</summary>
    internal const ushort ChargedWaveStart = 0xaf4c;

    /// <summary>$93:AF4C/B672, ChargedWave_WaveSBA/ChargedIceWave: two four-quadrant core poses, then paired four-quadrant lobes.</summary>
    internal const ushort ChargedIceWaveStart = 0xb672;

    /// <summary>$93:D10E, S_SI_SW_SIW: four orientations of four-part beams, then four of two-part beams; spread repeats each seed on three lanes.</summary>
    internal const ushort SpazerStart = 0xd10e;

    /// <summary>$93:D8EE, Charged_S_SI_SW_SIW: two four-part axial groups, six six-part diagonal groups, then four four-part axial groups.</summary>
    internal const ushort ChargedSpazerStart = 0xd8ee;

    /// <summary>$93:A37D: Plasma/PlasmaIce axial four-tile strips alternate with six-part diagonal strips.</summary>
    internal const ushort PlasmaStart = 0xa37d;

    /// <summary>$93:A7C9: missile explosion grows from one central tile to five four-quadrant poses.</summary>
    internal const ushort MissileExplosionStart = 0xa7c9;

    /// <summary>$93:ABB3: beam explosion has two single-tile core poses followed by four four-quadrant poses.</summary>
    internal const ushort BeamExplosionStart = 0xabb3;

    /// <summary>$93:EE12: six Spazer startup orientation groups serialize length1..4 strips then one/two/three two-tile pairs.</summary>
    internal const ushort SpazerStartupStart = 0xee12;

    /// <summary>$93:AA84: SuperMissileExplosion starts with three four-quadrant cores, followed by core/spikes, a large-part ring, then ring/spikes.</summary>
    internal const ushort SuperMissileExplosionStart = 0xaa84;

    /// <summary>$93:BC0A/BCC8: horizontal uncharged four-tile and charged seven-tile cores.</summary>
    internal const ushort PlasmaWaveHorizontalShort = 0xbc0a;

    /// <summary>$93:BC0A/BCC8: horizontal uncharged four-tile and charged seven-tile cores.</summary>
    internal const ushort PlasmaWaveHorizontalLong = 0xbcc8;

    /// <summary>$93:BE0D/BF25: down-right/up-left diagonal six-part and ten-part cores.</summary>
    internal const ushort PlasmaWaveDownRightShort = 0xbe0d;

    /// <summary>$93:BE0D/BF25: down-right/up-left diagonal six-part and ten-part cores.</summary>
    internal const ushort PlasmaWaveDownRightLong = 0xbf25;

    /// <summary>$93:C0F1/C1AF: vertical uncharged four-tile and charged seven-tile cores.</summary>
    internal const ushort PlasmaWaveVerticalShort = 0xc0f1;

    /// <summary>$93:C0F1/C1AF: vertical uncharged four-tile and charged seven-tile cores.</summary>
    internal const ushort PlasmaWaveVerticalLong = 0xc1af;

    /// <summary>$93:C3B2/C669: alternate charged horizontal seven-part and down-right twelve-part cores, after native unselected composition groups.</summary>
    internal const ushort PlasmaWaveHorizontalAlternate = 0xc3b2;

    /// <summary>$93:C3B2/C669: alternate charged horizontal seven-part and down-right twelve-part cores, after native unselected composition groups.</summary>
    internal const ushort PlasmaWaveDownRightAlternate = 0xc669;

    /// <summary>$93:C94D/CC04: alternate charged vertical seven-part and down-left twelve-part cores, after native unselected composition groups.</summary>
    internal const ushort PlasmaWaveVerticalAlternate = 0xc94d;

    /// <summary>$93:C94D/CC04: alternate charged vertical seven-part and down-left twelve-part cores, after native unselected composition groups.</summary>
    internal const ushort PlasmaWaveDownLeftAlternate = 0xcc04;

    /// <summary>$93:CE2A/CF42: down-left/up-right diagonal six-part and ten-part cores.</summary>
    internal const ushort PlasmaWaveDownLeftShort = 0xce2a;

    /// <summary>$93:CE2A/CF42: down-left/up-right diagonal six-part and ten-part cores.</summary>
    internal const ushort PlasmaWaveDownLeftLong = 0xcf42;

    /// <summary>$93:F0FA: Charged_P_PI_PW_PIW startup, two axial orientation groups then two diagonal groups, repeated with alternate orientation/art.</summary>
    internal const ushort PlasmaStartupStart = 0xf0fa;
}
