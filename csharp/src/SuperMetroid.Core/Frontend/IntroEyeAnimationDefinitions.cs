namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Calculated bank-$8C eye programs: two four-phase blinks for pages1..5,
/// then page6 closed/half-open/deadpan phases with a deadpan self-loop.
/// NativeD5DF..D628 durations, fixed position and sixteen-byte frame stride are
/// independently verified, including odd-address word reads. Artwork stays separate.
/// </summary>
internal static class IntroEyeAnimationDefinitions
{
    /// <summary>$8C:D5DF, first Samus portrait eye BG-object instruction.</summary>
    internal const ushort StartPointer = CinematicCodePointers.BackgroundLists.SamusBlinking;
    /// <summary>$8C:D629, exclusive end after the page-six eye loop.</summary>
    internal const ushort EndPointer = 0xd629;
    /// <summary>$8C:D781, first of four 16-byte portrait-eye draw records.</summary>
    internal const ushort FrameStartPointer = 0xd781;
    /// <summary>Four frames referenced by the normal and page-six scripts.</summary>
    internal const int FrameCount = 4;
    /// <summary>Each native record holds a draw function, dimensions and six words.</summary>
    internal const int FrameStride = 0x10;
    /// <summary>Native portrait-eye rectangle width in BG tiles.</summary>
    internal const int FrameColumns = 3;
    /// <summary>Native portrait-eye rectangle height in BG tiles.</summary>
    internal const int FrameRows = 2;
    /// <summary>$8C:D61F, page-six deadpan record and final self-loop target.</summary>
    private const ushort DeadpanLoop = 0xd61f;
    /// <summary>Native BG record position bytes17,13: portrait eye tile column/row.</summary>
    private const ushort PackedPosition = 17 | (13 << 8);

    /// <summary>Frame selectors used by the page-six script after its initial closed-eye hold.</summary>
    private enum EyeFrame
    {
        /// <summary>Uses the partially open portrait-eye draw record.</summary>
        HalfOpen = 1,
        /// <summary>Uses the fully closed portrait-eye draw record.</summary>
        Closed = 2,
        /// <summary>Uses the page-six neutral expression draw record.</summary>
        Deadpan = 3
    }

    /// <summary>Compiles one word of the bank-$8C eye instruction stream from its script position.</summary>
    /// <param name="index">Zero-based word offset from <see cref="StartPointer"/>.</param>
    /// <returns>The duration, packed portrait position, frame pointer, or native goto operand at that position.</returns>
    private static ushort ProgramWord(int index)
    {
        if (index is 24 or 35) return CinematicCodePointers.CinematicBackgroundObject_Instruction_Goto;
        if (index == 25) return StartPointer;
        if (index == 36) return DeadpanLoop;
        int duration, frame, field;
        if (index < 24)
        {
            int record = index / 3, phase = record % 4;
            bool firstBlink = record < 4;
            duration = phase == 0 ? (firstBlink ? 128 : 80) : (firstBlink ? 10 : 8);
            frame = 2 - Math.Abs(2 - phase); // open, halfway, closed, halfway
            field = index % 3;
        }
        else
        {
            int record = (index - 26) / 3;
            (duration, EyeFrame eye) = record switch
            {
                0 => (64, EyeFrame.Closed),
                1 => (8, EyeFrame.HalfOpen),
                _ => (16, EyeFrame.Deadpan),
            };
            frame = (int)eye;
            field = (index - 26) % 3;
        }
        return field == 0 ? (ushort)duration : field == 1 ? PackedPosition
            : (ushort)(FrameStartPointer + FrameStride * frame);
    }

    /// <summary>Reads one little-endian program byte from the compiled eye script's native pointer range.</summary>
    /// <param name="pointer">Bank-$8C address of the byte to read.</param>
    /// <returns>The byte stored at that script address.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The address lies outside the compiled script range.</exception>
    internal static byte ReadByte(ushort pointer)
    {
        if (pointer < StartPointer || pointer >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        int offset = pointer - StartPointer;
        return unchecked((byte)(ProgramWord(offset / 2) >> (8 * (offset & 1))));
    }

    /// <summary>Attempts to read a complete little-endian script word without crossing the compiled program boundary.</summary>
    /// <param name="pointer">Bank-$8C address of the word's first byte.</param>
    /// <param name="word">Receives the decoded word on success, or zero when the address is outside the script.</param>
    /// <returns><see langword="true"/> when the pointer begins a complete word within the script.</returns>
    /// <exception cref="InvalidDataException">The pointer addresses the final byte, so the word would overrun the program.</exception>
    internal static bool TryReadWord(ushort pointer, out ushort word)
    {
        if (pointer < StartPointer || pointer >= EndPointer)
        {
            word = 0;
            return false;
        }
        if (pointer == EndPointer - 1)
            throw new InvalidDataException("Opening eye script read crosses its compiled program boundary.");
        word = (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
        return true;
    }

    /// <summary>Recognizes pointers to the start of one of the four fixed-stride portrait-eye draw records.</summary>
    /// <param name="pointer">Bank-$8C address to check against the draw-record table.</param>
    /// <param name="index">Receives the zero-based frame number when the pointer is an aligned record start.</param>
    /// <returns><see langword="true"/> only for an aligned pointer within the four-record table.</returns>
    internal static bool TryFrameIndex(ushort pointer, out int index)
    {
        int offset = pointer - FrameStartPointer;
        index = offset / FrameStride;
        return offset >= 0 && offset < FrameCount * FrameStride &&
            offset % FrameStride == 0;
    }
}
