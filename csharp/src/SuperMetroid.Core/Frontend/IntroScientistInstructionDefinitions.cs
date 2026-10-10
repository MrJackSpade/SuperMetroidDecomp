using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>Calculated delivery/examination pulse programs: ten cycles, page request,
/// then continuous pulses. The original examination view also includes caret/arrow
/// neighbors. Native8B:CB9F..CC2A bytes and overlapping reads are independently verified.</summary>
internal static class IntroScientistInstructionDefinitions
{
    /// <summary>$8B:CB9F, delivered baby's animation and page-four request.</summary>
    internal const ushort DeliveryStart = CinematicCodePointers.Lists.BabyMetroidBeingDelivered;
    /// <summary>$8B:CBCD, exclusive end of the delivered-baby program.</summary>
    internal const ushort DeliveryEnd = CinematicCodePointers.Lists.BabyMetroidBeingExamined;
    /// <summary>$8B:CBCD, examined baby's animation and page-five request.</summary>
    internal const ushort ExaminationStart = CinematicCodePointers.Lists.BabyMetroidBeingExamined;
    /// <summary>$8B:CC2B, exclusive end of the examined-baby program.</summary>
    internal const ushort ExaminationEnd = CinematicCodePointers.Lists.ConfusedBabyMetroid;
    /// <summary>$8B:CE53, the shared actor-delete instruction.</summary>
    internal const ushort DeletePointer = CinematicCodePointers.Lists.Delete;

    /// <summary>$8B:CC0F, subtitle-arrow loop included in the original examination view.</summary>
    private const ushort ArrowStart = 0xcc0f;

    private static ushort ActorWord(bool examination, int word)
    {
        ushort start = examination ? ExaminationStart : DeliveryStart;
        if (word == 0) return (ushort)CinematicSpriteInstruction.SetTimer;
        if (word == 1) return 10;
        if (word == 10) return (ushort)CinematicSpriteInstruction.DecrementTimerAndGoto;
        if (word == 11) return (ushort)(start + 4);
        if (word == 12) return examination ? (ushort)IntroScientistBabyInstruction.StartIntroPage5 : (ushort)IntroScientistBabyInstruction.StartIntroPage4;
        if (word == 21) return (ushort)CinematicSpriteInstruction.Goto;
        if (word == 22) return (ushort)(start + 26);
        int displayWord = word - (word < 10 ? 2 : 13);
        if ((displayWord & 1) == 0) return 10;
        int frame = 2 - Math.Abs(2 - displayWord / 2);
        return IntroScientistSpriteDefinitions.FramePointer((examination ? 6 : 3) + frame);
    }

    private static ushort ArrowWord(int word)
    {
        if (word < 8)
            return (word & 1) == 0 ? (ushort)10 : IntroScientistSpriteDefinitions.FramePointer(2 - Math.Abs(2 - word / 2));
        return word switch
        {
            8 or 12 => (ushort)CinematicSpriteInstruction.Goto,
            10 => 60, // blank hold before returning to the ordinary arrow loop
            11 => 0,
            _ => ArrowStart,
        };
    }

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer is >= DeletePointer and < (DeletePointer + 2))
            return IntroBabyDiscoveryInstructionDefinitions.ReadByte(pointer);
        if (pointer is < DeliveryStart or >= ExaminationEnd)
            throw new InvalidDataException($"Intro scientist actor byte $8B:{pointer:X4} leaves its compiled lists.");
        if (pointer is >= IntroCaretInstructionDefinitions.StartPointer and < IntroCaretInstructionDefinitions.EndPointer)
            return IntroCaretInstructionDefinitions.ReadByte(pointer);
        int offset;
        ushort word;
        if (pointer >= ArrowStart)
        {
            offset = pointer - ArrowStart;
            word = ArrowWord(offset / 2);
        }
        else
        {
            bool examination = pointer >= ExaminationStart;
            offset = pointer - (examination ? ExaminationStart : DeliveryStart);
            word = ActorWord(examination, offset / 2);
        }
        return unchecked((byte)(word >> (8 * (offset & 1))));
    }

    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer == DeletePointer)
            return IntroBabyDiscoveryInstructionDefinitions.ReadWord(pointer);
        if (pointer is not (>= DeliveryStart and < (DeliveryEnd - 1)) and
            not (>= ExaminationStart and < (ExaminationEnd - 1)))
            throw new InvalidDataException($"Intro scientist actor word $8B:{pointer:X4} leaves its compiled lists.");
        return (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
    }
}
