using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Immutable bank-$8B sprite instructions for the intro Mother Brain actor.
/// The first list loops four visual frames; the second begins at $CB19 and
/// redirects control to page two. The egg actor's separate list starts at $CB33.
/// </summary>
internal static class IntroMotherBrainInstructionDefinitions
{
    /// <summary>$8B:CB05, first normal animation record.</summary>
    internal const ushort StartPointer = CinematicCodePointers.Lists.IntroMotherBrain;
    /// <summary>$8B:CB33, exclusive end before the Metroid-egg instruction list.</summary>
    internal const ushort EndPointer = CinematicCodePointers.Lists.MetroidEgg;
    /// <summary>$8B:CB1F, first frame record after the page-two handoff callbacks.</summary>
    internal const ushort PageTwoLoopPointer = 0xcb1f;

    private static ushort ProgramWord(int word)
    {
        if (word is >= 10 and <= 12)
            return word switch
            {
                10 => CinematicCodePointers.Instruction_StartIntroPage2,
                11 => CinematicCodePointers.CinematicSpriteObject_Instruction_SetPreInstruction,
                _ => CinematicCodePointers.PreInstruction_IntroMotherBrain_CrossFading,
            };
        bool pageTwo = word >= 13;
        int field = pageTwo ? word - 13 : word;
        if (field < 8)
        {
            int phase = field / 2;
            return (field & 1) == 0 ? (ushort)16
                : IntroMotherBrainSpriteDefinitions.FramePointer(phase <= 2 ? phase : 4 - phase);
        }
        return field == 8 ? CinematicCodePointers.CinematicSpriteObject_Instruction_Goto
            : pageTwo ? PageTwoLoopPointer : StartPointer;
    }

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer is < StartPointer or >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        int offset = pointer - StartPointer;
        return (byte)(ProgramWord(offset / 2) >> (8 * (offset & 1)));
    }

    internal static ushort ReadWord(ushort pointer)
    {
        if (pointer is < StartPointer or >= (EndPointer - 1))
            throw new InvalidDataException(
                $"Intro Mother Brain instruction read $8B:{pointer:X4} leaves its compiled program.");
        return (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
    }
}