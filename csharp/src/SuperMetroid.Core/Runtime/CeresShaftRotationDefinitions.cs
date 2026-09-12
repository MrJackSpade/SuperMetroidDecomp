namespace SuperMetroid.Core.Runtime;

/// <summary>Immutable timer and Mode 7 records authored for the Ceres escape shaft.</summary>
public static class CeresShaftRotationDefinitions
{
    /// <summary>$89:AD5F, RoomCode_CeresElevatorShaft timer/sine/cosine records.</summary>
    public const int ReferenceAddress = 0x89ad5f;

    /// <summary>$89:AD5F-$AEFC contains 69 records, from sine -34 through +34.</summary>
    public const int RecordCount = 69;

    /// <summary>Native record timers ordered by absolute sine magnitude, zero through 34.</summary>
    private static ReadOnlySpan<ushort> Timers =>
    [
        1, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 5,
        5, 6, 6, 7, 7, 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 60,
    ];

    /// <summary>Resolves the native wrapping six-byte offset, including encoded reverse phases.</summary>
    public static (ushort Timer, ushort Sine, ushort Cosine) Read(ushort phase)
    {
        // Multiplication wraps before indexing: $8044 selects record 68, not a
        // negative index. Reject non-record offsets instead of inventing ROM data.
        int offset = unchecked((ushort)(6 * phase));
        int record = offset / 6;
        if (offset % 6 != 0 || record >= RecordCount)
            throw new InvalidDataException($"Ceres shaft rotation phase ${phase:X4} does not select an authored record.");
        int sine = record - 34;
        int magnitude = Math.Abs(sine);
        ushort cosine = (ushort)(magnitude <= 16 ? 256 : magnitude <= 27 ? 255 : 254);
        return (Timers[magnitude], unchecked((ushort)sine), cosine);
    }
}
