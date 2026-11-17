namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The Samus projectiles' animations (#1301): one segment per animation a definition or loop enters.</summary>
public enum SamusProjectileSegment
{
    /// <summary>A power beam shot flying straight up, held.</summary>
    PowerUpFacingRight,
    /// <summary>A power beam shot flying up and right, held.</summary>
    PowerUpRight,
    /// <summary>A power beam shot flying right, held.</summary>
    PowerRight,
    /// <summary>A power beam shot flying down and right, held.</summary>
    PowerDownRight,
    /// <summary>A power beam shot flying straight down, held.</summary>
    PowerDownFacingRight,
    /// <summary>A power beam shot flying down and left, held.</summary>
    PowerDownLeft,
    /// <summary>A power beam shot flying left, held.</summary>
    PowerLeft,
    /// <summary>A power beam shot flying up and left, held.</summary>
    PowerUpLeft,
    /// <summary>
    /// A wave beam shot fired straight up: unseen at first, then swinging from side to side as it flies, repeating.
    /// </summary>
    WaveUpFacingRight,
    /// <summary>
    /// A wave beam shot flying straight up or down, swinging from side to side to its peaks and back, repeating.
    /// </summary>
    WaveDownFacingRight,
    /// <summary>
    /// A wave beam shot flying up and right, swinging across its path to its peaks and back, repeating.
    /// </summary>
    WaveUpRight,
    /// <summary>A wave beam shot flying right, swinging across its path to its peaks and back, repeating.</summary>
    WaveRight,
    /// <summary>
    /// A wave beam shot flying down and right, swinging across its path to its peaks and back, repeating.
    /// </summary>
    WaveDownRight,
    /// <summary>An ice beam shot flying straight up, glinting as a star and a cross, repeating.</summary>
    IceUpFacingRight,
    /// <summary>A Spazer shot flying straight up, its seed parting into its beams, then flying on spread.</summary>
    SpazerUpFacingRight,
    /// <summary>The Spazer up facing right animation's repeating part.</summary>
    SpazerUpFacingRightLoop,
    /// <summary>A Spazer shot flying up and right, its seed parting into its beams, then flying on spread.</summary>
    SpazerUpRight,
    /// <summary>The Spazer up right animation's repeating part.</summary>
    SpazerUpRightLoop,
    /// <summary>A Spazer shot flying right, its seed parting into its beams, then flying on spread.</summary>
    SpazerRight,
    /// <summary>The Spazer right animation's repeating part.</summary>
    SpazerRightLoop,
    /// <summary>A Spazer shot flying down and right, its seed parting into its beams, then flying on spread.</summary>
    SpazerDownRight,
    /// <summary>The Spazer down right animation's repeating part.</summary>
    SpazerDownRightLoop,
    /// <summary>A Spazer shot flying straight down, its seed parting into its beams, then flying on spread.</summary>
    SpazerDownFacingRight,
    /// <summary>The Spazer down facing right animation's repeating part.</summary>
    SpazerDownFacingRightLoop,
    /// <summary>A Spazer shot flying down and left, its seed parting into its beams, then flying on spread.</summary>
    SpazerDownLeft,
    /// <summary>The Spazer down left animation's repeating part.</summary>
    SpazerDownLeftLoop,
    /// <summary>A Spazer shot flying left, its seed parting into its beams, then flying on spread.</summary>
    SpazerLeft,
    /// <summary>The Spazer left animation's repeating part.</summary>
    SpazerLeftLoop,
    /// <summary>A Spazer shot flying up and left, its seed parting into its beams, then flying on spread.</summary>
    SpazerUpLeft,
    /// <summary>The Spazer up left animation's repeating part.</summary>
    SpazerUpLeftLoop,
    /// <summary>
    /// A Spazer and wave shot flying straight up, its beams splitting, spreading wide and closing again, repeating.
    /// </summary>
    SpazerWaveUpFacingRight,
    /// <summary>
    /// A Spazer and wave shot flying up and right, its beams splitting, spreading wide and closing again, repeating.
    /// </summary>
    SpazerWaveUpRight,
    /// <summary>
    /// A Spazer and wave shot flying right, its beams splitting, spreading wide and closing again, repeating.
    /// </summary>
    SpazerWaveRight,
    /// <summary>
    /// A Spazer and wave shot flying down and right, its beams splitting, spreading wide and closing again, repeating.
    /// </summary>
    SpazerWaveDownRight,
    /// <summary>
    /// A Spazer and wave shot flying straight down, its beams splitting, spreading wide and closing again, repeating.
    /// </summary>
    SpazerWaveDownFacingRight,
    /// <summary>
    /// A Spazer and wave shot flying down and left, its beams splitting, spreading wide and closing again, repeating.
    /// </summary>
    SpazerWaveDownLeft,
    /// <summary>
    /// A Spazer and wave shot flying left, its beams splitting, spreading wide and closing again, repeating.
    /// </summary>
    SpazerWaveLeft,
    /// <summary>
    /// A Spazer and wave shot flying up and left, its beams splitting, spreading wide and closing again, repeating.
    /// </summary>
    SpazerWaveUpLeft,
    /// <summary>A plasma shot flying straight up, its core first, then at full length.</summary>
    PlasmaUpFacingRight,
    /// <summary>The Plasma up facing right animation's repeating part.</summary>
    PlasmaUpFacingRightLoop,
    /// <summary>A plasma shot flying up and right, its core first, then at full length.</summary>
    PlasmaUpRight,
    /// <summary>The Plasma up right animation's repeating part.</summary>
    PlasmaUpRightLoop,
    /// <summary>A plasma shot flying right, its core first, then at full length.</summary>
    PlasmaRight,
    /// <summary>The Plasma right animation's repeating part.</summary>
    PlasmaRightLoop,
    /// <summary>A plasma shot flying down and right, its core first, then at full length.</summary>
    PlasmaDownRight,
    /// <summary>The Plasma down right animation's repeating part.</summary>
    PlasmaDownRightLoop,
    /// <summary>
    /// A plasma, ice and wave shot flying straight up: its core, then its lobes swinging out and back.
    /// </summary>
    PlasmaIceWaveUpFacingRight,
    /// <summary>
    /// A plasma and wave shot flying straight up, its lobes swinging out to their peak and back, repeating.
    /// </summary>
    PlasmaWaveUpFacingRight,
    /// <summary>
    /// A plasma, ice and wave shot flying up and right: its core, then its lobes swinging out and back.
    /// </summary>
    PlasmaIceWaveUpRight,
    /// <summary>
    /// A plasma and wave shot flying up and right, its lobes swinging out to their peak and back, repeating.
    /// </summary>
    PlasmaWaveUpRight,
    /// <summary>
    /// A plasma and wave shot flying right: its core, then its lobes swinging out to their peak and back, repeating.
    /// </summary>
    PlasmaWaveRight,
    /// <summary>The Plasma Wave right animation's repeating part.</summary>
    PlasmaWaveRightLoop,
    /// <summary>
    /// A plasma, ice and wave shot flying down and right: its core, then its lobes swinging out and back.
    /// </summary>
    PlasmaIceWaveDownRight,
    /// <summary>
    /// A plasma and wave shot flying down and right, its lobes swinging out to their peak and back, repeating.
    /// </summary>
    PlasmaWaveDownRight,
    /// <summary>A charged power beam shot flying straight up, flickering between its two pictures, repeating.</summary>
    ChargedPowerUpFacingRight,
    /// <summary>
    /// A charged power beam shot flying up and right, flickering between its two pictures, repeating.
    /// </summary>
    ChargedPowerUpRight,
    /// <summary>A charged power beam shot flying right, flickering between its two pictures, repeating.</summary>
    ChargedPowerRight,
    /// <summary>
    /// A charged power beam shot flying down and right, flickering between its two pictures, repeating.
    /// </summary>
    ChargedPowerDownRight,
    /// <summary>
    /// A charged power beam shot flying straight down, flickering between its two pictures, repeating.
    /// </summary>
    ChargedPowerDownFacingRight,
    /// <summary>
    /// A charged power beam shot flying down and left, flickering between its two pictures, repeating.
    /// </summary>
    ChargedPowerDownLeft,
    /// <summary>A charged power beam shot flying left, flickering between its two pictures, repeating.</summary>
    ChargedPowerLeft,
    /// <summary>A charged power beam shot flying up and left, flickering between its two pictures, repeating.</summary>
    ChargedPowerUpLeft,
    /// <summary>
    /// A charged wave shot fired straight up: unseen at first, then its lobes swinging out and back, flickering,
    /// repeating.
    /// </summary>
    ChargedWaveUpFacingRight,
    /// <summary>
    /// A charged wave shot flying straight up or down, its lobes swinging out to their peak and back, flickering,
    /// repeating.
    /// </summary>
    ChargedWaveDownFacingRight,
    /// <summary>
    /// A charged wave shot flying up and right, its lobes swinging out to their peak and back, flickering, repeating.
    /// </summary>
    ChargedWaveUpRight,
    /// <summary>
    /// A charged wave shot flying right, its lobes swinging out to their peak and back, flickering, repeating.
    /// </summary>
    ChargedWaveRight,
    /// <summary>
    /// A charged wave shot flying down and right, its lobes swinging out to their peak and back, flickering, repeating.
    /// </summary>
    ChargedWaveDownRight,
    /// <summary>A charged ice beam shot flying straight up, flickering between a cross and a star, repeating.</summary>
    ChargedIceUpFacingRight,
    /// <summary>
    /// A charged ice and wave shot fired straight up: unseen at first, then its lobes swinging out and back,
    /// flickering, repeating.
    /// </summary>
    ChargedIceWaveUpFacingRight,
    /// <summary>
    /// A charged ice and wave shot flying straight up or down, its lobes swinging out to their peak and back,
    /// flickering, repeating.
    /// </summary>
    ChargedIceWaveDownFacingRight,
    /// <summary>
    /// A charged ice and wave shot flying up and right, its lobes swinging out to their peak and back, flickering,
    /// repeating.
    /// </summary>
    ChargedIceWaveUpRight,
    /// <summary>
    /// A charged ice and wave shot flying right, its lobes swinging out to their peak and back, flickering, repeating.
    /// </summary>
    ChargedIceWaveRight,
    /// <summary>
    /// A charged ice and wave shot flying down and right, its lobes swinging out to their peak and back, flickering,
    /// repeating.
    /// </summary>
    ChargedIceWaveDownRight,
    /// <summary>
    /// A charged Spazer shot flying straight up, flickering as it starts short, grows long and its seed parts, then
    /// flying on spread.
    /// </summary>
    ChargedSpazerUpFacingRight,
    /// <summary>The charged Spazer up facing right animation's repeating part.</summary>
    ChargedSpazerUpFacingRightLoop,
    /// <summary>
    /// A charged Spazer shot flying up and right, flickering as it starts short, grows long and its seed parts, then
    /// flying on spread.
    /// </summary>
    ChargedSpazerUpRight,
    /// <summary>The charged Spazer up right animation's repeating part.</summary>
    ChargedSpazerUpRightLoop,
    /// <summary>
    /// A charged Spazer shot flying right, flickering as it starts short, grows long and its seed parts, then flying on
    /// spread.
    /// </summary>
    ChargedSpazerRight,
    /// <summary>The charged Spazer right animation's repeating part.</summary>
    ChargedSpazerRightLoop,
    /// <summary>
    /// A charged Spazer shot flying down and right, flickering as it starts short, grows long and its seed parts, then
    /// flying on spread.
    /// </summary>
    ChargedSpazerDownRight,
    /// <summary>The charged Spazer down right animation's repeating part.</summary>
    ChargedSpazerDownRightLoop,
    /// <summary>
    /// A charged Spazer and wave shot flying straight up, flickering as it starts short and grows long, then waving.
    /// </summary>
    ChargedSpazerWaveUpFacingRight,
    /// <summary>The charged Spazer Wave up facing right animation's repeating part.</summary>
    ChargedSpazerWaveUpFacingRightLoop,
    /// <summary>
    /// A charged Spazer and wave shot flying up and right, flickering as it starts short and grows long, then waving.
    /// </summary>
    ChargedSpazerWaveUpRight,
    /// <summary>The charged Spazer Wave up right animation's repeating part.</summary>
    ChargedSpazerWaveUpRightLoop,
    /// <summary>
    /// A charged Spazer and wave shot flying right, flickering as it starts short and grows long, then waving.
    /// </summary>
    ChargedSpazerWaveRight,
    /// <summary>The charged Spazer Wave right animation's repeating part.</summary>
    ChargedSpazerWaveRightLoop,
    /// <summary>
    /// A charged Spazer and wave shot flying down and right, flickering as it starts short and grows long, then waving.
    /// </summary>
    ChargedSpazerWaveDownRight,
    /// <summary>The charged Spazer Wave down right animation's repeating part.</summary>
    ChargedSpazerWaveDownRightLoop,
    /// <summary>
    /// A charged Spazer and wave shot flying straight down, flickering as it starts short and grows long, then waving.
    /// </summary>
    ChargedSpazerWaveDownFacingRight,
    /// <summary>The charged Spazer Wave down facing right animation's repeating part.</summary>
    ChargedSpazerWaveDownFacingRightLoop,
    /// <summary>
    /// A charged Spazer and wave shot flying down and left, flickering as it starts short and grows long, then waving.
    /// </summary>
    ChargedSpazerWaveDownLeft,
    /// <summary>The charged Spazer Wave down left animation's repeating part.</summary>
    ChargedSpazerWaveDownLeftLoop,
    /// <summary>
    /// A charged Spazer and wave shot flying left, flickering as it starts short and grows long, then waving.
    /// </summary>
    ChargedSpazerWaveLeft,
    /// <summary>The charged Spazer Wave left animation's repeating part.</summary>
    ChargedSpazerWaveLeftLoop,
    /// <summary>
    /// A charged Spazer and wave shot flying up and left, flickering as it starts short and grows long, then waving.
    /// </summary>
    ChargedSpazerWaveUpLeft,
    /// <summary>The charged Spazer Wave up left animation's repeating part.</summary>
    ChargedSpazerWaveUpLeftLoop,
    /// <summary>
    /// A charged plasma shot flying straight up, flickering as its core grows short then long, then flying on at full
    /// length.
    /// </summary>
    ChargedPlasmaUpFacingRight,
    /// <summary>The charged Plasma up facing right animation's repeating part.</summary>
    ChargedPlasmaUpFacingRightLoop,
    /// <summary>
    /// A charged plasma shot flying up and right, flickering as its core grows short then long, then flying on at full
    /// length.
    /// </summary>
    ChargedPlasmaUpRight,
    /// <summary>The charged Plasma up right animation's repeating part.</summary>
    ChargedPlasmaUpRightLoop,
    /// <summary>
    /// A charged plasma shot flying right, flickering as its core grows short then long, then flying on at full length.
    /// </summary>
    ChargedPlasmaRight,
    /// <summary>The charged Plasma right animation's repeating part.</summary>
    ChargedPlasmaRightLoop,
    /// <summary>
    /// A charged plasma shot flying down and right, flickering as its core grows short then long, then flying on at
    /// full length.
    /// </summary>
    ChargedPlasmaDownRight,
    /// <summary>The charged Plasma down right animation's repeating part.</summary>
    ChargedPlasmaDownRightLoop,
    /// <summary>
    /// A charged plasma and wave shot flying straight up, flickering as its core grows short then long, then waving.
    /// </summary>
    ChargedPlasmaWaveUpFacingRight,
    /// <summary>The charged Plasma Wave up facing right animation's repeating part.</summary>
    ChargedPlasmaWaveUpFacingRightLoop,
    /// <summary>
    /// A charged plasma and wave shot flying up and right, flickering as its core grows short then long, then waving.
    /// </summary>
    ChargedPlasmaWaveUpRight,
    /// <summary>The charged Plasma Wave up right animation's repeating part.</summary>
    ChargedPlasmaWaveUpRightLoop,
    /// <summary>
    /// A charged plasma and wave shot flying right, flickering as its core grows short then long, then waving.
    /// </summary>
    ChargedPlasmaWaveRight,
    /// <summary>The charged Plasma Wave right animation's repeating part.</summary>
    ChargedPlasmaWaveRightLoop,
    /// <summary>
    /// A charged plasma and wave shot flying down and right, flickering as its core grows short then long, then waving.
    /// </summary>
    ChargedPlasmaWaveDownRight,
    /// <summary>The charged Plasma Wave down right animation's repeating part.</summary>
    ChargedPlasmaWaveDownRightLoop,
    /// <summary>A missile flying straight up, held.</summary>
    MissileUpFacingRight,
    /// <summary>A missile flying up and right, held.</summary>
    MissileUpRight,
    /// <summary>A missile flying right, held.</summary>
    MissileRight,
    /// <summary>A missile flying down and right, held.</summary>
    MissileDownRight,
    /// <summary>A missile flying straight down, held.</summary>
    MissileDownFacingRight,
    /// <summary>A missile flying down and left, held.</summary>
    MissileDownLeft,
    /// <summary>A missile flying left, held.</summary>
    MissileLeft,
    /// <summary>A missile flying up and left, held.</summary>
    MissileUpLeft,
    /// <summary>A super missile flying straight up, held.</summary>
    SuperMissileUpFacingRight,
    /// <summary>A super missile flying up and right, held.</summary>
    SuperMissileUpRight,
    /// <summary>A super missile flying right, held.</summary>
    SuperMissileRight,
    /// <summary>A super missile flying down and right, held.</summary>
    SuperMissileDownRight,
    /// <summary>A super missile flying straight down, held.</summary>
    SuperMissileDownFacingRight,
    /// <summary>A super missile flying down and left, held.</summary>
    SuperMissileDownLeft,
    /// <summary>A super missile flying left, held.</summary>
    SuperMissileLeft,
    /// <summary>A super missile flying up and left, held.</summary>
    SuperMissileUpLeft,
    /// <summary>The unseen part a super missile flies linked with, held.</summary>
    SuperMissileLink,
    /// <summary>A power bomb pulsing bright, dimming and dark, repeating.</summary>
    PowerBomb,
    /// <summary>A power bomb pulsing bright, dimming and dark quickly as it nears its blast, repeating.</summary>
    FastPowerBomb,
    /// <summary>A bomb dark, smouldering, glowing and flashing, repeating.</summary>
    Bomb,
    /// <summary>A bomb dark, smouldering, glowing and flashing quickly as it nears its blast, repeating.</summary>
    FastBomb,
    /// <summary>A beam shot's impact: a speck, a glint and a flash ringing out and fading, then gone.</summary>
    BeamExplosion,
    /// <summary>A missile's blast: a spark, a ball, a blast, a flare, a burst and embers, then gone.</summary>
    MissileExplosion,
    /// <summary>A bomb's blast: an orb sparkling, bursting wide and ringing out, then gone.</summary>
    BombExplosion,
    /// <summary>The plasma special attack's orbs sparkling, bursting wide and ringing out, repeating.</summary>
    PlasmaSpecialAttack,
    /// <summary>A super missile's blast: a ball, a blast and a flare ringing out and fading, then gone.</summary>
    SuperMissileExplosion,
    /// <summary>A shinespark echo fired straight up, unseen, repeating.</summary>
    ShinesparkEchoUpFacingRight,
    /// <summary>
    /// The Spazer special attack's trail fired straight up, unseen, narrow then wide, then unseen as it flies on.
    /// </summary>
    SpazerSpecialAttackTrailUpFacingRight,
    /// <summary>The Spazer special attack trail up facing right animation's repeating part.</summary>
    SpazerSpecialAttackTrailUpFacingRightLoop,
    /// <summary>The wave special attack's orb flickering between its two pictures, repeating.</summary>
    WaveSpecialAttack,
}
