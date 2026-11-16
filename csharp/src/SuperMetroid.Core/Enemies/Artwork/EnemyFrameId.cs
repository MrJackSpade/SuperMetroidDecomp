namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>
/// The frames an enemy shows: every installed enemy frame, and the empty frame every enemy family shares. Each is named for
/// what its picture shows: its owner, then its pose and facing, or the turn, size or placement that tells it from the
/// owner's other pictures, which its summary spells out; no animation that shows it and no place in one names it. A frame
/// of its own that shows another frame's picture where that frame shows it is that frame's twin. Its installed name is
/// its name in snake case (<see cref="EnemyFrameNameDefinitions"/>), which gives its installed sheet key
/// (<see cref="EnemyFrameSheetKeys"/>), so a frame's identity never reaches a file.
/// </summary>
public enum EnemyFrameId
{
    /// <summary>Boyon: resting, round.</summary>
    BoyonRestingRound,
    /// <summary>Boyon: resting, round and glinting.</summary>
    BoyonRestingGlinting,
    /// <summary>Boyon: resting, squashed low.</summary>
    BoyonRestingSquat,
    /// <summary>Boyon: bouncing, bulging out of shape.</summary>
    BoyonBouncingBulging,
    /// <summary>Boyon: bouncing, pulled into a star.</summary>
    BoyonBouncingStarShaped,
    /// <summary>Boyon: bouncing, a star spread wider.</summary>
    BoyonBouncingStarSpread,
    /// <summary>Boyon: bouncing, a star with its points spiked out.</summary>
    BoyonBouncingStarSpiked,
    /// <summary>Cacatac: upright on the floor, legs splayed.</summary>
    CacatacUprightSplayed,
    /// <summary>Cacatac: upright on the floor, legs drawing in.</summary>
    CacatacUprightDrawingIn,
    /// <summary>Cacatac: upright on the floor, legs drawn in.</summary>
    CacatacUprightDrawnIn,
    /// <summary>Cacatac: upright on the floor, crouching on drawn-in legs.</summary>
    CacatacUprightCrouching,
    /// <summary>Cacatac: upright on the floor, legs gathered under it, low.</summary>
    CacatacUprightGathered,
    /// <summary>Cacatac: upright on the floor, legs gathered under it, standing.</summary>
    CacatacUprightGatheredTall,
    /// <summary>Cacatac: upright on the floor, legs spreading.</summary>
    CacatacUprightSpreading,
    /// <summary>Cacatac: upright on the floor, legs stretched tall.</summary>
    CacatacUprightStretched,
    /// <summary>Cacatac: upright on the floor, puffing up to shoot its spikes.</summary>
    CacatacUprightPuffingUp,
    /// <summary>Cacatac: upright on the floor, puffed up to shoot its spikes.</summary>
    CacatacUprightPuffedUp,
    /// <summary>Cacatac: upside down on the ceiling, legs splayed.</summary>
    CacatacUpsideDownSplayed,
    /// <summary>Cacatac: upside down on the ceiling, legs drawing in.</summary>
    CacatacUpsideDownDrawingIn,
    /// <summary>Cacatac: upside down on the ceiling, legs drawn in.</summary>
    CacatacUpsideDownDrawnIn,
    /// <summary>Cacatac: upside down on the ceiling, crouching on drawn-in legs.</summary>
    CacatacUpsideDownCrouching,
    /// <summary>Cacatac: upside down on the ceiling, legs gathered under it, low.</summary>
    CacatacUpsideDownGathered,
    /// <summary>Cacatac: upside down on the ceiling, legs gathered under it, standing.</summary>
    CacatacUpsideDownGatheredTall,
    /// <summary>Cacatac: upside down on the ceiling, legs spreading.</summary>
    CacatacUpsideDownSpreading,
    /// <summary>Cacatac: upside down on the ceiling, legs stretched tall.</summary>
    CacatacUpsideDownStretched,
    /// <summary>Cacatac: upside down on the ceiling, puffing up to shoot its spikes.</summary>
    CacatacUpsideDownPuffingUp,
    /// <summary>Cacatac: upside down on the ceiling, puffed up to shoot its spikes.</summary>
    CacatacUpsideDownPuffedUp,
    /// <summary>The boulder: rolling, upright.</summary>
    BoulderTurned0,
    /// <summary>The boulder: rolling, turned an eighth.</summary>
    BoulderTurned45,
    /// <summary>The boulder: rolling, turned a quarter.</summary>
    BoulderTurned90,
    /// <summary>The boulder: rolling, turned three eighths.</summary>
    BoulderTurned135,
    /// <summary>The boulder: rolling, turned half way.</summary>
    BoulderTurned180,
    /// <summary>The boulder: rolling, turned five eighths.</summary>
    BoulderTurned225,
    /// <summary>The boulder: rolling, turned three quarters.</summary>
    BoulderTurned270,
    /// <summary>The boulder: rolling, turned seven eighths.</summary>
    BoulderTurned315,
    /// <summary>Atomic: drifting up and right, its orbiting lights upright.</summary>
    AtomicFacingUpRightTurned0,
    /// <summary>Atomic: drifting up and right, its orbiting lights turned a sixth.</summary>
    AtomicFacingUpRightTurned60,
    /// <summary>Atomic: drifting up and right, its orbiting lights turned a third.</summary>
    AtomicFacingUpRightTurned120,
    /// <summary>Atomic: drifting up and right, its orbiting lights turned half way.</summary>
    AtomicFacingUpRightTurned180,
    /// <summary>Atomic: drifting up and right, its orbiting lights turned two thirds.</summary>
    AtomicFacingUpRightTurned240,
    /// <summary>Atomic: drifting up and right, its orbiting lights turned five sixths.</summary>
    AtomicFacingUpRightTurned300,
    /// <summary>Atomic: drifting up and left, its orbiting lights upright.</summary>
    AtomicFacingUpLeftTurned0,
    /// <summary>Atomic: drifting up and left, its orbiting lights turned a sixth.</summary>
    AtomicFacingUpLeftTurned60,
    /// <summary>Atomic: drifting up and left, its orbiting lights turned a third.</summary>
    AtomicFacingUpLeftTurned120,
    /// <summary>Atomic: drifting up and left, its orbiting lights turned half way.</summary>
    AtomicFacingUpLeftTurned180,
    /// <summary>Atomic: drifting up and left, its orbiting lights turned two thirds.</summary>
    AtomicFacingUpLeftTurned240,
    /// <summary>Atomic: drifting up and left, its orbiting lights turned five sixths.</summary>
    AtomicFacingUpLeftTurned300,
    /// <summary>Skultera: swimming left, tail straight.</summary>
    SkulteraSwimmingLeftTailStraight,
    /// <summary>Skultera: swimming left, tail waving.</summary>
    SkulteraSwimmingLeftTailWaving,
    /// <summary>Skultera: swimming left, tail flicking.</summary>
    SkulteraSwimmingLeftTailFlicking,
    /// <summary>Skultera: turning from facing left toward facing right, 22 degrees round.</summary>
    SkulteraTurningRightBy22,
    /// <summary>Skultera: turning from facing left toward facing right, 45 degrees round.</summary>
    SkulteraTurningRightBy45,
    /// <summary>Skultera: turning from facing left toward facing right, 67 degrees round.</summary>
    SkulteraTurningRightBy67,
    /// <summary>Skultera: turning from facing left toward facing right, 90 degrees round.</summary>
    SkulteraTurningRightBy90,
    /// <summary>Skultera: turning from facing left toward facing right, 112 degrees round.</summary>
    SkulteraTurningRightBy112,
    /// <summary>Skultera: turning from facing left toward facing right, 135 degrees round.</summary>
    SkulteraTurningRightBy135,
    /// <summary>Skultera: turning from facing left toward facing right, 157 degrees round.</summary>
    SkulteraTurningRightBy157,
    /// <summary>Skultera: turning from facing left toward facing right, 180 degrees round.</summary>
    SkulteraTurningRightBy180,
    /// <summary>Skultera: swimming right, tail straight.</summary>
    SkulteraSwimmingRightTailStraight,
    /// <summary>Skultera: swimming right, tail waving.</summary>
    SkulteraSwimmingRightTailWaving,
    /// <summary>Skultera: swimming right, tail flicking.</summary>
    SkulteraSwimmingRightTailFlicking,
    /// <summary>Skultera: turning from facing right toward facing left, 22 degrees round.</summary>
    SkulteraTurningLeftBy22,
    /// <summary>Skultera: turning from facing right toward facing left, 45 degrees round.</summary>
    SkulteraTurningLeftBy45,
    /// <summary>Skultera: turning from facing right toward facing left, 67 degrees round.</summary>
    SkulteraTurningLeftBy67,
    /// <summary>Skultera: turning from facing right toward facing left, 90 degrees round.</summary>
    SkulteraTurningLeftBy90,
    /// <summary>Skultera: turning from facing right toward facing left, 112 degrees round.</summary>
    SkulteraTurningLeftBy112,
    /// <summary>Skultera: turning from facing right toward facing left, 135 degrees round.</summary>
    SkulteraTurningLeftBy135,
    /// <summary>Skultera: turning from facing right toward facing left, 157 degrees round.</summary>
    SkulteraTurningLeftBy157,
    /// <summary>Skultera: turning from facing right toward facing left, 180 degrees round.</summary>
    SkulteraTurningLeftBy180,
    /// <summary>Waver: flying level, facing left.</summary>
    WaverLevelFacingLeft,
    /// <summary>Waver: flying level, facing right.</summary>
    WaverLevelFacingRight,
    /// <summary>Waver: rolling over facing left, edge on.</summary>
    WaverRollingFacingLeftEdgeOn,
    /// <summary>Waver: rolling over facing left, tilted to show its underside.</summary>
    WaverRollingFacingLeftTilted,
    /// <summary>Waver: rolling over facing left, showing its underside.</summary>
    WaverRollingFacingLeftUnderside,
    /// <summary>Waver: rolling over facing left, turning back edge on.</summary>
    WaverRollingFacingLeftTurningBack,
    /// <summary>Waver: rolling over facing right, edge on.</summary>
    WaverRollingFacingRightEdgeOn,
    /// <summary>Waver: rolling over facing right, tilted to show its underside.</summary>
    WaverRollingFacingRightTilted,
    /// <summary>Waver: rolling over facing right, showing its underside.</summary>
    WaverRollingFacingRightUnderside,
    /// <summary>Waver: rolling over facing right, turning back edge on.</summary>
    WaverRollingFacingRightTurningBack,
    /// <summary>Metaree: hanging, facing the viewer, wings folded around it.</summary>
    MetareeFacingViewer,
    /// <summary>Metaree: hanging, turning away at an angle.</summary>
    MetareeTurningOblique,
    /// <summary>Metaree: hanging, turned edge on.</summary>
    MetareeEdgeOn,
    /// <summary>Metaree: hanging, turning back at an angle.</summary>
    MetareeReturningOblique,
    /// <summary>Metaree: facing the viewer, wings flared to dive.</summary>
    MetareeWingsFlared,
    /// <summary>Skree: hanging, facing the viewer, wings folded around it.</summary>
    SkreeFacingViewer,
    /// <summary>Skree: hanging, turning away at an angle.</summary>
    SkreeTurningOblique,
    /// <summary>Skree: hanging, turned edge on.</summary>
    SkreeEdgeOn,
    /// <summary>Skree: hanging, turning back at an angle.</summary>
    SkreeReturningOblique,
    /// <summary>Skree: facing the viewer, wings flared to dive.</summary>
    SkreeWingsFlared,
    /// <summary>Zoa: darting facing left, tail tucked.</summary>
    ZoaDartingFacingLeftTailTucked,
    /// <summary>Zoa: darting facing left, tail kicking.</summary>
    ZoaDartingFacingLeftTailKicking,
    /// <summary>Zoa: darting facing left, tail extended.</summary>
    ZoaDartingFacingLeftTailExtended,
    /// <summary>Zoa: swimming up facing left, legs drawn in.</summary>
    ZoaSwimmingFacingLeftLegsIn,
    /// <summary>Zoa: swimming up facing left, legs paddling.</summary>
    ZoaSwimmingFacingLeftLegsPaddling,
    /// <summary>Zoa: swimming up facing left, legs stretched out.</summary>
    ZoaSwimmingFacingLeftLegsOut,
    /// <summary>Zoa: darting facing right, tail tucked.</summary>
    ZoaDartingFacingRightTailTucked,
    /// <summary>Zoa: darting facing right, tail kicking.</summary>
    ZoaDartingFacingRightTailKicking,
    /// <summary>Zoa: darting facing right, tail extended.</summary>
    ZoaDartingFacingRightTailExtended,
    /// <summary>Zoa: swimming up facing right, legs drawn in.</summary>
    ZoaSwimmingFacingRightLegsIn,
    /// <summary>Zoa: swimming up facing right, legs paddling.</summary>
    ZoaSwimmingFacingRightLegsPaddling,
    /// <summary>Zoa: swimming up facing right, legs stretched out.</summary>
    ZoaSwimmingFacingRightLegsOut,
    /// <summary>Zeb: rising facing left, wings up.</summary>
    ZebFacingLeftWingsUp,
    /// <summary>Zeb: rising facing left, wings raised.</summary>
    ZebFacingLeftWingsRaised,
    /// <summary>Zeb: rising facing left, wings level.</summary>
    ZebFacingLeftWingsLevel,
    /// <summary>Zeb: rising facing left, wings lowered.</summary>
    ZebFacingLeftWingsLowered,
    /// <summary>Zeb: rising facing left, wings down.</summary>
    ZebFacingLeftWingsDown,
    /// <summary>Zeb: rising facing right, wings up.</summary>
    ZebFacingRightWingsUp,
    /// <summary>Zeb: rising facing right, wings raised.</summary>
    ZebFacingRightWingsRaised,
    /// <summary>Zeb: rising facing right, wings level.</summary>
    ZebFacingRightWingsLevel,
    /// <summary>Zeb: rising facing right, wings lowered.</summary>
    ZebFacingRightWingsLowered,
    /// <summary>Zeb: rising facing right, wings down.</summary>
    ZebFacingRightWingsDown,
    /// <summary>Zebbo: darting facing left, wings up.</summary>
    ZebboDartingFacingLeftWingsUp,
    /// <summary>Zebbo: darting facing left, wings level.</summary>
    ZebboDartingFacingLeftWingsLevel,
    /// <summary>Zebbo: darting facing left, wings down.</summary>
    ZebboDartingFacingLeftWingsDown,
    /// <summary>Zebbo: rising facing left, wings up.</summary>
    ZebboRisingFacingLeftWingsUp,
    /// <summary>Zebbo: rising facing left, wings level.</summary>
    ZebboRisingFacingLeftWingsLevel,
    /// <summary>Zebbo: rising facing left, wings down.</summary>
    ZebboRisingFacingLeftWingsDown,
    /// <summary>Zebbo: darting facing right, wings up.</summary>
    ZebboDartingFacingRightWingsUp,
    /// <summary>Zebbo: darting facing right, wings level.</summary>
    ZebboDartingFacingRightWingsLevel,
    /// <summary>Zebbo: darting facing right, wings down.</summary>
    ZebboDartingFacingRightWingsDown,
    /// <summary>Zebbo: rising facing right, wings up.</summary>
    ZebboRisingFacingRightWingsUp,
    /// <summary>Zebbo: rising facing right, wings level.</summary>
    ZebboRisingFacingRightWingsLevel,
    /// <summary>Zebbo: rising facing right, wings down.</summary>
    ZebboRisingFacingRightWingsDown,
    /// <summary>Gamet: rising facing left, wings up.</summary>
    GametFacingLeftWingsUp,
    /// <summary>Gamet: rising facing left, wings raised.</summary>
    GametFacingLeftWingsRaised,
    /// <summary>Gamet: rising facing left, wings level.</summary>
    GametFacingLeftWingsLevel,
    /// <summary>Gamet: rising facing left, wings lowered.</summary>
    GametFacingLeftWingsLowered,
    /// <summary>Gamet: rising facing left, wings down.</summary>
    GametFacingLeftWingsDown,
    /// <summary>Gamet: rising facing right, wings up.</summary>
    GametFacingRightWingsUp,
    /// <summary>Gamet: rising facing right, wings raised.</summary>
    GametFacingRightWingsRaised,
    /// <summary>Gamet: rising facing right, wings level.</summary>
    GametFacingRightWingsLevel,
    /// <summary>Gamet: rising facing right, wings lowered.</summary>
    GametFacingRightWingsLowered,
    /// <summary>Gamet: rising facing right, wings down.</summary>
    GametFacingRightWingsDown,
    /// <summary>Geega: flying level facing left, wings up.</summary>
    GeegaLevelFacingLeftWingsUp,
    /// <summary>Geega: flying level facing left, wings level.</summary>
    GeegaLevelFacingLeftWingsLevel,
    /// <summary>Geega: flying level facing left, wings down.</summary>
    GeegaLevelFacingLeftWingsDown,
    /// <summary>Geega: tilted into a swoop facing left, wings up.</summary>
    GeegaTiltedFacingLeftWingsUp,
    /// <summary>Geega: tilted into a swoop facing left, wings level.</summary>
    GeegaTiltedFacingLeftWingsLevel,
    /// <summary>Geega: tilted into a swoop facing left, wings down.</summary>
    GeegaTiltedFacingLeftWingsDown,
    /// <summary>Geega: flying level facing right, wings up.</summary>
    GeegaLevelFacingRightWingsUp,
    /// <summary>Geega: flying level facing right, wings level.</summary>
    GeegaLevelFacingRightWingsLevel,
    /// <summary>Geega: flying level facing right, wings down.</summary>
    GeegaLevelFacingRightWingsDown,
    /// <summary>Geega: tilted into a swoop facing right, wings up.</summary>
    GeegaTiltedFacingRightWingsUp,
    /// <summary>Geega: tilted into a swoop facing right, wings level.</summary>
    GeegaTiltedFacingRightWingsLevel,
    /// <summary>Geega: tilted into a swoop facing right, wings down.</summary>
    GeegaTiltedFacingRightWingsDown,
    /// <summary>Fake Kraid: stepping facing left, feet together.</summary>
    FakeKraidSteppingFacingLeftFeetTogether,
    /// <summary>Fake Kraid: stepping facing left, front foot lifting.</summary>
    FakeKraidSteppingFacingLeftFrontFootLifting,
    /// <summary>Fake Kraid: stepping facing left, front foot forward.</summary>
    FakeKraidSteppingFacingLeftFrontFootForward,
    /// <summary>Fake Kraid: stepping facing left, feet planted apart.</summary>
    FakeKraidSteppingFacingLeftFeetPlanted,
    /// <summary>Fake Kraid: facing left, mouth closed.</summary>
    FakeKraidFacingLeftMouthClosed,
    /// <summary>Fake Kraid: facing left, mouth opening.</summary>
    FakeKraidFacingLeftMouthOpening,
    /// <summary>Fake Kraid: facing left, head thrown back, mouth wide open.</summary>
    FakeKraidFacingLeftMouthWide,
    /// <summary>Fake Kraid: stepping facing right, feet together.</summary>
    FakeKraidSteppingFacingRightFeetTogether,
    /// <summary>Fake Kraid: stepping facing right, front foot lifting.</summary>
    FakeKraidSteppingFacingRightFrontFootLifting,
    /// <summary>Fake Kraid: stepping facing right, front foot forward.</summary>
    FakeKraidSteppingFacingRightFrontFootForward,
    /// <summary>Fake Kraid: stepping facing right, feet planted apart.</summary>
    FakeKraidSteppingFacingRightFeetPlanted,
    /// <summary>Fake Kraid: facing right, mouth closed.</summary>
    FakeKraidFacingRightMouthClosed,
    /// <summary>Fake Kraid: facing right, mouth opening.</summary>
    FakeKraidFacingRightMouthOpening,
    /// <summary>Fake Kraid: facing right, head thrown back, mouth wide open.</summary>
    FakeKraidFacingRightMouthWide,
    /// <summary>Kraid: its spinning nail, upright.</summary>
    KraidNailTurned0,
    /// <summary>Kraid: its spinning nail, turned an eighth.</summary>
    KraidNailTurned45,
    /// <summary>Kraid: its spinning nail, turned a quarter.</summary>
    KraidNailTurned90,
    /// <summary>Kraid: its spinning nail, turned three eighths.</summary>
    KraidNailTurned135,
    /// <summary>Kraid: its spinning nail, turned half way.</summary>
    KraidNailTurned180,
    /// <summary>Kraid: its spinning nail, turned five eighths.</summary>
    KraidNailTurned225,
    /// <summary>Kraid: its spinning nail, turned three quarters.</summary>
    KraidNailTurned270,
    /// <summary>Kraid: its spinning nail, turned seven eighths.</summary>
    KraidNailTurned315,
    /// <summary>Owtch: crawling, its feet at rest.</summary>
    OwtchCrawlingFeetAtRest,
    /// <summary>Owtch: crawling, its feet shuffling.</summary>
    OwtchCrawlingFeetShuffling,
    /// <summary>Owtch: crawling, its feet stretched.</summary>
    OwtchCrawlingFeetStretched,
    /// <summary>Gripper: flying left, its jet flame medium.</summary>
    GrappleRipperFlyingLeftFlameMedium,
    /// <summary>Gripper: flying left, its jet flame short.</summary>
    GrappleRipperFlyingLeftFlameShort,
    /// <summary>Gripper: flying left, its jet flame long.</summary>
    GrappleRipperFlyingLeftFlameLong,
    /// <summary>Gripper: flying right, its jet flame medium.</summary>
    GrappleRipperFlyingRightFlameMedium,
    /// <summary>Gripper: flying right, its jet flame short.</summary>
    GrappleRipperFlyingRightFlameShort,
    /// <summary>Gripper: flying right, its jet flame long.</summary>
    GrappleRipperFlyingRightFlameLong,
    /// <summary>Ripper: frozen facing left.</summary>
    RipperFrozenFacingLeft,
    /// <summary>Ripper: frozen facing right.</summary>
    RipperFrozenFacingRight,
    /// <summary>Ripper: flying left, its eye lit.</summary>
    RipperFlyingLeftEyeLit,
    /// <summary>Ripper: flying left, its eye dark.</summary>
    RipperFlyingLeftEyeDark,
    /// <summary>Ripper: flying left, its eye glinting.</summary>
    RipperFlyingLeftEyeGlinting,
    /// <summary>Ripper: flying right, its eye lit.</summary>
    RipperFlyingRightEyeLit,
    /// <summary>Ripper: flying right, its eye dark.</summary>
    RipperFlyingRightEyeDark,
    /// <summary>Ripper: flying right, its eye glinting.</summary>
    RipperFlyingRightEyeGlinting,
    /// <summary>Fireflea: glowing brightest, wings up.</summary>
    FirefleaGlowBrightestWingsUp,
    /// <summary>Fireflea: glowing brightest, wings level.</summary>
    FirefleaGlowBrightestWingsLevel,
    /// <summary>Fireflea: glowing brightest, wings down.</summary>
    FirefleaGlowBrightestWingsDown,
    /// <summary>Fireflea: glowing brighter, wings up.</summary>
    FirefleaGlowBrighterWingsUp,
    /// <summary>Fireflea: glowing brighter, wings level.</summary>
    FirefleaGlowBrighterWingsLevel,
    /// <summary>Fireflea: glowing brighter, wings down.</summary>
    FirefleaGlowBrighterWingsDown,
    /// <summary>Fireflea: glowing bright, wings up.</summary>
    FirefleaGlowBrightWingsUp,
    /// <summary>Fireflea: glowing bright, wings level.</summary>
    FirefleaGlowBrightWingsLevel,
    /// <summary>Fireflea: glowing bright, wings down.</summary>
    FirefleaGlowBrightWingsDown,
    /// <summary>Fireflea: glowing half bright, wings up.</summary>
    FirefleaGlowMidWingsUp,
    /// <summary>Fireflea: glowing half bright, wings level.</summary>
    FirefleaGlowMidWingsLevel,
    /// <summary>Fireflea: glowing half bright, wings down.</summary>
    FirefleaGlowMidWingsDown,
    /// <summary>Fireflea: glowing dim, wings up.</summary>
    FirefleaGlowDimWingsUp,
    /// <summary>Fireflea: glowing dim, wings level.</summary>
    FirefleaGlowDimWingsLevel,
    /// <summary>Fireflea: glowing dim, wings down.</summary>
    FirefleaGlowDimWingsDown,
    /// <summary>Fireflea: glowing dimmer, wings up.</summary>
    FirefleaGlowDimmerWingsUp,
    /// <summary>Fireflea: glowing dimmer, wings level.</summary>
    FirefleaGlowDimmerWingsLevel,
    /// <summary>Fireflea: glowing dimmer, wings down.</summary>
    FirefleaGlowDimmerWingsDown,
    /// <summary>Fireflea: glowing dimmest, wings up.</summary>
    FirefleaGlowDimmestWingsUp,
    /// <summary>Fireflea: glowing dimmest, wings level.</summary>
    FirefleaGlowDimmestWingsLevel,
    /// <summary>Fireflea: glowing dimmest, wings down.</summary>
    FirefleaGlowDimmestWingsDown,
    /// <summary>Magdollite: its head above the lava facing left, bobbing at its highest.</summary>
    MagdolliteHeadFacingLeftHigh,
    /// <summary>Magdollite: its head above the lava facing left, bobbing half way.</summary>
    MagdolliteHeadFacingLeftMid,
    /// <summary>Magdollite: its head above the lava facing left, bobbing at its lowest.</summary>
    MagdolliteHeadFacingLeftLow,
    /// <summary>Magdollite: rearing up tall, facing left.</summary>
    MagdolliteRearingFacingLeft,
    /// <summary>Magdollite: leaning forward, facing left.</summary>
    MagdolliteLeaningFacingLeft,
    /// <summary>Magdollite: stretched out sideways, facing left.</summary>
    MagdolliteReachingFacingLeft,
    /// <summary>Magdollite: flattened in a swipe, facing left.</summary>
    MagdolliteThrowingFacingLeft,
    /// <summary>Magdollite: pillar cap.</summary>
    MagdollitePillarCap,
    /// <summary>Magdollite: a low mound of lava with an eye, facing left.</summary>
    MagdolliteMoundFacingLeft,
    /// <summary>Magdollite: sinking flat into the lava, facing left.</summary>
    MagdolliteSinkingFacingLeft,
    /// <summary>Magdollite: sunk almost flat into the lava, facing left.</summary>
    MagdolliteSunkFacingLeft,
    /// <summary>Magdollite: its head above the lava facing right, bobbing at its highest.</summary>
    MagdolliteHeadFacingRightHigh,
    /// <summary>Magdollite: its head above the lava facing right, bobbing half way.</summary>
    MagdolliteHeadFacingRightMid,
    /// <summary>Magdollite: its head above the lava facing right, bobbing at its lowest.</summary>
    MagdolliteHeadFacingRightLow,
    /// <summary>Magdollite: rearing up tall, facing right.</summary>
    MagdolliteRearingFacingRight,
    /// <summary>Magdollite: leaning forward, facing right.</summary>
    MagdolliteLeaningFacingRight,
    /// <summary>Magdollite: stretched out sideways, facing right.</summary>
    MagdolliteReachingFacingRight,
    /// <summary>Magdollite: flattened in a swipe, facing right.</summary>
    MagdolliteThrowingFacingRight,
    /// <summary>Magdollite: a low mound of lava with an eye, facing right.</summary>
    MagdolliteMoundFacingRight,
    /// <summary>Magdollite: sinking flat into the lava, facing right.</summary>
    MagdolliteSinkingFacingRight,
    /// <summary>Magdollite: sunk almost flat into the lava, facing right.</summary>
    MagdolliteSunkFacingRight,
    /// <summary>Magdollite: the pillar one segment high.</summary>
    MagdolliteOneSegmentPillar,
    /// <summary>Magdollite: the pillar two segments high.</summary>
    MagdolliteTwoSegmentPillar,
    /// <summary>Magdollite: the pillar three segments high.</summary>
    MagdolliteThreeSegmentPillar,
    /// <summary>Magdollite: the pillar four segments high.</summary>
    MagdolliteFourSegmentPillar,
    /// <summary>Magdollite: the pillar five segments high.</summary>
    MagdolliteFiveSegmentPillar,
    /// <summary>Magdollite: the pillar six segments high.</summary>
    MagdolliteSixSegmentPillar,
    /// <summary>Magdollite: the pillar seven segments high.</summary>
    MagdolliteSevenSegmentPillar,
    /// <summary>Magdollite: the pillar eight segments high.</summary>
    MagdolliteEightSegmentPillar,
    /// <summary>The Ceres elevator room's blocked door, standing until the station starts to self-destruct.</summary>
    CeresElevatorDoorBlocked,
    /// <summary>A Ceres door facing left: closed.</summary>
    CeresDoorFacingLeftClosed,
    /// <summary>A Ceres door facing left: a quarter open.</summary>
    CeresDoorFacingLeftQuarterOpen,
    /// <summary>A Ceres door facing left: half open.</summary>
    CeresDoorFacingLeftHalfOpen,
    /// <summary>A Ceres door facing left: three quarters open.</summary>
    CeresDoorFacingLeftThreeQuartersOpen,
    /// <summary>A Ceres door facing left: open.</summary>
    CeresDoorFacingLeftOpen,
    /// <summary>A Ceres door facing right: closed.</summary>
    CeresDoorFacingRightClosed,
    /// <summary>A Ceres door facing right: a quarter open.</summary>
    CeresDoorFacingRightQuarterOpen,
    /// <summary>A Ceres door facing right: half open.</summary>
    CeresDoorFacingRightHalfOpen,
    /// <summary>A Ceres door facing right: three quarters open.</summary>
    CeresDoorFacingRightThreeQuartersOpen,
    /// <summary>A Ceres door facing right: open.</summary>
    CeresDoorFacingRightOpen,
    /// <summary>Every Ceres door and wall: a small square above and left of its origin.</summary>
    CeresDoorOrWallSmallSquare,
    /// <summary>The left wall of Ceres Ridley's getaway chamber.</summary>
    CeresGetawayChamberLeftWall,
    /// <summary>The right wall of Ceres Ridley's getaway chamber.</summary>
    CeresGetawayChamberRightWall,
    /// <summary>The door of Ceres Ridley's room, closed, as Ridley's scene draws it while the door hands its drawing to him: the picture of CeresDoorFacingRightClosed.</summary>
    CeresRidleyRoomDoorClosed,
    /// <summary>The Ceres baby Metroid: horizontal.</summary>
    CeresBabyHorizontal,
    /// <summary>The Ceres baby Metroid: round.</summary>
    CeresBabyRound,
    /// <summary>The Ceres baby Metroid: vertical.</summary>
    CeresBabyVertical,
    /// <summary>Rio: wings up, legs tucked.</summary>
    RioWingsUpLegsTucked,
    /// <summary>Rio: wings level, legs tucked.</summary>
    RioWingsLevelLegsTucked,
    /// <summary>Rio: wings up, legs hanging.</summary>
    RioWingsUpLegsHanging,
    /// <summary>Rio: wings level, legs hanging.</summary>
    RioWingsLevelLegsHanging,
    /// <summary>Rio: wings up, legs spread wide.</summary>
    RioWingsUpLegsSpread,
    /// <summary>Rio: wings level, legs spread wide.</summary>
    RioWingsLevelLegsSpread,
    /// <summary>Rio: wings up, claws curled.</summary>
    RioWingsUpClawsCurled,
    /// <summary>Rio: wings level, claws curled.</summary>
    RioWingsLevelClawsCurled,
    /// <summary>The Lower Norfair Rio: hovering, wings raised.</summary>
    LowerNorfairRioWingsRaised,
    /// <summary>The Lower Norfair Rio: hovering, wings level.</summary>
    LowerNorfairRioWingsLevel,
    /// <summary>The Lower Norfair Rio: hovering, wings level, a pixel lower.</summary>
    LowerNorfairRioWingsLevelDropped,
    /// <summary>The Lower Norfair Rio: wings lifting.</summary>
    LowerNorfairRioWingsLifting,
    /// <summary>The Lower Norfair Rio: wings tilting out.</summary>
    LowerNorfairRioWingsTilting,
    /// <summary>The Lower Norfair Rio: wings opening.</summary>
    LowerNorfairRioWingsOpening,
    /// <summary>The Lower Norfair Rio: wings spread.</summary>
    LowerNorfairRioWingsSpread,
    /// <summary>The Lower Norfair Rio: wings fully spread.</summary>
    LowerNorfairRioWingsFullySpread,
    /// <summary>The Lower Norfair Rio: wings closing.</summary>
    LowerNorfairRioWingsClosing,
    /// <summary>The Lower Norfair Rio: wings folding.</summary>
    LowerNorfairRioWingsFolding,
    /// <summary>The Lower Norfair Rio: wings folded back.</summary>
    LowerNorfairRioWingsFolded,
    /// <summary>The Lower Norfair Rio: wings swept back.</summary>
    LowerNorfairRioWingsSwept,
    /// <summary>The Lower Norfair Rio: wings swept back: the picture of LowerNorfairRioWingsSwept, as a frame of its own.</summary>
    LowerNorfairRioWingsSweptTwinB,
    /// <summary>The Lower Norfair Rio: wings swept back: the picture of LowerNorfairRioWingsSwept, as a frame of its own.</summary>
    LowerNorfairRioWingsSweptTwinC,
    /// <summary>The Lower Norfair Rio: wings swept back: the picture of LowerNorfairRioWingsSwept, as a frame of its own.</summary>
    LowerNorfairRioWingsSweptTwin,
    /// <summary>The Lower Norfair Rio: the flame trailing it, large.</summary>
    LowerNorfairRioFlameLarge,
    /// <summary>The Lower Norfair Rio: the flame trailing it, medium.</summary>
    LowerNorfairRioFlameMedium,
    /// <summary>The Lower Norfair Rio: the flame trailing it, small.</summary>
    LowerNorfairRioFlameSmall,
    /// <summary>The Norfair Rio: hovering, wings level.</summary>
    NorfairRioWingsLevel,
    /// <summary>The Norfair Rio: hovering, wings spread.</summary>
    NorfairRioWingsSpread,
    /// <summary>The Norfair Rio: hovering, wings drooping.</summary>
    NorfairRioWingsDrooping,
    /// <summary>The Norfair Rio: the flame trailing it as it rises, large.</summary>
    NorfairRioRisingFlameLarge,
    /// <summary>The Norfair Rio: the flame trailing it as it rises, medium.</summary>
    NorfairRioRisingFlameMedium,
    /// <summary>The Norfair Rio: the flame trailing it as it rises, small.</summary>
    NorfairRioRisingFlameSmall,
    /// <summary>The Norfair Rio: diving, wings swept up.</summary>
    NorfairRioDiving,
    /// <summary>The Norfair Rio: diving, wings swept up: the picture of NorfairRioDiving, as a frame of its own.</summary>
    NorfairRioDivingTwin,
    /// <summary>The Norfair Rio: diving, wings swept up: the picture of NorfairRioDiving, as a frame of its own.</summary>
    NorfairRioDivingTwinB,
    /// <summary>The Norfair Rio: wings flattening.</summary>
    NorfairRioWingsFlattening,
    /// <summary>The Norfair Rio: wings tilting up.</summary>
    NorfairRioWingsTilting,
    /// <summary>The Norfair Rio: wings lifting.</summary>
    NorfairRioWingsLifting,
    /// <summary>The Norfair Rio: wings raising.</summary>
    NorfairRioWingsRaising,
    /// <summary>The Norfair Rio: wings swept up over it.</summary>
    NorfairRioWingsSweptUp,
    /// <summary>The Norfair Rio: climbing, wings spread low.</summary>
    NorfairRioClimbing,
    /// <summary>The Norfair Rio: climbing, wings spread low: the picture of NorfairRioClimbing, as a frame of its own.</summary>
    NorfairRioClimbingTwin,
    /// <summary>The Norfair Rio: climbing, wings spread low: the picture of NorfairRioClimbing, as a frame of its own.</summary>
    NorfairRioClimbingTwinB,
    /// <summary>The Norfair Rio: the flame trailing it as it falls, large.</summary>
    NorfairRioFallingFlameLarge,
    /// <summary>The Norfair Rio: the flame trailing it as it falls, medium.</summary>
    NorfairRioFallingFlameMedium,
    /// <summary>The Norfair Rio: the flame trailing it as it falls, small.</summary>
    NorfairRioFallingFlameSmall,
    /// <summary>Puyo: on the ground, domed tall.</summary>
    PuyoGroundedTall,
    /// <summary>Puyo: on the ground, flattening.</summary>
    PuyoGroundedFlattening,
    /// <summary>Puyo: on the ground, flat.</summary>
    PuyoGroundedFlat,
    /// <summary>Puyo: in the air, curling open.</summary>
    PuyoAirborneOpening,
    /// <summary>Puyo: in the air, half spread.</summary>
    PuyoAirborneHalfOpen,
    /// <summary>Puyo: in the air, spread wide.</summary>
    PuyoAirborneSpread,
    /// <summary>Puyo: in the air, half folded back.</summary>
    PuyoAirborneHalfClosed,
    /// <summary>Puyo: in the air, folding closed.</summary>
    PuyoAirborneClosing,
    /// <summary>Bull: floating, tentacles upright.</summary>
    BullTentaclesUpright,
    /// <summary>Bull: floating, tentacles tilted.</summary>
    BullTentaclesTilted,
    /// <summary>Bull: floating, tentacles tilted further.</summary>
    BullTentaclesTiltedFurther,
    /// <summary>Alcoon: walking left, its near leg stepping forward.</summary>
    AlcoonWalkingLeftNearLegStepping,
    /// <summary>Alcoon: walking left, its near leg planted.</summary>
    AlcoonWalkingLeftNearLegPlanted,
    /// <summary>Alcoon: walking left, its far leg stepping forward.</summary>
    AlcoonWalkingLeftFarLegStepping,
    /// <summary>Alcoon: walking left, its far leg planted.</summary>
    AlcoonWalkingLeftFarLegPlanted,
    /// <summary>Alcoon: standing facing left, mouth closed.</summary>
    AlcoonFacingLeftMouthClosed,
    /// <summary>Alcoon: standing facing left, mouth opening.</summary>
    AlcoonFacingLeftMouthOpening,
    /// <summary>Alcoon: standing facing left, mouth open, a fireball glowing in its throat.</summary>
    AlcoonFacingLeftThroatGlowing,
    /// <summary>Alcoon: standing facing left, mouth wide open, snout raised.</summary>
    AlcoonFacingLeftMouthWide,
    /// <summary>Alcoon: stretched upright in a leap, facing left.</summary>
    AlcoonLeapingFacingLeft,
    /// <summary>Alcoon: walking right, its near leg stepping forward.</summary>
    AlcoonWalkingRightNearLegStepping,
    /// <summary>Alcoon: walking right, its near leg planted.</summary>
    AlcoonWalkingRightNearLegPlanted,
    /// <summary>Alcoon: walking right, its far leg stepping forward.</summary>
    AlcoonWalkingRightFarLegStepping,
    /// <summary>Alcoon: walking right, its far leg planted.</summary>
    AlcoonWalkingRightFarLegPlanted,
    /// <summary>Alcoon: standing facing right, mouth closed.</summary>
    AlcoonFacingRightMouthClosed,
    /// <summary>Alcoon: standing facing right, mouth opening.</summary>
    AlcoonFacingRightMouthOpening,
    /// <summary>Alcoon: standing facing right, mouth open, a fireball glowing in its throat.</summary>
    AlcoonFacingRightThroatGlowing,
    /// <summary>Alcoon: standing facing right, mouth wide open, snout raised.</summary>
    AlcoonFacingRightMouthWide,
    /// <summary>Alcoon: stretched upright in a leap, facing right.</summary>
    AlcoonLeapingFacingRight,
    /// <summary>Beetom: crawling left, legs tucked.</summary>
    BeetomCrawlingLeftLegsTucked,
    /// <summary>Beetom: crawling left, legs part way out.</summary>
    BeetomCrawlingLeftLegsMid,
    /// <summary>Beetom: crawling left, legs out.</summary>
    BeetomCrawlingLeftLegsOut,
    /// <summary>Beetom: hopping left, its hind leg pushing off.</summary>
    BeetomHoppingLeftLegPushing,
    /// <summary>Beetom: hopping left, its hind leg stretched out.</summary>
    BeetomHoppingLeftLegStretched,
    /// <summary>Beetom: latched on facing left, its gem bright green.</summary>
    BeetomLatchedFacingLeftGemGreenBright,
    /// <summary>Beetom: latched on facing left, its gem green.</summary>
    BeetomLatchedFacingLeftGemGreen,
    /// <summary>Beetom: latched on facing left, its gem dim green.</summary>
    BeetomLatchedFacingLeftGemGreenDim,
    /// <summary>Beetom: latched on facing left, its gem bright red.</summary>
    BeetomLatchedFacingLeftGemRedBright,
    /// <summary>Beetom: latched on facing left, its gem red.</summary>
    BeetomLatchedFacingLeftGemRed,
    /// <summary>Beetom: latched on facing left, its gem dark red.</summary>
    BeetomLatchedFacingLeftGemRedDim,
    /// <summary>Beetom: crawling right, legs tucked.</summary>
    BeetomCrawlingRightLegsTucked,
    /// <summary>Beetom: crawling right, legs part way out.</summary>
    BeetomCrawlingRightLegsMid,
    /// <summary>Beetom: crawling right, legs out.</summary>
    BeetomCrawlingRightLegsOut,
    /// <summary>Beetom: hopping right, its hind leg pushing off.</summary>
    BeetomHoppingRightLegPushing,
    /// <summary>Beetom: hopping right, its hind leg stretched out.</summary>
    BeetomHoppingRightLegStretched,
    /// <summary>Beetom: latched on facing right, its gem bright green.</summary>
    BeetomLatchedFacingRightGemGreenBright,
    /// <summary>Beetom: latched on facing right, its gem green.</summary>
    BeetomLatchedFacingRightGemGreen,
    /// <summary>Beetom: latched on facing right, its gem dim green.</summary>
    BeetomLatchedFacingRightGemGreenDim,
    /// <summary>Beetom: latched on facing right, its gem bright red.</summary>
    BeetomLatchedFacingRightGemRedBright,
    /// <summary>Beetom: latched on facing right, its gem red.</summary>
    BeetomLatchedFacingRightGemRed,
    /// <summary>Beetom: latched on facing right, its gem dark red.</summary>
    BeetomLatchedFacingRightGemRedDim,
    /// <summary>Sidehopper: on the floor, legs straight.</summary>
    SidehopperLegsStraight,
    /// <summary>Sidehopper: on the floor, legs bent.</summary>
    SidehopperLegsBent,
    /// <summary>Sidehopper: on the floor, crouched low.</summary>
    SidehopperLegsCrouched,
    /// <summary>Sidehopper: upside down on the ceiling, legs straight.</summary>
    SidehopperUpsideDownLegsStraight,
    /// <summary>Sidehopper: upside down on the ceiling, legs bent.</summary>
    SidehopperUpsideDownLegsBent,
    /// <summary>Sidehopper: upside down on the ceiling, crouched low.</summary>
    SidehopperUpsideDownLegsCrouched,
    /// <summary>Dessgeega: on the floor, legs straight.</summary>
    DessgeegaLegsStraight,
    /// <summary>Dessgeega: on the floor, legs bent.</summary>
    DessgeegaLegsBent,
    /// <summary>Dessgeega: on the floor, crouched low.</summary>
    DessgeegaLegsCrouched,
    /// <summary>Dessgeega: upside down on the ceiling, legs straight.</summary>
    DessgeegaUpsideDownLegsStraight,
    /// <summary>Dessgeega: upside down on the ceiling, legs bent.</summary>
    DessgeegaUpsideDownLegsBent,
    /// <summary>Dessgeega: upside down on the ceiling, crouched low.</summary>
    DessgeegaUpsideDownLegsCrouched,
    /// <summary>The large Sidehopper: on the floor, legs straight.</summary>
    LargeSidehopperLegsStraight,
    /// <summary>The large Sidehopper: on the floor, legs bent.</summary>
    LargeSidehopperLegsBent,
    /// <summary>The large Sidehopper: on the floor, crouched low.</summary>
    LargeSidehopperLegsCrouched,
    /// <summary>The large Sidehopper: upside down on the ceiling, legs straight.</summary>
    LargeSidehopperUpsideDownLegsStraight,
    /// <summary>The large Sidehopper: upside down on the ceiling, legs bent.</summary>
    LargeSidehopperUpsideDownLegsBent,
    /// <summary>The large Sidehopper: upside down on the ceiling, crouched low.</summary>
    LargeSidehopperUpsideDownLegsCrouched,
    /// <summary>The large Dessgeega: on the floor, legs straight.</summary>
    LargeDessgeegaLegsStraight,
    /// <summary>The large Dessgeega: on the floor, legs bent.</summary>
    LargeDessgeegaLegsBent,
    /// <summary>The large Dessgeega: on the floor, crouched low.</summary>
    LargeDessgeegaLegsCrouched,
    /// <summary>The large Dessgeega: upside down on the ceiling, legs straight.</summary>
    LargeDessgeegaUpsideDownLegsStraight,
    /// <summary>The large Dessgeega: upside down on the ceiling, legs bent.</summary>
    LargeDessgeegaUpsideDownLegsBent,
    /// <summary>The large Dessgeega: upside down on the ceiling, crouched low.</summary>
    LargeDessgeegaUpsideDownLegsCrouched,
    /// <summary>Choot: lying flat.</summary>
    ChootLyingFlat,
    /// <summary>Choot: springing up, its edges folding.</summary>
    ChootSpringing,
    /// <summary>Choot: folded up.</summary>
    ChootFolded,
    /// <summary>Choot: spread out wide as it drifts down.</summary>
    ChootSpreadOut,
    /// <summary>The horizontal Zoomer: crawling with its top right, its spines turned 0 degrees.</summary>
    OrangeZoomerUpsideRightSpinesTurned0,
    /// <summary>The horizontal Zoomer: crawling with its top right, its spines turned 12 degrees.</summary>
    OrangeZoomerUpsideRightSpinesTurned12,
    /// <summary>The horizontal Zoomer: crawling with its top right, its spines turned 24 degrees.</summary>
    OrangeZoomerUpsideRightSpinesTurned24,
    /// <summary>The horizontal Zoomer: crawling with its top right, its spines turned 36 degrees.</summary>
    OrangeZoomerUpsideRightSpinesTurned36,
    /// <summary>The horizontal Zoomer: crawling with its top right, its spines turned 48 degrees.</summary>
    OrangeZoomerUpsideRightSpinesTurned48,
    /// <summary>The horizontal Zoomer: crawling with its top left, its spines turned 0 degrees.</summary>
    OrangeZoomerUpsideLeftSpinesTurned0,
    /// <summary>The horizontal Zoomer: crawling with its top left, its spines turned 12 degrees.</summary>
    OrangeZoomerUpsideLeftSpinesTurned12,
    /// <summary>The horizontal Zoomer: crawling with its top left, its spines turned 24 degrees.</summary>
    OrangeZoomerUpsideLeftSpinesTurned24,
    /// <summary>The horizontal Zoomer: crawling with its top left, its spines turned 36 degrees.</summary>
    OrangeZoomerUpsideLeftSpinesTurned36,
    /// <summary>The horizontal Zoomer: crawling with its top left, its spines turned 48 degrees.</summary>
    OrangeZoomerUpsideLeftSpinesTurned48,
    /// <summary>The horizontal Zoomer: crawling with its top down, its spines turned 0 degrees.</summary>
    OrangeZoomerUpsideDownSpinesTurned0,
    /// <summary>The horizontal Zoomer: crawling with its top down, its spines turned 12 degrees.</summary>
    OrangeZoomerUpsideDownSpinesTurned12,
    /// <summary>The horizontal Zoomer: crawling with its top down, its spines turned 24 degrees.</summary>
    OrangeZoomerUpsideDownSpinesTurned24,
    /// <summary>The horizontal Zoomer: crawling with its top down, its spines turned 36 degrees.</summary>
    OrangeZoomerUpsideDownSpinesTurned36,
    /// <summary>The horizontal Zoomer: crawling with its top down, its spines turned 48 degrees.</summary>
    OrangeZoomerUpsideDownSpinesTurned48,
    /// <summary>The horizontal Zoomer: crawling with its top up, its spines turned 0 degrees.</summary>
    OrangeZoomerUpsideUpSpinesTurned0,
    /// <summary>The horizontal Zoomer: crawling with its top up, its spines turned 12 degrees.</summary>
    OrangeZoomerUpsideUpSpinesTurned12,
    /// <summary>The horizontal Zoomer: crawling with its top up, its spines turned 24 degrees.</summary>
    OrangeZoomerUpsideUpSpinesTurned24,
    /// <summary>The horizontal Zoomer: crawling with its top up, its spines turned 36 degrees.</summary>
    OrangeZoomerUpsideUpSpinesTurned36,
    /// <summary>The horizontal Zoomer: crawling with its top up, its spines turned 48 degrees.</summary>
    OrangeZoomerUpsideUpSpinesTurned48,
    /// <summary>Sbug: facing right, wings up.</summary>
    SbugFacingRightWingsUp,
    /// <summary>Sbug: facing right, wings level.</summary>
    SbugFacingRightWingsLevel,
    /// <summary>Sbug: facing right, wings down.</summary>
    SbugFacingRightWingsDown,
    /// <summary>Sbug: facing up and right, wings up.</summary>
    SbugFacingUpRightWingsUp,
    /// <summary>Sbug: facing up and right, wings level.</summary>
    SbugFacingUpRightWingsLevel,
    /// <summary>Sbug: facing up and right, wings down.</summary>
    SbugFacingUpRightWingsDown,
    /// <summary>Sbug: facing up, wings up.</summary>
    SbugFacingUpWingsUp,
    /// <summary>Sbug: facing up, wings level.</summary>
    SbugFacingUpWingsLevel,
    /// <summary>Sbug: facing up, wings down.</summary>
    SbugFacingUpWingsDown,
    /// <summary>Sbug: facing up and left, wings up.</summary>
    SbugFacingUpLeftWingsUp,
    /// <summary>Sbug: facing up and left, wings level.</summary>
    SbugFacingUpLeftWingsLevel,
    /// <summary>Sbug: facing up and left, wings down.</summary>
    SbugFacingUpLeftWingsDown,
    /// <summary>Sbug: facing left, wings up.</summary>
    SbugFacingLeftWingsUp,
    /// <summary>Sbug: facing left, wings level.</summary>
    SbugFacingLeftWingsLevel,
    /// <summary>Sbug: facing left, wings down.</summary>
    SbugFacingLeftWingsDown,
    /// <summary>Sbug: facing down and left, wings up.</summary>
    SbugFacingDownLeftWingsUp,
    /// <summary>Sbug: facing down and left, wings level.</summary>
    SbugFacingDownLeftWingsLevel,
    /// <summary>Sbug: facing down and left, wings down.</summary>
    SbugFacingDownLeftWingsDown,
    /// <summary>Sbug: facing down, wings up.</summary>
    SbugFacingDownWingsUp,
    /// <summary>Sbug: facing down, wings level.</summary>
    SbugFacingDownWingsLevel,
    /// <summary>Sbug: facing down, wings down.</summary>
    SbugFacingDownWingsDown,
    /// <summary>Sbug: facing down and right, wings up.</summary>
    SbugFacingDownRightWingsUp,
    /// <summary>Sbug: facing down and right, wings level.</summary>
    SbugFacingDownRightWingsLevel,
    /// <summary>Sbug: facing down and right, wings down.</summary>
    SbugFacingDownRightWingsDown,
    /// <summary>Fune: facing left, mouth closed.</summary>
    FuneFacingLeftMouthClosed,
    /// <summary>Fune: facing left, mouth opening.</summary>
    FuneFacingLeftMouthOpening,
    /// <summary>Fune: facing left, mouth half open.</summary>
    FuneFacingLeftMouthHalfOpen,
    /// <summary>Fune: facing left, mouth open.</summary>
    FuneFacingLeftMouthOpen,
    /// <summary>Fune: facing left, mouth wide open.</summary>
    FuneFacingLeftMouthWide,
    /// <summary>Fune: facing right, mouth closed.</summary>
    FuneFacingRightMouthClosed,
    /// <summary>Fune: facing right, mouth opening.</summary>
    FuneFacingRightMouthOpening,
    /// <summary>Fune: facing right, mouth half open.</summary>
    FuneFacingRightMouthHalfOpen,
    /// <summary>Fune: facing right, mouth open.</summary>
    FuneFacingRightMouthOpen,
    /// <summary>Fune: facing right, mouth wide open.</summary>
    FuneFacingRightMouthWide,
    /// <summary>Namihe: facing left, mouth closed.</summary>
    NamiheFacingLeftMouthClosed,
    /// <summary>Namihe: facing left, mouth opening.</summary>
    NamiheFacingLeftMouthOpening,
    /// <summary>Namihe: facing left, mouth half open.</summary>
    NamiheFacingLeftMouthHalfOpen,
    /// <summary>Namihe: facing left, mouth open.</summary>
    NamiheFacingLeftMouthOpen,
    /// <summary>Namihe: facing left, mouth wide open.</summary>
    NamiheFacingLeftMouthWide,
    /// <summary>Namihe: facing left, mouth stretched widest.</summary>
    NamiheFacingLeftMouthWidest,
    /// <summary>Namihe: facing right, mouth closed.</summary>
    NamiheFacingRightMouthClosed,
    /// <summary>Namihe: facing right, mouth opening.</summary>
    NamiheFacingRightMouthOpening,
    /// <summary>Namihe: facing right, mouth half open.</summary>
    NamiheFacingRightMouthHalfOpen,
    /// <summary>Namihe: facing right, mouth open.</summary>
    NamiheFacingRightMouthOpen,
    /// <summary>Namihe: facing right, mouth wide open.</summary>
    NamiheFacingRightMouthWide,
    /// <summary>Namihe: facing right, mouth stretched widest.</summary>
    NamiheFacingRightMouthWidest,
    /// <summary>Kamer: its light dark.</summary>
    KamerLightDark,
    /// <summary>Kamer: its light dim.</summary>
    KamerLightDim,
    /// <summary>Kamer: its light lit.</summary>
    KamerLightLit,
    /// <summary>Kamer: its light bright.</summary>
    KamerLightBright,
    /// <summary>The elevator: its pad unlit.</summary>
    ElevatorPadUnlit,
    /// <summary>The elevator: its pad lit.</summary>
    ElevatorPadLit,
    /// <summary>A room effect actor: an Evir's body facing left, all white, legs stretched down.</summary>
    EffectActorEvirFacingLeftLegsStretched,
    /// <summary>A room effect actor: an Evir's body facing left, all white, legs lowered.</summary>
    EffectActorEvirFacingLeftLegsLowered,
    /// <summary>A room effect actor: an Evir's body facing left, all white, legs tucked.</summary>
    EffectActorEvirFacingLeftLegsTucked,
    /// <summary>A room effect actor: an Evir's body facing left, all white, legs lowering again.</summary>
    EffectActorEvirFacingLeftLegsLowering,
    /// <summary>A room effect actor: a cluster of Draygon's breath bubbles 6 pixels wide and 7 high, risen 0 pixels.</summary>
    EffectActorDraygonBreathBubbles6By7Risen0,
    /// <summary>A room effect actor: a cluster of Draygon's breath bubbles 12 pixels wide and 12 high, risen 6 pixels.</summary>
    EffectActorDraygonBreathBubbles12By12Risen6,
    /// <summary>A room effect actor: a cluster of Draygon's breath bubbles 14 pixels wide and 13 high, risen 8 pixels.</summary>
    EffectActorDraygonBreathBubbles14By13Risen8,
    /// <summary>A room effect actor: a cluster of Draygon's breath bubbles 12 pixels wide and 14 high, risen 11 pixels.</summary>
    EffectActorDraygonBreathBubbles12By14Risen11,
    /// <summary>A room effect actor: a cluster of Draygon's breath bubbles 12 pixels wide and 14 high, risen 14 pixels.</summary>
    EffectActorDraygonBreathBubbles12By14Risen14,
    /// <summary>A room effect actor: a cluster of Draygon's breath bubbles 10 pixels wide and 13 high, risen 15 pixels.</summary>
    EffectActorDraygonBreathBubbles10By13Risen15,
    /// <summary>A room effect actor: a cluster of Draygon's breath bubbles 10 pixels wide and 12 high, risen 16 pixels.</summary>
    EffectActorDraygonBreathBubbles10By12Risen16,
    /// <summary>A room effect actor: a cluster of Draygon's breath bubbles 8 pixels wide and 10 high, risen 16 pixels.</summary>
    EffectActorDraygonBreathBubbles8By10Risen16,
    /// <summary>A room effect actor: a cluster of Draygon's breath bubbles 4 pixels wide and 6 high, risen 14 pixels.</summary>
    EffectActorDraygonBreathBubbles4By6Risen14,
    /// <summary>A room effect actor: empty.</summary>
    EffectActorEmpty,
    /// <summary>A room effect actor: a puff of dust.</summary>
    EffectActorBigDustCloudPuff,
    /// <summary>A room effect actor: a dust cloud swelling.</summary>
    EffectActorBigDustCloudSwelling,
    /// <summary>A room effect actor: a dust cloud swollen.</summary>
    EffectActorBigDustCloudSwollen,
    /// <summary>A room effect actor: a dust cloud billowing.</summary>
    EffectActorBigDustCloudBillowing,
    /// <summary>A room effect actor: a dust cloud at its fullest.</summary>
    EffectActorBigDustCloudFull,
    /// <summary>A room effect actor: a dust cloud thinning.</summary>
    EffectActorBigDustCloudThinning,
    /// <summary>A room effect actor: a dust cloud fading.</summary>
    EffectActorBigDustCloudFading,
    /// <summary>A room effect actor: the last wisps of a dust cloud.</summary>
    EffectActorBigDustCloudWisps,
    /// <summary>A room effect actor: a spark.</summary>
    EffectActorSmallExplosionSpark,
    /// <summary>A room effect actor: a small fireball.</summary>
    EffectActorSmallExplosionFireball,
    /// <summary>A room effect actor: a white flash.</summary>
    EffectActorSmallExplosionFlash,
    /// <summary>A room effect actor: a burst of flame round a ring.</summary>
    EffectActorSmallExplosionBurst,
    /// <summary>A room effect actor: a ring of fire.</summary>
    EffectActorSmallExplosionRing,
    /// <summary>A room effect actor: scattering embers.</summary>
    EffectActorSmallExplosionScattering,
    /// <summary>A room effect actor: a dot of light.</summary>
    EffectActorDudShotDot,
    /// <summary>A room effect actor: a spark.</summary>
    EffectActorDudShotSpark,
    /// <summary>A room effect actor: a white flash.</summary>
    EffectActorDudShotFlash,
    /// <summary>A room effect actor: a ring round a glowing core.</summary>
    EffectActorDudShotRing,
    /// <summary>A room effect actor: a thin ring.</summary>
    EffectActorDudShotThinRing,
    /// <summary>A room effect actor: a fading ring.</summary>
    EffectActorDudShotFadingRing,
    /// <summary>A room effect actor: a smoke cloud.</summary>
    EffectActorSmallDudShotSmoke,
    /// <summary>A room effect actor: a smoke cloud rising.</summary>
    EffectActorSmallDudShotSmokeRising,
    /// <summary>A room effect actor: a smoke cloud risen.</summary>
    EffectActorSmallDudShotSmokeRisen,
    /// <summary>A room effect actor: a smoke cloud fading.</summary>
    EffectActorSmallDudShotSmokeFading,
    /// <summary>A room effect actor: a puff of smoke.</summary>
    EffectActorSmallDudShotPuff,
    /// <summary>A room effect actor: a puff of dust.</summary>
    EffectActorSpacePirateLandingDustPuff,
    /// <summary>A room effect actor: a puff of dust rising.</summary>
    EffectActorSpacePirateLandingDustPuffRising,
    /// <summary>A room effect actor: a puff of dust risen.</summary>
    EffectActorSpacePirateLandingDustPuffRisen,
    /// <summary>A room effect actor: a smoke puff, low.</summary>
    EffectActorSmokeLow,
    /// <summary>A room effect actor: a smoke puff rising.</summary>
    EffectActorSmokeRising,
    /// <summary>A room effect actor: a smoke puff risen.</summary>
    EffectActorSmokeRisen,
    /// <summary>A room effect actor: a smoke puff fading away.</summary>
    EffectActorSmokeFading,
    /// <summary>A room effect actor: a fireball.</summary>
    EffectActorBigExplosionFireball,
    /// <summary>A room effect actor: a white flash.</summary>
    EffectActorBigExplosionFlash,
    /// <summary>A room effect actor: a burst of flame round a ring.</summary>
    EffectActorBigExplosionBurst,
    /// <summary>A room effect actor: a thick white ring.</summary>
    EffectActorBigExplosionRing,
    /// <summary>A room effect actor: a wide white ring.</summary>
    EffectActorBigExplosionWideRing,
    /// <summary>A room effect actor: a fading ring.</summary>
    EffectActorBigExplosionFadingRing,
    /// <summary>A room effect actor: a nuclear waffle's swirling body, upright.</summary>
    EffectActorNuclearWaffleBodyTurned0,
    /// <summary>A room effect actor: a nuclear waffle's swirling body, turned an eighth.</summary>
    EffectActorNuclearWaffleBodyTurned45,
    /// <summary>A room effect actor: a nuclear waffle's swirling body, turned three eighths.</summary>
    EffectActorNuclearWaffleBodyTurned135,
    /// <summary>A room effect actor: a nuclear waffle's swirling body, turned half way.</summary>
    EffectActorNuclearWaffleBodyTurned180,
    /// <summary>A room effect actor: a nuclear waffle's swirling body, turned a quarter.</summary>
    EffectActorNuclearWaffleBodyTurned90,
    /// <summary>A room effect actor: a nuclear waffle's swirling body, turned five eighths.</summary>
    EffectActorNuclearWaffleBodyTurned225,
    /// <summary>A room effect actor: a nuclear waffle's swirling body, turned three quarters.</summary>
    EffectActorNuclearWaffleBodyTurned270,
    /// <summary>A room effect actor: a nuclear waffle's swirling body, turned seven eighths.</summary>
    EffectActorNuclearWaffleBodyTurned315,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the right: an ember.</summary>
    EffectActorNuclearWaffleRightExplosionEmber,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the right: a small fireball.</summary>
    EffectActorNuclearWaffleRightExplosionBall,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the right: a fireball swelling.</summary>
    EffectActorNuclearWaffleRightExplosionSwelling,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the right: a fireball glowing.</summary>
    EffectActorNuclearWaffleRightExplosionGlowing,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the right: a fireball bursting.</summary>
    EffectActorNuclearWaffleRightExplosionBursting,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the right: embers scattering.</summary>
    EffectActorNuclearWaffleRightExplosionScattering,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the right: the last scattered embers.</summary>
    EffectActorNuclearWaffleRightExplosionScattered,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the left: an ember.</summary>
    EffectActorNuclearWaffleLeftExplosionEmber,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the left: a small fireball.</summary>
    EffectActorNuclearWaffleLeftExplosionBall,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the left: a fireball swelling.</summary>
    EffectActorNuclearWaffleLeftExplosionSwelling,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the left: a fireball glowing.</summary>
    EffectActorNuclearWaffleLeftExplosionGlowing,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the left: a fireball bursting.</summary>
    EffectActorNuclearWaffleLeftExplosionBursting,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the left: embers scattering.</summary>
    EffectActorNuclearWaffleLeftExplosionScattering,
    /// <summary>A room effect actor: a nuclear waffle's explosion off to the left: the last scattered embers.</summary>
    EffectActorNuclearWaffleLeftExplosionScattered,
    /// <summary>A room effect actor: a nuclear waffle's lava splash, 25 pixels wide.</summary>
    EffectActorNuclearWaffleSplash25Wide,
    /// <summary>A room effect actor: a nuclear waffle's lava splash, 29 pixels wide.</summary>
    EffectActorNuclearWaffleSplash29Wide,
    /// <summary>A room effect actor: a nuclear waffle's lava splash, 39 pixels wide.</summary>
    EffectActorNuclearWaffleSplash39Wide,
    /// <summary>A room effect actor: a nuclear waffle's lava splash, 45 pixels wide.</summary>
    EffectActorNuclearWaffleSplash45Wide,
    /// <summary>A room effect actor: a falling spark's trailing sparkle, bright.</summary>
    EffectActorFallingSparkTrailBright,
    /// <summary>A room effect actor: a falling spark's trailing sparkle, twinkling.</summary>
    EffectActorFallingSparkTrailTwinkling,
    /// <summary>A room effect actor: a falling spark's trailing sparkle, dimming.</summary>
    EffectActorFallingSparkTrailDimming,
    /// <summary>A room effect actor: a falling spark's trailing sparkle, fading.</summary>
    EffectActorFallingSparkTrailFading,
    /// <summary>A room effect actor: a frozen Metroid's shell, 38 pixels wide and 27 high.</summary>
    EffectActorMetroidShell38By27,
    /// <summary>A room effect actor: a frozen Metroid's shell, 44 pixels wide and 29 high.</summary>
    EffectActorMetroidShell44By29,
    /// <summary>A room effect actor: a frozen Metroid's shell, 38 pixels wide and 28 high.</summary>
    EffectActorMetroidShell38By28,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 23 pixels wide and 21 high, anchored 16 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc23By21Anchored16By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 22 pixels wide and 22 high, anchored 16 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc22By22Anchored16By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 24 pixels wide and 23 high, anchored 16 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc24By23Anchored16By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 24 pixels wide and 18 high, anchored 8 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc24By18Anchored8By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 24 pixels wide and 21 high, anchored 8 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc24By21Anchored8By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 23 pixels wide and 22 high, anchored 8 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc23By22Anchored8By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 23 pixels wide and 21 high, anchored 8 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc23By21Anchored8By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 22 pixels wide and 22 high, anchored 8 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc22By22Anchored8By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 24 pixels wide and 23 high, anchored 8 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc24By23Anchored8By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 19 pixels wide and 18 high, anchored 16 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc19By18Anchored16By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 19 pixels wide and 22 high, anchored 16 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc19By22Anchored16By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 20 pixels wide and 21 high, anchored 16 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc20By21Anchored16By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 16 pixels wide and 7 high, anchored 16 pixels in and 4 down.</summary>
    EffectActorMetroidElectricityArc16By7Anchored16By4,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 8 pixels wide and 8 high, anchored 16 pixels in and 4 down.</summary>
    EffectActorMetroidElectricityArc8By8Anchored16By4,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 16 pixels wide and 6 high, anchored 16 pixels in and 4 down.</summary>
    EffectActorMetroidElectricityArc16By6Anchored16By4,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 7 pixels wide and 12 high, anchored 8 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc7By12Anchored8By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 8 pixels wide and 14 high, anchored 8 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc8By14Anchored8By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 7 pixels wide and 14 high, anchored 8 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc7By14Anchored8By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 16 pixels wide and 7 high, anchored 0 pixels in and 4 down.</summary>
    EffectActorMetroidElectricityArc16By7Anchored0By4,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 16 pixels wide and 8 high, anchored 0 pixels in and 4 down.</summary>
    EffectActorMetroidElectricityArc16By8Anchored0By4,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 16 pixels wide and 6 high, anchored 0 pixels in and 4 down.</summary>
    EffectActorMetroidElectricityArc16By6Anchored0By4,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 4 pixels wide and 6 high, anchored 0 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc4By6Anchored0By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 3 pixels wide and 6 high, anchored 0 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc3By6Anchored0By20,
    /// <summary>A room effect actor: an arc of a Metroid's electricity 4 pixels wide and 7 high, anchored 0 pixels in and 20 down.</summary>
    EffectActorMetroidElectricityArc4By7Anchored0By20,
    /// <summary>A room effect actor: yapping maw base facing down.</summary>
    EffectActorYappingMawBaseFacingDown,
    /// <summary>A room effect actor: yapping maw base facing up.</summary>
    EffectActorYappingMawBaseFacingUp,
    /// <summary>A room effect actor: an Evir's body facing right, all white, legs stretched down.</summary>
    EffectActorEvirFacingRightLegsStretched,
    /// <summary>A room effect actor: an Evir's body facing right, all white, legs lowered.</summary>
    EffectActorEvirFacingRightLegsLowered,
    /// <summary>A room effect actor: an Evir's body facing right, all white, legs tucked.</summary>
    EffectActorEvirFacingRightLegsTucked,
    /// <summary>A room effect actor: an Evir's body facing right, all white, legs lowering again.</summary>
    EffectActorEvirFacingRightLegsLowering,
    /// <summary>A room effect actor: foam from Draygon's mouth 9 pixels wide and 8 high, risen 0 pixels.</summary>
    EffectActorDraygonFoam9By8Risen0,
    /// <summary>A room effect actor: foam from Draygon's mouth 13 pixels wide and 14 high, risen 4 pixels.</summary>
    EffectActorDraygonFoam13By14Risen4,
    /// <summary>A room effect actor: foam from Draygon's mouth 15 pixels wide and 14 high, risen 8 pixels.</summary>
    EffectActorDraygonFoam15By14Risen8,
    /// <summary>A room effect actor: foam from Draygon's mouth 15 pixels wide and 16 high, risen 12 pixels.</summary>
    EffectActorDraygonFoam15By16Risen12,
    /// <summary>A room effect actor: foam from Draygon's mouth 15 pixels wide and 15 high, risen 16 pixels.</summary>
    EffectActorDraygonFoam15By15Risen16,
    /// <summary>A room effect actor: foam from Draygon's mouth 15 pixels wide and 12 high, risen 20 pixels.</summary>
    EffectActorDraygonFoam15By12Risen20,
    /// <summary>A room effect actor: foam from Draygon's mouth 14 pixels wide and 9 high, risen 24 pixels.</summary>
    EffectActorDraygonFoam14By9Risen24,
    /// <summary>A room effect actor: foam from Draygon's mouth 13 pixels wide and 9 high, risen 28 pixels.</summary>
    EffectActorDraygonFoam13By9Risen28,
    /// <summary>Yapping Maw: reaching up, jaws wide open.</summary>
    YappingMawFacingUpJawsWide,
    /// <summary>Yapping Maw: reaching up, jaws closing.</summary>
    YappingMawFacingUpJawsClosing,
    /// <summary>Yapping Maw: reaching up, jaws shut.</summary>
    YappingMawFacingUpJawsShut,
    /// <summary>Yapping Maw: reaching up and right, jaws wide open.</summary>
    YappingMawFacingUpRightJawsWide,
    /// <summary>Yapping Maw: reaching up and right, jaws closing.</summary>
    YappingMawFacingUpRightJawsClosing,
    /// <summary>Yapping Maw: reaching up and right, jaws shut.</summary>
    YappingMawFacingUpRightJawsShut,
    /// <summary>Yapping Maw: reaching right, jaws wide open.</summary>
    YappingMawFacingRightJawsWide,
    /// <summary>Yapping Maw: reaching right, jaws closing.</summary>
    YappingMawFacingRightJawsClosing,
    /// <summary>Yapping Maw: reaching right, jaws shut.</summary>
    YappingMawFacingRightJawsShut,
    /// <summary>Yapping Maw: reaching down and right, jaws wide open.</summary>
    YappingMawFacingDownRightJawsWide,
    /// <summary>Yapping Maw: reaching down and right, jaws closing.</summary>
    YappingMawFacingDownRightJawsClosing,
    /// <summary>Yapping Maw: reaching down and right, jaws shut.</summary>
    YappingMawFacingDownRightJawsShut,
    /// <summary>Yapping Maw: reaching down, jaws wide open.</summary>
    YappingMawFacingDownJawsWide,
    /// <summary>Yapping Maw: reaching down, jaws closing.</summary>
    YappingMawFacingDownJawsClosing,
    /// <summary>Yapping Maw: reaching down, jaws shut.</summary>
    YappingMawFacingDownJawsShut,
    /// <summary>Yapping Maw: reaching down and left, jaws wide open.</summary>
    YappingMawFacingDownLeftJawsWide,
    /// <summary>Yapping Maw: reaching down and left, jaws closing.</summary>
    YappingMawFacingDownLeftJawsClosing,
    /// <summary>Yapping Maw: reaching down and left, jaws shut.</summary>
    YappingMawFacingDownLeftJawsShut,
    /// <summary>Yapping Maw: reaching left, jaws wide open.</summary>
    YappingMawFacingLeftJawsWide,
    /// <summary>Yapping Maw: reaching left, jaws closing.</summary>
    YappingMawFacingLeftJawsClosing,
    /// <summary>Yapping Maw: reaching left, jaws shut.</summary>
    YappingMawFacingLeftJawsShut,
    /// <summary>Yapping Maw: reaching up and left, jaws wide open.</summary>
    YappingMawFacingUpLeftJawsWide,
    /// <summary>Yapping Maw: reaching up and left, jaws closing.</summary>
    YappingMawFacingUpLeftJawsClosing,
    /// <summary>Yapping Maw: reaching up and left, jaws shut.</summary>
    YappingMawFacingUpLeftJawsShut,
    /// <summary>Ki-Hunter: hovering facing left.</summary>
    KiHunterHoveringFacingLeft,
    /// <summary>Ki-Hunter: hovering facing left, a pixel lower.</summary>
    KiHunterHoveringFacingLeftDropped,
    /// <summary>Ki-Hunter: hovering facing left: the picture of KiHunterHoveringFacingLeft, as a frame of its own.</summary>
    KiHunterHoveringFacingLeftTwin,
    /// <summary>Ki-Hunter: its wings facing left, raised.</summary>
    KiHunterWingsFacingLeftRaised,
    /// <summary>Ki-Hunter: its wings facing left, half lowered.</summary>
    KiHunterWingsFacingLeftMid,
    /// <summary>Ki-Hunter: its wings facing left, level.</summary>
    KiHunterWingsFacingLeftLevel,
    /// <summary>Ki-Hunter: hovering facing right.</summary>
    KiHunterHoveringFacingRight,
    /// <summary>Ki-Hunter: hovering facing right, a pixel lower.</summary>
    KiHunterHoveringFacingRightDropped,
    /// <summary>Ki-Hunter: hovering facing right: the picture of KiHunterHoveringFacingRight, as a frame of its own.</summary>
    KiHunterHoveringFacingRightTwin,
    /// <summary>Ki-Hunter: its wings facing right, raised.</summary>
    KiHunterWingsFacingRightRaised,
    /// <summary>Ki-Hunter: its wings facing right, half lowered.</summary>
    KiHunterWingsFacingRightMid,
    /// <summary>Ki-Hunter: its wings facing right, level.</summary>
    KiHunterWingsFacingRightLevel,
    /// <summary>Ki-Hunter: detached wings.</summary>
    KiHunterDetachedWings,
    /// <summary>Ki-Hunter: hovering facing left: the picture of KiHunterHoveringFacingLeft, as a frame of its own.</summary>
    KiHunterHoveringFacingLeftTwinB,
    /// <summary>Ki-Hunter: swooping facing left, arms swept back.</summary>
    KiHunterSwoopingFacingLeftArmsBack,
    /// <summary>Ki-Hunter: swooping facing left, arms drawn in.</summary>
    KiHunterSwoopingFacingLeftArmsIn,
    /// <summary>Ki-Hunter: swooping facing left, legs reaching out.</summary>
    KiHunterSwoopingFacingLeftLegsReaching,
    /// <summary>Ki-Hunter: swooping facing left, legs stretched in a lunge.</summary>
    KiHunterSwoopingFacingLeftLegsStretched,
    /// <summary>Ki-Hunter: hovering facing right: the picture of KiHunterHoveringFacingRight, as a frame of its own.</summary>
    KiHunterHoveringFacingRightTwinB,
    /// <summary>Ki-Hunter: swooping facing right, arms swept back.</summary>
    KiHunterSwoopingFacingRightArmsBack,
    /// <summary>Ki-Hunter: swooping facing right, arms drawn in.</summary>
    KiHunterSwoopingFacingRightArmsIn,
    /// <summary>Ki-Hunter: swooping facing right, legs reaching out.</summary>
    KiHunterSwoopingFacingRightLegsReaching,
    /// <summary>Ki-Hunter: swooping facing right, legs stretched in a lunge.</summary>
    KiHunterSwoopingFacingRightLegsStretched,
    /// <summary>Ki-Hunter: crouched facing left.</summary>
    KiHunterCrouchedFacingLeft,
    /// <summary>Ki-Hunter: crouching low, facing left.</summary>
    KiHunterCrouchingLowFacingLeft,
    /// <summary>Ki-Hunter: springing up, facing left.</summary>
    KiHunterSpringingFacingLeft,
    /// <summary>Ki-Hunter: leaping, legs hanging, facing left.</summary>
    KiHunterLeapingFacingLeft,
    /// <summary>Ki-Hunter: crouched facing right.</summary>
    KiHunterCrouchedFacingRight,
    /// <summary>Ki-Hunter: crouching low, facing right.</summary>
    KiHunterCrouchingLowFacingRight,
    /// <summary>Ki-Hunter: springing up, facing right.</summary>
    KiHunterSpringingFacingRight,
    /// <summary>Ki-Hunter: leaping, legs hanging, facing right.</summary>
    KiHunterLeapingFacingRight,
    /// <summary>Ki-Hunter: crouched facing left: the picture of KiHunterCrouchedFacingLeft, as a frame of its own.</summary>
    KiHunterCrouchedFacingLeftTwin,
    /// <summary>Ki-Hunter: spitting facing left, a ball of acid gathering in its mouth.</summary>
    KiHunterSpittingFacingLeftAcidGathering,
    /// <summary>Ki-Hunter: spitting facing left, a swollen ball of acid in its mouth.</summary>
    KiHunterSpittingFacingLeftAcidSwollen,
    /// <summary>Ki-Hunter: spitting facing left, head thrust forward, spitting acid.</summary>
    KiHunterSpittingFacingLeftAcidSpat,
    /// <summary>Ki-Hunter: spitting facing left, a last fleck of acid in its mouth.</summary>
    KiHunterSpittingFacingLeftAcidSpent,
    /// <summary>Ki-Hunter: crouched facing right: the picture of KiHunterCrouchedFacingRight, as a frame of its own.</summary>
    KiHunterCrouchedFacingRightTwin,
    /// <summary>Ki-Hunter: spitting facing right, a ball of acid gathering in its mouth.</summary>
    KiHunterSpittingFacingRightAcidGathering,
    /// <summary>Ki-Hunter: spitting facing right, a swollen ball of acid in its mouth.</summary>
    KiHunterSpittingFacingRightAcidSwollen,
    /// <summary>Ki-Hunter: spitting facing right, head thrust forward, spitting acid.</summary>
    KiHunterSpittingFacingRightAcidSpat,
    /// <summary>Ki-Hunter: spitting facing right, a last fleck of acid in its mouth.</summary>
    KiHunterSpittingFacingRightAcidSpent,
    /// <summary>Mother Brain: its head, the brain whole, its eye shut.</summary>
    MotherBrainHeadEyeShut,
    /// <summary>Mother Brain: its head, the brain whole, its eye open.</summary>
    MotherBrainHeadEyeOpen,
    /// <summary>Mother Brain: its head, the brain whole, its eye glaring red.</summary>
    MotherBrainHeadEyeGlaring,
    /// <summary>Mother Brain: its head, the brain whole, its jaw dropping.</summary>
    MotherBrainHeadJawDropping,
    /// <summary>Mother Brain: its head, the brain whole, its jaw dropped wide.</summary>
    MotherBrainHeadJawDropped,
    /// <summary>Mother Brain: neck joint.</summary>
    MotherBrainNeckJoint,
    /// <summary>Mother Brain: its head, the brain darkened, its eye shut.</summary>
    MotherBrainDarkHeadEyeShut,
    /// <summary>Mother Brain: its head, the brain darkened, its eye open.</summary>
    MotherBrainDarkHeadEyeOpen,
    /// <summary>Mother Brain: its head, the brain darkened, its eye glaring red.</summary>
    MotherBrainDarkHeadEyeGlaring,
    /// <summary>Mother Brain: its head, the brain darkened, its jaw dropping.</summary>
    MotherBrainDarkHeadJawDropping,
    /// <summary>Mother Brain: its head, the brain darkened, its jaw dropped wide.</summary>
    MotherBrainDarkHeadJawDropped,
    /// <summary>Mother Brain: its lifeless head slumping.</summary>
    MotherBrainHeadCorpseSlumping,
    /// <summary>Mother Brain: its lifeless head slumped over.</summary>
    MotherBrainHeadCorpseSlumped,
    /// <summary>Mother Brain: a length of its tube bending right.</summary>
    MotherBrainTubeBendRight,
    /// <summary>Mother Brain: a length of its tube bending left.</summary>
    MotherBrainTubeBendLeft,
    /// <summary>Mother Brain: a straight length of its tube, its joint to the left.</summary>
    MotherBrainTubeStraightLeft,
    /// <summary>Mother Brain: a straight length of its tube, its joint to the right.</summary>
    MotherBrainTubeStraightRight,
    /// <summary>Mother Brain: the machinery column its tubes feed.</summary>
    MotherBrainTubeColumn,
    /// <summary>The dead Torizo: its remains crumbled to a low mound.</summary>
    DeadTorizoRubbleMound,
    /// <summary>Ridley: the tail link nearest his body, which the two links at its base show.</summary>
    RidleyTailBaseLink,
    /// <summary>Ridley: the middle tail link, which the third and fourth links show.</summary>
    RidleyTailMiddleLink,
    /// <summary>Ridley: the outer tail link, which the fifth and sixth links, nearest the tip, show.</summary>
    RidleyTailOuterLink,
    /// <summary>Ridley: the tip of its tail, pointing toward 270 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward270,
    /// <summary>Ridley: the tip of its tail, pointing toward 292 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward292,
    /// <summary>Ridley: the tip of its tail, pointing toward 315 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward315,
    /// <summary>Ridley: the tip of its tail, pointing toward 337 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward337,
    /// <summary>Ridley: the tip of its tail, pointing toward 0 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward0,
    /// <summary>Ridley: the tip of its tail, pointing toward 22 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward22,
    /// <summary>Ridley: the tip of its tail, pointing toward 45 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward45,
    /// <summary>Ridley: the tip of its tail, pointing toward 67 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward67,
    /// <summary>Ridley: the tip of its tail, pointing toward 90 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward90,
    /// <summary>Ridley: the tip of its tail, pointing toward 112 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward112,
    /// <summary>Ridley: the tip of its tail, pointing toward 135 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward135,
    /// <summary>Ridley: the tip of its tail, pointing toward 157 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward157,
    /// <summary>Ridley: the tip of its tail, pointing toward 180 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward180,
    /// <summary>Ridley: the tip of its tail, pointing toward 202 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward202,
    /// <summary>Ridley: the tip of its tail, pointing toward 225 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward225,
    /// <summary>Ridley: the tip of its tail, pointing toward 247 degrees clockwise from straight up.</summary>
    RidleyTailTipPointingToward247,
    /// <summary>Ridley: its wing facing left, raised.</summary>
    RidleyWingFacingLeftRaised,
    /// <summary>Ridley: its wing facing left, lowering.</summary>
    RidleyWingFacingLeftLowering,
    /// <summary>Ridley: its wing facing left, edge on.</summary>
    RidleyWingFacingLeftEdgeOn,
    /// <summary>Ridley: its wing facing left, edge on, tilted.</summary>
    RidleyWingFacingLeftEdgeOnTilted,
    /// <summary>Ridley: its wing facing left, dipping.</summary>
    RidleyWingFacingLeftDipping,
    /// <summary>Ridley: its wing facing left, down.</summary>
    RidleyWingFacingLeftDown,
    /// <summary>Ridley: its wing facing right, raised.</summary>
    RidleyWingFacingRightRaised,
    /// <summary>Ridley: its wing facing right, lowering.</summary>
    RidleyWingFacingRightLowering,
    /// <summary>Ridley: its wing facing right, edge on.</summary>
    RidleyWingFacingRightEdgeOn,
    /// <summary>Ridley: its wing facing right, edge on, tilted.</summary>
    RidleyWingFacingRightEdgeOnTilted,
    /// <summary>Ridley: its wing facing right, dipping.</summary>
    RidleyWingFacingRightDipping,
    /// <summary>Ridley: its wing facing right, down.</summary>
    RidleyWingFacingRightDown,
    /// <summary>Sciser: scuttling with its top right, legs tucked.</summary>
    SciserUpsideRightLegsTucked,
    /// <summary>Sciser: scuttling with its top right, legs part way out.</summary>
    SciserUpsideRightLegsMid,
    /// <summary>Sciser: scuttling with its top right, legs out.</summary>
    SciserUpsideRightLegsOut,
    /// <summary>Sciser: scuttling with its top left, legs tucked.</summary>
    SciserUpsideLeftLegsTucked,
    /// <summary>Sciser: scuttling with its top left, legs part way out.</summary>
    SciserUpsideLeftLegsMid,
    /// <summary>Sciser: scuttling with its top left, legs out.</summary>
    SciserUpsideLeftLegsOut,
    /// <summary>Sciser: scuttling with its top down, legs tucked.</summary>
    SciserUpsideDownLegsTucked,
    /// <summary>Sciser: scuttling with its top down, legs part way out.</summary>
    SciserUpsideDownLegsMid,
    /// <summary>Sciser: scuttling with its top down, legs out.</summary>
    SciserUpsideDownLegsOut,
    /// <summary>Sciser: scuttling with its top up, legs tucked.</summary>
    SciserUpsideUpLegsTucked,
    /// <summary>Sciser: scuttling with its top up, legs part way out.</summary>
    SciserUpsideUpLegsMid,
    /// <summary>Sciser: scuttling with its top up, legs out.</summary>
    SciserUpsideUpLegsOut,
    /// <summary>The fly: wings raised and out.</summary>
    FlyWingsRaisedOut,
    /// <summary>The fly: wings raised.</summary>
    FlyWingsRaised,
    /// <summary>The fly: wings level.</summary>
    FlyWingsLevel,
    /// <summary>The fly: wings lowered.</summary>
    FlyWingsLowered,
    /// <summary>Kago: its hive at rest.</summary>
    KagoHiveAtRest,
    /// <summary>Kago: its hive swelling.</summary>
    KagoHiveSwelling,
    /// <summary>Kago: its hive swollen.</summary>
    KagoHiveSwollen,
    /// <summary>The face block: neutral.</summary>
    FaceBlockNeutral,
    /// <summary>The blue Brinstar face block: glancing left.</summary>
    BlueBrinstarFaceBlockGlancingLeft,
    /// <summary>The blue Brinstar face block: looking left.</summary>
    BlueBrinstarFaceBlockLookingLeft,
    /// <summary>The blue Brinstar face block: glancing right.</summary>
    BlueBrinstarFaceBlockGlancingRight,
    /// <summary>The blue Brinstar face block: looking right.</summary>
    BlueBrinstarFaceBlockLookingRight,
    /// <summary>The Morph Ball eye open, looking up and a little right.</summary>
    MorphBallEyeLookingUpUpRight,
    /// <summary>The Morph Ball eye open, looking up and to the right.</summary>
    MorphBallEyeLookingUpRight,
    /// <summary>The Morph Ball eye open, looking right and a little up.</summary>
    MorphBallEyeLookingRightUpRight,
    /// <summary>The Morph Ball eye open, looking straight right.</summary>
    MorphBallEyeLookingRight,
    /// <summary>The Morph Ball eye open, looking right and a little down.</summary>
    MorphBallEyeLookingRightDownRight,
    /// <summary>The Morph Ball eye open, looking down and to the right.</summary>
    MorphBallEyeLookingDownRight,
    /// <summary>The Morph Ball eye open, looking down and a little right.</summary>
    MorphBallEyeLookingDownDownRight,
    /// <summary>The Morph Ball eye open, looking down and a little left.</summary>
    MorphBallEyeLookingDownDownLeft,
    /// <summary>The Morph Ball eye open, looking down and to the left.</summary>
    MorphBallEyeLookingDownLeft,
    /// <summary>The Morph Ball eye open, looking left and a little down.</summary>
    MorphBallEyeLookingLeftDownLeft,
    /// <summary>The Morph Ball eye open, looking straight left.</summary>
    MorphBallEyeLookingLeft,
    /// <summary>The Morph Ball eye open, looking left and a little up.</summary>
    MorphBallEyeLookingLeftUpLeft,
    /// <summary>The Morph Ball eye open, looking up and to the left.</summary>
    MorphBallEyeLookingUpLeft,
    /// <summary>The Morph Ball eye open, looking up and a little left.</summary>
    MorphBallEyeLookingUpUpLeft,
    /// <summary>The Morph Ball eye: turned right, half open.</summary>
    MorphBallEyeFacingRightHalfOpen,
    /// <summary>The Morph Ball eye: turned right, closed.</summary>
    MorphBallEyeFacingRightClosed,
    /// <summary>The Morph Ball eye: turned left, half open.</summary>
    MorphBallEyeFacingLeftHalfOpen,
    /// <summary>The Morph Ball eye: turned left, closed.</summary>
    MorphBallEyeFacingLeftClosed,
    /// <summary>The Morph Ball eye: mount right.</summary>
    MorphEyeMountRight,
    /// <summary>The Morph Ball eye: mount down.</summary>
    MorphEyeMountDown,
    /// <summary>The Morph Ball eye: mount left.</summary>
    MorphEyeMountLeft,
    /// <summary>The Morph Ball eye: mount up.</summary>
    MorphEyeMountUp,
    /// <summary>The shutter: ten pixels long.</summary>
    ShutterTenPixelsLong,
    /// <summary>The shutter: twenty pixels long.</summary>
    ShutterTwentyPixelsLong,
    /// <summary>The shutter: thirty pixels long.</summary>
    ShutterThirtyPixelsLong,
    /// <summary>The shutter: forty pixels long.</summary>
    ShutterFortyPixelsLong,
    /// <summary>The Metroid: its nuclei dark.</summary>
    MetroidNucleiDark,
    /// <summary>The Metroid: its nuclei glowing.</summary>
    MetroidNucleiGlowing,
    /// <summary>The Metroid: its nuclei bright.</summary>
    MetroidNucleiBright,
    /// <summary>The Metroid: its nuclei brightest.</summary>
    MetroidNucleiBrightest,
    /// <summary>Shaktool: the final piece of its saw hand, its blade upright.</summary>
    ShaktoolSawHandFinalPieceTurned0,
    /// <summary>Shaktool: the final piece of its saw hand, its blade turned a twelfth.</summary>
    ShaktoolSawHandFinalPieceTurned30,
    /// <summary>Shaktool: the final piece of its saw hand, its blade turned a sixth.</summary>
    ShaktoolSawHandFinalPieceTurned60,
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
    /// <summary>Shaktool: the primary piece of its saw hand, its blade upright.</summary>
    ShaktoolSawHandPrimaryPieceTurned0,
    /// <summary>Shaktool: the primary piece of its saw hand, its blade turned a twelfth.</summary>
    ShaktoolSawHandPrimaryPieceTurned30,
    /// <summary>Shaktool: the primary piece of its saw hand, its blade turned a sixth.</summary>
    ShaktoolSawHandPrimaryPieceTurned60,
    /// <summary>Tripper: hovering facing left, its downward jet short.</summary>
    TripperHoveringFacingLeftJetShort,
    /// <summary>Tripper: hovering facing left, its downward jet long.</summary>
    TripperHoveringFacingLeftJetLong,
    /// <summary>Tripper: hovering facing left, its downward jet long: the picture of TripperHoveringFacingLeftJetLong, as a frame of its own.</summary>
    TripperHoveringFacingLeftJetLongTwin,
    /// <summary>Tripper: hovering facing right, its downward jet short.</summary>
    TripperHoveringFacingRightJetShort,
    /// <summary>Tripper: hovering facing right, its downward jet long.</summary>
    TripperHoveringFacingRightJetLong,
    /// <summary>Tripper: hovering facing right, its downward jet long: the picture of TripperHoveringFacingRightJetLong, as a frame of its own.</summary>
    TripperHoveringFacingRightJetLongTwin,
    /// <summary>Tripper: thrusting left, its jet out its right side medium.</summary>
    TripperThrustingLeftJetMedium,
    /// <summary>Tripper: thrusting left, its jet out its right side short.</summary>
    TripperThrustingLeftJetShort,
    /// <summary>Tripper: thrusting left, its jet out its right side long.</summary>
    TripperThrustingLeftJetLong,
    /// <summary>Tripper: thrusting right, its jet out its left side medium.</summary>
    TripperThrustingRightJetMedium,
    /// <summary>Tripper: thrusting right, its jet out its left side short.</summary>
    TripperThrustingRightJetShort,
    /// <summary>Tripper: thrusting right, its jet out its left side long.</summary>
    TripperThrustingRightJetLong,
    /// <summary>Tripper: frozen facing left.</summary>
    TripperFrozenFacingLeft,
    /// <summary>Tripper: frozen facing right.</summary>
    TripperFrozenFacingRight,
    /// <summary>Its light dark.</summary>
    SuspensorPlatformLightDark,
    /// <summary>Its light dim.</summary>
    SuspensorPlatformLightDim,
    /// <summary>Its light lit.</summary>
    SuspensorPlatformLightLit,
    /// <summary>Its light bright.</summary>
    SuspensorPlatformLightBright,
    /// <summary>Dragon: its body facing left.</summary>
    DragonBodyFacingLeft,
    /// <summary>Dragon: its wing facing left, raised.</summary>
    DragonWingFacingLeftRaised,
    /// <summary>Dragon: its wing facing left, lowered.</summary>
    DragonWingFacingLeftLowered,
    /// <summary>Dragon: its body facing right.</summary>
    DragonBodyFacingRight,
    /// <summary>Dragon: its wing facing right, raised.</summary>
    DragonWingFacingRightRaised,
    /// <summary>Dragon: its wing facing right, lowered.</summary>
    DragonWingFacingRightLowered,
    /// <summary>Dragon: spitting fire facing left, head drawn back.</summary>
    DragonSpittingFacingLeftHeadBack,
    /// <summary>Dragon: spitting fire facing left, head raised.</summary>
    DragonSpittingFacingLeftHeadRaised,
    /// <summary>Dragon: spitting fire facing left, head thrust forward.</summary>
    DragonSpittingFacingLeftHeadForward,
    /// <summary>Dragon: spitting fire facing right, head drawn back.</summary>
    DragonSpittingFacingRightHeadBack,
    /// <summary>Dragon: spitting fire facing right, head raised.</summary>
    DragonSpittingFacingRightHeadRaised,
    /// <summary>Dragon: spitting fire facing right, head thrust forward.</summary>
    DragonSpittingFacingRightHeadForward,
    /// <summary>Multiviola: a plain fireball.</summary>
    MultiviolaPlain,
    /// <summary>Multiviola: a fireball, its face faint.</summary>
    MultiviolaFaceFaint,
    /// <summary>Multiviola: a fireball, its face forming.</summary>
    MultiviolaFaceForming,
    /// <summary>Multiviola: a fireball, its face dim.</summary>
    MultiviolaFaceDim,
    /// <summary>Multiviola: a fireball, its face lit.</summary>
    MultiviolaFaceLit,
    /// <summary>Multiviola: a fireball, its face bright.</summary>
    MultiviolaFaceBright,
    /// <summary>Multiviola: a fireball, its face brighter.</summary>
    MultiviolaFaceBrighter,
    /// <summary>Multiviola: a fireball, its face brightest.</summary>
    MultiviolaFaceBrightest,
    /// <summary>The Norfair lava jumper: claws curled down beneath it.</summary>
    NorfairLavaJumperClawsCurledDown,
    /// <summary>The Norfair lava jumper: claws spreading.</summary>
    NorfairLavaJumperClawsSpreading,
    /// <summary>The Norfair lava jumper: claws spread.</summary>
    NorfairLavaJumperClawsSpread,
    /// <summary>The Norfair lava jumper: claws spread flat.</summary>
    NorfairLavaJumperClawsSpreadFlat,
    /// <summary>The Norfair lava jumper: claws raising.</summary>
    NorfairLavaJumperClawsRaising,
    /// <summary>The Norfair lava jumper: claws raised.</summary>
    NorfairLavaJumperClawsRaised,
    /// <summary>The Norfair lava jumper: claws folded up over it.</summary>
    NorfairLavaJumperClawsFolded,
    /// <summary>The Norfair lava jumper: the flame trailing it, flared wide.</summary>
    NorfairLavaJumperFlameFlare,
    /// <summary>The Norfair lava jumper: the flame trailing it, small.</summary>
    NorfairLavaJumperFlameSmall,
    /// <summary>The Norfair lava jumper: the flame trailing it, tall.</summary>
    NorfairLavaJumperFlameTall,
    /// <summary>The Chozo statue: hands cupped together before its chest.</summary>
    ChozoStatueLowerNorfairHandsCupped,
    /// <summary>The Chozo statue: a hand reaching out.</summary>
    ChozoStatueLowerNorfairHandReaching,
    /// <summary>The Chozo statue: a hand stretched out flat.</summary>
    ChozoStatueLowerNorfairHandOutstretched,
    /// <summary>The Chozo statue: its outstretched hand opening.</summary>
    ChozoStatueLowerNorfairHandOpening,
    /// <summary>The Chozo statue: its outstretched hand open.</summary>
    ChozoStatueLowerNorfairHandOpen,
    /// <summary>The Chozo statue: its outstretched hand open: the picture of ChozoStatueLowerNorfairHandOpen, as a frame of its own.</summary>
    ChozoStatueLowerNorfairHandOpenTwin,
    /// <summary>The Chozo statue: the fingers of its outstretched hand curling.</summary>
    ChozoStatueLowerNorfairFingersCurling,
    /// <summary>The Chozo statue: the fingers of its outstretched hand curled.</summary>
    ChozoStatueLowerNorfairFingersCurled,
    /// <summary>The Chozo statue: the fingers of its outstretched hand clenched.</summary>
    ChozoStatueLowerNorfairFingersClenched,
    /// <summary>The Chozo statue: hands cupped together before its chest.</summary>
    ChozoStatueWreckedShipHandsCupped,
    /// <summary>The Chozo statue: a hand reaching out.</summary>
    ChozoStatueWreckedShipHandReaching,
    /// <summary>The Chozo statue: a hand stretched out flat.</summary>
    ChozoStatueWreckedShipHandOutstretched,
    /// <summary>The Chozo statue: its outstretched hand opening.</summary>
    ChozoStatueWreckedShipHandOpening,
    /// <summary>The Chozo statue: its outstretched hand open.</summary>
    ChozoStatueWreckedShipHandOpen,
    /// <summary>The Chozo statue: its outstretched hand open: the picture of ChozoStatueWreckedShipHandOpen, as a frame of its own.</summary>
    ChozoStatueWreckedShipHandOpenTwin,
    /// <summary>The Chozo statue: the fingers of its outstretched hand curling.</summary>
    ChozoStatueWreckedShipFingersCurling,
    /// <summary>The Chozo statue: the fingers of its outstretched hand curled.</summary>
    ChozoStatueWreckedShipFingersCurled,
    /// <summary>The Chozo statue: the fingers of its outstretched hand clenched.</summary>
    ChozoStatueWreckedShipFingersClenched,
    /// <summary>The Chozo statue: walking, its near leg lifting.</summary>
    ChozoStatueWreckedShipWalkingNearLegLifting,
    /// <summary>The Chozo statue: walking, its near leg striding out.</summary>
    ChozoStatueWreckedShipWalkingNearLegStriding,
    /// <summary>The Chozo statue: walking, its near leg planting.</summary>
    ChozoStatueWreckedShipWalkingNearLegPlanting,
    /// <summary>The Chozo statue: walking, its near leg planted, legs together.</summary>
    ChozoStatueWreckedShipWalkingNearLegPlanted,
    /// <summary>The Chozo statue: walking, its far leg lifting.</summary>
    ChozoStatueWreckedShipWalkingFarLegLifting,
    /// <summary>The Chozo statue: walking, its far leg striding out.</summary>
    ChozoStatueWreckedShipWalkingFarLegStriding,
    /// <summary>The Chozo statue: walking, its far leg planting.</summary>
    ChozoStatueWreckedShipWalkingFarLegPlanting,
    /// <summary>The Chozo statue: walking, its far leg planted, legs together.</summary>
    ChozoStatueWreckedShipWalkingFarLegPlanted,
    /// <summary>Viola: a plain glowing ball.</summary>
    ViolaPlain,
    /// <summary>Viola: a glowing ball, its face faint.</summary>
    ViolaFaceFaint,
    /// <summary>Viola: a glowing ball, its face forming.</summary>
    ViolaFaceForming,
    /// <summary>Viola: a glowing ball, its face dim.</summary>
    ViolaFaceDim,
    /// <summary>Viola: a glowing ball, its face lit.</summary>
    ViolaFaceLit,
    /// <summary>Viola: a glowing ball, its face bright.</summary>
    ViolaFaceBright,
    /// <summary>Viola: a glowing ball, its face brighter.</summary>
    ViolaFaceBrighter,
    /// <summary>Viola: a glowing ball, its face brightest.</summary>
    ViolaFaceBrightest,
    /// <summary>Rinka: a white-hot ring.</summary>
    RinkaWhiteHot,
    /// <summary>Rinka: a yellow ring.</summary>
    RinkaYellow,
    /// <summary>Rinka: an orange ring.</summary>
    RinkaOrange,
    /// <summary>Rinka: a red ring.</summary>
    RinkaRed,
    /// <summary>Rinka: a dark red ring.</summary>
    RinkaDarkRed,
    /// <summary>The dead Torizo: its lifeless body standing hunched.</summary>
    DeadTorizoStanding,
    /// <summary>The dead Zoomer: lying on its back.</summary>
    DeadZoomerCorpseOnItsBack,
    /// <summary>The dead Zoomer: lying the right way up.</summary>
    DeadZoomerCorpseUpright,
    /// <summary>The dead Ripper: lying level.</summary>
    DeadRipperCorpseLevel,
    /// <summary>The dead Ripper: lying tilted on a diagonal.</summary>
    DeadRipperCorpseTilted,
    /// <summary>The dead Skree: its corpse.</summary>
    DeadSkreeCorpse,
    /// <summary>The dead Sidehopper: legs bent.</summary>
    DeadSidehopperLegsBent,
    /// <summary>The dead Sidehopper: crouched low.</summary>
    DeadSidehopperLegsCrouched,
    /// <summary>The dead Sidehopper: legs straight.</summary>
    DeadSidehopperLegsStraight,
    /// <summary>The dead Sidehopper: sprawled flat.</summary>
    DeadSidehopperSprawled,
    /// <summary>The dead Sidehopper: sprawled flat, legs raised.</summary>
    DeadSidehopperSprawledLegsUp,
    /// <summary>Mochtroid: floating, its membrane wide.</summary>
    MochtroidFloatingWide,
    /// <summary>Mochtroid: floating, its membrane squeezing in.</summary>
    MochtroidFloatingSqueezing,
    /// <summary>Mochtroid: floating, its membrane squeezed narrow.</summary>
    MochtroidFloatingSqueezed,
    /// <summary>Mochtroid: latched on, nucleus bright, its membrane wide.</summary>
    MochtroidLatchedWide,
    /// <summary>Mochtroid: latched on, nucleus bright, its membrane squeezing in.</summary>
    MochtroidLatchedSqueezing,
    /// <summary>Mochtroid: latched on, nucleus bright, its membrane squeezed narrow.</summary>
    MochtroidLatchedSqueezed,
    /// <summary>Evir: its body facing left, legs tucked.</summary>
    EvirBodyFacingLeftLegsTucked,
    /// <summary>Evir: its body facing left, legs curling.</summary>
    EvirBodyFacingLeftLegsCurling,
    /// <summary>Evir: its body facing left, legs lowering.</summary>
    EvirBodyFacingLeftLegsLowering,
    /// <summary>Evir: its body facing left, legs hanging.</summary>
    EvirBodyFacingLeftLegsHanging,
    /// <summary>Evir: its body facing left, legs stretched down.</summary>
    EvirBodyFacingLeftLegsStretched,
    /// <summary>Evir: its body facing left, legs lifting.</summary>
    EvirBodyFacingLeftLegsLifting,
    /// <summary>Evir: its arms facing left, reaching forward.</summary>
    EvirArmsFacingLeftReaching,
    /// <summary>Evir: its arms facing left, curling in.</summary>
    EvirArmsFacingLeftCurling,
    /// <summary>Evir: its arms facing left, folding.</summary>
    EvirArmsFacingLeftFolding,
    /// <summary>Evir: its arms facing left, spreading.</summary>
    EvirArmsFacingLeftSpreading,
    /// <summary>Evir: its arms facing left, folded back.</summary>
    EvirArmsFacingLeftFolded,
    /// <summary>Evir: its body facing right, legs tucked.</summary>
    EvirBodyFacingRightLegsTucked,
    /// <summary>Evir: its body facing right, legs curling.</summary>
    EvirBodyFacingRightLegsCurling,
    /// <summary>Evir: its body facing right, legs lowering.</summary>
    EvirBodyFacingRightLegsLowering,
    /// <summary>Evir: its body facing right, legs hanging.</summary>
    EvirBodyFacingRightLegsHanging,
    /// <summary>Evir: its body facing right, legs stretched down.</summary>
    EvirBodyFacingRightLegsStretched,
    /// <summary>Evir: its body facing right, legs lifting.</summary>
    EvirBodyFacingRightLegsLifting,
    /// <summary>Evir: its arms facing right, reaching forward.</summary>
    EvirArmsFacingRightReaching,
    /// <summary>Evir: its arms facing right, curling in.</summary>
    EvirArmsFacingRightCurling,
    /// <summary>Evir: its arms facing right, folding.</summary>
    EvirArmsFacingRightFolding,
    /// <summary>Evir: its arms facing right, spreading.</summary>
    EvirArmsFacingRightSpreading,
    /// <summary>Evir: its arms facing right, folded back.</summary>
    EvirArmsFacingRightFolded,
    /// <summary>Evir: the spined ball it throws.</summary>
    EvirSpineBall,
    /// <summary>Evir: the spined ball it throws, small as it regrows.</summary>
    EvirSpineBallRegrowing,
    /// <summary>The work robot: walking left on its near foot, leaning back.</summary>
    WorkRobotWalkingLeftNearFootLeanBack,
    /// <summary>The work robot: walking left on its near foot, easing upright.</summary>
    WorkRobotWalkingLeftNearFootEasingUp,
    /// <summary>The work robot: walking left on its near foot, upright.</summary>
    WorkRobotWalkingLeftNearFootUpright,
    /// <summary>The work robot: walking left on its near foot, easing forward.</summary>
    WorkRobotWalkingLeftNearFootEasingForward,
    /// <summary>The work robot: walking left on its near foot, leaning forward.</summary>
    WorkRobotWalkingLeftNearFootLeanForward,
    /// <summary>The work robot: walking left on its near foot, settling upright.</summary>
    WorkRobotWalkingLeftNearFootSettling,
    /// <summary>The work robot: walking left on its far foot, leaning back.</summary>
    WorkRobotWalkingLeftFarFootLeanBack,
    /// <summary>The work robot: walking left on its far foot, easing upright.</summary>
    WorkRobotWalkingLeftFarFootEasingUp,
    /// <summary>The work robot: walking left on its far foot, upright.</summary>
    WorkRobotWalkingLeftFarFootUpright,
    /// <summary>The work robot: walking left on its far foot, easing forward.</summary>
    WorkRobotWalkingLeftFarFootEasingForward,
    /// <summary>The work robot: walking left on its far foot, leaning forward.</summary>
    WorkRobotWalkingLeftFarFootLeanForward,
    /// <summary>The work robot: walking left on its far foot, settling upright.</summary>
    WorkRobotWalkingLeftFarFootSettling,
    /// <summary>The work robot: walking right on its near foot, leaning back.</summary>
    WorkRobotWalkingRightNearFootLeanBack,
    /// <summary>The work robot: walking right on its near foot, easing upright.</summary>
    WorkRobotWalkingRightNearFootEasingUp,
    /// <summary>The work robot: walking right on its near foot, upright.</summary>
    WorkRobotWalkingRightNearFootUpright,
    /// <summary>The work robot: walking right on its near foot, easing forward.</summary>
    WorkRobotWalkingRightNearFootEasingForward,
    /// <summary>The work robot: walking right on its near foot, leaning forward.</summary>
    WorkRobotWalkingRightNearFootLeanForward,
    /// <summary>The work robot: walking right on its near foot, settling upright.</summary>
    WorkRobotWalkingRightNearFootSettling,
    /// <summary>The work robot: walking right on its far foot, leaning back.</summary>
    WorkRobotWalkingRightFarFootLeanBack,
    /// <summary>The work robot: walking right on its far foot, easing upright.</summary>
    WorkRobotWalkingRightFarFootEasingUp,
    /// <summary>The work robot: walking right on its far foot, upright.</summary>
    WorkRobotWalkingRightFarFootUpright,
    /// <summary>The work robot: walking right on its far foot, easing forward.</summary>
    WorkRobotWalkingRightFarFootEasingForward,
    /// <summary>The work robot: walking right on its far foot, leaning forward.</summary>
    WorkRobotWalkingRightFarFootLeanForward,
    /// <summary>The work robot: walking right on its far foot, settling upright.</summary>
    WorkRobotWalkingRightFarFootSettling,
    /// <summary>The work robot: unpowered, standing upright.</summary>
    WorkRobotUnpoweredUpright,
    /// <summary>The work robot: unpowered, tilted left.</summary>
    WorkRobotUnpoweredTiltedLeft,
    /// <summary>The work robot: unpowered, tilted right.</summary>
    WorkRobotUnpoweredTiltedRight,
    /// <summary>Yard: rounding an outside corner with its top to the right, facing up, turned a quarter of the way round.</summary>
    YardRoundingOutsideCornerUpsideRightFacingUpTurned22,
    /// <summary>Yard: rounding an outside corner with its top to the right, facing up, turned half way round.</summary>
    YardRoundingOutsideCornerUpsideRightFacingUpTurned45,
    /// <summary>Yard: rounding an outside corner with its top to the right, facing up, turned three quarters of the way round.</summary>
    YardRoundingOutsideCornerUpsideRightFacingUpTurned67,
    /// <summary>Yard: crawling upright, facing left, legs tucked.</summary>
    YardCrawlingUpsideUpFacingLeftLegsTucked,
    /// <summary>Yard: crawling upright, facing left, legs spread.</summary>
    YardCrawlingUpsideUpFacingLeftLegsSpread,
    /// <summary>Yard: crawling upright, facing left, legs stepping.</summary>
    YardCrawlingUpsideUpFacingLeftLegsStepping,
    /// <summary>Yard: rounding an outside corner upright, facing left, turned a quarter of the way round.</summary>
    YardRoundingOutsideCornerUpsideUpFacingLeftTurned22,
    /// <summary>Yard: rounding an outside corner upright, facing left, turned half way round.</summary>
    YardRoundingOutsideCornerUpsideUpFacingLeftTurned45,
    /// <summary>Yard: rounding an outside corner upright, facing left, turned three quarters of the way round.</summary>
    YardRoundingOutsideCornerUpsideUpFacingLeftTurned67,
    /// <summary>Yard: crawling with its top to the left, facing down, legs tucked.</summary>
    YardCrawlingUpsideLeftFacingDownLegsTucked,
    /// <summary>Yard: crawling with its top to the left, facing down, legs spread.</summary>
    YardCrawlingUpsideLeftFacingDownLegsSpread,
    /// <summary>Yard: crawling with its top to the left, facing down, legs stepping.</summary>
    YardCrawlingUpsideLeftFacingDownLegsStepping,
    /// <summary>Yard: rounding an outside corner with its top to the left, facing down, turned a quarter of the way round.</summary>
    YardRoundingOutsideCornerUpsideLeftFacingDownTurned22,
    /// <summary>Yard: rounding an outside corner with its top to the left, facing down, turned half way round.</summary>
    YardRoundingOutsideCornerUpsideLeftFacingDownTurned45,
    /// <summary>Yard: rounding an outside corner with its top to the left, facing down, turned three quarters of the way round.</summary>
    YardRoundingOutsideCornerUpsideLeftFacingDownTurned67,
    /// <summary>Yard: crawling upside down, facing right, legs tucked.</summary>
    YardCrawlingUpsideDownFacingRightLegsTucked,
    /// <summary>Yard: crawling upside down, facing right, legs spread.</summary>
    YardCrawlingUpsideDownFacingRightLegsSpread,
    /// <summary>Yard: crawling upside down, facing right, legs stepping.</summary>
    YardCrawlingUpsideDownFacingRightLegsStepping,
    /// <summary>Yard: rounding an outside corner upside down, facing right, turned a quarter of the way round.</summary>
    YardRoundingOutsideCornerUpsideDownFacingRightTurned22,
    /// <summary>Yard: rounding an outside corner upside down, facing right, turned half way round.</summary>
    YardRoundingOutsideCornerUpsideDownFacingRightTurned45,
    /// <summary>Yard: rounding an outside corner upside down, facing right, turned three quarters of the way round.</summary>
    YardRoundingOutsideCornerUpsideDownFacingRightTurned67,
    /// <summary>Yard: crawling with its top to the right, facing up, legs tucked.</summary>
    YardCrawlingUpsideRightFacingUpLegsTucked,
    /// <summary>Yard: crawling with its top to the right, facing up, legs spread.</summary>
    YardCrawlingUpsideRightFacingUpLegsSpread,
    /// <summary>Yard: crawling with its top to the right, facing up, legs stepping.</summary>
    YardCrawlingUpsideRightFacingUpLegsStepping,
    /// <summary>Yard: rounding an outside corner with its top to the left, facing up, turned a quarter of the way round.</summary>
    YardRoundingOutsideCornerUpsideLeftFacingUpTurned22,
    /// <summary>Yard: rounding an outside corner with its top to the left, facing up, turned half way round.</summary>
    YardRoundingOutsideCornerUpsideLeftFacingUpTurned45,
    /// <summary>Yard: rounding an outside corner with its top to the left, facing up, turned three quarters of the way round.</summary>
    YardRoundingOutsideCornerUpsideLeftFacingUpTurned67,
    /// <summary>Yard: crawling upright, facing right, legs tucked.</summary>
    YardCrawlingUpsideUpFacingRightLegsTucked,
    /// <summary>Yard: crawling upright, facing right, legs spread.</summary>
    YardCrawlingUpsideUpFacingRightLegsSpread,
    /// <summary>Yard: crawling upright, facing right, legs stepping.</summary>
    YardCrawlingUpsideUpFacingRightLegsStepping,
    /// <summary>Yard: rounding an outside corner upright, facing right, turned a quarter of the way round.</summary>
    YardRoundingOutsideCornerUpsideUpFacingRightTurned22,
    /// <summary>Yard: rounding an outside corner upright, facing right, turned half way round.</summary>
    YardRoundingOutsideCornerUpsideUpFacingRightTurned45,
    /// <summary>Yard: rounding an outside corner upright, facing right, turned three quarters of the way round.</summary>
    YardRoundingOutsideCornerUpsideUpFacingRightTurned67,
    /// <summary>Yard: crawling with its top to the right, facing down, legs tucked.</summary>
    YardCrawlingUpsideRightFacingDownLegsTucked,
    /// <summary>Yard: crawling with its top to the right, facing down, legs spread.</summary>
    YardCrawlingUpsideRightFacingDownLegsSpread,
    /// <summary>Yard: crawling with its top to the right, facing down, legs stepping.</summary>
    YardCrawlingUpsideRightFacingDownLegsStepping,
    /// <summary>Yard: rounding an outside corner with its top to the right, facing down, turned a quarter of the way round.</summary>
    YardRoundingOutsideCornerUpsideRightFacingDownTurned22,
    /// <summary>Yard: rounding an outside corner with its top to the right, facing down, turned half way round.</summary>
    YardRoundingOutsideCornerUpsideRightFacingDownTurned45,
    /// <summary>Yard: rounding an outside corner with its top to the right, facing down, turned three quarters of the way round.</summary>
    YardRoundingOutsideCornerUpsideRightFacingDownTurned67,
    /// <summary>Yard: crawling upside down, facing left, legs tucked.</summary>
    YardCrawlingUpsideDownFacingLeftLegsTucked,
    /// <summary>Yard: crawling upside down, facing left, legs spread.</summary>
    YardCrawlingUpsideDownFacingLeftLegsSpread,
    /// <summary>Yard: crawling upside down, facing left, legs stepping.</summary>
    YardCrawlingUpsideDownFacingLeftLegsStepping,
    /// <summary>Yard: rounding an outside corner upside down, facing left, turned a quarter of the way round.</summary>
    YardRoundingOutsideCornerUpsideDownFacingLeftTurned22,
    /// <summary>Yard: rounding an outside corner upside down, facing left, turned half way round.</summary>
    YardRoundingOutsideCornerUpsideDownFacingLeftTurned45,
    /// <summary>Yard: rounding an outside corner upside down, facing left, turned three quarters of the way round.</summary>
    YardRoundingOutsideCornerUpsideDownFacingLeftTurned67,
    /// <summary>Yard: crawling with its top to the left, facing up, legs tucked.</summary>
    YardCrawlingUpsideLeftFacingUpLegsTucked,
    /// <summary>Yard: crawling with its top to the left, facing up, legs spread.</summary>
    YardCrawlingUpsideLeftFacingUpLegsSpread,
    /// <summary>Yard: crawling with its top to the left, facing up, legs stepping.</summary>
    YardCrawlingUpsideLeftFacingUpLegsStepping,
    /// <summary>Yard: rounding an inside corner upright, facing left, turned a quarter of the way round.</summary>
    YardRoundingInsideCornerUpsideUpFacingLeftTurned22,
    /// <summary>Yard: rounding an inside corner upright, facing left, turned half way round.</summary>
    YardRoundingInsideCornerUpsideUpFacingLeftTurned45,
    /// <summary>Yard: rounding an inside corner upright, facing left, turned three quarters of the way round.</summary>
    YardRoundingInsideCornerUpsideUpFacingLeftTurned67,
    /// <summary>Yard: rounding an inside corner with its top to the right, facing up, turned a quarter of the way round.</summary>
    YardRoundingInsideCornerUpsideRightFacingUpTurned22,
    /// <summary>Yard: rounding an inside corner with its top to the right, facing up, turned half way round.</summary>
    YardRoundingInsideCornerUpsideRightFacingUpTurned45,
    /// <summary>Yard: rounding an inside corner with its top to the right, facing up, turned three quarters of the way round.</summary>
    YardRoundingInsideCornerUpsideRightFacingUpTurned67,
    /// <summary>Yard: rounding an inside corner upside down, facing right, turned a quarter of the way round.</summary>
    YardRoundingInsideCornerUpsideDownFacingRightTurned22,
    /// <summary>Yard: rounding an inside corner upside down, facing right, turned half way round.</summary>
    YardRoundingInsideCornerUpsideDownFacingRightTurned45,
    /// <summary>Yard: rounding an inside corner upside down, facing right, turned three quarters of the way round.</summary>
    YardRoundingInsideCornerUpsideDownFacingRightTurned67,
    /// <summary>Yard: rounding an inside corner with its top to the left, facing down, turned a quarter of the way round.</summary>
    YardRoundingInsideCornerUpsideLeftFacingDownTurned22,
    /// <summary>Yard: rounding an inside corner with its top to the left, facing down, turned half way round.</summary>
    YardRoundingInsideCornerUpsideLeftFacingDownTurned45,
    /// <summary>Yard: rounding an inside corner with its top to the left, facing down, turned three quarters of the way round.</summary>
    YardRoundingInsideCornerUpsideLeftFacingDownTurned67,
    /// <summary>Yard: rounding an inside corner upright, facing right, turned a quarter of the way round.</summary>
    YardRoundingInsideCornerUpsideUpFacingRightTurned22,
    /// <summary>Yard: rounding an inside corner upright, facing right, turned half way round.</summary>
    YardRoundingInsideCornerUpsideUpFacingRightTurned45,
    /// <summary>Yard: rounding an inside corner upright, facing right, turned three quarters of the way round.</summary>
    YardRoundingInsideCornerUpsideUpFacingRightTurned67,
    /// <summary>Yard: rounding an inside corner with its top to the left, facing up, turned a quarter of the way round.</summary>
    YardRoundingInsideCornerUpsideLeftFacingUpTurned22,
    /// <summary>Yard: rounding an inside corner with its top to the left, facing up, turned half way round.</summary>
    YardRoundingInsideCornerUpsideLeftFacingUpTurned45,
    /// <summary>Yard: rounding an inside corner with its top to the left, facing up, turned three quarters of the way round.</summary>
    YardRoundingInsideCornerUpsideLeftFacingUpTurned67,
    /// <summary>Yard: rounding an inside corner upside down, facing left, turned a quarter of the way round.</summary>
    YardRoundingInsideCornerUpsideDownFacingLeftTurned22,
    /// <summary>Yard: rounding an inside corner upside down, facing left, turned half way round.</summary>
    YardRoundingInsideCornerUpsideDownFacingLeftTurned45,
    /// <summary>Yard: rounding an inside corner upside down, facing left, turned three quarters of the way round.</summary>
    YardRoundingInsideCornerUpsideDownFacingLeftTurned67,
    /// <summary>Yard: rounding an inside corner with its top to the right, facing down, turned a quarter of the way round.</summary>
    YardRoundingInsideCornerUpsideRightFacingDownTurned22,
    /// <summary>Yard: rounding an inside corner with its top to the right, facing down, turned half way round.</summary>
    YardRoundingInsideCornerUpsideRightFacingDownTurned45,
    /// <summary>Yard: rounding an inside corner with its top to the right, facing down, turned three quarters of the way round.</summary>
    YardRoundingInsideCornerUpsideRightFacingDownTurned67,
    /// <summary>Yard: upright, facing left, tucking its legs into its shell.</summary>
    YardUpsideUpFacingLeftLegsTucking,
    /// <summary>Yard: upright, facing left, withdrawn into its closed shell.</summary>
    YardUpsideUpFacingLeftShellClosed,
    /// <summary>Yard: upright, facing left, withdrawn into its closed shell: the picture of YardUpsideUpFacingLeftShellClosed, as a frame of its own.</summary>
    YardUpsideUpFacingLeftShellClosedTwin,
    /// <summary>Yard: upright, facing left, tucking its legs into its shell: the picture of YardUpsideUpFacingLeftLegsTucking, drawn 3 pixels left of it.</summary>
    YardUpsideUpFacingLeftLegsTuckingShiftedLeft3,
    /// <summary>Yard: upside down, facing left, tucking its legs into its shell.</summary>
    YardUpsideDownFacingLeftLegsTucking,
    /// <summary>Yard: upside down, facing left, withdrawn into its closed shell.</summary>
    YardUpsideDownFacingLeftShellClosed,
    /// <summary>Yard: upside down, facing left, withdrawn into its closed shell: the picture of YardUpsideDownFacingLeftShellClosed, as a frame of its own.</summary>
    YardUpsideDownFacingLeftShellClosedTwin,
    /// <summary>Yard: upside down, facing left, tucking its legs into its shell: the picture of YardUpsideDownFacingLeftLegsTucking, drawn 2 pixels left of it.</summary>
    YardUpsideDownFacingLeftLegsTuckingShiftedLeft2,
    /// <summary>Yard: upside down, facing right, tucking its legs into its shell.</summary>
    YardUpsideDownFacingRightLegsTucking,
    /// <summary>Yard: upside down, facing right, withdrawn into its closed shell.</summary>
    YardUpsideDownFacingRightShellClosed,
    /// <summary>Yard: upside down, facing right, withdrawn into its closed shell: the picture of YardUpsideDownFacingRightShellClosed, as a frame of its own.</summary>
    YardUpsideDownFacingRightShellClosedTwin,
    /// <summary>Yard: upside down, facing right, tucking its legs into its shell: the picture of YardUpsideDownFacingRightLegsTucking, drawn 2 pixels right of it.</summary>
    YardUpsideDownFacingRightLegsTuckingShiftedRight2,
    /// <summary>Yard: upright, facing right, tucking its legs into its shell.</summary>
    YardUpsideUpFacingRightLegsTucking,
    /// <summary>Yard: upright, facing right, withdrawn into its closed shell.</summary>
    YardUpsideUpFacingRightShellClosed,
    /// <summary>Yard: upright, facing right, withdrawn into its closed shell: the picture of YardUpsideUpFacingRightShellClosed, as a frame of its own.</summary>
    YardUpsideUpFacingRightShellClosedTwin,
    /// <summary>Yard: upright, facing right, tucking its legs into its shell: the picture of YardUpsideUpFacingRightLegsTucking, drawn 3 pixels right of it.</summary>
    YardUpsideUpFacingRightLegsTuckingShiftedRight3,
    /// <summary>Yard: with its top to the right, facing up, tucking its legs into its shell.</summary>
    YardUpsideRightFacingUpLegsTucking,
    /// <summary>Yard: with its top to the right, facing up, withdrawn into its closed shell.</summary>
    YardUpsideRightFacingUpShellClosed,
    /// <summary>Yard: with its top to the right, facing up, withdrawn into its closed shell: the picture of YardUpsideRightFacingUpShellClosed, as a frame of its own.</summary>
    YardUpsideRightFacingUpShellClosedTwin,
    /// <summary>Yard: with its top to the right, facing up, tucking its legs further into its shell.</summary>
    YardUpsideRightFacingUpLegsTuckingFurther,
    /// <summary>Yard: with its top to the left, facing up, tucking its legs into its shell.</summary>
    YardUpsideLeftFacingUpLegsTucking,
    /// <summary>Yard: with its top to the left, facing up, withdrawn into its closed shell.</summary>
    YardUpsideLeftFacingUpShellClosed,
    /// <summary>Yard: with its top to the left, facing up, withdrawn into its closed shell: the picture of YardUpsideLeftFacingUpShellClosed, as a frame of its own.</summary>
    YardUpsideLeftFacingUpShellClosedTwin,
    /// <summary>Yard: with its top to the left, facing up, tucking its legs into its shell: the picture of YardUpsideLeftFacingUpLegsTucking, drawn 2 pixels up of it.</summary>
    YardUpsideLeftFacingUpLegsTuckingShiftedUp2,
    /// <summary>Yard: with its top to the left, facing down, tucking its legs into its shell.</summary>
    YardUpsideLeftFacingDownLegsTucking,
    /// <summary>Yard: with its top to the left, facing down, withdrawn into its closed shell.</summary>
    YardUpsideLeftFacingDownShellClosed,
    /// <summary>Yard: with its top to the left, facing down, withdrawn into its closed shell: the picture of YardUpsideLeftFacingDownShellClosed, as a frame of its own.</summary>
    YardUpsideLeftFacingDownShellClosedTwin,
    /// <summary>Yard: with its top to the left, facing down, tucking its legs into its shell: the picture of YardUpsideLeftFacingDownLegsTucking, drawn 2 pixels down of it.</summary>
    YardUpsideLeftFacingDownLegsTuckingShiftedDown2,
    /// <summary>Yard: with its top to the right, facing down, tucking its legs into its shell.</summary>
    YardUpsideRightFacingDownLegsTucking,
    /// <summary>Yard: with its top to the right, facing down, withdrawn into its closed shell.</summary>
    YardUpsideRightFacingDownShellClosed,
    /// <summary>Yard: with its top to the right, facing down, withdrawn into its closed shell: the picture of YardUpsideRightFacingDownShellClosed, as a frame of its own.</summary>
    YardUpsideRightFacingDownShellClosedTwin,
    /// <summary>Yard: with its top to the right, facing down, tucking its legs into its shell: the picture of YardUpsideRightFacingDownLegsTucking, drawn 2 pixels down of it.</summary>
    YardUpsideRightFacingDownLegsTuckingShiftedDown2,
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
    /// <summary>Samus's gunship: its entrance pad shut.</summary>
    GunshipEntrancePadShut,
    /// <summary>Samus's gunship: its entrance pad shut, 1 pixels higher.</summary>
    GunshipEntrancePadShutRaised1,
    /// <summary>Samus's gunship: its entrance pad shut, 2 pixels higher.</summary>
    GunshipEntrancePadShutRaised2,
    /// <summary>Samus's gunship: its entrance pad shut, 4 pixels higher.</summary>
    GunshipEntrancePadShutRaised4,
    /// <summary>Samus's gunship: its entrance pad opening, 32 pixels wide.</summary>
    GunshipEntrancePadOpening32Wide,
    /// <summary>Samus's gunship: its entrance pad opening, 36 pixels wide.</summary>
    GunshipEntrancePadOpening36Wide,
    /// <summary>Samus's gunship: its entrance pad opening, 40 pixels wide.</summary>
    GunshipEntrancePadOpening40Wide,
    /// <summary>Samus's gunship: its entrance pad opening, 44 pixels wide.</summary>
    GunshipEntrancePadOpening44Wide,
    /// <summary>Samus's gunship: its entrance pad opening, 48 pixels wide.</summary>
    GunshipEntrancePadOpening48Wide,
    /// <summary>Samus's gunship: its entrance pad opening, 52 pixels wide.</summary>
    GunshipEntrancePadOpening52Wide,
    /// <summary>Samus's gunship: entrance pad open.</summary>
    GunshipEntrancePadOpen,
    /// <summary>Samus's gunship: top hull.</summary>
    GunshipTopHull,
    /// <summary>Samus's gunship: bottom hull.</summary>
    GunshipBottomHull,
    /// <summary>The baby turtle: crawling left, legs tucked.</summary>
    TurtleBabyCrawlingLeftLegsTucked,
    /// <summary>The baby turtle: crawling left, legs reaching.</summary>
    TurtleBabyCrawlingLeftLegsReaching,
    /// <summary>The baby turtle: crawling left, legs planted.</summary>
    TurtleBabyCrawlingLeftLegsPlanted,
    /// <summary>The baby turtle: crawling left, legs pushing.</summary>
    TurtleBabyCrawlingLeftLegsPushing,
    /// <summary>The baby turtle: crawling left, legs trailing.</summary>
    TurtleBabyCrawlingLeftLegsTrailing,
    /// <summary>The baby turtle: crawling left, legs lifting.</summary>
    TurtleBabyCrawlingLeftLegsLifting,
    /// <summary>The baby turtle: crawling left, legs returning.</summary>
    TurtleBabyCrawlingLeftLegsReturning,
    /// <summary>The baby turtle: withdrawn into its spinning shell, its shell turned side on.</summary>
    TurtleBabyShellTurned0,
    /// <summary>The baby turtle: withdrawn into its spinning shell, its shell turned to the viewer.</summary>
    TurtleBabyShellTurned90,
    /// <summary>The baby turtle: withdrawn into its spinning shell, its shell turned to its other side.</summary>
    TurtleBabyShellTurned180,
    /// <summary>The baby turtle: withdrawn into its spinning shell, its shell turned away.</summary>
    TurtleBabyShellTurned270,
    /// <summary>The mama turtle: withdrawn into its shell, turned left.</summary>
    TurtleMamaInShellFacingLeft,
    /// <summary>The mama turtle: withdrawn into its shell, turned to the viewer.</summary>
    TurtleMamaInShellFacingViewer,
    /// <summary>The mama turtle: withdrawn into its shell, turned right.</summary>
    TurtleMamaInShellFacingRight,
    /// <summary>The mama turtle: head and legs out of its shell, facing left.</summary>
    TurtleMamaOutOfShellFacingLeft,
    /// <summary>The mama turtle: head and legs withdrawing into its shell, facing left.</summary>
    TurtleMamaWithdrawingFacingLeft,
    /// <summary>The mama turtle: head and legs withdrawn, its shell facing left.</summary>
    TurtleMamaWithdrawnFacingLeft,
    /// <summary>The baby turtle: crawling right, legs tucked.</summary>
    TurtleBabyCrawlingRightLegsTucked,
    /// <summary>The baby turtle: crawling right, legs reaching.</summary>
    TurtleBabyCrawlingRightLegsReaching,
    /// <summary>The baby turtle: crawling right, legs planted.</summary>
    TurtleBabyCrawlingRightLegsPlanted,
    /// <summary>The baby turtle: crawling right, legs pushing.</summary>
    TurtleBabyCrawlingRightLegsPushing,
    /// <summary>The baby turtle: crawling right, legs trailing.</summary>
    TurtleBabyCrawlingRightLegsTrailing,
    /// <summary>The baby turtle: crawling right, legs lifting.</summary>
    TurtleBabyCrawlingRightLegsLifting,
    /// <summary>The baby turtle: crawling right, legs returning.</summary>
    TurtleBabyCrawlingRightLegsReturning,
    /// <summary>The mama turtle: head and legs out of its shell, facing right.</summary>
    TurtleMamaOutOfShellFacingRight,
    /// <summary>The mama turtle: head and legs withdrawing into its shell, facing right.</summary>
    TurtleMamaWithdrawingFacingRight,
    /// <summary>The mama turtle: head and legs withdrawn, its shell facing right.</summary>
    TurtleMamaWithdrawnFacingRight,
    /// <summary>The mama turtle: withdrawn into its shell, turned right: the picture of TurtleMamaInShellFacingRight, as a frame of its own.</summary>
    TurtleMamaInShellFacingRightTwin,
    /// <summary>The baby turtle: withdrawn into its shell, turned to its other side: the picture of TurtleBabyShellTurned180, as a frame of its own.</summary>
    TurtleBabyShellTurned180Twin,
    /// <summary>Zero: crawling with its top right, contracted.</summary>
    ZeroUpsideRightContracted,
    /// <summary>Zero: crawling with its top right, stretching, the gaps between its segments showing.</summary>
    ZeroUpsideRightStretching,
    /// <summary>Zero: crawling with its top right, stretched, its segments parted.</summary>
    ZeroUpsideRightStretched,
    /// <summary>Zero: crawling with its top right, fully stretched, its segments wide apart.</summary>
    ZeroUpsideRightFullyStretched,
    /// <summary>Zero: crawling with its top left, contracted.</summary>
    ZeroUpsideLeftContracted,
    /// <summary>Zero: crawling with its top left, stretching, the gaps between its segments showing.</summary>
    ZeroUpsideLeftStretching,
    /// <summary>Zero: crawling with its top left, stretched, its segments parted.</summary>
    ZeroUpsideLeftStretched,
    /// <summary>Zero: crawling with its top left, fully stretched, its segments wide apart.</summary>
    ZeroUpsideLeftFullyStretched,
    /// <summary>Zero: crawling with its top down, contracted.</summary>
    ZeroUpsideDownContracted,
    /// <summary>Zero: crawling with its top down, stretching, the gaps between its segments showing.</summary>
    ZeroUpsideDownStretching,
    /// <summary>Zero: crawling with its top down, stretched, its segments parted.</summary>
    ZeroUpsideDownStretched,
    /// <summary>Zero: crawling with its top down, fully stretched, its segments wide apart.</summary>
    ZeroUpsideDownFullyStretched,
    /// <summary>Zero: crawling with its top up, contracted.</summary>
    ZeroUpsideUpContracted,
    /// <summary>Zero: crawling with its top up, stretching, the gaps between its segments showing.</summary>
    ZeroUpsideUpStretching,
    /// <summary>Zero: crawling with its top up, stretched, its segments parted.</summary>
    ZeroUpsideUpStretched,
    /// <summary>Zero: crawling with its top up, fully stretched, its segments wide apart.</summary>
    ZeroUpsideUpFullyStretched,
    /// <summary>Etecoon: standing, looking right.</summary>
    EtecoonLookingRight,
    /// <summary>Etecoon: standing, turning left.</summary>
    EtecoonTurningLeft,
    /// <summary>Etecoon: running left, stretched out mid stride.</summary>
    EtecoonRunningLeftStretched,
    /// <summary>Etecoon: running left, bunched up.</summary>
    EtecoonRunningLeftBunched,
    /// <summary>Etecoon: running left, pushing off.</summary>
    EtecoonRunningLeftPushingOff,
    /// <summary>Etecoon: leaping left, stretched flat.</summary>
    EtecoonLeapingLeft,
    /// <summary>Etecoon: curled up spinning left, upright.</summary>
    EtecoonSpinningLeftTurned0,
    /// <summary>Etecoon: curled up spinning left, turned a quarter.</summary>
    EtecoonSpinningLeftTurned90,
    /// <summary>Etecoon: curled up spinning left, upside down.</summary>
    EtecoonSpinningLeftTurned180,
    /// <summary>Etecoon: curled up spinning left, turned three quarters.</summary>
    EtecoonSpinningLeftTurned270,
    /// <summary>Etecoon: crouched to hop, facing left.</summary>
    EtecoonHopCrouchFacingLeft,
    /// <summary>Etecoon: hopping facing left, arms raising.</summary>
    EtecoonHopArmsRaisingFacingLeft,
    /// <summary>Etecoon: hopping facing left, arms up.</summary>
    EtecoonHopArmsUpFacingLeft,
    /// <summary>Etecoon: hopping facing left, arms flung wide.</summary>
    EtecoonHopArmsFlungFacingLeft,
    /// <summary>Etecoon: crouched against a wall, facing left.</summary>
    EtecoonClingingFacingLeft,
    /// <summary>Etecoon: standing, looking left.</summary>
    EtecoonLookingLeft,
    /// <summary>Etecoon: standing, turning right.</summary>
    EtecoonTurningRight,
    /// <summary>Etecoon: running right, stretched out mid stride.</summary>
    EtecoonRunningRightStretched,
    /// <summary>Etecoon: running right, bunched up.</summary>
    EtecoonRunningRightBunched,
    /// <summary>Etecoon: running right, pushing off.</summary>
    EtecoonRunningRightPushingOff,
    /// <summary>Etecoon: leaping right, stretched flat.</summary>
    EtecoonLeapingRight,
    /// <summary>Etecoon: curled up spinning right, upright.</summary>
    EtecoonSpinningRightTurned0,
    /// <summary>Etecoon: curled up spinning right, turned a quarter.</summary>
    EtecoonSpinningRightTurned90,
    /// <summary>Etecoon: curled up spinning right, upside down.</summary>
    EtecoonSpinningRightTurned180,
    /// <summary>Etecoon: curled up spinning right, turned three quarters.</summary>
    EtecoonSpinningRightTurned270,
    /// <summary>Etecoon: crouched to hop, facing right.</summary>
    EtecoonHopCrouchFacingRight,
    /// <summary>Etecoon: hopping facing right, arms raising.</summary>
    EtecoonHopArmsRaisingFacingRight,
    /// <summary>Etecoon: hopping facing right, arms up.</summary>
    EtecoonHopArmsUpFacingRight,
    /// <summary>Etecoon: hopping facing right, arms flung wide.</summary>
    EtecoonHopArmsFlungFacingRight,
    /// <summary>Etecoon: crouched against a wall, facing right.</summary>
    EtecoonClingingFacingRight,
    /// <summary>Etecoon: standing facing the viewer, arms at its sides.</summary>
    EtecoonFacingViewer,
    /// <summary>Dachora: running left, its far leg stretched behind.</summary>
    DachoraRunningLeftFarLegBack,
    /// <summary>Dachora: running left, its far leg swinging past.</summary>
    DachoraRunningLeftFarLegPassing,
    /// <summary>Dachora: running left, its far leg reaching forward.</summary>
    DachoraRunningLeftFarLegReaching,
    /// <summary>Dachora: running left, its near leg stretched behind.</summary>
    DachoraRunningLeftNearLegBack,
    /// <summary>Dachora: running left, its near leg swinging past.</summary>
    DachoraRunningLeftNearLegPassing,
    /// <summary>Dachora: running left, its near leg reaching forward.</summary>
    DachoraRunningLeftNearLegReaching,
    /// <summary>Dachora: standing facing left, head up.</summary>
    DachoraStandingFacingLeftHeadUp,
    /// <summary>Dachora: standing facing left, neck stretched forward.</summary>
    DachoraStandingFacingLeftNeckStretched,
    /// <summary>Dachora: standing facing left, head lowered.</summary>
    DachoraStandingFacingLeftHeadLowered,
    /// <summary>Dachora: standing facing left, head down to the ground.</summary>
    DachoraStandingFacingLeftHeadToGround,
    /// <summary>Dachora: standing facing left, head raising.</summary>
    DachoraStandingFacingLeftHeadRaising,
    /// <summary>Dachora: standing facing left, eye half shut.</summary>
    DachoraStandingFacingLeftEyeHalfShut,
    /// <summary>Dachora: standing facing left, eye shut.</summary>
    DachoraStandingFacingLeftEyeShut,
    /// <summary>Dachora: stretched straight up, facing left.</summary>
    DachoraStretchedUpFacingLeft,
    /// <summary>Dachora: running right, its far leg stretched behind.</summary>
    DachoraRunningRightFarLegBack,
    /// <summary>Dachora: running right, its far leg swinging past.</summary>
    DachoraRunningRightFarLegPassing,
    /// <summary>Dachora: running right, its far leg reaching forward.</summary>
    DachoraRunningRightFarLegReaching,
    /// <summary>Dachora: running right, its near leg stretched behind.</summary>
    DachoraRunningRightNearLegBack,
    /// <summary>Dachora: running right, its near leg swinging past.</summary>
    DachoraRunningRightNearLegPassing,
    /// <summary>Dachora: running right, its near leg reaching forward.</summary>
    DachoraRunningRightNearLegReaching,
    /// <summary>Dachora: standing facing right, head up.</summary>
    DachoraStandingFacingRightHeadUp,
    /// <summary>Dachora: standing facing right, neck stretched forward.</summary>
    DachoraStandingFacingRightNeckStretched,
    /// <summary>Dachora: standing facing right, head lowered.</summary>
    DachoraStandingFacingRightHeadLowered,
    /// <summary>Dachora: standing facing right, head down to the ground.</summary>
    DachoraStandingFacingRightHeadToGround,
    /// <summary>Dachora: standing facing right, head raising.</summary>
    DachoraStandingFacingRightHeadRaising,
    /// <summary>Dachora: standing facing right, eye half shut.</summary>
    DachoraStandingFacingRightEyeHalfShut,
    /// <summary>Dachora: standing facing right, eye shut.</summary>
    DachoraStandingFacingRightEyeShut,
    /// <summary>Dachora: crouched to charge, facing right.</summary>
    DachoraCrouchedFacingRight,
    /// <summary>Dachora: stretched straight up, facing right.</summary>
    DachoraStretchedUpFacingRight,
    /// <summary>The escaping Etecoons: running left, stretched out mid stride.</summary>
    EscapeEtecoonRunningLeftStretched,
    /// <summary>The escaping Etecoons: running left, bunched up.</summary>
    EscapeEtecoonRunningLeftBunched,
    /// <summary>The escaping Etecoons: running left, pushing off.</summary>
    EscapeEtecoonRunningLeftPushingOff,
    /// <summary>The escaping Etecoons: running right, stretched out mid stride.</summary>
    EscapeEtecoonRunningRightStretched,
    /// <summary>The escaping Etecoons: running right, bunched up.</summary>
    EscapeEtecoonRunningRightBunched,
    /// <summary>The escaping Etecoons: running right, pushing off.</summary>
    EscapeEtecoonRunningRightPushingOff,
    /// <summary>The escaping Etecoons: standing, looking right.</summary>
    EscapeEtecoonLookingRight,
    /// <summary>The escaping Etecoons: standing, turning left.</summary>
    EscapeEtecoonTurningLeft,
    /// <summary>The escaping Etecoons: standing, looking left.</summary>
    EscapeEtecoonLookingLeft,
    /// <summary>The escaping Etecoons: standing, turning right.</summary>
    EscapeEtecoonTurningRight,
    /// <summary>The escaping Dachora: running left, its far leg stretched behind.</summary>
    EscapeDachoraRunningLeftFarLegBack,
    /// <summary>The escaping Dachora: running left, its far leg swinging past.</summary>
    EscapeDachoraRunningLeftFarLegPassing,
    /// <summary>The escaping Dachora: running left, its far leg reaching forward.</summary>
    EscapeDachoraRunningLeftFarLegReaching,
    /// <summary>The escaping Dachora: running left, its near leg stretched behind.</summary>
    EscapeDachoraRunningLeftNearLegBack,
    /// <summary>The escaping Dachora: running left, its near leg swinging past.</summary>
    EscapeDachoraRunningLeftNearLegPassing,
    /// <summary>The escaping Dachora: running left, its near leg reaching forward.</summary>
    EscapeDachoraRunningLeftNearLegReaching,
    /// <summary>The escaping Dachora: running right, its far leg stretched behind.</summary>
    EscapeDachoraRunningRightFarLegBack,
    /// <summary>The escaping Dachora: running right, its far leg swinging past.</summary>
    EscapeDachoraRunningRightFarLegPassing,
    /// <summary>The escaping Dachora: running right, its far leg reaching forward.</summary>
    EscapeDachoraRunningRightFarLegReaching,
    /// <summary>The escaping Dachora: running right, its near leg stretched behind.</summary>
    EscapeDachoraRunningRightNearLegBack,
    /// <summary>The escaping Dachora: running right, its near leg swinging past.</summary>
    EscapeDachoraRunningRightNearLegPassing,
    /// <summary>The escaping Dachora: running right, its near leg reaching forward.</summary>
    EscapeDachoraRunningRightNearLegReaching,
    /// <summary>Hibashi: a lone ember.</summary>
    HibashiEmber,
    /// <summary>Hibashi: a column of flame 19 pixels wide and 19 high.</summary>
    HibashiFlame19By19,
    /// <summary>Hibashi: a column of flame 23 pixels wide and 26 high.</summary>
    HibashiFlame23By26,
    /// <summary>Hibashi: a column of flame 33 pixels wide and 35 high.</summary>
    HibashiFlame33By35,
    /// <summary>Hibashi: a column of flame 37 pixels wide and 41 high.</summary>
    HibashiFlame37By41,
    /// <summary>Hibashi: a column of flame 19 pixels wide and 43 high.</summary>
    HibashiFlame19By43,
    /// <summary>Hibashi: a column of flame 23 pixels wide and 45 high.</summary>
    HibashiFlame23By45,
    /// <summary>Hibashi: a column of flame 33 pixels wide and 47 high.</summary>
    HibashiFlame33By47,
    /// <summary>Hibashi: a column of flame 37 pixels wide and 55 high.</summary>
    HibashiFlame37By55,
    /// <summary>Hibashi: a column of flame 19 pixels wide and 63 high.</summary>
    HibashiFlame19By63,
    /// <summary>Hibashi: a column of flame 23 pixels wide and 71 high.</summary>
    HibashiFlame23By71,
    /// <summary>Hibashi: a column of flame 33 pixels wide and 80 high.</summary>
    HibashiFlame33By80,
    /// <summary>Hibashi: a column of flame 41 pixels wide and 87 high.</summary>
    HibashiFlame41By87,
    /// <summary>Hibashi: a column of flame 45 pixels wide and 95 high.</summary>
    HibashiFlame45By95,
    /// <summary>Hibashi: a column of flame 17 pixels wide and 99 high.</summary>
    HibashiFlame17By99,
    /// <summary>Hibashi: a column of flame 19 pixels wide and 109 high.</summary>
    HibashiFlame19By109,
    /// <summary>Hibashi: a column of flame 23 pixels wide and 112 high.</summary>
    HibashiFlame23By112,
    /// <summary>Hibashi: a column of flame 33 pixels wide and 109 high.</summary>
    HibashiFlame33By109,
    /// <summary>Hibashi: a column of flame 37 pixels wide and 111 high.</summary>
    HibashiFlame37By111,
    /// <summary>Hibashi: the top of its flame dispersing, 17 pixels wide and 44 high.</summary>
    HibashiDispersing17By44,
    /// <summary>Hibashi: the top of its flame dispersing, 17 pixels wide and 32 high.</summary>
    HibashiDispersing17By32,
    /// <summary>Hibashi: the top of its flame dispersing, 17 pixels wide and 25 high.</summary>
    HibashiDispersing17By25,
    /// <summary>Hibashi: the top of its flame dispersing, 14 pixels wide and 16 high.</summary>
    HibashiDispersing14By16,
    /// <summary>The zebetite: standing full height, intact.</summary>
    ZebetiteTallIntact,
    /// <summary>The zebetite: standing full height, a fifth drained.</summary>
    ZebetiteTallDrained,
    /// <summary>The zebetite: standing full height, two fifths drained.</summary>
    ZebetiteTallHalfDrained,
    /// <summary>The zebetite: standing full height, three fifths drained.</summary>
    ZebetiteTallMostlyDrained,
    /// <summary>The zebetite: standing full height, nearly drained.</summary>
    ZebetiteTallNearlyDrained,
    /// <summary>The zebetite: standing half height, intact.</summary>
    ZebetiteShortIntact,
    /// <summary>The zebetite: standing half height, a fifth drained.</summary>
    ZebetiteShortDrained,
    /// <summary>The zebetite: standing half height, two fifths drained.</summary>
    ZebetiteShortHalfDrained,
    /// <summary>The zebetite: standing half height, three fifths drained.</summary>
    ZebetiteShortMostlyDrained,
    /// <summary>The zebetite: standing half height, nearly drained.</summary>
    ZebetiteShortNearlyDrained,
    /// <summary>The Wrecked Ship ghost: floating, its wisps at rest.</summary>
    WreckedShipGhostWispsAtRest,
    /// <summary>The Wrecked Ship ghost: floating, its wisps curling.</summary>
    WreckedShipGhostWispsCurling,
    /// <summary>The Wrecked Ship ghost: floating, its wisps stretched.</summary>
    WreckedShipGhostWispsStretched,
    /// <summary>Powamp: its spiny body, tail curled left.</summary>
    PowampBodyTailLeft,
    /// <summary>Powamp: its spiny body, tail hanging straight down.</summary>
    PowampBodyTailDown,
    /// <summary>Powamp: its spiny body, tail curled right.</summary>
    PowampBodyTailRight,
    /// <summary>Powamp: its balloon, small.</summary>
    PowampBalloonSmall,
    /// <summary>Powamp: its balloon, half inflated.</summary>
    PowampBalloonHalfInflated,
    /// <summary>Powamp: its balloon, fully inflated.</summary>
    PowampBalloonInflated,
    /// <summary>The spark: a large cross-shaped spark among scattered motes.</summary>
    SparkLargeCross,
    /// <summary>The spark: a large slanted spark among scattered motes.</summary>
    SparkLargeSlant,
    /// <summary>The spark: a cross-shaped spark, its motes scattering below.</summary>
    SparkLargeCrossScattering,
    /// <summary>The spark: a slanted spark, its motes scattering below.</summary>
    SparkLargeSlantScattering,
    /// <summary>The spark: a small cross-shaped spark.</summary>
    SparkSmallCross,
    /// <summary>The spark: a small slanted spark trailing motes.</summary>
    SparkSmallSlantTrailing,
    /// <summary>The spark: a small cross-shaped spark trailing motes.</summary>
    SparkSmallCrossTrailing,
    /// <summary>The spark: a small slanted spark trailing motes further.</summary>
    SparkSmallSlantTrailingFar,
    /// <summary>The baby Metroid: its nuclei bright.</summary>
    BabyMetroidNucleiBright,
    /// <summary>The baby Metroid: its nuclei glowing.</summary>
    BabyMetroidNucleiGlowing,
    /// <summary>The baby Metroid: its nuclei dark.</summary>
    BabyMetroidNucleiDark,
    /// <summary>Kraid: a belly spike budding, its points clustered.</summary>
    KraidLintBudding,
    /// <summary>Kraid: a belly spike grown, its points swept back.</summary>
    KraidLintGrown,
    /// <summary>The nuclear waffle: its swirling body, upright.</summary>
    NuclearWaffleBodyTurned0,
    /// <summary>The nuclear waffle: its swirling body, turned an eighth.</summary>
    NuclearWaffleBodyTurned45,
    /// <summary>The nuclear waffle: its swirling body, turned a quarter.</summary>
    NuclearWaffleBodyTurned90,
    /// <summary>The nuclear waffle: its swirling body, turned three eighths.</summary>
    NuclearWaffleBodyTurned135,
    /// <summary>The nuclear waffle: its swirling body, turned half way.</summary>
    NuclearWaffleBodyTurned180,
    /// <summary>The nuclear waffle: its swirling body, turned five eighths.</summary>
    NuclearWaffleBodyTurned225,
    /// <summary>The nuclear waffle: its swirling body, turned three quarters.</summary>
    NuclearWaffleBodyTurned270,
    /// <summary>The nuclear waffle: its swirling body, turned seven eighths.</summary>
    NuclearWaffleBodyTurned315,
    /// <summary>Kzan: its spiked block.</summary>
    KzanSpikedBlock,
    /// <summary>Polyp: a molten rock.</summary>
    PolypRock,
    /// <summary>Ridley: breakup legs left.</summary>
    RidleyBreakupLegsLeft,
    /// <summary>Ridley: breakup legs right.</summary>
    RidleyBreakupLegsRight,
    /// <summary>Ridley: breakup open head and neck, facing left.</summary>
    RidleyBreakupOpenHeadAndNeckLeft,
    /// <summary>Ridley: breakup open head and neck, facing right.</summary>
    RidleyBreakupOpenHeadAndNeckRight,
    /// <summary>Ridley: breakup torso, facing left.</summary>
    RidleyBreakupTorsoLeft,
    /// <summary>Ridley: breakup torso, facing right.</summary>
    RidleyBreakupTorsoRight,
    /// <summary>Ridley: breakup claw left.</summary>
    RidleyBreakupClawLeft,
    /// <summary>Ridley: breakup claw right.</summary>
    RidleyBreakupClawRight,
    /// <summary>The walking space pirate: flinch left.</summary>
    WalkingPirateFlinchLeft,
    /// <summary>The walking space pirate: flinch right.</summary>
    WalkingPirateFlinchRight,
    /// <summary>The walking space pirate: walking left, its far leg stretched behind.</summary>
    WalkingPirateWalkingLeftFarLegBack,
    /// <summary>The walking space pirate: walking left, its far leg lifting behind.</summary>
    WalkingPirateWalkingLeftFarLegLifting,
    /// <summary>The walking space pirate: walking left, its far leg swinging past its knee.</summary>
    WalkingPirateWalkingLeftFarLegPassing,
    /// <summary>The walking space pirate: walking left, its far leg planting.</summary>
    WalkingPirateWalkingLeftFarLegPlanting,
    /// <summary>The walking space pirate: walking left, its near leg stretched behind.</summary>
    WalkingPirateWalkingLeftNearLegBack,
    /// <summary>The walking space pirate: walking left, its near leg lifting behind.</summary>
    WalkingPirateWalkingLeftNearLegLifting,
    /// <summary>The walking space pirate: walking left, its near leg swinging past its knee.</summary>
    WalkingPirateWalkingLeftNearLegPassing,
    /// <summary>The walking space pirate: walking left, its near leg planting.</summary>
    WalkingPirateWalkingLeftNearLegPlanting,
    /// <summary>The walking space pirate: standing at ease facing left, claws down.</summary>
    WalkingPirateStandingFacingLeft,
    /// <summary>The walking space pirate: facing left, claws lifting.</summary>
    WalkingPirateClawsLiftingFacingLeft,
    /// <summary>The walking space pirate: facing left, claws held level and forward.</summary>
    WalkingPirateClawsForwardFacingLeft,
    /// <summary>The walking space pirate: facing left, claws raised.</summary>
    WalkingPirateClawsRaisedFacingLeft,
    /// <summary>The walking space pirate: facing left, claws raised high, leaning in.</summary>
    WalkingPirateClawsRaisedLeaningFacingLeft,
    /// <summary>The walking space pirate: crouched facing left, claws thrust forward to fire.</summary>
    WalkingPirateFiringFacingLeft,
    /// <summary>The walking space pirate: standing facing left, its head turned to the viewer.</summary>
    WalkingPirateLookingAtViewerFacingLeft,
    /// <summary>The walking space pirate: standing at ease facing left, claws down: the picture and hitboxes of WalkingPirateStandingFacingLeft, as a frame of its own.</summary>
    WalkingPirateStandingFacingLeftTwin,
    /// <summary>The walking space pirate: standing facing left, its head turned away from the viewer.</summary>
    WalkingPirateLookingAwayFacingLeft,
    /// <summary>The walking space pirate: standing facing the viewer, arms at its sides.</summary>
    WalkingPirateFacingViewer,
    /// <summary>The walking space pirate: walking right, its far leg stretched behind.</summary>
    WalkingPirateWalkingRightFarLegBack,
    /// <summary>The walking space pirate: walking right, its far leg lifting behind.</summary>
    WalkingPirateWalkingRightFarLegLifting,
    /// <summary>The walking space pirate: walking right, its far leg swinging past its knee.</summary>
    WalkingPirateWalkingRightFarLegPassing,
    /// <summary>The walking space pirate: walking right, its far leg planting.</summary>
    WalkingPirateWalkingRightFarLegPlanting,
    /// <summary>The walking space pirate: walking right, its near leg stretched behind.</summary>
    WalkingPirateWalkingRightNearLegBack,
    /// <summary>The walking space pirate: walking right, its near leg lifting behind.</summary>
    WalkingPirateWalkingRightNearLegLifting,
    /// <summary>The walking space pirate: walking right, its near leg swinging past its knee.</summary>
    WalkingPirateWalkingRightNearLegPassing,
    /// <summary>The walking space pirate: walking right, its near leg planting.</summary>
    WalkingPirateWalkingRightNearLegPlanting,
    /// <summary>The walking space pirate: standing at ease facing right, claws down.</summary>
    WalkingPirateStandingFacingRight,
    /// <summary>The walking space pirate: facing right, claws lifting.</summary>
    WalkingPirateClawsLiftingFacingRight,
    /// <summary>The walking space pirate: facing right, claws held level and forward.</summary>
    WalkingPirateClawsForwardFacingRight,
    /// <summary>The walking space pirate: facing right, claws raised.</summary>
    WalkingPirateClawsRaisedFacingRight,
    /// <summary>The walking space pirate: facing right, claws raised high, leaning in.</summary>
    WalkingPirateClawsRaisedLeaningFacingRight,
    /// <summary>The walking space pirate: crouched facing right, claws thrust forward to fire.</summary>
    WalkingPirateFiringFacingRight,
    /// <summary>The walking space pirate: standing facing right, its head turned to the viewer.</summary>
    WalkingPirateLookingAtViewerFacingRight,
    /// <summary>The walking space pirate: standing at ease facing right, claws down: the picture and hitboxes of WalkingPirateStandingFacingRight, as a frame of its own.</summary>
    WalkingPirateStandingFacingRightTwin,
    /// <summary>The walking space pirate: standing facing right, its head turned away from the viewer.</summary>
    WalkingPirateLookingAwayFacingRight,
    /// <summary>The wall space pirate: clinging to a wall on its right, turned out to face left, claws together.</summary>
    WallPirateOnRightWallClawsTogether,
    /// <summary>The wall space pirate: clinging to a wall on its right, turned out to face left, claws spread to fire.</summary>
    WallPirateOnRightWallClawsSpread,
    /// <summary>The wall space pirate: knees tucked, claws stretched up and left to a wall on its left.</summary>
    WallPirateReachingForLeftWall,
    /// <summary>The wall space pirate: curled up, leaping left.</summary>
    WallPirateCurledLeapingLeft,
    /// <summary>The wall space pirate: climbing a wall on its left, legs hanging.</summary>
    WallPirateOnLeftWallLegsHanging,
    /// <summary>The wall space pirate: climbing a wall on its left, knees bending.</summary>
    WallPirateOnLeftWallKneesBending,
    /// <summary>The wall space pirate: climbing a wall on its left, knees tucked up.</summary>
    WallPirateOnLeftWallKneesTucked,
    /// <summary>The wall space pirate: climbing a wall on its left, legs pushing down.</summary>
    WallPirateOnLeftWallLegsPushing,
    /// <summary>The wall space pirate: climbing a wall on its left, legs stretched down.</summary>
    WallPirateOnLeftWallLegsStretched,
    /// <summary>The wall space pirate: clinging to a wall on its left, turned out to face right, claws together.</summary>
    WallPirateOnLeftWallClawsTogether,
    /// <summary>The wall space pirate: clinging to a wall on its left, turned out to face right, claws spread to fire.</summary>
    WallPirateOnLeftWallClawsSpread,
    /// <summary>The wall space pirate: knees tucked, claws stretched up and right to a wall on its right.</summary>
    WallPirateReachingForRightWall,
    /// <summary>The wall space pirate: curled up, leaping right.</summary>
    WallPirateCurledLeapingRight,
    /// <summary>The wall space pirate: climbing a wall on its right, legs hanging.</summary>
    WallPirateOnRightWallLegsHanging,
    /// <summary>The wall space pirate: climbing a wall on its right, knees bending.</summary>
    WallPirateOnRightWallKneesBending,
    /// <summary>The wall space pirate: climbing a wall on its right, knees tucked up.</summary>
    WallPirateOnRightWallKneesTucked,
    /// <summary>The wall space pirate: climbing a wall on its right, legs pushing down.</summary>
    WallPirateOnRightWallLegsPushing,
    /// <summary>The wall space pirate: climbing a wall on its right, legs stretched down.</summary>
    WallPirateOnRightWallLegsStretched,
    /// <summary>The ninja space pirate: crouched facing left, claws held low and forward.</summary>
    NinjaPirateCrouchFacingLeft,
    /// <summary>The ninja space pirate: facing left, claws level and ready.</summary>
    NinjaPirateClawsReadyFacingLeft,
    /// <summary>The ninja space pirate: facing left, its upper claw drawn back over its shoulder.</summary>
    NinjaPirateClawDrawnBackFacingLeft,
    /// <summary>The ninja space pirate: facing left, its upper claw raised high and forward.</summary>
    NinjaPirateClawRaisedHighFacingLeft,
    /// <summary>The ninja space pirate: stretched tall facing left, one claw raised straight overhead.</summary>
    NinjaPirateClawRaisedOverheadFacingLeft,
    /// <summary>The ninja space pirate: facing left, its upper claw swung forward to throw.</summary>
    NinjaPirateClawSwungForwardFacingLeft,
    /// <summary>The ninja space pirate: facing left, claws level and ready: the picture and hitboxes of NinjaPirateClawsReadyFacingLeft, as a frame of its own.</summary>
    NinjaPirateClawsReadyFacingLeftTwin,
    /// <summary>The ninja space pirate: facing left, claws swept down.</summary>
    NinjaPirateClawsSweptDownFacingLeft,
    /// <summary>The ninja space pirate: standing upright facing left, claws hanging down.</summary>
    NinjaPirateUprightClawsLoweredFacingLeft,
    /// <summary>The ninja space pirate: curled into a spinning ball facing left, upright.</summary>
    NinjaPirateSpinningFacingLeftTurned0,
    /// <summary>The ninja space pirate: curled into a spinning ball facing left, turned an eighth.</summary>
    NinjaPirateSpinningFacingLeftTurned45,
    /// <summary>The ninja space pirate: curled into a spinning ball facing left, turned a quarter.</summary>
    NinjaPirateSpinningFacingLeftTurned90,
    /// <summary>The ninja space pirate: curled into a spinning ball facing left, turned three eighths.</summary>
    NinjaPirateSpinningFacingLeftTurned135,
    /// <summary>The ninja space pirate: curled into a spinning ball facing left, upside down.</summary>
    NinjaPirateSpinningFacingLeftTurned180,
    /// <summary>The ninja space pirate: curled into a spinning ball facing left, turned five eighths.</summary>
    NinjaPirateSpinningFacingLeftTurned225,
    /// <summary>The ninja space pirate: curled into a spinning ball facing left, turned three quarters.</summary>
    NinjaPirateSpinningFacingLeftTurned270,
    /// <summary>The ninja space pirate: curled into a spinning ball facing left, turned seven eighths.</summary>
    NinjaPirateSpinningFacingLeftTurned315,
    /// <summary>The ninja space pirate: in its ready stance facing left, claws forward, feet close.</summary>
    NinjaPirateReadyFacingLeftFeetClose,
    /// <summary>The ninja space pirate: in its ready stance facing left, claws forward, feet apart.</summary>
    NinjaPirateReadyFacingLeftFeetApart,
    /// <summary>The ninja space pirate: in its ready stance facing left, claws forward, feet wide and knees bent.</summary>
    NinjaPirateReadyFacingLeftFeetWide,
    /// <summary>The ninja space pirate: flinch facing left.</summary>
    NinjaPirateFlinchFacingLeft,
    /// <summary>The ninja space pirate: crouched facing left, claws held low and forward, with the hitboxes of its leap: the picture of NinjaPirateCrouchFacingLeft.</summary>
    NinjaPirateLeapCrouchFacingLeft,
    /// <summary>The ninja space pirate: jumping facing left, knees tucked.</summary>
    NinjaPirateTuckedJumpFacingLeft,
    /// <summary>The ninja space pirate: diving down and left, one leg kicked out and one claw raised.</summary>
    NinjaPirateDiveKickFacingLeft,
    /// <summary>The ninja space pirate: running left, its far leg stretched behind.</summary>
    NinjaPirateRunningLeftFarLegBack,
    /// <summary>The ninja space pirate: running left, its far leg lifting behind.</summary>
    NinjaPirateRunningLeftFarLegLifting,
    /// <summary>The ninja space pirate: running left, its far leg swinging past its knee.</summary>
    NinjaPirateRunningLeftFarLegPassing,
    /// <summary>The ninja space pirate: running left, its far leg planting.</summary>
    NinjaPirateRunningLeftFarLegPlanting,
    /// <summary>The ninja space pirate: running left, its near leg stretched behind.</summary>
    NinjaPirateRunningLeftNearLegBack,
    /// <summary>The ninja space pirate: running left, its near leg lifting behind.</summary>
    NinjaPirateRunningLeftNearLegLifting,
    /// <summary>The ninja space pirate: running left, its near leg swinging past its knee.</summary>
    NinjaPirateRunningLeftNearLegPassing,
    /// <summary>The ninja space pirate: running left, its near leg planting.</summary>
    NinjaPirateRunningLeftNearLegPlanting,
    /// <summary>The ninja space pirate: standing facing left, its head turned to the viewer.</summary>
    NinjaPirateLookingAtViewerFacingLeft,
    /// <summary>The ninja space pirate: standing at ease facing left, claws down.</summary>
    NinjaPirateStandingFacingLeft,
    /// <summary>The ninja space pirate: standing facing left, its head turned away from the viewer.</summary>
    NinjaPirateLookingAwayFacingLeft,
    /// <summary>The ninja space pirate: standing facing the viewer, arms at its sides.</summary>
    NinjaPirateFacingViewer,
    /// <summary>The ninja space pirate: standing at ease facing right, claws down.</summary>
    NinjaPirateStandingFacingRight,
    /// <summary>The ninja space pirate: in its ready stance facing right, claws forward, feet close.</summary>
    NinjaPirateReadyFacingRightFeetClose,
    /// <summary>The ninja space pirate: in its ready stance facing right, claws forward, feet apart.</summary>
    NinjaPirateReadyFacingRightFeetApart,
    /// <summary>The ninja space pirate: in its ready stance facing right, claws forward, feet wide and knees bent.</summary>
    NinjaPirateReadyFacingRightFeetWide,
    /// <summary>The ninja space pirate: facing left, one knee raised to kick.</summary>
    NinjaPirateKickChamberedFacingLeft,
    /// <summary>The ninja space pirate: facing left, its kicking leg extending.</summary>
    NinjaPirateKickExtendingFacingLeft,
    /// <summary>The ninja space pirate: facing left, its kicking leg fully extended.</summary>
    NinjaPirateKickExtendedFacingLeft,
    /// <summary>The ninja space pirate: crouched facing right, claws held low and forward.</summary>
    NinjaPirateCrouchFacingRight,
    /// <summary>The ninja space pirate: facing right, claws level and ready.</summary>
    NinjaPirateClawsReadyFacingRight,
    /// <summary>The ninja space pirate: facing right, its upper claw drawn back over its shoulder.</summary>
    NinjaPirateClawDrawnBackFacingRight,
    /// <summary>The ninja space pirate: facing right, its upper claw raised high and forward.</summary>
    NinjaPirateClawRaisedHighFacingRight,
    /// <summary>The ninja space pirate: stretched tall facing right, one claw raised straight overhead.</summary>
    NinjaPirateClawRaisedOverheadFacingRight,
    /// <summary>The ninja space pirate: facing right, its upper claw swung forward to throw.</summary>
    NinjaPirateClawSwungForwardFacingRight,
    /// <summary>The ninja space pirate: facing right, claws swept down.</summary>
    NinjaPirateClawsSweptDownFacingRight,
    /// <summary>The ninja space pirate: standing upright facing right, claws hanging down.</summary>
    NinjaPirateUprightClawsLoweredFacingRight,
    /// <summary>The ninja space pirate: curled into a spinning ball facing right, upright.</summary>
    NinjaPirateSpinningFacingRightTurned0,
    /// <summary>The ninja space pirate: curled into a spinning ball facing right, turned an eighth.</summary>
    NinjaPirateSpinningFacingRightTurned45,
    /// <summary>The ninja space pirate: curled into a spinning ball facing right, turned a quarter.</summary>
    NinjaPirateSpinningFacingRightTurned90,
    /// <summary>The ninja space pirate: curled into a spinning ball facing right, turned three eighths.</summary>
    NinjaPirateSpinningFacingRightTurned135,
    /// <summary>The ninja space pirate: curled into a spinning ball facing right, upside down.</summary>
    NinjaPirateSpinningFacingRightTurned180,
    /// <summary>The ninja space pirate: curled into a spinning ball facing right, turned five eighths.</summary>
    NinjaPirateSpinningFacingRightTurned225,
    /// <summary>The ninja space pirate: curled into a spinning ball facing right, turned three quarters.</summary>
    NinjaPirateSpinningFacingRightTurned270,
    /// <summary>The ninja space pirate: curled into a spinning ball facing right, turned seven eighths.</summary>
    NinjaPirateSpinningFacingRightTurned315,
    /// <summary>The ninja space pirate: flinch facing right.</summary>
    NinjaPirateFlinchFacingRight,
    /// <summary>The ninja space pirate: crouched facing right, claws held low and forward, with the hitboxes of its leap: the picture of NinjaPirateCrouchFacingRight.</summary>
    NinjaPirateLeapCrouchFacingRight,
    /// <summary>The ninja space pirate: jumping facing right, knees tucked.</summary>
    NinjaPirateTuckedJumpFacingRight,
    /// <summary>The ninja space pirate: diving down and right, one leg kicked out and one claw raised.</summary>
    NinjaPirateDiveKickFacingRight,
    /// <summary>The ninja space pirate: running right, its far leg stretched behind.</summary>
    NinjaPirateRunningRightFarLegBack,
    /// <summary>The ninja space pirate: running right, its far leg lifting behind.</summary>
    NinjaPirateRunningRightFarLegLifting,
    /// <summary>The ninja space pirate: running right, its far leg swinging past its knee.</summary>
    NinjaPirateRunningRightFarLegPassing,
    /// <summary>The ninja space pirate: running right, its far leg planting.</summary>
    NinjaPirateRunningRightFarLegPlanting,
    /// <summary>The ninja space pirate: running right, its near leg stretched behind.</summary>
    NinjaPirateRunningRightNearLegBack,
    /// <summary>The ninja space pirate: running right, its near leg lifting behind.</summary>
    NinjaPirateRunningRightNearLegLifting,
    /// <summary>The ninja space pirate: running right, its near leg swinging past its knee.</summary>
    NinjaPirateRunningRightNearLegPassing,
    /// <summary>The ninja space pirate: running right, its near leg planting.</summary>
    NinjaPirateRunningRightNearLegPlanting,
    /// <summary>The ninja space pirate: standing facing right, its head turned to the viewer.</summary>
    NinjaPirateLookingAtViewerFacingRight,
    /// <summary>The ninja space pirate: standing facing right, its head turned away from the viewer.</summary>
    NinjaPirateLookingAwayFacingRight,
    /// <summary>The ninja space pirate: facing right, one knee raised to kick.</summary>
    NinjaPirateKickChamberedFacingRight,
    /// <summary>The ninja space pirate: facing right, its kicking leg extending.</summary>
    NinjaPirateKickExtendingFacingRight,
    /// <summary>The ninja space pirate: facing right, its kicking leg fully extended.</summary>
    NinjaPirateKickExtendedFacingRight,
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
    /// <summary>Draygon: its arms facing left, swept forward.</summary>
    DraygonArmsFacingLeftForward,
    /// <summary>Draygon: its arms facing left, lowering.</summary>
    DraygonArmsFacingLeftLowering,
    /// <summary>Draygon: its arms facing left, down.</summary>
    DraygonArmsFacingLeftDown,
    /// <summary>Draygon: its arms facing left, hanging down, stretched.</summary>
    DraygonArmsFacingLeftDownStretched,
    /// <summary>Draygon: its arms facing left, raising.</summary>
    DraygonArmsFacingLeftRaising,
    /// <summary>Draygon: its arms facing left, reaching forward.</summary>
    DraygonArmsFacingLeftReaching,
    /// <summary>Draygon: its arms facing left, spread wide.</summary>
    DraygonArmsFacingLeftSpreadWide,
    /// <summary>Draygon: its arms facing left, closing.</summary>
    DraygonArmsFacingLeftClosing,
    /// <summary>Draygon: its arms facing left, gathering in.</summary>
    DraygonArmsFacingLeftGathering,
    /// <summary>Draygon: its arms facing left, tucked in.</summary>
    DraygonArmsFacingLeftTucked,
    /// <summary>Draygon: its tail facing left, its tip curled in.</summary>
    DraygonTailFacingLeftTipCurledIn,
    /// <summary>Draygon: its tail facing left, its tip swinging out.</summary>
    DraygonTailFacingLeftTipSwingingOut,
    /// <summary>Draygon: its tail facing left, its tip out.</summary>
    DraygonTailFacingLeftTipOut,
    /// <summary>Draygon: its tail facing left, its tip further out.</summary>
    DraygonTailFacingLeftTipFurtherOut,
    /// <summary>Draygon: its tail facing left, its tip farthest out.</summary>
    DraygonTailFacingLeftTipFarthestOut,
    /// <summary>Draygon: its tail facing left, its tip returning.</summary>
    DraygonTailFacingLeftTipReturning,
    /// <summary>Draygon: its tail facing left, its tip swung back.</summary>
    DraygonTailFacingLeftTipBack,
    /// <summary>Draygon: its tail facing left, cocking to whip.</summary>
    DraygonTailFacingLeftWhipCocking,
    /// <summary>Draygon: its tail facing left, raising to whip.</summary>
    DraygonTailFacingLeftWhipRaising,
    /// <summary>Draygon: its tail facing left, hanging long.</summary>
    DraygonTailFacingLeftWhipHangingLong,
    /// <summary>Draygon: its tail facing left, swinging.</summary>
    DraygonTailFacingLeftWhipSwinging,
    /// <summary>Draygon: its tail facing left, lashed out flat.</summary>
    DraygonTailFacingLeftWhipLashedOut,
    /// <summary>Draygon: its tail facing left, curling back.</summary>
    DraygonTailFacingLeftWhipCurling,
    /// <summary>Draygon: its tail facing left, recoiling.</summary>
    DraygonTailFacingLeftWhipRecoiling,
    /// <summary>Draygon: its arms facing right, swept forward.</summary>
    DraygonArmsFacingRightForward,
    /// <summary>Draygon: its arms facing right, lowering.</summary>
    DraygonArmsFacingRightLowering,
    /// <summary>Draygon: its arms facing right, down.</summary>
    DraygonArmsFacingRightDown,
    /// <summary>Draygon: its arms facing right, hanging down, stretched.</summary>
    DraygonArmsFacingRightDownStretched,
    /// <summary>Draygon: its arms facing right, raising.</summary>
    DraygonArmsFacingRightRaising,
    /// <summary>Draygon: its arms facing right, reaching forward.</summary>
    DraygonArmsFacingRightReaching,
    /// <summary>Draygon: its arms facing right, spread wide.</summary>
    DraygonArmsFacingRightSpreadWide,
    /// <summary>Draygon: its arms facing right, closing.</summary>
    DraygonArmsFacingRightClosing,
    /// <summary>Draygon: its arms facing right, gathering in.</summary>
    DraygonArmsFacingRightGathering,
    /// <summary>Draygon: its arms facing right, tucked in.</summary>
    DraygonArmsFacingRightTucked,
    /// <summary>Draygon: its tail facing right, its tip curled in.</summary>
    DraygonTailFacingRightTipCurledIn,
    /// <summary>Draygon: its tail facing right, its tip swinging out.</summary>
    DraygonTailFacingRightTipSwingingOut,
    /// <summary>Draygon: its tail facing right, its tip out.</summary>
    DraygonTailFacingRightTipOut,
    /// <summary>Draygon: its tail facing right, its tip further out.</summary>
    DraygonTailFacingRightTipFurtherOut,
    /// <summary>Draygon: its tail facing right, its tip farthest out.</summary>
    DraygonTailFacingRightTipFarthestOut,
    /// <summary>Draygon: its tail facing right, its tip returning.</summary>
    DraygonTailFacingRightTipReturning,
    /// <summary>Draygon: its tail facing right, its tip swung back.</summary>
    DraygonTailFacingRightTipBack,
    /// <summary>Draygon: its tail facing right, cocking to whip.</summary>
    DraygonTailFacingRightWhipCocking,
    /// <summary>Draygon: its tail facing right, raising to whip.</summary>
    DraygonTailFacingRightWhipRaising,
    /// <summary>Draygon: its tail facing right, hanging long.</summary>
    DraygonTailFacingRightWhipHangingLong,
    /// <summary>Draygon: its tail facing right, swinging.</summary>
    DraygonTailFacingRightWhipSwinging,
    /// <summary>Draygon: its tail facing right, lashed out flat.</summary>
    DraygonTailFacingRightWhipLashedOut,
    /// <summary>Draygon: its tail facing right, curling back.</summary>
    DraygonTailFacingRightWhipCurling,
    /// <summary>Draygon: its tail facing right, recoiling.</summary>
    DraygonTailFacingRightWhipRecoiling,
    /// <summary>Spore Spawn: hardened to a husk, the picture of SporeSpawnShut, which Samus's touch passes through.</summary>
    SporeSpawnHusk,
    /// <summary>Spore Spawn: its shell shut.</summary>
    SporeSpawnShut,
    /// <summary>Spore Spawn: its shell cracked, parted 8 pixels round its core.</summary>
    SporeSpawnCracked,
    /// <summary>Spore Spawn: its shell ajar, parted 16 pixels round its core.</summary>
    SporeSpawnAjar,
    /// <summary>Spore Spawn: its shell parting, 24 pixels round its core.</summary>
    SporeSpawnParting,
    /// <summary>Spore Spawn: its shell half open, parted 32 pixels round its core.</summary>
    SporeSpawnHalfOpen,
    /// <summary>Spore Spawn: its shell opening wide, parted 40 pixels round its core.</summary>
    SporeSpawnWideOpening,
    /// <summary>Spore Spawn: its shell nearly open, parted 44 pixels round its core.</summary>
    SporeSpawnNearlyOpen,
    /// <summary>Spore Spawn: its shell fully open, parted 48 pixels round its core.</summary>
    SporeSpawnOpen,
    /// <summary>Spore Spawn: its shell fully open, its core glowing.</summary>
    SporeSpawnOpenCoreGlowing,
    /// <summary>Spore Spawn: its shell fully open, its core bright.</summary>
    SporeSpawnOpenCoreBright,
    /// <summary>Ceres steam: up.</summary>
    CeresSteamUp,
    /// <summary>Ceres steam: a jet of steam up, 22 pixels long.</summary>
    CeresSteamUpJet22,
    /// <summary>Ceres steam: a jet of steam up, 31 pixels long.</summary>
    CeresSteamUpJet31,
    /// <summary>Ceres steam: a jet of steam up, 40 pixels long.</summary>
    CeresSteamUpJet40,
    /// <summary>Ceres steam: a cloud of steam drifting off up, 33 pixels long.</summary>
    CeresSteamUpDrifting33,
    /// <summary>Ceres steam: a cloud of steam drifting off up, 27 pixels long.</summary>
    CeresSteamUpDrifting27,
    /// <summary>Ceres steam: a cloud of steam drifting off up, 16 pixels long.</summary>
    CeresSteamUpDrifting16,
    /// <summary>Ceres steam: left.</summary>
    CeresSteamLeft,
    /// <summary>Ceres steam: a jet of steam left, 24 pixels long.</summary>
    CeresSteamLeftJet24,
    /// <summary>Ceres steam: a jet of steam left, 32 pixels long.</summary>
    CeresSteamLeftJet32,
    /// <summary>Ceres steam: a jet of steam left, 40 pixels long.</summary>
    CeresSteamLeftJet40,
    /// <summary>Ceres steam: a cloud of steam drifting off left, 32 pixels long.</summary>
    CeresSteamLeftDrifting32,
    /// <summary>Ceres steam: a cloud of steam drifting off left, 26 pixels long.</summary>
    CeresSteamLeftDrifting26,
    /// <summary>Ceres steam: a cloud of steam drifting off left, 16 pixels long.</summary>
    CeresSteamLeftDrifting16,
    /// <summary>Ceres steam: down.</summary>
    CeresSteamDown,
    /// <summary>Ceres steam: a jet of steam down, 22 pixels long.</summary>
    CeresSteamDownJet22,
    /// <summary>Ceres steam: a jet of steam down, 31 pixels long.</summary>
    CeresSteamDownJet31,
    /// <summary>Ceres steam: a jet of steam down, 39 pixels long.</summary>
    CeresSteamDownJet39,
    /// <summary>Ceres steam: a cloud of steam drifting off down, 31 pixels long.</summary>
    CeresSteamDownDrifting31,
    /// <summary>Ceres steam: a cloud of steam drifting off down, 24 pixels long.</summary>
    CeresSteamDownDrifting24,
    /// <summary>Ceres steam: a cloud of steam drifting off down, 16 pixels long.</summary>
    CeresSteamDownDrifting16,
    /// <summary>Ceres steam: right.</summary>
    CeresSteamRight,
    /// <summary>Ceres steam: a jet of steam right, 24 pixels long.</summary>
    CeresSteamRightJet24,
    /// <summary>Ceres steam: a jet of steam right, 32 pixels long.</summary>
    CeresSteamRightJet32,
    /// <summary>Ceres steam: a jet of steam right, 40 pixels long.</summary>
    CeresSteamRightJet40,
    /// <summary>Ceres steam: a cloud of steam drifting off right, 32 pixels long.</summary>
    CeresSteamRightDrifting32,
    /// <summary>Ceres steam: a cloud of steam drifting off right, 26 pixels long.</summary>
    CeresSteamRightDrifting26,
    /// <summary>Ceres steam: a cloud of steam drifting off right, 16 pixels long.</summary>
    CeresSteamRightDrifting16,
    /// <summary>Oum: its shell facing left, upright.</summary>
    OumFacingLeftTurned0,
    /// <summary>Oum: its shell facing left, a vent opening on its front.</summary>
    OumFacingLeftVentOpening,
    /// <summary>Oum: its shell facing left, a vent open on its front.</summary>
    OumFacingLeftVentOpen,
    /// <summary>Oum: its shell facing left, a jet flaring from its vent.</summary>
    OumFacingLeftJetFlaring,
    /// <summary>Oum: its shell facing left, a short jet from its vent.</summary>
    OumFacingLeftJetShort,
    /// <summary>Oum: its shell facing left, a medium jet from its vent.</summary>
    OumFacingLeftJetMedium,
    /// <summary>Oum: its shell facing left, a long jet from its vent.</summary>
    OumFacingLeftJetLong,
    /// <summary>Oum: its shell facing left, its longest jet from its vent.</summary>
    OumFacingLeftJetLongest,
    /// <summary>Oum: its shell facing left, rolled an eighth over.</summary>
    OumFacingLeftTurned45,
    /// <summary>Oum: its shell facing left, rolled a quarter over.</summary>
    OumFacingLeftTurned90,
    /// <summary>Oum: its shell facing left, rolled three eighths over.</summary>
    OumFacingLeftTurned135,
    /// <summary>Oum: its shell facing left, rolled half way over.</summary>
    OumFacingLeftTurned180,
    /// <summary>Oum: its shell facing left, rolled five eighths over.</summary>
    OumFacingLeftTurned225,
    /// <summary>Oum: its shell facing left, rolled three quarters over.</summary>
    OumFacingLeftTurned270,
    /// <summary>Oum: its shell facing left, rolled seven eighths over.</summary>
    OumFacingLeftTurned315,
    /// <summary>Oum: its shell facing right, upright.</summary>
    OumFacingRightTurned0,
    /// <summary>Oum: its shell facing right, a vent opening on its front.</summary>
    OumFacingRightVentOpening,
    /// <summary>Oum: its shell facing right, a vent open on its front.</summary>
    OumFacingRightVentOpen,
    /// <summary>Oum: its shell facing right, a jet flaring from its vent.</summary>
    OumFacingRightJetFlaring,
    /// <summary>Oum: its shell facing right, a short jet from its vent.</summary>
    OumFacingRightJetShort,
    /// <summary>Oum: its shell facing right, a medium jet from its vent.</summary>
    OumFacingRightJetMedium,
    /// <summary>Oum: its shell facing right, a long jet from its vent.</summary>
    OumFacingRightJetLong,
    /// <summary>Oum: its shell facing right, its longest jet from its vent.</summary>
    OumFacingRightJetLongest,
    /// <summary>Oum: its shell facing right, rolled an eighth over.</summary>
    OumFacingRightTurned45,
    /// <summary>Oum: its shell facing right, rolled a quarter over.</summary>
    OumFacingRightTurned90,
    /// <summary>Oum: its shell facing right, rolled three eighths over.</summary>
    OumFacingRightTurned135,
    /// <summary>Oum: its shell facing right, rolled half way over.</summary>
    OumFacingRightTurned180,
    /// <summary>Oum: its shell facing right, rolled five eighths over.</summary>
    OumFacingRightTurned225,
    /// <summary>Oum: its shell facing right, rolled three quarters over.</summary>
    OumFacingRightTurned270,
    /// <summary>Oum: its shell facing right, rolled seven eighths over.</summary>
    OumFacingRightTurned315,
    /// <summary>Crocomire: its tongue curling down.</summary>
    CrocomireTongueCurlingDown,
    /// <summary>Crocomire: its tongue flat, held higher.</summary>
    CrocomireTongueFlatRaised,
    /// <summary>Crocomire: its tongue curling up.</summary>
    CrocomireTongueCurlingUp,
    /// <summary>Crocomire: its tongue flat.</summary>
    CrocomireTongueFlat,
    /// <summary>Crocomire: its arm alone, bent at the elbow.</summary>
    CrocomireArmAloneBent,
    /// <summary>Crocomire: its arm alone, raised high.</summary>
    CrocomireArmAloneRaised,
    /// <summary>Crocomire: its arm alone, stretched out flat.</summary>
    CrocomireArmAloneOutstretched,
    /// <summary>Crocomire: its arm alone, lowering.</summary>
    CrocomireArmAloneLowering,
    /// <summary>Crocomire: its arm alone, returning, bent upright.</summary>
    CrocomireArmAloneReturning,
    /// <summary>Crocomire: striding, near foot forward, its arm bent at the elbow.</summary>
    CrocomireBodyNearFootForwardArmBent,
    /// <summary>Crocomire: striding, near foot forward, its arm level, forearm upright.</summary>
    CrocomireBodyNearFootForwardArmSquare,
    /// <summary>Crocomire: striding, near foot forward, its arm reaching up and forward.</summary>
    CrocomireBodyNearFootForwardArmReaching,
    /// <summary>Crocomire: striding, near foot forward, its arm sweeping up.</summary>
    CrocomireBodyNearFootForwardArmSweeping,
    /// <summary>Crocomire: striding, near foot forward, its arm lifting.</summary>
    CrocomireBodyNearFootForwardArmLifting,
    /// <summary>Crocomire: striding, near foot forward, its arm upright.</summary>
    CrocomireBodyNearFootForwardArmUpright,
    /// <summary>Crocomire: striding, far foot forward, its arm bent at the elbow.</summary>
    CrocomireBodyFarFootForwardArmBent,
    /// <summary>Crocomire: striding, far foot forward, its arm level, forearm upright.</summary>
    CrocomireBodyFarFootForwardArmSquare,
    /// <summary>Crocomire: striding, far foot forward, its arm reaching up and forward.</summary>
    CrocomireBodyFarFootForwardArmReaching,
    /// <summary>Crocomire: striding, far foot forward, its arm sweeping up.</summary>
    CrocomireBodyFarFootForwardArmSweeping,
    /// <summary>Crocomire: striding, far foot forward, its arm lifting.</summary>
    CrocomireBodyFarFootForwardArmLifting,
    /// <summary>Crocomire: striding, far foot forward, its arm upright.</summary>
    CrocomireBodyFarFootForwardArmUpright,
    /// <summary>Crocomire: its arm upright, its mouth open wide.</summary>
    CrocomireBodyArmUprightMouthOpen,
    /// <summary>Crocomire: its arm upright, its mouth a little less open.</summary>
    CrocomireBodyArmUprightMouthOpenLess,
    /// <summary>Crocomire: its arm upright, its mouth closing.</summary>
    CrocomireBodyArmUprightMouthClosing,
    /// <summary>Crocomire: its arm upright, its mouth half closed.</summary>
    CrocomireBodyArmUprightMouthHalfClosed,
    /// <summary>Crocomire: its arm upright, its mouth mostly closed.</summary>
    CrocomireBodyArmUprightMouthMostlyClosed,
    /// <summary>Crocomire: its arm upright, its mouth nearly shut.</summary>
    CrocomireBodyArmUprightMouthNearlyShut,
    /// <summary>Crocomire: its arm upright, its mouth shut, its mouth pieces hidden.</summary>
    CrocomireBodyArmUprightMouthShut,
    /// <summary>Crocomire: its arm bent at the elbow as it swings to throw, feet together.</summary>
    CrocomireBodySwingingArmBent,
    /// <summary>Crocomire: its arm raised high as it swings to throw, feet together.</summary>
    CrocomireBodySwingingArmRaised,
    /// <summary>Crocomire: its arm stretched out flat as it swings to throw, feet together.</summary>
    CrocomireBodySwingingArmOutstretched,
    /// <summary>Crocomire: its arm lowering as it swings to throw, feet together.</summary>
    CrocomireBodySwingingArmLowering,
    /// <summary>Crocomire: its arm returning, bent upright as it swings to throw, feet together.</summary>
    CrocomireBodySwingingArmReturning,
    /// <summary>Crocomire: striding, far foot forward, its arm upright: the picture of CrocomireBodyFarFootForwardArmUpright, as a frame of its own.</summary>
    CrocomireBodyFarFootForwardArmUprightTwinB,
    /// <summary>Crocomire: striding, far foot forward, its arm upright: the picture of CrocomireBodyFarFootForwardArmUpright, as a frame of its own.</summary>
    CrocomireBodyFarFootForwardArmUprightTwinC,
    /// <summary>Crocomire: striding, far foot forward, its arm upright: the picture of CrocomireBodyFarFootForwardArmUpright, as a frame of its own.</summary>
    CrocomireBodyFarFootForwardArmUprightTwinD,
    /// <summary>Crocomire: striding, near foot forward, its arm bent at the elbow: the picture of CrocomireBodyNearFootForwardArmBent, as a frame of its own.</summary>
    CrocomireBodyNearFootForwardArmBentTwin,
    /// <summary>Crocomire: striding: the picture of CrocomireBodyNearFootForwardArmSweeping, as a frame of its own.</summary>
    CrocomireBodyNearFootForwardArmSweepingTwin,
    /// <summary>Crocomire: striding: the picture of CrocomireBodyNearFootForwardArmLifting, as a frame of its own.</summary>
    CrocomireBodyNearFootForwardArmLiftingTwin,
    /// <summary>Crocomire: striding: the picture of CrocomireBodyNearFootForwardArmUpright, as a frame of its own.</summary>
    CrocomireBodyNearFootForwardArmUprightTwin,
    /// <summary>Crocomire: striding: the picture of CrocomireBodyFarFootForwardArmBent, as a frame of its own.</summary>
    CrocomireBodyFarFootForwardArmBentTwin,
    /// <summary>Crocomire: striding: the picture of CrocomireBodyFarFootForwardArmSquare, as a frame of its own.</summary>
    CrocomireBodyFarFootForwardArmSquareTwin,
    /// <summary>Crocomire: striding: the picture of CrocomireBodyFarFootForwardArmReaching, as a frame of its own.</summary>
    CrocomireBodyFarFootForwardArmReachingTwin,
    /// <summary>Crocomire: striding: the picture of CrocomireBodyFarFootForwardArmSweeping, as a frame of its own.</summary>
    CrocomireBodyFarFootForwardArmSweepingTwin,
    /// <summary>Crocomire: striding: the picture of CrocomireBodyFarFootForwardArmLifting, as a frame of its own.</summary>
    CrocomireBodyFarFootForwardArmLiftingTwin,
    /// <summary>Crocomire: striding: the picture of CrocomireBodyFarFootForwardArmUpright, as a frame of its own.</summary>
    CrocomireBodyFarFootForwardArmUprightTwin,
    /// <summary>Crocomire: its arm bent at the elbow as it lunges, feet apart.</summary>
    CrocomireBodyLungingArmBent,
    /// <summary>Crocomire: its arm raised high as it lunges, feet apart.</summary>
    CrocomireBodyLungingArmRaised,
    /// <summary>Crocomire: its arm stretched out flat as it lunges, feet apart.</summary>
    CrocomireBodyLungingArmOutstretched,
    /// <summary>Crocomire: its arm lowering as it lunges, feet apart.</summary>
    CrocomireBodyLungingArmLowering,
    /// <summary>Crocomire: its arm returning, bent upright as it lunges, feet apart.</summary>
    CrocomireBodyLungingArmReturning,
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
    /// <summary>Torizo: facing left, half risen.</summary>
    TorizoFacingLeftHalfRisen,
    /// <summary>Kraid: its arm raised on a diagonal, claw closed.</summary>
    KraidArmRaisedClawClosed,
    /// <summary>Kraid: its arm raised on a diagonal, claw open.</summary>
    KraidArmRaisedClawOpen,
    /// <summary>Kraid: its arm lowering, claw hooked.</summary>
    KraidArmLoweringClawHooked,
    /// <summary>Kraid: its arm held level, claw hooked.</summary>
    KraidArmLevelClawHooked,
    /// <summary>Kraid: its arm held level, claw hanging down.</summary>
    KraidArmLevelClawHangingDown,
    /// <summary>Kraid: its arm held level, claw curling under.</summary>
    KraidArmLevelClawCurlingUnder,
    /// <summary>Kraid: its arm held level, claw tucked.</summary>
    KraidArmLevelClawTucked,
    /// <summary>Kraid: its arm held level, claw tucked tight.</summary>
    KraidArmLevelClawTuckedTight,
    /// <summary>Kraid: its arm held level, claw folded.</summary>
    KraidArmLevelClawFolded,
    /// <summary>Kraid: its arm held level, claw folded flat.</summary>
    KraidArmLevelClawFoldedFlat,
    /// <summary>Kraid: its arm drooping on a diagonal.</summary>
    KraidArmDroopingDiagonal,
    /// <summary>Kraid: its arm stretched out flat.</summary>
    KraidArmOutstretched,
    /// <summary>Torizo: facing left, crouched.</summary>
    TorizoFacingLeftCrouched,
    /// <summary>Torizo: facing left, rising from its seat, knees deeply bent.</summary>
    TorizoFacingLeftRisingFromSeat,
    /// <summary>Torizo: facing left seated.</summary>
    TorizoFacingLeftSeated,
    /// <summary>Torizo: facing left, nearly upright.</summary>
    TorizoFacingLeftNearlyUpright,
    /// <summary>Torizo: facing left, upright, stepping out.</summary>
    TorizoFacingLeftUprightStepping,
    /// <summary>Torizo: facing left, upright, feet planted wide.</summary>
    TorizoFacingLeftUprightWideStance,
    /// <summary>Torizo: walking facing left, its right leg lifting.</summary>
    TorizoFacingLeftWalkingRightLegLifting,
    /// <summary>Torizo: walking facing left, its right leg swinging forward.</summary>
    TorizoFacingLeftWalkingRightLegSwinging,
    /// <summary>Torizo: walking facing left, its right leg reaching out.</summary>
    TorizoFacingLeftWalkingRightLegReaching,
    /// <summary>Torizo: walking facing left, its right leg planting.</summary>
    TorizoFacingLeftWalkingRightLegPlanting,
    /// <summary>Torizo: walking facing left, its right leg planted.</summary>
    TorizoFacingLeftWalkingRightLegPlanted,
    /// <summary>Torizo: walking facing left, its left leg lifting.</summary>
    TorizoFacingLeftWalkingLeftLegLifting,
    /// <summary>Torizo: walking facing left, its left leg swinging forward.</summary>
    TorizoFacingLeftWalkingLeftLegSwinging,
    /// <summary>Torizo: walking facing left, its left leg reaching out.</summary>
    TorizoFacingLeftWalkingLeftLegReaching,
    /// <summary>Torizo: walking facing left, its left leg planting.</summary>
    TorizoFacingLeftWalkingLeftLegPlanting,
    /// <summary>Torizo: walking facing left, its left leg planted.</summary>
    TorizoFacingLeftWalkingLeftLegPlanted,
    /// <summary>Torizo: standing facing the viewer.</summary>
    TorizoFacingViewer,
    /// <summary>Torizo: walking facing right, its left leg lifting.</summary>
    TorizoFacingRightWalkingLeftLegLifting,
    /// <summary>Torizo: walking facing right, its left leg swinging forward.</summary>
    TorizoFacingRightWalkingLeftLegSwinging,
    /// <summary>Torizo: walking facing right, its left leg reaching out.</summary>
    TorizoFacingRightWalkingLeftLegReaching,
    /// <summary>Torizo: walking facing right, its left leg planting.</summary>
    TorizoFacingRightWalkingLeftLegPlanting,
    /// <summary>Torizo: walking facing right, its left leg planted.</summary>
    TorizoFacingRightWalkingLeftLegPlanted,
    /// <summary>Torizo: walking facing right, its right leg lifting.</summary>
    TorizoFacingRightWalkingRightLegLifting,
    /// <summary>Torizo: walking facing right, its right leg swinging forward.</summary>
    TorizoFacingRightWalkingRightLegSwinging,
    /// <summary>Torizo: walking facing right, its right leg reaching out.</summary>
    TorizoFacingRightWalkingRightLegReaching,
    /// <summary>Torizo: walking facing right, its right leg planting.</summary>
    TorizoFacingRightWalkingRightLegPlanting,
    /// <summary>Torizo: walking facing right, its right leg planted.</summary>
    TorizoFacingRightWalkingRightLegPlanted,
    /// <summary>Torizo: facing right, crouched to jump.</summary>
    TorizoFacingRightJumpCrouch,
    /// <summary>Torizo: facing right, springing up.</summary>
    TorizoFacingRightSpringing,
    /// <summary>Torizo: facing right, leaping, legs tucked.</summary>
    TorizoFacingRightLeaping,
    /// <summary>Torizo: facing right, right foot forward, spewing orbs, head lowered.</summary>
    TorizoFacingRightRightFootForwardSpewingHeadLowered,
    /// <summary>Torizo: facing right, right foot forward, spewing orbs, head raised, its eye glowing.</summary>
    TorizoFacingRightRightFootForwardSpewingHeadRaised,
    /// <summary>Torizo: facing right, right foot forward, spewing orbs, mouth opening.</summary>
    TorizoFacingRightRightFootForwardSpewingMouthOpening,
    /// <summary>Torizo: facing right, right foot forward, spewing orbs, an arm thrust forward.</summary>
    TorizoFacingRightRightFootForwardSpewingArmThrust,
    /// <summary>Torizo: facing right, right foot forward, spewing orbs, an arm raised.</summary>
    TorizoFacingRightRightFootForwardSpewingArmRaised,
    /// <summary>Torizo: facing right, right foot forward, spewing orbs, an arm drawn back.</summary>
    TorizoFacingRightRightFootForwardSpewingArmDrawnBack,
    /// <summary>Torizo: facing right, left foot forward, spewing orbs, head lowered.</summary>
    TorizoFacingRightLeftFootForwardSpewingHeadLowered,
    /// <summary>Torizo: facing right, left foot forward, its near arm down.</summary>
    TorizoFacingRightLeftFootForwardNearArmDown,
    /// <summary>Torizo: facing right, left foot forward, its near arm lifting.</summary>
    TorizoFacingRightLeftFootForwardNearArmLifting,
    /// <summary>Torizo: facing right, left foot forward, its near arm level.</summary>
    TorizoFacingRightLeftFootForwardNearArmLevel,
    /// <summary>Torizo: facing right, left foot forward, its near arm raised.</summary>
    TorizoFacingRightLeftFootForwardNearArmRaised,
    /// <summary>Torizo: facing right, left foot forward, its near arm straight overhead.</summary>
    TorizoFacingRightLeftFootForwardNearArmOverhead,
    /// <summary>Torizo: facing right, left foot forward, its far arm down.</summary>
    TorizoFacingRightLeftFootForwardFarArmDown,
    /// <summary>Torizo: facing right, left foot forward, its far arm lifting.</summary>
    TorizoFacingRightLeftFootForwardFarArmLifting,
    /// <summary>Torizo: facing right, left foot forward, its far arm level.</summary>
    TorizoFacingRightLeftFootForwardFarArmLevel,
    /// <summary>Torizo: facing right, left foot forward, its far arm raised.</summary>
    TorizoFacingRightLeftFootForwardFarArmRaised,
    /// <summary>Torizo: facing right, left foot forward, its far arm straight overhead.</summary>
    TorizoFacingRightLeftFootForwardFarArmOverhead,
    /// <summary>Torizo: facing right, right foot forward, its near arm down.</summary>
    TorizoFacingRightRightFootForwardNearArmDown,
    /// <summary>Torizo: facing right, right foot forward, its near arm lifting.</summary>
    TorizoFacingRightRightFootForwardNearArmLifting,
    /// <summary>Torizo: facing right, right foot forward, its near arm level.</summary>
    TorizoFacingRightRightFootForwardNearArmLevel,
    /// <summary>Torizo: facing right, right foot forward, its near arm raised.</summary>
    TorizoFacingRightRightFootForwardNearArmRaised,
    /// <summary>Torizo: facing right, right foot forward, its near arm straight overhead.</summary>
    TorizoFacingRightRightFootForwardNearArmOverhead,
    /// <summary>Torizo: facing right, right foot forward, its far arm down.</summary>
    TorizoFacingRightRightFootForwardFarArmDown,
    /// <summary>Torizo: facing right, right foot forward, its far arm lifting.</summary>
    TorizoFacingRightRightFootForwardFarArmLifting,
    /// <summary>Torizo: facing right, right foot forward, its far arm level.</summary>
    TorizoFacingRightRightFootForwardFarArmLevel,
    /// <summary>Torizo: facing right, right foot forward, its far arm raised.</summary>
    TorizoFacingRightRightFootForwardFarArmRaised,
    /// <summary>Torizo: facing right, right foot forward, its far arm straight overhead.</summary>
    TorizoFacingRightRightFootForwardFarArmOverhead,
    /// <summary>Torizo: facing left, springing up, as it leaps and as it falls.</summary>
    TorizoFacingLeftSpringing,
    /// <summary>Torizo: facing right, left foot forward, spewing orbs, head raised, its eye glowing.</summary>
    TorizoFacingRightLeftFootForwardSpewingHeadRaised,
    /// <summary>Torizo: facing right, left foot forward, spewing orbs, mouth opening.</summary>
    TorizoFacingRightLeftFootForwardSpewingMouthOpening,
    /// <summary>Torizo: facing right, left foot forward, spewing orbs, an arm thrust forward.</summary>
    TorizoFacingRightLeftFootForwardSpewingArmThrust,
    /// <summary>Torizo: facing right, left foot forward, spewing orbs, an arm raised.</summary>
    TorizoFacingRightLeftFootForwardSpewingArmRaised,
    /// <summary>Torizo: facing right, left foot forward, spewing orbs, an arm drawn back.</summary>
    TorizoFacingRightLeftFootForwardSpewingArmDrawnBack,
    /// <summary>Torizo: facing left, crouched to jump.</summary>
    TorizoFacingLeftJumpCrouch,
    /// <summary>Torizo: facing left, leaping, legs tucked.</summary>
    TorizoFacingLeftLeaping,
    /// <summary>Torizo: facing left, right foot forward, spewing orbs, head lowered.</summary>
    TorizoFacingLeftRightFootForwardSpewingHeadLowered,
    /// <summary>Torizo: facing left, right foot forward, spewing orbs, head raised, its eye glowing.</summary>
    TorizoFacingLeftRightFootForwardSpewingHeadRaised,
    /// <summary>Torizo: facing left, right foot forward, spewing orbs, mouth opening.</summary>
    TorizoFacingLeftRightFootForwardSpewingMouthOpening,
    /// <summary>Torizo: facing left, right foot forward, spewing orbs, an arm thrust forward.</summary>
    TorizoFacingLeftRightFootForwardSpewingArmThrust,
    /// <summary>Torizo: facing left, right foot forward, spewing orbs, an arm raised.</summary>
    TorizoFacingLeftRightFootForwardSpewingArmRaised,
    /// <summary>Torizo: facing left, right foot forward, spewing orbs, an arm drawn back.</summary>
    TorizoFacingLeftRightFootForwardSpewingArmDrawnBack,
    /// <summary>Torizo: facing left, left foot forward, spewing orbs, head lowered.</summary>
    TorizoFacingLeftLeftFootForwardSpewingHeadLowered,
    /// <summary>Torizo: facing left, left foot forward, spewing orbs, head raised, its eye glowing.</summary>
    TorizoFacingLeftLeftFootForwardSpewingHeadRaised,
    /// <summary>Torizo: facing left, left foot forward, spewing orbs, mouth opening.</summary>
    TorizoFacingLeftLeftFootForwardSpewingMouthOpening,
    /// <summary>Torizo: facing left, left foot forward, spewing orbs, an arm thrust forward.</summary>
    TorizoFacingLeftLeftFootForwardSpewingArmThrust,
    /// <summary>Torizo: facing left, left foot forward, spewing orbs, an arm raised.</summary>
    TorizoFacingLeftLeftFootForwardSpewingArmRaised,
    /// <summary>Torizo: facing left, left foot forward, spewing orbs, an arm drawn back.</summary>
    TorizoFacingLeftLeftFootForwardSpewingArmDrawnBack,
    /// <summary>Torizo: facing left, right foot forward, its near arm down.</summary>
    TorizoFacingLeftRightFootForwardNearArmDown,
    /// <summary>Torizo: facing left, right foot forward, its near arm lifting.</summary>
    TorizoFacingLeftRightFootForwardNearArmLifting,
    /// <summary>Torizo: facing left, right foot forward, its near arm level.</summary>
    TorizoFacingLeftRightFootForwardNearArmLevel,
    /// <summary>Torizo: facing left, right foot forward, its near arm raised.</summary>
    TorizoFacingLeftRightFootForwardNearArmRaised,
    /// <summary>Torizo: facing left, right foot forward, its near arm straight overhead.</summary>
    TorizoFacingLeftRightFootForwardNearArmOverhead,
    /// <summary>Torizo: facing left, right foot forward, its far arm down.</summary>
    TorizoFacingLeftRightFootForwardFarArmDown,
    /// <summary>Torizo: facing left, right foot forward, its far arm lifting.</summary>
    TorizoFacingLeftRightFootForwardFarArmLifting,
    /// <summary>Torizo: facing left, right foot forward, its far arm level.</summary>
    TorizoFacingLeftRightFootForwardFarArmLevel,
    /// <summary>Torizo: facing left, right foot forward, its far arm raised.</summary>
    TorizoFacingLeftRightFootForwardFarArmRaised,
    /// <summary>Torizo: facing left, right foot forward, its far arm straight overhead.</summary>
    TorizoFacingLeftRightFootForwardFarArmOverhead,
    /// <summary>Torizo: facing left, left foot forward, its near arm down.</summary>
    TorizoFacingLeftLeftFootForwardNearArmDown,
    /// <summary>Torizo: facing left, left foot forward, its near arm lifting.</summary>
    TorizoFacingLeftLeftFootForwardNearArmLifting,
    /// <summary>Torizo: facing left, left foot forward, its near arm level.</summary>
    TorizoFacingLeftLeftFootForwardNearArmLevel,
    /// <summary>Torizo: facing left, left foot forward, its near arm raised.</summary>
    TorizoFacingLeftLeftFootForwardNearArmRaised,
    /// <summary>Torizo: facing left, left foot forward, its near arm straight overhead.</summary>
    TorizoFacingLeftLeftFootForwardNearArmOverhead,
    /// <summary>Torizo: facing left, left foot forward, its far arm down.</summary>
    TorizoFacingLeftLeftFootForwardFarArmDown,
    /// <summary>Torizo: facing left, left foot forward, its far arm lifting.</summary>
    TorizoFacingLeftLeftFootForwardFarArmLifting,
    /// <summary>Torizo: facing left, left foot forward, its far arm level.</summary>
    TorizoFacingLeftLeftFootForwardFarArmLevel,
    /// <summary>Torizo: facing left, left foot forward, its far arm raised.</summary>
    TorizoFacingLeftLeftFootForwardFarArmRaised,
    /// <summary>Torizo: facing left, left foot forward, its far arm straight overhead.</summary>
    TorizoFacingLeftLeftFootForwardFarArmOverhead,
    /// <summary>Torizo: facing right seated.</summary>
    TorizoFacingRightSeated,
    /// <summary>Torizo: facing right, rising from its seat, knees deeply bent.</summary>
    TorizoFacingRightRisingFromSeat,
    /// <summary>Torizo: facing right, crouched.</summary>
    TorizoFacingRightCrouched,
    /// <summary>Torizo: facing right, half risen.</summary>
    TorizoFacingRightHalfRisen,
    /// <summary>Torizo: facing right, nearly upright.</summary>
    TorizoFacingRightNearlyUpright,
    /// <summary>Torizo: facing right, upright, stepping out.</summary>
    TorizoFacingRightUprightStepping,
    /// <summary>Torizo: facing right, upright, feet planted wide.</summary>
    TorizoFacingRightUprightWideStance,
    /// <summary>Mother Brain: body standing.</summary>
    MotherBrainBodyStanding,
    /// <summary>Mother Brain: its body walking, its near leg lifting.</summary>
    MotherBrainBodyWalkingNearLegLifting,
    /// <summary>Mother Brain: its body walking, its near leg reaching forward.</summary>
    MotherBrainBodyWalkingNearLegReaching,
    /// <summary>Mother Brain: its body walking, its near leg planting.</summary>
    MotherBrainBodyWalkingNearLegPlanting,
    /// <summary>Mother Brain: its body walking, legs apart.</summary>
    MotherBrainBodyWalkingLegsApart,
    /// <summary>Mother Brain: its body walking, its far leg lifting.</summary>
    MotherBrainBodyWalkingFarLegLifting,
    /// <summary>Mother Brain: its body walking, its far leg swinging past.</summary>
    MotherBrainBodyWalkingFarLegPassing,
    /// <summary>Mother Brain: its body walking, its far leg planting.</summary>
    MotherBrainBodyWalkingFarLegPlanting,
    /// <summary>Mother Brain: its body walking, legs together.</summary>
    MotherBrainBodyWalkingLegsTogether,
    /// <summary>Mother Brain: body crouched.</summary>
    MotherBrainBodyCrouched,
    /// <summary>Mother Brain: body uncrouching.</summary>
    MotherBrainBodyUncrouching,
    /// <summary>Mother Brain: body leaning down.</summary>
    MotherBrainBodyLeaningDown,
    /// <summary>
    /// Mother Brain: her brain's hit area (#2220), which her head shows from its first animation update on and her body
    /// until its first animation; it has no picture, and what meets it meets her brain's rectangle.
    /// </summary>
    MotherBrainBrainHitArea,
    /// <summary>Mother Brain: her body standing as her hand beam begins, her hand curling.</summary>
    MotherBrainBodyHandCurling,
    /// <summary>Mother Brain: her body with her hand beam's arm drawn back, its fist closed.</summary>
    MotherBrainBodyArmDrawnBack,
    /// <summary>Mother Brain: her body with her hand beam's arm flung out high, its hand open.</summary>
    MotherBrainBodyArmFlungOut,
    /// <summary>Mother Brain: her body as her hand beam's arm returns: the standing picture, its arm's hitbox a pixel further out.</summary>
    MotherBrainBodyArmReturning,
    /// <summary>Phantoon: invulnerable body.</summary>
    PhantoonInvulnerableBody,
    /// <summary>Phantoon: full hitbox body.</summary>
    PhantoonFullHitboxBody,
    /// <summary>Phantoon: eye hitbox body.</summary>
    PhantoonEyeHitboxBody,
    /// <summary>Phantoon: eye closed.</summary>
    PhantoonEyeClosed,
    /// <summary>Phantoon: its eye half open.</summary>
    PhantoonEyeHalfOpen,
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
    /// <summary>Phantoon: its tentacles drawn in.</summary>
    PhantoonTentaclesNarrow,
    /// <summary>Phantoon: its tentacles spreading.</summary>
    PhantoonTentaclesSpreading,
    /// <summary>Phantoon: its tentacles spread.</summary>
    PhantoonTentaclesSpread,
    /// <summary>Phantoon: its mouth open.</summary>
    PhantoonMouthOpen,
    /// <summary>Phantoon: its mouth half open.</summary>
    PhantoonMouthHalfOpen,
    /// <summary>Phantoon: its mouth narrowed.</summary>
    PhantoonMouthNarrow,
    /// <summary>Draygon: a limp arm facing left, its claw raised.</summary>
    DraygonLimpArmFacingLeftClawRaised,
    /// <summary>Draygon: a limp arm facing left, its claw dropping.</summary>
    DraygonLimpArmFacingLeftClawDropping,
    /// <summary>Draygon: a limp arm facing left, its claw dropped.</summary>
    DraygonLimpArmFacingLeftClawDropped,
    /// <summary>Draygon: a limp arm facing left, its claw curled.</summary>
    DraygonLimpArmFacingLeftClawCurled,
    /// <summary>Draygon: its body facing left, its jaw shut.</summary>
    DraygonBodyFacingLeftJawShut,
    /// <summary>Draygon: its body facing left, its jaw parting.</summary>
    DraygonBodyFacingLeftJawParting,
    /// <summary>Draygon: its body facing left, its jaw open.</summary>
    DraygonBodyFacingLeftJawOpen,
    /// <summary>Draygon: its body facing left, its jaw wide open.</summary>
    DraygonBodyFacingLeftJawWide,
    /// <summary>Draygon: its eye facing left, shut.</summary>
    DraygonEyeFacingLeftShut,
    /// <summary>Draygon: its eye facing left, opening.</summary>
    DraygonEyeFacingLeftOpening,
    /// <summary>Draygon: its eye facing left, half open.</summary>
    DraygonEyeFacingLeftHalfOpen,
    /// <summary>Draygon: its eye facing left, wide open.</summary>
    DraygonEyeFacingLeftWide,
    /// <summary>Draygon: its eye facing left, open.</summary>
    DraygonEyeFacingLeftOpen,
    /// <summary>Draygon: its eye facing left, open and bloodshot.</summary>
    DraygonEyeFacingLeftBloodshot,
    /// <summary>Draygon: its eye facing left, narrowing.</summary>
    DraygonEyeFacingLeftNarrowing,
    /// <summary>Draygon: its eye facing left, closing.</summary>
    DraygonEyeFacingLeftClosing,
    /// <summary>Draygon: its body facing left, its jaw shut: the picture of DraygonBodyFacingLeftJawShut, as a frame of its own.</summary>
    DraygonBodyFacingLeftJawShutTwin,
    /// <summary>Draygon: a limp arm facing right, its claw raised.</summary>
    DraygonLimpArmFacingRightClawRaised,
    /// <summary>Draygon: a limp arm facing right, its claw dropping.</summary>
    DraygonLimpArmFacingRightClawDropping,
    /// <summary>Draygon: a limp arm facing right, its claw dropped.</summary>
    DraygonLimpArmFacingRightClawDropped,
    /// <summary>Draygon: a limp arm facing right, its claw curled.</summary>
    DraygonLimpArmFacingRightClawCurled,
    /// <summary>Draygon: its body facing right, its jaw shut.</summary>
    DraygonBodyFacingRightJawShut,
    /// <summary>Draygon: its body facing right, its jaw parting.</summary>
    DraygonBodyFacingRightJawParting,
    /// <summary>Draygon: its body facing right, its jaw open.</summary>
    DraygonBodyFacingRightJawOpen,
    /// <summary>Draygon: its body facing right, its jaw wide open.</summary>
    DraygonBodyFacingRightJawWide,
    /// <summary>Draygon: its eye facing right, shut.</summary>
    DraygonEyeFacingRightShut,
    /// <summary>Draygon: its eye facing right, opening.</summary>
    DraygonEyeFacingRightOpening,
    /// <summary>Draygon: its eye facing right, half open.</summary>
    DraygonEyeFacingRightHalfOpen,
    /// <summary>Draygon: its eye facing right, wide open.</summary>
    DraygonEyeFacingRightWide,
    /// <summary>Draygon: its eye facing right, open.</summary>
    DraygonEyeFacingRightOpen,
    /// <summary>Draygon: its eye facing right, open and bloodshot.</summary>
    DraygonEyeFacingRightBloodshot,
    /// <summary>Draygon: its eye facing right, narrowing.</summary>
    DraygonEyeFacingRightNarrowing,
    /// <summary>Draygon: its eye facing right, closing.</summary>
    DraygonEyeFacingRightClosing,
    /// <summary>Draygon: its body facing right, its jaw shut: the picture of DraygonBodyFacingRightJawShut, as a frame of its own.</summary>
    DraygonBodyFacingRightJawShutTwin,
    /// <summary>Crocomire: its skeleton starting to topple.</summary>
    CrocomireBodySkeletonToppling,
    /// <summary>Crocomire: its skeleton leaning as it topples.</summary>
    CrocomireBodySkeletonLeaning,
    /// <summary>Crocomire: its skeleton tipping over.</summary>
    CrocomireBodySkeletonTipping,
    /// <summary>Crocomire: its skeleton collapsing, 158 pixels wide and 134 high.</summary>
    CrocomireBodySkeletonCollapsing158By134,
    /// <summary>Crocomire: its skeleton collapsing, 141 pixels wide and 132 high.</summary>
    CrocomireBodySkeletonCollapsing141By132,
    /// <summary>Crocomire: its skeleton collapsing, 142 pixels wide and 127 high.</summary>
    CrocomireBodySkeletonCollapsing142By127,
    /// <summary>Crocomire: its skeleton collapsing, 143 pixels wide and 130 high.</summary>
    CrocomireBodySkeletonCollapsing143By130,
    /// <summary>Crocomire: its skeleton collapsing, 143 pixels wide and 131 high.</summary>
    CrocomireBodySkeletonCollapsing143By131,
    /// <summary>Crocomire: its skeleton collapsing, 160 pixels wide and 130 high.</summary>
    CrocomireBodySkeletonCollapsing160By130,
    /// <summary>Crocomire: its skeleton collapsing, 159 pixels wide and 122 high.</summary>
    CrocomireBodySkeletonCollapsing159By122,
    /// <summary>Crocomire: its skeleton collapsing, 158 pixels wide and 118 high.</summary>
    CrocomireBodySkeletonCollapsing158By118,
    /// <summary>Crocomire: its skeleton collapsing, 158 pixels wide and 108 high.</summary>
    CrocomireBodySkeletonCollapsing158By108,
    /// <summary>Crocomire: its skeleton collapsing, 158 pixels wide and 109 high.</summary>
    CrocomireBodySkeletonCollapsing158By109,
    /// <summary>Crocomire: its skeleton collapsing, 177 pixels wide and 87 high.</summary>
    CrocomireBodySkeletonCollapsing177By87,
    /// <summary>Crocomire: its skeleton collapsing, 174 pixels wide and 82 high.</summary>
    CrocomireBodySkeletonCollapsing174By82,
    /// <summary>Crocomire: its skeleton collapsing, 185 pixels wide and 79 high.</summary>
    CrocomireBodySkeletonCollapsing185By79,
    /// <summary>Crocomire: its skeleton collapsing, 189 pixels wide and 77 high.</summary>
    CrocomireBodySkeletonCollapsing189By77,
    /// <summary>Crocomire: its skeleton collapsing, 173 pixels wide and 77 high.</summary>
    CrocomireBodySkeletonCollapsing173By77,
    /// <summary>Crocomire: its skeleton collapsing, 153 pixels wide and 84 high.</summary>
    CrocomireBodySkeletonCollapsing153By84,
    /// <summary>Crocomire: its skeleton collapsing, 120 pixels wide and 71 high.</summary>
    CrocomireBodySkeletonCollapsing120By71,
    /// <summary>Crocomire: its skeleton collapsing, 120 pixels wide and 68 high.</summary>
    CrocomireBodySkeletonCollapsing120By68,
    /// <summary>Crocomire: body skeleton stable.</summary>
    CrocomireBodySkeletonStable,
    /// <summary>Crocomire: body dead.</summary>
    CrocomireBodyDead,
    /// <summary>Crocomire: bubbles rising from its skeleton, 6 pixels wide and 7 high, risen 0 pixels.</summary>
    CrocomireBubbles6By7Risen0,
    /// <summary>Crocomire: bubbles rising from its skeleton, 12 pixels wide and 12 high, risen 6 pixels.</summary>
    CrocomireBubbles12By12Risen6,
    /// <summary>Crocomire: bubbles rising from its skeleton, 14 pixels wide and 13 high, risen 8 pixels.</summary>
    CrocomireBubbles14By13Risen8,
    /// <summary>Crocomire: bubbles rising from its skeleton, 12 pixels wide and 14 high, risen 11 pixels.</summary>
    CrocomireBubbles12By14Risen11,
    /// <summary>Crocomire: bubbles rising from its skeleton, 12 pixels wide and 14 high, risen 14 pixels.</summary>
    CrocomireBubbles12By14Risen14,
    /// <summary>Crocomire: bubbles rising from its skeleton, 10 pixels wide and 13 high, risen 15 pixels.</summary>
    CrocomireBubbles10By13Risen15,
    /// <summary>Crocomire: bubbles rising from its skeleton, 10 pixels wide and 12 high, risen 16 pixels.</summary>
    CrocomireBubbles10By12Risen16,
    /// <summary>Crocomire: bubbles rising from its skeleton, 8 pixels wide and 10 high, risen 16 pixels.</summary>
    CrocomireBubbles8By10Risen16,
    /// <summary>Crocomire: bubbles rising from its skeleton, 4 pixels wide and 6 high, risen 14 pixels.</summary>
    CrocomireBubbles4By6Risen14,
    /// <summary>Crocomire: bubbles rising from its skeleton, 4 pixels wide and 6 high, drifted off: the picture of CrocomireBubbles4By6Risen14, drawn 32 pixels right and 32 pixels lower.</summary>
    CrocomireBubbles4By6Adrift,
    /// <summary>Kraid: its foot planted, its leg angled back.</summary>
    KraidFootPlanted,
    /// <summary>Kraid: its foot lifting, its leg upright.</summary>
    KraidFootLifting,
    /// <summary>Kraid: its foot raised, its leg straight up.</summary>
    KraidFootRaised,
    /// <summary>Kraid: its foot swinging forward, its leg leaning in.</summary>
    KraidFootSwinging,
    /// <summary>Kraid: its foot reaching forward.</summary>
    KraidFootReaching,
    /// <summary>
    /// The empty frame every enemy family shares: an enemy showing it draws nothing, and one that collides by its frames'
    /// hitboxes collides at the single point at its origin.
    /// </summary>
    Empty,
}
