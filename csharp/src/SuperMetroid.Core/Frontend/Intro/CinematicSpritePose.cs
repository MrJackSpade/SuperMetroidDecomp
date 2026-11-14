namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// A pose a cinematic sprite animation shows (#1747); each cinematic sprite family's artwork is keyed by the poses it
/// draws, and its sheets name their frames by them. Its values are arbitrary.
/// </summary>
public enum CinematicSpritePose
{
    /// <summary>Mother brain frame 0.</summary>
    MotherBrainFrame0,
    /// <summary>Mother brain frame 1.</summary>
    MotherBrainFrame1,
    /// <summary>Mother brain frame 2.</summary>
    MotherBrainFrame2,
    /// <summary>Egg intact.</summary>
    EggIntact,
    /// <summary>Egg crack 1.</summary>
    EggCrack1,
    /// <summary>Egg crack 2.</summary>
    EggCrack2,
    /// <summary>Egg crack 3.</summary>
    EggCrack3,
    /// <summary>Egg crack 4.</summary>
    EggCrack4,
    /// <summary>Egg crack 5.</summary>
    EggCrack5,
    /// <summary>Egg crack 6.</summary>
    EggCrack6,
    /// <summary>Egg crack 7.</summary>
    EggCrack7,
    /// <summary>Egg hatched.</summary>
    EggHatched,
    /// <summary>Egg remnant 1.</summary>
    EggRemnant1,
    /// <summary>Egg remnant 2.</summary>
    EggRemnant2,
    /// <summary>Egg remnant 3.</summary>
    EggRemnant3,
    /// <summary>Egg remnant 4.</summary>
    EggRemnant4,
    /// <summary>Egg remnant 5.</summary>
    EggRemnant5,
    /// <summary>Egg remnant 6.</summary>
    EggRemnant6,
    /// <summary>Egg remnant 7.</summary>
    EggRemnant7,
    /// <summary>Delivered baby 1.</summary>
    DeliveredBaby1,
    /// <summary>Delivered baby 2.</summary>
    DeliveredBaby2,
    /// <summary>Delivered baby 3.</summary>
    DeliveredBaby3,
    /// <summary>Examined baby 1.</summary>
    ExaminedBaby1,
    /// <summary>Examined baby 2.</summary>
    ExaminedBaby2,
    /// <summary>Examined baby 3.</summary>
    ExaminedBaby3,
    /// <summary>Caret visible.</summary>
    CaretVisible,
    /// <summary>The pose that draws nothing.</summary>
    Blank,
    /// <summary>Next page arrow 1.</summary>
    NextPageArrow1,
    /// <summary>Next page arrow 2.</summary>
    NextPageArrow2,
    /// <summary>Next page arrow 3.</summary>
    NextPageArrow3,
    /// <summary>Confused baby 1.</summary>
    ConfusedBaby1,
    /// <summary>Confused baby 2.</summary>
    ConfusedBaby2,
    /// <summary>Confused baby 3.</summary>
    ConfusedBaby3,
    /// <summary>Station under attack large asteroid.</summary>
    StationUnderAttackLargeAsteroid,
    /// <summary>Station under attack.</summary>
    StationUnderAttack,
    /// <summary>Small asteroid.</summary>
    SmallAsteroid,
    /// <summary>Vortex even.</summary>
    VortexEven,
    /// <summary>Vortex odd.</summary>
    VortexOdd,
    /// <summary>Planet zebes.</summary>
    PlanetZebes,
    /// <summary>Planet zebes title.</summary>
    PlanetZebesTitle,
    /// <summary>Zebes's Japanese title.</summary>
    ZebesJapaneseTitle,
    /// <summary>Small explosion 0.</summary>
    SmallExplosion0,
    /// <summary>Small explosion 1.</summary>
    SmallExplosion1,
    /// <summary>Small explosion 2.</summary>
    SmallExplosion2,
    /// <summary>Small explosion 3.</summary>
    SmallExplosion3,
    /// <summary>Small explosion 4.</summary>
    SmallExplosion4,
    /// <summary>Small explosion 5.</summary>
    SmallExplosion5,
    /// <summary>Large blast 0.</summary>
    LargeBlast0,
    /// <summary>Large blast 1.</summary>
    LargeBlast1,
    /// <summary>Large blast 2.</summary>
    LargeBlast2,
    /// <summary>Large blast 3.</summary>
    LargeBlast3,
    /// <summary>Egg fragment 0.</summary>
    EggFragment0,
    /// <summary>Egg fragment 1.</summary>
    EggFragment1,
    /// <summary>Egg fragment 2.</summary>
    EggFragment2,
    /// <summary>Egg fragment 3.</summary>
    EggFragment3,
    /// <summary>Egg fragment 4.</summary>
    EggFragment4,
    /// <summary>Egg fragment 5.</summary>
    EggFragment5,
    /// <summary>Slime moving.</summary>
    SlimeMoving,
    /// <summary>Slime impact 0.</summary>
    SlimeImpact0,
    /// <summary>Slime impact 1.</summary>
    SlimeImpact1,
    /// <summary>Slime impact 2.</summary>
    SlimeImpact2,
    /// <summary>Slime impact 3.</summary>
    SlimeImpact3,
    /// <summary>Zebes stars upper left.</summary>
    ZebesStarsUpperLeft,
    /// <summary>Zebes stars upper right.</summary>
    ZebesStarsUpperRight,
    /// <summary>Zebes stars lower left.</summary>
    ZebesStarsLowerLeft,
    /// <summary>Zebes stars lower right.</summary>
    ZebesStarsLowerRight,
    /// <summary>Flight stars.</summary>
    FlightStars,
    /// <summary>Big explosion 0.</summary>
    BigExplosion0,
    /// <summary>Big explosion 1.</summary>
    BigExplosion1,
    /// <summary>Big explosion 2.</summary>
    BigExplosion2,
    /// <summary>Big explosion 3.</summary>
    BigExplosion3,
    /// <summary>Big explosion 4.</summary>
    BigExplosion4,
    /// <summary>Big explosion 5.</summary>
    BigExplosion5,
    /// <summary>Rinka 0.</summary>
    Rinka0,
    /// <summary>Rinka 1.</summary>
    Rinka1,
    /// <summary>Rinka 2.</summary>
    Rinka2,
    /// <summary>Station blast 0.</summary>
    StationBlast0,
    /// <summary>Station blast 1.</summary>
    StationBlast1,
    /// <summary>Station blast 2.</summary>
    StationBlast2,
    /// <summary>Station blast 3.</summary>
    StationBlast3,
    /// <summary>Station blast 4.</summary>
    StationBlast4,
    /// <summary>Station blast 5.</summary>
    StationBlast5,
    /// <summary>Large asteroid.</summary>
    LargeAsteroid,
    /// <summary>Planet damage 0.</summary>
    PlanetDamage0,
    /// <summary>Planet damage 1.</summary>
    PlanetDamage1,
    /// <summary>Planet damage 2.</summary>
    PlanetDamage2,
    /// <summary>Planet damage 3.</summary>
    PlanetDamage3,
    /// <summary>Planet flash 0.</summary>
    PlanetFlash0,
    /// <summary>Planet flash 1.</summary>
    PlanetFlash1,
    /// <summary>Planet flash 2.</summary>
    PlanetFlash2,
    /// <summary>Planet flash 3.</summary>
    PlanetFlash3,
    /// <summary>Planet glow 0.</summary>
    PlanetGlow0,
    /// <summary>Planet glow 1.</summary>
    PlanetGlow1,
    /// <summary>Planet glow 2.</summary>
    PlanetGlow2,
    /// <summary>Ending starfield.</summary>
    EndingStarfield,
    /// <summary>Planet lava 0.</summary>
    PlanetLava0,
    /// <summary>Planet lava 1.</summary>
    PlanetLava1,
    /// <summary>Planet silhouette.</summary>
    PlanetSilhouette,
    /// <summary>Planet afterglow.</summary>
    PlanetAfterglow,
    /// <summary>Operation text 00.</summary>
    OperationText00,
    /// <summary>Operation text 01.</summary>
    OperationText01,
    /// <summary>Operation text 02.</summary>
    OperationText02,
    /// <summary>Operation text 03.</summary>
    OperationText03,
    /// <summary>Operation text 04.</summary>
    OperationText04,
    /// <summary>Operation text 05.</summary>
    OperationText05,
    /// <summary>Operation text 06.</summary>
    OperationText06,
    /// <summary>Operation text 07.</summary>
    OperationText07,
    /// <summary>Operation text 08.</summary>
    OperationText08,
    /// <summary>Operation text 09.</summary>
    OperationText09,
    /// <summary>Operation text 10.</summary>
    OperationText10,
    /// <summary>Operation text 11.</summary>
    OperationText11,
    /// <summary>Operation text 12.</summary>
    OperationText12,
    /// <summary>Operation text 13.</summary>
    OperationText13,
    /// <summary>Operation text 14.</summary>
    OperationText14,
    /// <summary>Completed text 00.</summary>
    CompletedText00,
    /// <summary>Completed text 01.</summary>
    CompletedText01,
    /// <summary>Completed text 02.</summary>
    CompletedText02,
    /// <summary>Completed text 03.</summary>
    CompletedText03,
    /// <summary>Completed text 04.</summary>
    CompletedText04,
    /// <summary>Completed text 05.</summary>
    CompletedText05,
    /// <summary>Completed text 06.</summary>
    CompletedText06,
    /// <summary>Completed text 07.</summary>
    CompletedText07,
    /// <summary>Completed text 08.</summary>
    CompletedText08,
    /// <summary>Completed text 09.</summary>
    CompletedText09,
    /// <summary>Completed text 10.</summary>
    CompletedText10,
    /// <summary>Completed text 11.</summary>
    CompletedText11,
    /// <summary>Completed text 12.</summary>
    CompletedText12,
    /// <summary>Completed text 13.</summary>
    CompletedText13,
    /// <summary>Completed text 14.</summary>
    CompletedText14,
    /// <summary>Completed text 15.</summary>
    CompletedText15,
    /// <summary>Completed text 16.</summary>
    CompletedText16,
    /// <summary>Completed text 17.</summary>
    CompletedText17,
    /// <summary>Completed text 18.</summary>
    CompletedText18,
    /// <summary>Completed text 19.</summary>
    CompletedText19,
    /// <summary>Completed text 20.</summary>
    CompletedText20,
    /// <summary>Clear time text 00.</summary>
    ClearTimeText00,
    /// <summary>Clear time text 01.</summary>
    ClearTimeText01,
    /// <summary>Clear time text 02.</summary>
    ClearTimeText02,
    /// <summary>Clear time text 03.</summary>
    ClearTimeText03,
    /// <summary>Clear time text 04.</summary>
    ClearTimeText04,
    /// <summary>Clear time text 05.</summary>
    ClearTimeText05,
    /// <summary>Clear time text 06.</summary>
    ClearTimeText06,
    /// <summary>Clear time text 07.</summary>
    ClearTimeText07,
    /// <summary>Clear time text 08.</summary>
    ClearTimeText08,
    /// <summary>Clear time digit 0.</summary>
    ClearTimeDigit0,
    /// <summary>Clear time digit 1.</summary>
    ClearTimeDigit1,
    /// <summary>Clear time digit 2.</summary>
    ClearTimeDigit2,
    /// <summary>Clear time digit 3.</summary>
    ClearTimeDigit3,
    /// <summary>Clear time digit 4.</summary>
    ClearTimeDigit4,
    /// <summary>Clear time digit 5.</summary>
    ClearTimeDigit5,
    /// <summary>Clear time digit 6.</summary>
    ClearTimeDigit6,
    /// <summary>Clear time digit 7.</summary>
    ClearTimeDigit7,
    /// <summary>Clear time digit 8.</summary>
    ClearTimeDigit8,
    /// <summary>Clear time digit 9.</summary>
    ClearTimeDigit9,
    /// <summary>Clear time colon.</summary>
    ClearTimeColon,
    /// <summary>Escape pod 0.</summary>
    EscapePod0,
    /// <summary>Escape pod 1.</summary>
    EscapePod1,
    /// <summary>Escape pod 2.</summary>
    EscapePod2,
    /// <summary>Escape pod 3.</summary>
    EscapePod3,
    /// <summary>Cloud scene b upper a.</summary>
    CloudSceneBUpperA,
    /// <summary>Cloud scene b upper b.</summary>
    CloudSceneBUpperB,
    /// <summary>Cloud scene b lower a.</summary>
    CloudSceneBLowerA,
    /// <summary>Cloud scene b lower b.</summary>
    CloudSceneBLowerB,
    /// <summary>Cloud scene a right.</summary>
    CloudSceneARight,
    /// <summary>Cloud scene a left.</summary>
    CloudSceneALeft,
    /// <summary>Suitless idle upper.</summary>
    SuitlessIdleUpper,
    /// <summary>Suitless lower.</summary>
    SuitlessLower,
    /// <summary>Suitless hair 1.</summary>
    SuitlessHair1,
    /// <summary>Suitless hair 2.</summary>
    SuitlessHair2,
    /// <summary>Suitless hair 3.</summary>
    SuitlessHair3,
    /// <summary>Suitless hair 4.</summary>
    SuitlessHair4,
    /// <summary>Suitless hair 5.</summary>
    SuitlessHair5,
    /// <summary>Suitless hair 6.</summary>
    SuitlessHair6,
    /// <summary>Suitless hair 7.</summary>
    SuitlessHair7,
    /// <summary>Suitless hair 8.</summary>
    SuitlessHair8,
    /// <summary>Suitless standing.</summary>
    SuitlessStanding,
    /// <summary>Suitless prepare jump.</summary>
    SuitlessPrepareJump,
    /// <summary>Suitless jumping.</summary>
    SuitlessJumping,
    /// <summary>Samus falling.</summary>
    SamusFalling,
    /// <summary>Samus landing.</summary>
    SamusLanding,
    /// <summary>Samus landed.</summary>
    SamusLanded,
    /// <summary>Samus shooting.</summary>
    SamusShooting,
    /// <summary>Suited idle body.</summary>
    SuitedIdleBody,
    /// <summary>Suited helmet head.</summary>
    SuitedHelmetHead,
    /// <summary>Helmetless head 1.</summary>
    HelmetlessHead1,
    /// <summary>Suited headless body.</summary>
    SuitedHeadlessBody,
    /// <summary>Suited arm 1.</summary>
    SuitedArm1,
    /// <summary>Suited arm 2.</summary>
    SuitedArm2,
    /// <summary>Suited arm 3.</summary>
    SuitedArm3,
    /// <summary>Suited arm 4.</summary>
    SuitedArm4,
    /// <summary>Suited arm 5.</summary>
    SuitedArm5,
    /// <summary>Suited arm 6.</summary>
    SuitedArm6,
    /// <summary>Suited arm 7.</summary>
    SuitedArm7,
    /// <summary>Suited arm 8.</summary>
    SuitedArm8,
    /// <summary>Helmetless head 2.</summary>
    HelmetlessHead2,
    /// <summary>Helmetless head 3.</summary>
    HelmetlessHead3,
    /// <summary>Helmetless head 4.</summary>
    HelmetlessHead4,
    /// <summary>Suited prepare jump.</summary>
    SuitedPrepareJump,
    /// <summary>Suited jumping.</summary>
    SuitedJumping,
    /// <summary>Suited helmet jump head 1.</summary>
    SuitedHelmetJumpHead1,
    /// <summary>Suited helmet jump head 2.</summary>
    SuitedHelmetJumpHead2,
    /// <summary>Helmetless jump head.</summary>
    HelmetlessJumpHead,
    /// <summary>Logo s upper.</summary>
    LogoSUpper,
    /// <summary>Logo s lower.</summary>
    LogoSLower,
    /// <summary>Logo circle right 1.</summary>
    LogoCircleRight1,
    /// <summary>Logo circle right 2.</summary>
    LogoCircleRight2,
    /// <summary>Logo circle right 3.</summary>
    LogoCircleRight3,
    /// <summary>Logo circle left 1.</summary>
    LogoCircleLeft1,
    /// <summary>Logo circle left 2.</summary>
    LogoCircleLeft2,
    /// <summary>Logo circle left 3.</summary>
    LogoCircleLeft3,
}
