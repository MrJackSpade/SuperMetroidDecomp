namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>
/// The Samus projectiles' animations (#1301, #2347): one segment per run of pictures an animation plays, named for the
/// travel it serves (a direction, or vertical, rising diagonal, horizontal or falling diagonal travel, or any direction) and
/// for what a repeating part shows.
/// </summary>
public enum SamusProjectileSegment
{
    /// <summary>A Power shot fired up, facing either way, its one picture held.</summary>
    PowerUp,
    /// <summary>A Power shot fired up and right, its one picture held.</summary>
    PowerUpRight,
    /// <summary>A Power shot fired right, its one picture held.</summary>
    PowerRight,
    /// <summary>A Power shot fired down and right, its one picture held.</summary>
    PowerDownRight,
    /// <summary>A Power shot fired down, facing either way, its one picture held.</summary>
    PowerDown,
    /// <summary>A Power shot fired down and left, its one picture held.</summary>
    PowerDownLeft,
    /// <summary>A Power shot fired left, its one picture held.</summary>
    PowerLeft,
    /// <summary>A Power shot fired up and left, its one picture held.</summary>
    PowerUpLeft,
    /// <summary>
    /// A Wave or Ice-Wave shot fired up, facing either way: unseen at first, before it swings as
    /// <see cref="WaveVertical"/> does.
    /// </summary>
    WaveUp,
    /// <summary>
    /// A Wave or Ice-Wave shot flying up or down, fired down or past its upward start: swinging from side to side to
    /// its peaks and back, repeating.
    /// </summary>
    WaveVertical,
    /// <summary>
    /// A Wave or Ice-Wave shot fired up and right or down and left, swinging across its path to its peaks and back,
    /// repeating.
    /// </summary>
    WaveRisingDiagonal,
    /// <summary>
    /// A Wave or Ice-Wave shot fired right or left, swinging across its path to its peaks and back, repeating.
    /// </summary>
    WaveHorizontal,
    /// <summary>
    /// A Wave or Ice-Wave shot fired down and right or up and left, swinging across its path to its peaks and back,
    /// repeating.
    /// </summary>
    WaveFallingDiagonal,
    /// <summary>An Ice shot fired in any direction, glinting as a star and a cross, repeating.</summary>
    Ice,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired up, facing either way, its seed parting into its beams before it flies on
    /// spread; the Spazer special attack's particles start it too.
    /// </summary>
    SpazerUp,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired up, facing either way, and the Spazer special attack's particles, once the
    /// beams have parted: flying on spread, the picture held.
    /// </summary>
    SpazerUpSpread,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired up and right, its seed parting into its beams before it flies on spread.
    /// </summary>
    SpazerUpRight,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired up and right once the beams have parted: flying on spread, the picture held.
    /// </summary>
    SpazerUpRightSpread,
    /// <summary>A Spazer or Spazer-Ice shot fired right, its seed parting into its beams before it flies on spread.</summary>
    SpazerRight,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired right once the beams have parted: flying on spread, the picture held.
    /// </summary>
    SpazerRightSpread,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired down and right, its seed parting into its beams before it flies on spread.
    /// </summary>
    SpazerDownRight,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired down and right once the beams have parted: flying on spread, the picture held.
    /// </summary>
    SpazerDownRightSpread,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired down, facing either way, its seed parting into its beams before it flies on
    /// spread.
    /// </summary>
    SpazerDown,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired down, facing either way once the beams have parted: flying on spread, the
    /// picture held.
    /// </summary>
    SpazerDownSpread,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired down and left, its seed parting into its beams before it flies on spread.
    /// </summary>
    SpazerDownLeft,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired down and left once the beams have parted: flying on spread, the picture held.
    /// </summary>
    SpazerDownLeftSpread,
    /// <summary>A Spazer or Spazer-Ice shot fired left, its seed parting into its beams before it flies on spread.</summary>
    SpazerLeft,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired left once the beams have parted: flying on spread, the picture held.
    /// </summary>
    SpazerLeftSpread,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired up and left, its seed parting into its beams before it flies on spread.
    /// </summary>
    SpazerUpLeft,
    /// <summary>
    /// A Spazer or Spazer-Ice shot fired up and left once the beams have parted: flying on spread, the picture held.
    /// </summary>
    SpazerUpLeftSpread,
    /// <summary>
    /// A Spazer-Wave or Spazer-Ice-Wave shot fired up, facing either way, its beams splitting, spreading wide and
    /// closing again, repeating.
    /// </summary>
    SpazerWaveUp,
    /// <summary>
    /// A Spazer-Wave or Spazer-Ice-Wave shot fired up and right, its beams splitting, spreading wide and closing again,
    /// repeating.
    /// </summary>
    SpazerWaveUpRight,
    /// <summary>
    /// A Spazer-Wave or Spazer-Ice-Wave shot fired right, its beams splitting, spreading wide and closing again,
    /// repeating.
    /// </summary>
    SpazerWaveRight,
    /// <summary>
    /// A Spazer-Wave or Spazer-Ice-Wave shot fired down and right, its beams splitting, spreading wide and closing
    /// again, repeating.
    /// </summary>
    SpazerWaveDownRight,
    /// <summary>
    /// A Spazer-Wave or Spazer-Ice-Wave shot fired down, facing either way, its beams splitting, spreading wide and
    /// closing again, repeating.
    /// </summary>
    SpazerWaveDown,
    /// <summary>
    /// A Spazer-Wave or Spazer-Ice-Wave shot fired down and left, its beams splitting, spreading wide and closing
    /// again, repeating.
    /// </summary>
    SpazerWaveDownLeft,
    /// <summary>
    /// A Spazer-Wave or Spazer-Ice-Wave shot fired left, its beams splitting, spreading wide and closing again,
    /// repeating.
    /// </summary>
    SpazerWaveLeft,
    /// <summary>
    /// A Spazer-Wave or Spazer-Ice-Wave shot fired up and left, its beams splitting, spreading wide and closing again,
    /// repeating.
    /// </summary>
    SpazerWaveUpLeft,
    /// <summary>
    /// A Plasma or Plasma-Ice shot fired up or down: its core for one update, before it flies at full length.
    /// </summary>
    PlasmaVertical,
    /// <summary>
    /// A Plasma or Plasma-Ice shot fired up or down, past its core: flying at full length, its picture held.
    /// </summary>
    PlasmaVerticalFullLength,
    /// <summary>
    /// A Plasma or Plasma-Ice shot fired up and right or down and left: its core for one update, before it flies at
    /// full length.
    /// </summary>
    PlasmaRisingDiagonal,
    /// <summary>
    /// A Plasma or Plasma-Ice shot fired up and right or down and left, past its core: flying at full length, its
    /// picture held.
    /// </summary>
    PlasmaRisingDiagonalFullLength,
    /// <summary>
    /// A Plasma or Plasma-Ice shot fired right or left: its core for one update, before it flies at full length.
    /// </summary>
    PlasmaHorizontal,
    /// <summary>
    /// A Plasma or Plasma-Ice shot fired right or left, past its core: flying at full length, its picture held.
    /// </summary>
    PlasmaHorizontalFullLength,
    /// <summary>
    /// A Plasma or Plasma-Ice shot fired down and right or up and left: its core for one update, before it flies at
    /// full length.
    /// </summary>
    PlasmaFallingDiagonal,
    /// <summary>
    /// A Plasma or Plasma-Ice shot fired down and right or up and left, past its core: flying at full length, its
    /// picture held.
    /// </summary>
    PlasmaFallingDiagonalFullLength,
    /// <summary>
    /// A Plasma-Ice-Wave shot fired up or down: its core for one update, before its lobes swing as
    /// <see cref="PlasmaWaveVertical"/>'s do.
    /// </summary>
    PlasmaIceWaveVertical,
    /// <summary>
    /// A Plasma-Wave shot fired up or down, and a Plasma-Ice-Wave shot past its core: its lobes swinging out to their
    /// peak and back, repeating.
    /// </summary>
    PlasmaWaveVertical,
    /// <summary>
    /// A Plasma-Ice-Wave shot fired up and right or down and left: its core for one update, before its lobes swing as
    /// <see cref="PlasmaWaveRisingDiagonal"/>'s do.
    /// </summary>
    PlasmaIceWaveRisingDiagonal,
    /// <summary>
    /// A Plasma-Wave shot fired up and right or down and left, and a Plasma-Ice-Wave shot past its core: its lobes
    /// swinging out to their peak and back, repeating.
    /// </summary>
    PlasmaWaveRisingDiagonal,
    /// <summary>
    /// A Plasma-Wave or Plasma-Ice-Wave shot fired right or left: its core for one update, before its lobes swing.
    /// </summary>
    PlasmaWaveHorizontal,
    /// <summary>
    /// A Plasma-Wave or Plasma-Ice-Wave shot fired right or left, past its core: its lobes swinging out to their peak
    /// and back, repeating.
    /// </summary>
    PlasmaWaveHorizontalSwing,
    /// <summary>
    /// A Plasma-Ice-Wave shot fired down and right or up and left: its core for one update, before its lobes swing as
    /// <see cref="PlasmaWaveFallingDiagonal"/>'s do.
    /// </summary>
    PlasmaIceWaveFallingDiagonal,
    /// <summary>
    /// A Plasma-Wave shot fired down and right or up and left, and a Plasma-Ice-Wave shot past its core: its lobes
    /// swinging out to their peak and back, repeating.
    /// </summary>
    PlasmaWaveFallingDiagonal,
    /// <summary>A charged Power shot fired up, facing either way, flickering between its two pictures, repeating.</summary>
    ChargedPowerUp,
    /// <summary>A charged Power shot fired up and right, flickering between its two pictures, repeating.</summary>
    ChargedPowerUpRight,
    /// <summary>A charged Power shot fired right, flickering between its two pictures, repeating.</summary>
    ChargedPowerRight,
    /// <summary>A charged Power shot fired down and right, flickering between its two pictures, repeating.</summary>
    ChargedPowerDownRight,
    /// <summary>A charged Power shot fired down, facing either way, flickering between its two pictures, repeating.</summary>
    ChargedPowerDown,
    /// <summary>A charged Power shot fired down and left, flickering between its two pictures, repeating.</summary>
    ChargedPowerDownLeft,
    /// <summary>A charged Power shot fired left, flickering between its two pictures, repeating.</summary>
    ChargedPowerLeft,
    /// <summary>A charged Power shot fired up and left, flickering between its two pictures, repeating.</summary>
    ChargedPowerUpLeft,
    /// <summary>
    /// A charged Wave shot fired up, facing either way: unseen at first, before its lobes swing as
    /// <see cref="ChargedWaveVertical"/>'s do.
    /// </summary>
    ChargedWaveUp,
    /// <summary>
    /// A charged Wave shot flying up or down, fired down or past its upward start: its lobes swinging out to their peak
    /// and back, flickering, repeating.
    /// </summary>
    ChargedWaveVertical,
    /// <summary>
    /// A charged Wave shot fired up and right or down and left, its lobes swinging out to their peak and back,
    /// flickering, repeating.
    /// </summary>
    ChargedWaveRisingDiagonal,
    /// <summary>
    /// A charged Wave shot fired right or left, its lobes swinging out to their peak and back, flickering, repeating.
    /// </summary>
    ChargedWaveHorizontal,
    /// <summary>
    /// A charged Wave shot fired down and right or up and left, its lobes swinging out to their peak and back,
    /// flickering, repeating.
    /// </summary>
    ChargedWaveFallingDiagonal,
    /// <summary>A charged Ice shot fired in any direction, flickering between a cross and a star, repeating.</summary>
    ChargedIce,
    /// <summary>
    /// A charged Ice-Wave shot fired up, facing either way: unseen at first, before its lobes swing as
    /// <see cref="ChargedIceWaveVertical"/>'s do.
    /// </summary>
    ChargedIceWaveUp,
    /// <summary>
    /// A charged Ice-Wave shot flying up or down, fired down or past its upward start: its lobes swinging out to their
    /// peak and back, flickering, repeating.
    /// </summary>
    ChargedIceWaveVertical,
    /// <summary>
    /// A charged Ice-Wave shot fired up and right or down and left, its lobes swinging out to their peak and back,
    /// flickering, repeating.
    /// </summary>
    ChargedIceWaveRisingDiagonal,
    /// <summary>
    /// A charged Ice-Wave shot fired right or left, its lobes swinging out to their peak and back, flickering,
    /// repeating.
    /// </summary>
    ChargedIceWaveHorizontal,
    /// <summary>
    /// A charged Ice-Wave shot fired down and right or up and left, its lobes swinging out to their peak and back,
    /// flickering, repeating.
    /// </summary>
    ChargedIceWaveFallingDiagonal,
    /// <summary>
    /// A charged Spazer or Spazer-Ice shot fired up or down, flickering as it starts short, grows long and its seed
    /// parts, before it flies on spread.
    /// </summary>
    ChargedSpazerVertical,
    /// <summary>
    /// A charged Spazer or Spazer-Ice shot fired up or down, its beams parted: flying on spread, flickering, repeating.
    /// </summary>
    ChargedSpazerVerticalSpread,
    /// <summary>
    /// A charged Spazer or Spazer-Ice shot fired up and right or down and left, flickering as it starts short, grows
    /// long and its seed parts, before it flies on spread.
    /// </summary>
    ChargedSpazerRisingDiagonal,
    /// <summary>
    /// A charged Spazer or Spazer-Ice shot fired up and right or down and left, its beams parted: flying on spread,
    /// flickering, repeating.
    /// </summary>
    ChargedSpazerRisingDiagonalSpread,
    /// <summary>
    /// A charged Spazer or Spazer-Ice shot fired right or left, flickering as it starts short, grows long and its seed
    /// parts, before it flies on spread.
    /// </summary>
    ChargedSpazerHorizontal,
    /// <summary>
    /// A charged Spazer or Spazer-Ice shot fired right or left, its beams parted: flying on spread, flickering,
    /// repeating.
    /// </summary>
    ChargedSpazerHorizontalSpread,
    /// <summary>
    /// A charged Spazer or Spazer-Ice shot fired down and right or up and left, flickering as it starts short, grows
    /// long and its seed parts, before it flies on spread.
    /// </summary>
    ChargedSpazerFallingDiagonal,
    /// <summary>
    /// A charged Spazer or Spazer-Ice shot fired down and right or up and left, its beams parted: flying on spread,
    /// flickering, repeating.
    /// </summary>
    ChargedSpazerFallingDiagonalSpread,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired up, facing either way, flickering as it starts short and
    /// grows long, before its beams wave.
    /// </summary>
    ChargedSpazerWaveUp,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired up, facing either way, past its growth: its beams splitting,
    /// spreading wide and closing again, flickering, repeating.
    /// </summary>
    ChargedSpazerWaveUpWaving,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired up and right, flickering as it starts short and grows long,
    /// before its beams wave.
    /// </summary>
    ChargedSpazerWaveUpRight,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired up and right, past its growth: its beams splitting,
    /// spreading wide and closing again, flickering, repeating.
    /// </summary>
    ChargedSpazerWaveUpRightWaving,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired right, flickering as it starts short and grows long, before
    /// its beams wave.
    /// </summary>
    ChargedSpazerWaveRight,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired right, past its growth: its beams splitting, spreading wide
    /// and closing again, flickering, repeating.
    /// </summary>
    ChargedSpazerWaveRightWaving,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired down and right, flickering as it starts short and grows
    /// long, before its beams wave.
    /// </summary>
    ChargedSpazerWaveDownRight,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired down and right, past its growth: its beams splitting,
    /// spreading wide and closing again, flickering, repeating.
    /// </summary>
    ChargedSpazerWaveDownRightWaving,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired down, facing either way, flickering as it starts short and
    /// grows long, before its beams wave.
    /// </summary>
    ChargedSpazerWaveDown,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired down, facing either way, past its growth: its beams
    /// splitting, spreading wide and closing again, flickering, repeating.
    /// </summary>
    ChargedSpazerWaveDownWaving,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired down and left, flickering as it starts short and grows long,
    /// before its beams wave.
    /// </summary>
    ChargedSpazerWaveDownLeft,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired down and left, past its growth: its beams splitting,
    /// spreading wide and closing again, flickering, repeating.
    /// </summary>
    ChargedSpazerWaveDownLeftWaving,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired left, flickering as it starts short and grows long, before
    /// its beams wave.
    /// </summary>
    ChargedSpazerWaveLeft,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired left, past its growth: its beams splitting, spreading wide
    /// and closing again, flickering, repeating.
    /// </summary>
    ChargedSpazerWaveLeftWaving,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired up and left, flickering as it starts short and grows long,
    /// before its beams wave.
    /// </summary>
    ChargedSpazerWaveUpLeft,
    /// <summary>
    /// A charged Spazer-Wave or Spazer-Ice-Wave shot fired up and left, past its growth: its beams splitting, spreading
    /// wide and closing again, flickering, repeating.
    /// </summary>
    ChargedSpazerWaveUpLeftWaving,
    /// <summary>
    /// A charged Plasma or Plasma-Ice shot fired up or down, flickering as its core grows short then long, before it
    /// flies at full length.
    /// </summary>
    ChargedPlasmaVertical,
    /// <summary>
    /// A charged Plasma or Plasma-Ice shot fired up or down, past its growth: flying at full length, flickering,
    /// repeating.
    /// </summary>
    ChargedPlasmaVerticalFullLength,
    /// <summary>
    /// A charged Plasma or Plasma-Ice shot fired up and right or down and left, flickering as its core grows short then
    /// long, before it flies at full length.
    /// </summary>
    ChargedPlasmaRisingDiagonal,
    /// <summary>
    /// A charged Plasma or Plasma-Ice shot fired up and right or down and left, past its growth: flying at full length,
    /// flickering, repeating.
    /// </summary>
    ChargedPlasmaRisingDiagonalFullLength,
    /// <summary>
    /// A charged Plasma or Plasma-Ice shot fired right or left, flickering as its core grows short then long, before it
    /// flies at full length.
    /// </summary>
    ChargedPlasmaHorizontal,
    /// <summary>
    /// A charged Plasma or Plasma-Ice shot fired right or left, past its growth: flying at full length, flickering,
    /// repeating.
    /// </summary>
    ChargedPlasmaHorizontalFullLength,
    /// <summary>
    /// A charged Plasma or Plasma-Ice shot fired down and right or up and left, flickering as its core grows short then
    /// long, before it flies at full length.
    /// </summary>
    ChargedPlasmaFallingDiagonal,
    /// <summary>
    /// A charged Plasma or Plasma-Ice shot fired down and right or up and left, past its growth: flying at full length,
    /// flickering, repeating.
    /// </summary>
    ChargedPlasmaFallingDiagonalFullLength,
    /// <summary>
    /// A charged Plasma-Wave or Plasma-Ice-Wave shot fired up or down, flickering as its core grows short then long,
    /// before its lobes swing.
    /// </summary>
    ChargedPlasmaWaveVertical,
    /// <summary>
    /// A charged Plasma-Wave or Plasma-Ice-Wave shot fired up or down, past its growth: its lobes swinging out to their
    /// peak and back, flickering, repeating.
    /// </summary>
    ChargedPlasmaWaveVerticalWaving,
    /// <summary>
    /// A charged Plasma-Wave or Plasma-Ice-Wave shot fired up and right or down and left, flickering as its core grows
    /// short then long, before its lobes swing.
    /// </summary>
    ChargedPlasmaWaveRisingDiagonal,
    /// <summary>
    /// A charged Plasma-Wave or Plasma-Ice-Wave shot fired up and right or down and left, past its growth: its lobes
    /// swinging out to their peak and back, flickering, repeating.
    /// </summary>
    ChargedPlasmaWaveRisingDiagonalWaving,
    /// <summary>
    /// A charged Plasma-Wave or Plasma-Ice-Wave shot fired right or left, flickering as its core grows short then long,
    /// before its lobes swing.
    /// </summary>
    ChargedPlasmaWaveHorizontal,
    /// <summary>
    /// A charged Plasma-Wave or Plasma-Ice-Wave shot fired right or left, past its growth: its lobes swinging out to
    /// their peak and back, flickering, repeating.
    /// </summary>
    ChargedPlasmaWaveHorizontalWaving,
    /// <summary>
    /// A charged Plasma-Wave or Plasma-Ice-Wave shot fired down and right or up and left, flickering as its core grows
    /// short then long, before its lobes swing.
    /// </summary>
    ChargedPlasmaWaveFallingDiagonal,
    /// <summary>
    /// A charged Plasma-Wave or Plasma-Ice-Wave shot fired down and right or up and left, past its growth: its lobes
    /// swinging out to their peak and back, flickering, repeating.
    /// </summary>
    ChargedPlasmaWaveFallingDiagonalWaving,
    /// <summary>A missile fired up, facing either way, its one picture held.</summary>
    MissileUp,
    /// <summary>A missile fired up and right, its one picture held.</summary>
    MissileUpRight,
    /// <summary>A missile fired right, its one picture held.</summary>
    MissileRight,
    /// <summary>A missile fired down and right, its one picture held.</summary>
    MissileDownRight,
    /// <summary>A missile fired down, facing either way, its one picture held.</summary>
    MissileDown,
    /// <summary>A missile fired down and left, its one picture held.</summary>
    MissileDownLeft,
    /// <summary>A missile fired left, its one picture held.</summary>
    MissileLeft,
    /// <summary>A missile fired up and left, its one picture held.</summary>
    MissileUpLeft,
    /// <summary>A Super Missile fired up, facing either way, its one picture held.</summary>
    SuperMissileUp,
    /// <summary>A Super Missile fired up and right, its one picture held.</summary>
    SuperMissileUpRight,
    /// <summary>A Super Missile fired right, its one picture held.</summary>
    SuperMissileRight,
    /// <summary>A Super Missile fired down and right, its one picture held.</summary>
    SuperMissileDownRight,
    /// <summary>A Super Missile fired down, facing either way, its one picture held.</summary>
    SuperMissileDown,
    /// <summary>A Super Missile fired down and left, its one picture held.</summary>
    SuperMissileDownLeft,
    /// <summary>A Super Missile fired left, its one picture held.</summary>
    SuperMissileLeft,
    /// <summary>A Super Missile fired up and left, its one picture held.</summary>
    SuperMissileUpLeft,
    /// <summary>The unseen link a Super Missile flies joined to, held.</summary>
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
    /// <summary>A Shinespark crash's echo, whatever its direction: unseen, held.</summary>
    ShinesparkEcho,
    /// <summary>
    /// The Spazer special attack's trailing pair, whatever its direction: unseen, narrow then wide, before it flies on
    /// at its widest.
    /// </summary>
    SpazerSpecialAttackTrail,
    /// <summary>The Spazer special attack's trailing pair, once it has widened: unseen at its widest, held.</summary>
    SpazerSpecialAttackTrailWidest,
    /// <summary>The wave special attack's orb flickering between its two pictures, repeating.</summary>
    WaveSpecialAttack,
}
