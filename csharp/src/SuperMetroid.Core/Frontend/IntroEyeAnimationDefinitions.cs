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

    private enum EyeFrame { HalfOpen = 1, Closed = 2, Deadpan = 3 }

    private static ushort ProgramWord(int index)
    {
        if (index is 24 or 35) return (ushort)CinematicBackgroundInstruction.Goto;
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

    internal static byte ReadByte(ushort pointer)
    {
        if (pointer is < StartPointer or >= EndPointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        int offset = pointer - StartPointer;
        return unchecked((byte)(ProgramWord(offset / 2) >> (8 * (offset & 1))));
    }

    internal static bool TryReadWord(ushort pointer, out ushort word)
    {
        if (pointer is < StartPointer or >= EndPointer)
        {
            word = 0;
            return false;
        }
        if (pointer == EndPointer - 1)
            throw new InvalidDataException("Opening eye script read crosses its compiled program boundary.");
        word = (ushort)(ReadByte(pointer) | ReadByte((ushort)(pointer + 1)) << 8);
        return true;
    }

    internal static bool TryFrameIndex(ushort pointer, out int index)
    {
        int offset = pointer - FrameStartPointer;
        index = offset / FrameStride;
        return offset >= 0 && offset < FrameCount * FrameStride &&
            offset % FrameStride == 0;
    }
}
