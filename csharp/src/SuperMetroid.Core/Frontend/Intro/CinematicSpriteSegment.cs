namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Every cinematic sprite animation (#641), in authored order.</summary>
internal enum CinematicSpriteSegment
{
    /// <summary>The intro Mother Brain's four-pose loop.</summary>
    IntroMotherBrain,
    /// <summary>Starts the intro's second page and its cross-fade.</summary>
    IntroMotherBrainStartPage2,
    /// <summary>The intro Mother Brain's loop during the second page.</summary>
    IntroMotherBrainPageTwo,
    /// <summary>The Metroid egg at rest.</summary>
    MetroidEgg,
    /// <summary>The Metroid egg shaking and hatching.</summary>
    MetroidEggHatching,
    /// <summary>The Metroid egg's shaking and hatching frames.</summary>
    MetroidEggShaking,
    /// <summary>The hatched egg's remnants settling, until the third page starts.</summary>
    MetroidEggRemnantsSettling,
    /// <summary>The hatched egg.</summary>
    MetroidEggHatched,
    /// <summary>The Baby Metroid being delivered.</summary>
    BabyMetroidBeingDelivered,
    /// <summary>The delivered Baby Metroid's counted loop before the fourth page.</summary>
    BabyMetroidBeingDeliveredLoop,
    /// <summary>The delivered Baby Metroid.</summary>
    BabyMetroidDelivered,
    /// <summary>The Baby Metroid being examined.</summary>
    BabyMetroidBeingExamined,
    /// <summary>The examined Baby Metroid's counted loop before the fifth page.</summary>
    BabyMetroidBeingExaminedLoop,
    /// <summary>The examined Baby Metroid.</summary>
    BabyMetroidExamined,
    /// <summary>The intro text caret.</summary>
    IntroTextCaret,
    /// <summary>The intro text caret blinking.</summary>
    IntroTextCaretBlink,
    /// <summary>The Japanese text's next-page arrow's loop, which the arrow starts on at each page's input wait (#1655).</summary>
    IntroNextPageArrow,
    /// <summary>The Japanese text's next-page arrow hidden while a second subtitle holds the input wait, then its loop.</summary>
    IntroNextPageArrowDelayed,
    /// <summary>The confused Baby Metroid.</summary>
    ConfusedBabyMetroid,
    /// <summary>The destruction scene's large asteroid.</summary>
    CeresDestructionLargeAsteroid,
    /// <summary>Ceres station under attack.</summary>
    CeresUnderAttack,
    /// <summary>The small asteroids.</summary>
    CeresSmallAsteroid,
    /// <summary>The purple space vortex.</summary>
    CeresVortex,
    /// <summary>Planet Zebes.</summary>
    ZebesPlanet,
    /// <summary>The PLANET ZEBES title.</summary>
    ZebesTitle,
    /// <summary>The Japanese PLANET ZEBES title.</summary>
    ZebesJapaneseTitle,
    /// <summary>Ceres's first delayed small explosion.</summary>
    CeresSmallExplosion,
    /// <summary>Ceres's repeating small explosion.</summary>
    CeresRepeatingSmallExplosion,
    /// <summary>The repeating part of CeresRepeatingSmallExplosion.</summary>
    CeresRepeatingSmallExplosionLoop,
    /// <summary>Ceres's final large explosion.</summary>
    CeresLargeExplosion,
    /// <summary>The repeating part of CeresLargeExplosion.</summary>
    CeresLargeExplosionLoop,
    /// <summary>The piece at the upper left of the hatched egg's shell.</summary>
    MetroidEggShellUpperLeft,
    /// <summary>The piece at the upper right of the hatched egg's shell.</summary>
    MetroidEggShellUpperRight,
    /// <summary>The piece at the left of the hatched egg's shell.</summary>
    MetroidEggShellLeft,
    /// <summary>The piece at the centre of the hatched egg's shell.</summary>
    MetroidEggShellCentre,
    /// <summary>The piece at the right of the hatched egg's shell.</summary>
    MetroidEggShellRight,
    /// <summary>The piece at the bottom of the hatched egg's shell.</summary>
    MetroidEggShellLower,
    /// <summary>A drop of slime falling from the hatched egg.</summary>
    MetroidEggSlimeDrop,
    /// <summary>A drop of slime splashing on the ground.</summary>
    MetroidEggSlimeImpact,
    /// <summary>The upper-left quarter of the stars around Zebes.</summary>
    ZebesStarsUpperLeft,
    /// <summary>The upper-right quarter of the stars around Zebes.</summary>
    ZebesStarsUpperRight,
    /// <summary>The lower-left quarter of the stars around Zebes.</summary>
    ZebesStarsLowerLeft,
    /// <summary>The lower-right quarter of the stars around Zebes.</summary>
    ZebesStarsLowerRight,
    /// <summary>The Ceres flight's star field.</summary>
    CeresStars,
    /// <summary>The intro Mother Brain's big explosion.</summary>
    IntroMotherBrainExplosionBig,
    /// <summary>The intro Mother Brain's small explosion.</summary>
    IntroMotherBrainExplosionSmall,
    /// <summary>An intro Rinka.</summary>
    IntroRinka,
    /// <summary>An intro Rinka moving.</summary>
    IntroRinkaMoving,
    /// <summary>The intro Rinka spawner.</summary>
    IntroRinkaSpawner,
    /// <summary>The Ceres station blast.</summary>
    CeresStationBlast,
    /// <summary>The Ceres explosion spawner's two waves.</summary>
    CeresExplosionSpawner,
    /// <summary>The Ceres flight's large asteroid.</summary>
    CeresLargeAsteroid,
    /// <summary>Deletes the actor.</summary>
    Delete,
    /// <summary>The exploding planet.</summary>
    ZebesExplosionPlanet,
    /// <summary>The repeating part of ZebesExplosionPlanet.</summary>
    ZebesExplosionPlanetLoop,
    /// <summary>The explosion's purple glow.</summary>
    ZebesExplosionGlow,
    /// <summary>The explosion's stars.</summary>
    ZebesExplosionStars,
    /// <summary>The delayed lava.</summary>
    ZebesExplosionLava,
    /// <summary>The repeating part of ZebesExplosionLava.</summary>
    ZebesExplosionLavaLoop,
    /// <summary>The explosion silhouette.</summary>
    ZebesExplosionSilhouette,
    /// <summary>The right stars and the scene hand-off.</summary>
    ZebesExplosionRightStars,
    /// <summary>The left stars.</summary>
    ZebesExplosionLeftStars,
    /// <summary>The afterglow.</summary>
    ZebesExplosionAfterglow,
    /// <summary>THE OPERATION WAS.</summary>
    OperationWasText,
    /// <summary>THE OPERATION WAS held.</summary>
    OperationWasTextHold,
    /// <summary>COMPLETED SUCCESSFULLY.</summary>
    CompletedSuccessfullyText,
    /// <summary>COMPLETED SUCCESSFULLY held.</summary>
    CompletedSuccessfullyTextHold,
    /// <summary>CLEAR TIME and the clear time's spawns.</summary>
    ClearTimeText,
    /// <summary>CLEAR TIME held.</summary>
    ClearTimeTextHold,
    /// <summary>The clear time digit 0.</summary>
    ClearTimeDigit0,
    /// <summary>The clear time digit 1.</summary>
    ClearTimeDigit1,
    /// <summary>The clear time digit 2.</summary>
    ClearTimeDigit2,
    /// <summary>The clear time digit 3.</summary>
    ClearTimeDigit3,
    /// <summary>The clear time digit 4.</summary>
    ClearTimeDigit4,
    /// <summary>The clear time digit 5.</summary>
    ClearTimeDigit5,
    /// <summary>The clear time digit 6.</summary>
    ClearTimeDigit6,
    /// <summary>The clear time digit 7.</summary>
    ClearTimeDigit7,
    /// <summary>The clear time digit 8.</summary>
    ClearTimeDigit8,
    /// <summary>The clear time digit 9.</summary>
    ClearTimeDigit9,
    /// <summary>The clear time's colon.</summary>
    ClearTimeColon,
    /// <summary>The animals escaping Zebes.</summary>
    EndingAnimalEscape,
    /// <summary>The pattern of the upper clouds the gunship escapes between.</summary>
    EndingTopCloudPattern,
    /// <summary>The edge of the upper clouds the gunship escapes between.</summary>
    EndingTopCloudEdge,
    /// <summary>The edge of the lower clouds the gunship escapes between.</summary>
    EndingBottomCloudEdge,
    /// <summary>The pattern of the lower clouds the gunship escapes between.</summary>
    EndingBottomCloudPattern,
    /// <summary>A cloud passing on the right of the escaping gunship.</summary>
    EndingRightSideCloud,
    /// <summary>A cloud passing on the left of the escaping gunship.</summary>
    EndingLeftSideCloud,
    /// <summary>The suitless Samus at rest.</summary>
    SuitlessIdle,
    /// <summary>The suitless Samus's legs at rest.</summary>
    SuitlessLegsIdle,
    /// <summary>The suitless Samus letting her hair down.</summary>
    HairRelease,
    /// <summary>The suitless Samus's legs while she lets her hair down.</summary>
    HairReleaseLegs,
    /// <summary>The suitless Samus jumping.</summary>
    SuitlessJump,
    /// <summary>The suitless Samus in flight.</summary>
    SuitlessJumpFlight,
    /// <summary>Samus falling.</summary>
    Falling,
    /// <summary>Samus landing and shooting.</summary>
    Landing,
    /// <summary>The suited Samus at rest.</summary>
    SuitedIdle,
    /// <summary>The helmet at rest.</summary>
    HelmetIdle,
    /// <summary>The helmetless head at rest.</summary>
    HeadIdle,
    /// <summary>The suited body during the gesture.</summary>
    GestureBody,
    /// <summary>The suited arm's gesture.</summary>
    GestureArm,
    /// <summary>The helmet during the gesture.</summary>
    GestureHelmet,
    /// <summary>The helmetless head during the gesture.</summary>
    GestureHead,
    /// <summary>The suited Samus jumping.</summary>
    SuitedJump,
    /// <summary>The suited Samus in flight.</summary>
    SuitedJumpFlight,
    /// <summary>The helmet's jump.</summary>
    HelmetJump,
    /// <summary>The helmet in flight.</summary>
    HelmetFlight,
    /// <summary>The helmetless head's jump.</summary>
    HeadJump,
    /// <summary>The helmetless head in flight.</summary>
    HeadFlight,
    /// <summary>The icon's upper S.</summary>
    LogoUpperS,
    /// <summary>The icon's lower S.</summary>
    LogoLowerS,
    /// <summary>The icon's upper circle and its right wrap.</summary>
    LogoUpperCircle,
    /// <summary>The repeating part of LogoUpperCircle.</summary>
    LogoUpperCircleLoop,
    /// <summary>The icon's lower circle and its left wrap.</summary>
    LogoLowerCircle,
    /// <summary>The repeating part of LogoLowerCircle.</summary>
    LogoLowerCircleLoop,
}
