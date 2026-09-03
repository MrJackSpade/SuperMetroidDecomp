namespace SuperMetroid.Core.Frontend;

/// <summary>Named bank-$8B cinematic instruction and function entry points.</summary>
internal static class CinematicCodePointers
{
    /// <summary>Bit distinguishing an executable code pointer from a timed list record.</summary>
    public const ushort InstructionCommandBit = 0x8000;

    // Shared bank-$8B cinematic sprite interpreter instructions.
    /// <summary><c>CinematicSpriteObject_Instruction_Sleep</c> at $8B:9442.</summary>
    public const ushort CinematicSpriteObject_Instruction_Sleep = 0x9442;

    /// <summary><c>CinematicSpriteObject_Instruction_SetPreInstruction</c> at $8B:944C.</summary>
    public const ushort CinematicSpriteObject_Instruction_SetPreInstruction = 0x944c;

    /// <summary><c>CinematicSpriteObject_Instruction_Goto</c> at $8B:94BC.</summary>
    public const ushort CinematicSpriteObject_Instruction_Goto = 0x94bc;

    /// <summary><c>CinematicSpriteObject_Instruction_DecrementTimerAndGoto</c> at $8B:94C3.</summary>
    public const ushort CinematicSpriteObject_Instruction_DecrementTimerAndGoto = 0x94c3;

    /// <summary><c>CinematicSpriteObject_Instruction_SetTimer</c> at $8B:94D6.</summary>
    public const ushort CinematicSpriteObject_Instruction_SetTimer = 0x94d6;

    /// <summary><c>CinematicBGObject_Instruction_Delete</c> at $8B:9698.</summary>
    public const ushort CinematicBackgroundObject_Instruction_Delete = 0x9698;

    /// <summary><c>CinematicBGObject_Instruction_Goto</c> at $8B:971E.</summary>
    public const ushort CinematicBackgroundObject_Instruction_Goto = 0x971e;

    /// <summary><c>IndirectInstructionFunction_DoNothing</c> at $8B:8849.</summary>
    public const ushort IndirectInstruction_DoNothing = 0x8849;

    /// <summary><c>IndirectInstructionFunction_DrawTextCharacter</c> at $8B:884D.</summary>
    public const ushort IndirectInstruction_DrawTextCharacter = 0x884d;

    /// <summary><c>IndirectInstructionFunction_DrawToCinematicBGTilemap</c> at $8B:88B7.</summary>
    public const ushort IndirectInstruction_DrawToBackgroundTilemap = 0x88b7;

    /// <summary><c>IndirectInstructionFunction_DrawToBG2Tilemap</c> at $8B:88FD.</summary>
    public const ushort IndirectInstruction_DrawToPortraitTilemap = 0x88fd;

    /// <summary><c>Instruction_FadeInPlanetZebesText</c> at $8B:C9A5.</summary>
    public const ushort Instruction_FadeInPlanetZebesText = 0xc9a5;

    /// <summary><c>Instruction_SpawnPlanetZebesJapanTextIfNeeded</c> at $8B:C9AF.</summary>
    public const ushort Instruction_SpawnPlanetZebesJapanTextIfNeeded = 0xc9af;

    /// <summary><c>Instruction_FadeOutPlanetZebesText</c> at $8B:C9BD.</summary>
    public const ushort Instruction_FadeOutPlanetZebesText = 0xc9bd;

    /// <summary><c>Instruction_StartFlyingToZebes</c> at $8B:C9C7.</summary>
    public const ushort Instruction_StartFlyingToZebes = 0xc9c7;

    /// <summary><c>Instruction_SpawnMetroidEggParticles</c> at $8B:A918.</summary>
    public const ushort Instruction_SpawnMetroidEggParticles = 0xa918;

    /// <summary><c>Instruction_StartIntroPage3</c> at $8B:B33E.</summary>
    public const ushort Instruction_StartIntroPage3 = 0xb33e;

    /// <summary><c>Instruction_StartIntroPage2</c> at $8B:B336.</summary>
    public const ushort Instruction_StartIntroPage2 = 0xb336;

    /// <summary><c>Instruction_SetCaretToBlink</c> at $8B:ADD4.</summary>
    public const ushort Instruction_SetCaretToBlink = 0xadd4;

