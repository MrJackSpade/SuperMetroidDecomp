namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Reserve-strip visual identities and import-only bindings.</summary>
public static class PauseReserveTankDefinitions
{
    /// <summary>Six origins, including the trailing cap position.</summary>
    public const int AnchorCount = 6;

    /// <summary>Full tank, trailing cap, empty tank and seven fill levels, as ten stable
    /// frame-name/picture pairs in full, end-cap, empty, then Fill1..Fill7 order.</summary>
    internal static readonly PauseReserveTankFrame[] Frames =
    [
        new("Full", PauseReserveTankVisual.Full),
        new("EndCap", PauseReserveTankVisual.EndCap),
        new("Empty", PauseReserveTankVisual.Empty),
        new("Fill1", PauseReserveTankVisual.Fill1),
        new("Fill2", PauseReserveTankVisual.Fill2),
        new("Fill3", PauseReserveTankVisual.Fill3),
        new("Fill4", PauseReserveTankVisual.Fill4),
        new("Fill5", PauseReserveTankVisual.Fill5),
        new("Fill6", PauseReserveTankVisual.Fill6),
        new("Fill7", PauseReserveTankVisual.Fill7),
    ];

    /// <summary>The partial-tank picture identity for each fill-step count, empty (zero steps) through seven steps.</summary>
    internal static readonly PauseReserveTankVisual[] FillLevels =
    [
        PauseReserveTankVisual.Empty,
        PauseReserveTankVisual.Fill1,
        PauseReserveTankVisual.Fill2,
        PauseReserveTankVisual.Fill3,
        PauseReserveTankVisual.Fill4,
        PauseReserveTankVisual.Fill5,
        PauseReserveTankVisual.Fill6,
        PauseReserveTankVisual.Fill7,
    ];
}
