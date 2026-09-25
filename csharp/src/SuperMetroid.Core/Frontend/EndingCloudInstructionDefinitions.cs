using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The six fixed bank-$8B cloud loops used by both atmospheric ending views.
/// Each eight-byte list displays its own bank-$8C composition for one handler call,
/// then uses the native goto opcode to repeat from the same list address.
/// </summary>
internal static class EndingCloudInstructionDefinitions
{
    /// <summary>$8B:ECED, first upper cloud instruction list.</summary>
    internal const ushort Start = 0xeced;
    /// <summary>$8B:ED1D, exclusive end after the sixth cloud list.</summary>
    internal const ushort End = 0xed1d;
    private const int ListBytes = 8;

    /// <summary>Reads only the complete words belonging to these six bounded lists.</summary>
    internal static ushort ReadWord(ushort pointer)
    {
        int offset = pointer - Start;
        if ((uint)offset >= End - Start || (offset & 1) != 0)
            throw new InvalidDataException(
                $"Ending cloud instruction $8B:{pointer:X4} leaves the compiled lists.");
        int list = offset / ListBytes;
        return (ushort)((offset % ListBytes) switch
        {
            0 => 1,
            2 => EndingCloudSpriteDefinitions.Frames[list].Pointer,
            4 => CinematicCodePointers.CinematicSpriteObject_Instruction_Goto,
            6 => unchecked((ushort)(Start + list * ListBytes)),
            _ => throw new InvalidOperationException("Validated cloud word offset became invalid."),
        });
    }
}