    public const ushort Instruction_BeginEnglishPage1 = 0xae43;
    public const ushort Instruction_FinishEnglishPage1 = 0xae5b;
    public const ushort Instruction_BeginEnglishPage2 = 0xae79;
    public const ushort Instruction_FinishEnglishPage2 = 0xae91;
    public const ushort Instruction_BeginEnglishPage3 = 0xb074;
    public const ushort Instruction_FinishEnglishPage3 = 0xb08c;
    public const ushort Instruction_BeginEnglishPage4 = 0xb0b3;
    public const ushort Instruction_FinishEnglishPage4 = 0xb0cb;
    public const ushort Instruction_BeginEnglishPage5 = 0xb19b;
    public const ushort Instruction_FinishEnglishPage5 = 0xb1b3;
    public const ushort Instruction_BeginEnglishPage6 = 0xb228;
    public const ushort Instruction_FinishIntro = 0xb240;

    /// <summary><c>PreInstruction_ConfusedBabyMetroid_Hatched</c> at $8B:BA73.</summary>
    public const ushort PreInstruction_ConfusedBabyMetroid_Hatched = 0xba73;

    /// <summary><c>PreInstruction_ConfusedBabyMetroid_Idling</c> at $8B:BB0D.</summary>
    public const ushort PreInstruction_ConfusedBabyMetroid_Idling = 0xbb0d;

    /// <summary><c>PreInstruction_ConfusedBabyMetroid_Dancing</c> at $8B:BB24.</summary>
    public const ushort PreInstruction_ConfusedBabyMetroid_Dancing = 0xbb24;

    public const ushort PreInstruction_MetroidEgg_DeleteAfterCrossFade = 0xa903;
    public const ushort PreInstruction_IntroMotherBrain_CrossFading = 0xb82e;
    public const ushort PreInstruction_IntroRinka_Moving_HitsSamus = 0xb8d8;
    public const ushort PreInstruction_IntroRinka_Moving_MissesSamus = 0xb93b;

    public const ushort Instruction_StartMoving_IntroRinka = 0xb8c5;
    public const ushort Instruction_Spawn_IntroRinkas_0_1 = 0xba21;
    public const ushort Instruction_Spawn_IntroRinkas_2_3 = 0xba36;

    /// <summary><c>Instruction_PlayBabyMetroid_Cry1</c> at $8B:A25B.</summary>
    public const ushort Instruction_PlayBabyMetroid_Cry1 = 0xa25b;

    /// <summary><c>Instruction_PlayBabyMetroid_Cry2</c> at $8B:A263.</summary>
    public const ushort Instruction_PlayBabyMetroid_Cry2 = 0xa263;

    /// <summary><c>Instruction_PlayBabyMetroid_Cry3</c> at $8B:A26B.</summary>
    public const ushort Instruction_PlayBabyMetroid_Cry3 = 0xa26b;

    /// <summary><c>Instruction_StartIntroPage4</c> at $8B:B346.</summary>
    public const ushort Instruction_StartIntroPage4 = 0xb346;

    /// <summary><c>Instruction_StartIntroPage5</c> at $8B:B34E.</summary>
    public const ushort Instruction_StartIntroPage5 = 0xb34e;

    /// <summary><c>Instruction_TriggerTitleSequenceScene0</c> at $8B:9CE1.</summary>
    public const ushort Instruction_TriggerTitleSequenceScene0 = 0x9ce1;

    /// <summary><c>Instruction_TriggerTitleSequenceScene1</c> at $8B:9D5D.</summary>
    public const ushort Instruction_TriggerTitleSequenceScene1 = 0x9d5d;

    /// <summary><c>Instruction_TriggerTitleSequenceScene2</c> at $8B:9DD6.</summary>
    public const ushort Instruction_TriggerTitleSequenceScene2 = 0x9dd6;

    /// <summary><c>Instruction_TriggerTitleSequenceScene3</c> at $8B:9E58.</summary>
    public const ushort Instruction_TriggerTitleSequenceScene3 = 0x9e58;

    /// <summary><c>CinematicSpriteObject_Instruction_Delete</c> at $8B:9438.</summary>
    public const ushort CinematicSpriteObject_Instruction_Delete = 0x9438;

