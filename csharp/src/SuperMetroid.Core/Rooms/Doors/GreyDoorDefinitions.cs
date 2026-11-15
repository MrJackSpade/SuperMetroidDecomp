namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The grey caps' authored sequences (#1554). A grey cap shows its closed image, flashes while its condition holds and
/// opens at the first shot after; Bomb Torizo's door stays open until Samus has Bombs.
/// </summary>
internal static class GreyDoorDefinitions
{
    /// <summary>A locked cap's closed image, for one update.</summary>
    internal static IReadOnlyList<DoorCapStep> Locked { get; } = [new(DoorCapFrame.Closed, 1)];

    /// <summary>An unlocked cap's flash, which repeats until a shot opens it: the blue image for three updates and the cap for four, three times.</summary>
    internal static IReadOnlyList<DoorCapStep> Flash { get; } =
    [
        new(DoorCapFrame.BlueFlash, 3),
        new(DoorCapFrame.Closed, 4),
        new(DoorCapFrame.BlueFlash, 3),
        new(DoorCapFrame.Closed, 4),
        new(DoorCapFrame.BlueFlash, 3),
        new(DoorCapFrame.Closed, 4),
    ];

    /// <summary>A shot cap opening: its three opening images, four updates each, then the cleared opening for one.</summary>
    internal static IReadOnlyList<DoorCapStep> Opening { get; } =
    [
        new(DoorCapFrame.Ajar, 4, DoorCapSequenceDefinitions.OpenSound),
        new(DoorCapFrame.HalfOpen, 4),
        new(DoorCapFrame.NearlyOpen, 4),
        new(DoorCapFrame.Clear, 1),
    ];

    /// <summary>Bomb Torizo's door held open behind Samus, which repeats until she has Bombs: the cleared opening for two updates.</summary>
    internal static IReadOnlyList<DoorCapStep> BombTorizoHeldOpen { get; } = [new(DoorCapFrame.Clear, 2)];

    /// <summary>
    /// Bomb Torizo's door closing once Samus has Bombs: the cleared opening for forty updates, then its three opening
    /// images in reverse, two updates each, sounding as the first of them draws, and its closed image for one update.
    /// </summary>
    internal static IReadOnlyList<DoorCapStep> BombTorizoClosing { get; } =
    [
        new(DoorCapFrame.Clear, 40),
        new(DoorCapFrame.NearlyOpen, 2, DoorCapSequenceDefinitions.CloseSound),
        new(DoorCapFrame.HalfOpen, 2),
        new(DoorCapFrame.Ajar, 2),
        new(DoorCapFrame.Closed, 1),
    ];
}
