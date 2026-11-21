namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// The pause screen's reserve-tank strip: how many places it draws a tank at, the pictures its tanks show with the name
/// each is installed under, and the partial tank each fill step shows.
/// </summary>
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

    /// <summary>Each reserve visual's installed frame name, one for every visual.</summary>
    internal static InstalledKeys<PauseReserveTankVisual> InstalledNames { get; } = new(Frames.ToDictionary(frame => frame.Id, frame => frame.Name));

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
