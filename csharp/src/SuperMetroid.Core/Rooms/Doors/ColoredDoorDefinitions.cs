namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The coloured caps' authored data (#1554): how many hits each colour takes, the counter a Super Missile leaves on a red
/// cap, and each colour and facing's sequences. Every cap plays the same shape; the yellow downward cap waits on its
/// closed image twice, the yellow left cap opens slowly and lingers open, and the red cap sounds each hit.
/// </summary>
internal static class ColoredDoorDefinitions
{
    /// <summary>The sound each hit on a red cap makes as its flash starts.</summary>
    internal static readonly SoundEffectId RedHitSound = RoomSound.MissileDoorHit;

    /// <summary>The hit count a Super Missile leaves on a red cap: the hit it then counts passes any threshold.</summary>
    internal const int RedSuperMissileHitCounter = 119;

    /// <summary>The hits each colour's cap takes before it opens.</summary>
    internal static IReadOnlyDictionary<ColoredDoorColor, int> HitThresholds { get; } = new Dictionary<ColoredDoorColor, int>
    {
        [ColoredDoorColor.Yellow] = 1,
        [ColoredDoorColor.Green] = 1,
        [ColoredDoorColor.Red] = 5,
    };

    /// <summary>A waiting cap's closed image, for one update.</summary>
    private static readonly DoorCapStep[] Waiting = [new(DoorCapFrame.Closed0, 1)];

    /// <summary>The yellow downward cap's closed image, for two updates and then one.</summary>
    private static readonly DoorCapStep[] YellowDownWaiting = [new(DoorCapFrame.Closed0, 2), new(DoorCapFrame.Closed0, 1)];

    /// <summary>A hit short of opening: the blue image for three updates and the cap for four, three times.</summary>
    private static readonly DoorCapStep[] Flash =
    [
        new(DoorCapFrame.BlueFlash, 3),
        new(DoorCapFrame.Closed0, 4),
        new(DoorCapFrame.BlueFlash, 3),
        new(DoorCapFrame.Closed0, 4),
        new(DoorCapFrame.BlueFlash, 3),
        new(DoorCapFrame.Closed0, 4),
    ];

    /// <summary>A hit on a red cap short of opening: <see cref="Flash"/>, sounding as it starts.</summary>
    private static readonly DoorCapStep[] RedFlash =
    [
        new(DoorCapFrame.BlueFlash, 3, RedHitSound),
        new(DoorCapFrame.Closed0, 4),
        new(DoorCapFrame.BlueFlash, 3),
        new(DoorCapFrame.Closed0, 4),
        new(DoorCapFrame.BlueFlash, 3),
        new(DoorCapFrame.Closed0, 4),
    ];

    /// <summary>A cap opening: its three opening images, six updates each, then the cleared opening for one.</summary>
    private static readonly DoorCapStep[] Opening =
    [
        new(DoorCapFrame.Closed1, 6, DoorCapSequenceDefinitions.OpenSound),
        new(DoorCapFrame.Closed2, 6),
        new(DoorCapFrame.Closed3, 6),
        new(DoorCapFrame.Clear, 1),
    ];

    /// <summary>The yellow left cap opening: its three opening images, four updates each, then the cleared opening for 92.</summary>
    private static readonly DoorCapStep[] YellowLeftOpening =
    [
        new(DoorCapFrame.Closed1, 4, DoorCapSequenceDefinitions.OpenSound),
        new(DoorCapFrame.Closed2, 4),
        new(DoorCapFrame.Closed3, 4),
        new(DoorCapFrame.Clear, 92),
    ];

    /// <summary>Each colour and facing's sequences.</summary>
    internal static IReadOnlyDictionary<ColoredDoorVariant, ColoredDoorSequences> Sequences { get; } =
        new Dictionary<ColoredDoorVariant, ColoredDoorSequences>
    {
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left)] = new(Waiting, Flash, YellowLeftOpening),
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right)] = new(Waiting, Flash, Opening),
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up)] = new(Waiting, Flash, Opening),
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down)] = new(YellowDownWaiting, Flash, Opening),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Left)] = new(Waiting, Flash, Opening),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Right)] = new(Waiting, Flash, Opening),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Up)] = new(Waiting, Flash, Opening),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Down)] = new(Waiting, Flash, Opening),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Left)] = new(Waiting, RedFlash, Opening),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Right)] = new(Waiting, RedFlash, Opening),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Up)] = new(Waiting, RedFlash, Opening),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Down)] = new(Waiting, RedFlash, Opening),
    };
}
