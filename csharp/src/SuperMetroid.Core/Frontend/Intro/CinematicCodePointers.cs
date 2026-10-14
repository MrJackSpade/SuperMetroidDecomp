namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Named bank-$8B cinematic function entry points and instruction-list cursors.</summary>
internal static class CinematicCodePointers
{
    /// <summary>Bit distinguishing an executable code pointer from a timed list record.</summary>
    public const ushort InstructionCommandBit = 0x8000;

    /// <summary><c>RTS_8B93D9</c>, the shared cinematic-sprite no-op callback.</summary>
    public const ushort CinematicSpriteObject_PreInstruction_NoOp = 0x93d9;

    /// <summary><c>IndirectInstructionFunction_DrawToBG2Tilemap</c> at $8B:88FD.</summary>
    public const ushort IndirectInstruction_DrawToPortraitTilemap = 0x88fd;

    /// <summary><c>Instruction_StartIntroPage2</c> at $8B:B336.</summary>
    public const ushort Instruction_StartIntroPage2 = 0xb336;

    /// <summary>$8B:A903, installed after the egg's cross-fade so its sprite redirects to the shared delete list.</summary>
    public const ushort PreInstruction_MetroidEgg_DeleteAfterCrossFade = 0xa903;
    /// <summary>$8B:B82E, the Mother Brain pre-instruction checked while the intro flashback cross-fades.</summary>
    public const ushort PreInstruction_IntroMotherBrain_CrossFading = 0xb82e;

    /// <summary>Named bank-$8B instruction-list cursors assigned by translated scenes.</summary>
    public static class Lists
    {
        /// <summary>$8B:CB05, first intro Mother Brain sprite instruction before the second-page redirect.</summary>
        public const ushort IntroMotherBrain = 0xcb05;
        /// <summary>$8B:CB19, Mother Brain sprite entry installed when the intro advances to page two.</summary>
        public const ushort IntroMotherBrainStartPage2 = 0xcb19;
        /// <summary>$8B:CB33, beginning of the egg's discovery and hatching instruction span.</summary>
        public const ushort MetroidEgg = 0xcb33;
        /// <summary>$8B:CB3B, egg sprite cursor selected when hatching begins.</summary>
        public const ushort MetroidEggHatching = 0xcb3b;
        /// <summary>$8B:CB79, threshold used to identify the egg's later hatched pose.</summary>
        public const ushort MetroidEggHatchedFrame2 = 0xcb79;
        /// <summary>$8B:CB9F, first baby-delivery instruction used by the scientist cutscene.</summary>
        public const ushort BabyMetroidBeingDelivered = 0xcb9f;
        /// <summary>$8B:CBCD, first scientist examination instruction and end of the delivery span.</summary>
        public const ushort BabyMetroidBeingExamined = 0xcbcd;
        /// <summary>$8B:CC2B, confused baby instruction and boundary after the examination span.</summary>
        public const ushort ConfusedBabyMetroid = 0xcc2b;
        /// <summary>$8B:CBFB, intro narration caret entry before its blinking redirect.</summary>
        public const ushort IntroTextCaret = 0xcbfb;
        /// <summary>$8B:CC03, blinking caret entry selected by the narration state.</summary>
        public const ushort IntroTextCaretBlink = 0xcc03;
        /// <summary>$8B:CC47, boundary after baby-discovery instructions as the Ceres attack scene begins.</summary>
        public const ushort CeresUnderAttack = 0xcc47;
        /// <summary>$8B:CD39, first egg-fragment particle instruction before the ground-impact redirect.</summary>
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
        /// <summary>$8B:CDAB, large fourth-hit Mother Brain explosion instruction entry.</summary>
        public const ushort IntroMotherBrainExplosionBig = 0xcdab;
        /// <summary>$8B:CDCB, small fourth-hit Mother Brain explosion instruction entry.</summary>
        public const ushort IntroMotherBrainExplosionSmall = 0xcdcb;
        /// <summary>$8B:CDEB, expanding Rinka animation entry ahead of its movement pulse.</summary>
        public const ushort IntroRinka = 0xcdeb;
        /// <summary>$8B:CE53, shared cinematic sprite deletion list used after several intro effects finish.</summary>
        public const ushort Delete = 0xce53;
    }

    /// <summary>Named bank-$8C background-object lists consumed by translated intro scenes.</summary>
    public static class BackgroundLists
    {
        /// <summary>$8C:D5DF, initial background-object list for Samus's blinking eye.</summary>
        public const ushort SamusBlinking = 0xd5df;
        /// <summary>$8C:D613, blinking-eye background list installed when the intro reaches page six.</summary>
        public const ushort SamusBlinkingPage6 = 0xd613;
    }
}
