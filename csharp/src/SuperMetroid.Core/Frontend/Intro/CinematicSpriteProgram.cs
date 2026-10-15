namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Every bank-$8B cinematic sprite list as a typed program (#641), in authored order.</summary>
internal enum CinematicSpriteProgram
{
    /// <summary>The intro Mother Brain's four-pose loop ($8B:CB05).</summary>
    IntroMotherBrain,
    /// <summary>Starts the intro's second page and its cross-fade ($8B:CB19).</summary>
    IntroMotherBrainStartPage2,
    /// <summary>The intro Mother Brain's loop during the second page ($8B:CB1F).</summary>
    IntroMotherBrainPageTwo,
    /// <summary>The Metroid egg at rest ($8B:CB33).</summary>
    MetroidEgg,
    /// <summary>The Metroid egg shaking and hatching ($8B:CB3B).</summary>
    MetroidEggHatching,
    /// <summary>The Metroid egg's shaking and hatching frames ($8B:CB43).</summary>
    MetroidEggShaking,
    /// <summary>The hatched egg's remaining frames before the third page ($8B:CB79).</summary>
    MetroidEggHatchedFrame2,
    /// <summary>The hatched egg ($8B:CB97).</summary>
    MetroidEggHatched,
    /// <summary>The Baby Metroid being delivered ($8B:CB9F).</summary>
    BabyMetroidBeingDelivered,
    /// <summary>The delivered Baby Metroid's counted loop before the fourth page ($8B:CBA3).</summary>
    BabyMetroidBeingDeliveredLoop,
    /// <summary>The delivered Baby Metroid ($8B:CBB9).</summary>
    BabyMetroidDelivered,
    /// <summary>The Baby Metroid being examined ($8B:CBCD).</summary>
    BabyMetroidBeingExamined,
    /// <summary>The examined Baby Metroid's counted loop before the fifth page ($8B:CBD1).</summary>
    BabyMetroidBeingExaminedLoop,
    /// <summary>The examined Baby Metroid ($8B:CBE7).</summary>
    BabyMetroidExamined,
    /// <summary>The intro text caret ($8B:CBFB).</summary>
    IntroTextCaret,
    /// <summary>The intro text caret blinking ($8B:CC03).</summary>
    IntroTextCaretBlink,
    /// <summary>The scientist cutscene's arrow ($8B:CC0F).</summary>
    IntroScientistArrow,
    /// <summary>The scientist cutscene's arrow after its delay ($8B:CC23).</summary>
    IntroScientistArrowDelayed,
    /// <summary>The confused Baby Metroid ($8B:CC2B).</summary>
    ConfusedBabyMetroid,
    /// <summary>The destruction scene's large asteroid ($8B:CC3F).</summary>
    CeresDestructionLargeAsteroid,
    /// <summary>Ceres station under attack ($8B:CC47).</summary>
    CeresUnderAttack,
    /// <summary>The small asteroids ($8B:CC4F).</summary>
    CeresSmallAsteroid,
    /// <summary>The purple space vortex ($8B:CC57).</summary>
    CeresVortex,
    /// <summary>Planet Zebes ($8B:CCAB).</summary>
    ZebesPlanet,
    /// <summary>The PLANET ZEBES title ($8B:CCBB).</summary>
    ZebesTitle,
    /// <summary>The Japanese PLANET ZEBES title ($8B:CCD5).</summary>
    ZebesJapaneseTitle,
    /// <summary>Ceres's first delayed small explosion ($8B:CCDB).</summary>
    CeresSmallExplosion,
    /// <summary>Ceres's repeating small explosion ($8B:CCF5).</summary>
    CeresRepeatingSmallExplosion,
    /// <summary>The repeating part of CeresRepeatingSmallExplosion ($8B:CCF9).</summary>
    CeresRepeatingSmallExplosionLoop,
    /// <summary>Ceres's final large explosion ($8B:CD1B).</summary>
    CeresLargeExplosion,
    /// <summary>The repeating part of CeresLargeExplosion ($8B:CD1F).</summary>
    CeresLargeExplosionLoop,
    /// <summary>Metroid egg particle 1 ($8B:CD39).</summary>
    MetroidEggParticle1,
    /// <summary>Metroid egg particle 2 ($8B:CD41).</summary>
    MetroidEggParticle2,
    /// <summary>Metroid egg particle 3 ($8B:CD49).</summary>
    MetroidEggParticle3,
    /// <summary>Metroid egg particle 4 ($8B:CD51).</summary>
    MetroidEggParticle4,
    /// <summary>Metroid egg particle 5 ($8B:CD59).</summary>
    MetroidEggParticle5,
    /// <summary>Metroid egg particle 6 ($8B:CD61).</summary>
    MetroidEggParticle6,
    /// <summary>Metroid egg particle 7 ($8B:CD69).</summary>
    MetroidEggParticle7,
    /// <summary>An egg particle hitting the ground ($8B:CD71).</summary>
    MetroidEggParticleHitGround,
    /// <summary>Zebes star sheet 0 ($8B:CD83).</summary>
    ZebesStarSheet0,
    /// <summary>Zebes star sheet 1 ($8B:CD8B).</summary>
    ZebesStarSheet1,
    /// <summary>Zebes star sheet 2 ($8B:CD93).</summary>
    ZebesStarSheet2,
    /// <summary>Zebes star sheet 3 ($8B:CD9B).</summary>
    ZebesStarSheet3,
    /// <summary>The Ceres flight's star field ($8B:CDA3).</summary>
    CeresStars,
    /// <summary>The intro Mother Brain's big explosion ($8B:CDAB).</summary>
    IntroMotherBrainExplosionBig,
    /// <summary>The intro Mother Brain's small explosion ($8B:CDCB).</summary>
    IntroMotherBrainExplosionSmall,
    /// <summary>An intro Rinka ($8B:CDEB).</summary>
    IntroRinka,
    /// <summary>An intro Rinka moving ($8B:CDF9).</summary>
    IntroRinkaMoving,
    /// <summary>The intro Rinka spawner ($8B:CE0D).</summary>
    IntroRinkaSpawner,
    /// <summary>The Ceres station blast ($8B:CE1B).</summary>
    CeresStationBlast,
    /// <summary>The Ceres explosion spawner's two waves ($8B:CE35).</summary>
    CeresExplosionSpawner,
    /// <summary>The Ceres flight's large asteroid ($8B:CE4B).</summary>
    CeresLargeAsteroid,
    /// <summary>Deletes the actor ($8B:CE53).</summary>
    Delete,
    /// <summary>The exploding planet ($8B:EB0F).</summary>
    ZebesExplosionPlanet,
    /// <summary>The repeating part of ZebesExplosionPlanet ($8B:EB13).</summary>
    ZebesExplosionPlanetLoop,
    /// <summary>The explosion's purple glow ($8B:EB3D).</summary>
    ZebesExplosionGlow,
    /// <summary>The explosion's stars ($8B:EB51).</summary>
    ZebesExplosionStars,
    /// <summary>The delayed lava ($8B:EB59).</summary>
    ZebesExplosionLava,
    /// <summary>The repeating part of ZebesExplosionLava ($8B:EB5D).</summary>
    ZebesExplosionLavaLoop,
    /// <summary>The explosion silhouette ($8B:EB69).</summary>
    ZebesExplosionSilhouette,
    /// <summary>The right stars and the scene hand-off ($8B:EB71).</summary>
    ZebesExplosionRightStars,
    /// <summary>The left stars ($8B:EB81).</summary>
    ZebesExplosionLeftStars,
    /// <summary>The afterglow ($8B:EB89).</summary>
    ZebesExplosionAfterglow,
    /// <summary>THE OPERATION WAS ($8B:EB91).</summary>
    OperationWasText,
    /// <summary>THE OPERATION WAS held ($8B:EBCF).</summary>
    OperationWasTextHold,
    /// <summary>COMPLETED SUCCESSFULLY ($8B:EBD7).</summary>
    CompletedSuccessfullyText,
    /// <summary>COMPLETED SUCCESSFULLY held ($8B:EC2D).</summary>
    CompletedSuccessfullyTextHold,
    /// <summary>CLEAR TIME and the clear time's spawns ($8B:EC35).</summary>
    ClearTimeText,
    /// <summary>CLEAR TIME held ($8B:EC79).</summary>
    ClearTimeTextHold,
    /// <summary>The clear time digit 0 ($8B:EC81).</summary>
    ClearTimeDigit0,
    /// <summary>The clear time digit 1 ($8B:EC89).</summary>
    ClearTimeDigit1,
    /// <summary>The clear time digit 2 ($8B:EC91).</summary>
    ClearTimeDigit2,
    /// <summary>The clear time digit 3 ($8B:EC99).</summary>
    ClearTimeDigit3,
    /// <summary>The clear time digit 4 ($8B:ECA1).</summary>
    ClearTimeDigit4,
    /// <summary>The clear time digit 5 ($8B:ECA9).</summary>
    ClearTimeDigit5,
    /// <summary>The clear time digit 6 ($8B:ECB1).</summary>
    ClearTimeDigit6,
    /// <summary>The clear time digit 7 ($8B:ECB9).</summary>
    ClearTimeDigit7,
    /// <summary>The clear time digit 8 ($8B:ECC1).</summary>
    ClearTimeDigit8,
    /// <summary>The clear time digit 9 ($8B:ECC9).</summary>
    ClearTimeDigit9,
    /// <summary>The clear time's colon ($8B:ECD1).</summary>
    ClearTimeColon,
    /// <summary>The animals escaping Zebes ($8B:ECD9).</summary>
    EndingAnimalEscape,
    /// <summary>Ending cloud 0 ($8B:ECED).</summary>
    EndingCloud0,
    /// <summary>Ending cloud 1 ($8B:ECF5).</summary>
    EndingCloud1,
    /// <summary>Ending cloud 2 ($8B:ECFD).</summary>
    EndingCloud2,
    /// <summary>Ending cloud 3 ($8B:ED05).</summary>
    EndingCloud3,
    /// <summary>Ending cloud 4 ($8B:ED0D).</summary>
    EndingCloud4,
    /// <summary>Ending cloud 5 ($8B:ED15).</summary>
    EndingCloud5,
    /// <summary>The suitless Samus at rest ($8B:ED1D).</summary>
    SuitlessIdle,
    /// <summary>The suitless Samus's legs at rest ($8B:ED25).</summary>
    SuitlessLegsIdle,
    /// <summary>The suitless Samus letting her hair down ($8B:ED2D).</summary>
    HairRelease,
    /// <summary>The suitless Samus's legs while she lets her hair down ($8B:ED59).</summary>
    HairReleaseLegs,
    /// <summary>The suitless Samus jumping ($8B:ED7F).</summary>
    SuitlessJump,
    /// <summary>The suitless Samus in flight ($8B:ED8D).</summary>
    SuitlessJumpFlight,
    /// <summary>Samus falling ($8B:ED95).</summary>
    Falling,
    /// <summary>Samus landing and shooting ($8B:ED9D).</summary>
    Landing,
    /// <summary>The suited Samus at rest ($8B:EDB1).</summary>
    SuitedIdle,
    /// <summary>The helmet at rest ($8B:EDB9).</summary>
    HelmetIdle,
    /// <summary>The helmetless head at rest ($8B:EDC1).</summary>
    HeadIdle,
    /// <summary>The suited body during the gesture ($8B:EDC9).</summary>
    GestureBody,
    /// <summary>The suited arm's gesture ($8B:EDD3).</summary>
    GestureArm,
    /// <summary>The helmet during the gesture ($8B:EE0F).</summary>
    GestureHelmet,
    /// <summary>The helmetless head during the gesture ($8B:EE15).</summary>
    GestureHead,
    /// <summary>The suited Samus jumping ($8B:EE27).</summary>
    SuitedJump,
    /// <summary>The suited Samus in flight ($8B:EE35).</summary>
    SuitedJumpFlight,
    /// <summary>The helmet's jump ($8B:EE3D).</summary>
    HelmetJump,
    /// <summary>The helmet in flight ($8B:EE45).</summary>
    HelmetFlight,
    /// <summary>The helmetless head's jump ($8B:EE4D).</summary>
    HeadJump,
    /// <summary>The helmetless head in flight ($8B:EE55).</summary>
    HeadFlight,
    /// <summary>The icon's upper S ($8B:EE5D).</summary>
    LogoUpperS,
    /// <summary>The icon's lower S ($8B:EE65).</summary>
    LogoLowerS,
    /// <summary>The icon's upper circle and its right wrap ($8B:EE6D).</summary>
    LogoUpperCircle,
    /// <summary>The repeating part of LogoUpperCircle ($8B:EE7F).</summary>
    LogoUpperCircleLoop,
    /// <summary>The icon's lower circle and its left wrap ($8B:EE87).</summary>
    LogoLowerCircle,
    /// <summary>The repeating part of LogoLowerCircle ($8B:EE93).</summary>
    LogoLowerCircleLoop,
}
