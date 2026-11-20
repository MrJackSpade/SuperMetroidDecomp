namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// A picture a cinematic sprite animation shows (#1747, #1964), each named for what it shows; each cinematic sprite
/// family's artwork is keyed by the poses it draws, and its sheets name their frames by them. An explosion's poses are the
/// stages it burns through. Its values are arbitrary.
/// </summary>
public enum CinematicSpritePose
{
    /// <summary>Mother Brain in her tank at the low point of her pulse.</summary>
    MotherBrainPulseLow,
    /// <summary>Mother Brain swelling or easing between the low and high points of her pulse.</summary>
    MotherBrainPulseMiddle,
    /// <summary>Mother Brain at the high point of her pulse, swollen and brightest.</summary>
    MotherBrainPulseHigh,
    /// <summary>The Metroid egg, whole and upright.</summary>
    EggIntact,
    /// <summary>The egg rocked a pixel to the left as it shakes.</summary>
    EggRockingLeft,
    /// <summary>The egg rocked a pixel to the right as it shakes.</summary>
    EggRockingRight,
    /// <summary>The upright egg with its first crack, at its upper left.</summary>
    EggCrackingUpperLeft,
    /// <summary>The cracking egg, its cracks now reaching across its top.</summary>
    EggCrackingTop,
    /// <summary>The cracking egg, a crack now opening on its left.</summary>
    EggCrackingLeft,
    /// <summary>The cracking egg, a crack now opening at its upper right.</summary>
    EggCrackingUpperRight,
    /// <summary>The cracking egg, a crack now opening on its right, the last before it bursts.</summary>
    EggCrackingRight,
    /// <summary>The egg bursting open on its right.</summary>
    EggHatched,
    /// <summary>The burst shell, its dome sagging into the base.</summary>
    EggRemnantDomeSagging,
    /// <summary>The burst shell, its dome falling in.</summary>
    EggRemnantDomeFalling,
    /// <summary>The burst shell, the last of its dome crumbling away.</summary>
    EggRemnantDomeCrumbling,
    /// <summary>The shell's base with its ribs bared.</summary>
    EggRemnantRibsBared,
    /// <summary>The shell's ribs shifting as the base settles.</summary>
    EggRemnantRibsShifting,
    /// <summary>The shell's ribs settling.</summary>
    EggRemnantRibsSettling,
    /// <summary>The settled remains of the shell, which stay after the hatching.</summary>
    EggRemnantSettled,
    /// <summary>The delivered Baby Metroid at the low point of its pulse.</summary>
    DeliveredBabyPulseLow,
    /// <summary>The delivered Baby Metroid between the low and high points of its pulse.</summary>
    DeliveredBabyPulseMiddle,
    /// <summary>The delivered Baby Metroid at the high point of its pulse.</summary>
    DeliveredBabyPulseHigh,
    /// <summary>The examined Baby Metroid in its capsule, spread wide.</summary>
    ExaminedBabyWide,
    /// <summary>The examined Baby Metroid drawing in between wide and narrow.</summary>
    ExaminedBabyNarrowing,
    /// <summary>The examined Baby Metroid drawn in narrow.</summary>
    ExaminedBabyNarrow,
    /// <summary>The narration's green typing caret, shown.</summary>
    CaretVisible,
    /// <summary>The next-page arrow at its brightest.</summary>
    NextPageArrowBright,
    /// <summary>The next-page arrow between bright and faint.</summary>
    NextPageArrowFading,
    /// <summary>The next-page arrow at its faintest.</summary>
    NextPageArrowFaint,
    /// <summary>The confused Baby Metroid stretched tall.</summary>
    ConfusedBabyStretched,
    /// <summary>The confused Baby Metroid round, between stretched and squashed.</summary>
    ConfusedBabyRound,
    /// <summary>The confused Baby Metroid squashed wide, its legs spread.</summary>
    ConfusedBabySquashed,
    /// <summary>The large asteroids drifting past Ceres as the station is destroyed.</summary>
    StationUnderAttackLargeAsteroid,
    /// <summary>Ceres station as the gunship sees it in space.</summary>
    StationUnderAttack,
    /// <summary>A field of small asteroids drifting through space.</summary>
    SmallAsteroid,
    /// <summary>The swirling purple vortex in space with bright sparks scattered round it, alternating every update with its faint-sparked drawing.</summary>
    VortexBrightSparks,
    /// <summary>The swirling purple vortex in space with its sparks mostly faded, alternating every update with its bright-sparked drawing.</summary>
    VortexFaintSparks,
    /// <summary>Planet Zebes, as the gunship flies to it after Ceres.</summary>
    PlanetZebes,
    /// <summary>The PLANET ZEBES caption.</summary>
    PlanetZebesTitle,
    /// <summary>Zebes's Japanese title.</summary>
    ZebesJapaneseTitle,
    /// <summary>A small explosion's opening spark.</summary>
    SmallExplosionSpark,
    /// <summary>A small explosion flaring into a small ball.</summary>
    SmallExplosionFlare,
    /// <summary>A small explosion's full fireball.</summary>
    SmallExplosionFireball,
    /// <summary>A small explosion's fireball hollowing from its centre.</summary>
    SmallExplosionHollowing,
    /// <summary>A small explosion burned out to a ring.</summary>
    SmallExplosionRing,
    /// <summary>A small explosion's ring fading, its last picture.</summary>
    SmallExplosionFadingRing,
    /// <summary>A large blast's cluster of fireballs.</summary>
    LargeBlastFireballCluster,
    /// <summary>A large blast's cluster breaking apart.</summary>
    LargeBlastBreakingCluster,
    /// <summary>A large blast scattering into embers.</summary>
    LargeBlastScatteringEmbers,
    /// <summary>A large blast's embers fading, its last picture.</summary>
    LargeBlastFadingEmbers,
    /// <summary>The piece at the upper left of the egg's shell.</summary>
    EggShellUpperLeft,
    /// <summary>The piece at the upper right of the egg's shell.</summary>
    EggShellUpperRight,
    /// <summary>The piece at the left of the egg's shell.</summary>
    EggShellLeft,
    /// <summary>The piece at the centre of the egg's shell.</summary>
    EggShellCentre,
    /// <summary>The piece at the right of the egg's shell.</summary>
    EggShellRight,
    /// <summary>The piece at the bottom of the egg's shell.</summary>
    EggShellLower,
    /// <summary>A drop of slime falling from the Metroid egg.</summary>
    SlimeMoving,
    /// <summary>A slime drop splashing into a crown on the floor.</summary>
    SlimeSplashCrown,
    /// <summary>A slime splash rising to its peak.</summary>
    SlimeSplashPeak,
    /// <summary>A slime splash sinking back.</summary>
    SlimeSplashSinking,
    /// <summary>The flat puddle a slime splash leaves.</summary>
    SlimePuddle,
    /// <summary>The stars of the upper left of the sky around Zebes.</summary>
    ZebesStarsUpperLeft,
    /// <summary>The stars of the upper right of the sky around Zebes.</summary>
    ZebesStarsUpperRight,
    /// <summary>The stars of the lower left of the sky around Zebes.</summary>
    ZebesStarsLowerLeft,
    /// <summary>The stars of the lower right of the sky around Zebes.</summary>
    ZebesStarsLowerRight,
    /// <summary>The starfield the gunship flies through on its way to Ceres.</summary>
    FlightStars,
    /// <summary>A big explosion's opening spark.</summary>
    BigExplosionSpark,
    /// <summary>A big explosion flaring into a ball.</summary>
    BigExplosionFlare,
    /// <summary>A big explosion's full fireball.</summary>
    BigExplosionFireball,
    /// <summary>A big explosion's fireball bursting, a ring opening inside its spikes.</summary>
    BigExplosionBursting,
    /// <summary>A big explosion burned out to a ring of radiating spikes.</summary>
    BigExplosionSpikedRing,
    /// <summary>A big explosion's scattered smoke, its last picture.</summary>
    BigExplosionScatteredSmoke,
    /// <summary>A Rinka at its dimmest.</summary>
    RinkaDim,
    /// <summary>A Rinka glowing, between dim and bright.</summary>
    RinkaGlowing,
    /// <summary>A Rinka at its brightest.</summary>
    RinkaBright,
    /// <summary>A station blast flaring into a ball.</summary>
    StationBlastFlare,
    /// <summary>A station blast's full fireball.</summary>
    StationBlastFireball,
    /// <summary>A station blast's fireball bursting, a ring opening inside its spikes.</summary>
    StationBlastBursting,
    /// <summary>A station blast's widened fireball with a ring inside it.</summary>
    StationBlastRingedFireball,
    /// <summary>A station blast burned out to a thick ring.</summary>
    StationBlastRing,
    /// <summary>A station blast's thin ring fading, its last picture.</summary>
    StationBlastFadingRing,
    /// <summary>The large asteroids drifting past on the gunship's flight to Ceres.</summary>
    LargeAsteroid,
    /// <summary>Zebes rumbling, its lower half in deep shadow.</summary>
    PlanetRumbleDeepShadow,
    /// <summary>Zebes rumbling, its shadow lifting.</summary>
    PlanetRumbleShadow,
    /// <summary>Zebes rumbling, its surface lit.</summary>
    PlanetRumbleLit,
    /// <summary>Zebes rumbling, its surface at its brightest.</summary>
    PlanetRumbleBright,
    /// <summary>Zebes cracking, the first fissures across its surface.</summary>
    PlanetCracking,
    /// <summary>Zebes fracturing, its fissures widening.</summary>
    PlanetFracturing,
    /// <summary>Zebes breaking apart, its lower half falling away.</summary>
    PlanetBreakingApart,
    /// <summary>Zebes crumbling, the last picture before it explodes.</summary>
    PlanetCrumbling,
    /// <summary>The exploding planet's glow with a tight halo.</summary>
    PlanetGlowTightHalo,
    /// <summary>The exploding planet's glow with its halo spreading.</summary>
    PlanetGlowSpreadingHalo,
    /// <summary>The exploding planet's glow with its widest halo.</summary>
    PlanetGlowWideHalo,
    /// <summary>The starfield around Zebes as it explodes.</summary>
    EndingStarfield,
    /// <summary>The planet's molten core, dark at its edge.</summary>
    PlanetLavaDark,
    /// <summary>The planet's molten core, bright throughout.</summary>
    PlanetLavaBright,
    /// <summary>Zebes's dark silhouette ringed by the explosion's radiating rays.</summary>
    PlanetSilhouette,
    /// <summary>The glowing cloud Zebes leaves after it explodes.</summary>
    PlanetAfterglow,
    /// <summary>The clear time's digit zero.</summary>
    ClearTimeDigit0,
    /// <summary>The clear time's digit one.</summary>
    ClearTimeDigit1,
    /// <summary>The clear time's digit two.</summary>
    ClearTimeDigit2,
    /// <summary>The clear time's digit three.</summary>
    ClearTimeDigit3,
    /// <summary>The clear time's digit four.</summary>
    ClearTimeDigit4,
    /// <summary>The clear time's digit five.</summary>
    ClearTimeDigit5,
    /// <summary>The clear time's digit six.</summary>
    ClearTimeDigit6,
    /// <summary>The clear time's digit seven.</summary>
    ClearTimeDigit7,
    /// <summary>The clear time's digit eight.</summary>
    ClearTimeDigit8,
    /// <summary>The clear time's digit nine.</summary>
    ClearTimeDigit9,
    /// <summary>The colon between the clear time's hours and minutes.</summary>
    ClearTimeColon,
    /// <summary>The animals' escape pod with a short trail.</summary>
    EscapePodShortTrail,
    /// <summary>The animals' escape pod with a long trail.</summary>
    EscapePodLongTrail,
    /// <summary>The upper clouds' pattern, above their edge.</summary>
    TopCloudPattern,
    /// <summary>The upper clouds' lower edge.</summary>
    TopCloudEdge,
    /// <summary>The lower clouds' upper edge.</summary>
    BottomCloudEdge,
    /// <summary>The lower clouds' pattern, below their edge.</summary>
    BottomCloudPattern,
    /// <summary>The cloud passing on the right of the side-cloud escape.</summary>
    RightSideCloud,
    /// <summary>The cloud passing on the left of the side-cloud escape.</summary>
    LeftSideCloud,
    /// <summary>Suitless Samus standing at rest, the picture her upper body's actor shows.</summary>
    SuitlessIdleUpper,
    /// <summary>Suitless Samus's legs, which her lower body's actor shows while she lets her hair down.</summary>
    SuitlessLower,
    /// <summary>Suitless Samus, her hair tied in its ponytail and her arms lowered.</summary>
    SuitlessPonytailTied,
    /// <summary>Suitless Samus raising her hands to her ponytail.</summary>
    SuitlessHandsToPonytail,
    /// <summary>Suitless Samus untying her ponytail.</summary>
    SuitlessUntyingPonytail,
    /// <summary>Suitless Samus with her ponytail untied, her hands still at her hair.</summary>
    SuitlessPonytailUntied,
    /// <summary>Suitless Samus's hair falling loose as she lowers her arms.</summary>
    SuitlessHairFallingLoose,
    /// <summary>Suitless Samus's loose hair swinging out.</summary>
    SuitlessHairSwinging,
    /// <summary>Suitless Samus's hair swept back as her arm sweeps down.</summary>
    SuitlessHairSweptBack,
    /// <summary>Suitless Samus's hair flowing, settling after the sweep.</summary>
    SuitlessHairFlowing,
    /// <summary>Suitless Samus standing, before her jump.</summary>
    SuitlessStanding,
    /// <summary>Suitless Samus crouching to jump.</summary>
    SuitlessPrepareJump,
    /// <summary>Suitless Samus leaping, her arms raised.</summary>
    SuitlessJumping,
    /// <summary>Suited Samus dropping in, her arm cannon raised.</summary>
    SamusFalling,
    /// <summary>Suited Samus touching down, her knees bent.</summary>
    SamusLanding,
    /// <summary>Suited Samus standing after her landing.</summary>
    SamusLanded,
    /// <summary>Suited Samus firing her arm cannon toward the viewer.</summary>
    SamusShooting,
    /// <summary>Suited Samus standing still.</summary>
    SuitedIdleBody,
    /// <summary>Suited Samus's helmet, facing forward.</summary>
    SuitedHelmetHead,
    /// <summary>Samus's helmetless head at rest.</summary>
    HelmetlessHeadResting,
    /// <summary>Suited Samus's body without her head, whose helmet is drawn on its own as she takes it off.</summary>
    SuitedHeadlessBody,
    /// <summary>The suited arm lowered at Samus's side.</summary>
    SuitedArmLowered,
    /// <summary>The suited arm swinging up.</summary>
    SuitedArmSwingingUp,
    /// <summary>The suited arm raised, its fist forward.</summary>
    SuitedArmFistForward,
    /// <summary>The suited arm's fist raised and drawing back.</summary>
    SuitedArmFistRaised,
    /// <summary>The suited arm's fist drawing in towards the chest.</summary>
    SuitedArmFistDrawingIn,
    /// <summary>The suited arm's fist at the chest.</summary>
    SuitedArmFistAtChest,
    /// <summary>The suited arm's fist turning at the chest.</summary>
    SuitedArmFistTurning,
    /// <summary>The suited arm's fist turned at the chest, the gesture's held picture.</summary>
    SuitedArmFistTurned,
    /// <summary>Samus's helmetless head starting to smile.</summary>
    HelmetlessHeadSmileStarting,
    /// <summary>Samus's helmetless head, her smile widening.</summary>
    HelmetlessHeadSmileWidening,
    /// <summary>Samus's helmetless head smiling.</summary>
    HelmetlessHeadSmiling,
    /// <summary>Suited Samus crouching to jump.</summary>
    SuitedPrepareJump,
    /// <summary>Suited Samus leaping, her arm raised.</summary>
    SuitedJumping,
    /// <summary>The jumping suited helmet held level, as it leaves the ground.</summary>
    SuitedHelmetJumpLevel,
    /// <summary>The jumping suited helmet tilted in flight.</summary>
    SuitedHelmetJumpTilted,
    /// <summary>Samus's helmetless head as she jumps.</summary>
    HelmetlessJumpHead,
    /// <summary>The upper half of the logo's S.</summary>
    LogoSUpper,
    /// <summary>The lower half of the logo's S.</summary>
    LogoSLower,
    /// <summary>The logo's right circle drawn as far as its tip.</summary>
    LogoRightCircleTip,
    /// <summary>The logo's right circle drawn as far as its tip and its blade.</summary>
    LogoRightCircleTipAndBlade,
    /// <summary>The logo's whole right circle.</summary>
    LogoRightCircleWhole,
    /// <summary>The logo's left circle drawn as far as its tip.</summary>
    LogoLeftCircleTip,
    /// <summary>The logo's left circle drawn as far as its tip and its blade.</summary>
    LogoLeftCircleTipAndBlade,
    /// <summary>The logo's whole left circle.</summary>
    LogoLeftCircleWhole,
}
