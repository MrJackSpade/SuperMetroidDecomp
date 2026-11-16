namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The geometry and timing of a door's opening scroll in each direction (#2078): how far the camera and the background
/// move per update, where each starts from its endpoint, how many updates move them, and the upward scroll's own camera
/// snap, parallax basis and delayed recalculation of the layer scrolls.
/// </summary>
internal static class DoorOpeningScrollDefinitions
{
    /// <summary>Pixels the camera and the background move per scroll update along the travel axis.</summary>
    internal const int LayerStepPixels = 4;

    /// <summary>Where the camera stands after the directional setup's first call, from the destination screen's origin.</summary>
    internal static readonly IReadOnlyDictionary<DoorDirection, Offset16<RoomPixels>> SetupCameraOffsets =
        new Dictionary<DoorDirection, Offset16<RoomPixels>>
        {
            [DoorDirection.Right] = new(-252, 0),
            [DoorDirection.Left] = new(252, 0),
            [DoorDirection.Down] = new(0, -224),
            [DoorDirection.Up] = new(0, 251),
        };

    /// <summary>
    /// Where the background starts, from its endpoint: as far before it as the setup camera stands before the destination,
    /// except an upward door's, which begins 220 pixels below it. A downward door's first update only readies the row
    /// below the screen.
    /// </summary>
    internal static readonly IReadOnlyDictionary<DoorDirection, Offset16<BackgroundPixels>> SetupBackgroundOffsets =
        new Dictionary<DoorDirection, Offset16<BackgroundPixels>>
        {
            [DoorDirection.Right] = new(-252, 0),
            [DoorDirection.Left] = new(252, 0),
            [DoorDirection.Down] = new(0, -224),
            [DoorDirection.Up] = new(0, 220),
        };

    /// <summary>
    /// The scroll updates that remain after setup: a right or left setup takes the first step before placement (63
    /// remain), down's first update only readies a row (56 remain), and an upward door's placement correction leaves its
    /// first moving step to setup (55 remain).
    /// </summary>
    internal static readonly IReadOnlyDictionary<DoorDirection, int> MovingScrollUpdates = new Dictionary<DoorDirection, int>
    {
        [DoorDirection.Right] = 63,
        [DoorDirection.Left] = 63,
        [DoorDirection.Down] = 56,
        [DoorDirection.Up] = 55,
    };

    /// <summary>
    /// Where the opening's foreground stands from the setup camera: horizontal setups have not yet taken their step, and an
    /// upward door's correction of the source room's top row places the foreground five (the background four) pixels lower.
    /// </summary>
    internal static readonly IReadOnlyDictionary<DoorDirection, Offset16<RoomPixels>> OpeningForegroundOffsets =
        new Dictionary<DoorDirection, Offset16<RoomPixels>>
        {
            [DoorDirection.Right] = new(-4, 0),
            [DoorDirection.Left] = new(4, 0),
            [DoorDirection.Down] = Offset16<RoomPixels>.Zero,
            [DoorDirection.Up] = new(0, 5),
        };

    /// <summary>
    /// How far below its destination screen's origin an upward door's scroll ends the camera: the scroll's completion snaps
    /// it there, while Samus's setup uses the unmodified destination.
    /// </summary>
    internal static readonly Offset16<RoomPixels> UpwardCameraSnap = new(0, 32);

    /// <summary>
    /// Where an upward door's parallax basis lies from the camera's endpoint: one pixel above it, so a parallax room's
    /// background is calculated from the row before the snap's and keeps no one-pixel layer mismatch.
    /// </summary>
    internal static readonly Offset16<RoomPixels> UpwardParallaxBasis = new(0, -1);

    /// <summary>The upward scroll's first updates, which leave the layer scrolls as the setup left them.</summary>
    internal const int UpwardUpdatesBeforeScrollRecalculation = 3;
}
