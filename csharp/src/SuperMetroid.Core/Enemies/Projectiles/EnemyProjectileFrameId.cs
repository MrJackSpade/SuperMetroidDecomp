namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The frames an enemy projectile shows: the compositions its segments select, then every installed animation frame. Each
/// is named for what its picture shows: its owner, then the shape, turn, size or placement that tells it from the owner's
/// other pictures, which its summary spells out; no animation that shows it and no place in one names it. One picture is
/// one frame however many records store it. Its values are arbitrary and its installed name is its name in snake case.
/// </summary>
public enum EnemyProjectileFrameId
{
    /// <summary>The Skree debris, one composed picture.</summary>
    SkreeDebris,
    /// <summary>The Metaree debris, one composed picture.</summary>
    MetareeDebris,
    /// <summary>Draygon's goop: a spiky blob.</summary>
    DraygonGoopSpiky,
    /// <summary>Draygon's goop: a puffed-out blob.</summary>
    DraygonGoopPuffed,
    /// <summary>Draygon's goop: a rounded blob.</summary>
    DraygonGoopRounded,
    /// <summary>Draygon's goop: a lopsided blob.</summary>
    DraygonGoopLopsided,
    /// <summary>Draygon's goop: a blob curling up.</summary>
    DraygonGoopCurled,
    /// <summary>Draygon's goop: scattered scraps.</summary>
    DraygonGoopScattered,
    /// <summary>Draygon's wall turret shot: four sparks gathering in from their widest spread.</summary>
    DraygonWallTurretSparksGathering,
    /// <summary>Draygon's wall turret shot: four sparks drawing in.</summary>
    DraygonWallTurretSparksDrawingIn,
    /// <summary>Draygon's wall turret shot: four sparks closing in.</summary>
    DraygonWallTurretSparksClosingIn,
    /// <summary>Draygon's wall turret shot: four sparks converging.</summary>
    DraygonWallTurretSparksConverging,
    /// <summary>Draygon's wall turret shot: four sparks nearly met.</summary>
    DraygonWallTurretSparksNearing,
    /// <summary>Draygon's wall turret shot: four sparks meeting at their closest.</summary>
    DraygonWallTurretSparksMeeting,
    /// <summary>Draygon's wall turret shot: four sparks set the other way round, gathering in from their widest spread.</summary>
    DraygonWallTurretSparksTwistedGathering,
    /// <summary>Draygon's wall turret shot: four sparks set the other way round, drawing in.</summary>
    DraygonWallTurretSparksTwistedDrawingIn,
    /// <summary>Draygon's wall turret shot: four sparks set the other way round, closing in.</summary>
    DraygonWallTurretSparksTwistedClosingIn,
    /// <summary>Draygon's wall turret shot: four sparks set the other way round, converging.</summary>
    DraygonWallTurretSparksTwistedConverging,
    /// <summary>Draygon's wall turret shot: four sparks set the other way round, nearly met.</summary>
    DraygonWallTurretSparksTwistedNearing,
    /// <summary>Draygon's wall turret shot: four sparks set the other way round, meeting at their closest.</summary>
    DraygonWallTurretSparksTwistedMeeting,
    /// <summary>Draygon's wall turret shot: a faint green glow.</summary>
    DraygonWallTurretGlowFaint,
    /// <summary>Draygon's wall turret shot: a dim green glow.</summary>
    DraygonWallTurretGlowDim,
    /// <summary>Draygon's wall turret shot: a bright green glow.</summary>
    DraygonWallTurretGlowBright,
    /// <summary>Draygon's wall turret shot: a green orb forming.</summary>
    DraygonWallTurretOrbForming,
    /// <summary>Draygon's wall turret shot: a bright green orb.</summary>
    DraygonWallTurretOrbBright,
    /// <summary>Draygon's wall turret shot: a green orb flickering.</summary>
    DraygonWallTurretOrbFlickering,
    /// <summary>Draygon's wall turret shot: a dimmed green orb.</summary>
    DraygonWallTurretOrbDimmed,
    /// <summary>Crocomire's mouth glow: a small grey ball.</summary>
    CrocomireMouthBallSmall,
    /// <summary>Crocomire's mouth glow: a medium grey ball.</summary>
    CrocomireMouthBallMedium,
    /// <summary>Crocomire's mouth glow: a large grey ball.</summary>
    CrocomireMouthBallLarge,
    /// <summary>Crocomire's mouth glow: the largest grey ball.</summary>
    CrocomireMouthBallLargest,
    /// <summary>Crocomire's bridge: a falling fragment.</summary>
    CrocomireBridgeFragment,
    /// <summary>Crocomire's spike wall: a crumbling piece.</summary>
    CrocomireSpikeWallPiece,
    /// <summary>Crocomire's shot bursting: a small spark.</summary>
    CrocomireShotSpark,
    /// <summary>Crocomire's shot bursting: a ball of light.</summary>
    CrocomireShotFlareBall,
    /// <summary>Crocomire's shot bursting: a ring of light around a burning core.</summary>
    CrocomireShotFlareRingCored,
    /// <summary>Crocomire's shot bursting: a ring of light.</summary>
    CrocomireShotFlareRing,
    /// <summary>Crocomire's shot bursting: a fading ring of light.</summary>
    CrocomireShotFlareRingFading,
    /// <summary>Ridley's fireball: a swirl of flame.</summary>
    RidleyFireballTurned0,
    /// <summary>Ridley's fireball: a swirl of flame turned a quarter.</summary>
    RidleyFireballTurned90,
    /// <summary>Ridley's fireball: a swirl of flame turned half round.</summary>
    RidleyFireballTurned180,
    /// <summary>Ridley's fireball: a swirl of flame turned three quarters.</summary>
    RidleyFireballTurned270,
    /// <summary>Ridley's fireball: a small spark.</summary>
    RidleyFireballSpark,
    /// <summary>Ridley's fireball: a cloud of fire.</summary>
    RidleyFireballFireCloud,
    /// <summary>Ridley's fireball: a cloud of fire spreading.</summary>
    RidleyFireballFireCloudSpreading,
    /// <summary>Ridley's fireball: a cloud of fire breaking up.</summary>
    RidleyFireballFireCloudBreaking,
    /// <summary>Ridley's fireball: the embers of a cloud of fire.</summary>
    RidleyFireballFireCloudEmbers,
    /// <summary>Ceres falling debris: a light chunk.</summary>
    CeresFallingDebrisLight,
    /// <summary>Ceres falling debris: a dark chunk.</summary>
    CeresFallingDebrisDark,
    /// <summary>Phantoon's flame: a tall flame around a ringed eye.</summary>
    PhantoonFlameRingedEye,
    /// <summary>Phantoon's flame: a tall flame around a bright eye.</summary>
    PhantoonFlameBrightEye,
    /// <summary>Phantoon's flame: a tall flame flickering.</summary>
    PhantoonFlameFlickering,
    /// <summary>Phantoon's flame: a flame splashed flat.</summary>
    PhantoonFlameSplashed,
    /// <summary>Phantoon's flame: a tall flame swelling.</summary>
    PhantoonFlameSwelling,
    /// <summary>Phantoon's flame: a tall flame leaning.</summary>
    PhantoonFlameLeaning,
    /// <summary>Phantoon's flame: a tall flame at rest.</summary>
    PhantoonFlameResting,
    /// <summary>Phantoon's flame: a tall flame swaying at rest.</summary>
    PhantoonFlameRestingSwaying,
    /// <summary>Phantoon's flame: a tall flame low at rest.</summary>
    PhantoonFlameRestingLow,
    /// <summary>Phantoon's flame: a flame dying down.</summary>
    PhantoonFlameDying,
    /// <summary>Phantoon's flame: a flame guttering.</summary>
    PhantoonFlameGuttering,
    /// <summary>Phantoon's flame: wisps of a flame.</summary>
    PhantoonFlameWisps,
    /// <summary>Phantoon's flame: the last wisp of a flame.</summary>
    PhantoonFlameLastWisp,
    /// <summary>Phantoon's flame: a flame kindling.</summary>
    PhantoonFlameKindling,
    /// <summary>Phantoon's flame: a teardrop of flame.</summary>
    PhantoonFlameTeardrop,
    /// <summary>Phantoon's flame: a flame sputtering.</summary>
    PhantoonFlameSputtering,
    /// <summary>Kraid's rock: a large rock.</summary>
    KraidRockLarge,
    /// <summary>Kraid's rock: a small rock.</summary>
    KraidRockSmall,
    /// <summary>Kraid's rock bursting: a small spark.</summary>
    KraidRockSpark,
    /// <summary>Kraid's rock bursting: a ball of light.</summary>
    KraidRockFlareBall,
    /// <summary>Kraid's rock bursting: a ring of light around a burning core.</summary>
    KraidRockFlareRingCored,
    /// <summary>Kraid's rock bursting: a ring of light.</summary>
    KraidRockFlareRing,
    /// <summary>Kraid's rock bursting: a fading ring of light.</summary>
    KraidRockFlareRingFading,
    /// <summary>Fake Kraid: a gob of spit.</summary>
    FakeKraidSpit,
    /// <summary>Fake Kraid: a spike pointing left.</summary>
    FakeKraidSpikeLeft,
    /// <summary>Fake Kraid: a spike pointing right.</summary>
    FakeKraidSpikeRight,
    /// <summary>The Alcoon's fireball: a spinning flame.</summary>
    AlcoonFireballTurned0,
    /// <summary>The Alcoon's fireball: a spinning flame turned a quarter.</summary>
    AlcoonFireballTurned90,
    /// <summary>The Alcoon's fireball: a spinning flame turned half round.</summary>
    AlcoonFireballTurned180,
    /// <summary>The Alcoon's fireball: a spinning flame turned three quarters.</summary>
    AlcoonFireballTurned270,
    /// <summary>A Space Pirate's laser: a glint of light.</summary>
    SpacePirateLaserFlareGlint,
    /// <summary>A Space Pirate's laser: a small glow.</summary>
    SpacePirateLaserFlareGlow,
    /// <summary>A Space Pirate's laser: a ball of light.</summary>
    SpacePirateLaserFlareBall,
    /// <summary>A Space Pirate's laser: a bright zigzag just leaving the muzzle.</summary>
    SpacePirateLaserEmergingBright,
    /// <summary>A Space Pirate's laser: a dim zigzag just leaving the muzzle.</summary>
    SpacePirateLaserEmergingDim,
    /// <summary>A Space Pirate's laser: a bright zigzag extending.</summary>
    SpacePirateLaserExtendingBright,
    /// <summary>A Space Pirate's laser: a dim zigzag extending.</summary>
    SpacePirateLaserExtendingDim,
    /// <summary>A Space Pirate's laser: a bright zigzag lengthening further.</summary>
    SpacePirateLaserLengtheningBright,
    /// <summary>A Space Pirate's laser: a dim zigzag lengthening further.</summary>
    SpacePirateLaserLengtheningDim,
    /// <summary>A Space Pirate's laser: a bright zigzag at its full length.</summary>
    SpacePirateLaserFullLengthBright,
    /// <summary>A Space Pirate's laser: a dim zigzag at its full length.</summary>
    SpacePirateLaserFullLengthDim,
    /// <summary>A Space Pirate's claw thrown left: the claw turned 0 degrees.</summary>
    SpacePirateClawLeftTurned0,
    /// <summary>A Space Pirate's claw thrown left: the claw turned 45 degrees.</summary>
    SpacePirateClawLeftTurned45,
    /// <summary>A Space Pirate's claw thrown left: the claw turned 90 degrees.</summary>
    SpacePirateClawLeftTurned90,
    /// <summary>A Space Pirate's claw thrown left: the claw turned 135 degrees.</summary>
    SpacePirateClawLeftTurned135,
    /// <summary>A Space Pirate's claw thrown left: the claw turned 180 degrees.</summary>
    SpacePirateClawLeftTurned180,
    /// <summary>A Space Pirate's claw thrown left: the claw turned 225 degrees.</summary>
    SpacePirateClawLeftTurned225,
    /// <summary>A Space Pirate's claw thrown left: the claw turned 270 degrees.</summary>
    SpacePirateClawLeftTurned270,
    /// <summary>A Space Pirate's claw thrown left: the claw turned 315 degrees.</summary>
    SpacePirateClawLeftTurned315,
    /// <summary>A Space Pirate's claw thrown right: the claw turned 0 degrees.</summary>
    SpacePirateClawRightTurned0,
    /// <summary>A Space Pirate's claw thrown right: the claw turned 45 degrees.</summary>
    SpacePirateClawRightTurned45,
    /// <summary>A Space Pirate's claw thrown right: the claw turned 90 degrees.</summary>
    SpacePirateClawRightTurned90,
    /// <summary>A Space Pirate's claw thrown right: the claw turned 135 degrees.</summary>
    SpacePirateClawRightTurned135,
    /// <summary>A Space Pirate's claw thrown right: the claw turned 180 degrees.</summary>
    SpacePirateClawRightTurned180,
    /// <summary>A Space Pirate's claw thrown right: the claw turned 225 degrees.</summary>
    SpacePirateClawRightTurned225,
    /// <summary>A Space Pirate's claw thrown right: the claw turned 270 degrees.</summary>
    SpacePirateClawRightTurned270,
    /// <summary>A Space Pirate's claw thrown right: the claw turned 315 degrees.</summary>
    SpacePirateClawRightTurned315,
    /// <summary>The gunship's inner dust cloud on its right: a puff of dust.</summary>
    GunshipDustRightInnerPuff,
    /// <summary>The gunship's inner dust cloud on its right: dust billowing.</summary>
    GunshipDustRightInnerBillowing,
    /// <summary>The gunship's inner dust cloud on its right: dust spreading.</summary>
    GunshipDustRightInnerSpreading,
    /// <summary>The gunship's inner dust cloud on its right: dust drifting.</summary>
    GunshipDustRightInnerDrifting,
    /// <summary>The gunship's inner dust cloud on its right: dust thinning.</summary>
    GunshipDustRightInnerThinning,
    /// <summary>The gunship's inner dust cloud on its right: dust fading.</summary>
    GunshipDustRightInnerFading,
    /// <summary>The gunship's inner dust cloud on its right: wisps of dust.</summary>
    GunshipDustRightInnerWisps,
    /// <summary>The gunship's inner dust cloud on its right: faint dust.</summary>
    GunshipDustRightInnerFaint,
    /// <summary>The gunship's middle dust cloud on its right: a puff of dust.</summary>
    GunshipDustRightMiddlePuff,
    /// <summary>The gunship's middle dust cloud on its right: dust billowing.</summary>
    GunshipDustRightMiddleBillowing,
    /// <summary>The gunship's middle dust cloud on its right: dust spreading.</summary>
    GunshipDustRightMiddleSpreading,
    /// <summary>The gunship's middle dust cloud on its right: dust drifting.</summary>
    GunshipDustRightMiddleDrifting,
    /// <summary>The gunship's middle dust cloud on its right: dust thinning.</summary>
    GunshipDustRightMiddleThinning,
    /// <summary>The gunship's middle dust cloud on its right: dust fading.</summary>
    GunshipDustRightMiddleFading,
    /// <summary>The gunship's middle dust cloud on its right: wisps of dust.</summary>
    GunshipDustRightMiddleWisps,
    /// <summary>The gunship's middle dust cloud on its right: faint dust.</summary>
    GunshipDustRightMiddleFaint,
    /// <summary>The gunship's outer dust cloud on its right: a puff of dust.</summary>
    GunshipDustRightOuterPuff,
    /// <summary>The gunship's outer dust cloud on its right: dust billowing.</summary>
    GunshipDustRightOuterBillowing,
    /// <summary>The gunship's outer dust cloud on its right: dust spreading.</summary>
    GunshipDustRightOuterSpreading,
    /// <summary>The gunship's outer dust cloud on its right: dust drifting.</summary>
    GunshipDustRightOuterDrifting,
    /// <summary>The gunship's outer dust cloud on its right: dust thinning.</summary>
    GunshipDustRightOuterThinning,
    /// <summary>The gunship's outer dust cloud on its right: dust fading.</summary>
    GunshipDustRightOuterFading,
    /// <summary>The gunship's outer dust cloud on its right: faint dust.</summary>
    GunshipDustRightOuterFaint,
    /// <summary>The gunship's inner dust cloud on its left: a puff of dust.</summary>
    GunshipDustLeftInnerPuff,
    /// <summary>The gunship's inner dust cloud on its left: dust billowing.</summary>
    GunshipDustLeftInnerBillowing,
    /// <summary>The gunship's inner dust cloud on its left: dust spreading.</summary>
    GunshipDustLeftInnerSpreading,
    /// <summary>The gunship's inner dust cloud on its left: dust drifting.</summary>
    GunshipDustLeftInnerDrifting,
    /// <summary>The gunship's inner dust cloud on its left: dust thinning.</summary>
    GunshipDustLeftInnerThinning,
    /// <summary>The gunship's inner dust cloud on its left: dust fading.</summary>
    GunshipDustLeftInnerFading,
    /// <summary>The gunship's inner dust cloud on its left: wisps of dust.</summary>
    GunshipDustLeftInnerWisps,
    /// <summary>The gunship's inner dust cloud on its left: faint dust.</summary>
    GunshipDustLeftInnerFaint,
    /// <summary>The gunship's middle dust cloud on its left: a puff of dust.</summary>
    GunshipDustLeftMiddlePuff,
    /// <summary>The gunship's middle dust cloud on its left: dust billowing.</summary>
    GunshipDustLeftMiddleBillowing,
    /// <summary>The gunship's middle dust cloud on its left: dust spreading.</summary>
    GunshipDustLeftMiddleSpreading,
    /// <summary>The gunship's middle dust cloud on its left: dust drifting.</summary>
    GunshipDustLeftMiddleDrifting,
    /// <summary>The gunship's middle dust cloud on its left: dust thinning.</summary>
    GunshipDustLeftMiddleThinning,
    /// <summary>The gunship's middle dust cloud on its left: dust fading.</summary>
    GunshipDustLeftMiddleFading,
    /// <summary>The gunship's middle dust cloud on its left: wisps of dust.</summary>
    GunshipDustLeftMiddleWisps,
    /// <summary>The gunship's middle dust cloud on its left: faint dust.</summary>
    GunshipDustLeftMiddleFaint,
    /// <summary>The gunship's outer dust cloud on its left: a puff of dust.</summary>
    GunshipDustLeftOuterPuff,
    /// <summary>The gunship's outer dust cloud on its left: dust billowing.</summary>
    GunshipDustLeftOuterBillowing,
    /// <summary>The gunship's outer dust cloud on its left: dust spreading.</summary>
    GunshipDustLeftOuterSpreading,
    /// <summary>The gunship's outer dust cloud on its left: dust drifting.</summary>
    GunshipDustLeftOuterDrifting,
    /// <summary>The gunship's outer dust cloud on its left: dust thinning.</summary>
    GunshipDustLeftOuterThinning,
    /// <summary>The gunship's outer dust cloud on its left: dust fading.</summary>
    GunshipDustLeftOuterFading,
    /// <summary>The gunship's outer dust cloud on its left: faint dust.</summary>
    GunshipDustLeftOuterFaint,
    /// <summary>A Torizo's explosion: a small spark.</summary>
    TorizoExplosionSpark,
    /// <summary>A Torizo's explosion: a ball of light.</summary>
    TorizoExplosionFlareBall,
    /// <summary>A Torizo's explosion: a ring of light around a burning core.</summary>
    TorizoExplosionFlareRingCored,
    /// <summary>A Torizo's explosion: a ring of light.</summary>
    TorizoExplosionFlareRing,
    /// <summary>A Torizo's explosion: a fading ring of light.</summary>
    TorizoExplosionFlareRingFading,
    /// <summary>
    /// A Torizo's death explosion: a small spark, the picture of TorizoExplosionSpark drawn over the room's front tiles, as
    /// the rest of the death explosion is.
    /// </summary>
    TorizoDeathExplosionSpark,
    /// <summary>A Torizo's explosion: a small fireball.</summary>
    TorizoExplosionFireballSmall,
    /// <summary>A Torizo's explosion: a large fireball.</summary>
    TorizoExplosionFireballLarge,
    /// <summary>A Torizo's explosion: a fireball opening into a flaming ring.</summary>
    TorizoExplosionFlamingRing,
    /// <summary>A Torizo's explosion: a starburst of flame.</summary>
    TorizoExplosionStarburst,
    /// <summary>A Torizo's explosion: scattering embers.</summary>
    TorizoExplosionEmbers,
    /// <summary>A Torizo's explosion: a cloud of fire.</summary>
    TorizoExplosionFireCloud,
    /// <summary>A Torizo's explosion: a cloud of fire spreading.</summary>
    TorizoExplosionFireCloudSpreading,
    /// <summary>A Torizo's explosion: a cloud of fire breaking up.</summary>
    TorizoExplosionFireCloudBreaking,
    /// <summary>A Torizo's explosion: the embers of a cloud of fire.</summary>
    TorizoExplosionFireCloudEmbers,
    /// <summary>A Torizo's drool: a falling drop.</summary>
    TorizoDroolDrop,
    /// <summary>A Torizo's drool: a splash rising.</summary>
    TorizoDroolSplashRising,
    /// <summary>A Torizo's drool: a splash spreading.</summary>
    TorizoDroolSplashSpreading,
    /// <summary>A Torizo's drool: a flat splash.</summary>
    TorizoDroolSplashFlat,
    /// <summary>A Torizo's explosive swipe: a small spark.</summary>
    TorizoSwipeSpark,
    /// <summary>A Torizo's explosive swipe: a ball of light.</summary>
    TorizoSwipeFlareBall,
    /// <summary>A Torizo's explosive swipe: a ring of light around a burning core.</summary>
    TorizoSwipeFlareRingCored,
    /// <summary>A Torizo's explosive swipe: a ring of light.</summary>
    TorizoSwipeFlareRing,
    /// <summary>A Torizo's explosive swipe: a fading ring of light.</summary>
    TorizoSwipeFlareRingFading,
    /// <summary>The Bomb Torizo statue's crumbling: a clump of moss.</summary>
    BombTorizoStatueMossClump,
    /// <summary>The Bomb Torizo statue's crumbling: rubble lit at its right.</summary>
    BombTorizoStatueRubbleLitRight,
    /// <summary>The Bomb Torizo statue's crumbling: rubble lit at its centre.</summary>
    BombTorizoStatueRubbleLitCenter,
    /// <summary>The Bomb Torizo statue's crumbling: rubble with one lit spot.</summary>
    BombTorizoStatueRubbleLitSpot,
    /// <summary>The Bomb Torizo statue's crumbling: rubble with moss on top.</summary>
    BombTorizoStatueRubbleMossyTop,
    /// <summary>The Bomb Torizo statue's crumbling: rubble with a speck of moss.</summary>
    BombTorizoStatueRubbleMossSpeck,
    /// <summary>The Bomb Torizo statue's crumbling: rubble lit along its base.</summary>
    BombTorizoStatueRubbleLitBase,
    /// <summary>The Bomb Torizo statue's crumbling: a flat slab lit along its edge.</summary>
    BombTorizoStatueSlab,
    /// <summary>A Torizo's Chozo orb: the orb facing left.</summary>
    TorizoChozoOrbFacingLeft,
    /// <summary>A Torizo's Chozo orb: the orb facing right.</summary>
    TorizoChozoOrbFacingRight,
    /// <summary>A Torizo's Chozo orb: a small spark.</summary>
    TorizoChozoOrbSpark,
    /// <summary>A Torizo's Chozo orb: a ball of light.</summary>
    TorizoChozoOrbFlareBall,
    /// <summary>A Torizo's Chozo orb: a ring of light around a burning core.</summary>
    TorizoChozoOrbFlareRingCored,
    /// <summary>A Torizo's Chozo orb: a ring of light.</summary>
    TorizoChozoOrbFlareRing,
    /// <summary>A Torizo's Chozo orb: a fading ring of light.</summary>
    TorizoChozoOrbFlareRingFading,
    /// <summary>A Torizo's Chozo orb: a burst of flame.</summary>
    TorizoChozoOrbFlameBurst,
    /// <summary>A Torizo's Chozo orb: a low dome of flame.</summary>
    TorizoChozoOrbFlameDome,
    /// <summary>A Torizo's Chozo orb: a short column of flame.</summary>
    TorizoChozoOrbFlameColumnShort,
    /// <summary>A Torizo's Chozo orb: a tall column of flame.</summary>
    TorizoChozoOrbFlameColumnTall,
    /// <summary>A Torizo's Chozo orb: a burst of flame above a dome of flame.</summary>
    TorizoChozoOrbFlameBurstAboveDome,
    /// <summary>A Torizo's Chozo orb: embers above a dome of flame.</summary>
    TorizoChozoOrbFlameEmbersAboveDome,
    /// <summary>A Torizo's sonic boom: a small crescent opening left.</summary>
    TorizoSonicBoomLeftSmall,
    /// <summary>A Torizo's sonic boom: a crescent opening left.</summary>
    TorizoSonicBoomLeftMedium,
    /// <summary>A Torizo's sonic boom: a tall crescent opening left.</summary>
    TorizoSonicBoomLeftLarge,
    /// <summary>A Torizo's sonic boom: a small crescent opening right.</summary>
    TorizoSonicBoomRightSmall,
    /// <summary>A Torizo's sonic boom: a crescent opening right.</summary>
    TorizoSonicBoomRightMedium,
    /// <summary>A Torizo's sonic boom: a tall crescent opening right.</summary>
    TorizoSonicBoomRightLarge,
    /// <summary>A Torizo's sonic boom: a small spark.</summary>
    TorizoSonicBoomSpark,
    /// <summary>A Torizo's sonic boom: a ball of light.</summary>
    TorizoSonicBoomFlareBall,
    /// <summary>A Torizo's sonic boom: a ring of light around a burning core.</summary>
    TorizoSonicBoomFlareRingCored,
    /// <summary>A Torizo's sonic boom: a ring of light.</summary>
    TorizoSonicBoomFlareRing,
    /// <summary>A Torizo's sonic boom: a fading ring of light.</summary>
    TorizoSonicBoomFlareRingFading,
    /// <summary>A Chozo statue's dust: a small puff of dust.</summary>
    ChozoDustPuff,
    /// <summary>A Chozo statue's dust: a puff of dust and a smaller one.</summary>
    ChozoDustPuffPair,
    /// <summary>A Chozo statue's dust: three puffs of dust.</summary>
    ChozoDustPuffTrio,
    /// <summary>A Chozo statue's dust: dense clouds of dust.</summary>
    ChozoDustCloudsDense,
    /// <summary>A Torizo's landing dust at its right foot: a small puff of dust.</summary>
    TorizoLandingDustRightDustPuff,
    /// <summary>A Torizo's landing dust at its right foot: a smaller puff of dust.</summary>
    TorizoLandingDustRightDustPuffSmall,
    /// <summary>A Torizo's landing dust at its right foot: a thin puff of dust.</summary>
    TorizoLandingDustRightDustPuffThin,
    /// <summary>A Torizo's landing dust at its right foot: a faint puff of dust.</summary>
    TorizoLandingDustRightDustPuffFaint,
    /// <summary>A Torizo's landing dust at its left foot: a small puff of dust.</summary>
    TorizoLandingDustLeftDustPuff,
    /// <summary>A Torizo's landing dust at its left foot: a smaller puff of dust.</summary>
    TorizoLandingDustLeftDustPuffSmall,
    /// <summary>A Torizo's landing dust at its left foot: a thin puff of dust.</summary>
    TorizoLandingDustLeftDustPuffThin,
    /// <summary>A Torizo's landing dust at its left foot: a faint puff of dust.</summary>
    TorizoLandingDustLeftDustPuffFaint,
    /// <summary>Golden Torizo's egg: a whole egg lit from the left.</summary>
    GoldenTorizoEggWholeLeft,
    /// <summary>Golden Torizo's egg: an egg cracked, lit from the left.</summary>
    GoldenTorizoEggCrackedLeft,
    /// <summary>Golden Torizo's egg: an egg splitting, lit from the left.</summary>
    GoldenTorizoEggSplittingLeft,
    /// <summary>Golden Torizo's egg: an egg hatching, an eye showing at its left.</summary>
    GoldenTorizoEggHatchingLeft,
    /// <summary>Golden Torizo's egg: a whole egg lit from the right.</summary>
    GoldenTorizoEggWholeRight,
    /// <summary>Golden Torizo's egg: an egg cracked, lit from the right.</summary>
    GoldenTorizoEggCrackedRight,
    /// <summary>Golden Torizo's egg: an egg splitting, lit from the right.</summary>
    GoldenTorizoEggSplittingRight,
    /// <summary>Golden Torizo's egg: an egg hatching, an eye showing at its right.</summary>
    GoldenTorizoEggHatchingRight,
    /// <summary>Golden Torizo's hatchling: facing left, claws raised.</summary>
    GoldenTorizoHatchlingLeftClawsRaised,
    /// <summary>Golden Torizo's hatchling: facing left, claws lowered.</summary>
    GoldenTorizoHatchlingLeftClawsLowered,
    /// <summary>Golden Torizo's hatchling: facing left, hunched.</summary>
    GoldenTorizoHatchlingLeftHunched,
    /// <summary>Golden Torizo's hatchling: facing right, claws raised.</summary>
    GoldenTorizoHatchlingRightClawsRaised,
    /// <summary>Golden Torizo's hatchling: facing right, claws lowered.</summary>
    GoldenTorizoHatchlingRightClawsLowered,
    /// <summary>Golden Torizo's hatchling: facing right, hunched.</summary>
    GoldenTorizoHatchlingRightHunched,
    /// <summary>Golden Torizo's hatchling: facing left, crumbling.</summary>
    GoldenTorizoHatchlingLeftCrumbling,
    /// <summary>Golden Torizo's hatchling: facing left, collapsing.</summary>
    GoldenTorizoHatchlingLeftCollapsing,
    /// <summary>Golden Torizo's hatchling: facing left, slumped.</summary>
    GoldenTorizoHatchlingLeftSlumped,
    /// <summary>Golden Torizo's hatchling: its remains, facing left.</summary>
    GoldenTorizoHatchlingLeftRemains,
    /// <summary>Golden Torizo's hatchling: its flattened remains, facing left.</summary>
    GoldenTorizoHatchlingLeftRemainsFlat,
    /// <summary>Golden Torizo's hatchling: facing right, crumbling.</summary>
    GoldenTorizoHatchlingRightCrumbling,
    /// <summary>Golden Torizo's hatchling: facing right, collapsing.</summary>
    GoldenTorizoHatchlingRightCollapsing,
    /// <summary>Golden Torizo's hatchling: facing right, slumped.</summary>
    GoldenTorizoHatchlingRightSlumped,
    /// <summary>Golden Torizo's hatchling: its remains, facing right.</summary>
    GoldenTorizoHatchlingRightRemains,
    /// <summary>Golden Torizo's hatchling: its flattened remains, facing right.</summary>
    GoldenTorizoHatchlingRightRemainsFlat,
    /// <summary>Golden Torizo's super missile: pointing down.</summary>
    GoldenTorizoSuperMissilePointingDown,
    /// <summary>Golden Torizo's super missile: pointing down and right.</summary>
    GoldenTorizoSuperMissilePointingDownRight,
    /// <summary>Golden Torizo's super missile: pointing right.</summary>
    GoldenTorizoSuperMissilePointingRight,
    /// <summary>Golden Torizo's super missile: pointing up and right.</summary>
    GoldenTorizoSuperMissilePointingUpRight,
    /// <summary>Golden Torizo's super missile: pointing up.</summary>
    GoldenTorizoSuperMissilePointingUp,
    /// <summary>Golden Torizo's super missile: pointing up and left.</summary>
    GoldenTorizoSuperMissilePointingUpLeft,
    /// <summary>Golden Torizo's super missile: pointing left.</summary>
    GoldenTorizoSuperMissilePointingLeft,
    /// <summary>Golden Torizo's super missile: pointing down and left.</summary>
    GoldenTorizoSuperMissilePointingDownLeft,
    /// <summary>Golden Torizo's super missile exploding: a small fireball.</summary>
    GoldenTorizoSuperMissileFireballSmall,
    /// <summary>Golden Torizo's super missile exploding: a large fireball.</summary>
    GoldenTorizoSuperMissileFireballLarge,
    /// <summary>Golden Torizo's super missile exploding: a fireball opening into a flaming ring.</summary>
    GoldenTorizoSuperMissileFlamingRing,
    /// <summary>Golden Torizo's super missile exploding: a huge fireball with a ring inside it.</summary>
    GoldenTorizoSuperMissileFireballHuge,
    /// <summary>Golden Torizo's super missile exploding: a wide ring of fire.</summary>
    GoldenTorizoSuperMissileWideRing,
    /// <summary>Golden Torizo's super missile exploding: a wide, fading ring of fire.</summary>
    GoldenTorizoSuperMissileWideRingFading,
    /// <summary>Golden Torizo's eye beam: a small spark.</summary>
    GoldenTorizoEyeBeamSpark,
    /// <summary>Golden Torizo's eye beam: a ball of light.</summary>
    GoldenTorizoEyeBeamFlareBall,
    /// <summary>Golden Torizo's eye beam: a ring of light around a burning core.</summary>
    GoldenTorizoEyeBeamFlareRingCored,
    /// <summary>Golden Torizo's eye beam: a ring of light.</summary>
    GoldenTorizoEyeBeamFlareRing,
    /// <summary>Golden Torizo's eye beam: a fading ring of light.</summary>
    GoldenTorizoEyeBeamFlareRingFading,
    /// <summary>Golden Torizo's eye beam: a burst of flame.</summary>
    GoldenTorizoEyeBeamFlameBurst,
    /// <summary>Golden Torizo's eye beam: a low dome of flame.</summary>
    GoldenTorizoEyeBeamFlameDome,
    /// <summary>Golden Torizo's eye beam: a short column of flame.</summary>
    GoldenTorizoEyeBeamFlameColumnShort,
    /// <summary>Golden Torizo's eye beam: a tall column of flame.</summary>
    GoldenTorizoEyeBeamFlameColumnTall,
    /// <summary>Golden Torizo's eye beam: a burst of flame above a dome of flame.</summary>
    GoldenTorizoEyeBeamFlameBurstAboveDome,
    /// <summary>Golden Torizo's eye beam: embers above a dome of flame.</summary>
    GoldenTorizoEyeBeamFlameEmbersAboveDome,
    /// <summary>The old Tourian escape shaft wall's explosion: a small spark.</summary>
    OldTourianWallExplosionSpark,
    /// <summary>The old Tourian escape shaft wall's explosion: a small fireball.</summary>
    OldTourianWallExplosionFireballSmall,
    /// <summary>The old Tourian escape shaft wall's explosion: a large fireball.</summary>
    OldTourianWallExplosionFireballLarge,
    /// <summary>The old Tourian escape shaft wall's explosion: a fireball opening into a flaming ring.</summary>
    OldTourianWallExplosionFlamingRing,
    /// <summary>The old Tourian escape shaft wall's explosion: a starburst of flame.</summary>
    OldTourianWallExplosionStarburst,
    /// <summary>The old Tourian escape shaft wall's explosion: scattering embers.</summary>
    OldTourianWallExplosionEmbers,
    /// <summary>A Dragon's fireball: rising to the left, flaring.</summary>
    DragonFireballRisingLeftFlaring,
    /// <summary>A Dragon's fireball: rising to the left, compact.</summary>
    DragonFireballRisingLeftCompact,
    /// <summary>A Dragon's fireball: rising to the right, flaring.</summary>
    DragonFireballRisingRightFlaring,
    /// <summary>A Dragon's fireball: rising to the right, compact.</summary>
    DragonFireballRisingRightCompact,
    /// <summary>A Dragon's fireball: falling to the left, flaring.</summary>
    DragonFireballFallingLeftFlaring,
    /// <summary>A Dragon's fireball: falling to the left, compact.</summary>
    DragonFireballFallingLeftCompact,
    /// <summary>A Dragon's fireball: falling to the right, flaring.</summary>
    DragonFireballFallingRightFlaring,
    /// <summary>A Dragon's fireball: falling to the right, compact.</summary>
    DragonFireballFallingRightCompact,
    /// <summary>An eye door's energy ball: a faint ring.</summary>
    EyeDoorBallRingFaint,
    /// <summary>An eye door's energy ball: a blue ring.</summary>
    EyeDoorBallRing,
    /// <summary>An eye door's energy ball: a bright ring around a blue core.</summary>
    EyeDoorBallRingCored,
    /// <summary>An eye door's energy ball: a whole blue ball.</summary>
    EyeDoorBallWhole,
    /// <summary>An eye door's energy ball: a cloud of fire.</summary>
    EyeDoorBallFireCloud,
    /// <summary>An eye door's energy ball: a cloud of fire spreading.</summary>
    EyeDoorBallFireCloudSpreading,
    /// <summary>An eye door's energy ball: a cloud of fire breaking up.</summary>
    EyeDoorBallFireCloudBreaking,
    /// <summary>An eye door's energy ball: the embers of a cloud of fire.</summary>
    EyeDoorBallFireCloudEmbers,
    /// <summary>An eye door's sweat: a falling drop.</summary>
    EyeDoorSweatDrop,
    /// <summary>An eye door's sweat: a low puff of smoke.</summary>
    EyeDoorSweatSmokePuffLow,
    /// <summary>An eye door's sweat: a puff of smoke rising in specks.</summary>
    EyeDoorSweatSmokePuffRising,
    /// <summary>An eye door's sweat: a puff of smoke scattering.</summary>
    EyeDoorSweatSmokePuffScattering,
    /// <summary>A Tourian statue's splash: a low splash.</summary>
    TourianStatueSplashLow,
    /// <summary>A Tourian statue's splash: a splash rising.</summary>
    TourianStatueSplashRising,
    /// <summary>A Tourian statue's splash: a splash scattering.</summary>
    TourianStatueSplashScattering,
    /// <summary>A Tourian statue's splash: a splash fading.</summary>
    TourianStatueSplashFading,
    /// <summary>A Tourian statue's eye glow: a faint, wide ring of specks.</summary>
    TourianStatueEyeGlowRingFaint,
    /// <summary>A Tourian statue's eye glow: a ring around a dash.</summary>
    TourianStatueEyeGlowRing,
    /// <summary>A Tourian statue's eye glow: a ring crossed by a bright cross.</summary>
    TourianStatueEyeGlowRingCrossed,
    /// <summary>A Tourian statue's eye glow: a bright diagonal streak.</summary>
    TourianStatueEyeGlowStreak,
    /// <summary>A Tourian statue's eye glow: a cross inside a ring.</summary>
    TourianStatueEyeGlowCrossRinged,
    /// <summary>A Tourian statue's eye glow: a sparkle.</summary>
    TourianStatueEyeGlowSparkle,
    /// <summary>A Tourian statue's eye glow: a small sparkle.</summary>
    TourianStatueEyeGlowSparkleSmall,
    /// <summary>A Tourian statue's eye glow: a small diagonal streak.</summary>
    TourianStatueEyeGlowStreakSmall,
    /// <summary>A Tourian statue's eye glow: one dot.</summary>
    TourianStatueEyeGlowDot,
    /// <summary>A Tourian statue's particle: a curl.</summary>
    TourianStatueParticleTurned0,
    /// <summary>A Tourian statue's particle: a curl turned a quarter.</summary>
    TourianStatueParticleTurned90,
    /// <summary>A Tourian statue's particle: a curl turned half round.</summary>
    TourianStatueParticleTurned180,
    /// <summary>A Tourian statue's particle: a curl turned three quarters.</summary>
    TourianStatueParticleTurned270,
    /// <summary>A Tourian statue's sparkle trail: bright sparkles.</summary>
    TourianStatueTailBright,
    /// <summary>A Tourian statue's sparkle trail: sparkles.</summary>
    TourianStatueTailSparkles,
    /// <summary>A Tourian statue's sparkle trail: dim sparkles.</summary>
    TourianStatueTailDim,
    /// <summary>A Tourian statue's sparkle trail: faint sparkles.</summary>
    TourianStatueTailFaint,
    /// <summary>A Tourian statue's soul: its mouth closed.</summary>
    TourianStatueSoulMouthClosed,
    /// <summary>A Tourian statue's soul: its mouth open.</summary>
    TourianStatueSoulMouthOpen,
    /// <summary>A Tourian statue's base decoration: the base's mossy decoration.</summary>
    TourianStatueBaseDecoration,
    /// <summary>The Tourian statue: Ridley's figure.</summary>
    TourianStatueRidley,
    /// <summary>The Tourian statue: Phantoon's figure.</summary>
    TourianStatuePhantoon,
    /// <summary>The Nuclear Waffle's fireball: a swirling fireball.</summary>
    NuclearWaffleFireballSwirl,
    /// <summary>The Nuclear Waffle's fireball: a blazing fireball.</summary>
    NuclearWaffleFireballBlazing,
    /// <summary>The Nuclear Waffle's fireball: a mottled fireball.</summary>
    NuclearWaffleFireballMottled,
    /// <summary>The Nuclear Waffle's fireball: a swirling fireball turned the other way.</summary>
    NuclearWaffleFireballSwirlTurned,
    /// <summary>The Nuclear Waffle's fireball: a blazing fireball in a dark ring.</summary>
    NuclearWaffleFireballBlazingRinged,
    /// <summary>The Nuclear Waffle's fireball: a densely mottled fireball.</summary>
    NuclearWaffleFireballMottledDense,
    /// <summary>The Nuclear Waffle's fireball: a mottled fireball turned the other way.</summary>
    NuclearWaffleFireballMottledTurned,
    /// <summary>The Nuclear Waffle's fireball: a sparsely mottled fireball.</summary>
    NuclearWaffleFireballMottledSparse,
    /// <summary>The Polyp's lava rock: a glowing rock.</summary>
    PolypRock,
    /// <summary>A Mother Brain turret: its barrel facing left.</summary>
    MotherBrainTurretFacingLeft,
    /// <summary>A Mother Brain turret: its barrel facing down and left.</summary>
    MotherBrainTurretFacingDownLeft,
    /// <summary>A Mother Brain turret: its barrel facing down.</summary>
    MotherBrainTurretFacingDown,
    /// <summary>A Mother Brain turret: its barrel facing down and right.</summary>
    MotherBrainTurretFacingDownRight,
    /// <summary>A Mother Brain turret: its barrel facing right.</summary>
    MotherBrainTurretFacingRight,
    /// <summary>A Mother Brain turret: its barrel facing up and right.</summary>
    MotherBrainTurretFacingUpRight,
    /// <summary>A Mother Brain turret: its barrel facing up.</summary>
    MotherBrainTurretFacingUp,
    /// <summary>A Mother Brain turret: its barrel facing up and left.</summary>
    MotherBrainTurretFacingUpLeft,
    /// <summary>A Mother Brain turret's bullet: flying left.</summary>
    MotherBrainTurretBulletFacingLeft,
    /// <summary>A Mother Brain turret's bullet: flying down and left.</summary>
    MotherBrainTurretBulletFacingDownLeft,
    /// <summary>A Mother Brain turret's bullet: flying down.</summary>
    MotherBrainTurretBulletFacingDown,
    /// <summary>A Mother Brain turret's bullet: flying down and right.</summary>
    MotherBrainTurretBulletFacingDownRight,
    /// <summary>A Mother Brain turret's bullet: flying right.</summary>
    MotherBrainTurretBulletFacingRight,
    /// <summary>A Mother Brain turret's bullet: flying up and right.</summary>
    MotherBrainTurretBulletFacingUpRight,
    /// <summary>A Mother Brain turret's bullet: flying up.</summary>
    MotherBrainTurretBulletFacingUp,
    /// <summary>A Mother Brain turret's bullet: flying up and left.</summary>
    MotherBrainTurretBulletFacingUpLeft,
    /// <summary>A Mother Brain turret's bullet: a cloud of fire.</summary>
    MotherBrainTurretBulletFireCloud,
    /// <summary>A Mother Brain turret's bullet: a cloud of fire spreading.</summary>
    MotherBrainTurretBulletFireCloudSpreading,
    /// <summary>A Mother Brain turret's bullet: a cloud of fire breaking up.</summary>
    MotherBrainTurretBulletFireCloudBreaking,
    /// <summary>A Mother Brain turret's bullet: the embers of a cloud of fire.</summary>
    MotherBrainTurretBulletFireCloudEmbers,
    /// <summary>A Mother Brain turret's bullet: one faint speck.</summary>
    MotherBrainTurretBulletSpeck,
    /// <summary>Mother Brain's blue ring: a small blue ring, just formed.</summary>
    MotherBrainBlueRingForming,
    /// <summary>Mother Brain's blue ring: the blue ring opening out.</summary>
    MotherBrainBlueRingOpening,
    /// <summary>Mother Brain's blue ring: the blue ring spreading wider.</summary>
    MotherBrainBlueRingSpreading,
    /// <summary>Mother Brain's blue ring: the blue ring growing on.</summary>
    MotherBrainBlueRingGrowing,
    /// <summary>Mother Brain's blue ring: the blue ring swelling to nearly its full size.</summary>
    MotherBrainBlueRingSwelling,
    /// <summary>Mother Brain's blue ring: the blue ring at its full size.</summary>
    MotherBrainBlueRingFullSize,
    /// <summary>Mother Brain's blue ring exploding: a small fireball.</summary>
    MotherBrainBlueRingFireballSmall,
    /// <summary>Mother Brain's blue ring exploding: a large fireball.</summary>
    MotherBrainBlueRingFireballLarge,
    /// <summary>Mother Brain's blue ring exploding: a fireball opening into a flaming ring.</summary>
    MotherBrainBlueRingFlamingRing,
    /// <summary>Mother Brain's blue ring exploding: a huge fireball with a ring inside it.</summary>
    MotherBrainBlueRingFireballHuge,
    /// <summary>Mother Brain's blue ring exploding: a wide ring of fire.</summary>
    MotherBrainBlueRingWideRing,
    /// <summary>Mother Brain's blue ring exploding: a wide, fading ring of fire.</summary>
    MotherBrainBlueRingWideRingFading,
    /// <summary>Mother Brain's bomb: a dark ball glowing red.</summary>
    MotherBrainBombGlowRed,
    /// <summary>Mother Brain's bomb: a dark ball glowing orange at its right.</summary>
    MotherBrainBombGlowOrangeRight,
    /// <summary>Mother Brain's bomb: a dark ball glowing yellow at its right.</summary>
    MotherBrainBombGlowYellowRight,
    /// <summary>Mother Brain's bomb: a dark ball glowing yellow at its top.</summary>
    MotherBrainBombGlowYellowTop,
    /// <summary>Mother Brain's bomb: a dark ball glowing orange at its centre.</summary>
    MotherBrainBombGlowOrangeCenter,
    /// <summary>Mother Brain's bomb: a dark ball glowing yellow at its bottom.</summary>
    MotherBrainBombGlowYellowBottom,
    /// <summary>Mother Brain's bomb: a dark ball glowing yellow at its left.</summary>
    MotherBrainBombGlowYellowLeft,
    /// <summary>Mother Brain's bomb: a dark ball glowing orange at its left.</summary>
    MotherBrainBombGlowOrangeLeft,
    /// <summary>Mother Brain's bomb: a dark ball glowing a dim red.</summary>
    MotherBrainBombGlowRedDim,
    /// <summary>Mother Brain's hand beam: a small red orb.</summary>
    MotherBrainHandBeamOrbSmall,
    /// <summary>Mother Brain's hand beam: a red orb with a yellow speck.</summary>
    MotherBrainHandBeamOrbRed,
    /// <summary>Mother Brain's hand beam: a red orb with a yellow core.</summary>
    MotherBrainHandBeamOrbRedYellowCore,
    /// <summary>Mother Brain's hand beam: an orange orb with a white core.</summary>
    MotherBrainHandBeamOrbOrange,
    /// <summary>Mother Brain's hand beam: an orange orb with a wide white core.</summary>
    MotherBrainHandBeamOrbWhiteCore,
    /// <summary>Mother Brain's hand beam: a yellow orb with a white core.</summary>
    MotherBrainHandBeamOrbYellow,
    /// <summary>Mother Brain's hand beam: a white orb with a yellow rim.</summary>
    MotherBrainHandBeamOrbWhite,
    /// <summary>Mother Brain's rainbow beam charging: sparks spread 86 pixels across.</summary>
    MotherBrainRainbowChargeSparksSpread,
    /// <summary>Mother Brain's rainbow beam charging: sparks closing in, 80 pixels across.</summary>
    MotherBrainRainbowChargeSparksClosing,
    /// <summary>Mother Brain's rainbow beam charging: streaks closing in, 73 pixels across.</summary>
    MotherBrainRainbowChargeStreaksClosing,
    /// <summary>Mother Brain's rainbow beam charging: streaks drawing near, 64 pixels across.</summary>
    MotherBrainRainbowChargeStreaksNear,
    /// <summary>Mother Brain's rainbow beam charging: streaks gathered, 50 pixels across.</summary>
    MotherBrainRainbowChargeStreaksGathered,
    /// <summary>Mother Brain's rainbow beam charging: streaks and fireballs, 90 pixels across.</summary>
    MotherBrainRainbowChargeStreaksAndFireballs,
    /// <summary>Mother Brain's drool: a speck of drool.</summary>
    MotherBrainDroolSpeck,
    /// <summary>Mother Brain's drool: a bead of drool.</summary>
    MotherBrainDroolBead,
    /// <summary>Mother Brain's drool: a drop of drool hanging.</summary>
    MotherBrainDroolHanging,
    /// <summary>Mother Brain's drool: drool stretching.</summary>
    MotherBrainDroolStretching,
    /// <summary>Mother Brain's drool: a long strand of drool.</summary>
    MotherBrainDroolStrand,
    /// <summary>Mother Brain's drool: a falling drop.</summary>
    MotherBrainDroolDrop,
    /// <summary>Mother Brain's drool: a splash rising.</summary>
    MotherBrainDroolSplashRising,
    /// <summary>Mother Brain's drool: a scattered splash.</summary>
    MotherBrainDroolSplashScattered,
    /// <summary>Mother Brain's drool: a flat splash.</summary>
    MotherBrainDroolSplashFlat,
    /// <summary>Mother Brain's drool: a faint splash.</summary>
    MotherBrainDroolSplashFaint,
    /// <summary>The escape door's debris: a chip seen edge on.</summary>
    MotherBrainEscapeDoorDebrisEdgeTurned0,
    /// <summary>The escape door's debris: a chip seen face on.</summary>
    MotherBrainEscapeDoorDebrisFaceTurned0,
    /// <summary>The escape door's debris: a chip seen edge on, turned a quarter.</summary>
    MotherBrainEscapeDoorDebrisEdgeTurned90,
    /// <summary>The escape door's debris: a chip seen face on, turned a quarter.</summary>
    MotherBrainEscapeDoorDebrisFaceTurned90,
    /// <summary>The escape door's debris: a chip seen edge on, turned half round.</summary>
    MotherBrainEscapeDoorDebrisEdgeTurned180,
    /// <summary>The escape door's debris: a chip seen face on, turned half round.</summary>
    MotherBrainEscapeDoorDebrisFaceTurned180,
    /// <summary>The escape door's debris: a chip seen edge on, turned three quarters.</summary>
    MotherBrainEscapeDoorDebrisEdgeTurned270,
    /// <summary>The escape door's debris: a chip seen face on, turned three quarters.</summary>
    MotherBrainEscapeDoorDebrisFaceTurned270,
    /// <summary>Mother Brain's large purple breath: a puff.</summary>
    MotherBrainBreathLargePuff,
    /// <summary>Mother Brain's large purple breath: a short plume.</summary>
    MotherBrainBreathLargePlume,
    /// <summary>Mother Brain's large purple breath: a long plume.</summary>
    MotherBrainBreathLargePlumeLong,
    /// <summary>Mother Brain's large purple breath: a full plume.</summary>
    MotherBrainBreathLargePlumeFull,
    /// <summary>Mother Brain's large purple breath: the plume dispersing.</summary>
    MotherBrainBreathLargeDispersing,
    /// <summary>Mother Brain's large purple breath: the plume thinning.</summary>
    MotherBrainBreathLargeThinning,
    /// <summary>Mother Brain's large purple breath: the plume fading.</summary>
    MotherBrainBreathLargeFading,
    /// <summary>Mother Brain's large purple breath: a faint wisp.</summary>
    MotherBrainBreathLargeFaint,
    /// <summary>Mother Brain's small purple breath: a puff.</summary>
    MotherBrainBreathSmallPuff,
    /// <summary>Mother Brain's small purple breath: a puff growing.</summary>
    MotherBrainBreathSmallPuffGrowing,
    /// <summary>Mother Brain's small purple breath: a cloud.</summary>
    MotherBrainBreathSmallCloud,
    /// <summary>Mother Brain's small purple breath: a long cloud.</summary>
    MotherBrainBreathSmallCloudLong,
    /// <summary>Mother Brain's small purple breath: the cloud dispersing.</summary>
    MotherBrainBreathSmallDispersing,
    /// <summary>Mother Brain's small purple breath: the cloud thinning.</summary>
    MotherBrainBreathSmallThinning,
    /// <summary>Mother Brain's small purple breath: the cloud fading.</summary>
    MotherBrainBreathSmallFading,
    /// <summary>Mother Brain's small purple breath: a faint speck.</summary>
    MotherBrainBreathSmallFaint,
    /// <summary>Mother Brain's room: the escape subtitle.</summary>
    MotherBrainEscapeSubtitle,
    /// <summary>Mother Brain's room: the top tube's top right piece.</summary>
    MotherBrainTopTubeTopRight,
    /// <summary>Mother Brain's room: the top tube's top left piece.</summary>
    MotherBrainTopTubeTopLeft,
    /// <summary>Mother Brain's room: the top tube's top middle left piece.</summary>
    MotherBrainTopTubeTopMiddleLeft,
    /// <summary>Mother Brain's room: the top tube's top middle right piece.</summary>
    MotherBrainTopTubeTopMiddleRight,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up, tumbled 0 degrees.</summary>
    MotherBrainGlassShardUpTurned0,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up, tumbled 45 degrees.</summary>
    MotherBrainGlassShardUpTurned45,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up, tumbled 90 degrees.</summary>
    MotherBrainGlassShardUpTurned90,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up, tumbled 135 degrees.</summary>
    MotherBrainGlassShardUpTurned135,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up, tumbled 180 degrees.</summary>
    MotherBrainGlassShardUpTurned180,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up, tumbled 225 degrees.</summary>
    MotherBrainGlassShardUpTurned225,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up, tumbled 270 degrees.</summary>
    MotherBrainGlassShardUpTurned270,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up, tumbled 315 degrees.</summary>
    MotherBrainGlassShardUpTurned315,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and right, tumbled 0 degrees.</summary>
    MotherBrainGlassShardUpRightTurned0,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and right, tumbled 45 degrees.</summary>
    MotherBrainGlassShardUpRightTurned45,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and right, tumbled 90 degrees.</summary>
    MotherBrainGlassShardUpRightTurned90,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and right, tumbled 135 degrees.</summary>
    MotherBrainGlassShardUpRightTurned135,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and right, tumbled 180 degrees.</summary>
    MotherBrainGlassShardUpRightTurned180,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and right, tumbled 225 degrees.</summary>
    MotherBrainGlassShardUpRightTurned225,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and right, tumbled 270 degrees.</summary>
    MotherBrainGlassShardUpRightTurned270,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and right, tumbled 315 degrees.</summary>
    MotherBrainGlassShardUpRightTurned315,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying right, tumbled 0 degrees.</summary>
    MotherBrainGlassShardRightTurned0,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying right, tumbled 45 degrees.</summary>
    MotherBrainGlassShardRightTurned45,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying right, tumbled 90 degrees.</summary>
    MotherBrainGlassShardRightTurned90,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying right, tumbled 135 degrees.</summary>
    MotherBrainGlassShardRightTurned135,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying right, tumbled 180 degrees.</summary>
    MotherBrainGlassShardRightTurned180,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying right, tumbled 225 degrees.</summary>
    MotherBrainGlassShardRightTurned225,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying right, tumbled 270 degrees.</summary>
    MotherBrainGlassShardRightTurned270,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying right, tumbled 315 degrees.</summary>
    MotherBrainGlassShardRightTurned315,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and right, tumbled 0 degrees.</summary>
    MotherBrainGlassShardDownRightTurned0,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and right, tumbled 45 degrees.</summary>
    MotherBrainGlassShardDownRightTurned45,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and right, tumbled 90 degrees.</summary>
    MotherBrainGlassShardDownRightTurned90,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and right, tumbled 135 degrees.</summary>
    MotherBrainGlassShardDownRightTurned135,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and right, tumbled 180 degrees.</summary>
    MotherBrainGlassShardDownRightTurned180,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and right, tumbled 225 degrees.</summary>
    MotherBrainGlassShardDownRightTurned225,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and right, tumbled 270 degrees.</summary>
    MotherBrainGlassShardDownRightTurned270,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and right, tumbled 315 degrees.</summary>
    MotherBrainGlassShardDownRightTurned315,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down, tumbled 0 degrees.</summary>
    MotherBrainGlassShardDownTurned0,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down, tumbled 45 degrees.</summary>
    MotherBrainGlassShardDownTurned45,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down, tumbled 90 degrees.</summary>
    MotherBrainGlassShardDownTurned90,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down, tumbled 135 degrees.</summary>
    MotherBrainGlassShardDownTurned135,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down, tumbled 180 degrees.</summary>
    MotherBrainGlassShardDownTurned180,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down, tumbled 225 degrees.</summary>
    MotherBrainGlassShardDownTurned225,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down, tumbled 270 degrees.</summary>
    MotherBrainGlassShardDownTurned270,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down, tumbled 315 degrees.</summary>
    MotherBrainGlassShardDownTurned315,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and left, tumbled 0 degrees.</summary>
    MotherBrainGlassShardDownLeftTurned0,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and left, tumbled 45 degrees.</summary>
    MotherBrainGlassShardDownLeftTurned45,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and left, tumbled 90 degrees.</summary>
    MotherBrainGlassShardDownLeftTurned90,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and left, tumbled 135 degrees.</summary>
    MotherBrainGlassShardDownLeftTurned135,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and left, tumbled 180 degrees.</summary>
    MotherBrainGlassShardDownLeftTurned180,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and left, tumbled 225 degrees.</summary>
    MotherBrainGlassShardDownLeftTurned225,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and left, tumbled 270 degrees.</summary>
    MotherBrainGlassShardDownLeftTurned270,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying down and left, tumbled 315 degrees.</summary>
    MotherBrainGlassShardDownLeftTurned315,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying left, tumbled 0 degrees.</summary>
    MotherBrainGlassShardLeftTurned0,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying left, tumbled 45 degrees.</summary>
    MotherBrainGlassShardLeftTurned45,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying left, tumbled 90 degrees.</summary>
    MotherBrainGlassShardLeftTurned90,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying left, tumbled 135 degrees.</summary>
    MotherBrainGlassShardLeftTurned135,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying left, tumbled 180 degrees.</summary>
    MotherBrainGlassShardLeftTurned180,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying left, tumbled 225 degrees.</summary>
    MotherBrainGlassShardLeftTurned225,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying left, tumbled 270 degrees.</summary>
    MotherBrainGlassShardLeftTurned270,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying left, tumbled 315 degrees.</summary>
    MotherBrainGlassShardLeftTurned315,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and left, tumbled 0 degrees.</summary>
    MotherBrainGlassShardUpLeftTurned0,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and left, tumbled 45 degrees.</summary>
    MotherBrainGlassShardUpLeftTurned45,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and left, tumbled 90 degrees.</summary>
    MotherBrainGlassShardUpLeftTurned90,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and left, tumbled 135 degrees.</summary>
    MotherBrainGlassShardUpLeftTurned135,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and left, tumbled 180 degrees.</summary>
    MotherBrainGlassShardUpLeftTurned180,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and left, tumbled 225 degrees.</summary>
    MotherBrainGlassShardUpLeftTurned225,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and left, tumbled 270 degrees.</summary>
    MotherBrainGlassShardUpLeftTurned270,
    /// <summary>A shard of Mother Brain's tank glass: a shard flying up and left, tumbled 315 degrees.</summary>
    MotherBrainGlassShardUpLeftTurned315,
    /// <summary>A sparkle of Mother Brain's tank glass: an upright cross.</summary>
    MotherBrainGlassSparkleCross,
    /// <summary>A sparkle of Mother Brain's tank glass: a small upright cross.</summary>
    MotherBrainGlassSparkleCrossSmall,
    /// <summary>A sparkle of Mother Brain's tank glass: a diagonal cross.</summary>
    MotherBrainGlassSparkleDiagonal,
    /// <summary>A sparkle of Mother Brain's tank glass: a faint diagonal cross.</summary>
    MotherBrainGlassSparkleDiagonalFaint,
    /// <summary>A Ki-Hunter's acid spit: a droplet flying left.</summary>
    KiHunterAcidSpitLeftDroplet,
    /// <summary>A Ki-Hunter's acid spit: a streak flying left.</summary>
    KiHunterAcidSpitLeftStreak,
    /// <summary>A Ki-Hunter's acid spit: a blob flying left.</summary>
    KiHunterAcidSpitLeftBlob,
    /// <summary>A Ki-Hunter's acid spit: a blob trailing drops to its right.</summary>
    KiHunterAcidSpitLeftBlobTrailing,
    /// <summary>A Ki-Hunter's acid spit: a blob spattering drops behind it.</summary>
    KiHunterAcidSpitLeftBlobSpattering,
    /// <summary>A Ki-Hunter's acid spit: a round blob.</summary>
    KiHunterAcidSpitLeftBall,
    /// <summary>A Ki-Hunter's acid spit: a mound of acid.</summary>
    KiHunterAcidSpitSplashMound,
    /// <summary>A Ki-Hunter's acid spit: acid spreading.</summary>
    KiHunterAcidSpitSplashSpreading,
    /// <summary>A Ki-Hunter's acid spit: a wide splash.</summary>
    KiHunterAcidSpitSplashWide,
    /// <summary>A Ki-Hunter's acid spit: a flat splash.</summary>
    KiHunterAcidSpitSplashFlat,
    /// <summary>A Ki-Hunter's acid spit: a splash fading.</summary>
    KiHunterAcidSpitSplashFading,
    /// <summary>A Ki-Hunter's acid spit: a droplet flying right.</summary>
    KiHunterAcidSpitRightDroplet,
    /// <summary>A Ki-Hunter's acid spit: a streak flying right.</summary>
    KiHunterAcidSpitRightStreak,
    /// <summary>A Ki-Hunter's acid spit: a blob flying right.</summary>
    KiHunterAcidSpitRightBlob,
    /// <summary>A Ki-Hunter's acid spit: a blob trailing drops to its left.</summary>
    KiHunterAcidSpitRightBlobTrailing,
    /// <summary>A Ki-Hunter's acid spit: a blob spattering drops behind it.</summary>
    KiHunterAcidSpitRightBlobSpattering,
    /// <summary>A Ki-Hunter's acid spit: a round blob.</summary>
    KiHunterAcidSpitRightBall,
    /// <summary>A Kago bug: curled up into a pale ball.</summary>
    KagoBugCurledUp,
    /// <summary>A Kago bug: crouched, legs tucked.</summary>
    KagoBugCrouched,
    /// <summary>A Kago bug: standing, legs out.</summary>
    KagoBugStanding,
    /// <summary>A Kago bug: leaping, legs stretched.</summary>
    KagoBugLeaping,
    /// <summary>A Kago bug bursting: a small spark.</summary>
    KagoBugSpark,
    /// <summary>A Kago bug bursting: a ball of light.</summary>
    KagoBugFlareBall,
    /// <summary>A Kago bug bursting: a ring of light around a burning core.</summary>
    KagoBugFlareRingCored,
    /// <summary>A Kago bug bursting: a ring of light.</summary>
    KagoBugFlareRing,
    /// <summary>A Kago bug bursting: a fading ring of light.</summary>
    KagoBugFlareRingFading,
    /// <summary>A Powamp's spike: a spiked star.</summary>
    PowampSpikeStar,
    /// <summary>A Powamp's spike: a cluster of spikes.</summary>
    PowampSpikeCluster,
    /// <summary>A Powamp's spike: one glowing point.</summary>
    PowampSpikeGlow,
    /// <summary>The work robot's laser: a glint of light.</summary>
    WorkRobotLaserFlareGlint,
    /// <summary>The work robot's laser: a small glow.</summary>
    WorkRobotLaserFlareGlow,
    /// <summary>The work robot's laser: a ball of light.</summary>
    WorkRobotLaserFlareBall,
    /// <summary>The work robot's laser: a ring of light around a burning core.</summary>
    WorkRobotLaserFlareRingCored,
    /// <summary>The work robot's laser: a ring of light.</summary>
    WorkRobotLaserFlareRing,
    /// <summary>The work robot's laser: a fading ring of light.</summary>
    WorkRobotLaserFlareRingFading,
    /// <summary>The noob tube's crack: a small star of cracks where the glass gives.</summary>
    NoobTubeCrackStarting,
    /// <summary>The noob tube's crack: the star's cracks spreading out and forking.</summary>
    NoobTubeCrackSpreading,
    /// <summary>The noob tube's crack: a web of cracks widening across the glass.</summary>
    NoobTubeCrackWidening,
    /// <summary>The noob tube's crack: the web of cracks reaching most of the way across the tube.</summary>
    NoobTubeCrackReaching,
    /// <summary>The noob tube's crack: the web of cracks spanning the tube from side to side.</summary>
    NoobTubeCrackSpanning,
    /// <summary>The noob tube's crack: the web of cracks across the tube branching further at its edges.</summary>
    NoobTubeCrackBranching,
    /// <summary>The noob tube's shards: six shards bursting out of the shattered glass.</summary>
    NoobTubeShardsBursting,
    /// <summary>The noob tube's shards: the six shards flying out from the tube.</summary>
    NoobTubeShardsFlyingOut,
    /// <summary>The noob tube's shards: the six shards flying apart, the upper pair still climbing.</summary>
    NoobTubeShardsFlyingApart,
    /// <summary>The noob tube's shards: the six shards spreading wide.</summary>
    NoobTubeShardsSpreading,
    /// <summary>The noob tube's shards: the six shards slowing as they spread.</summary>
    NoobTubeShardsSlowing,
    /// <summary>The noob tube's shards: the upper pair of shards at their highest, the others still spreading.</summary>
    NoobTubeShardsPeaking,
    /// <summary>The noob tube's shards: the upper pair of shards turning to fall, the outer pair still spreading.</summary>
    NoobTubeShardsTurning,
    /// <summary>The noob tube's shards: the outer pair of shards coasting outward as the upper pair sinks.</summary>
    NoobTubeShardsCoasting,
    /// <summary>The noob tube's shards: the outer pair of shards tipping downward as they near their widest.</summary>
    NoobTubeShardsTipping,
    /// <summary>The noob tube's shards: the six shards at their widest, every one of them starting down.</summary>
    NoobTubeShardsAtWidest,
    /// <summary>The noob tube's shards: the six shards dipping as they begin to settle, each at its own pace.</summary>
    NoobTubeShardsDipping,
    /// <summary>The noob tube's shards: the six shards dropping a little lower, rocking as they go.</summary>
    NoobTubeShardsDropping,
    /// <summary>The noob tube's shards: the six shards sinking.</summary>
    NoobTubeShardsSinking,
    /// <summary>The noob tube's shards: the six shards lowering further.</summary>
    NoobTubeShardsLowering,
    /// <summary>The noob tube's shards: the six shards descending.</summary>
    NoobTubeShardsDescending,
    /// <summary>The noob tube's shards: the six shards falling slowly.</summary>
    NoobTubeShardsFalling,
    /// <summary>The noob tube's shards: the six shards drifting down.</summary>
    NoobTubeShardsDrifting,
    /// <summary>The noob tube's shards: the six shards floating down.</summary>
    NoobTubeShardsFloating,
    /// <summary>The noob tube's shards: the six shards wafting down.</summary>
    NoobTubeShardsWafting,
    /// <summary>The noob tube's shards: the six shards gliding down, halfway through settling.</summary>
    NoobTubeShardsGliding,
    /// <summary>The noob tube's shards: the six shards sailing down.</summary>
    NoobTubeShardsSailing,
    /// <summary>The noob tube's shards: the six shards fluttering down.</summary>
    NoobTubeShardsFluttering,
    /// <summary>The noob tube's shards: the six shards subsiding.</summary>
    NoobTubeShardsSubsiding,
    /// <summary>The noob tube's shards: the six shards slipping lower.</summary>
    NoobTubeShardsSlipping,
    /// <summary>The noob tube's shards: the six shards sifting down.</summary>
    NoobTubeShardsSifting,
    /// <summary>The noob tube's shards: the six shards settling.</summary>
    NoobTubeShardsSettling,
    /// <summary>The noob tube's shards: the six shards settling lower.</summary>
    NoobTubeShardsSettlingLower,
    /// <summary>The noob tube's shards: the six shards nearly settled.</summary>
    NoobTubeShardsNearlySettled,
    /// <summary>The noob tube's shards: the six shards coming to rest, which the fallen crack shows by turns with <see cref="NoobTubeShardsLanded"/>.</summary>
    NoobTubeShardsComingToRest,
    /// <summary>The noob tube's shards: six shards landed.</summary>
    NoobTubeShardsLanded,
    /// <summary>A noob tube shard: its down outward piece.</summary>
    NoobTubeShardDownOutward,
    /// <summary>A noob tube shard: its down outward piece, mirrored.</summary>
    NoobTubeShardDownOutwardMirrored,
    /// <summary>A noob tube shard: its up outward piece.</summary>
    NoobTubeShardUpOutward,
    /// <summary>A noob tube shard: its up outward piece, mirrored.</summary>
    NoobTubeShardUpOutwardMirrored,
    /// <summary>A noob tube shard: its up steep piece.</summary>
    NoobTubeShardUpSteep,
    /// <summary>A noob tube shard: its up steep piece, mirrored.</summary>
    NoobTubeShardUpSteepMirrored,
    /// <summary>A noob tube shard: its up diagonal inner piece.</summary>
    NoobTubeShardUpDiagonalInner,
    /// <summary>A noob tube shard: its up diagonal inner piece, mirrored.</summary>
    NoobTubeShardUpDiagonalInnerMirrored,
    /// <summary>A noob tube shard: its up diagonal outer piece.</summary>
    NoobTubeShardUpDiagonalOuter,
    /// <summary>A noob tube shard: its up diagonal outer piece, mirrored.</summary>
    NoobTubeShardUpDiagonalOuterMirrored,
    /// <summary>A noob tube shard: its down diagonal piece.</summary>
    NoobTubeShardDownDiagonal,
    /// <summary>A noob tube shard: its down diagonal piece, mirrored.</summary>
    NoobTubeShardDownDiagonalMirrored,
    /// <summary>A noob tube shard: its down outer piece.</summary>
    NoobTubeShardDownOuter,
    /// <summary>A noob tube shard: its down outer piece, mirrored.</summary>
    NoobTubeShardDownOuterMirrored,
    /// <summary>A noob tube shard: its outward piece.</summary>
    NoobTubeShardOutward,
    /// <summary>A noob tube shard: its outward piece, mirrored.</summary>
    NoobTubeShardOutwardMirrored,
    /// <summary>A noob tube shard: its up piece.</summary>
    NoobTubeShardUp,
    /// <summary>A noob tube shard: its down inner piece.</summary>
    NoobTubeShardDownInner,
    /// <summary>A noob tube shard: its down inner piece, mirrored.</summary>
    NoobTubeShardDownInnerMirrored,
    /// <summary>The noob tube's dust: a cloud, its trail just forming below it.</summary>
    NoobTubeDustTrailForming,
    /// <summary>The noob tube's dust: a cloud, its trail growing.</summary>
    NoobTubeDustTrailGrowing,
    /// <summary>The noob tube's dust: a cloud, its trail lengthening.</summary>
    NoobTubeDustTrailLengthening,
    /// <summary>The noob tube's dust: a cloud with its trail at full length.</summary>
    NoobTubeDustTrailFullLength,
    /// <summary>The noob tube's dust: the trail thinning.</summary>
    NoobTubeDustTrailThinning,
    /// <summary>The noob tube's dust: a thin trail.</summary>
    NoobTubeDustTrailThin,
    /// <summary>The noob tube's dust: the trail breaking into bubbles.</summary>
    NoobTubeDustTrailBreaking,
    /// <summary>The noob tube's dust: bubbles forming.</summary>
    NoobTubeDustBubblesForming,
    /// <summary>The noob tube's dust: bubbles.</summary>
    NoobTubeDustBubbles,
    /// <summary>The noob tube's dust: bubbles drifting.</summary>
    NoobTubeDustBubblesDrifting,
    /// <summary>The noob tube's dust: sparse bubbles.</summary>
    NoobTubeDustBubblesSparse,
    /// <summary>The noob tube's dust: bubbles bursting.</summary>
    NoobTubeDustBubblesBursting,
    /// <summary>The noob tube's dust: specks.</summary>
    NoobTubeDustSpecks,
    /// <summary>The noob tube's dust: faint specks.</summary>
    NoobTubeDustSpecksFaint,
    /// <summary>A Cacatac's spike: pointing left, a pixel higher.</summary>
    CacatacSpikeLeftHigh,
    /// <summary>A Cacatac's spike: pointing up and left.</summary>
    CacatacSpikeUpLeft,
    /// <summary>A Cacatac's spike: pointing up.</summary>
    CacatacSpikeUp,
    /// <summary>A Cacatac's spike: pointing up and right.</summary>
    CacatacSpikeUpRight,
    /// <summary>A Cacatac's spike: pointing right, a pixel higher.</summary>
    CacatacSpikeRightHigh,
    /// <summary>A Cacatac's spike: pointing left, a pixel lower.</summary>
    CacatacSpikeLeftLow,
    /// <summary>A Cacatac's spike: pointing down and left.</summary>
    CacatacSpikeDownLeft,
    /// <summary>A Cacatac's spike: pointing down.</summary>
    CacatacSpikeDown,
    /// <summary>A Cacatac's spike: pointing down and right.</summary>
    CacatacSpikeDownRight,
    /// <summary>A Cacatac's spike: pointing right, a pixel lower.</summary>
    CacatacSpikeRightLow,
    /// <summary>Spore Spawn's spore spawner: closed.</summary>
    SporeSpawnSpawnerClosed,
    /// <summary>Spore Spawn's spore spawner: opening.</summary>
    SporeSpawnSpawnerOpening,
    /// <summary>Spore Spawn's spore spawner: open.</summary>
    SporeSpawnSpawnerOpen,
    /// <summary>A Spore Spawn spore: glowing.</summary>
    SporeSpawnSporeGlowing,
    /// <summary>A Spore Spawn spore: dim.</summary>
    SporeSpawnSporeDim,
    /// <summary>A Spore Spawn spore: sparkling.</summary>
    SporeSpawnSporeSparkling,
    /// <summary>Spore Spawn's stalk: one segment.</summary>
    SporeSpawnStalk,
    /// <summary>A Spore Spawn spore bursting: a small spark.</summary>
    SporeSpawnSporeBurstSpark,
    /// <summary>A Spore Spawn spore bursting: a small fireball.</summary>
    SporeSpawnSporeBurstFireballSmall,
    /// <summary>A Spore Spawn spore bursting: a large fireball.</summary>
    SporeSpawnSporeBurstFireballLarge,
    /// <summary>A Spore Spawn spore bursting: a fireball opening into a flaming ring.</summary>
    SporeSpawnSporeBurstFlamingRing,
    /// <summary>A Spore Spawn spore bursting: a starburst of flame.</summary>
    SporeSpawnSporeBurstStarburst,
    /// <summary>A Spore Spawn spore bursting: scattering embers.</summary>
    SporeSpawnSporeBurstEmbers,
    /// <summary>A Fune's or Namihe's fireball: flying left, trailing flame.</summary>
    FuneNamiheFireballLeftTrailing,
    /// <summary>A Fune's or Namihe's fireball: flying left, round.</summary>
    FuneNamiheFireballLeftRound,
    /// <summary>A Fune's or Namihe's fireball: flying left, dissolving.</summary>
    FuneNamiheFireballLeftDissolving,
    /// <summary>A Fune's or Namihe's fireball: flying right, trailing flame.</summary>
    FuneNamiheFireballRightTrailing,
    /// <summary>A Fune's or Namihe's fireball: flying right, round.</summary>
    FuneNamiheFireballRightRound,
    /// <summary>A Fune's or Namihe's fireball: flying right, dissolving.</summary>
    FuneNamiheFireballRightDissolving,
    /// <summary>A Magdollite's lava: thrown left.</summary>
    MagdolliteLavaLeft,
    /// <summary>A Magdollite's lava: thrown right.</summary>
    MagdolliteLavaRight,
    /// <summary>A dust explosion: four sparks gathering in from their widest spread.</summary>
    DustExplosionSparksGathering,
    /// <summary>A dust explosion: four sparks drawing in.</summary>
    DustExplosionSparksDrawingIn,
    /// <summary>A dust explosion: four sparks closing in.</summary>
    DustExplosionSparksClosingIn,
    /// <summary>A dust explosion: four sparks converging.</summary>
    DustExplosionSparksConverging,
    /// <summary>A dust explosion: four sparks nearly met.</summary>
    DustExplosionSparksNearing,
    /// <summary>A dust explosion: four sparks meeting at their closest.</summary>
    DustExplosionSparksMeeting,
    /// <summary>A dust explosion: four sparks set the other way round, gathering in from their widest spread.</summary>
    DustExplosionSparksTwistedGathering,
    /// <summary>A dust explosion: four sparks set the other way round, drawing in.</summary>
    DustExplosionSparksTwistedDrawingIn,
    /// <summary>A dust explosion: four sparks set the other way round, closing in.</summary>
    DustExplosionSparksTwistedClosingIn,
    /// <summary>A dust explosion: four sparks set the other way round, converging.</summary>
    DustExplosionSparksTwistedConverging,
    /// <summary>A dust explosion: four sparks set the other way round, nearly met.</summary>
    DustExplosionSparksTwistedNearing,
    /// <summary>A dust explosion: four sparks set the other way round, meeting at their closest.</summary>
    DustExplosionSparksTwistedMeeting,
    /// <summary>A dust explosion: a small spark.</summary>
    DustExplosionSpark,
    /// <summary>A dust explosion: a small fireball.</summary>
    DustExplosionFireballSmall,
    /// <summary>A dust explosion: a large fireball.</summary>
    DustExplosionFireballLarge,
    /// <summary>A dust explosion: a fireball opening into a flaming ring.</summary>
    DustExplosionFlamingRing,
    /// <summary>A dust explosion: a starburst of flame.</summary>
    DustExplosionStarburst,
    /// <summary>A dust explosion: scattering embers.</summary>
    DustExplosionEmbers,
    /// <summary>A dust explosion: a small green orb.</summary>
    DustExplosionGreenOrbSmall,
    /// <summary>A dust explosion: a green orb.</summary>
    DustExplosionGreenOrb,
    /// <summary>A dust explosion: a green ring around a dark green core.</summary>
    DustExplosionGreenRingCored,
    /// <summary>A dust explosion: a thick green ring around a dark green core.</summary>
    DustExplosionGreenRingThick,
    /// <summary>A dust explosion: a green ring.</summary>
    DustExplosionGreenRing,
    /// <summary>A dust explosion: a glint of light.</summary>
    DustExplosionFlareGlint,
    /// <summary>A dust explosion: a small glow.</summary>
    DustExplosionFlareGlow,
    /// <summary>A dust explosion: a ball of light.</summary>
    DustExplosionFlareBall,
    /// <summary>A dust explosion: a ring of light around a burning core.</summary>
    DustExplosionFlareRingCored,
    /// <summary>A dust explosion: a ring of light.</summary>
    DustExplosionFlareRing,
    /// <summary>A dust explosion: a fading ring of light.</summary>
    DustExplosionFlareRingFading,
    /// <summary>A dust explosion: a small puff of dust.</summary>
    DustExplosionPuff,
    /// <summary>A dust explosion: a cloud of dust.</summary>
    DustExplosionCloud,
    /// <summary>A dust explosion: a cloud of dust thinning.</summary>
    DustExplosionCloudThinning,
    /// <summary>A dust explosion: a thin cloud of dust.</summary>
    DustExplosionCloudThin,
    /// <summary>A dust explosion: a faint cloud of dust.</summary>
    DustExplosionCloudFaint,
    /// <summary>A dust explosion: a smaller puff of dust.</summary>
    DustExplosionPuffSmall,
    /// <summary>A dust explosion: a thin puff of dust.</summary>
    DustExplosionPuffThin,
    /// <summary>A dust explosion: a faint puff of dust.</summary>
    DustExplosionPuffFaint,
    /// <summary>A dust explosion: a low puff of smoke.</summary>
    DustExplosionSmokePuffLow,
    /// <summary>A dust explosion: a puff of smoke rising in specks.</summary>
    DustExplosionSmokePuffRising,
    /// <summary>A dust explosion: a puff of smoke scattering.</summary>
    DustExplosionSmokePuffScattering,
    /// <summary>A dust explosion: a fading puff of smoke.</summary>
    DustExplosionSmokePuffFading,
    /// <summary>A dust explosion: a cloud of fire.</summary>
    DustExplosionFireCloud,
    /// <summary>A dust explosion: a cloud of fire spreading.</summary>
    DustExplosionFireCloudSpreading,
    /// <summary>A dust explosion: a cloud of fire breaking up.</summary>
    DustExplosionFireCloudBreaking,
    /// <summary>A dust explosion: the embers of a cloud of fire.</summary>
    DustExplosionFireCloudEmbers,
    /// <summary>A dust explosion: a huge fireball with a ring inside it.</summary>
    DustExplosionFireballHuge,
    /// <summary>A dust explosion: a wide ring of fire.</summary>
    DustExplosionWideRing,
    /// <summary>A dust explosion: a wide, fading ring of fire.</summary>
    DustExplosionWideRingFading,
    /// <summary>A dust explosion: four glowing pebbles bursting from the blast, close together.</summary>
    DustExplosionPebblesBursting,
    /// <summary>A dust explosion: four glowing pebbles flying out from the blast.</summary>
    DustExplosionPebblesFlyingOut,
    /// <summary>A dust explosion: four glowing pebbles flying apart, the upper pair climbing.</summary>
    DustExplosionPebblesFlyingApart,
    /// <summary>A dust explosion: four glowing pebbles climbing as they spread.</summary>
    DustExplosionPebblesClimbing,
    /// <summary>A dust explosion: four glowing pebbles slowing as they near their highest.</summary>
    DustExplosionPebblesSlowing,
    /// <summary>A dust explosion: four glowing pebbles at their highest.</summary>
    DustExplosionPebblesPeaking,
    /// <summary>A dust explosion: four cooling pebbles turning over to fall.</summary>
    DustExplosionPebblesTurningOver,
    /// <summary>A dust explosion: four cooling pebbles starting down.</summary>
    DustExplosionPebblesStartingDown,
    /// <summary>A dust explosion: four cooling pebbles dropping, darker.</summary>
    DustExplosionPebblesDropping,
    /// <summary>A dust explosion: four cooling pebbles falling as they spread.</summary>
    DustExplosionPebblesFalling,
    /// <summary>A dust explosion: four pebbles cooled to ash, falling on.</summary>
    DustExplosionPebblesTurningAshen,
    /// <summary>A dust explosion: four ashen pebbles tumbling down.</summary>
    DustExplosionPebblesTumbling,
    /// <summary>A dust explosion: four ashen pebbles plunging.</summary>
    DustExplosionPebblesPlunging,
    /// <summary>A dust explosion: four ashen pebbles plummeting.</summary>
    DustExplosionPebblesPlummeting,
    /// <summary>A dust explosion: four ashen pebbles scattering wide as they fall.</summary>
    DustExplosionPebblesScattering,
    /// <summary>A dust explosion: four ashen pebbles falling away at their widest.</summary>
    DustExplosionPebblesFallingAway,
    /// <summary>A dust explosion: a small puff of dust up and to the left.</summary>
    DustExplosionPuffUpLeft,
    /// <summary>A dust explosion: a puff of dust and a smaller one.</summary>
    DustExplosionPuffPair,
    /// <summary>A dust explosion: three puffs of dust.</summary>
    DustExplosionPuffTrio,
    /// <summary>A dust explosion: dense clouds of dust.</summary>
    DustExplosionCloudsDense,
    /// <summary>A dust explosion: clouds of dust thinning.</summary>
    DustExplosionCloudsThinning,
    /// <summary>A dust explosion: thin clouds of dust.</summary>
    DustExplosionCloudsThin,
    /// <summary>A dust explosion: faint clouds of dust.</summary>
    DustExplosionCloudsFaint,
    /// <summary>A dust explosion: a faint cloud of dust to the right.</summary>
    DustExplosionCloudFaintRight,
    /// <summary>A dust explosion: a small bubble.</summary>
    DustExplosionBubbleSmall,
    /// <summary>A dust explosion: a cluster of bubbles.</summary>
    DustExplosionBubbles,
    /// <summary>A dust explosion: bubbles rising.</summary>
    DustExplosionBubblesRising,
    /// <summary>A dust explosion: bubbles spreading.</summary>
    DustExplosionBubblesSpreading,
    /// <summary>A dust explosion: scattered bubbles.</summary>
    DustExplosionBubblesScattered,
    /// <summary>A dust explosion: sparse bubbles.</summary>
    DustExplosionBubblesSparse,
    /// <summary>A dust explosion: a few bubbles.</summary>
    DustExplosionBubblesFew,
    /// <summary>A dust explosion: the last bubbles.</summary>
    DustExplosionBubblesLast,
    /// <summary>A dust explosion: one faint speck.</summary>
    DustExplosionSpeck,
    /// <summary>A downward gate: one block tall.</summary>
    DownwardGateOneBlockTall,
    /// <summary>A downward gate: two blocks tall.</summary>
    DownwardGateTwoBlocksTall,
    /// <summary>A downward gate: three blocks tall.</summary>
    DownwardGateThreeBlocksTall,
    /// <summary>A downward gate: four blocks tall.</summary>
    DownwardGateFourBlocksTall,
    /// <summary>A save station's electricity beginning its sweep down the station: its zigzag band's picture placed 32 pixels above its place.</summary>
    SaveStationElectricitySweepBeginning,
    /// <summary>A save station's electricity descending on its sweep: its zigzag band's picture placed 24 pixels above its place.</summary>
    SaveStationElectricitySweepDescending,
    /// <summary>A save station's electricity lowering on its sweep: its zigzag band's picture placed 16 pixels above its place.</summary>
    SaveStationElectricitySweepLowering,
    /// <summary>A save station's electricity nearing halfway down its sweep: its zigzag band's picture placed 8 pixels above its place.</summary>
    SaveStationElectricitySweepNearingHalfway,
    /// <summary>A save station's electricity passing halfway down its sweep: its zigzag band's picture level with its place.</summary>
    SaveStationElectricitySweepPassingHalfway,
    /// <summary>A save station's electricity past halfway down its sweep: its zigzag band's picture placed 8 pixels below its place.</summary>
    SaveStationElectricitySweepPastHalfway,
    /// <summary>A save station's electricity nearing the end of its sweep: its zigzag band's picture placed 16 pixels below its place.</summary>
    SaveStationElectricitySweepNearingEnd,
    /// <summary>A save station's electricity at the end of its sweep: its zigzag band's picture placed 24 pixels below its place.</summary>
    SaveStationElectricitySweepEnding,
    /// <summary>A Botwoon body segment: facing up and left, its stripes rippled 0 degrees round their cycle.</summary>
    BotwoonBodyUpLeftRipple0,
    /// <summary>A Botwoon body segment: facing up and left, its stripes rippled 90 degrees round their cycle.</summary>
    BotwoonBodyUpLeftRipple90,
    /// <summary>A Botwoon body segment: facing up and left, its stripes rippled 180 degrees round their cycle.</summary>
    BotwoonBodyUpLeftRipple180,
    /// <summary>A Botwoon body segment: facing up and left, its stripes rippled 270 degrees round their cycle.</summary>
    BotwoonBodyUpLeftRipple270,
    /// <summary>A Botwoon body segment: facing left, its stripes rippled 0 degrees round their cycle.</summary>
    BotwoonBodyLeftRipple0,
    /// <summary>A Botwoon body segment: facing left, its stripes rippled 90 degrees round their cycle.</summary>
    BotwoonBodyLeftRipple90,
    /// <summary>A Botwoon body segment: facing left, its stripes rippled 180 degrees round their cycle.</summary>
    BotwoonBodyLeftRipple180,
    /// <summary>A Botwoon body segment: facing left, its stripes rippled 270 degrees round their cycle.</summary>
    BotwoonBodyLeftRipple270,
    /// <summary>A Botwoon body segment: facing down and left, its stripes rippled 0 degrees round their cycle.</summary>
    BotwoonBodyDownLeftRipple0,
    /// <summary>A Botwoon body segment: facing down and left, its stripes rippled 90 degrees round their cycle.</summary>
    BotwoonBodyDownLeftRipple90,
    /// <summary>A Botwoon body segment: facing down and left, its stripes rippled 180 degrees round their cycle.</summary>
    BotwoonBodyDownLeftRipple180,
    /// <summary>A Botwoon body segment: facing down and left, its stripes rippled 270 degrees round their cycle.</summary>
    BotwoonBodyDownLeftRipple270,
    /// <summary>A Botwoon body segment: facing down, its stripes rippled 0 degrees round their cycle.</summary>
    BotwoonBodyDownRipple0,
    /// <summary>A Botwoon body segment: facing down, its stripes rippled 90 degrees round their cycle.</summary>
    BotwoonBodyDownRipple90,
    /// <summary>A Botwoon body segment: facing down, its stripes rippled 180 degrees round their cycle.</summary>
    BotwoonBodyDownRipple180,
    /// <summary>A Botwoon body segment: facing down, its stripes rippled 270 degrees round their cycle.</summary>
    BotwoonBodyDownRipple270,
    /// <summary>A Botwoon body segment: facing down and right, its stripes rippled 0 degrees round their cycle.</summary>
    BotwoonBodyDownRightRipple0,
    /// <summary>A Botwoon body segment: facing down and right, its stripes rippled 90 degrees round their cycle.</summary>
    BotwoonBodyDownRightRipple90,
    /// <summary>A Botwoon body segment: facing down and right, its stripes rippled 180 degrees round their cycle.</summary>
    BotwoonBodyDownRightRipple180,
    /// <summary>A Botwoon body segment: facing down and right, its stripes rippled 270 degrees round their cycle.</summary>
    BotwoonBodyDownRightRipple270,
    /// <summary>A Botwoon body segment: facing right, its stripes rippled 0 degrees round their cycle.</summary>
    BotwoonBodyRightRipple0,
    /// <summary>A Botwoon body segment: facing right, its stripes rippled 90 degrees round their cycle.</summary>
    BotwoonBodyRightRipple90,
    /// <summary>A Botwoon body segment: facing right, its stripes rippled 180 degrees round their cycle.</summary>
    BotwoonBodyRightRipple180,
    /// <summary>A Botwoon body segment: facing right, its stripes rippled 270 degrees round their cycle.</summary>
    BotwoonBodyRightRipple270,
    /// <summary>A Botwoon body segment: facing up and right, its stripes rippled 0 degrees round their cycle.</summary>
    BotwoonBodyUpRightRipple0,
    /// <summary>A Botwoon body segment: facing up and right, its stripes rippled 90 degrees round their cycle.</summary>
    BotwoonBodyUpRightRipple90,
    /// <summary>A Botwoon body segment: facing up and right, its stripes rippled 180 degrees round their cycle.</summary>
    BotwoonBodyUpRightRipple180,
    /// <summary>A Botwoon body segment: facing up and right, its stripes rippled 270 degrees round their cycle.</summary>
    BotwoonBodyUpRightRipple270,
    /// <summary>A Botwoon body segment: facing up, its stripes rippled 0 degrees round their cycle.</summary>
    BotwoonBodyUpRipple0,
    /// <summary>A Botwoon body segment: facing up, its stripes rippled 90 degrees round their cycle.</summary>
    BotwoonBodyUpRipple90,
    /// <summary>A Botwoon body segment: facing up, its stripes rippled 180 degrees round their cycle.</summary>
    BotwoonBodyUpRipple180,
    /// <summary>A Botwoon body segment: facing up, its stripes rippled 270 degrees round their cycle.</summary>
    BotwoonBodyUpRipple270,
    /// <summary>A Botwoon tail segment: pointing up.</summary>
    BotwoonTailUp,
    /// <summary>A Botwoon tail segment: pointing up and left.</summary>
    BotwoonTailUpLeft,
    /// <summary>A Botwoon tail segment: pointing left.</summary>
    BotwoonTailLeft,
    /// <summary>A Botwoon tail segment: pointing down and left.</summary>
    BotwoonTailDownLeft,
    /// <summary>A Botwoon tail segment: pointing down.</summary>
    BotwoonTailDown,
    /// <summary>A Botwoon tail segment: pointing down and right.</summary>
    BotwoonTailDownRight,
    /// <summary>A Botwoon tail segment: pointing right.</summary>
    BotwoonTailRight,
    /// <summary>A Botwoon tail segment: pointing up and right.</summary>
    BotwoonTailUpRight,
    /// <summary>Botwoon's spit: a dim green ball.</summary>
    BotwoonSpitDim,
    /// <summary>Botwoon's spit: a bright green ball.</summary>
    BotwoonSpitBright,
    /// <summary>Botwoon's spit: a green ring.</summary>
    BotwoonSpitRinged,
    /// <summary>Botwoon's spit: a bright green ring.</summary>
    BotwoonSpitRingedBright,
    /// <summary>Botwoon's spit: a green ball dissolving.</summary>
    BotwoonSpitDissolving,
    /// <summary>A Yapping Maw body segment: facing down.</summary>
    YappingMawFacingDown,
    /// <summary>A Yapping Maw body segment: facing up.</summary>
    YappingMawFacingUp,
    /// <summary>An enemy's death: a small fireball.</summary>
    EnemyDeathFireballSmall,
    /// <summary>An enemy's death: a large fireball.</summary>
    EnemyDeathFireballLarge,
    /// <summary>An enemy's death: a fireball opening into a flaming ring.</summary>
    EnemyDeathFlamingRing,
    /// <summary>An enemy's death: a huge fireball with a ring inside it.</summary>
    EnemyDeathFireballHuge,
    /// <summary>An enemy's death: a wide ring of fire.</summary>
    EnemyDeathWideRing,
    /// <summary>An enemy's death: a wide, fading ring of fire.</summary>
    EnemyDeathWideRingFading,
    /// <summary>An enemy's death: a small spark.</summary>
    EnemyDeathSpark,
    /// <summary>An enemy's death: a starburst of flame.</summary>
    EnemyDeathStarburst,
    /// <summary>An enemy's death: scattering embers.</summary>
    EnemyDeathEmbers,
    /// <summary>An enemy's energy drop: a small dark orb.</summary>
    EnemyPickupEnergySmallDark,
    /// <summary>An enemy's energy drop: a small ringed orb.</summary>
    EnemyPickupEnergySmallRinged,
    /// <summary>An enemy's energy drop: a small bright orb.</summary>
    EnemyPickupEnergySmallBright,
    /// <summary>An enemy's energy drop: specks of energy.</summary>
    EnemyPickupEnergyScattered,
    /// <summary>An enemy's energy drop: a big bright orb.</summary>
    EnemyPickupEnergyBigBright,
    /// <summary>An enemy's energy drop: a big ringed orb.</summary>
    EnemyPickupEnergyBigRinged,
    /// <summary>An enemy's energy drop: a big dark orb.</summary>
    EnemyPickupEnergyBigDark,
    /// <summary>An enemy's missile drop: a missile.</summary>
    EnemyPickupMissile,
    /// <summary>An enemy's missile drop: a missile flashing white.</summary>
    EnemyPickupMissileFlashing,
    /// <summary>An enemy's super missile drop: a super missile.</summary>
    EnemyPickupSuperMissile,
    /// <summary>An enemy's super missile drop: a super missile flashing white.</summary>
    EnemyPickupSuperMissileFlashing,
    /// <summary>An enemy's power bomb drop: a bright power bomb.</summary>
    EnemyPickupPowerBombBright,
    /// <summary>An enemy's power bomb drop: a power bomb fading.</summary>
    EnemyPickupPowerBombFading,
    /// <summary>An enemy's power bomb drop: a dim power bomb.</summary>
    EnemyPickupPowerBombDim,
    /// <summary>An enemy's power bomb drop: a power bomb flashing white.</summary>
    EnemyPickupPowerBombFlashing,
    /// <summary>An enemy's death: a fireball ringed by four smaller ones.</summary>
    EnemyDeathFireballCluster,
    /// <summary>An enemy's death: a starburst ringed by four fireballs.</summary>
    EnemyDeathStarburstCluster,
    /// <summary>An enemy's death: embers ringed by four fading fireballs.</summary>
    EnemyDeathEmbersCluster,
    /// <summary>An enemy's death: four fireballs amid embers.</summary>
    EnemyDeathFourFireballsAmidEmbers,
    /// <summary>An enemy's death: four fireballs amid ash.</summary>
    EnemyDeathFourFireballsAmidAsh,
    /// <summary>An enemy's death: four glowing embers flying apart.</summary>
    EnemyDeathFourEmbersFlyingApart,
    /// <summary>An enemy's death: four glowing embers spreading further apart.</summary>
    EnemyDeathFourEmbersSpreading,
    /// <summary>An enemy's death: four embers cooling as they spread.</summary>
    EnemyDeathFourEmbersCooling,
    /// <summary>An enemy's death: four embers dimming at their widest.</summary>
    EnemyDeathFourEmbersDimming,
    /// <summary>An enemy's death: four flecks of ash drifting apart.</summary>
    EnemyDeathFourAshDrifting,
    /// <summary>An enemy's death: four flecks of ash spreading apart.</summary>
    EnemyDeathFourAshSpreading,
    /// <summary>An enemy's death: four flecks of ash sinking as they spread.</summary>
    EnemyDeathFourAshSinking,
    /// <summary>An enemy's death: four flecks of ash falling.</summary>
    EnemyDeathFourAshFalling,
    /// <summary>An enemy's death: four flecks of ash settling lower.</summary>
    EnemyDeathFourAshSettling,
    /// <summary>An enemy's death: four flecks of ash dispersing at their lowest.</summary>
    EnemyDeathFourAshDispersing,
    /// <summary>A falling spark: an upright cross.</summary>
    FallingSparkUpright,
    /// <summary>A falling spark: a diagonal cross.</summary>
    FallingSparkDiagonal,
    /// <summary>A falling spark: a small upright cross.</summary>
    FallingSparkUprightSmall,
}
