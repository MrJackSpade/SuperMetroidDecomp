using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus;

/// <summary>The stored-word boundary of <see cref="ReserveTankMode"/>.</summary>
public static class ReserveTankModes
{
    /// <summary>A stored word as a reserve mode; any other word is corrupt.</summary>
    public static ReserveTankMode Decode(ushort word) => word switch
    {
        0 => ReserveTankMode.None,
        1 => ReserveTankMode.Auto,
        2 => ReserveTankMode.Manual,
        _ => throw new InvalidDataException($"Reserve-tank mode word ${word:X4} is not none, AUTO or MANUAL."),
    };

    /// <summary>The stored word of a reserve mode.</summary>
    public static ushort ToNativeWord(this ReserveTankMode mode) => mode switch
    {
        ReserveTankMode.None => 0,
        ReserveTankMode.Auto => 1,
        ReserveTankMode.Manual => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Undefined reserve-tank mode."),
    };
}
