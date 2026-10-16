namespace SuperMetroid.Core.Samus;

/// <summary>The stored-word boundary of <see cref="ReserveTankMode"/>.</summary>
public static class ReserveTankModes
{
    /// <summary>A stored word as a reserve mode; any other word is corrupt.</summary>
    public static ReserveTankMode Decode(ushort word) => Enum.IsDefined((ReserveTankMode)word)
        ? (ReserveTankMode)word
        : throw new InvalidDataException($"Reserve-tank mode word ${word:X4} is not none, AUTO or MANUAL.");

    /// <summary>The stored word of a reserve mode.</summary>
    public static ushort ToNativeWord(this ReserveTankMode mode) => (ushort)mode;
}
