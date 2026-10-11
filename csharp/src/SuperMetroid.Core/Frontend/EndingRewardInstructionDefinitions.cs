using SuperMetroid.Core.Assets;
using Frame = SuperMetroid.Core.Assets.EndingRewardSpriteFrame;

namespace SuperMetroid.Core.Frontend;

/// <summary>Post-credits reward display stages and jump/shot control flow.</summary>
internal static class EndingRewardInstructionDefinitions
{
    /// <summary>$8B:ED1D, suitless idle list.</summary>
    internal const ushort Start = 0xed1d;
    /// <summary>$8B:EE5D, exclusive end after the flight head lists.</summary>
    internal const ushort End = 0xee5d;
    private enum List : ushort
    {
        /// <summary>$8B:ED1D, SuitlessIdle actor instruction list.</summary>
        SuitlessIdle = 0xed1d,
        /// <summary>$8B:ED25, SuitlessLegsIdle actor instruction list.</summary>
        SuitlessLegsIdle = 0xed25,
        /// <summary>$8B:ED2D, HairRelease actor instruction list.</summary>
        HairRelease = 0xed2d,
        /// <summary>$8B:ED59, HairReleaseLegs actor instruction list.</summary>
        HairReleaseLegs = 0xed59,
        /// <summary>$8B:ED7F, SuitlessJump actor instruction list.</summary>
        SuitlessJump = 0xed7f,
        /// <summary>$8B:ED95, Falling actor instruction list.</summary>
        Falling = 0xed95,
        /// <summary>$8B:ED9D, Landing actor instruction list.</summary>
        Landing = 0xed9d,
        /// <summary>$8B:EDB1, SuitedIdle actor instruction list.</summary>
        SuitedIdle = 0xedb1,
        /// <summary>$8B:EDB9, HelmetIdle actor instruction list.</summary>
        HelmetIdle = 0xedb9,
        /// <summary>$8B:EDC1, HeadIdle actor instruction list.</summary>
        HeadIdle = 0xedc1,
        /// <summary>$8B:EDC9, GestureBody actor instruction list.</summary>
        GestureBody = 0xedc9,
        /// <summary>$8B:EDD3, GestureArm actor instruction list.</summary>
        GestureArm = 0xedd3,
        /// <summary>$8B:EE0F, GestureHelmet actor instruction list.</summary>
        GestureHelmet = 0xee0f,
        /// <summary>$8B:EE15, GestureHead actor instruction list.</summary>
        GestureHead = 0xee15,
        /// <summary>$8B:EE27, SuitedJump actor instruction list.</summary>
        SuitedJump = 0xee27,
        /// <summary>$8B:EE3D, HelmetJump actor instruction list.</summary>
        HelmetJump = 0xee3d,
        /// <summary>$8B:EE4D, HeadJump actor instruction list.</summary>
        HeadJump = 0xee4d,
    }