    // Credits and ending callbacks are also bank-$8B code words embedded in bank-$8C lists.
    public const ushort CreditsObject_Instruction_Delete = 0x99fe;
    public const ushort CreditsObject_Instruction_DecrementTimerAndGoto = 0x9a0d;
    public const ushort CreditsObject_Instruction_SetTimer = 0x9a17;
    public const ushort CreditsObject_Instruction_EndCredits = 0xf6fe;
    public const ushort Ending_Instruction_DrawItemPercentage = 0xe627;
    public const ushort Ending_Instruction_DrawItemPercentageSubtitle = 0xe769;
    public const ushort Ending_Instruction_ClearItemPercentageSubtitle = 0xe780;
    public const ushort Ending_Instruction_FadeExplosionPalette = 0xf284;
    public const ushort Ending_Instruction_SpawnExplosionSilhouette = 0xf295;
    public const ushort Ending_Instruction_StartZebesExplosion = 0xf2b7;
    public const ushort Ending_Instruction_ExplosionFinale = 0xf2fa;
    public const ushort Ending_Instruction_EndZebesExplosion = 0xf32b;
    public const ushort Ending_Instruction_SpawnCompletedText = 0xf3b0;
    public const ushort Ending_Instruction_SpawnClearTime = 0xf3ce;
    public const ushort Ending_Instruction_SpawnHoursTens = 0xf41b;
    public const ushort Ending_Instruction_SpawnHoursUnits = 0xf424;
    public const ushort Ending_Instruction_SpawnColon = 0xf42d;
    public const ushort Ending_Instruction_SpawnMinutesTens = 0xf436;
    public const ushort Ending_Instruction_SpawnMinutesUnits = 0xf43f;
    public const ushort Ending_Instruction_TransitionToCredits = 0xf448;

    /// <summary>Named bank-$8B instruction-list cursors assigned by translated scenes.</summary>
    public static class Lists
    {
        public const ushort IntroMotherBrain = 0xcb05;
        public const ushort IntroMotherBrainStartPage2 = 0xcb19;
        public const ushort MetroidEgg = 0xcb33;
        public const ushort MetroidEggHatching = 0xcb3b;
        public const ushort MetroidEggHatchedFrame2 = 0xcb79;
        public const ushort BabyMetroidBeingDelivered = 0xcb9f;
        public const ushort BabyMetroidBeingExamined = 0xcbcd;
        public const ushort ConfusedBabyMetroid = 0xcc2b;
        public const ushort IntroTextCaret = 0xcbfb;
        public const ushort IntroTextCaretBlink = 0xcc03;
        public const ushort CeresUnderAttack = 0xcc47;
        public const ushort CeresSmallAsteroids = 0xcc4f;
        public const ushort CeresPurpleSpaceVortex = 0xcc57;
        public const ushort MetroidEggParticle1 = 0xcd39;
        public const ushort MetroidEggParticleStride = 8;
        public const ushort MetroidEggSlimeDrops = 0xcd69;
        public const ushort MetroidEggParticleHitGround = 0xcd71;
        public const ushort CeresStars = 0xcda3;
        public const ushort IntroMotherBrainExplosionBig = 0xcdab;
        public const ushort IntroMotherBrainExplosionSmall = 0xcdcb;
        public const ushort IntroRinka = 0xcdeb;
        public const ushort IntroRinkaSpawner = 0xce0d;
        public const ushort CeresExplosionLargeAsteroids = 0xce4b;
        public const ushort Delete = 0xce53;
    }

    /// <summary>Named bank-$8C background-object lists consumed by translated intro scenes.</summary>
    public static class BackgroundLists
    {
        public const ushort IntroTextPage1 = 0xc383;
        public const ushort IntroTextPage2 = 0xc797;
        public const ushort IntroTextPage3 = 0xcb45;
        public const ushort IntroTextPage4 = 0xce33;
        public const ushort IntroTextPage5 = 0xd15d;
        public const ushort IntroTextPage6 = 0xd511;
        public const ushort SamusBlinking = 0xd5df;
        public const ushort SamusBlinkingPage6 = 0xd613;
    }

    /// <summary>Named bank-$8C indirect-data records compared by cinematic draw code.</summary>
    public static class IndirectData
    {
        public const ushort IntroTextSpace = 0xd67d;
    }

}
