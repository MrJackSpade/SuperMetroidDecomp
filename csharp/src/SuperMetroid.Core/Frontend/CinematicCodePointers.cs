namespace SuperMetroid.Core.Frontend;

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

    /// <summary>Stores pre instruction metroid egg delete after cross fade.</summary>
    public const ushort PreInstruction_MetroidEgg_DeleteAfterCrossFade = 0xa903;
    /// <summary>Stores pre instruction intro mother brain cross fading.</summary>
    public const ushort PreInstruction_IntroMotherBrain_CrossFading = 0xb82e;

    /// <summary>Named bank-$8B instruction-list cursors assigned by translated scenes.</summary>
    public static class Lists
    {
        /// <summary>Stores intro mother brain.</summary>
        public const ushort IntroMotherBrain = 0xcb05;
        /// <summary>Stores intro mother brain start page2.</summary>
        public const ushort IntroMotherBrainStartPage2 = 0xcb19;
        /// <summary>Stores metroid egg.</summary>
        public const ushort MetroidEgg = 0xcb33;
        /// <summary>Stores metroid egg hatching.</summary>
        public const ushort MetroidEggHatching = 0xcb3b;
        /// <summary>Stores metroid egg hatched frame2.</summary>
        public const ushort MetroidEggHatchedFrame2 = 0xcb79;
        /// <summary>Stores baby metroid being delivered.</summary>
        public const ushort BabyMetroidBeingDelivered = 0xcb9f;
        /// <summary>Stores baby metroid being examined.</summary>
        public const ushort BabyMetroidBeingExamined = 0xcbcd;
        /// <summary>Stores confused baby metroid.</summary>
        public const ushort ConfusedBabyMetroid = 0xcc2b;
        /// <summary>Stores intro text caret.</summary>
        public const ushort IntroTextCaret = 0xcbfb;
        /// <summary>Stores intro text caret blink.</summary>
        public const ushort IntroTextCaretBlink = 0xcc03;
        /// <summary>Stores ceres under attack.</summary>
        public const ushort CeresUnderAttack = 0xcc47;
        /// <summary>Stores metroid egg particle1.</summary>
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
        /// <summary>Stores intro mother brain explosion big.</summary>
        public const ushort IntroMotherBrainExplosionBig = 0xcdab;
        /// <summary>Stores intro mother brain explosion small.</summary>
        public const ushort IntroMotherBrainExplosionSmall = 0xcdcb;
        /// <summary>Stores intro rinka.</summary>
        public const ushort IntroRinka = 0xcdeb;
        /// <summary>Stores delete.</summary>
        public const ushort Delete = 0xce53;
    }

    /// <summary>Named bank-$8C background-object lists consumed by translated intro scenes.</summary>
    public static class BackgroundLists
    {
        /// <summary>Stores samus blinking.</summary>
        public const ushort SamusBlinking = 0xd5df;
        /// <summary>Stores samus blinking page6.</summary>
        public const ushort SamusBlinkingPage6 = 0xd613;
    }
}