    internal static ushort ReadWord(ushort pointer)
    {
        int offset = pointer - Start;
        if ((uint)offset >= End - Start || (offset & 1) != 0)
            throw new InvalidDataException($"Ending reward instruction $8B:{pointer:X4} leaves its compiled lists.");
        if (pointer < (ushort)List.HairRelease)
        {
            bool legs = pointer >= (ushort)List.SuitlessLegsIdle;
            return Hold(pointer, legs ? List.SuitlessLegsIdle : List.SuitlessIdle, 128,
                legs ? Frame.SuitlessSamusLowerBody : Frame.SuitlessSamusStandingArmsStraight);
        }
        if (pointer < (ushort)List.SuitlessJump)
        {
            bool legs = pointer >= (ushort)List.HairReleaseLegs;
            int word = Word(pointer, legs ? List.HairReleaseLegs : List.HairRelease);
            int displayWords = legs ? 18 : 20;
            if (word < displayWords)
            {
                var display = HairDisplays[word / 2];
                return DisplayWord(word, display.Duration, legs ? Frame.SuitlessSamusLowerBody : display.Frame);
            }
            return !legs && word == displayWords ? (ushort)EndingRewardGestureInstruction.SpawnSuitlessJump : Delete;
        }
        if (pointer < (ushort)List.Falling) return SuitlessJumpWord(Word(pointer, List.SuitlessJump));
        if (pointer < (ushort)List.Landing) return Hold(pointer, List.Falling, 10, Frame.SamusFalling);
        if (pointer < (ushort)List.SuitedIdle) return LandingWord(Word(pointer, List.Landing));
        if (pointer < (ushort)List.HelmetIdle) return Hold(pointer, List.SuitedIdle, 10, Frame.LargeSamusFromEndingStanding);
        if (pointer < (ushort)List.HeadIdle) return Hold(pointer, List.HelmetIdle, 10, Frame.SamusHeadWithHelmetFromEnding);
        if (pointer < (ushort)List.GestureBody) return Hold(pointer, List.HeadIdle, 10, Frame.SamusHeadFromEndingFrame1);
        if (pointer < (ushort)List.GestureArm)
        {
            int word = Word(pointer, List.GestureBody);
            return word < 2 ? DisplayWord(word, 64, Frame.LargeSamusFromEndingStanding) :
                word < 4 ? DisplayWord(word, 264, Frame.HeadlessArmlessSuitedSamus) : Delete;
        }
        if (pointer < (ushort)List.GestureHelmet)
        {
            int word = Word(pointer, List.GestureArm);
            if (word >= 2 * ArmDisplays.Length) return word == 2 * ArmDisplays.Length ? (ushort)EndingRewardGestureInstruction.SpawnSuitedJump : Delete;
            var display = ArmDisplays[word / 2];
            return DisplayWord(word, display.Duration, display.Frame);
        }
        if (pointer < (ushort)List.GestureHead)
        {
            int word = Word(pointer, List.GestureHelmet);
            return word < 2 ? DisplayWord(word, 328, Frame.SamusHeadWithHelmetFromEnding) : Delete;
        }
        if (pointer < (ushort)List.SuitedJump)
        {
            int word = Word(pointer, List.GestureHead);
            if (word == 8) return Delete;
            // Four consecutive two-part OAM records turn the helmetless head.
            int stage = word / 2;
            return DisplayWord(word, (ushort)(stage == 0 ? 128 : stage == 3 ? 190 : 5),
                EndingRewardSpriteFrameSeries.HelmetlessHead[stage]);
        }
        if (pointer < (ushort)List.HelmetJump) return SuitedJumpWord(Word(pointer, List.SuitedJump));
        bool helmet = pointer < (ushort)List.HeadJump;
        List head = helmet ? List.HelmetJump : List.HeadJump;
        int headWord = Word(pointer, head);
        if (headWord < 2) return DisplayWord(headWord, 10,
            helmet ? Frame.LargeSamusHelmetFromEndingFrame1 : Frame.JumpingSamusHeadFromEnding);
        if (headWord == 2) return SetPreInstruction;
        if (headWord == 3) return EndingRewardJumpDefinitions.HeadFlight;
        return HoldWord(headWord - 4, (ushort)((ushort)head + 8), 5,
            helmet ? Frame.LargeSamusHelmetFromEndingFrame2 : Frame.JumpingSamusHeadFromEnding);
    }

    /// <summary>Hair-release display records in list order: standing, eight opening frames, ready.</summary>
    private static readonly (ushort Duration, Frame Frame)[] HairDisplays =
    [
        (90, Frame.SuitlessSamusStandingArmsStraight),
        (8, Frame.SuitlessSamusOpeningHairFrame1),
        (10, Frame.SuitlessSamusOpeningHairFrame2),
        (10, Frame.SuitlessSamusOpeningHairFrame3),
        (32, Frame.SuitlessSamusOpeningHairFrame4),
        (10, Frame.SuitlessSamusOpeningHairFrame5),
        (9, Frame.SuitlessSamusOpeningHairFrame6),
        (16, Frame.SuitlessSamusOpeningHairFrame7),
        (10, Frame.SuitlessSamusOpeningHairFrame8),
        (48, Frame.SuitlessSamusStanding),
    ];

