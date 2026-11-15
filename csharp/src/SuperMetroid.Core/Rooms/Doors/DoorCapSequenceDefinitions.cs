namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The door caps' shared sounds and sequences (#1554): the closing every coloured and grey cap plays behind Samus, and
/// the blue cap's opening, closing and closed image.
/// </summary>
internal static class DoorCapSequenceDefinitions
{
    /// <summary>The sound a cap makes as it starts to open.</summary>
    internal static readonly SoundEffectId OpenSound = RoomSound.DoorOpened;

    /// <summary>The sound a cap makes as it closes behind Samus.</summary>
    internal static readonly SoundEffectId CloseSound = RoomSound.DoorClosed;

    /// <summary>How crowded the sound queue may be for a door cap's sound to join it.</summary>
    internal static readonly SoundRequestPriority SoundPriority = SoundRequestPriority.Normal;

    /// <summary>
    /// A coloured or grey cap closing behind Samus: the cleared opening, then its three opening images in reverse, two
    /// updates each, sounding as the second of them draws, and its closed image for one update.
    /// </summary>
    internal static IReadOnlyList<DoorCapStep> CapClosing { get; } =
    [
        new(DoorCapFrame.Clear, 2),
        new(DoorCapFrame.NearlyOpen, 2),
        new(DoorCapFrame.HalfOpen, 2, CloseSound),
        new(DoorCapFrame.Ajar, 2),
        new(DoorCapFrame.Closed, 1),
    ];

    /// <summary>A shot blue cap opening: its three opening images, six updates each, then the cleared opening for 94.</summary>
    internal static IReadOnlyList<DoorCapStep> BlueOpening { get; } =
    [
        new(DoorCapFrame.Ajar, 6, OpenSound),
        new(DoorCapFrame.HalfOpen, 6),
        new(DoorCapFrame.NearlyOpen, 6),
        new(DoorCapFrame.Clear, 94),
    ];

    /// <summary>
    /// A blue cap closing behind Samus: the cleared opening, then its three opening images in reverse, two updates each,
    /// sounding as the second of them draws.
    /// </summary>
    internal static IReadOnlyList<DoorCapStep> BlueClosing { get; } =
    [
        new(DoorCapFrame.Clear, 2),
        new(DoorCapFrame.NearlyOpen, 2),
        new(DoorCapFrame.HalfOpen, 2, CloseSound),
        new(DoorCapFrame.Ajar, 2),
    ];

    /// <summary>A blue cap closed again: its closed image for one update.</summary>
    internal static IReadOnlyList<DoorCapStep> BlueClosed { get; } = [new(DoorCapFrame.Closed, 1)];
}
