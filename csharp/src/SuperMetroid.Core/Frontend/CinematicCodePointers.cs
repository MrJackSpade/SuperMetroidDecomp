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

    public const ushort PreInstruction_MetroidEgg_DeleteAfterCrossFade = 0xa903;
    public const ushort PreInstruction_IntroMotherBrain_CrossFading = 0xb82e;

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
        public const ushort IntroMotherBrainExplosionBig = 0xcdab;
        public const ushort IntroMotherBrainExplosionSmall = 0xcdcb;
        public const ushort IntroRinka = 0xcdeb;
        public const ushort Delete = 0xce53;
    }

    /// <summary>Named bank-$8C background-object lists consumed by translated intro scenes.</summary>
    public static class BackgroundLists
    {
        public const ushort SamusBlinking = 0xd5df;
        public const ushort SamusBlinkingPage6 = 0xd613;
    }
}