    /// <summary>Gesture-arm display records in list order: wait (no arm), raise, thumb up,
    /// turn, lower and rest.</summary>
    private static readonly (ushort Duration, Frame? Frame)[] ArmDisplays =
    [
        (64, null),
        (8, Frame.SamusArmFromEndingFrame1),
        (8, Frame.SamusArmFromEndingFrame2),
        (5, Frame.SamusArmFromEndingFrame3),
        (4, Frame.SamusArmFromEndingFrame4),
        (3, Frame.SamusArmFromEndingFrame5),
        (32, Frame.SamusArmFromEndingFrame6),
        (8, Frame.SamusArmFromEndingFrame7),
        (64, Frame.SamusArmFromEndingFrame8),
        (5, Frame.SamusArmFromEndingFrame6),
        (5, Frame.SamusArmFromEndingFrame5),
        (5, Frame.SamusArmFromEndingFrame4),
        (5, Frame.SamusArmFromEndingFrame3),
        (112, Frame.SamusArmFromEndingFrame2),
    ];

    private static ushort SuitlessJumpWord(int word)
    {
        if (word < 2) return DisplayWord(word, 48, Frame.SuitlessSamusStanding);
        if (word < 4) return DisplayWord(word, 10, Frame.SuitlessSamusPreparingToJump);
        if (word == 4) return (ushort)EndingRewardJumpInstruction.Launch;
        if (word == 5) return SetPreInstruction;
        if (word == 6) return EndingRewardJumpDefinitions.BodyFlight;
        return HoldWord(word - 7, (ushort)((ushort)List.SuitlessJump + 14), 48, Frame.SuitlessSamusJumping);
    }

    private static ushort SuitedJumpWord(int word)
    {
        if (word == 0) return (ushort)EndingRewardJumpInstruction.PrepareHead;
        if (word < 3) return DisplayWord(word - 1, 10, Frame.LargeSamusFromEndingPreparingToJump);
        if (word == 3) return (ushort)EndingRewardJumpInstruction.LaunchHead;
        if (word == 4) return (ushort)EndingRewardJumpInstruction.Launch;
        if (word == 5) return SetPreInstruction;
        if (word == 6) return EndingRewardJumpDefinitions.BodyFlight;
        return HoldWord(word - 7, (ushort)((ushort)List.SuitedJump + 14), 5, Frame.LargeSamusFromEndingJumping);
    }

    private static ushort LandingWord(int word)
    {
        if (word < 2) return DisplayWord(word, 10, Frame.SamusLanding);
        if (word < 4) return DisplayWord(word, 16, Frame.SamusLanded);
        if (word < 6) return DisplayWord(word, 48, Frame.SamusShooting);
        if (word == 6) return (ushort)EndingRewardJumpInstruction.Shoot;
        return word < 9 ? DisplayWord(word - 7, 128, Frame.SamusShooting) : Delete;
    }

    private static int Word(ushort pointer, List list) => (pointer - (ushort)list) / 2;
    private static ushort DisplayWord(int word, ushort duration, Frame? frame) => (word & 1) == 0
        ? duration : frame.HasValue ? EndingRewardSpriteDefinitions.FramePointer(frame.Value) : (ushort)0;
    private static ushort Hold(ushort pointer, List start, ushort duration, Frame frame) => HoldWord(Word(pointer, start), (ushort)start, duration, frame);
    private static ushort HoldWord(int word, ushort start, ushort duration, Frame frame) => word < 2
        ? DisplayWord(word, duration, frame) : word == 2 ? (ushort)CinematicSpriteInstruction.Goto : start;
    private const ushort Delete = (ushort)CinematicSpriteInstruction.Delete;
    private const ushort SetPreInstruction = (ushort)CinematicSpriteInstruction.SetPreInstruction;
}
