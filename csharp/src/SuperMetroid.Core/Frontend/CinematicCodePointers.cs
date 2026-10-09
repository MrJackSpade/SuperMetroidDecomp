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

    /// <summary><c>RTS_8B93D9</c>, the shared cinematic-sprite no-op callback.</summary>
    public const ushort CinematicSpriteObject_PreInstruction_NoOp = 0x93d9;

    /// <summary><c>CinematicBGObject_Instruction_Delete</c> at $8B:9698.</summary>
    public const ushort CinematicBackgroundObject_Instruction_Delete = 0x9698;

    /// <summary><c>CinematicBGObject_Instruction_Goto</c> at $8B:971E.</summary>
    public const ushort CinematicBackgroundObject_Instruction_Goto = 0x971e;

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

    /// <summary>Bank-$8B instruction that starts English narration page one.</summary>
    public const ushort Instruction_BeginEnglishPage1 = 0xae43;
    /// <summary>Bank-$8B instruction that completes page one's English narration sequence.</summary>
    public const ushort Instruction_FinishEnglishPage1 = 0xae5b;
    /// <summary>Bank-$8B instruction that starts English narration page two.</summary>
    public const ushort Instruction_BeginEnglishPage2 = 0xae79;
    /// <summary>Bank-$8B instruction that completes page two's English narration sequence.</summary>
    public const ushort Instruction_FinishEnglishPage2 = 0xae91;
    /// <summary>Bank-$8B instruction that starts English narration page three.</summary>
    public const ushort Instruction_BeginEnglishPage3 = 0xb074;
    /// <summary>Bank-$8B instruction that completes page three's English narration sequence.</summary>
    public const ushort Instruction_FinishEnglishPage3 = 0xb08c;
    /// <summary>Bank-$8B instruction that starts English narration page four.</summary>
    public const ushort Instruction_BeginEnglishPage4 = 0xb0b3;
    /// <summary>Bank-$8B instruction that completes page four's English narration sequence.</summary>
    public const ushort Instruction_FinishEnglishPage4 = 0xb0cb;
    /// <summary>Bank-$8B instruction that starts English narration page five.</summary>
    public const ushort Instruction_BeginEnglishPage5 = 0xb19b;
    /// <summary>Bank-$8B instruction that completes page five's English narration sequence.</summary>
    public const ushort Instruction_FinishEnglishPage5 = 0xb1b3;
    /// <summary>Bank-$8B instruction that starts the final English narration page.</summary>
    public const ushort Instruction_BeginEnglishPage6 = 0xb228;
    /// <summary>Bank-$8B instruction that finishes the intro after the final page.</summary>
    public const ushort Instruction_FinishIntro = 0xb240;

    /// <summary><c>PreInstruction_ConfusedBabyMetroid_Hatched</c> at $8B:BA73.</summary>
    public const ushort PreInstruction_ConfusedBabyMetroid_Hatched = 0xba73;

    /// <summary><c>PreInstruction_ConfusedBabyMetroid_Idling</c> at $8B:BB0D.</summary>
    public const ushort PreInstruction_ConfusedBabyMetroid_Idling = 0xbb0d;

    /// <summary><c>PreInstruction_ConfusedBabyMetroid_Dancing</c> at $8B:BB24.</summary>
    public const ushort PreInstruction_ConfusedBabyMetroid_Dancing = 0xbb24;

    /// <summary>Deletes the egg actor once its page-transition crossfade has completed.</summary>
    public const ushort PreInstruction_MetroidEgg_DeleteAfterCrossFade = 0xa903;
    /// <summary>Updates the Mother Brain actor while its intro scene is crossfading.</summary>
    public const ushort PreInstruction_IntroMotherBrain_CrossFading = 0xb82e;
    /// <summary>Rinka movement callback for a projectile path that hits Samus.</summary>
    public const ushort PreInstruction_IntroRinka_Moving_HitsSamus = 0xb8d8;
    /// <summary>Rinka movement callback for a projectile path that misses Samus.</summary>
    public const ushort PreInstruction_IntroRinka_Moving_MissesSamus = 0xb93b;

    /// <summary>Starts the moving behavior used by intro Rinka actors.</summary>
    public const ushort Instruction_StartMoving_IntroRinka = 0xb8c5;
    /// <summary>Spawns the first pair of Rinkas in the intro attack sequence.</summary>
    public const ushort Instruction_Spawn_IntroRinkas_0_1 = 0xba21;
    /// <summary>Spawns the second pair of Rinkas in the intro attack sequence.</summary>
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
    /// <summary>Begins fading the ending explosion palette.</summary>
    public const ushort Ending_Instruction_FadeExplosionPalette = 0xf284;
    /// <summary>Creates the silhouette actor used at the start of the ending explosion.</summary>
    public const ushort Ending_Instruction_SpawnExplosionSilhouette = 0xf295;
    /// <summary>Starts the scripted Zebes explosion sequence.</summary>
    public const ushort Ending_Instruction_StartZebesExplosion = 0xf2b7;
    /// <summary>Runs the finale portion of the Zebes explosion sequence.</summary>
    public const ushort Ending_Instruction_ExplosionFinale = 0xf2fa;
    /// <summary>Ends the active Zebes explosion sequence.</summary>
    public const ushort Ending_Instruction_EndZebesExplosion = 0xf32b;
    /// <summary>Spawns the completed-game text after the explosion.</summary>
    public const ushort Ending_Instruction_SpawnCompletedText = 0xf3b0;
    /// <summary>Starts the clear-time display in the ending.</summary>
    public const ushort Ending_Instruction_SpawnClearTime = 0xf3ce;
    /// <summary>Displays the tens digit of the clear-time hours.</summary>
    public const ushort Ending_Instruction_SpawnHoursTens = 0xf41b;
    /// <summary>Displays the units digit of the clear-time hours.</summary>
    public const ushort Ending_Instruction_SpawnHoursUnits = 0xf424;
    /// <summary>Displays the separator between clear-time hours and minutes.</summary>
    public const ushort Ending_Instruction_SpawnColon = 0xf42d;
    /// <summary>Displays the tens digit of the clear-time minutes.</summary>
    public const ushort Ending_Instruction_SpawnMinutesTens = 0xf436;
    /// <summary>Displays the units digit of the clear-time minutes.</summary>
    public const ushort Ending_Instruction_SpawnMinutesUnits = 0xf43f;
    /// <summary>Moves the ending sequence into the credits.</summary>
    public const ushort Ending_Instruction_TransitionToCredits = 0xf448;

    /// <summary>Named bank-$8B instruction-list cursors assigned by translated scenes.</summary>
    public static class Lists
    {
        /// <summary>Mother Brain actor's initial intro instruction sequence.</summary>
        public const ushort IntroMotherBrain = 0xcb05;
        /// <summary>Mother Brain instruction sequence that begins narration page two.</summary>
        public const ushort IntroMotherBrainStartPage2 = 0xcb19;
        /// <summary>Metroid egg's pre-hatch idle sequence.</summary>
        public const ushort MetroidEgg = 0xcb33;
        /// <summary>Metroid egg hatching animation sequence.</summary>
        public const ushort MetroidEggHatching = 0xcb3b;
        /// <summary>Second frame sequence used after the egg has hatched.</summary>
        public const ushort MetroidEggHatchedFrame2 = 0xcb79;
        /// <summary>Baby Metroid's actor sequence while being delivered.</summary>
        public const ushort BabyMetroidBeingDelivered = 0xcb9f;
        /// <summary>Baby Metroid's actor sequence while being examined.</summary>
        public const ushort BabyMetroidBeingExamined = 0xcbcd;
        /// <summary>Baby Metroid behavior after it becomes confused.</summary>
        public const ushort ConfusedBabyMetroid = 0xcc2b;
        /// <summary>Intro narration caret's steady visible sequence.</summary>
        public const ushort IntroTextCaret = 0xcbfb;
        /// <summary>Intro narration caret's blinking sequence.</summary>
        public const ushort IntroTextCaretBlink = 0xcc03;
        /// <summary>Station actors' instruction sequence during the Ceres attack scene.</summary>
        public const ushort CeresUnderAttack = 0xcc47;
        /// <summary>First particle sequence emitted by the hatching Metroid egg.</summary>
        public const ushort MetroidEggParticle1 = 0xcd39;
        /// <summary><c>InstList_MetroidEggParticle_HitGround</c> at $8B:CD71, the slime impact sequence.</summary>
        /// <remarks>
        /// Issues #625 and #984: after a slime drop reaches Y=$00A8,
        /// IntroEggSlimeDrop redirects the generic sprite interpreter here.
        /// For frame f=0..3, $8B:CD71+4*f stores duration ten and bank-$8C
        /// spritemap pointer $8FAF+7*f; $8B:CD81 then stores delete opcode
        /// $9438. All nine words match pinned NTSC J/U v1.0 ROM and
        /// bank_8B.asm, and each selected bank-$8C spritemap has one entry.
        /// The interpreter loads the first frame on redirect, counts ten
        /// calls per frame, and deletes after the fourth; no fifth frame is
        /// inferred from the adjacent $8B:CD83 list.
        /// </remarks>
        public const ushort MetroidEggParticleHitGround = 0xcd71;
        /// <summary>Large explosion actor sequence in the Mother Brain flashback.</summary>
        public const ushort IntroMotherBrainExplosionBig = 0xcdab;
        /// <summary>Small explosion actor sequence in the Mother Brain flashback.</summary>
        public const ushort IntroMotherBrainExplosionSmall = 0xcdcb;
        /// <summary>Instruction sequence used by Rinka actors in the intro attack.</summary>
        public const ushort IntroRinka = 0xcdeb;
        /// <summary>Shared bank-$8B list that immediately deletes its actor.</summary>
        public const ushort Delete = 0xce53;
    }

    /// <summary>Named bank-$8C background-object lists consumed by translated intro scenes.</summary>
    public static class BackgroundLists
    {
        /// <summary>Background-object instructions for narration page one.</summary>
        public const ushort IntroTextPage1 = 0xc383;
        /// <summary>Background-object instructions for narration page two.</summary>
        public const ushort IntroTextPage2 = 0xc797;
        /// <summary>Background-object instructions for narration page three.</summary>
        public const ushort IntroTextPage3 = 0xcb45;
        /// <summary>Background-object instructions for narration page four.</summary>
        public const ushort IntroTextPage4 = 0xce33;
        /// <summary>Background-object instructions for narration page five.</summary>
        public const ushort IntroTextPage5 = 0xd15d;
        /// <summary>Background-object instructions for the final narration page.</summary>
        public const ushort IntroTextPage6 = 0xd511;
        /// <summary>Samus portrait sequence with the ordinary blinking animation.</summary>
        public const ushort SamusBlinking = 0xd5df;
        /// <summary>Samus portrait blinking sequence used specifically on page six.</summary>
        public const ushort SamusBlinkingPage6 = 0xd613;
    }
}
