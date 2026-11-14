namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The frames an enemy shows: every installed enemy frame, and the empty frame every enemy family shares. Each is named for
/// what it shows: its owner, the animation (or the part, facing or table key) that first shows it, and its place there,
/// which its summary spells out. Its installed name is its name in snake case (<see cref="EnemyFrameNameDefinitions"/>),
/// which gives its installed sheet key (<see cref="EnemyFrameSheetKeys"/>), so a frame's identity never reaches a file.
/// </summary>
public enum EnemyFrameId
{
    /// <summary>Boyon: idle loop, first frame.</summary>
    BoyonIdleLoopFirst = 1,
    /// <summary>Boyon: idle loop, second frame.</summary>
    BoyonIdleLoopSecond,
    /// <summary>Boyon: idle loop, third frame.</summary>
    BoyonIdleLoopThird,
    /// <summary>Boyon: bouncing loop, first frame.</summary>
    BoyonBouncingLoopFirst,
    /// <summary>Boyon: bouncing loop, second frame.</summary>
    BoyonBouncingLoopSecond,
    /// <summary>Boyon: bouncing loop, third frame.</summary>
    BoyonBouncingLoopThird,
    /// <summary>Boyon: bouncing loop, fourth frame.</summary>
    BoyonBouncingLoopFourth,
    /// <summary>Cacatac: upside up idle, first frame.</summary>
    CacatacUpsideUpIdleFirst,
    /// <summary>Cacatac: upside up idle, second frame.</summary>
    CacatacUpsideUpIdleSecond,
    /// <summary>Cacatac: upside up idle, third frame.</summary>
    CacatacUpsideUpIdleThird,
    /// <summary>Cacatac: upside up idle, fourth frame.</summary>
    CacatacUpsideUpIdleFourth,
    /// <summary>Cacatac: upside up idle, fifth frame.</summary>
    CacatacUpsideUpIdleFifth,
    /// <summary>Cacatac: upside up idle, sixth frame.</summary>
    CacatacUpsideUpIdleSixth,
    /// <summary>Cacatac: upside up idle, seventh frame.</summary>
    CacatacUpsideUpIdleSeventh,
    /// <summary>Cacatac: upside up idle, eighth frame.</summary>
    CacatacUpsideUpIdleEighth,
    /// <summary>Cacatac: upside up attack, second frame.</summary>
    CacatacUpsideUpAttackSecond,
    /// <summary>Cacatac: upside up attack, third frame.</summary>
    CacatacUpsideUpAttackThird,
    /// <summary>Cacatac: upside down idle loop, first frame.</summary>
    CacatacUpsideDownIdleLoopFirst,
    /// <summary>Cacatac: upside down idle loop, second frame.</summary>
    CacatacUpsideDownIdleLoopSecond,
    /// <summary>Cacatac: upside down idle loop, third frame.</summary>
    CacatacUpsideDownIdleLoopThird,
    /// <summary>Cacatac: upside down idle loop, fourth frame.</summary>
    CacatacUpsideDownIdleLoopFourth,
    /// <summary>Cacatac: upside down idle loop, fifth frame.</summary>
    CacatacUpsideDownIdleLoopFifth,
    /// <summary>Cacatac: upside down idle loop, sixth frame.</summary>
    CacatacUpsideDownIdleLoopSixth,
    /// <summary>Cacatac: upside down idle loop, seventh frame.</summary>
    CacatacUpsideDownIdleLoopSeventh,
    /// <summary>Cacatac: upside down idle loop, eighth frame.</summary>
    CacatacUpsideDownIdleLoopEighth,
    /// <summary>Cacatac: upside down attack, second frame.</summary>
    CacatacUpsideDownAttackSecond,
    /// <summary>Cacatac: upside down attack, third frame.</summary>
    CacatacUpsideDownAttackThird,
    /// <summary>The boulder: left, first frame.</summary>
    BoulderLeftFirst,
    /// <summary>The boulder: left, second frame.</summary>
    BoulderLeftSecond,
    /// <summary>The boulder: left, third frame.</summary>
    BoulderLeftThird,
    /// <summary>The boulder: left, fourth frame.</summary>
    BoulderLeftFourth,
    /// <summary>The boulder: left, fifth frame.</summary>
    BoulderLeftFifth,
    /// <summary>The boulder: left, sixth frame.</summary>
    BoulderLeftSixth,
    /// <summary>The boulder: left, seventh frame.</summary>
    BoulderLeftSeventh,
    /// <summary>The boulder: left, eighth frame.</summary>
    BoulderLeftEighth,
    /// <summary>Atomic: up right, first frame.</summary>
    AtomicUpRightFirst,
    /// <summary>Atomic: up right, second frame.</summary>
    AtomicUpRightSecond,
    /// <summary>Atomic: up right, third frame.</summary>
    AtomicUpRightThird,
    /// <summary>Atomic: up right, fourth frame.</summary>
    AtomicUpRightFourth,
    /// <summary>Atomic: up right, fifth frame.</summary>
    AtomicUpRightFifth,
    /// <summary>Atomic: up right, sixth frame.</summary>
    AtomicUpRightSixth,
    /// <summary>Atomic: up left, first frame.</summary>
    AtomicUpLeftFirst,
    /// <summary>Atomic: up left, second frame.</summary>
    AtomicUpLeftSecond,
    /// <summary>Atomic: up left, third frame.</summary>
    AtomicUpLeftThird,
    /// <summary>Atomic: up left, fourth frame.</summary>
    AtomicUpLeftFourth,
    /// <summary>Atomic: up left, fifth frame.</summary>
    AtomicUpLeftFifth,
    /// <summary>Atomic: up left, sixth frame.</summary>
    AtomicUpLeftSixth,
    /// <summary>Skultera: swimming left loop, first frame.</summary>
    SkulteraSwimmingLeftLoopFirst,
    /// <summary>Skultera: swimming left loop, second frame.</summary>
    SkulteraSwimmingLeftLoopSecond,
    /// <summary>Skultera: swimming left loop, third frame.</summary>
    SkulteraSwimmingLeftLoopThird,
    /// <summary>Skultera: turning right, first frame.</summary>
    SkulteraTurningRightFirst,
    /// <summary>Skultera: turning right, second frame.</summary>
    SkulteraTurningRightSecond,
    /// <summary>Skultera: turning right, third frame.</summary>
    SkulteraTurningRightThird,
    /// <summary>Skultera: turning right, fourth frame.</summary>
    SkulteraTurningRightFourth,
    /// <summary>Skultera: turning right, fifth frame.</summary>
    SkulteraTurningRightFifth,
    /// <summary>Skultera: turning right, sixth frame.</summary>
    SkulteraTurningRightSixth,
    /// <summary>Skultera: turning right, seventh frame.</summary>
    SkulteraTurningRightSeventh,
    /// <summary>Skultera: turning right, eighth frame.</summary>
    SkulteraTurningRightEighth,
    /// <summary>Skultera: swimming right loop, first frame.</summary>
    SkulteraSwimmingRightLoopFirst,
    /// <summary>Skultera: swimming right loop, second frame.</summary>
    SkulteraSwimmingRightLoopSecond,
    /// <summary>Skultera: swimming right loop, third frame.</summary>
    SkulteraSwimmingRightLoopThird,
    /// <summary>Skultera: turning left, first frame.</summary>
    SkulteraTurningLeftFirst,
    /// <summary>Skultera: turning left, second frame.</summary>
    SkulteraTurningLeftSecond,
    /// <summary>Skultera: turning left, third frame.</summary>
    SkulteraTurningLeftThird,
    /// <summary>Skultera: turning left, fourth frame.</summary>
    SkulteraTurningLeftFourth,
    /// <summary>Skultera: turning left, fifth frame.</summary>
    SkulteraTurningLeftFifth,
    /// <summary>Skultera: turning left, sixth frame.</summary>
    SkulteraTurningLeftSixth,
    /// <summary>Skultera: turning left, seventh frame.</summary>
    SkulteraTurningLeftSeventh,
    /// <summary>Skultera: turning left, eighth frame.</summary>
    SkulteraTurningLeftEighth,
    /// <summary>Waver: steady left.</summary>
    WaverSteadyLeft,
    /// <summary>Waver: steady right.</summary>
    WaverSteadyRight,
    /// <summary>Waver: spinning facing left, first frame.</summary>
    WaverSpinningFacingLeftFirst,
    /// <summary>Waver: spinning facing left, second frame.</summary>
    WaverSpinningFacingLeftSecond,
    /// <summary>Waver: spinning facing left, third frame.</summary>
    WaverSpinningFacingLeftThird,
    /// <summary>Waver: spinning facing left, fourth frame.</summary>
    WaverSpinningFacingLeftFourth,
    /// <summary>Waver: spinning facing right, first frame.</summary>
    WaverSpinningFacingRightFirst,
    /// <summary>Waver: spinning facing right, second frame.</summary>
    WaverSpinningFacingRightSecond,
    /// <summary>Waver: spinning facing right, third frame.</summary>
    WaverSpinningFacingRightThird,
    /// <summary>Waver: spinning facing right, fourth frame.</summary>
    WaverSpinningFacingRightFourth,
    /// <summary>Metaree: idling, first frame.</summary>
    MetareeIdlingFirst,
    /// <summary>Metaree: idling, second frame.</summary>
    MetareeIdlingSecond,
    /// <summary>Metaree: idling, third frame.</summary>
    MetareeIdlingThird,
    /// <summary>Metaree: idling, fourth frame.</summary>
    MetareeIdlingFourth,
    /// <summary>Metaree: preparing attack, second frame.</summary>
    MetareePreparingAttackSecond,
    /// <summary>Skree: idling, first frame.</summary>
    SkreeIdlingFirst,
    /// <summary>Skree: idling, second frame.</summary>
    SkreeIdlingSecond,
    /// <summary>Skree: idling, third frame.</summary>
    SkreeIdlingThird,
    /// <summary>Skree: idling, fourth frame.</summary>
    SkreeIdlingFourth,
    /// <summary>Skree: preparing attack, second frame.</summary>
    SkreePreparingAttackSecond,
    /// <summary>Zoa: facing left shooting, first frame.</summary>
    ZoaFacingLeftShootingFirst,
    /// <summary>Zoa: facing left shooting, second frame.</summary>
    ZoaFacingLeftShootingSecond,
    /// <summary>Zoa: facing left shooting, third frame.</summary>
    ZoaFacingLeftShootingThird,
    /// <summary>Zoa: facing left rising, first frame.</summary>
    ZoaFacingLeftRisingFirst,
    /// <summary>Zoa: facing left rising, second frame.</summary>
    ZoaFacingLeftRisingSecond,
    /// <summary>Zoa: facing left rising, third frame.</summary>
    ZoaFacingLeftRisingThird,
    /// <summary>Zoa: facing right shooting, first frame.</summary>
    ZoaFacingRightShootingFirst,
    /// <summary>Zoa: facing right shooting, second frame.</summary>
    ZoaFacingRightShootingSecond,
    /// <summary>Zoa: facing right shooting, third frame.</summary>
    ZoaFacingRightShootingThird,
    /// <summary>Zoa: facing right rising, first frame.</summary>
    ZoaFacingRightRisingFirst,
    /// <summary>Zoa: facing right rising, second frame.</summary>
    ZoaFacingRightRisingSecond,
    /// <summary>Zoa: facing right rising, third frame.</summary>
    ZoaFacingRightRisingThird,
    /// <summary>Zeb: rising left, first frame.</summary>
    ZebRisingLeftFirst,
    /// <summary>Zeb: rising left, second frame.</summary>
    ZebRisingLeftSecond,
    /// <summary>Zeb: rising left, third frame.</summary>
    ZebRisingLeftThird,
    /// <summary>Zeb: rising left, fourth frame.</summary>
    ZebRisingLeftFourth,
    /// <summary>Zeb: rising left, fifth frame.</summary>
    ZebRisingLeftFifth,
    /// <summary>Zeb: rising right, first frame.</summary>
    ZebRisingRightFirst,
    /// <summary>Zeb: rising right, second frame.</summary>
    ZebRisingRightSecond,
    /// <summary>Zeb: rising right, third frame.</summary>
    ZebRisingRightThird,
    /// <summary>Zeb: rising right, fourth frame.</summary>
    ZebRisingRightFourth,
    /// <summary>Zeb: rising right, fifth frame.</summary>
    ZebRisingRightFifth,
    /// <summary>Zebbo: shooting left, first frame.</summary>
    ZebboShootingLeftFirst,
    /// <summary>Zebbo: shooting left, second frame.</summary>
    ZebboShootingLeftSecond,
    /// <summary>Zebbo: shooting left, third frame.</summary>
    ZebboShootingLeftThird,
    /// <summary>Zebbo: rising left, first frame.</summary>
    ZebboRisingLeftFirst,
    /// <summary>Zebbo: rising left, second frame.</summary>
    ZebboRisingLeftSecond,
    /// <summary>Zebbo: rising left, third frame.</summary>
    ZebboRisingLeftThird,
    /// <summary>Zebbo: shooting right, first frame.</summary>
    ZebboShootingRightFirst,
    /// <summary>Zebbo: shooting right, second frame.</summary>
    ZebboShootingRightSecond,
    /// <summary>Zebbo: shooting right, third frame.</summary>
    ZebboShootingRightThird,
    /// <summary>Zebbo: rising right, first frame.</summary>
    ZebboRisingRightFirst,
    /// <summary>Zebbo: rising right, second frame.</summary>
    ZebboRisingRightSecond,
    /// <summary>Zebbo: rising right, third frame.</summary>
    ZebboRisingRightThird,
    /// <summary>Gamet: rising left, first frame.</summary>
    GametRisingLeftFirst,
    /// <summary>Gamet: rising left, second frame.</summary>
    GametRisingLeftSecond,
    /// <summary>Gamet: rising left, third frame.</summary>
    GametRisingLeftThird,
    /// <summary>Gamet: rising left, fourth frame.</summary>
    GametRisingLeftFourth,
    /// <summary>Gamet: rising left, fifth frame.</summary>
    GametRisingLeftFifth,
    /// <summary>Gamet: rising right, first frame.</summary>
    GametRisingRightFirst,
    /// <summary>Gamet: rising right, second frame.</summary>
    GametRisingRightSecond,
    /// <summary>Gamet: rising right, third frame.</summary>
    GametRisingRightThird,
    /// <summary>Gamet: rising right, fourth frame.</summary>
    GametRisingRightFourth,
    /// <summary>Gamet: rising right, fifth frame.</summary>
    GametRisingRightFifth,
    /// <summary>Geega: flying left, first frame.</summary>
    GeegaFlyingLeftFirst,
    /// <summary>Geega: flying left, second frame.</summary>
    GeegaFlyingLeftSecond,
    /// <summary>Geega: flying left, third frame.</summary>
    GeegaFlyingLeftThird,
    /// <summary>Geega: arcing left, first frame.</summary>
    GeegaArcingLeftFirst,
    /// <summary>Geega: arcing left, second frame.</summary>
    GeegaArcingLeftSecond,
    /// <summary>Geega: arcing left, third frame.</summary>
    GeegaArcingLeftThird,
    /// <summary>Geega: flying right, first frame.</summary>
    GeegaFlyingRightFirst,
    /// <summary>Geega: flying right, second frame.</summary>
    GeegaFlyingRightSecond,
    /// <summary>Geega: flying right, third frame.</summary>
    GeegaFlyingRightThird,
    /// <summary>Geega: arcing right, first frame.</summary>
    GeegaArcingRightFirst,
    /// <summary>Geega: arcing right, second frame.</summary>
    GeegaArcingRightSecond,
    /// <summary>Geega: arcing right, third frame.</summary>
    GeegaArcingRightThird,
    /// <summary>Fake Kraid: step forwards facing left, first frame.</summary>
    FakeKraidStepForwardsFacingLeftFirst,
    /// <summary>Fake Kraid: step forwards facing left, second frame.</summary>
    FakeKraidStepForwardsFacingLeftSecond,
    /// <summary>Fake Kraid: step forwards facing left, third frame.</summary>
    FakeKraidStepForwardsFacingLeftThird,
    /// <summary>Fake Kraid: step forwards facing left, fourth frame.</summary>
    FakeKraidStepForwardsFacingLeftFourth,
    /// <summary>Fake Kraid: fire spit facing left, first frame.</summary>
    FakeKraidFireSpitFacingLeftFirst,
    /// <summary>Fake Kraid: fire spit facing left, second frame.</summary>
    FakeKraidFireSpitFacingLeftSecond,
    /// <summary>Fake Kraid: fire spit facing left, third frame.</summary>
    FakeKraidFireSpitFacingLeftThird,
    /// <summary>Fake Kraid: step forwards facing right, first frame.</summary>
    FakeKraidStepForwardsFacingRightFirst,
    /// <summary>Fake Kraid: step forwards facing right, second frame.</summary>
    FakeKraidStepForwardsFacingRightSecond,
    /// <summary>Fake Kraid: step forwards facing right, third frame.</summary>
    FakeKraidStepForwardsFacingRightThird,
    /// <summary>Fake Kraid: step forwards facing right, fourth frame.</summary>
    FakeKraidStepForwardsFacingRightFourth,
    /// <summary>Fake Kraid: fire spit facing right, first frame.</summary>
    FakeKraidFireSpitFacingRightFirst,
    /// <summary>Fake Kraid: fire spit facing right, second frame.</summary>
    FakeKraidFireSpitFacingRightSecond,
    /// <summary>Fake Kraid: fire spit facing right, third frame.</summary>
    FakeKraidFireSpitFacingRightThird,
    /// <summary>Kraid: nail loop, first frame.</summary>
    KraidNailLoopFirst,
    /// <summary>Kraid: nail loop, second frame.</summary>
    KraidNailLoopSecond,
    /// <summary>Kraid: nail loop, third frame.</summary>
    KraidNailLoopThird,
    /// <summary>Kraid: nail loop, fourth frame.</summary>
    KraidNailLoopFourth,
    /// <summary>Kraid: nail loop, fifth frame.</summary>
    KraidNailLoopFifth,
    /// <summary>Kraid: nail loop, sixth frame.</summary>
    KraidNailLoopSixth,
    /// <summary>Kraid: nail loop, seventh frame.</summary>
    KraidNailLoopSeventh,
    /// <summary>Kraid: nail loop, eighth frame.</summary>
    KraidNailLoopEighth,
    /// <summary>Owtch: moving left loop, first frame.</summary>
    OwtchMovingLeftLoopFirst,
    /// <summary>Owtch: moving left loop, second frame.</summary>
    OwtchMovingLeftLoopSecond,
    /// <summary>Owtch: moving left loop, third frame.</summary>
    OwtchMovingLeftLoopThird,
    /// <summary>Gripper: moving left, first frame.</summary>
    GRipperMovingLeftFirst,
    /// <summary>Gripper: moving left, second frame.</summary>
    GRipperMovingLeftSecond,
    /// <summary>Gripper: moving left, third frame.</summary>
    GRipperMovingLeftThird,
    /// <summary>Gripper: moving right, first frame.</summary>
    GRipperMovingRightFirst,
    /// <summary>Gripper: moving right, second frame.</summary>
    GRipperMovingRightSecond,
    /// <summary>Gripper: moving right, third frame.</summary>
    GRipperMovingRightThird,
    /// <summary>Ripper: shared frozen left.</summary>
    RipperSharedFrozenLeft,
    /// <summary>Ripper: shared frozen right.</summary>
    RipperSharedFrozenRight,
    /// <summary>Ripper: moving left, first frame.</summary>
    RipperMovingLeftFirst,
    /// <summary>Ripper: moving left, second frame.</summary>
    RipperMovingLeftSecond,
    /// <summary>Ripper: moving left, third frame.</summary>
    RipperMovingLeftThird,
    /// <summary>Ripper: moving right, first frame.</summary>
    RipperMovingRightFirst,
    /// <summary>Ripper: moving right, second frame.</summary>
    RipperMovingRightSecond,
    /// <summary>Ripper: moving right, third frame.</summary>
    RipperMovingRightThird,
    /// <summary>Fireflea: loop, first frame.</summary>
    FirefleaLoopFirst,
    /// <summary>Fireflea: loop, second frame.</summary>
    FirefleaLoopSecond,
    /// <summary>Fireflea: loop, third frame.</summary>
    FirefleaLoopThird,
    /// <summary>Fireflea: loop, fourth frame.</summary>
    FirefleaLoopFourth,
    /// <summary>Fireflea: loop, fifth frame.</summary>
    FirefleaLoopFifth,
    /// <summary>Fireflea: loop, sixth frame.</summary>
    FirefleaLoopSixth,
    /// <summary>Fireflea: loop, seventh frame.</summary>
    FirefleaLoopSeventh,
    /// <summary>Fireflea: loop, eighth frame.</summary>
    FirefleaLoopEighth,
    /// <summary>Fireflea: loop, ninth frame.</summary>
    FirefleaLoopNinth,
    /// <summary>Fireflea: loop, tenth frame.</summary>
    FirefleaLoopTenth,
    /// <summary>Fireflea: loop, eleventh frame.</summary>
    FirefleaLoopEleventh,
    /// <summary>Fireflea: loop, twelfth frame.</summary>
    FirefleaLoopTwelfth,
    /// <summary>Fireflea: loop, thirteenth frame.</summary>
    FirefleaLoopThirteenth,
    /// <summary>Fireflea: loop, fourteenth frame.</summary>
    FirefleaLoopFourteenth,
    /// <summary>Fireflea: loop, fifteenth frame.</summary>
    FirefleaLoopFifteenth,
    /// <summary>Fireflea: loop, sixteenth frame.</summary>
    FirefleaLoopSixteenth,
    /// <summary>Fireflea: loop, seventeenth frame.</summary>
    FirefleaLoopSeventeenth,
    /// <summary>Fireflea: loop, eighteenth frame.</summary>
    FirefleaLoopEighteenth,
    /// <summary>Fireflea: loop, nineteenth frame.</summary>
    FirefleaLoopNineteenth,
    /// <summary>Fireflea: loop, twentieth frame.</summary>
    FirefleaLoopTwentieth,
    /// <summary>Fireflea: loop, twenty-first frame.</summary>
    FirefleaLoopTwentyFirst,
    /// <summary>Magdollite: left idle, first frame.</summary>
    MagdolliteLeftIdleFirst,
    /// <summary>Magdollite: left idle, second frame.</summary>
    MagdolliteLeftIdleSecond,
    /// <summary>Magdollite: left idle, third frame.</summary>
    MagdolliteLeftIdleThird,
    /// <summary>Magdollite: left throw, first frame.</summary>
    MagdolliteLeftThrowFirst,
    /// <summary>Magdollite: left throw, second frame.</summary>
    MagdolliteLeftThrowSecond,
    /// <summary>Magdollite: left throw, third frame.</summary>
    MagdolliteLeftThrowThird,
    /// <summary>Magdollite: left throw, fourth frame.</summary>
    MagdolliteLeftThrowFourth,
    /// <summary>Magdollite: pillar cap.</summary>
    MagdollitePillarCap,
    /// <summary>Magdollite: left raise pillar.</summary>
    MagdolliteLeftRaisePillar,
    /// <summary>Magdollite: left submerge, second frame.</summary>
    MagdolliteLeftSubmergeSecond,
    /// <summary>Magdollite: left submerge, third frame.</summary>
    MagdolliteLeftSubmergeThird,
    /// <summary>Magdollite: right idle, first frame.</summary>
    MagdolliteRightIdleFirst,
    /// <summary>Magdollite: right idle, second frame.</summary>
    MagdolliteRightIdleSecond,
    /// <summary>Magdollite: right idle, third frame.</summary>
    MagdolliteRightIdleThird,
    /// <summary>Magdollite: right throw, first frame.</summary>
    MagdolliteRightThrowFirst,
    /// <summary>Magdollite: right throw, second frame.</summary>
    MagdolliteRightThrowSecond,
    /// <summary>Magdollite: right throw, third frame.</summary>
    MagdolliteRightThrowThird,
    /// <summary>Magdollite: right throw, fourth frame.</summary>
    MagdolliteRightThrowFourth,
    /// <summary>Magdollite: right raise pillar.</summary>
    MagdolliteRightRaisePillar,
    /// <summary>Magdollite: right submerge, second frame.</summary>
    MagdolliteRightSubmergeSecond,
    /// <summary>Magdollite: right submerge, third frame.</summary>
    MagdolliteRightSubmergeThird,
    /// <summary>Magdollite: pillar phase, first frame.</summary>
    MagdollitePillarPhaseFirst,
    /// <summary>Magdollite: pillar phase, second frame.</summary>
    MagdollitePillarPhaseSecond,
    /// <summary>Magdollite: pillar phase, third frame.</summary>
    MagdollitePillarPhaseThird,
    /// <summary>Magdollite: pillar phase, fourth frame.</summary>
    MagdollitePillarPhaseFourth,
    /// <summary>Magdollite: pillar phase, fifth frame.</summary>
    MagdollitePillarPhaseFifth,
    /// <summary>Magdollite: pillar phase, sixth frame.</summary>
    MagdollitePillarPhaseSixth,
    /// <summary>Magdollite: pillar phase, seventh frame.</summary>
    MagdollitePillarPhaseSeventh,
    /// <summary>Magdollite: pillar phase, eighth frame.</summary>
    MagdollitePillarPhaseEighth,
    /// <summary>The Ceres door: rotating overlay.</summary>
    CeresDoorRotatingOverlay,
    /// <summary>The Ceres door: left hold.</summary>
    CeresDoorLeftHold,
    /// <summary>The Ceres door: close facing left, fourth frame.</summary>
    CeresDoorCloseFacingLeftFourth,
    /// <summary>The Ceres door: close facing left, third frame.</summary>
    CeresDoorCloseFacingLeftThird,
    /// <summary>The Ceres door: close facing left, second frame.</summary>
    CeresDoorCloseFacingLeftSecond,
    /// <summary>The Ceres door: close facing left, first frame.</summary>
    CeresDoorCloseFacingLeftFirst,
    /// <summary>The Ceres door: right hold.</summary>
    CeresDoorRightHold,
    /// <summary>The Ceres door: Ridley room facing right, fourth frame.</summary>
    CeresDoorRidleyRoomFacingRightFourth,
    /// <summary>The Ceres door: Ridley room facing right, third frame.</summary>
    CeresDoorRidleyRoomFacingRightThird,
    /// <summary>The Ceres door: Ridley room facing right, second frame.</summary>
    CeresDoorRidleyRoomFacingRightSecond,
    /// <summary>The Ceres door: Ridley room facing right, first frame.</summary>
    CeresDoorRidleyRoomFacingRightFirst,
    /// <summary>The Ceres door: initial.</summary>
    CeresDoorInitial,
    /// <summary>The Ceres door: turning floor left wall.</summary>
    CeresDoorTurningFloorLeftWall,
    /// <summary>The Ceres door: turning floor right wall.</summary>
    CeresDoorTurningFloorRightWall,
    /// <summary>The Ceres door: Ridley private overlay.</summary>
    CeresDoorRidleyPrivateOverlay,
    /// <summary>The Ceres baby Metroid: horizontal.</summary>
    CeresBabyHorizontal,
    /// <summary>The Ceres baby Metroid: round.</summary>
    CeresBabyRound,
    /// <summary>The Ceres baby Metroid: vertical.</summary>
    CeresBabyVertical,
    /// <summary>Rio: idle, first frame.</summary>
    RioIdleFirst,
    /// <summary>Rio: idle, second frame.</summary>
    RioIdleSecond,
    /// <summary>Rio: post swoop idle, third frame.</summary>
    RioPostSwoopIdleThird,
    /// <summary>Rio: post swoop idle, fourth frame.</summary>
    RioPostSwoopIdleFourth,
    /// <summary>Rio: swooping part one, third frame.</summary>
    RioSwoopingPartOneThird,
    /// <summary>Rio: swooping part one, fourth frame.</summary>
    RioSwoopingPartOneFourth,
    /// <summary>Rio: swooping part one, fifth frame.</summary>
    RioSwoopingPartOneFifth,
    /// <summary>Rio: swooping part two, first frame.</summary>
    RioSwoopingPartTwoFirst,
    /// <summary>The Lower Norfair Rio: idle loop, first frame.</summary>
    LowerNorfairRioIdleLoopFirst,
    /// <summary>The Lower Norfair Rio: idle loop, second frame.</summary>
    LowerNorfairRioIdleLoopSecond,
    /// <summary>The Lower Norfair Rio: idle loop, third frame.</summary>
    LowerNorfairRioIdleLoopThird,
    /// <summary>The Lower Norfair Rio: prepare to swoop, second frame.</summary>
    LowerNorfairRioPrepareToSwoopSecond,
    /// <summary>The Lower Norfair Rio: prepare to swoop, third frame.</summary>
    LowerNorfairRioPrepareToSwoopThird,
    /// <summary>The Lower Norfair Rio: prepare to swoop, fourth frame.</summary>
    LowerNorfairRioPrepareToSwoopFourth,
    /// <summary>The Lower Norfair Rio: prepare to swoop, fifth frame.</summary>
    LowerNorfairRioPrepareToSwoopFifth,
    /// <summary>The Lower Norfair Rio: prepare to swoop, sixth frame.</summary>
    LowerNorfairRioPrepareToSwoopSixth,
    /// <summary>The Lower Norfair Rio: prepare to swoop, seventh frame.</summary>
    LowerNorfairRioPrepareToSwoopSeventh,
    /// <summary>The Lower Norfair Rio: prepare to swoop, eighth frame.</summary>
    LowerNorfairRioPrepareToSwoopEighth,
    /// <summary>The Lower Norfair Rio: prepare to swoop, ninth frame.</summary>
    LowerNorfairRioPrepareToSwoopNinth,
    /// <summary>The Lower Norfair Rio: ascending part one, third frame.</summary>
    LowerNorfairRioAscendingPartOneThird,
    /// <summary>The Lower Norfair Rio: ascending part two loop, second frame.</summary>
    LowerNorfairRioAscendingPartTwoLoopSecond,
    /// <summary>The Lower Norfair Rio: ascending part two loop, third frame.</summary>
    LowerNorfairRioAscendingPartTwoLoopThird,
    /// <summary>The Lower Norfair Rio: ascending part two loop, first frame.</summary>
    LowerNorfairRioAscendingPartTwoLoopFirst,
    /// <summary>The Lower Norfair Rio: flames, first frame.</summary>
    LowerNorfairRioFlamesFirst,
    /// <summary>The Lower Norfair Rio: flames, second frame.</summary>
    LowerNorfairRioFlamesSecond,
    /// <summary>The Lower Norfair Rio: flames, third frame.</summary>
    LowerNorfairRioFlamesThird,
    /// <summary>The Norfair Rio: idle, second frame.</summary>
    NorfairRioIdleSecond,
    /// <summary>The Norfair Rio: idle, first frame.</summary>
    NorfairRioIdleFirst,
    /// <summary>The Norfair Rio: idle, third frame.</summary>
    NorfairRioIdleThird,
    /// <summary>The Norfair Rio: flames ascending, first frame.</summary>
    NorfairRioFlamesAscendingFirst,
    /// <summary>The Norfair Rio: flames ascending, second frame.</summary>
    NorfairRioFlamesAscendingSecond,
    /// <summary>The Norfair Rio: flames ascending, third frame.</summary>
    NorfairRioFlamesAscendingThird,
    /// <summary>The Norfair Rio: descending, first frame.</summary>
    NorfairRioDescendingFirst,
    /// <summary>The Norfair Rio: descending, second frame.</summary>
    NorfairRioDescendingSecond,
    /// <summary>The Norfair Rio: descending, third frame.</summary>
    NorfairRioDescendingThird,
    /// <summary>The Norfair Rio: start descending, second frame.</summary>
    NorfairRioStartDescendingSecond,
    /// <summary>The Norfair Rio: start descending, third frame.</summary>
    NorfairRioStartDescendingThird,
    /// <summary>The Norfair Rio: start descending, fourth frame.</summary>
    NorfairRioStartDescendingFourth,
    /// <summary>The Norfair Rio: start descending, fifth frame.</summary>
    NorfairRioStartDescendingFifth,
    /// <summary>The Norfair Rio: start descending, sixth frame.</summary>
    NorfairRioStartDescendingSixth,
    /// <summary>The Norfair Rio: ascending, first frame.</summary>
    NorfairRioAscendingFirst,
    /// <summary>The Norfair Rio: ascending, second frame.</summary>
    NorfairRioAscendingSecond,
    /// <summary>The Norfair Rio: ascending, third frame.</summary>
    NorfairRioAscendingThird,
    /// <summary>The Norfair Rio: flames descending, first frame.</summary>
    NorfairRioFlamesDescendingFirst,
    /// <summary>The Norfair Rio: flames descending, second frame.</summary>
    NorfairRioFlamesDescendingSecond,
    /// <summary>The Norfair Rio: flames descending, third frame.</summary>
    NorfairRioFlamesDescendingThird,
    /// <summary>Puyo: grounded fast, first frame.</summary>
    PuyoGroundedFastFirst,
    /// <summary>Puyo: grounded fast, second frame.</summary>
    PuyoGroundedFastSecond,
    /// <summary>Puyo: grounded fast, third frame.</summary>
    PuyoGroundedFastThird,
    /// <summary>Puyo: airborne, first frame.</summary>
    PuyoAirborneFirst,
    /// <summary>Puyo: airborne, second frame.</summary>
    PuyoAirborneSecond,
    /// <summary>Puyo: airborne, third frame.</summary>
    PuyoAirborneThird,
    /// <summary>Puyo: airborne, fourth frame.</summary>
    PuyoAirborneFourth,
    /// <summary>Puyo: airborne, fifth frame.</summary>
    PuyoAirborneFifth,
    /// <summary>Bull: normal, first frame.</summary>
    BullNormalFirst,
    /// <summary>Bull: normal, second frame.</summary>
    BullNormalSecond,
    /// <summary>Bull: normal, third frame.</summary>
    BullNormalThird,
    /// <summary>Alcoon: walking left loop, first frame.</summary>
    AlcoonWalkingLeftLoopFirst,
    /// <summary>Alcoon: walking left loop, second frame.</summary>
    AlcoonWalkingLeftLoopSecond,
    /// <summary>Alcoon: walking left loop, third frame.</summary>
    AlcoonWalkingLeftLoopThird,
    /// <summary>Alcoon: walking left loop, fourth frame.</summary>
    AlcoonWalkingLeftLoopFourth,
    /// <summary>Alcoon: fire left, first frame.</summary>
    AlcoonFireLeftFirst,
    /// <summary>Alcoon: fire left, second frame.</summary>
    AlcoonFireLeftSecond,
    /// <summary>Alcoon: fire left, third frame.</summary>
    AlcoonFireLeftThird,
    /// <summary>Alcoon: fire left, fourth frame.</summary>
    AlcoonFireLeftFourth,
    /// <summary>Alcoon: left air up.</summary>
    AlcoonLeftAirUp,
    /// <summary>Alcoon: walking right loop, first frame.</summary>
    AlcoonWalkingRightLoopFirst,
    /// <summary>Alcoon: walking right loop, second frame.</summary>
    AlcoonWalkingRightLoopSecond,
    /// <summary>Alcoon: walking right loop, third frame.</summary>
    AlcoonWalkingRightLoopThird,
    /// <summary>Alcoon: walking right loop, fourth frame.</summary>
    AlcoonWalkingRightLoopFourth,
    /// <summary>Alcoon: fire right, first frame.</summary>
    AlcoonFireRightFirst,
    /// <summary>Alcoon: fire right, second frame.</summary>
    AlcoonFireRightSecond,
    /// <summary>Alcoon: fire right, third frame.</summary>
    AlcoonFireRightThird,
    /// <summary>Alcoon: fire right, fourth frame.</summary>
    AlcoonFireRightFourth,
    /// <summary>Alcoon: right air up.</summary>
    AlcoonRightAirUp,
    /// <summary>Beetom: crawling left loop, first frame.</summary>
    BeetomCrawlingLeftLoopFirst,
    /// <summary>Beetom: crawling left loop, second frame.</summary>
    BeetomCrawlingLeftLoopSecond,
    /// <summary>Beetom: crawling left loop, third frame.</summary>
    BeetomCrawlingLeftLoopThird,
    /// <summary>Beetom: hop left, first frame.</summary>
    BeetomHopLeftFirst,
    /// <summary>Beetom: hop left, second frame.</summary>
    BeetomHopLeftSecond,
    /// <summary>Beetom: draining left, first frame.</summary>
    BeetomDrainingLeftFirst,
    /// <summary>Beetom: draining left, second frame.</summary>
    BeetomDrainingLeftSecond,
    /// <summary>Beetom: draining left, third frame.</summary>
    BeetomDrainingLeftThird,
    /// <summary>Beetom: draining left loop, first frame.</summary>
    BeetomDrainingLeftLoopFirst,
    /// <summary>Beetom: draining left loop, second frame.</summary>
    BeetomDrainingLeftLoopSecond,
    /// <summary>Beetom: draining left loop, third frame.</summary>
    BeetomDrainingLeftLoopThird,
    /// <summary>Beetom: crawling right loop, first frame.</summary>
    BeetomCrawlingRightLoopFirst,
    /// <summary>Beetom: crawling right loop, second frame.</summary>
    BeetomCrawlingRightLoopSecond,
    /// <summary>Beetom: crawling right loop, third frame.</summary>
    BeetomCrawlingRightLoopThird,
    /// <summary>Beetom: hop right, first frame.</summary>
    BeetomHopRightFirst,
    /// <summary>Beetom: hop right, second frame.</summary>
    BeetomHopRightSecond,
    /// <summary>Beetom: draining right, first frame.</summary>
    BeetomDrainingRightFirst,
    /// <summary>Beetom: draining right, second frame.</summary>
    BeetomDrainingRightSecond,
    /// <summary>Beetom: draining right, third frame.</summary>
    BeetomDrainingRightThird,
    /// <summary>Beetom: draining right loop, first frame.</summary>
    BeetomDrainingRightLoopFirst,
    /// <summary>Beetom: draining right loop, second frame.</summary>
    BeetomDrainingRightLoopSecond,
    /// <summary>Beetom: draining right loop, third frame.</summary>
    BeetomDrainingRightLoopThird,
    /// <summary>Sidehopper: jump floor.</summary>
    SidehopperJumpFloor,
    /// <summary>Sidehopper: floor landed, first frame.</summary>
    SidehopperFloorLandedFirst,
    /// <summary>Sidehopper: floor landed, second frame.</summary>
    SidehopperFloorLandedSecond,
    /// <summary>Sidehopper: jump ceiling.</summary>
    SidehopperJumpCeiling,
    /// <summary>Sidehopper: ceiling landed, first frame.</summary>
    SidehopperCeilingLandedFirst,
    /// <summary>Sidehopper: ceiling landed, second frame.</summary>
    SidehopperCeilingLandedSecond,
    /// <summary>Dessgeega: jump floor.</summary>
    DessgeegaJumpFloor,
    /// <summary>Dessgeega: floor landed, first frame.</summary>
    DessgeegaFloorLandedFirst,
    /// <summary>Dessgeega: floor landed, second frame.</summary>
    DessgeegaFloorLandedSecond,
    /// <summary>Dessgeega: jump ceiling.</summary>
    DessgeegaJumpCeiling,
    /// <summary>Dessgeega: ceiling landed, first frame.</summary>
    DessgeegaCeilingLandedFirst,
    /// <summary>Dessgeega: ceiling landed, second frame.</summary>
    DessgeegaCeilingLandedSecond,
    /// <summary>The large Sidehopper: jump floor.</summary>
    LargeSidehopperJumpFloor,
    /// <summary>The large Sidehopper: floor landed, first frame.</summary>
    LargeSidehopperFloorLandedFirst,
    /// <summary>The large Sidehopper: floor landed, second frame.</summary>
    LargeSidehopperFloorLandedSecond,
    /// <summary>The large Sidehopper: jump ceiling.</summary>
    LargeSidehopperJumpCeiling,
    /// <summary>The large Sidehopper: ceiling landed, first frame.</summary>
    LargeSidehopperCeilingLandedFirst,
    /// <summary>The large Sidehopper: ceiling landed, second frame.</summary>
    LargeSidehopperCeilingLandedSecond,
    /// <summary>The large Dessgeega: jump floor.</summary>
    LargeDessgeegaJumpFloor,
    /// <summary>The large Dessgeega: floor landed, first frame.</summary>
    LargeDessgeegaFloorLandedFirst,
    /// <summary>The large Dessgeega: floor landed, second frame.</summary>
    LargeDessgeegaFloorLandedSecond,
    /// <summary>The large Dessgeega: jump ceiling.</summary>
    LargeDessgeegaJumpCeiling,
    /// <summary>The large Dessgeega: ceiling landed, first frame.</summary>
    LargeDessgeegaCeilingLandedFirst,
    /// <summary>The large Dessgeega: ceiling landed, second frame.</summary>
    LargeDessgeegaCeilingLandedSecond,
    /// <summary>Choot: idle.</summary>
    ChootIdle,
    /// <summary>Choot: jump.</summary>
    ChootJump,
    /// <summary>Choot: jump apex.</summary>
    ChootJumpApex,
    /// <summary>Choot: fall end.</summary>
    ChootFallEnd,
    /// <summary>The horizontal Zoomer: upside right loop, first frame.</summary>
    HZoomerUpsideRightLoopFirst,
    /// <summary>The horizontal Zoomer: upside right loop, second frame.</summary>
    HZoomerUpsideRightLoopSecond,
    /// <summary>The horizontal Zoomer: upside right loop, third frame.</summary>
    HZoomerUpsideRightLoopThird,
    /// <summary>The horizontal Zoomer: upside right loop, fourth frame.</summary>
    HZoomerUpsideRightLoopFourth,
    /// <summary>The horizontal Zoomer: upside right loop, fifth frame.</summary>
    HZoomerUpsideRightLoopFifth,
    /// <summary>The horizontal Zoomer: upside left loop, first frame.</summary>
    HZoomerUpsideLeftLoopFirst,
    /// <summary>The horizontal Zoomer: upside left loop, second frame.</summary>
    HZoomerUpsideLeftLoopSecond,
    /// <summary>The horizontal Zoomer: upside left loop, third frame.</summary>
    HZoomerUpsideLeftLoopThird,
    /// <summary>The horizontal Zoomer: upside left loop, fourth frame.</summary>
    HZoomerUpsideLeftLoopFourth,
    /// <summary>The horizontal Zoomer: upside left loop, fifth frame.</summary>
    HZoomerUpsideLeftLoopFifth,
    /// <summary>The horizontal Zoomer: upside down loop, first frame.</summary>
    HZoomerUpsideDownLoopFirst,
    /// <summary>The horizontal Zoomer: upside down loop, second frame.</summary>
    HZoomerUpsideDownLoopSecond,
    /// <summary>The horizontal Zoomer: upside down loop, third frame.</summary>
    HZoomerUpsideDownLoopThird,
    /// <summary>The horizontal Zoomer: upside down loop, fourth frame.</summary>
    HZoomerUpsideDownLoopFourth,
    /// <summary>The horizontal Zoomer: upside down loop, fifth frame.</summary>
    HZoomerUpsideDownLoopFifth,
    /// <summary>The horizontal Zoomer: upside up loop, first frame.</summary>
    HZoomerUpsideUpLoopFirst,
    /// <summary>The horizontal Zoomer: upside up loop, second frame.</summary>
    HZoomerUpsideUpLoopSecond,
    /// <summary>The horizontal Zoomer: upside up loop, third frame.</summary>
    HZoomerUpsideUpLoopThird,
    /// <summary>The horizontal Zoomer: upside up loop, fourth frame.</summary>
    HZoomerUpsideUpLoopFourth,
    /// <summary>The horizontal Zoomer: upside up loop, fifth frame.</summary>
    HZoomerUpsideUpLoopFifth,
    /// <summary>Sbug: right, first frame.</summary>
    SbugRightFirst,
    /// <summary>Sbug: right, second frame.</summary>
    SbugRightSecond,
    /// <summary>Sbug: right, third frame.</summary>
    SbugRightThird,
    /// <summary>Sbug: up right, first frame.</summary>
    SbugUpRightFirst,
    /// <summary>Sbug: up right, second frame.</summary>
    SbugUpRightSecond,
    /// <summary>Sbug: up right, third frame.</summary>
    SbugUpRightThird,
    /// <summary>Sbug: up, first frame.</summary>
    SbugUpFirst,
    /// <summary>Sbug: up, second frame.</summary>
    SbugUpSecond,
    /// <summary>Sbug: up, third frame.</summary>
    SbugUpThird,
    /// <summary>Sbug: up left, first frame.</summary>
    SbugUpLeftFirst,
    /// <summary>Sbug: up left, second frame.</summary>
    SbugUpLeftSecond,
    /// <summary>Sbug: up left, third frame.</summary>
    SbugUpLeftThird,
    /// <summary>Sbug: left, first frame.</summary>
    SbugLeftFirst,
    /// <summary>Sbug: left, second frame.</summary>
    SbugLeftSecond,
    /// <summary>Sbug: left, third frame.</summary>
    SbugLeftThird,
    /// <summary>Sbug: down left, first frame.</summary>
    SbugDownLeftFirst,
    /// <summary>Sbug: down left, second frame.</summary>
    SbugDownLeftSecond,
    /// <summary>Sbug: down left, third frame.</summary>
    SbugDownLeftThird,
    /// <summary>Sbug: down, first frame.</summary>
    SbugDownFirst,
    /// <summary>Sbug: down, second frame.</summary>
    SbugDownSecond,
    /// <summary>Sbug: down, third frame.</summary>
    SbugDownThird,
    /// <summary>Sbug: down right, first frame.</summary>
    SbugDownRightFirst,
    /// <summary>Sbug: down right, second frame.</summary>
    SbugDownRightSecond,
    /// <summary>Sbug: down right, third frame.</summary>
    SbugDownRightThird,
    /// <summary>Fune: left idle.</summary>
    FuneLeftIdle,
    /// <summary>Fune: active left, second frame.</summary>
    FuneActiveLeftSecond,
    /// <summary>Fune: active left, third frame.</summary>
    FuneActiveLeftThird,
    /// <summary>Fune: active left, fourth frame.</summary>
    FuneActiveLeftFourth,
    /// <summary>Fune: active left, fifth frame.</summary>
    FuneActiveLeftFifth,
    /// <summary>Fune: right idle.</summary>
    FuneRightIdle,
    /// <summary>Fune: active right, second frame.</summary>
    FuneActiveRightSecond,
    /// <summary>Fune: active right, third frame.</summary>
    FuneActiveRightThird,
    /// <summary>Fune: active right, fourth frame.</summary>
    FuneActiveRightFourth,
    /// <summary>Fune: active right, fifth frame.</summary>
    FuneActiveRightFifth,
    /// <summary>Namihe: left idle.</summary>
    NamiheLeftIdle,
    /// <summary>Namihe: active left, second frame.</summary>
    NamiheActiveLeftSecond,
    /// <summary>Namihe: active left, third frame.</summary>
    NamiheActiveLeftThird,
    /// <summary>Namihe: active left, fourth frame.</summary>
    NamiheActiveLeftFourth,
    /// <summary>Namihe: active left, fifth frame.</summary>
    NamiheActiveLeftFifth,
    /// <summary>Namihe: active left, sixth frame.</summary>
    NamiheActiveLeftSixth,
    /// <summary>Namihe: right idle.</summary>
    NamiheRightIdle,
    /// <summary>Namihe: active right, second frame.</summary>
    NamiheActiveRightSecond,
    /// <summary>Namihe: active right, third frame.</summary>
    NamiheActiveRightThird,
    /// <summary>Namihe: active right, fourth frame.</summary>
    NamiheActiveRightFourth,
    /// <summary>Namihe: active right, fifth frame.</summary>
    NamiheActiveRightFifth,
    /// <summary>Namihe: active right, sixth frame.</summary>
    NamiheActiveRightSixth,
    /// <summary>Kamer: platform, first frame.</summary>
    KamerPlatformFirst,
    /// <summary>Kamer: platform, second frame.</summary>
    KamerPlatformSecond,
    /// <summary>Kamer: platform, third frame.</summary>
    KamerPlatformThird,
    /// <summary>Kamer: platform, fourth frame.</summary>
    KamerPlatformFourth,
    /// <summary>The elevator: loop, first frame.</summary>
    ElevatorLoopFirst,
    /// <summary>The elevator: loop, second frame.</summary>
    ElevatorLoopSecond,
    /// <summary>A room sprite object: evir facing left, first frame.</summary>
    SpriteObjectEvirFacingLeftFirst,
    /// <summary>A room sprite object: evir facing left, second frame.</summary>
    SpriteObjectEvirFacingLeftSecond,
    /// <summary>A room sprite object: evir facing left, third frame.</summary>
    SpriteObjectEvirFacingLeftThird,
    /// <summary>A room sprite object: evir facing left, fourth frame.</summary>
    SpriteObjectEvirFacingLeftFourth,
    /// <summary>A room sprite object: short Draygon breath bubbles, first frame.</summary>
    SpriteObjectShortDraygonBreathBubblesFirst,
    /// <summary>A room sprite object: short Draygon breath bubbles, second frame.</summary>
    SpriteObjectShortDraygonBreathBubblesSecond,
    /// <summary>A room sprite object: short Draygon breath bubbles, third frame.</summary>
    SpriteObjectShortDraygonBreathBubblesThird,
    /// <summary>A room sprite object: short Draygon breath bubbles, fourth frame.</summary>
    SpriteObjectShortDraygonBreathBubblesFourth,
    /// <summary>A room sprite object: short Draygon breath bubbles, fifth frame.</summary>
    SpriteObjectShortDraygonBreathBubblesFifth,
    /// <summary>A room sprite object: short Draygon breath bubbles, sixth frame.</summary>
    SpriteObjectShortDraygonBreathBubblesSixth,
    /// <summary>A room sprite object: short Draygon breath bubbles, seventh frame.</summary>
    SpriteObjectShortDraygonBreathBubblesSeventh,
    /// <summary>A room sprite object: short Draygon breath bubbles, eighth frame.</summary>
    SpriteObjectShortDraygonBreathBubblesEighth,
    /// <summary>A room sprite object: short Draygon breath bubbles, ninth frame.</summary>
    SpriteObjectShortDraygonBreathBubblesNinth,
    /// <summary>A room sprite object: empty.</summary>
    SpriteObjectEmpty,
    /// <summary>A room sprite object: short big dust cloud, first frame.</summary>
    SpriteObjectShortBigDustCloudFirst,
    /// <summary>A room sprite object: short big dust cloud, second frame.</summary>
    SpriteObjectShortBigDustCloudSecond,
    /// <summary>A room sprite object: short big dust cloud, third frame.</summary>
    SpriteObjectShortBigDustCloudThird,
    /// <summary>A room sprite object: short big dust cloud, fourth frame.</summary>
    SpriteObjectShortBigDustCloudFourth,
    /// <summary>A room sprite object: big dust cloud, fifth frame.</summary>
    SpriteObjectBigDustCloudFifth,
    /// <summary>A room sprite object: big dust cloud, sixth frame.</summary>
    SpriteObjectBigDustCloudSixth,
    /// <summary>A room sprite object: big dust cloud, seventh frame.</summary>
    SpriteObjectBigDustCloudSeventh,
    /// <summary>A room sprite object: big dust cloud, eighth frame.</summary>
    SpriteObjectBigDustCloudEighth,
    /// <summary>A room sprite object: small explosion, first frame.</summary>
    SpriteObjectSmallExplosionFirst,
    /// <summary>A room sprite object: small explosion, second frame.</summary>
    SpriteObjectSmallExplosionSecond,
    /// <summary>A room sprite object: small explosion, third frame.</summary>
    SpriteObjectSmallExplosionThird,
    /// <summary>A room sprite object: small explosion, fourth frame.</summary>
    SpriteObjectSmallExplosionFourth,
    /// <summary>A room sprite object: small explosion, fifth frame.</summary>
    SpriteObjectSmallExplosionFifth,
    /// <summary>A room sprite object: small explosion, sixth frame.</summary>
    SpriteObjectSmallExplosionSixth,
    /// <summary>A room sprite object: dud shot, first frame.</summary>
    SpriteObjectDudShotFirst,
    /// <summary>A room sprite object: dud shot, second frame.</summary>
    SpriteObjectDudShotSecond,
    /// <summary>A room sprite object: dud shot, third frame.</summary>
    SpriteObjectDudShotThird,
    /// <summary>A room sprite object: dud shot, fourth frame.</summary>
    SpriteObjectDudShotFourth,
    /// <summary>A room sprite object: dud shot, fifth frame.</summary>
    SpriteObjectDudShotFifth,
    /// <summary>A room sprite object: dud shot, sixth frame.</summary>
    SpriteObjectDudShotSixth,
    /// <summary>A room sprite object: small dud shot, second frame.</summary>
    SpriteObjectSmallDudShotSecond,
    /// <summary>A room sprite object: small dud shot, third frame.</summary>
    SpriteObjectSmallDudShotThird,
    /// <summary>A room sprite object: small dud shot, fourth frame.</summary>
    SpriteObjectSmallDudShotFourth,
    /// <summary>A room sprite object: small dud shot, fifth frame.</summary>
    SpriteObjectSmallDudShotFifth,
    /// <summary>A room sprite object: small dud shot, first frame.</summary>
    SpriteObjectSmallDudShotFirst,
    /// <summary>A room sprite object: space pirate landing dust cloud, second frame.</summary>
    SpriteObjectSpacePirateLandingDustCloudSecond,
    /// <summary>A room sprite object: space pirate landing dust cloud, third frame.</summary>
    SpriteObjectSpacePirateLandingDustCloudThird,
    /// <summary>A room sprite object: space pirate landing dust cloud, fourth frame.</summary>
    SpriteObjectSpacePirateLandingDustCloudFourth,
    /// <summary>A room sprite object: smoke, first frame.</summary>
    SpriteObjectSmokeFirst,
    /// <summary>A room sprite object: smoke, second frame.</summary>
    SpriteObjectSmokeSecond,
    /// <summary>A room sprite object: smoke, third frame.</summary>
    SpriteObjectSmokeThird,
    /// <summary>A room sprite object: smoke, fourth frame.</summary>
    SpriteObjectSmokeFourth,
    /// <summary>A room sprite object: big explosion, first frame.</summary>
    SpriteObjectBigExplosionFirst,
    /// <summary>A room sprite object: big explosion, second frame.</summary>
    SpriteObjectBigExplosionSecond,
    /// <summary>A room sprite object: big explosion, third frame.</summary>
    SpriteObjectBigExplosionThird,
    /// <summary>A room sprite object: big explosion, fourth frame.</summary>
    SpriteObjectBigExplosionFourth,
    /// <summary>A room sprite object: big explosion, fifth frame.</summary>
    SpriteObjectBigExplosionFifth,
    /// <summary>A room sprite object: big explosion, sixth frame.</summary>
    SpriteObjectBigExplosionSixth,
    /// <summary>A room sprite object: puromi body, first frame.</summary>
    SpriteObjectPuromiBodyFirst,
    /// <summary>A room sprite object: puromi body, second frame.</summary>
    SpriteObjectPuromiBodySecond,
    /// <summary>A room sprite object: puromi body, fourth frame.</summary>
    SpriteObjectPuromiBodyFourth,
    /// <summary>A room sprite object: puromi body, fifth frame.</summary>
    SpriteObjectPuromiBodyFifth,
    /// <summary>A room sprite object: puromi body, third frame.</summary>
    SpriteObjectPuromiBodyThird,
    /// <summary>A room sprite object: puromi body, sixth frame.</summary>
    SpriteObjectPuromiBodySixth,
    /// <summary>A room sprite object: puromi body, seventh frame.</summary>
    SpriteObjectPuromiBodySeventh,
    /// <summary>A room sprite object: puromi body, eighth frame.</summary>
    SpriteObjectPuromiBodyEighth,
    /// <summary>A room sprite object: puromi right explosion, first frame.</summary>
    SpriteObjectPuromiRightExplosionFirst,
    /// <summary>A room sprite object: puromi right explosion, second frame.</summary>
    SpriteObjectPuromiRightExplosionSecond,
    /// <summary>A room sprite object: puromi right explosion, third frame.</summary>
    SpriteObjectPuromiRightExplosionThird,
    /// <summary>A room sprite object: puromi right explosion, fourth frame.</summary>
    SpriteObjectPuromiRightExplosionFourth,
    /// <summary>A room sprite object: puromi right explosion, fifth frame.</summary>
    SpriteObjectPuromiRightExplosionFifth,
    /// <summary>A room sprite object: puromi right explosion, sixth frame.</summary>
    SpriteObjectPuromiRightExplosionSixth,
    /// <summary>A room sprite object: puromi right explosion, seventh frame.</summary>
    SpriteObjectPuromiRightExplosionSeventh,
    /// <summary>A room sprite object: puromi left explosion, first frame.</summary>
    SpriteObjectPuromiLeftExplosionFirst,
    /// <summary>A room sprite object: puromi left explosion, second frame.</summary>
    SpriteObjectPuromiLeftExplosionSecond,
    /// <summary>A room sprite object: puromi left explosion, third frame.</summary>
    SpriteObjectPuromiLeftExplosionThird,
    /// <summary>A room sprite object: puromi left explosion, fourth frame.</summary>
    SpriteObjectPuromiLeftExplosionFourth,
    /// <summary>A room sprite object: puromi left explosion, fifth frame.</summary>
    SpriteObjectPuromiLeftExplosionFifth,
    /// <summary>A room sprite object: puromi left explosion, sixth frame.</summary>
    SpriteObjectPuromiLeftExplosionSixth,
    /// <summary>A room sprite object: puromi left explosion, seventh frame.</summary>
    SpriteObjectPuromiLeftExplosionSeventh,
    /// <summary>A room sprite object: puromi splash, first frame.</summary>
    SpriteObjectPuromiSplashFirst,
    /// <summary>A room sprite object: puromi splash, second frame.</summary>
    SpriteObjectPuromiSplashSecond,
    /// <summary>A room sprite object: puromi splash, third frame.</summary>
    SpriteObjectPuromiSplashThird,
    /// <summary>A room sprite object: puromi splash, fourth frame.</summary>
    SpriteObjectPuromiSplashFourth,
    /// <summary>A room sprite object: falling spark trail, first frame.</summary>
    SpriteObjectFallingSparkTrailFirst,
    /// <summary>A room sprite object: falling spark trail, second frame.</summary>
    SpriteObjectFallingSparkTrailSecond,
    /// <summary>A room sprite object: falling spark trail, third frame.</summary>
    SpriteObjectFallingSparkTrailThird,
    /// <summary>A room sprite object: falling spark trail, fourth frame.</summary>
    SpriteObjectFallingSparkTrailFourth,
    /// <summary>A room sprite object: Metroid shell, first frame.</summary>
    SpriteObjectMetroidShellFirst,
    /// <summary>A room sprite object: Metroid shell, third frame.</summary>
    SpriteObjectMetroidShellThird,
    /// <summary>A room sprite object: Metroid shell loop, third frame.</summary>
    SpriteObjectMetroidShellLoopThird,
    /// <summary>A room sprite object: Metroid electricity, first frame.</summary>
    SpriteObjectMetroidElectricityFirst,
    /// <summary>A room sprite object: Metroid electricity, second frame.</summary>
    SpriteObjectMetroidElectricitySecond,
    /// <summary>A room sprite object: Metroid electricity, third frame.</summary>
    SpriteObjectMetroidElectricityThird,
    /// <summary>A room sprite object: Metroid electricity, fifth frame.</summary>
    SpriteObjectMetroidElectricityFifth,
    /// <summary>A room sprite object: Metroid electricity, sixth frame.</summary>
    SpriteObjectMetroidElectricitySixth,
    /// <summary>A room sprite object: Metroid electricity, seventh frame.</summary>
    SpriteObjectMetroidElectricitySeventh,
    /// <summary>A room sprite object: Metroid electricity loop, first frame.</summary>
    SpriteObjectMetroidElectricityLoopFirst,
    /// <summary>A room sprite object: Metroid electricity loop, second frame.</summary>
    SpriteObjectMetroidElectricityLoopSecond,
    /// <summary>A room sprite object: Metroid electricity loop, third frame.</summary>
    SpriteObjectMetroidElectricityLoopThird,
    /// <summary>A room sprite object: Metroid electricity, eleventh frame.</summary>
    SpriteObjectMetroidElectricityEleventh,
    /// <summary>A room sprite object: Metroid electricity, twelfth frame.</summary>
    SpriteObjectMetroidElectricityTwelfth,
    /// <summary>A room sprite object: Metroid electricity, thirteenth frame.</summary>
    SpriteObjectMetroidElectricityThirteenth,
    /// <summary>A room sprite object: Metroid electricity, eighth frame.</summary>
    SpriteObjectMetroidElectricityEighth,
    /// <summary>A room sprite object: Metroid electricity, ninth frame.</summary>
    SpriteObjectMetroidElectricityNinth,
    /// <summary>A room sprite object: Metroid electricity, tenth frame.</summary>
    SpriteObjectMetroidElectricityTenth,
    /// <summary>A room sprite object: Metroid electricity, fourteenth frame.</summary>
    SpriteObjectMetroidElectricityFourteenth,
    /// <summary>A room sprite object: Metroid electricity, fifteenth frame.</summary>
    SpriteObjectMetroidElectricityFifteenth,
    /// <summary>A room sprite object: Metroid electricity, sixteenth frame.</summary>
    SpriteObjectMetroidElectricitySixteenth,
    /// <summary>A room sprite object: Metroid electricity, seventeenth frame.</summary>
    SpriteObjectMetroidElectricitySeventeenth,
    /// <summary>A room sprite object: Metroid electricity, eighteenth frame.</summary>
    SpriteObjectMetroidElectricityEighteenth,
    /// <summary>A room sprite object: Metroid electricity, nineteenth frame.</summary>
    SpriteObjectMetroidElectricityNineteenth,
    /// <summary>A room sprite object: Metroid electricity loop, fifth frame.</summary>
    SpriteObjectMetroidElectricityLoopFifth,
    /// <summary>A room sprite object: Metroid electricity loop, sixth frame.</summary>
    SpriteObjectMetroidElectricityLoopSixth,
    /// <summary>A room sprite object: Metroid electricity loop, seventh frame.</summary>
    SpriteObjectMetroidElectricityLoopSeventh,
    /// <summary>A room sprite object: yapping maw base facing down.</summary>
    SpriteObjectYappingMawBaseFacingDown,
    /// <summary>A room sprite object: yapping maw base facing up.</summary>
    SpriteObjectYappingMawBaseFacingUp,
    /// <summary>A room sprite object: evir facing right, first frame.</summary>
    SpriteObjectEvirFacingRightFirst,
    /// <summary>A room sprite object: evir facing right, second frame.</summary>
    SpriteObjectEvirFacingRightSecond,
    /// <summary>A room sprite object: evir facing right, third frame.</summary>
    SpriteObjectEvirFacingRightThird,
    /// <summary>A room sprite object: evir facing right, fourth frame.</summary>
    SpriteObjectEvirFacingRightFourth,
    /// <summary>A room sprite object: Draygon foaming at the mouth, first frame.</summary>
    SpriteObjectDraygonFoamingAtTheMouthFirst,
    /// <summary>A room sprite object: Draygon foaming at the mouth, second frame.</summary>
    SpriteObjectDraygonFoamingAtTheMouthSecond,
    /// <summary>A room sprite object: Draygon foaming at the mouth, third frame.</summary>
    SpriteObjectDraygonFoamingAtTheMouthThird,
    /// <summary>A room sprite object: Draygon foaming at the mouth, fourth frame.</summary>
    SpriteObjectDraygonFoamingAtTheMouthFourth,
    /// <summary>A room sprite object: Draygon foaming at the mouth, fifth frame.</summary>
    SpriteObjectDraygonFoamingAtTheMouthFifth,
    /// <summary>A room sprite object: Draygon foaming at the mouth, sixth frame.</summary>
    SpriteObjectDraygonFoamingAtTheMouthSixth,
    /// <summary>A room sprite object: Draygon foaming at the mouth, seventh frame.</summary>
    SpriteObjectDraygonFoamingAtTheMouthSeventh,
    /// <summary>A room sprite object: Draygon foaming at the mouth, eighth frame.</summary>
    SpriteObjectDraygonFoamingAtTheMouthEighth,
    /// <summary>Yapping Maw: attacking up, first frame.</summary>
    YappingMawAttackingUpFirst,
    /// <summary>Yapping Maw: attacking up, second frame.</summary>
    YappingMawAttackingUpSecond,
    /// <summary>Yapping Maw: attacking up, third frame.</summary>
    YappingMawAttackingUpThird,
    /// <summary>Yapping Maw: attacking up right, first frame.</summary>
    YappingMawAttackingUpRightFirst,
    /// <summary>Yapping Maw: attacking up right, second frame.</summary>
    YappingMawAttackingUpRightSecond,
    /// <summary>Yapping Maw: attacking up right, third frame.</summary>
    YappingMawAttackingUpRightThird,
    /// <summary>Yapping Maw: attacking right, first frame.</summary>
    YappingMawAttackingRightFirst,
    /// <summary>Yapping Maw: attacking right, second frame.</summary>
    YappingMawAttackingRightSecond,
    /// <summary>Yapping Maw: attacking right, third frame.</summary>
    YappingMawAttackingRightThird,
    /// <summary>Yapping Maw: attacking down right, first frame.</summary>
    YappingMawAttackingDownRightFirst,
    /// <summary>Yapping Maw: attacking down right, second frame.</summary>
    YappingMawAttackingDownRightSecond,
    /// <summary>Yapping Maw: attacking down right, third frame.</summary>
    YappingMawAttackingDownRightThird,
    /// <summary>Yapping Maw: attacking down, first frame.</summary>
    YappingMawAttackingDownFirst,
    /// <summary>Yapping Maw: attacking down, second frame.</summary>
    YappingMawAttackingDownSecond,
    /// <summary>Yapping Maw: attacking down, third frame.</summary>
    YappingMawAttackingDownThird,
    /// <summary>Yapping Maw: attacking down left, first frame.</summary>
    YappingMawAttackingDownLeftFirst,
    /// <summary>Yapping Maw: attacking down left, second frame.</summary>
    YappingMawAttackingDownLeftSecond,
    /// <summary>Yapping Maw: attacking down left, third frame.</summary>
    YappingMawAttackingDownLeftThird,
    /// <summary>Yapping Maw: attacking left, first frame.</summary>
    YappingMawAttackingLeftFirst,
    /// <summary>Yapping Maw: attacking left, second frame.</summary>
    YappingMawAttackingLeftSecond,
    /// <summary>Yapping Maw: attacking left, third frame.</summary>
    YappingMawAttackingLeftThird,
    /// <summary>Yapping Maw: attacking up left, first frame.</summary>
    YappingMawAttackingUpLeftFirst,
    /// <summary>Yapping Maw: attacking up left, second frame.</summary>
    YappingMawAttackingUpLeftSecond,
    /// <summary>Yapping Maw: attacking up left, third frame.</summary>
    YappingMawAttackingUpLeftThird,
    /// <summary>Ki-Hunter: flying left, first frame.</summary>
    KiHunterFlyingLeftFirst,
    /// <summary>Ki-Hunter: flying left, second frame.</summary>
    KiHunterFlyingLeftSecond,
    /// <summary>Ki-Hunter: flying left, third frame.</summary>
    KiHunterFlyingLeftThird,
    /// <summary>Ki-Hunter: wings left, first frame.</summary>
    KiHunterWingsLeftFirst,
    /// <summary>Ki-Hunter: wings left, second frame.</summary>
    KiHunterWingsLeftSecond,
    /// <summary>Ki-Hunter: wings left, third frame.</summary>
    KiHunterWingsLeftThird,
    /// <summary>Ki-Hunter: flying right, first frame.</summary>
    KiHunterFlyingRightFirst,
    /// <summary>Ki-Hunter: flying right, second frame.</summary>
    KiHunterFlyingRightSecond,
    /// <summary>Ki-Hunter: flying right, third frame.</summary>
    KiHunterFlyingRightThird,
    /// <summary>Ki-Hunter: wings right, first frame.</summary>
    KiHunterWingsRightFirst,
    /// <summary>Ki-Hunter: wings right, second frame.</summary>
    KiHunterWingsRightSecond,
    /// <summary>Ki-Hunter: wings right, third frame.</summary>
    KiHunterWingsRightThird,
    /// <summary>Ki-Hunter: detached wings.</summary>
    KiHunterDetachedWings,
    /// <summary>Ki-Hunter: swoop left, third frame.</summary>
    KiHunterSwoopLeftThird,
    /// <summary>Ki-Hunter: swoop left, first frame.</summary>
    KiHunterSwoopLeftFirst,
    /// <summary>Ki-Hunter: swoop left, second frame.</summary>
    KiHunterSwoopLeftSecond,
    /// <summary>Ki-Hunter: swoop left, fourth frame.</summary>
    KiHunterSwoopLeftFourth,
    /// <summary>Ki-Hunter: swoop left, fifth frame.</summary>
    KiHunterSwoopLeftFifth,
    /// <summary>Ki-Hunter: swoop right, third frame.</summary>
    KiHunterSwoopRightThird,
    /// <summary>Ki-Hunter: swoop right, first frame.</summary>
    KiHunterSwoopRightFirst,
    /// <summary>Ki-Hunter: swoop right, second frame.</summary>
    KiHunterSwoopRightSecond,
    /// <summary>Ki-Hunter: swoop right, fourth frame.</summary>
    KiHunterSwoopRightFourth,
    /// <summary>Ki-Hunter: swoop right, fifth frame.</summary>
    KiHunterSwoopRightFifth,
    /// <summary>Ki-Hunter: jump left, first frame.</summary>
    KiHunterJumpLeftFirst,
    /// <summary>Ki-Hunter: jump left, second frame.</summary>
    KiHunterJumpLeftSecond,
    /// <summary>Ki-Hunter: jump left, third frame.</summary>
    KiHunterJumpLeftThird,
    /// <summary>Ki-Hunter: jump left, fourth frame.</summary>
    KiHunterJumpLeftFourth,
    /// <summary>Ki-Hunter: jump right, first frame.</summary>
    KiHunterJumpRightFirst,
    /// <summary>Ki-Hunter: jump right, second frame.</summary>
    KiHunterJumpRightSecond,
    /// <summary>Ki-Hunter: jump right, third frame.</summary>
    KiHunterJumpRightThird,
    /// <summary>Ki-Hunter: jump right, fourth frame.</summary>
    KiHunterJumpRightFourth,
    /// <summary>Ki-Hunter: spit left, first frame.</summary>
    KiHunterSpitLeftFirst,
    /// <summary>Ki-Hunter: spit left, second frame.</summary>
    KiHunterSpitLeftSecond,
    /// <summary>Ki-Hunter: spit left, third frame.</summary>
    KiHunterSpitLeftThird,
    /// <summary>Ki-Hunter: spit left, fourth frame.</summary>
    KiHunterSpitLeftFourth,
    /// <summary>Ki-Hunter: spit left, fifth frame.</summary>
    KiHunterSpitLeftFifth,
    /// <summary>Ki-Hunter: spit right, first frame.</summary>
    KiHunterSpitRightFirst,
    /// <summary>Ki-Hunter: spit right, second frame.</summary>
    KiHunterSpitRightSecond,
    /// <summary>Ki-Hunter: spit right, third frame.</summary>
    KiHunterSpitRightThird,
    /// <summary>Ki-Hunter: spit right, fourth frame.</summary>
    KiHunterSpitRightFourth,
    /// <summary>Ki-Hunter: spit right, fifth frame.</summary>
    KiHunterSpitRightFifth,
    /// <summary>Mother Brain: head initial.</summary>
    MotherBrainHeadInitial,
    /// <summary>Mother Brain: head neutral phase two, second frame.</summary>
    MotherBrainHeadNeutralPhaseTwoSecond,
    /// <summary>Mother Brain: head stretching phase two, first frame.</summary>
    MotherBrainHeadStretchingPhaseTwoFirst,
    /// <summary>Mother Brain: head stretching phase two, second frame.</summary>
    MotherBrainHeadStretchingPhaseTwoSecond,
    /// <summary>Mother Brain: head stretching phase two, third frame.</summary>
    MotherBrainHeadStretchingPhaseTwoThird,
    /// <summary>Mother Brain: neck joint.</summary>
    MotherBrainNeckJoint,
    /// <summary>Mother Brain: head decapitated loop.</summary>
    MotherBrainHeadDecapitatedLoop,
    /// <summary>Mother Brain: head decapitated, second frame.</summary>
    MotherBrainHeadDecapitatedSecond,
    /// <summary>Mother Brain: head stretching phase three, first frame.</summary>
    MotherBrainHeadStretchingPhaseThreeFirst,
    /// <summary>Mother Brain: head stretching phase three, second frame.</summary>
    MotherBrainHeadStretchingPhaseThreeSecond,
    /// <summary>Mother Brain: head stretching phase three, third frame.</summary>
    MotherBrainHeadStretchingPhaseThreeThird,
    /// <summary>Mother Brain: head corpse, fourth frame.</summary>
    MotherBrainHeadCorpseFourth,
    /// <summary>Mother Brain: head corpse loop.</summary>
    MotherBrainHeadCorpseLoop,
    /// <summary>Mother Brain: tube, first frame.</summary>
    MotherBrainTubeFirst,
    /// <summary>Mother Brain: tube, second frame.</summary>
    MotherBrainTubeSecond,
    /// <summary>Mother Brain: tube, third frame.</summary>
    MotherBrainTubeThird,
    /// <summary>Mother Brain: tube, fourth frame.</summary>
    MotherBrainTubeFourth,
    /// <summary>Mother Brain: tube, fifth frame.</summary>
    MotherBrainTubeFifth,
    /// <summary>The dead Torizo: corpse hook.</summary>
    DeadTorizoCorpseHook,
    /// <summary>Ridley: tail segment small.</summary>
    RidleyTailSegmentSmall,
    /// <summary>Ridley: tail segment medium.</summary>
    RidleyTailSegmentMedium,
    /// <summary>Ridley: tail segment large.</summary>
    RidleyTailSegmentLarge,
    /// <summary>Ridley: breakup tail tip, thirteenth frame.</summary>
    RidleyBreakupTailTipThirteenth,
    /// <summary>Ridley: breakup tail tip, twelfth frame.</summary>
    RidleyBreakupTailTipTwelfth,
    /// <summary>Ridley: breakup tail tip, eleventh frame.</summary>
    RidleyBreakupTailTipEleventh,
    /// <summary>Ridley: breakup tail tip, tenth frame.</summary>
    RidleyBreakupTailTipTenth,
    /// <summary>Ridley: breakup tail tip, ninth frame.</summary>
    RidleyBreakupTailTipNinth,
    /// <summary>Ridley: breakup tail tip, eighth frame.</summary>
    RidleyBreakupTailTipEighth,
    /// <summary>Ridley: breakup tail tip, seventh frame.</summary>
    RidleyBreakupTailTipSeventh,
    /// <summary>Ridley: breakup tail tip, sixth frame.</summary>
    RidleyBreakupTailTipSixth,
    /// <summary>Ridley: breakup tail tip, fifth frame.</summary>
    RidleyBreakupTailTipFifth,
    /// <summary>Ridley: breakup tail tip, fourth frame.</summary>
    RidleyBreakupTailTipFourth,
    /// <summary>Ridley: breakup tail tip, third frame.</summary>
    RidleyBreakupTailTipThird,
    /// <summary>Ridley: breakup tail tip, second frame.</summary>
    RidleyBreakupTailTipSecond,
    /// <summary>Ridley: breakup tail tip, first frame.</summary>
    RidleyBreakupTailTipFirst,
    /// <summary>Ridley: breakup tail tip, sixteenth frame.</summary>
    RidleyBreakupTailTipSixteenth,
    /// <summary>Ridley: breakup tail tip, fifteenth frame.</summary>
    RidleyBreakupTailTipFifteenth,
    /// <summary>Ridley: breakup tail tip, fourteenth frame.</summary>
    RidleyBreakupTailTipFourteenth,
    /// <summary>Ridley: wing facing left, first frame.</summary>
    RidleyWingFacingLeftFirst,
    /// <summary>Ridley: wing facing left, second frame.</summary>
    RidleyWingFacingLeftSecond,
    /// <summary>Ridley: wing facing left, third frame.</summary>
    RidleyWingFacingLeftThird,
    /// <summary>Ridley: wing facing left, fourth frame.</summary>
    RidleyWingFacingLeftFourth,
    /// <summary>Ridley: wing facing left, fifth frame.</summary>
    RidleyWingFacingLeftFifth,
    /// <summary>Ridley: wing facing left, sixth frame.</summary>
    RidleyWingFacingLeftSixth,
    /// <summary>Ridley: wing facing right, first frame.</summary>
    RidleyWingFacingRightFirst,
    /// <summary>Ridley: wing facing right, second frame.</summary>
    RidleyWingFacingRightSecond,
    /// <summary>Ridley: wing facing right, third frame.</summary>
    RidleyWingFacingRightThird,
    /// <summary>Ridley: wing facing right, fourth frame.</summary>
    RidleyWingFacingRightFourth,
    /// <summary>Ridley: wing facing right, fifth frame.</summary>
    RidleyWingFacingRightFifth,
    /// <summary>Ridley: wing facing right, sixth frame.</summary>
    RidleyWingFacingRightSixth,
    /// <summary>Sciser: upside right loop, first frame.</summary>
    SciserUpsideRightLoopFirst,
    /// <summary>Sciser: upside right loop, second frame.</summary>
    SciserUpsideRightLoopSecond,
    /// <summary>Sciser: upside right loop, third frame.</summary>
    SciserUpsideRightLoopThird,
    /// <summary>Sciser: upside left loop, first frame.</summary>
    SciserUpsideLeftLoopFirst,
    /// <summary>Sciser: upside left loop, second frame.</summary>
    SciserUpsideLeftLoopSecond,
    /// <summary>Sciser: upside left loop, third frame.</summary>
    SciserUpsideLeftLoopThird,
    /// <summary>Sciser: upside down loop, first frame.</summary>
    SciserUpsideDownLoopFirst,
    /// <summary>Sciser: upside down loop, second frame.</summary>
    SciserUpsideDownLoopSecond,
    /// <summary>Sciser: upside down loop, third frame.</summary>
    SciserUpsideDownLoopThird,
    /// <summary>Sciser: upside up loop, first frame.</summary>
    SciserUpsideUpLoopFirst,
    /// <summary>Sciser: upside up loop, second frame.</summary>
    SciserUpsideUpLoopSecond,
    /// <summary>Sciser: upside up loop, third frame.</summary>
    SciserUpsideUpLoopThird,
    /// <summary>The fly: flight, first frame.</summary>
    FlyFlightFirst,
    /// <summary>The fly: flight, second frame.</summary>
    FlyFlightSecond,
    /// <summary>The fly: flight, third frame.</summary>
    FlyFlightThird,
    /// <summary>The fly: flight, fourth frame.</summary>
    FlyFlightFourth,
    /// <summary>Kago: slow, first frame.</summary>
    KagoSlowFirst,
    /// <summary>Kago: slow, second frame.</summary>
    KagoSlowSecond,
    /// <summary>Kago: slow, third frame.</summary>
    KagoSlowThird,
    /// <summary>The face block: neutral.</summary>
    FaceBlockNeutral,
    /// <summary>The blue Brinstar face block: Samus left, second frame.</summary>
    BlueBrinstarFaceBlockSamusLeftSecond,
    /// <summary>The blue Brinstar face block: Samus left, third frame.</summary>
    BlueBrinstarFaceBlockSamusLeftThird,
    /// <summary>The blue Brinstar face block: Samus right, second frame.</summary>
    BlueBrinstarFaceBlockSamusRightSecond,
    /// <summary>The blue Brinstar face block: Samus right, third frame.</summary>
    BlueBrinstarFaceBlockSamusRightThird,
    /// <summary>The Morph Ball eye: active, first frame.</summary>
    MorphBallEyeActiveFirst,
    /// <summary>The Morph Ball eye: active, second frame.</summary>
    MorphBallEyeActiveSecond,
    /// <summary>The Morph Ball eye: active, third frame.</summary>
    MorphBallEyeActiveThird,
    /// <summary>The Morph Ball eye: active, fourth frame.</summary>
    MorphBallEyeActiveFourth,
    /// <summary>The Morph Ball eye: active, fifth frame.</summary>
    MorphBallEyeActiveFifth,
    /// <summary>The Morph Ball eye: active, sixth frame.</summary>
    MorphBallEyeActiveSixth,
    /// <summary>The Morph Ball eye: active, seventh frame.</summary>
    MorphBallEyeActiveSeventh,
    /// <summary>The Morph Ball eye: active, eighth frame.</summary>
    MorphBallEyeActiveEighth,
    /// <summary>The Morph Ball eye: active, ninth frame.</summary>
    MorphBallEyeActiveNinth,
    /// <summary>The Morph Ball eye: active, tenth frame.</summary>
    MorphBallEyeActiveTenth,
    /// <summary>The Morph Ball eye: active, eleventh frame.</summary>
    MorphBallEyeActiveEleventh,
    /// <summary>The Morph Ball eye: active, twelfth frame.</summary>
    MorphBallEyeActiveTwelfth,
    /// <summary>The Morph Ball eye: active, thirteenth frame.</summary>
    MorphBallEyeActiveThirteenth,
    /// <summary>The Morph Ball eye: active, fourteenth frame.</summary>
    MorphBallEyeActiveFourteenth,
    /// <summary>The Morph Ball eye: right transition.</summary>
    MorphEyeRightTransition,
    /// <summary>The Morph Ball eye: right closed.</summary>
    MorphEyeRightClosed,
    /// <summary>The Morph Ball eye: left transition.</summary>
    MorphEyeLeftTransition,
    /// <summary>The Morph Ball eye: left closed.</summary>
    MorphEyeLeftClosed,
    /// <summary>The Morph Ball eye: mount right.</summary>
    MorphEyeMountRight,
    /// <summary>The Morph Ball eye: mount down.</summary>
    MorphEyeMountDown,
    /// <summary>The Morph Ball eye: mount left.</summary>
    MorphEyeMountLeft,
    /// <summary>The Morph Ball eye: mount up.</summary>
    MorphEyeMountUp,
    /// <summary>The shutter: growing ten pixels.</summary>
    ShutterGrowingTenPixels,
    /// <summary>The shutter: growing twenty pixels.</summary>
    ShutterGrowingTwentyPixels,
    /// <summary>The shutter: growing thirty pixels.</summary>
    ShutterGrowingThirtyPixels,
    /// <summary>The shutter: forty pixels.</summary>
    ShutterFortyPixels,
    /// <summary>The Metroid: chasing Samus, first frame.</summary>
    MetroidChasingSamusFirst,
    /// <summary>The Metroid: chasing Samus, second frame.</summary>
    MetroidChasingSamusSecond,
    /// <summary>The Metroid: chasing Samus, third frame.</summary>
    MetroidChasingSamusThird,
    /// <summary>The Metroid: chasing Samus, fourth frame.</summary>
    MetroidChasingSamusFourth,
    /// <summary>Shaktool: saw hand final piece, first frame.</summary>
    ShaktoolSawHandFinalPieceFirst,
    /// <summary>Shaktool: saw hand final piece, second frame.</summary>
    ShaktoolSawHandFinalPieceSecond,
    /// <summary>Shaktool: saw hand final piece, third frame.</summary>
    ShaktoolSawHandFinalPieceThird,
    /// <summary>Shaktool: arm.</summary>
    ShaktoolArm,
    /// <summary>Shaktool: head left.</summary>
    ShaktoolHeadLeft,
    /// <summary>Shaktool: head up left.</summary>
    ShaktoolHeadUpLeft,
    /// <summary>Shaktool: head up.</summary>
    ShaktoolHeadUp,
    /// <summary>Shaktool: head up right.</summary>
    ShaktoolHeadUpRight,
    /// <summary>Shaktool: head right.</summary>
    ShaktoolHeadRight,
    /// <summary>Shaktool: head down right.</summary>
    ShaktoolHeadDownRight,
    /// <summary>Shaktool: head down.</summary>
    ShaktoolHeadDown,
    /// <summary>Shaktool: head down left.</summary>
    ShaktoolHeadDownLeft,
    /// <summary>Shaktool: saw hand primary piece, first frame.</summary>
    ShaktoolSawHandPrimaryPieceFirst,
    /// <summary>Shaktool: saw hand primary piece, second frame.</summary>
    ShaktoolSawHandPrimaryPieceSecond,
    /// <summary>Shaktool: saw hand primary piece, third frame.</summary>
    ShaktoolSawHandPrimaryPieceThird,
    /// <summary>Tripper: moving left loop, first frame.</summary>
    TripperMovingLeftLoopFirst,
    /// <summary>Tripper: moving left loop, second frame.</summary>
    TripperMovingLeftLoopSecond,
    /// <summary>Tripper: moving left loop, third frame.</summary>
    TripperMovingLeftLoopThird,
    /// <summary>Tripper: moving right loop, first frame.</summary>
    TripperMovingRightLoopFirst,
    /// <summary>Tripper: moving right loop, second frame.</summary>
    TripperMovingRightLoopSecond,
    /// <summary>Tripper: moving right loop, third frame.</summary>
    TripperMovingRightLoopThird,
    /// <summary>Tripper: still moving left loop, first frame.</summary>
    TripperStillMovingLeftLoopFirst,
    /// <summary>Tripper: still moving left loop, second frame.</summary>
    TripperStillMovingLeftLoopSecond,
    /// <summary>Tripper: still moving left loop, third frame.</summary>
    TripperStillMovingLeftLoopThird,
    /// <summary>Tripper: still moving right loop, first frame.</summary>
    TripperStillMovingRightLoopFirst,
    /// <summary>Tripper: still moving right loop, second frame.</summary>
    TripperStillMovingRightLoopSecond,
    /// <summary>Tripper: still moving right loop, third frame.</summary>
    TripperStillMovingRightLoopThird,
    /// <summary>Tripper: frozen moving left.</summary>
    TripperFrozenMovingLeft,
    /// <summary>Tripper: frozen moving right.</summary>
    TripperFrozenMovingRight,
    /// <summary>Kamer: moving left loop, first frame.</summary>
    KamerMovingLeftLoopFirst,
    /// <summary>Kamer: moving left loop, second frame.</summary>
    KamerMovingLeftLoopSecond,
    /// <summary>Kamer: moving left loop, third frame.</summary>
    KamerMovingLeftLoopThird,
    /// <summary>Kamer: moving left loop, fourth frame.</summary>
    KamerMovingLeftLoopFourth,
    /// <summary>Dragon: body idle left.</summary>
    DragonBodyIdleLeft,
    /// <summary>Dragon: wings facing left, first frame.</summary>
    DragonWingsFacingLeftFirst,
    /// <summary>Dragon: wings facing left, second frame.</summary>
    DragonWingsFacingLeftSecond,
    /// <summary>Dragon: body idle right.</summary>
    DragonBodyIdleRight,
    /// <summary>Dragon: wings facing right, first frame.</summary>
    DragonWingsFacingRightFirst,
    /// <summary>Dragon: wings facing right, second frame.</summary>
    DragonWingsFacingRightSecond,
    /// <summary>Dragon: attacking facing left, first frame.</summary>
    DragonAttackingFacingLeftFirst,
    /// <summary>Dragon: attacking facing left, second frame.</summary>
    DragonAttackingFacingLeftSecond,
    /// <summary>Dragon: attacking facing left, third frame.</summary>
    DragonAttackingFacingLeftThird,
    /// <summary>Dragon: attacking facing right, first frame.</summary>
    DragonAttackingFacingRightFirst,
    /// <summary>Dragon: attacking facing right, second frame.</summary>
    DragonAttackingFacingRightSecond,
    /// <summary>Dragon: attacking facing right, third frame.</summary>
    DragonAttackingFacingRightThird,
    /// <summary>Multiviola: flying, first frame.</summary>
    MultiviolaFlyingFirst,
    /// <summary>Multiviola: flying, second frame.</summary>
    MultiviolaFlyingSecond,
    /// <summary>Multiviola: flying, third frame.</summary>
    MultiviolaFlyingThird,
    /// <summary>Multiviola: flying, fourth frame.</summary>
    MultiviolaFlyingFourth,
    /// <summary>Multiviola: flying, fifth frame.</summary>
    MultiviolaFlyingFifth,
    /// <summary>Multiviola: flying, sixth frame.</summary>
    MultiviolaFlyingSixth,
    /// <summary>Multiviola: flying, seventh frame.</summary>
    MultiviolaFlyingSeventh,
    /// <summary>Multiviola: flying, eighth frame.</summary>
    MultiviolaFlyingEighth,
    /// <summary>The Norfair lava jumper: hidden.</summary>
    NorfairLavaJumperHidden,
    /// <summary>The Norfair lava jumper: jump, second frame.</summary>
    NorfairLavaJumperJumpSecond,
    /// <summary>The Norfair lava jumper: jump, third frame.</summary>
    NorfairLavaJumperJumpThird,
    /// <summary>The Norfair lava jumper: jump, fourth frame.</summary>
    NorfairLavaJumperJumpFourth,
    /// <summary>The Norfair lava jumper: jump, fifth frame.</summary>
    NorfairLavaJumperJumpFifth,
    /// <summary>The Norfair lava jumper: jump, sixth frame.</summary>
    NorfairLavaJumperJumpSixth,
    /// <summary>The Norfair lava jumper: jump, seventh frame.</summary>
    NorfairLavaJumperJumpSeventh,
    /// <summary>The Norfair lava jumper: follower, first frame.</summary>
    NorfairLavaJumperFollowerFirst,
    /// <summary>The Norfair lava jumper: follower loop, first frame.</summary>
    NorfairLavaJumperFollowerLoopFirst,
    /// <summary>The Norfair lava jumper: follower loop, third frame.</summary>
    NorfairLavaJumperFollowerLoopThird,
    /// <summary>The Chozo statue: Lower Norfair initial.</summary>
    ChozoStatueLowerNorfairInitial,
    /// <summary>The Chozo statue: Lower Norfair activated, second frame.</summary>
    ChozoStatueLowerNorfairActivatedSecond,
    /// <summary>The Chozo statue: Lower Norfair activated, third frame.</summary>
    ChozoStatueLowerNorfairActivatedThird,
    /// <summary>The Chozo statue: Lower Norfair activated, fourth frame.</summary>
    ChozoStatueLowerNorfairActivatedFourth,
    /// <summary>The Chozo statue: Lower Norfair activated, fifth frame.</summary>
    ChozoStatueLowerNorfairActivatedFifth,
    /// <summary>The Chozo statue: Lower Norfair activated, sixth frame.</summary>
    ChozoStatueLowerNorfairActivatedSixth,
    /// <summary>The Chozo statue: Lower Norfair activated, seventh frame.</summary>
    ChozoStatueLowerNorfairActivatedSeventh,
    /// <summary>The Chozo statue: Lower Norfair activated, eighth frame.</summary>
    ChozoStatueLowerNorfairActivatedEighth,
    /// <summary>The Chozo statue: Lower Norfair activated, ninth frame.</summary>
    ChozoStatueLowerNorfairActivatedNinth,
    /// <summary>The Chozo statue: Wrecked Ship initial.</summary>
    ChozoStatueWreckedShipInitial,
    /// <summary>The Chozo statue: Wrecked Ship activated, second frame.</summary>
    ChozoStatueWreckedShipActivatedSecond,
    /// <summary>The Chozo statue: Wrecked Ship activated, third frame.</summary>
    ChozoStatueWreckedShipActivatedThird,
    /// <summary>The Chozo statue: Wrecked Ship activated, fourth frame.</summary>
    ChozoStatueWreckedShipActivatedFourth,
    /// <summary>The Chozo statue: Wrecked Ship activated, fifth frame.</summary>
    ChozoStatueWreckedShipActivatedFifth,
    /// <summary>The Chozo statue: Wrecked Ship activated, sixth frame.</summary>
    ChozoStatueWreckedShipActivatedSixth,
    /// <summary>The Chozo statue: Wrecked Ship activated, seventh frame.</summary>
    ChozoStatueWreckedShipActivatedSeventh,
    /// <summary>The Chozo statue: Wrecked Ship activated, eighth frame.</summary>
    ChozoStatueWreckedShipActivatedEighth,
    /// <summary>The Chozo statue: Wrecked Ship activated, ninth frame.</summary>
    ChozoStatueWreckedShipActivatedNinth,
    /// <summary>The Chozo statue: Wrecked Ship walking, first frame.</summary>
    ChozoStatueWreckedShipWalkingFirst,
    /// <summary>The Chozo statue: Wrecked Ship walking, second frame.</summary>
    ChozoStatueWreckedShipWalkingSecond,
    /// <summary>The Chozo statue: Wrecked Ship walking, third frame.</summary>
    ChozoStatueWreckedShipWalkingThird,
    /// <summary>The Chozo statue: Wrecked Ship walking, fourth frame.</summary>
    ChozoStatueWreckedShipWalkingFourth,
    /// <summary>The Chozo statue: Wrecked Ship walking, fifth frame.</summary>
    ChozoStatueWreckedShipWalkingFifth,
    /// <summary>The Chozo statue: Wrecked Ship walking, sixth frame.</summary>
    ChozoStatueWreckedShipWalkingSixth,
    /// <summary>The Chozo statue: Wrecked Ship walking, seventh frame.</summary>
    ChozoStatueWreckedShipWalkingSeventh,
    /// <summary>The Chozo statue: Wrecked Ship walking, eighth frame.</summary>
    ChozoStatueWreckedShipWalkingEighth,
    /// <summary>Viola: normal, first frame.</summary>
    ViolaNormalFirst,
    /// <summary>Viola: normal, second frame.</summary>
    ViolaNormalSecond,
    /// <summary>Viola: normal, third frame.</summary>
    ViolaNormalThird,
    /// <summary>Viola: normal, fourth frame.</summary>
    ViolaNormalFourth,
    /// <summary>Viola: normal, fifth frame.</summary>
    ViolaNormalFifth,
    /// <summary>Viola: normal, sixth frame.</summary>
    ViolaNormalSixth,
    /// <summary>Viola: normal, seventh frame.</summary>
    ViolaNormalSeventh,
    /// <summary>Viola: normal, eighth frame.</summary>
    ViolaNormalEighth,
    /// <summary>Rinka: ordinary.</summary>
    RinkaOrdinary,
    /// <summary>Rinka: ordinary loop, second frame.</summary>
    RinkaOrdinaryLoopSecond,
    /// <summary>Rinka: ordinary loop, third frame.</summary>
    RinkaOrdinaryLoopThird,
    /// <summary>Rinka: ordinary loop, fourth frame.</summary>
    RinkaOrdinaryLoopFourth,
    /// <summary>Rinka: ordinary loop, fifth frame.</summary>
    RinkaOrdinaryLoopFifth,
    /// <summary>The dead Torizo: stationary.</summary>
    DeadTorizoStationary,
    /// <summary>The dead Zoomer: corpse, first frame.</summary>
    DeadZoomerCorpseFirst,
    /// <summary>The dead Zoomer: corpse, second frame.</summary>
    DeadZoomerCorpseSecond,
    /// <summary>The dead Zoomer: corpse, third frame.</summary>
    DeadZoomerCorpseThird,
    /// <summary>The dead Ripper: corpse, first frame.</summary>
    DeadRipperCorpseFirst,
    /// <summary>The dead Ripper: corpse, second frame.</summary>
    DeadRipperCorpseSecond,
    /// <summary>The dead Skree: corpse, first frame.</summary>
    DeadSkreeCorpseFirst,
    /// <summary>The dead Skree: corpse, second frame.</summary>
    DeadSkreeCorpseSecond,
    /// <summary>The dead Skree: corpse, third frame.</summary>
    DeadSkreeCorpseThird,
    /// <summary>The dead Sidehopper: hop body.</summary>
    DeadSidehopperHopBody,
    /// <summary>The dead Sidehopper: hop alt.</summary>
    DeadSidehopperHopAlt,
    /// <summary>The dead Sidehopper: idle.</summary>
    DeadSidehopperIdle,
    /// <summary>The dead Sidehopper: corpse.</summary>
    DeadSidehopperCorpse,
    /// <summary>The dead Sidehopper: initially dead.</summary>
    DeadSidehopperInitiallyDead,
    /// <summary>Mochtroid: free flight, first frame.</summary>
    MochtroidFreeFlightFirst,
    /// <summary>Mochtroid: free flight, second frame.</summary>
    MochtroidFreeFlightSecond,
    /// <summary>Mochtroid: free flight, third frame.</summary>
    MochtroidFreeFlightThird,
    /// <summary>Mochtroid: attached, first frame.</summary>
    MochtroidAttachedFirst,
    /// <summary>Mochtroid: attached, second frame.</summary>
    MochtroidAttachedSecond,
    /// <summary>Mochtroid: attached, third frame.</summary>
    MochtroidAttachedThird,
    /// <summary>Evir: body facing left, first frame.</summary>
    EvirBodyFacingLeftFirst,
    /// <summary>Evir: body facing left, second frame.</summary>
    EvirBodyFacingLeftSecond,
    /// <summary>Evir: body facing left, third frame.</summary>
    EvirBodyFacingLeftThird,
    /// <summary>Evir: body facing left, fourth frame.</summary>
    EvirBodyFacingLeftFourth,
    /// <summary>Evir: body facing left, fifth frame.</summary>
    EvirBodyFacingLeftFifth,
    /// <summary>Evir: body facing left, sixth frame.</summary>
    EvirBodyFacingLeftSixth,
    /// <summary>Evir: arms facing left, first frame.</summary>
    EvirArmsFacingLeftFirst,
    /// <summary>Evir: arms facing left, second frame.</summary>
    EvirArmsFacingLeftSecond,
    /// <summary>Evir: arms facing left, third frame.</summary>
    EvirArmsFacingLeftThird,
    /// <summary>Evir: arms facing left, fourth frame.</summary>
    EvirArmsFacingLeftFourth,
    /// <summary>Evir: arms facing left, fifth frame.</summary>
    EvirArmsFacingLeftFifth,
    /// <summary>Evir: body facing right, first frame.</summary>
    EvirBodyFacingRightFirst,
    /// <summary>Evir: body facing right, second frame.</summary>
    EvirBodyFacingRightSecond,
    /// <summary>Evir: body facing right, third frame.</summary>
    EvirBodyFacingRightThird,
    /// <summary>Evir: body facing right, fourth frame.</summary>
    EvirBodyFacingRightFourth,
    /// <summary>Evir: body facing right, fifth frame.</summary>
    EvirBodyFacingRightFifth,
    /// <summary>Evir: body facing right, sixth frame.</summary>
    EvirBodyFacingRightSixth,
    /// <summary>Evir: arms facing right, first frame.</summary>
    EvirArmsFacingRightFirst,
    /// <summary>Evir: arms facing right, second frame.</summary>
    EvirArmsFacingRightSecond,
    /// <summary>Evir: arms facing right, third frame.</summary>
    EvirArmsFacingRightThird,
    /// <summary>Evir: arms facing right, fourth frame.</summary>
    EvirArmsFacingRightFourth,
    /// <summary>Evir: arms facing right, fifth frame.</summary>
    EvirArmsFacingRightFifth,
    /// <summary>Evir: projectile flight.</summary>
    EvirProjectileFlight,
    /// <summary>Evir: projectile regenerating.</summary>
    EvirProjectileRegenerating,
    /// <summary>The work robot: facing left walking forwards loop, twelfth frame.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopTwelfth,
    /// <summary>The work robot: initial.</summary>
    WorkRobotInitial,
    /// <summary>The work robot: facing left walking forwards loop, second frame.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopSecond,
    /// <summary>The work robot: facing left walking forwards loop, third frame.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopThird,
    /// <summary>The work robot: facing left walking forwards loop, fourth frame.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopFourth,
    /// <summary>The work robot: facing left walking forwards loop, fifth frame.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopFifth,
    /// <summary>The work robot: facing left walking forwards loop, sixth frame.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopSixth,
    /// <summary>The work robot: facing left walking forwards loop, seventh frame.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopSeventh,
    /// <summary>The work robot: facing left walking forwards loop, eighth frame.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopEighth,
    /// <summary>The work robot: facing left walking forwards loop, ninth frame.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopNinth,
    /// <summary>The work robot: facing left walking forwards loop, tenth frame.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopTenth,
    /// <summary>The work robot: facing left walking forwards loop, eleventh frame.</summary>
    WorkRobotFacingLeftWalkingForwardsLoopEleventh,
    /// <summary>The work robot: approaching fall right, first frame.</summary>
    WorkRobotApproachingFallRightFirst,
    /// <summary>The work robot: approaching fall right, second frame.</summary>
    WorkRobotApproachingFallRightSecond,
    /// <summary>The work robot: approaching fall right, third frame.</summary>
    WorkRobotApproachingFallRightThird,
    /// <summary>The work robot: facing right walking forwards loop, fourth frame.</summary>
    WorkRobotFacingRightWalkingForwardsLoopFourth,
    /// <summary>The work robot: facing right walking forwards loop, fifth frame.</summary>
    WorkRobotFacingRightWalkingForwardsLoopFifth,
    /// <summary>The work robot: facing right walking forwards loop, sixth frame.</summary>
    WorkRobotFacingRightWalkingForwardsLoopSixth,
    /// <summary>The work robot: facing right walking forwards loop, seventh frame.</summary>
    WorkRobotFacingRightWalkingForwardsLoopSeventh,
    /// <summary>The work robot: facing right walking forwards loop, eighth frame.</summary>
    WorkRobotFacingRightWalkingForwardsLoopEighth,
    /// <summary>The work robot: facing right walking forwards loop, ninth frame.</summary>
    WorkRobotFacingRightWalkingForwardsLoopNinth,
    /// <summary>The work robot: facing right walking forwards loop, tenth frame.</summary>
    WorkRobotFacingRightWalkingForwardsLoopTenth,
    /// <summary>The work robot: facing right walking forwards loop, eleventh frame.</summary>
    WorkRobotFacingRightWalkingForwardsLoopEleventh,
    /// <summary>The work robot: facing right walking forwards loop, twelfth frame.</summary>
    WorkRobotFacingRightWalkingForwardsLoopTwelfth,
    /// <summary>The work robot: unpowered neutral.</summary>
    WorkRobotUnpoweredNeutral,
    /// <summary>The work robot: unpowered left.</summary>
    WorkRobotUnpoweredLeft,
    /// <summary>The work robot: unpowered right.</summary>
    WorkRobotUnpoweredRight,
    /// <summary>Yard: outside turn upside right moving up, first frame.</summary>
    YardOutsideTurnUpsideRightMovingUpFirst,
    /// <summary>Yard: outside turn upside right moving up, second frame.</summary>
    YardOutsideTurnUpsideRightMovingUpSecond,
    /// <summary>Yard: outside turn upside right moving up, third frame.</summary>
    YardOutsideTurnUpsideRightMovingUpThird,
    /// <summary>Yard: crawling upside up moving left, first frame.</summary>
    YardCrawlingUpsideUpMovingLeftFirst,
    /// <summary>Yard: crawling upside up moving left, second frame.</summary>
    YardCrawlingUpsideUpMovingLeftSecond,
    /// <summary>Yard: crawling upside up moving left, third frame.</summary>
    YardCrawlingUpsideUpMovingLeftThird,
    /// <summary>Yard: outside turn upside up moving left, first frame.</summary>
    YardOutsideTurnUpsideUpMovingLeftFirst,
    /// <summary>Yard: outside turn upside up moving left, second frame.</summary>
    YardOutsideTurnUpsideUpMovingLeftSecond,
    /// <summary>Yard: outside turn upside up moving left, third frame.</summary>
    YardOutsideTurnUpsideUpMovingLeftThird,
    /// <summary>Yard: crawling upside left moving down, first frame.</summary>
    YardCrawlingUpsideLeftMovingDownFirst,
    /// <summary>Yard: crawling upside left moving down, second frame.</summary>
    YardCrawlingUpsideLeftMovingDownSecond,
    /// <summary>Yard: crawling upside left moving down, third frame.</summary>
    YardCrawlingUpsideLeftMovingDownThird,
    /// <summary>Yard: outside turn upside left moving down, first frame.</summary>
    YardOutsideTurnUpsideLeftMovingDownFirst,
    /// <summary>Yard: outside turn upside left moving down, second frame.</summary>
    YardOutsideTurnUpsideLeftMovingDownSecond,
    /// <summary>Yard: outside turn upside left moving down, third frame.</summary>
    YardOutsideTurnUpsideLeftMovingDownThird,
    /// <summary>Yard: crawling upside down moving right, first frame.</summary>
    YardCrawlingUpsideDownMovingRightFirst,
    /// <summary>Yard: crawling upside down moving right, second frame.</summary>
    YardCrawlingUpsideDownMovingRightSecond,
    /// <summary>Yard: crawling upside down moving right, third frame.</summary>
    YardCrawlingUpsideDownMovingRightThird,
    /// <summary>Yard: outside turn upside down moving right, first frame.</summary>
    YardOutsideTurnUpsideDownMovingRightFirst,
    /// <summary>Yard: outside turn upside down moving right, second frame.</summary>
    YardOutsideTurnUpsideDownMovingRightSecond,
    /// <summary>Yard: outside turn upside down moving right, third frame.</summary>
    YardOutsideTurnUpsideDownMovingRightThird,
    /// <summary>Yard: crawling upside right moving up, first frame.</summary>
    YardCrawlingUpsideRightMovingUpFirst,
    /// <summary>Yard: crawling upside right moving up, second frame.</summary>
    YardCrawlingUpsideRightMovingUpSecond,
    /// <summary>Yard: crawling upside right moving up, third frame.</summary>
    YardCrawlingUpsideRightMovingUpThird,
    /// <summary>Yard: outside turn upside left moving up, first frame.</summary>
    YardOutsideTurnUpsideLeftMovingUpFirst,
    /// <summary>Yard: outside turn upside left moving up, second frame.</summary>
    YardOutsideTurnUpsideLeftMovingUpSecond,
    /// <summary>Yard: outside turn upside left moving up, third frame.</summary>
    YardOutsideTurnUpsideLeftMovingUpThird,
    /// <summary>Yard: crawling upside up moving right, first frame.</summary>
    YardCrawlingUpsideUpMovingRightFirst,
    /// <summary>Yard: crawling upside up moving right, second frame.</summary>
    YardCrawlingUpsideUpMovingRightSecond,
    /// <summary>Yard: crawling upside up moving right, third frame.</summary>
    YardCrawlingUpsideUpMovingRightThird,
    /// <summary>Yard: outside turn upside up moving right, first frame.</summary>
    YardOutsideTurnUpsideUpMovingRightFirst,
    /// <summary>Yard: outside turn upside up moving right, second frame.</summary>
    YardOutsideTurnUpsideUpMovingRightSecond,
    /// <summary>Yard: outside turn upside up moving right, third frame.</summary>
    YardOutsideTurnUpsideUpMovingRightThird,
    /// <summary>Yard: crawling upside right moving down, first frame.</summary>
    YardCrawlingUpsideRightMovingDownFirst,
    /// <summary>Yard: crawling upside right moving down, second frame.</summary>
    YardCrawlingUpsideRightMovingDownSecond,
    /// <summary>Yard: crawling upside right moving down, third frame.</summary>
    YardCrawlingUpsideRightMovingDownThird,
    /// <summary>Yard: outside turn upside right moving down, first frame.</summary>
    YardOutsideTurnUpsideRightMovingDownFirst,
    /// <summary>Yard: outside turn upside right moving down, second frame.</summary>
    YardOutsideTurnUpsideRightMovingDownSecond,
    /// <summary>Yard: outside turn upside right moving down, third frame.</summary>
    YardOutsideTurnUpsideRightMovingDownThird,
    /// <summary>Yard: crawling upside down moving left, first frame.</summary>
    YardCrawlingUpsideDownMovingLeftFirst,
    /// <summary>Yard: crawling upside down moving left, second frame.</summary>
    YardCrawlingUpsideDownMovingLeftSecond,
    /// <summary>Yard: crawling upside down moving left, third frame.</summary>
    YardCrawlingUpsideDownMovingLeftThird,
    /// <summary>Yard: outside turn upside down moving left, first frame.</summary>
    YardOutsideTurnUpsideDownMovingLeftFirst,
    /// <summary>Yard: outside turn upside down moving left, second frame.</summary>
    YardOutsideTurnUpsideDownMovingLeftSecond,
    /// <summary>Yard: outside turn upside down moving left, third frame.</summary>
    YardOutsideTurnUpsideDownMovingLeftThird,
    /// <summary>Yard: crawling upside left moving up, first frame.</summary>
    YardCrawlingUpsideLeftMovingUpFirst,
    /// <summary>Yard: crawling upside left moving up, second frame.</summary>
    YardCrawlingUpsideLeftMovingUpSecond,
    /// <summary>Yard: crawling upside left moving up, third frame.</summary>
    YardCrawlingUpsideLeftMovingUpThird,
    /// <summary>Yard: inside turn upside up moving left, first frame.</summary>
    YardInsideTurnUpsideUpMovingLeftFirst,
    /// <summary>Yard: inside turn upside up moving left, second frame.</summary>
    YardInsideTurnUpsideUpMovingLeftSecond,
    /// <summary>Yard: inside turn upside up moving left, third frame.</summary>
    YardInsideTurnUpsideUpMovingLeftThird,
    /// <summary>Yard: inside turn upside right moving up, first frame.</summary>
    YardInsideTurnUpsideRightMovingUpFirst,
    /// <summary>Yard: inside turn upside right moving up, second frame.</summary>
    YardInsideTurnUpsideRightMovingUpSecond,
    /// <summary>Yard: inside turn upside right moving up, third frame.</summary>
    YardInsideTurnUpsideRightMovingUpThird,
    /// <summary>Yard: inside turn upside down moving right, first frame.</summary>
    YardInsideTurnUpsideDownMovingRightFirst,
    /// <summary>Yard: inside turn upside down moving right, second frame.</summary>
    YardInsideTurnUpsideDownMovingRightSecond,
    /// <summary>Yard: inside turn upside down moving right, third frame.</summary>
    YardInsideTurnUpsideDownMovingRightThird,
    /// <summary>Yard: inside turn upside left moving down, first frame.</summary>
    YardInsideTurnUpsideLeftMovingDownFirst,
    /// <summary>Yard: inside turn upside left moving down, second frame.</summary>
    YardInsideTurnUpsideLeftMovingDownSecond,
    /// <summary>Yard: inside turn upside left moving down, third frame.</summary>
    YardInsideTurnUpsideLeftMovingDownThird,
    /// <summary>Yard: inside turn upside up moving right, first frame.</summary>
    YardInsideTurnUpsideUpMovingRightFirst,
    /// <summary>Yard: inside turn upside up moving right, second frame.</summary>
    YardInsideTurnUpsideUpMovingRightSecond,
    /// <summary>Yard: inside turn upside up moving right, third frame.</summary>
    YardInsideTurnUpsideUpMovingRightThird,
    /// <summary>Yard: inside turn upside left moving up, first frame.</summary>
    YardInsideTurnUpsideLeftMovingUpFirst,
    /// <summary>Yard: inside turn upside left moving up, second frame.</summary>
    YardInsideTurnUpsideLeftMovingUpSecond,
    /// <summary>Yard: inside turn upside left moving up, third frame.</summary>
    YardInsideTurnUpsideLeftMovingUpThird,
    /// <summary>Yard: inside turn upside down moving left, first frame.</summary>
    YardInsideTurnUpsideDownMovingLeftFirst,
    /// <summary>Yard: inside turn upside down moving left, second frame.</summary>
    YardInsideTurnUpsideDownMovingLeftSecond,
    /// <summary>Yard: inside turn upside down moving left, third frame.</summary>
    YardInsideTurnUpsideDownMovingLeftThird,
    /// <summary>Yard: inside turn upside right moving down, first frame.</summary>
    YardInsideTurnUpsideRightMovingDownFirst,
    /// <summary>Yard: inside turn upside right moving down, second frame.</summary>
    YardInsideTurnUpsideRightMovingDownSecond,
    /// <summary>Yard: inside turn upside right moving down, third frame.</summary>
    YardInsideTurnUpsideRightMovingDownThird,
    /// <summary>Yard: hiding upside up moving left, first frame.</summary>
    YardHidingUpsideUpMovingLeftFirst,
    /// <summary>Yard: hiding upside up moving left, second frame.</summary>
    YardHidingUpsideUpMovingLeftSecond,
    /// <summary>Yard: hidden upside up moving left, first frame.</summary>
    YardHiddenUpsideUpMovingLeftFirst,
    /// <summary>Yard: hidden upside up moving left, second frame.</summary>
    YardHiddenUpsideUpMovingLeftSecond,
    /// <summary>Yard: hiding upside down moving left, first frame.</summary>
    YardHidingUpsideDownMovingLeftFirst,
    /// <summary>Yard: hiding upside down moving left, second frame.</summary>
    YardHidingUpsideDownMovingLeftSecond,
    /// <summary>Yard: hiding upside down moving left, third frame.</summary>
    YardHidingUpsideDownMovingLeftThird,
    /// <summary>Yard: hiding upside down moving left, fourth frame.</summary>
    YardHidingUpsideDownMovingLeftFourth,
    /// <summary>Yard: hiding upside down moving right, first frame.</summary>
    YardHidingUpsideDownMovingRightFirst,
    /// <summary>Yard: hiding upside down moving right, second frame.</summary>
    YardHidingUpsideDownMovingRightSecond,
    /// <summary>Yard: hiding upside down moving right, third frame.</summary>
    YardHidingUpsideDownMovingRightThird,
    /// <summary>Yard: hiding upside down moving right, fourth frame.</summary>
    YardHidingUpsideDownMovingRightFourth,
    /// <summary>Yard: hiding upside up moving right, first frame.</summary>
    YardHidingUpsideUpMovingRightFirst,
    /// <summary>Yard: hiding upside up moving right, second frame.</summary>
    YardHidingUpsideUpMovingRightSecond,
    /// <summary>Yard: hidden upside up moving right, first frame.</summary>
    YardHiddenUpsideUpMovingRightFirst,
    /// <summary>Yard: hidden upside up moving right, second frame.</summary>
    YardHiddenUpsideUpMovingRightSecond,
    /// <summary>Yard: hiding upside right moving up, first frame.</summary>
    YardHidingUpsideRightMovingUpFirst,
    /// <summary>Yard: hiding upside right moving up, second frame.</summary>
    YardHidingUpsideRightMovingUpSecond,
    /// <summary>Yard: hiding upside right moving up, third frame.</summary>
    YardHidingUpsideRightMovingUpThird,
    /// <summary>Yard: hiding upside right moving up, fourth frame.</summary>
    YardHidingUpsideRightMovingUpFourth,
    /// <summary>Yard: hiding upside left moving up, first frame.</summary>
    YardHidingUpsideLeftMovingUpFirst,
    /// <summary>Yard: hiding upside left moving up, second frame.</summary>
    YardHidingUpsideLeftMovingUpSecond,
    /// <summary>Yard: hiding upside left moving up, third frame.</summary>
    YardHidingUpsideLeftMovingUpThird,
    /// <summary>Yard: hiding upside left moving up, fourth frame.</summary>
    YardHidingUpsideLeftMovingUpFourth,
    /// <summary>Yard: hiding upside left moving down, first frame.</summary>
    YardHidingUpsideLeftMovingDownFirst,
    /// <summary>Yard: hiding upside left moving down, second frame.</summary>
    YardHidingUpsideLeftMovingDownSecond,
    /// <summary>Yard: hiding upside left moving down, third frame.</summary>
    YardHidingUpsideLeftMovingDownThird,
    /// <summary>Yard: hiding upside left moving down, fourth frame.</summary>
    YardHidingUpsideLeftMovingDownFourth,
    /// <summary>Yard: hiding upside right moving down, first frame.</summary>
    YardHidingUpsideRightMovingDownFirst,
    /// <summary>Yard: hiding upside right moving down, second frame.</summary>
    YardHidingUpsideRightMovingDownSecond,
    /// <summary>Yard: hiding upside right moving down, third frame.</summary>
    YardHidingUpsideRightMovingDownThird,
    /// <summary>Yard: hiding upside right moving down, fourth frame.</summary>
    YardHidingUpsideRightMovingDownFourth,
    /// <summary>Botwoon: head closed up left.</summary>
    BotwoonHeadClosedUpLeft,
    /// <summary>Botwoon: head closed left.</summary>
    BotwoonHeadClosedLeft,
    /// <summary>Botwoon: head closed down left.</summary>
    BotwoonHeadClosedDownLeft,
    /// <summary>Botwoon: head closed down.</summary>
    BotwoonHeadClosedDown,
    /// <summary>Botwoon: head closed down right.</summary>
    BotwoonHeadClosedDownRight,
    /// <summary>Botwoon: head closed right.</summary>
    BotwoonHeadClosedRight,
    /// <summary>Botwoon: head closed up right.</summary>
    BotwoonHeadClosedUpRight,
    /// <summary>Botwoon: head closed up.</summary>
    BotwoonHeadClosedUp,
    /// <summary>Botwoon: head open up left.</summary>
    BotwoonHeadOpenUpLeft,
    /// <summary>Botwoon: head open left.</summary>
    BotwoonHeadOpenLeft,
    /// <summary>Botwoon: head open down left.</summary>
    BotwoonHeadOpenDownLeft,
    /// <summary>Botwoon: head open down.</summary>
    BotwoonHeadOpenDown,
    /// <summary>Botwoon: head open down right.</summary>
    BotwoonHeadOpenDownRight,
    /// <summary>Botwoon: head open right.</summary>
    BotwoonHeadOpenRight,
    /// <summary>Botwoon: head open up right.</summary>
    BotwoonHeadOpenUpRight,
    /// <summary>Botwoon: head open up.</summary>
    BotwoonHeadOpenUp,
    /// <summary>Samus's gunship: entrance pad opening, first frame.</summary>
    GunshipEntrancePadOpeningFirst,
    /// <summary>Samus's gunship: entrance pad opening, second frame.</summary>
    GunshipEntrancePadOpeningSecond,
    /// <summary>Samus's gunship: entrance pad opening, third frame.</summary>
    GunshipEntrancePadOpeningThird,
    /// <summary>Samus's gunship: entrance pad opening, fourth frame.</summary>
    GunshipEntrancePadOpeningFourth,
    /// <summary>Samus's gunship: entrance pad opening, fifth frame.</summary>
    GunshipEntrancePadOpeningFifth,
    /// <summary>Samus's gunship: entrance pad opening, sixth frame.</summary>
    GunshipEntrancePadOpeningSixth,
    /// <summary>Samus's gunship: entrance pad opening, seventh frame.</summary>
    GunshipEntrancePadOpeningSeventh,
    /// <summary>Samus's gunship: entrance pad opening, eighth frame.</summary>
    GunshipEntrancePadOpeningEighth,
    /// <summary>Samus's gunship: entrance pad opening, ninth frame.</summary>
    GunshipEntrancePadOpeningNinth,
    /// <summary>Samus's gunship: entrance pad opening, tenth frame.</summary>
    GunshipEntrancePadOpeningTenth,
    /// <summary>Samus's gunship: entrance pad open.</summary>
    GunshipEntrancePadOpen,
    /// <summary>Samus's gunship: top hull.</summary>
    GunshipTopHull,
    /// <summary>Samus's gunship: bottom hull.</summary>
    GunshipBottomHull,
    /// <summary>The baby turtle: crawling left, first frame.</summary>
    TurtleBabyCrawlingLeftFirst,
    /// <summary>The baby turtle: crawling left, second frame.</summary>
    TurtleBabyCrawlingLeftSecond,
    /// <summary>The baby turtle: crawling left, third frame.</summary>
    TurtleBabyCrawlingLeftThird,
    /// <summary>The baby turtle: crawling left, fourth frame.</summary>
    TurtleBabyCrawlingLeftFourth,
    /// <summary>The baby turtle: crawling left, fifth frame.</summary>
    TurtleBabyCrawlingLeftFifth,
    /// <summary>The baby turtle: crawling left, sixth frame.</summary>
    TurtleBabyCrawlingLeftSixth,
    /// <summary>The baby turtle: crawling left, seventh frame.</summary>
    TurtleBabyCrawlingLeftSeventh,
    /// <summary>The baby turtle: spinning, first frame.</summary>
    TurtleBabySpinningFirst,
    /// <summary>The baby turtle: spinning, second frame.</summary>
    TurtleBabySpinningSecond,
    /// <summary>The baby turtle: spinning, third frame.</summary>
    TurtleBabySpinningThird,
    /// <summary>The baby turtle: spinning, fourth frame.</summary>
    TurtleBabySpinningFourth,
    /// <summary>The mama turtle: spinning, first frame.</summary>
    TurtleMamaSpinningFirst,
    /// <summary>The mama turtle: spinning, second frame.</summary>
    TurtleMamaSpinningSecond,
    /// <summary>The mama turtle: spinning, third frame.</summary>
    TurtleMamaSpinningThird,
    /// <summary>The mama turtle: enter shell left, first frame.</summary>
    TurtleMamaEnterShellLeftFirst,
    /// <summary>The mama turtle: enter shell left, second frame.</summary>
    TurtleMamaEnterShellLeftSecond,
    /// <summary>The mama turtle: enter shell left, third frame.</summary>
    TurtleMamaEnterShellLeftThird,
    /// <summary>The baby turtle: crawling right, first frame.</summary>
    TurtleBabyCrawlingRightFirst,
    /// <summary>The baby turtle: crawling right, second frame.</summary>
    TurtleBabyCrawlingRightSecond,
    /// <summary>The baby turtle: crawling right, third frame.</summary>
    TurtleBabyCrawlingRightThird,
    /// <summary>The baby turtle: crawling right, fourth frame.</summary>
    TurtleBabyCrawlingRightFourth,
    /// <summary>The baby turtle: crawling right, fifth frame.</summary>
    TurtleBabyCrawlingRightFifth,
    /// <summary>The baby turtle: crawling right, sixth frame.</summary>
    TurtleBabyCrawlingRightSixth,
    /// <summary>The baby turtle: crawling right, seventh frame.</summary>
    TurtleBabyCrawlingRightSeventh,
    /// <summary>The mama turtle: enter shell right, first frame.</summary>
    TurtleMamaEnterShellRightFirst,
    /// <summary>The mama turtle: enter shell right, second frame.</summary>
    TurtleMamaEnterShellRightSecond,
    /// <summary>The mama turtle: enter shell right, third frame.</summary>
    TurtleMamaEnterShellRightThird,
    /// <summary>The mama turtle: enter shell right, fourth frame.</summary>
    TurtleMamaEnterShellRightFourth,
    /// <summary>The baby turtle: hiding right, third frame.</summary>
    TurtleBabyHidingRightThird,
    /// <summary>Zero: upside right loop, first frame.</summary>
    ZeroUpsideRightLoopFirst,
    /// <summary>Zero: upside right loop, second frame.</summary>
    ZeroUpsideRightLoopSecond,
    /// <summary>Zero: upside right loop, third frame.</summary>
    ZeroUpsideRightLoopThird,
    /// <summary>Zero: upside right loop, fourth frame.</summary>
    ZeroUpsideRightLoopFourth,
    /// <summary>Zero: upside left loop, first frame.</summary>
    ZeroUpsideLeftLoopFirst,
    /// <summary>Zero: upside left loop, second frame.</summary>
    ZeroUpsideLeftLoopSecond,
    /// <summary>Zero: upside left loop, third frame.</summary>
    ZeroUpsideLeftLoopThird,
    /// <summary>Zero: upside left loop, fourth frame.</summary>
    ZeroUpsideLeftLoopFourth,
    /// <summary>Zero: upside down loop, first frame.</summary>
    ZeroUpsideDownLoopFirst,
    /// <summary>Zero: upside down loop, second frame.</summary>
    ZeroUpsideDownLoopSecond,
    /// <summary>Zero: upside down loop, third frame.</summary>
    ZeroUpsideDownLoopThird,
    /// <summary>Zero: upside down loop, fourth frame.</summary>
    ZeroUpsideDownLoopFourth,
    /// <summary>Zero: upside up loop, first frame.</summary>
    ZeroUpsideUpLoopFirst,
    /// <summary>Zero: upside up loop, second frame.</summary>
    ZeroUpsideUpLoopSecond,
    /// <summary>Zero: upside up loop, third frame.</summary>
    ZeroUpsideUpLoopThird,
    /// <summary>Zero: upside up loop, fourth frame.</summary>
    ZeroUpsideUpLoopFourth,
    /// <summary>Etecoon: look right at Samus and run left.</summary>
    EtecoonLookRightAtSamusAndRunLeft,
    /// <summary>Etecoon: begin running left.</summary>
    EtecoonBeginRunningLeft,
    /// <summary>Etecoon: running left, first frame.</summary>
    EtecoonRunningLeftFirst,
    /// <summary>Etecoon: running left, second frame.</summary>
    EtecoonRunningLeftSecond,
    /// <summary>Etecoon: running left, third frame.</summary>
    EtecoonRunningLeftThird,
    /// <summary>Etecoon: wall jump left.</summary>
    EtecoonWallJumpLeft,
    /// <summary>Etecoon: wall jump left loop, first frame.</summary>
    EtecoonWallJumpLeftLoopFirst,
    /// <summary>Etecoon: wall jump left loop, second frame.</summary>
    EtecoonWallJumpLeftLoopSecond,
    /// <summary>Etecoon: wall jump left loop, third frame.</summary>
    EtecoonWallJumpLeftLoopThird,
    /// <summary>Etecoon: wall jump left loop, fourth frame.</summary>
    EtecoonWallJumpLeftLoopFourth,
    /// <summary>Etecoon: hopping facing left.</summary>
    EtecoonHoppingFacingLeft,
    /// <summary>Etecoon: continue hopping facing left, first frame.</summary>
    EtecoonContinueHoppingFacingLeftFirst,
    /// <summary>Etecoon: continue hopping facing left, second frame.</summary>
    EtecoonContinueHoppingFacingLeftSecond,
    /// <summary>Etecoon: hit ceiling, first frame.</summary>
    EtecoonHitCeilingFirst,
    /// <summary>Etecoon: wall jump left eligible.</summary>
    EtecoonWallJumpLeftEligible,
    /// <summary>Etecoon: look left at Samus and run right.</summary>
    EtecoonLookLeftAtSamusAndRunRight,
    /// <summary>Etecoon: begin running right.</summary>
    EtecoonBeginRunningRight,
    /// <summary>Etecoon: running right, first frame.</summary>
    EtecoonRunningRightFirst,
    /// <summary>Etecoon: running right, second frame.</summary>
    EtecoonRunningRightSecond,
    /// <summary>Etecoon: running right, third frame.</summary>
    EtecoonRunningRightThird,
    /// <summary>Etecoon: wall jump right.</summary>
    EtecoonWallJumpRight,
    /// <summary>Etecoon: jumping right, first frame.</summary>
    EtecoonJumpingRightFirst,
    /// <summary>Etecoon: jumping right, second frame.</summary>
    EtecoonJumpingRightSecond,
    /// <summary>Etecoon: jumping right, third frame.</summary>
    EtecoonJumpingRightThird,
    /// <summary>Etecoon: jumping right, fourth frame.</summary>
    EtecoonJumpingRightFourth,
    /// <summary>Etecoon: hopping facing right.</summary>
    EtecoonHoppingFacingRight,
    /// <summary>Etecoon: continue hopping facing right, first frame.</summary>
    EtecoonContinueHoppingFacingRightFirst,
    /// <summary>Etecoon: continue hopping facing right, second frame.</summary>
    EtecoonContinueHoppingFacingRightSecond,
    /// <summary>Etecoon: continue hopping facing right, third frame.</summary>
    EtecoonContinueHoppingFacingRightThird,
    /// <summary>Etecoon: wall jump right eligible.</summary>
    EtecoonWallJumpRightEligible,
    /// <summary>Etecoon: flexing loop, fifth frame.</summary>
    EtecoonFlexingLoopFifth,
    /// <summary>Dachora: running left, first frame.</summary>
    DachoraRunningLeftFirst,
    /// <summary>Dachora: running left, second frame.</summary>
    DachoraRunningLeftSecond,
    /// <summary>Dachora: running left, third frame.</summary>
    DachoraRunningLeftThird,
    /// <summary>Dachora: running left, fourth frame.</summary>
    DachoraRunningLeftFourth,
    /// <summary>Dachora: running left, fifth frame.</summary>
    DachoraRunningLeftFifth,
    /// <summary>Dachora: running left, sixth frame.</summary>
    DachoraRunningLeftSixth,
    /// <summary>Dachora: idle left, first frame.</summary>
    DachoraIdleLeftFirst,
    /// <summary>Dachora: idle left, second frame.</summary>
    DachoraIdleLeftSecond,
    /// <summary>Dachora: idle left, third frame.</summary>
    DachoraIdleLeftThird,
    /// <summary>Dachora: idle left, fourth frame.</summary>
    DachoraIdleLeftFourth,
    /// <summary>Dachora: idle left, fifth frame.</summary>
    DachoraIdleLeftFifth,
    /// <summary>Dachora: blink left, second frame.</summary>
    DachoraBlinkLeftSecond,
    /// <summary>Dachora: blink left, third frame.</summary>
    DachoraBlinkLeftThird,
    /// <summary>Dachora: echo left.</summary>
    DachoraEchoLeft,
    /// <summary>Dachora: running right, first frame.</summary>
    DachoraRunningRightFirst,
    /// <summary>Dachora: running right, second frame.</summary>
    DachoraRunningRightSecond,
    /// <summary>Dachora: running right, third frame.</summary>
    DachoraRunningRightThird,
    /// <summary>Dachora: running right, fourth frame.</summary>
    DachoraRunningRightFourth,
    /// <summary>Dachora: running right, fifth frame.</summary>
    DachoraRunningRightFifth,
    /// <summary>Dachora: running right, sixth frame.</summary>
    DachoraRunningRightSixth,
    /// <summary>Dachora: idle right, first frame.</summary>
    DachoraIdleRightFirst,
    /// <summary>Dachora: idle right, second frame.</summary>
    DachoraIdleRightSecond,
    /// <summary>Dachora: idle right, third frame.</summary>
    DachoraIdleRightThird,
    /// <summary>Dachora: idle right, fourth frame.</summary>
    DachoraIdleRightFourth,
    /// <summary>Dachora: idle right, fifth frame.</summary>
    DachoraIdleRightFifth,
    /// <summary>Dachora: blink right, second frame.</summary>
    DachoraBlinkRightSecond,
    /// <summary>Dachora: blink right, third frame.</summary>
    DachoraBlinkRightThird,
    /// <summary>Dachora: charge right.</summary>
    DachoraChargeRight,
    /// <summary>Dachora: echo right.</summary>
    DachoraEchoRight,
    /// <summary>The escaping Etecoons: running left low tide loop, first frame.</summary>
    EscapeEtecoonRunningLeftLowTideLoopFirst,
    /// <summary>The escaping Etecoons: running left low tide loop, second frame.</summary>
    EscapeEtecoonRunningLeftLowTideLoopSecond,
    /// <summary>The escaping Etecoons: running left low tide loop, third frame.</summary>
    EscapeEtecoonRunningLeftLowTideLoopThird,
    /// <summary>The escaping Etecoons: running right low tide loop, first frame.</summary>
    EscapeEtecoonRunningRightLowTideLoopFirst,
    /// <summary>The escaping Etecoons: running right low tide loop, second frame.</summary>
    EscapeEtecoonRunningRightLowTideLoopSecond,
    /// <summary>The escaping Etecoons: running right low tide loop, third frame.</summary>
    EscapeEtecoonRunningRightLowTideLoopThird,
    /// <summary>The escaping Etecoons: stationary, first frame.</summary>
    EscapeEtecoonStationaryFirst,
    /// <summary>The escaping Etecoons: stationary, second frame.</summary>
    EscapeEtecoonStationarySecond,
    /// <summary>The escaping Etecoons: stationary, third frame.</summary>
    EscapeEtecoonStationaryThird,
    /// <summary>The escaping Etecoons: stationary, fourth frame.</summary>
    EscapeEtecoonStationaryFourth,
    /// <summary>The escaping Dachora: running around low tide left, first frame.</summary>
    EscapeDachoraRunningAroundLowTideLeftFirst,
    /// <summary>The escaping Dachora: running around low tide left, second frame.</summary>
    EscapeDachoraRunningAroundLowTideLeftSecond,
    /// <summary>The escaping Dachora: running around low tide left, third frame.</summary>
    EscapeDachoraRunningAroundLowTideLeftThird,
    /// <summary>The escaping Dachora: running around low tide left, fourth frame.</summary>
    EscapeDachoraRunningAroundLowTideLeftFourth,
    /// <summary>The escaping Dachora: running around low tide left, fifth frame.</summary>
    EscapeDachoraRunningAroundLowTideLeftFifth,
    /// <summary>The escaping Dachora: running around low tide left, sixth frame.</summary>
    EscapeDachoraRunningAroundLowTideLeftSixth,
    /// <summary>The escaping Dachora: running around low tide right, first frame.</summary>
    EscapeDachoraRunningAroundLowTideRightFirst,
    /// <summary>The escaping Dachora: running around low tide right, second frame.</summary>
    EscapeDachoraRunningAroundLowTideRightSecond,
    /// <summary>The escaping Dachora: running around low tide right, third frame.</summary>
    EscapeDachoraRunningAroundLowTideRightThird,
    /// <summary>The escaping Dachora: running around low tide right, fourth frame.</summary>
    EscapeDachoraRunningAroundLowTideRightFourth,
    /// <summary>The escaping Dachora: running around low tide right, fifth frame.</summary>
    EscapeDachoraRunningAroundLowTideRightFifth,
    /// <summary>The escaping Dachora: running around low tide right, sixth frame.</summary>
    EscapeDachoraRunningAroundLowTideRightSixth,
    /// <summary>Hibashi: eruption, first frame.</summary>
    HibashiEruptionFirst,
    /// <summary>Hibashi: eruption, second frame.</summary>
    HibashiEruptionSecond,
    /// <summary>Hibashi: eruption, third frame.</summary>
    HibashiEruptionThird,
    /// <summary>Hibashi: eruption, fourth frame.</summary>
    HibashiEruptionFourth,
    /// <summary>Hibashi: eruption, fifth frame.</summary>
    HibashiEruptionFifth,
    /// <summary>Hibashi: eruption, sixth frame.</summary>
    HibashiEruptionSixth,
    /// <summary>Hibashi: eruption, seventh frame.</summary>
    HibashiEruptionSeventh,
    /// <summary>Hibashi: eruption, eighth frame.</summary>
    HibashiEruptionEighth,
    /// <summary>Hibashi: eruption, ninth frame.</summary>
    HibashiEruptionNinth,
    /// <summary>Hibashi: eruption, tenth frame.</summary>
    HibashiEruptionTenth,
    /// <summary>Hibashi: eruption, eleventh frame.</summary>
    HibashiEruptionEleventh,
    /// <summary>Hibashi: eruption, twelfth frame.</summary>
    HibashiEruptionTwelfth,
    /// <summary>Hibashi: eruption, thirteenth frame.</summary>
    HibashiEruptionThirteenth,
    /// <summary>Hibashi: eruption, fourteenth frame.</summary>
    HibashiEruptionFourteenth,
    /// <summary>Hibashi: eruption, fifteenth frame.</summary>
    HibashiEruptionFifteenth,
    /// <summary>Hibashi: eruption, sixteenth frame.</summary>
    HibashiEruptionSixteenth,
    /// <summary>Hibashi: eruption, seventeenth frame.</summary>
    HibashiEruptionSeventeenth,
    /// <summary>Hibashi: eruption, eighteenth frame.</summary>
    HibashiEruptionEighteenth,
    /// <summary>Hibashi: eruption, nineteenth frame.</summary>
    HibashiEruptionNineteenth,
    /// <summary>Hibashi: eruption, twentieth frame.</summary>
    HibashiEruptionTwentieth,
    /// <summary>Hibashi: eruption, twenty-first frame.</summary>
    HibashiEruptionTwentyFirst,
    /// <summary>Hibashi: eruption, twenty-second frame.</summary>
    HibashiEruptionTwentySecond,
    /// <summary>Hibashi: eruption, twenty-third frame.</summary>
    HibashiEruptionTwentyThird,
    /// <summary>The zebetite: first frame.</summary>
    ZebetiteFirst,
    /// <summary>The zebetite: second frame.</summary>
    ZebetiteSecond,
    /// <summary>The zebetite: third frame.</summary>
    ZebetiteThird,
    /// <summary>The zebetite: fourth frame.</summary>
    ZebetiteFourth,
    /// <summary>The zebetite: fifth frame.</summary>
    ZebetiteFifth,
    /// <summary>The zebetite: sixth frame.</summary>
    ZebetiteSixth,
    /// <summary>The zebetite: seventh frame.</summary>
    ZebetiteSeventh,
    /// <summary>The zebetite: eighth frame.</summary>
    ZebetiteEighth,
    /// <summary>The zebetite: ninth frame.</summary>
    ZebetiteNinth,
    /// <summary>The zebetite: tenth frame.</summary>
    ZebetiteTenth,
    /// <summary>The Wrecked Ship ghost: floating, first frame.</summary>
    WreckedShipGhostFloatingFirst,
    /// <summary>The Wrecked Ship ghost: floating, second frame.</summary>
    WreckedShipGhostFloatingSecond,
    /// <summary>The Wrecked Ship ghost: floating, third frame.</summary>
    WreckedShipGhostFloatingThird,
    /// <summary>Powamp: body fast, first frame.</summary>
    PowampBodyFastFirst,
    /// <summary>Powamp: body fast, second frame.</summary>
    PowampBodyFastSecond,
    /// <summary>Powamp: body fast, third frame.</summary>
    PowampBodyFastThird,
    /// <summary>Powamp: balloon inflate, first frame.</summary>
    PowampBalloonInflateFirst,
    /// <summary>Powamp: balloon inflate, second frame.</summary>
    PowampBalloonInflateSecond,
    /// <summary>Powamp: balloon inflate, third frame.</summary>
    PowampBalloonInflateThird,
    /// <summary>The spark: flicker on, first frame.</summary>
    SparkFlickerOnFirst,
    /// <summary>The spark: flicker on, third frame.</summary>
    SparkFlickerOnThird,
    /// <summary>The spark: flicker on, fourth frame.</summary>
    SparkFlickerOnFourth,
    /// <summary>The spark: flicker on, fifth frame.</summary>
    SparkFlickerOnFifth,
    /// <summary>The spark: active, first frame.</summary>
    SparkActiveFirst,
    /// <summary>The spark: active, second frame.</summary>
    SparkActiveSecond,
    /// <summary>The spark: active, third frame.</summary>
    SparkActiveThird,
    /// <summary>The spark: active, fourth frame.</summary>
    SparkActiveFourth,
    /// <summary>The baby Metroid: initial, third frame.</summary>
    BabyMetroidInitialThird,
    /// <summary>The baby Metroid: initial, second frame.</summary>
    BabyMetroidInitialSecond,
    /// <summary>The baby Metroid: initial, first frame.</summary>
    BabyMetroidInitialFirst,
    /// <summary>Kraid: lint initial.</summary>
    KraidLintInitial,
    /// <summary>Kraid: lint post growth.</summary>
    KraidLintPostGrowth,
    /// <summary>The nuclear waffle: body loop, first frame.</summary>
    NuclearWaffleBodyLoopFirst,
    /// <summary>The nuclear waffle: body loop, second frame.</summary>
    NuclearWaffleBodyLoopSecond,
    /// <summary>The nuclear waffle: body loop, third frame.</summary>
    NuclearWaffleBodyLoopThird,
    /// <summary>The nuclear waffle: body loop, fourth frame.</summary>
    NuclearWaffleBodyLoopFourth,
    /// <summary>The nuclear waffle: body loop, fifth frame.</summary>
    NuclearWaffleBodyLoopFifth,
    /// <summary>The nuclear waffle: body loop, sixth frame.</summary>
    NuclearWaffleBodyLoopSixth,
    /// <summary>The nuclear waffle: body loop, seventh frame.</summary>
    NuclearWaffleBodyLoopSeventh,
    /// <summary>The nuclear waffle: body loop, eighth frame.</summary>
    NuclearWaffleBodyLoopEighth,
    /// <summary>Kzan: idle.</summary>
    KzanIdle,
    /// <summary>Polyp: stationary.</summary>
    PolypStationary,
    /// <summary>Ridley: breakup legs left.</summary>
    RidleyBreakupLegsLeft,
    /// <summary>Ridley: breakup legs right.</summary>
    RidleyBreakupLegsRight,
    /// <summary>Ridley: breakup torso left.</summary>
    RidleyBreakupTorsoLeft,
    /// <summary>Ridley: breakup torso right.</summary>
    RidleyBreakupTorsoRight,
    /// <summary>Ridley: breakup head left.</summary>
    RidleyBreakupHeadLeft,
    /// <summary>Ridley: breakup head right.</summary>
    RidleyBreakupHeadRight,
    /// <summary>Ridley: breakup claw left.</summary>
    RidleyBreakupClawLeft,
    /// <summary>Ridley: breakup claw right.</summary>
    RidleyBreakupClawRight,
    /// <summary>The walking space pirate: flinch left.</summary>
    WalkingPirateFlinchLeft,
    /// <summary>The walking space pirate: flinch right.</summary>
    WalkingPirateFlinchRight,
    /// <summary>The walking space pirate: walking left loop, first frame.</summary>
    WalkingPirateWalkingLeftLoopFirst,
    /// <summary>The walking space pirate: walking left loop, second frame.</summary>
    WalkingPirateWalkingLeftLoopSecond,
    /// <summary>The walking space pirate: walking left loop, third frame.</summary>
    WalkingPirateWalkingLeftLoopThird,
    /// <summary>The walking space pirate: walking left loop, fourth frame.</summary>
    WalkingPirateWalkingLeftLoopFourth,
    /// <summary>The walking space pirate: walking left loop, fifth frame.</summary>
    WalkingPirateWalkingLeftLoopFifth,
    /// <summary>The walking space pirate: walking left loop, sixth frame.</summary>
    WalkingPirateWalkingLeftLoopSixth,
    /// <summary>The walking space pirate: walking left loop, seventh frame.</summary>
    WalkingPirateWalkingLeftLoopSeventh,
    /// <summary>The walking space pirate: walking left loop, eighth frame.</summary>
    WalkingPirateWalkingLeftLoopEighth,
    /// <summary>The walking space pirate: fire lasers left, first frame.</summary>
    WalkingPirateFireLasersLeftFirst,
    /// <summary>The walking space pirate: fire lasers left, second frame.</summary>
    WalkingPirateFireLasersLeftSecond,
    /// <summary>The walking space pirate: fire lasers left, third frame.</summary>
    WalkingPirateFireLasersLeftThird,
    /// <summary>The walking space pirate: fire lasers left, fourth frame.</summary>
    WalkingPirateFireLasersLeftFourth,
    /// <summary>The walking space pirate: fire lasers left, fifth frame.</summary>
    WalkingPirateFireLasersLeftFifth,
    /// <summary>The walking space pirate: fire lasers left, sixth frame.</summary>
    WalkingPirateFireLasersLeftSixth,
    /// <summary>The walking space pirate: looking facing left, first frame.</summary>
    WalkingPirateLookingFacingLeftFirst,
    /// <summary>The walking space pirate: looking facing left, second frame.</summary>
    WalkingPirateLookingFacingLeftSecond,
    /// <summary>The walking space pirate: looking facing left, third frame.</summary>
    WalkingPirateLookingFacingLeftThird,
    /// <summary>The walking space pirate: look shared.</summary>
    WalkingPirateLookShared,
    /// <summary>The walking space pirate: walking right loop, first frame.</summary>
    WalkingPirateWalkingRightLoopFirst,
    /// <summary>The walking space pirate: walking right loop, second frame.</summary>
    WalkingPirateWalkingRightLoopSecond,
    /// <summary>The walking space pirate: walking right loop, third frame.</summary>
    WalkingPirateWalkingRightLoopThird,
    /// <summary>The walking space pirate: walking right loop, fourth frame.</summary>
    WalkingPirateWalkingRightLoopFourth,
    /// <summary>The walking space pirate: walking right loop, fifth frame.</summary>
    WalkingPirateWalkingRightLoopFifth,
    /// <summary>The walking space pirate: walking right loop, sixth frame.</summary>
    WalkingPirateWalkingRightLoopSixth,
    /// <summary>The walking space pirate: walking right loop, seventh frame.</summary>
    WalkingPirateWalkingRightLoopSeventh,
    /// <summary>The walking space pirate: walking right loop, eighth frame.</summary>
    WalkingPirateWalkingRightLoopEighth,
    /// <summary>The walking space pirate: fire lasers right, first frame.</summary>
    WalkingPirateFireLasersRightFirst,
    /// <summary>The walking space pirate: fire lasers right, second frame.</summary>
    WalkingPirateFireLasersRightSecond,
    /// <summary>The walking space pirate: fire lasers right, third frame.</summary>
    WalkingPirateFireLasersRightThird,
    /// <summary>The walking space pirate: fire lasers right, fourth frame.</summary>
    WalkingPirateFireLasersRightFourth,
    /// <summary>The walking space pirate: fire lasers right, fifth frame.</summary>
    WalkingPirateFireLasersRightFifth,
    /// <summary>The walking space pirate: fire lasers right, sixth frame.</summary>
    WalkingPirateFireLasersRightSixth,
    /// <summary>The walking space pirate: looking facing right, first frame.</summary>
    WalkingPirateLookingFacingRightFirst,
    /// <summary>The walking space pirate: looking facing right, second frame.</summary>
    WalkingPirateLookingFacingRightSecond,
    /// <summary>The walking space pirate: looking facing right, third frame.</summary>
    WalkingPirateLookingFacingRightThird,
    /// <summary>The wall space pirate: fire and jump left, first frame.</summary>
    WallPirateFireAndJumpLeftFirst,
    /// <summary>The wall space pirate: fire and jump left, second frame.</summary>
    WallPirateFireAndJumpLeftSecond,
    /// <summary>The wall space pirate: fire and jump left, third frame.</summary>
    WallPirateFireAndJumpLeftThird,
    /// <summary>The wall space pirate: fire and jump left, fourth frame.</summary>
    WallPirateFireAndJumpLeftFourth,
    /// <summary>The wall space pirate: moving up left wall loop, first frame.</summary>
    WallPirateMovingUpLeftWallLoopFirst,
    /// <summary>The wall space pirate: moving up left wall loop, second frame.</summary>
    WallPirateMovingUpLeftWallLoopSecond,
    /// <summary>The wall space pirate: moving up left wall loop, third frame.</summary>
    WallPirateMovingUpLeftWallLoopThird,
    /// <summary>The wall space pirate: moving up left wall loop, fourth frame.</summary>
    WallPirateMovingUpLeftWallLoopFourth,
    /// <summary>The wall space pirate: moving up left wall loop, fifth frame.</summary>
    WallPirateMovingUpLeftWallLoopFifth,
    /// <summary>The wall space pirate: fire and jump right, first frame.</summary>
    WallPirateFireAndJumpRightFirst,
    /// <summary>The wall space pirate: fire and jump right, second frame.</summary>
    WallPirateFireAndJumpRightSecond,
    /// <summary>The wall space pirate: fire and jump right, third frame.</summary>
    WallPirateFireAndJumpRightThird,
    /// <summary>The wall space pirate: fire and jump right, fourth frame.</summary>
    WallPirateFireAndJumpRightFourth,
    /// <summary>The wall space pirate: moving down right wall loop, first frame.</summary>
    WallPirateMovingDownRightWallLoopFirst,
    /// <summary>The wall space pirate: moving down right wall loop, second frame.</summary>
    WallPirateMovingDownRightWallLoopSecond,
    /// <summary>The wall space pirate: moving down right wall loop, third frame.</summary>
    WallPirateMovingDownRightWallLoopThird,
    /// <summary>The wall space pirate: moving down right wall loop, fourth frame.</summary>
    WallPirateMovingDownRightWallLoopFourth,
    /// <summary>The wall space pirate: moving down right wall loop, fifth frame.</summary>
    WallPirateMovingDownRightWallLoopFifth,
    /// <summary>The ninja space pirate: claw attack left, first frame.</summary>
    NinjaPirateClawAttackLeftFirst,
    /// <summary>The ninja space pirate: claw attack left, second frame.</summary>
    NinjaPirateClawAttackLeftSecond,
    /// <summary>The ninja space pirate: claw attack left, third frame.</summary>
    NinjaPirateClawAttackLeftThird,
    /// <summary>The ninja space pirate: claw attack left, fourth frame.</summary>
    NinjaPirateClawAttackLeftFourth,
    /// <summary>The ninja space pirate: claw attack left, fifth frame.</summary>
    NinjaPirateClawAttackLeftFifth,
    /// <summary>The ninja space pirate: claw attack left, sixth frame.</summary>
    NinjaPirateClawAttackLeftSixth,
    /// <summary>The ninja space pirate: claw attack left, seventh frame.</summary>
    NinjaPirateClawAttackLeftSeventh,
    /// <summary>The ninja space pirate: claw attack left, eighth frame.</summary>
    NinjaPirateClawAttackLeftEighth,
    /// <summary>The ninja space pirate: claw attack left, ninth frame.</summary>
    NinjaPirateClawAttackLeftNinth,
    /// <summary>The ninja space pirate: spin jump left loop, first frame.</summary>
    NinjaPirateSpinJumpLeftLoopFirst,
    /// <summary>The ninja space pirate: spin jump left loop, second frame.</summary>
    NinjaPirateSpinJumpLeftLoopSecond,
    /// <summary>The ninja space pirate: spin jump left loop, third frame.</summary>
    NinjaPirateSpinJumpLeftLoopThird,
    /// <summary>The ninja space pirate: spin jump left loop, fourth frame.</summary>
    NinjaPirateSpinJumpLeftLoopFourth,
    /// <summary>The ninja space pirate: spin jump left loop, fifth frame.</summary>
    NinjaPirateSpinJumpLeftLoopFifth,
    /// <summary>The ninja space pirate: spin jump left loop, sixth frame.</summary>
    NinjaPirateSpinJumpLeftLoopSixth,
    /// <summary>The ninja space pirate: spin jump left loop, seventh frame.</summary>
    NinjaPirateSpinJumpLeftLoopSeventh,
    /// <summary>The ninja space pirate: spin jump left loop, eighth frame.</summary>
    NinjaPirateSpinJumpLeftLoopEighth,
    /// <summary>The ninja space pirate: active facing left loop, first frame.</summary>
    NinjaPirateActiveFacingLeftLoopFirst,
    /// <summary>The ninja space pirate: active facing left loop, second frame.</summary>
    NinjaPirateActiveFacingLeftLoopSecond,
    /// <summary>The ninja space pirate: active facing left loop, third frame.</summary>
    NinjaPirateActiveFacingLeftLoopThird,
    /// <summary>The ninja space pirate: flinch facing left.</summary>
    NinjaPirateFlinchFacingLeft,
    /// <summary>The ninja space pirate: divekick left jump.</summary>
    NinjaPirateDivekickLeftJump,
    /// <summary>The ninja space pirate: divekick left jump loop.</summary>
    NinjaPirateDivekickLeftJumpLoop,
    /// <summary>The ninja space pirate: divekick left dive.</summary>
    NinjaPirateDivekickLeftDive,
    /// <summary>The ninja space pirate: walk to left post loop, first frame.</summary>
    NinjaPirateWalkToLeftPostLoopFirst,
    /// <summary>The ninja space pirate: walk to left post loop, second frame.</summary>
    NinjaPirateWalkToLeftPostLoopSecond,
    /// <summary>The ninja space pirate: walk to left post loop, third frame.</summary>
    NinjaPirateWalkToLeftPostLoopThird,
    /// <summary>The ninja space pirate: walk to left post loop, fourth frame.</summary>
    NinjaPirateWalkToLeftPostLoopFourth,
    /// <summary>The ninja space pirate: walk to left post loop, fifth frame.</summary>
    NinjaPirateWalkToLeftPostLoopFifth,
    /// <summary>The ninja space pirate: walk to left post loop, sixth frame.</summary>
    NinjaPirateWalkToLeftPostLoopSixth,
    /// <summary>The ninja space pirate: walk to left post loop, seventh frame.</summary>
    NinjaPirateWalkToLeftPostLoopSeventh,
    /// <summary>The ninja space pirate: walk to left post loop, eighth frame.</summary>
    NinjaPirateWalkToLeftPostLoopEighth,
    /// <summary>The ninja space pirate: initial facing left loop, first frame.</summary>
    NinjaPirateInitialFacingLeftLoopFirst,
    /// <summary>The ninja space pirate: initial facing left loop, second frame.</summary>
    NinjaPirateInitialFacingLeftLoopSecond,
    /// <summary>The ninja space pirate: initial facing left loop, third frame.</summary>
    NinjaPirateInitialFacingLeftLoopThird,
    /// <summary>The ninja space pirate: land facing left, third frame.</summary>
    NinjaPirateLandFacingLeftThird,
    /// <summary>The ninja space pirate: land facing left, fourth frame.</summary>
    NinjaPirateLandFacingLeftFourth,
    /// <summary>The ninja space pirate: land facing left loop, first frame.</summary>
    NinjaPirateLandFacingLeftLoopFirst,
    /// <summary>The ninja space pirate: land facing left loop, second frame.</summary>
    NinjaPirateLandFacingLeftLoopSecond,
    /// <summary>The ninja space pirate: land facing left loop, third frame.</summary>
    NinjaPirateLandFacingLeftLoopThird,
    /// <summary>The ninja space pirate: kick facing left, first frame.</summary>
    NinjaPirateKickFacingLeftFirst,
    /// <summary>The ninja space pirate: kick facing left, second frame.</summary>
    NinjaPirateKickFacingLeftSecond,
    /// <summary>The ninja space pirate: kick facing left, third frame.</summary>
    NinjaPirateKickFacingLeftThird,
    /// <summary>The ninja space pirate: claw attack right, first frame.</summary>
    NinjaPirateClawAttackRightFirst,
    /// <summary>The ninja space pirate: claw attack right, second frame.</summary>
    NinjaPirateClawAttackRightSecond,
    /// <summary>The ninja space pirate: claw attack right, third frame.</summary>
    NinjaPirateClawAttackRightThird,
    /// <summary>The ninja space pirate: claw attack right, fourth frame.</summary>
    NinjaPirateClawAttackRightFourth,
    /// <summary>The ninja space pirate: claw attack right, fifth frame.</summary>
    NinjaPirateClawAttackRightFifth,
    /// <summary>The ninja space pirate: claw attack right, sixth frame.</summary>
    NinjaPirateClawAttackRightSixth,
    /// <summary>The ninja space pirate: claw attack right, seventh frame.</summary>
    NinjaPirateClawAttackRightSeventh,
    /// <summary>The ninja space pirate: claw attack right, eighth frame.</summary>
    NinjaPirateClawAttackRightEighth,
    /// <summary>The ninja space pirate: spin jump right loop, first frame.</summary>
    NinjaPirateSpinJumpRightLoopFirst,
    /// <summary>The ninja space pirate: spin jump right loop, second frame.</summary>
    NinjaPirateSpinJumpRightLoopSecond,
    /// <summary>The ninja space pirate: spin jump right loop, third frame.</summary>
    NinjaPirateSpinJumpRightLoopThird,
    /// <summary>The ninja space pirate: spin jump right loop, fourth frame.</summary>
    NinjaPirateSpinJumpRightLoopFourth,
    /// <summary>The ninja space pirate: spin jump right loop, fifth frame.</summary>
    NinjaPirateSpinJumpRightLoopFifth,
    /// <summary>The ninja space pirate: spin jump right loop, sixth frame.</summary>
    NinjaPirateSpinJumpRightLoopSixth,
    /// <summary>The ninja space pirate: spin jump right loop, seventh frame.</summary>
    NinjaPirateSpinJumpRightLoopSeventh,
    /// <summary>The ninja space pirate: spin jump right loop, eighth frame.</summary>
    NinjaPirateSpinJumpRightLoopEighth,
    /// <summary>The ninja space pirate: flinch facing right.</summary>
    NinjaPirateFlinchFacingRight,
    /// <summary>The ninja space pirate: divekick right jump.</summary>
    NinjaPirateDivekickRightJump,
    /// <summary>The ninja space pirate: divekick right jump loop.</summary>
    NinjaPirateDivekickRightJumpLoop,
    /// <summary>The ninja space pirate: divekick right dive.</summary>
    NinjaPirateDivekickRightDive,
    /// <summary>The ninja space pirate: walk to right post loop, first frame.</summary>
    NinjaPirateWalkToRightPostLoopFirst,
    /// <summary>The ninja space pirate: walk to right post loop, second frame.</summary>
    NinjaPirateWalkToRightPostLoopSecond,
    /// <summary>The ninja space pirate: walk to right post loop, third frame.</summary>
    NinjaPirateWalkToRightPostLoopThird,
    /// <summary>The ninja space pirate: walk to right post loop, fourth frame.</summary>
    NinjaPirateWalkToRightPostLoopFourth,
    /// <summary>The ninja space pirate: walk to right post loop, fifth frame.</summary>
    NinjaPirateWalkToRightPostLoopFifth,
    /// <summary>The ninja space pirate: walk to right post loop, sixth frame.</summary>
    NinjaPirateWalkToRightPostLoopSixth,
    /// <summary>The ninja space pirate: walk to right post loop, seventh frame.</summary>
    NinjaPirateWalkToRightPostLoopSeventh,
    /// <summary>The ninja space pirate: walk to right post loop, eighth frame.</summary>
    NinjaPirateWalkToRightPostLoopEighth,
    /// <summary>The ninja space pirate: initial facing right loop, first frame.</summary>
    NinjaPirateInitialFacingRightLoopFirst,
    /// <summary>The ninja space pirate: initial facing right loop, third frame.</summary>
    NinjaPirateInitialFacingRightLoopThird,
    /// <summary>The ninja space pirate: kick facing right, first frame.</summary>
    NinjaPirateKickFacingRightFirst,
    /// <summary>The ninja space pirate: kick facing right, second frame.</summary>
    NinjaPirateKickFacingRightSecond,
    /// <summary>The ninja space pirate: kick facing right, third frame.</summary>
    NinjaPirateKickFacingRightThird,
    /// <summary>Ridley: body left.</summary>
    RidleyBodyLeft,
    /// <summary>Ridley: body right.</summary>
    RidleyBodyRight,
    /// <summary>Ridley: body left mouth half open.</summary>
    RidleyBodyLeftMouthHalfOpen,
    /// <summary>Ridley: body left mouth open.</summary>
    RidleyBodyLeftMouthOpen,
    /// <summary>Ridley: body right mouth half open.</summary>
    RidleyBodyRightMouthHalfOpen,
    /// <summary>Ridley: body right mouth open.</summary>
    RidleyBodyRightMouthOpen,
    /// <summary>Ridley: body left legs half extended.</summary>
    RidleyBodyLeftLegsHalfExtended,
    /// <summary>Ridley: body left legs extended.</summary>
    RidleyBodyLeftLegsExtended,
    /// <summary>Ridley: body right legs half extended.</summary>
    RidleyBodyRightLegsHalfExtended,
    /// <summary>Ridley: body right legs extended.</summary>
    RidleyBodyRightLegsExtended,
    /// <summary>Ridley: body forward.</summary>
    RidleyBodyForward,
    /// <summary>Draygon: arms facing left idle, first frame.</summary>
    DraygonArmsFacingLeftIdleFirst,
    /// <summary>Draygon: arms facing left idle, second frame.</summary>
    DraygonArmsFacingLeftIdleSecond,
    /// <summary>Draygon: arms facing left idle, third frame.</summary>
    DraygonArmsFacingLeftIdleThird,
    /// <summary>Draygon: arms facing left idle, fourth frame.</summary>
    DraygonArmsFacingLeftIdleFourth,
    /// <summary>Draygon: arms facing left idle, fifth frame.</summary>
    DraygonArmsFacingLeftIdleFifth,
    /// <summary>Draygon: arms facing left idle, sixth frame.</summary>
    DraygonArmsFacingLeftIdleSixth,
    /// <summary>Draygon: arms facing left near swoop apex, first frame.</summary>
    DraygonArmsFacingLeftNearSwoopApexFirst,
    /// <summary>Draygon: arms facing left near swoop apex, second frame.</summary>
    DraygonArmsFacingLeftNearSwoopApexSecond,
    /// <summary>Draygon: arms facing left near swoop apex, third frame.</summary>
    DraygonArmsFacingLeftNearSwoopApexThird,
    /// <summary>Draygon: arms facing left near swoop apex, fourth frame.</summary>
    DraygonArmsFacingLeftNearSwoopApexFourth,
    /// <summary>Draygon: tail facing left idle, first frame.</summary>
    DraygonTailFacingLeftIdleFirst,
    /// <summary>Draygon: tail facing left idle, second frame.</summary>
    DraygonTailFacingLeftIdleSecond,
    /// <summary>Draygon: tail facing left idle, third frame.</summary>
    DraygonTailFacingLeftIdleThird,
    /// <summary>Draygon: tail facing left idle, fourth frame.</summary>
    DraygonTailFacingLeftIdleFourth,
    /// <summary>Draygon: tail facing left idle, fifth frame.</summary>
    DraygonTailFacingLeftIdleFifth,
    /// <summary>Draygon: tail facing left idle, sixth frame.</summary>
    DraygonTailFacingLeftIdleSixth,
    /// <summary>Draygon: tail facing left idle, seventh frame.</summary>
    DraygonTailFacingLeftIdleSeventh,
    /// <summary>Draygon: tail facing left initial fake whip, second frame.</summary>
    DraygonTailFacingLeftInitialFakeWhipSecond,
    /// <summary>Draygon: tail facing left initial fake whip, third frame.</summary>
    DraygonTailFacingLeftInitialFakeWhipThird,
    /// <summary>Draygon: tail facing left initial fake whip, fourth frame.</summary>
    DraygonTailFacingLeftInitialFakeWhipFourth,
    /// <summary>Draygon: tail facing left initial fake whip, fifth frame.</summary>
    DraygonTailFacingLeftInitialFakeWhipFifth,
    /// <summary>Draygon: tail facing left initial fake whip, sixth frame.</summary>
    DraygonTailFacingLeftInitialFakeWhipSixth,
    /// <summary>Draygon: tail facing left initial fake whip, seventh frame.</summary>
    DraygonTailFacingLeftInitialFakeWhipSeventh,
    /// <summary>Draygon: tail facing left initial fake whip, eighth frame.</summary>
    DraygonTailFacingLeftInitialFakeWhipEighth,
    /// <summary>Draygon: arms facing right idle, first frame.</summary>
    DraygonArmsFacingRightIdleFirst,
    /// <summary>Draygon: arms facing right idle, second frame.</summary>
    DraygonArmsFacingRightIdleSecond,
    /// <summary>Draygon: arms facing right idle, third frame.</summary>
    DraygonArmsFacingRightIdleThird,
    /// <summary>Draygon: arms facing right idle, fourth frame.</summary>
    DraygonArmsFacingRightIdleFourth,
    /// <summary>Draygon: arms facing right idle, fifth frame.</summary>
    DraygonArmsFacingRightIdleFifth,
    /// <summary>Draygon: arms facing right idle, sixth frame.</summary>
    DraygonArmsFacingRightIdleSixth,
    /// <summary>Draygon: arms facing right near swoop apex, first frame.</summary>
    DraygonArmsFacingRightNearSwoopApexFirst,
    /// <summary>Draygon: arms facing right near swoop apex, second frame.</summary>
    DraygonArmsFacingRightNearSwoopApexSecond,
    /// <summary>Draygon: arms facing right near swoop apex, third frame.</summary>
    DraygonArmsFacingRightNearSwoopApexThird,
    /// <summary>Draygon: arms facing right near swoop apex, fourth frame.</summary>
    DraygonArmsFacingRightNearSwoopApexFourth,
    /// <summary>Draygon: tail facing right idle, first frame.</summary>
    DraygonTailFacingRightIdleFirst,
    /// <summary>Draygon: tail facing right idle, second frame.</summary>
    DraygonTailFacingRightIdleSecond,
    /// <summary>Draygon: tail facing right idle, third frame.</summary>
    DraygonTailFacingRightIdleThird,
    /// <summary>Draygon: tail facing right idle, fourth frame.</summary>
    DraygonTailFacingRightIdleFourth,
    /// <summary>Draygon: tail facing right idle, fifth frame.</summary>
    DraygonTailFacingRightIdleFifth,
    /// <summary>Draygon: tail facing right idle, sixth frame.</summary>
    DraygonTailFacingRightIdleSixth,
    /// <summary>Draygon: tail facing right idle, seventh frame.</summary>
    DraygonTailFacingRightIdleSeventh,
    /// <summary>Draygon: tail facing right final whips loop, second frame.</summary>
    DraygonTailFacingRightFinalWhipsLoopSecond,
    /// <summary>Draygon: tail facing right final whips loop, third frame.</summary>
    DraygonTailFacingRightFinalWhipsLoopThird,
    /// <summary>Draygon: tail facing right final whips loop, fourth frame.</summary>
    DraygonTailFacingRightFinalWhipsLoopFourth,
    /// <summary>Draygon: tail facing right final whips loop, fifth frame.</summary>
    DraygonTailFacingRightFinalWhipsLoopFifth,
    /// <summary>Draygon: tail facing right final whips loop, sixth frame.</summary>
    DraygonTailFacingRightFinalWhipsLoopSixth,
    /// <summary>Draygon: tail facing right final whips loop, seventh frame.</summary>
    DraygonTailFacingRightFinalWhipsLoopSeventh,
    /// <summary>Draygon: tail facing right final whips loop, eighth frame.</summary>
    DraygonTailFacingRightFinalWhipsLoopEighth,
    /// <summary>Spore Spawn: initial dead.</summary>
    SporeSpawnInitialDead,
    /// <summary>Spore Spawn: initial alive.</summary>
    SporeSpawnInitialAlive,
    /// <summary>Spore Spawn: opening, first frame.</summary>
    SporeSpawnOpeningFirst,
    /// <summary>Spore Spawn: opening, second frame.</summary>
    SporeSpawnOpeningSecond,
    /// <summary>Spore Spawn: opening, third frame.</summary>
    SporeSpawnOpeningThird,
    /// <summary>Spore Spawn: opening, fourth frame.</summary>
    SporeSpawnOpeningFourth,
    /// <summary>Spore Spawn: opening, fifth frame.</summary>
    SporeSpawnOpeningFifth,
    /// <summary>Spore Spawn: death.</summary>
    SporeSpawnDeath,
    /// <summary>Spore Spawn: opening, seventh frame.</summary>
    SporeSpawnOpeningSeventh,
    /// <summary>Spore Spawn: open pulse, first frame.</summary>
    SporeSpawnOpenPulseFirst,
    /// <summary>Spore Spawn: open pulse, second frame.</summary>
    SporeSpawnOpenPulseSecond,
    /// <summary>Spore Spawn: open pulse, third frame.</summary>
    SporeSpawnOpenPulseThird,
    /// <summary>Ceres steam: up.</summary>
    CeresSteamUp,
    /// <summary>Ceres steam: up, second frame.</summary>
    CeresSteamUpSecond,
    /// <summary>Ceres steam: up, third frame.</summary>
    CeresSteamUpThird,
    /// <summary>Ceres steam: up, fourth frame.</summary>
    CeresSteamUpFourth,
    /// <summary>Ceres steam: up, fifth frame.</summary>
    CeresSteamUpFifth,
    /// <summary>Ceres steam: up, sixth frame.</summary>
    CeresSteamUpSixth,
    /// <summary>Ceres steam: up, seventh frame.</summary>
    CeresSteamUpSeventh,
    /// <summary>Ceres steam: left.</summary>
    CeresSteamLeft,
    /// <summary>Ceres steam: left, second frame.</summary>
    CeresSteamLeftSecond,
    /// <summary>Ceres steam: left, third frame.</summary>
    CeresSteamLeftThird,
    /// <summary>Ceres steam: left, fourth frame.</summary>
    CeresSteamLeftFourth,
    /// <summary>Ceres steam: left, fifth frame.</summary>
    CeresSteamLeftFifth,
    /// <summary>Ceres steam: left, sixth frame.</summary>
    CeresSteamLeftSixth,
    /// <summary>Ceres steam: left, seventh frame.</summary>
    CeresSteamLeftSeventh,
    /// <summary>Ceres steam: down.</summary>
    CeresSteamDown,
    /// <summary>Ceres steam: down, second frame.</summary>
    CeresSteamDownSecond,
    /// <summary>Ceres steam: down, third frame.</summary>
    CeresSteamDownThird,
    /// <summary>Ceres steam: down, fourth frame.</summary>
    CeresSteamDownFourth,
    /// <summary>Ceres steam: down, fifth frame.</summary>
    CeresSteamDownFifth,
    /// <summary>Ceres steam: down, sixth frame.</summary>
    CeresSteamDownSixth,
    /// <summary>Ceres steam: down, seventh frame.</summary>
    CeresSteamDownSeventh,
    /// <summary>Ceres steam: right.</summary>
    CeresSteamRight,
    /// <summary>Ceres steam: right, second frame.</summary>
    CeresSteamRightSecond,
    /// <summary>Ceres steam: right, third frame.</summary>
    CeresSteamRightThird,
    /// <summary>Ceres steam: right, fourth frame.</summary>
    CeresSteamRightFourth,
    /// <summary>Ceres steam: right, fifth frame.</summary>
    CeresSteamRightFifth,
    /// <summary>Ceres steam: right, sixth frame.</summary>
    CeresSteamRightSixth,
    /// <summary>Ceres steam: right, seventh frame.</summary>
    CeresSteamRightSeventh,
    /// <summary>Oum: facing left idle.</summary>
    OumFacingLeftIdle,
    /// <summary>Oum: facing left attacking, second frame.</summary>
    OumFacingLeftAttackingSecond,
    /// <summary>Oum: facing left attacking, third frame.</summary>
    OumFacingLeftAttackingThird,
    /// <summary>Oum: facing left attacking, fourth frame.</summary>
    OumFacingLeftAttackingFourth,
    /// <summary>Oum: facing left attacking, fifth frame.</summary>
    OumFacingLeftAttackingFifth,
    /// <summary>Oum: facing left attacking, sixth frame.</summary>
    OumFacingLeftAttackingSixth,
    /// <summary>Oum: facing left attacking, seventh frame.</summary>
    OumFacingLeftAttackingSeventh,
    /// <summary>Oum: facing left attacking, eighth frame.</summary>
    OumFacingLeftAttackingEighth,
    /// <summary>Oum: facing left rolling forwards, second frame.</summary>
    OumFacingLeftRollingForwardsSecond,
    /// <summary>Oum: facing left rolling forwards, third frame.</summary>
    OumFacingLeftRollingForwardsThird,
    /// <summary>Oum: facing left rolling forwards, fourth frame.</summary>
    OumFacingLeftRollingForwardsFourth,
    /// <summary>Oum: facing left rolling forwards, fifth frame.</summary>
    OumFacingLeftRollingForwardsFifth,
    /// <summary>Oum: facing left rolling forwards, sixth frame.</summary>
    OumFacingLeftRollingForwardsSixth,
    /// <summary>Oum: facing left rolling forwards, seventh frame.</summary>
    OumFacingLeftRollingForwardsSeventh,
    /// <summary>Oum: facing left rolling forwards, eighth frame.</summary>
    OumFacingLeftRollingForwardsEighth,
    /// <summary>Oum: facing right idle.</summary>
    OumFacingRightIdle,
    /// <summary>Oum: facing right attacking, second frame.</summary>
    OumFacingRightAttackingSecond,
    /// <summary>Oum: facing right attacking, third frame.</summary>
    OumFacingRightAttackingThird,
    /// <summary>Oum: facing right attacking, fourth frame.</summary>
    OumFacingRightAttackingFourth,
    /// <summary>Oum: facing right attacking, fifth frame.</summary>
    OumFacingRightAttackingFifth,
    /// <summary>Oum: facing right attacking, sixth frame.</summary>
    OumFacingRightAttackingSixth,
    /// <summary>Oum: facing right attacking, seventh frame.</summary>
    OumFacingRightAttackingSeventh,
    /// <summary>Oum: facing right attacking, eighth frame.</summary>
    OumFacingRightAttackingEighth,
    /// <summary>Oum: facing right rolling forwards, second frame.</summary>
    OumFacingRightRollingForwardsSecond,
    /// <summary>Oum: facing right rolling forwards, third frame.</summary>
    OumFacingRightRollingForwardsThird,
    /// <summary>Oum: facing right rolling forwards, fourth frame.</summary>
    OumFacingRightRollingForwardsFourth,
    /// <summary>Oum: facing right rolling forwards, fifth frame.</summary>
    OumFacingRightRollingForwardsFifth,
    /// <summary>Oum: facing right rolling forwards, sixth frame.</summary>
    OumFacingRightRollingForwardsSixth,
    /// <summary>Oum: facing right rolling forwards, seventh frame.</summary>
    OumFacingRightRollingForwardsSeventh,
    /// <summary>Oum: facing right rolling forwards, eighth frame.</summary>
    OumFacingRightRollingForwardsEighth,
    /// <summary>Crocomire: tongue fight, first frame.</summary>
    CrocomireTongueFightFirst,
    /// <summary>Crocomire: tongue fight, second frame.</summary>
    CrocomireTongueFightSecond,
    /// <summary>Crocomire: tongue fight, third frame.</summary>
    CrocomireTongueFightThird,
    /// <summary>Crocomire: tongue fight, fourth frame.</summary>
    CrocomireTongueFightFourth,
    /// <summary>Crocomire: tongue melting pose, first frame.</summary>
    CrocomireTongueMeltingPoseFirst,
    /// <summary>Crocomire: tongue melting pose, second frame.</summary>
    CrocomireTongueMeltingPoseSecond,
    /// <summary>Crocomire: tongue melting pose, third frame.</summary>
    CrocomireTongueMeltingPoseThird,
    /// <summary>Crocomire: tongue melting pose, fourth frame.</summary>
    CrocomireTongueMeltingPoseFourth,
    /// <summary>Crocomire: tongue melting pose, fifth frame.</summary>
    CrocomireTongueMeltingPoseFifth,
    /// <summary>Crocomire: body charging, first frame.</summary>
    CrocomireBodyChargingFirst,
    /// <summary>Crocomire: body charging, second frame.</summary>
    CrocomireBodyChargingSecond,
    /// <summary>Crocomire: body charging, third frame.</summary>
    CrocomireBodyChargingThird,
    /// <summary>Crocomire: body charging, fourth frame.</summary>
    CrocomireBodyChargingFourth,
    /// <summary>Crocomire: body charging, fifth frame.</summary>
    CrocomireBodyChargingFifth,
    /// <summary>Crocomire: body charging, sixth frame.</summary>
    CrocomireBodyChargingSixth,
    /// <summary>Crocomire: body charging, seventh frame.</summary>
    CrocomireBodyChargingSeventh,
    /// <summary>Crocomire: body charging, eighth frame.</summary>
    CrocomireBodyChargingEighth,
    /// <summary>Crocomire: body charging, ninth frame.</summary>
    CrocomireBodyChargingNinth,
    /// <summary>Crocomire: body charging, tenth frame.</summary>
    CrocomireBodyChargingTenth,
    /// <summary>Crocomire: body charging, eleventh frame.</summary>
    CrocomireBodyChargingEleventh,
    /// <summary>Crocomire: body charging, twelfth frame.</summary>
    CrocomireBodyChargingTwelfth,
    /// <summary>Crocomire: body initial.</summary>
    CrocomireBodyInitial,
    /// <summary>Crocomire: body wait for first second damage, sixth frame.</summary>
    CrocomireBodyWaitForFirstSecondDamageSixth,
    /// <summary>Crocomire: body wait for first second damage, fifth frame.</summary>
    CrocomireBodyWaitForFirstSecondDamageFifth,
    /// <summary>Crocomire: body wait for first second damage, fourth frame.</summary>
    CrocomireBodyWaitForFirstSecondDamageFourth,
    /// <summary>Crocomire: body wait for first second damage, third frame.</summary>
    CrocomireBodyWaitForFirstSecondDamageThird,
    /// <summary>Crocomire: body wait for first second damage, second frame.</summary>
    CrocomireBodyWaitForFirstSecondDamageSecond,
    /// <summary>Crocomire: body wait for first second damage, first frame.</summary>
    CrocomireBodyWaitForFirstSecondDamageFirst,
    /// <summary>Crocomire: body projectile attack, first frame.</summary>
    CrocomireBodyProjectileAttackFirst,
    /// <summary>Crocomire: body projectile attack, second frame.</summary>
    CrocomireBodyProjectileAttackSecond,
    /// <summary>Crocomire: body projectile attack, third frame.</summary>
    CrocomireBodyProjectileAttackThird,
    /// <summary>Crocomire: body projectile attack, fourth frame.</summary>
    CrocomireBodyProjectileAttackFourth,
    /// <summary>Crocomire: body projectile attack, fifth frame.</summary>
    CrocomireBodyProjectileAttackFifth,
    /// <summary>Crocomire: body projectile attack, sixth frame.</summary>
    CrocomireBodyProjectileAttackSixth,
    /// <summary>Crocomire: body projectile attack, seventh frame.</summary>
    CrocomireBodyProjectileAttackSeventh,
    /// <summary>Crocomire: body projectile attack, eighth frame.</summary>
    CrocomireBodyProjectileAttackEighth,
    /// <summary>Crocomire: body step forward after delay.</summary>
    CrocomireBodyStepForwardAfterDelay,
    /// <summary>Crocomire: body stepping forward, first frame.</summary>
    CrocomireBodySteppingForwardFirst,
    /// <summary>Crocomire: body stepping forward, second frame.</summary>
    CrocomireBodySteppingForwardSecond,
    /// <summary>Crocomire: body stepping forward, third frame.</summary>
    CrocomireBodySteppingForwardThird,
    /// <summary>Crocomire: body stepping forward, fourth frame.</summary>
    CrocomireBodySteppingForwardFourth,
    /// <summary>Crocomire: body stepping forward, fifth frame.</summary>
    CrocomireBodySteppingForwardFifth,
    /// <summary>Crocomire: body stepping forward, sixth frame.</summary>
    CrocomireBodySteppingForwardSixth,
    /// <summary>Crocomire: body stepping forward, seventh frame.</summary>
    CrocomireBodySteppingForwardSeventh,
    /// <summary>Crocomire: body stepping forward, eighth frame.</summary>
    CrocomireBodySteppingForwardEighth,
    /// <summary>Crocomire: body stepping forward, ninth frame.</summary>
    CrocomireBodySteppingForwardNinth,
    /// <summary>Crocomire: body projectile attack loop, first frame.</summary>
    CrocomireBodyProjectileAttackLoopFirst,
    /// <summary>Crocomire: body projectile attack loop, second frame.</summary>
    CrocomireBodyProjectileAttackLoopSecond,
    /// <summary>Crocomire: body projectile attack loop, third frame.</summary>
    CrocomireBodyProjectileAttackLoopThird,
    /// <summary>Crocomire: body projectile attack loop, fourth frame.</summary>
    CrocomireBodyProjectileAttackLoopFourth,
    /// <summary>Crocomire: body projectile attack loop, fifth frame.</summary>
    CrocomireBodyProjectileAttackLoopFifth,
    /// <summary>Crocomire: body melting one top row.</summary>
    CrocomireBodyMeltingOneTopRow,
    /// <summary>Crocomire: body melting one top two rows.</summary>
    CrocomireBodyMeltingOneTopTwoRows,
    /// <summary>Crocomire: body melting one top three rows.</summary>
    CrocomireBodyMeltingOneTopThreeRows,
    /// <summary>Crocomire: body melting one top four rows.</summary>
    CrocomireBodyMeltingOneTopFourRows,
    /// <summary>Crocomire: body melting two top row.</summary>
    CrocomireBodyMeltingTwoTopRow,
    /// <summary>Crocomire: body melting two top two rows.</summary>
    CrocomireBodyMeltingTwoTopTwoRows,
    /// <summary>Crocomire: body melting two top three rows.</summary>
    CrocomireBodyMeltingTwoTopThreeRows,
    /// <summary>Crocomire: body melting two top four rows.</summary>
    CrocomireBodyMeltingTwoTopFourRows,
    /// <summary>Bomb Torizo: dormant.</summary>
    BombTorizoDormant,
    /// <summary>Golden Torizo: initial.</summary>
    GoldenTorizoInitial,
    /// <summary>Kraid: arm normal, first frame.</summary>
    KraidArmNormalFirst,
    /// <summary>Kraid: arm normal, second frame.</summary>
    KraidArmNormalSecond,
    /// <summary>Kraid: arm normal, third frame.</summary>
    KraidArmNormalThird,
    /// <summary>Kraid: arm normal, fourth frame.</summary>
    KraidArmNormalFourth,
    /// <summary>Kraid: arm normal, fifth frame.</summary>
    KraidArmNormalFifth,
    /// <summary>Kraid: arm normal, sixth frame.</summary>
    KraidArmNormalSixth,
    /// <summary>Kraid: arm normal, seventh frame.</summary>
    KraidArmNormalSeventh,
    /// <summary>Kraid: arm normal, eighth frame.</summary>
    KraidArmNormalEighth,
    /// <summary>Kraid: arm normal, ninth frame.</summary>
    KraidArmNormalNinth,
    /// <summary>Kraid: arm normal, tenth frame.</summary>
    KraidArmNormalTenth,
    /// <summary>Kraid: arm rising or sinking, first frame.</summary>
    KraidArmRisingOrSinkingFirst,
    /// <summary>Kraid: arm rising or sinking, second frame.</summary>
    KraidArmRisingOrSinkingSecond,
    /// <summary>Kraid: arm rising or sinking, third frame.</summary>
    KraidArmRisingOrSinkingThird,
    /// <summary>Kraid: arm rising or sinking, fourth frame.</summary>
    KraidArmRisingOrSinkingFourth,
    /// <summary>Kraid: arm rising or sinking, fifth frame.</summary>
    KraidArmRisingOrSinkingFifth,
    /// <summary>Kraid: arm rising or sinking, sixth frame.</summary>
    KraidArmRisingOrSinkingSixth,
    /// <summary>Kraid: arm rising or sinking, seventh frame.</summary>
    KraidArmRisingOrSinkingSeventh,
    /// <summary>Kraid: arm rising or sinking, eighth frame.</summary>
    KraidArmRisingOrSinkingEighth,
    /// <summary>Kraid: arm rising or sinking, ninth frame.</summary>
    KraidArmRisingOrSinkingNinth,
    /// <summary>Kraid: arm rising or sinking, tenth frame.</summary>
    KraidArmRisingOrSinkingTenth,
    /// <summary>Kraid: arm dying or preparing to lunge, second frame.</summary>
    KraidArmDyingOrPreparingToLungeSecond,
    /// <summary>Kraid: arm dying or preparing to lunge, third frame.</summary>
    KraidArmDyingOrPreparingToLungeThird,
    /// <summary>Torizo: facing left standing up, second frame.</summary>
    TorizoFacingLeftStandingUpSecond,
    /// <summary>Torizo: facing left standing up, first frame.</summary>
    TorizoFacingLeftStandingUpFirst,
    /// <summary>Torizo: facing left seated.</summary>
    TorizoFacingLeftSeated,
    /// <summary>Torizo: facing left standing up, fourth frame.</summary>
    TorizoFacingLeftStandingUpFourth,
    /// <summary>Torizo: facing left standing up, fifth frame.</summary>
    TorizoFacingLeftStandingUpFifth,
    /// <summary>Torizo: facing left standing up, sixth frame.</summary>
    TorizoFacingLeftStandingUpSixth,
    /// <summary>Torizo: facing left walking right leg moving, first frame.</summary>
    TorizoFacingLeftWalkingRightLegMovingFirst,
    /// <summary>Torizo: facing left walking right leg moving, second frame.</summary>
    TorizoFacingLeftWalkingRightLegMovingSecond,
    /// <summary>Torizo: facing left walking right leg moving, third frame.</summary>
    TorizoFacingLeftWalkingRightLegMovingThird,
    /// <summary>Torizo: facing left walking right leg moving, fourth frame.</summary>
    TorizoFacingLeftWalkingRightLegMovingFourth,
    /// <summary>Torizo: facing left walking right leg moving, fifth frame.</summary>
    TorizoFacingLeftWalkingRightLegMovingFifth,
    /// <summary>Torizo: facing left walking left leg moving, first frame.</summary>
    TorizoFacingLeftWalkingLeftLegMovingFirst,
    /// <summary>Torizo: facing left walking left leg moving, second frame.</summary>
    TorizoFacingLeftWalkingLeftLegMovingSecond,
    /// <summary>Torizo: facing left walking left leg moving, third frame.</summary>
    TorizoFacingLeftWalkingLeftLegMovingThird,
    /// <summary>Torizo: facing left walking left leg moving, fourth frame.</summary>
    TorizoFacingLeftWalkingLeftLegMovingFourth,
    /// <summary>Torizo: facing left walking left leg moving, fifth frame.</summary>
    TorizoFacingLeftWalkingLeftLegMovingFifth,
    /// <summary>Torizo: facing left turning left.</summary>
    TorizoFacingLeftTurningLeft,
    /// <summary>Torizo: facing right walking left leg moving, first frame.</summary>
    TorizoFacingRightWalkingLeftLegMovingFirst,
    /// <summary>Torizo: facing right walking left leg moving, second frame.</summary>
    TorizoFacingRightWalkingLeftLegMovingSecond,
    /// <summary>Torizo: facing right walking left leg moving, third frame.</summary>
    TorizoFacingRightWalkingLeftLegMovingThird,
    /// <summary>Torizo: facing right walking left leg moving, fourth frame.</summary>
    TorizoFacingRightWalkingLeftLegMovingFourth,
    /// <summary>Torizo: facing right walking left leg moving, fifth frame.</summary>
    TorizoFacingRightWalkingLeftLegMovingFifth,
    /// <summary>Torizo: facing right walking right leg moving, first frame.</summary>
    TorizoFacingRightWalkingRightLegMovingFirst,
    /// <summary>Torizo: facing right walking right leg moving, second frame.</summary>
    TorizoFacingRightWalkingRightLegMovingSecond,
    /// <summary>Torizo: facing right walking right leg moving, third frame.</summary>
    TorizoFacingRightWalkingRightLegMovingThird,
    /// <summary>Torizo: facing right walking right leg moving, fourth frame.</summary>
    TorizoFacingRightWalkingRightLegMovingFourth,
    /// <summary>Torizo: facing right walking right leg moving, fifth frame.</summary>
    TorizoFacingRightWalkingRightLegMovingFifth,
    /// <summary>Torizo: facing right jumping forwards, first frame.</summary>
    TorizoFacingRightJumpingForwardsFirst,
    /// <summary>Torizo: facing right jumping forwards, second frame.</summary>
    TorizoFacingRightJumpingForwardsSecond,
    /// <summary>Torizo: facing right jumping forwards, third frame.</summary>
    TorizoFacingRightJumpingForwardsThird,
    /// <summary>Torizo: facing right spewing Chozo orbs right foot forward, first frame.</summary>
    TorizoFacingRightSpewingChozoOrbsRightFootForwardFirst,
    /// <summary>Torizo: facing right spewing Chozo orbs right foot forward, second frame.</summary>
    TorizoFacingRightSpewingChozoOrbsRightFootForwardSecond,
    /// <summary>Torizo: facing right spewing Chozo orbs right foot forward, third frame.</summary>
    TorizoFacingRightSpewingChozoOrbsRightFootForwardThird,
    /// <summary>Torizo: facing right spewing Chozo orbs right foot forward, fourth frame.</summary>
    TorizoFacingRightSpewingChozoOrbsRightFootForwardFourth,
    /// <summary>Torizo: facing right spewing Chozo orbs right foot forward, fifth frame.</summary>
    TorizoFacingRightSpewingChozoOrbsRightFootForwardFifth,
    /// <summary>Torizo: facing right spewing Chozo orbs right foot forward, sixth frame.</summary>
    TorizoFacingRightSpewingChozoOrbsRightFootForwardSixth,
    /// <summary>Torizo: facing right spewing Chozo orbs left foot forward, first frame.</summary>
    TorizoFacingRightSpewingChozoOrbsLeftFootForwardFirst,
    /// <summary>Torizo: facing right sonic booms left foot forward, second frame.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardSecond,
    /// <summary>Torizo: facing right sonic booms left foot forward, third frame.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardThird,
    /// <summary>Torizo: facing right sonic booms left foot forward, fourth frame.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardFourth,
    /// <summary>Torizo: facing right sonic booms left foot forward, fifth frame.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardFifth,
    /// <summary>Torizo: facing right sonic booms left foot forward, sixth frame.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardSixth,
    /// <summary>Torizo: facing right sonic booms left foot forward, seventh frame.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardSeventh,
    /// <summary>Torizo: facing right sonic booms left foot forward, eighth frame.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardEighth,
    /// <summary>Torizo: facing right sonic booms left foot forward, ninth frame.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardNinth,
    /// <summary>Torizo: facing right sonic booms left foot forward, tenth frame.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardTenth,
    /// <summary>Torizo: facing right sonic booms left foot forward, eleventh frame.</summary>
    TorizoFacingRightSonicBoomsLeftFootForwardEleventh,
    /// <summary>Torizo: facing right sonic booms right foot forward, second frame.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardSecond,
    /// <summary>Torizo: facing right sonic booms right foot forward, third frame.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardThird,
    /// <summary>Torizo: facing right sonic booms right foot forward, fourth frame.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardFourth,
    /// <summary>Torizo: facing right sonic booms right foot forward, fifth frame.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardFifth,
    /// <summary>Torizo: facing right sonic booms right foot forward, sixth frame.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardSixth,
    /// <summary>Torizo: facing right sonic booms right foot forward, seventh frame.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardSeventh,
    /// <summary>Torizo: facing right sonic booms right foot forward, eighth frame.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardEighth,
    /// <summary>Torizo: facing right sonic booms right foot forward, ninth frame.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardNinth,
    /// <summary>Torizo: facing right sonic booms right foot forward, tenth frame.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardTenth,
    /// <summary>Torizo: facing right sonic booms right foot forward, eleventh frame.</summary>
    TorizoFacingRightSonicBoomsRightFootForwardEleventh,
    /// <summary>Torizo: falling left.</summary>
    TorizoFallingLeft,
    /// <summary>Torizo: facing right spewing Chozo orbs left foot forward, second frame.</summary>
    TorizoFacingRightSpewingChozoOrbsLeftFootForwardSecond,
    /// <summary>Torizo: facing right spewing Chozo orbs left foot forward, third frame.</summary>
    TorizoFacingRightSpewingChozoOrbsLeftFootForwardThird,
    /// <summary>Torizo: facing right spewing Chozo orbs left foot forward, fourth frame.</summary>
    TorizoFacingRightSpewingChozoOrbsLeftFootForwardFourth,
    /// <summary>Torizo: facing right spewing Chozo orbs left foot forward, fifth frame.</summary>
    TorizoFacingRightSpewingChozoOrbsLeftFootForwardFifth,
    /// <summary>Torizo: facing right spewing Chozo orbs left foot forward, sixth frame.</summary>
    TorizoFacingRightSpewingChozoOrbsLeftFootForwardSixth,
    /// <summary>Torizo: facing left jumping forwards, first frame.</summary>
    TorizoFacingLeftJumpingForwardsFirst,
    /// <summary>Torizo: facing left jumping forwards, third frame.</summary>
    TorizoFacingLeftJumpingForwardsThird,
    /// <summary>Torizo: facing left spewing Chozo orbs right foot forward, first frame.</summary>
    TorizoFacingLeftSpewingChozoOrbsRightFootForwardFirst,
    /// <summary>Torizo: facing left spewing Chozo orbs right foot forward, second frame.</summary>
    TorizoFacingLeftSpewingChozoOrbsRightFootForwardSecond,
    /// <summary>Torizo: facing left spewing Chozo orbs right foot forward, third frame.</summary>
    TorizoFacingLeftSpewingChozoOrbsRightFootForwardThird,
    /// <summary>Torizo: facing left spewing Chozo orbs right foot forward, fourth frame.</summary>
    TorizoFacingLeftSpewingChozoOrbsRightFootForwardFourth,
    /// <summary>Torizo: facing left spewing Chozo orbs right foot forward, fifth frame.</summary>
    TorizoFacingLeftSpewingChozoOrbsRightFootForwardFifth,
    /// <summary>Torizo: facing left spewing Chozo orbs right foot forward, sixth frame.</summary>
    TorizoFacingLeftSpewingChozoOrbsRightFootForwardSixth,
    /// <summary>Torizo: facing left spewing Chozo orbs left foot forward, first frame.</summary>
    TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFirst,
    /// <summary>Torizo: facing left spewing Chozo orbs left foot forward, second frame.</summary>
    TorizoFacingLeftSpewingChozoOrbsLeftFootForwardSecond,
    /// <summary>Torizo: facing left spewing Chozo orbs left foot forward, third frame.</summary>
    TorizoFacingLeftSpewingChozoOrbsLeftFootForwardThird,
    /// <summary>Torizo: facing left spewing Chozo orbs left foot forward, fourth frame.</summary>
    TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFourth,
    /// <summary>Torizo: facing left spewing Chozo orbs left foot forward, fifth frame.</summary>
    TorizoFacingLeftSpewingChozoOrbsLeftFootForwardFifth,
    /// <summary>Torizo: facing left spewing Chozo orbs left foot forward, sixth frame.</summary>
    TorizoFacingLeftSpewingChozoOrbsLeftFootForwardSixth,
    /// <summary>Torizo: facing left sonic booms right foot forward, second frame.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardSecond,
    /// <summary>Torizo: facing left sonic booms right foot forward, third frame.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardThird,
    /// <summary>Torizo: facing left sonic booms right foot forward, fourth frame.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardFourth,
    /// <summary>Torizo: facing left sonic booms right foot forward, fifth frame.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardFifth,
    /// <summary>Torizo: facing left sonic booms right foot forward, sixth frame.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardSixth,
    /// <summary>Torizo: facing left sonic booms right foot forward, seventh frame.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardSeventh,
    /// <summary>Torizo: facing left sonic booms right foot forward, eighth frame.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardEighth,
    /// <summary>Torizo: facing left sonic booms right foot forward, ninth frame.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardNinth,
    /// <summary>Torizo: facing left sonic booms right foot forward, tenth frame.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardTenth,
    /// <summary>Torizo: facing left sonic booms right foot forward, eleventh frame.</summary>
    TorizoFacingLeftSonicBoomsRightFootForwardEleventh,
    /// <summary>Torizo: facing left sonic booms left foot forward, second frame.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardSecond,
    /// <summary>Torizo: facing left sonic booms left foot forward, third frame.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardThird,
    /// <summary>Torizo: facing left sonic booms left foot forward, fourth frame.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardFourth,
    /// <summary>Torizo: facing left sonic booms left foot forward, fifth frame.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardFifth,
    /// <summary>Torizo: facing left sonic booms left foot forward, sixth frame.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardSixth,
    /// <summary>Torizo: facing left sonic booms left foot forward, seventh frame.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardSeventh,
    /// <summary>Torizo: facing left sonic booms left foot forward, eighth frame.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardEighth,
    /// <summary>Torizo: facing left sonic booms left foot forward, ninth frame.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardNinth,
    /// <summary>Torizo: facing left sonic booms left foot forward, tenth frame.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardTenth,
    /// <summary>Torizo: facing left sonic booms left foot forward, eleventh frame.</summary>
    TorizoFacingLeftSonicBoomsLeftFootForwardEleventh,
    /// <summary>Torizo: facing right seated.</summary>
    TorizoFacingRightSeated,
    /// <summary>Torizo: facing right standing up, first frame.</summary>
    TorizoFacingRightStandingUpFirst,
    /// <summary>Torizo: facing right standing up, second frame.</summary>
    TorizoFacingRightStandingUpSecond,
    /// <summary>Torizo: facing right standing up, third frame.</summary>
    TorizoFacingRightStandingUpThird,
    /// <summary>Torizo: facing right standing up, fourth frame.</summary>
    TorizoFacingRightStandingUpFourth,
    /// <summary>Torizo: facing right standing up, fifth frame.</summary>
    TorizoFacingRightStandingUpFifth,
    /// <summary>Torizo: facing right standing up, sixth frame.</summary>
    TorizoFacingRightStandingUpSixth,
    /// <summary>Mother Brain: body standing.</summary>
    MotherBrainBodyStanding,
    /// <summary>Mother Brain: body walking, first frame.</summary>
    MotherBrainBodyWalkingFirst,
    /// <summary>Mother Brain: body walking, second frame.</summary>
    MotherBrainBodyWalkingSecond,
    /// <summary>Mother Brain: body walking, third frame.</summary>
    MotherBrainBodyWalkingThird,
    /// <summary>Mother Brain: body walking, fourth frame.</summary>
    MotherBrainBodyWalkingFourth,
    /// <summary>Mother Brain: body walking, fifth frame.</summary>
    MotherBrainBodyWalkingFifth,
    /// <summary>Mother Brain: body walking, sixth frame.</summary>
    MotherBrainBodyWalkingSixth,
    /// <summary>Mother Brain: body walking, seventh frame.</summary>
    MotherBrainBodyWalkingSeventh,
    /// <summary>Mother Brain: body walking, eighth frame.</summary>
    MotherBrainBodyWalkingEighth,
    /// <summary>Mother Brain: body crouched.</summary>
    MotherBrainBodyCrouched,
    /// <summary>Mother Brain: body uncrouching.</summary>
    MotherBrainBodyUncrouching,
    /// <summary>Mother Brain: body leaning down.</summary>
    MotherBrainBodyLeaningDown,
    /// <summary>Mother Brain: body initial dummy.</summary>
    MotherBrainBodyInitialDummy,
    /// <summary>Mother Brain: body hand beam, second frame.</summary>
    MotherBrainBodyHandBeamSecond,
    /// <summary>Mother Brain: body hand beam, third frame.</summary>
    MotherBrainBodyHandBeamThird,
    /// <summary>Mother Brain: body hand beam, fourth frame.</summary>
    MotherBrainBodyHandBeamFourth,
    /// <summary>Mother Brain: body hand beam, fifth frame.</summary>
    MotherBrainBodyHandBeamFifth,
    /// <summary>Phantoon: invulnerable body.</summary>
    PhantoonInvulnerableBody,
    /// <summary>Phantoon: full hitbox body.</summary>
    PhantoonFullHitboxBody,
    /// <summary>Phantoon: eye hitbox body.</summary>
    PhantoonEyeHitboxBody,
    /// <summary>Phantoon: eye closed.</summary>
    PhantoonEyeClosed,
    /// <summary>Phantoon: eye open, first frame.</summary>
    PhantoonEyeOpenFirst,
    /// <summary>Phantoon: eye open, second frame.</summary>
    PhantoonEyeOpenSecond,
    /// <summary>Phantoon: eye open, third frame.</summary>
    PhantoonEyeOpenThird,
    /// <summary>Phantoon: eyeball centered.</summary>
    PhantoonEyeballCentered,
    /// <summary>Phantoon: eye looking up.</summary>
    PhantoonEyeLookingUp,
    /// <summary>Phantoon: eye looking down.</summary>
    PhantoonEyeLookingDown,
    /// <summary>Phantoon: eye looking left.</summary>
    PhantoonEyeLookingLeft,
    /// <summary>Phantoon: eye looking right.</summary>
    PhantoonEyeLookingRight,
    /// <summary>Phantoon: eye looking down left.</summary>
    PhantoonEyeLookingDownLeft,
    /// <summary>Phantoon: eye looking down right.</summary>
    PhantoonEyeLookingDownRight,
    /// <summary>Phantoon: eye looking up left.</summary>
    PhantoonEyeLookingUpLeft,
    /// <summary>Phantoon: eye looking up right.</summary>
    PhantoonEyeLookingUpRight,
    /// <summary>Phantoon: tentacles, first frame.</summary>
    PhantoonTentaclesFirst,
    /// <summary>Phantoon: tentacles, second frame.</summary>
    PhantoonTentaclesSecond,
    /// <summary>Phantoon: tentacles, third frame.</summary>
    PhantoonTentaclesThird,
    /// <summary>Phantoon: mouth initial.</summary>
    PhantoonMouthInitial,
    /// <summary>Phantoon: mouth spawn flame, second frame.</summary>
    PhantoonMouthSpawnFlameSecond,
    /// <summary>Phantoon: mouth spawn flame, first frame.</summary>
    PhantoonMouthSpawnFlameFirst,
    /// <summary>Draygon: arms facing left dying, first frame.</summary>
    DraygonArmsFacingLeftDyingFirst,
    /// <summary>Draygon: arms facing left dying, second frame.</summary>
    DraygonArmsFacingLeftDyingSecond,
    /// <summary>Draygon: arms facing left dying, third frame.</summary>
    DraygonArmsFacingLeftDyingThird,
    /// <summary>Draygon: arms facing left dying, fourth frame.</summary>
    DraygonArmsFacingLeftDyingFourth,
    /// <summary>Draygon: body facing left fire goop, first frame.</summary>
    DraygonBodyFacingLeftFireGoopFirst,
    /// <summary>Draygon: body facing left fire goop, second frame.</summary>
    DraygonBodyFacingLeftFireGoopSecond,
    /// <summary>Draygon: body facing left fire goop, third frame.</summary>
    DraygonBodyFacingLeftFireGoopThird,
    /// <summary>Draygon: body facing left fire goop, fourth frame.</summary>
    DraygonBodyFacingLeftFireGoopFourth,
    /// <summary>Draygon: eye facing left idle, first frame.</summary>
    DraygonEyeFacingLeftIdleFirst,
    /// <summary>Draygon: eye facing left idle, second frame.</summary>
    DraygonEyeFacingLeftIdleSecond,
    /// <summary>Draygon: eye facing left idle, third frame.</summary>
    DraygonEyeFacingLeftIdleThird,
    /// <summary>Draygon: eye facing left dying loop, fifth frame.</summary>
    DraygonEyeFacingLeftDyingLoopFifth,
    /// <summary>Draygon: eye facing left idle, fourth frame.</summary>
    DraygonEyeFacingLeftIdleFourth,
    /// <summary>Draygon: eye facing left dying loop, third frame.</summary>
    DraygonEyeFacingLeftDyingLoopThird,
    /// <summary>Draygon: eye facing left idle, fifth frame.</summary>
    DraygonEyeFacingLeftIdleFifth,
    /// <summary>Draygon: eye facing left idle, sixth frame.</summary>
    DraygonEyeFacingLeftIdleSixth,
    /// <summary>Draygon: body facing left reset.</summary>
    DraygonBodyFacingLeftReset,
    /// <summary>Draygon: arms facing right dying, first frame.</summary>
    DraygonArmsFacingRightDyingFirst,
    /// <summary>Draygon: arms facing right dying, second frame.</summary>
    DraygonArmsFacingRightDyingSecond,
    /// <summary>Draygon: arms facing right dying, third frame.</summary>
    DraygonArmsFacingRightDyingThird,
    /// <summary>Draygon: arms facing right dying, fourth frame.</summary>
    DraygonArmsFacingRightDyingFourth,
    /// <summary>Draygon: body facing right fire goop, first frame.</summary>
    DraygonBodyFacingRightFireGoopFirst,
    /// <summary>Draygon: body facing right fire goop, second frame.</summary>
    DraygonBodyFacingRightFireGoopSecond,
    /// <summary>Draygon: body facing right fire goop, third frame.</summary>
    DraygonBodyFacingRightFireGoopThird,
    /// <summary>Draygon: body facing right fire goop, fourth frame.</summary>
    DraygonBodyFacingRightFireGoopFourth,
    /// <summary>Draygon: eye facing right idle, first frame.</summary>
    DraygonEyeFacingRightIdleFirst,
    /// <summary>Draygon: eye facing right idle, second frame.</summary>
    DraygonEyeFacingRightIdleSecond,
    /// <summary>Draygon: eye facing right idle, third frame.</summary>
    DraygonEyeFacingRightIdleThird,
    /// <summary>Draygon: eye facing right dying loop, fifth frame.</summary>
    DraygonEyeFacingRightDyingLoopFifth,
    /// <summary>Draygon: eye facing right idle, fourth frame.</summary>
    DraygonEyeFacingRightIdleFourth,
    /// <summary>Draygon: eye facing right dying loop, third frame.</summary>
    DraygonEyeFacingRightDyingLoopThird,
    /// <summary>Draygon: eye facing right idle, fifth frame.</summary>
    DraygonEyeFacingRightIdleFifth,
    /// <summary>Draygon: eye facing right idle, sixth frame.</summary>
    DraygonEyeFacingRightIdleSixth,
    /// <summary>Draygon: body facing right reset.</summary>
    DraygonBodyFacingRightReset,
    /// <summary>Crocomire: body skeleton falling, first frame.</summary>
    CrocomireBodySkeletonFallingFirst,
    /// <summary>Crocomire: body skeleton falling, second frame.</summary>
    CrocomireBodySkeletonFallingSecond,
    /// <summary>Crocomire: body skeleton falling, third frame.</summary>
    CrocomireBodySkeletonFallingThird,
    /// <summary>Crocomire: body skeleton falls apart, first frame.</summary>
    CrocomireBodySkeletonFallsApartFirst,
    /// <summary>Crocomire: body skeleton falls apart, second frame.</summary>
    CrocomireBodySkeletonFallsApartSecond,
    /// <summary>Crocomire: body skeleton falls apart, third frame.</summary>
    CrocomireBodySkeletonFallsApartThird,
    /// <summary>Crocomire: body skeleton falls apart, fourth frame.</summary>
    CrocomireBodySkeletonFallsApartFourth,
    /// <summary>Crocomire: body skeleton falls apart, fifth frame.</summary>
    CrocomireBodySkeletonFallsApartFifth,
    /// <summary>Crocomire: body skeleton falls apart, sixth frame.</summary>
    CrocomireBodySkeletonFallsApartSixth,
    /// <summary>Crocomire: body skeleton falls apart, seventh frame.</summary>
    CrocomireBodySkeletonFallsApartSeventh,
    /// <summary>Crocomire: body skeleton falls apart, eighth frame.</summary>
    CrocomireBodySkeletonFallsApartEighth,
    /// <summary>Crocomire: body skeleton falls apart, ninth frame.</summary>
    CrocomireBodySkeletonFallsApartNinth,
    /// <summary>Crocomire: body skeleton falls apart, tenth frame.</summary>
    CrocomireBodySkeletonFallsApartTenth,
    /// <summary>Crocomire: body skeleton falls apart, eleventh frame.</summary>
    CrocomireBodySkeletonFallsApartEleventh,
    /// <summary>Crocomire: body skeleton falls apart, twelfth frame.</summary>
    CrocomireBodySkeletonFallsApartTwelfth,
    /// <summary>Crocomire: body skeleton falls apart, thirteenth frame.</summary>
    CrocomireBodySkeletonFallsApartThirteenth,
    /// <summary>Crocomire: body skeleton falls apart, fourteenth frame.</summary>
    CrocomireBodySkeletonFallsApartFourteenth,
    /// <summary>Crocomire: body skeleton falls apart, fifteenth frame.</summary>
    CrocomireBodySkeletonFallsApartFifteenth,
    /// <summary>Crocomire: body skeleton falls apart, sixteenth frame.</summary>
    CrocomireBodySkeletonFallsApartSixteenth,
    /// <summary>Crocomire: body skeleton falls apart, seventeenth frame.</summary>
    CrocomireBodySkeletonFallsApartSeventeenth,
    /// <summary>Crocomire: body skeleton falls apart, eighteenth frame.</summary>
    CrocomireBodySkeletonFallsApartEighteenth,
    /// <summary>Crocomire: body skeleton stable.</summary>
    CrocomireBodySkeletonStable,
    /// <summary>Crocomire: body dead.</summary>
    CrocomireBodyDead,
    /// <summary>Crocomire: body skeleton flowing down river, first frame.</summary>
    CrocomireBodySkeletonFlowingDownRiverFirst,
    /// <summary>Crocomire: body skeleton flowing down river, second frame.</summary>
    CrocomireBodySkeletonFlowingDownRiverSecond,
    /// <summary>Crocomire: body skeleton flowing down river, third frame.</summary>
    CrocomireBodySkeletonFlowingDownRiverThird,
    /// <summary>Crocomire: body skeleton flowing down river, fourth frame.</summary>
    CrocomireBodySkeletonFlowingDownRiverFourth,
    /// <summary>Crocomire: body skeleton flowing down river, fifth frame.</summary>
    CrocomireBodySkeletonFlowingDownRiverFifth,
    /// <summary>Crocomire: body skeleton flowing down river, sixth frame.</summary>
    CrocomireBodySkeletonFlowingDownRiverSixth,
    /// <summary>Crocomire: body skeleton flowing down river, seventh frame.</summary>
    CrocomireBodySkeletonFlowingDownRiverSeventh,
    /// <summary>Crocomire: body skeleton flowing down river, eighth frame.</summary>
    CrocomireBodySkeletonFlowingDownRiverEighth,
    /// <summary>Crocomire: body skeleton flowing down river, ninth frame.</summary>
    CrocomireBodySkeletonFlowingDownRiverNinth,
    /// <summary>Crocomire: body skeleton flowing down river, tenth frame.</summary>
    CrocomireBodySkeletonFlowingDownRiverTenth,
    /// <summary>Kraid: foot neutral.</summary>
    KraidFootNeutral,
    /// <summary>Kraid: foot walking forward, second frame.</summary>
    KraidFootWalkingForwardSecond,
    /// <summary>Kraid: foot walking forward, third frame.</summary>
    KraidFootWalkingForwardThird,
    /// <summary>Kraid: foot walking forward, fourth frame.</summary>
    KraidFootWalkingForwardFourth,
    /// <summary>Kraid: foot walking forward, fifth frame.</summary>
    KraidFootWalkingForwardFifth,
    /// <summary>Kraid: foot walking forward, sixth frame.</summary>
    KraidFootWalkingForwardSixth,
    /// <summary>Kraid: foot walking forward, seventh frame.</summary>
    KraidFootWalkingForwardSeventh,
    /// <summary>Kraid: foot walking forward, eighth frame.</summary>
    KraidFootWalkingForwardEighth,
    /// <summary>Kraid: foot walking forward, ninth frame.</summary>
    KraidFootWalkingForwardNinth,
    /// <summary>Kraid: foot walking forward, tenth frame.</summary>
    KraidFootWalkingForwardTenth,
    /// <summary>Kraid: foot walking forward, eleventh frame.</summary>
    KraidFootWalkingForwardEleventh,
    /// <summary>Kraid: foot walking forward, twelfth frame.</summary>
    KraidFootWalkingForwardTwelfth,
    /// <summary>Kraid: foot walking forward, thirteenth frame.</summary>
    KraidFootWalkingForwardThirteenth,
    /// <summary>Kraid: foot walking forward, fourteenth frame.</summary>
    KraidFootWalkingForwardFourteenth,
    /// <summary>Kraid: foot walking forward, fifteenth frame.</summary>
    KraidFootWalkingForwardFifteenth,
    /// <summary>Kraid: foot walking forward, sixteenth frame.</summary>
    KraidFootWalkingForwardSixteenth,
    /// <summary>Kraid: foot walking forward, seventeenth frame.</summary>
    KraidFootWalkingForwardSeventeenth,
    /// <summary>Kraid: foot walking forward, eighteenth frame.</summary>
    KraidFootWalkingForwardEighteenth,
    /// <summary>Kraid: foot walking forward, nineteenth frame.</summary>
    KraidFootWalkingForwardNineteenth,
    /// <summary>Kraid: foot walking forward, twentieth frame.</summary>
    KraidFootWalkingForwardTwentieth,
    /// <summary>Kraid: foot walking backward, thirteenth frame.</summary>
    KraidFootWalkingBackwardThirteenth,
    /// <summary>Kraid: foot walking forward, twenty-first frame.</summary>
    KraidFootWalkingForwardTwentyFirst,
    /// <summary>Kraid: foot walking forward, twenty-second frame.</summary>
    KraidFootWalkingForwardTwentySecond,
    /// <summary>Kraid: foot walking forward, twenty-third frame.</summary>
    KraidFootWalkingForwardTwentyThird,
    /// <summary>Kraid: foot walking forward, twenty-fourth frame.</summary>
    KraidFootWalkingForwardTwentyFourth,
    /// <summary>Kraid: foot walking forward, twenty-fifth frame.</summary>
    KraidFootWalkingForwardTwentyFifth,
    /// <summary>Kraid: foot walking forward, twenty-sixth frame.</summary>
    KraidFootWalkingForwardTwentySixth,
    /// <summary>Kraid: foot walking forward, twenty-seventh frame.</summary>
    KraidFootWalkingForwardTwentySeventh,
    /// <summary>Kraid: foot walking forward, twenty-eighth frame.</summary>
    KraidFootWalkingForwardTwentyEighth,
    /// <summary>Kraid: foot walking forward, twenty-ninth frame.</summary>
    KraidFootWalkingForwardTwentyNinth,
    /// <summary>Kraid: foot walking forward, thirtieth frame.</summary>
    KraidFootWalkingForwardThirtieth,
    /// <summary>Kraid: foot walking forward, thirty-first frame.</summary>
    KraidFootWalkingForwardThirtyFirst,
    /// <summary>Kraid: foot walking backward, second frame.</summary>
    KraidFootWalkingBackwardSecond,
    /// <summary>Kraid: foot walking backward, first frame.</summary>
    KraidFootWalkingBackwardFirst,
    /// <summary>Kraid: foot standing.</summary>
    KraidFootStanding,
    /// <summary>
    /// The empty frame every enemy family shares: an enemy showing it draws nothing, and one that collides by its frames'
    /// hitboxes collides at the single point at its origin.
    /// </summary>
    Empty,
}
