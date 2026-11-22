namespace SuperMetroid.Core.Rooms.Doors;

/// <summary>
/// The geometry and timing of a door's opening scroll in each direction (#2078): how far the camera and the background
/// move per update, where each starts from its endpoint, how many updates move them, and the upward scroll's own camera
/// snap, parallax basis and delayed recalculation of the layer scrolls.
/// </summary>
internal static partial class DoorOpeningScrollDefinitions
{
    /// <summary>Pixels the camera and the background move per scroll update along the travel axis.</summary>
    internal const int LayerStepPixels = 4;

    /// <summary>Where the camera stands after the directional setup's first update, from the destination screen's origin.</summary>
    internal static readonly IdentityTable<DoorDirection, Offset16<RoomPixels>> SetupCameraOffsets =
        new(new Dictionary<DoorDirection, Offset16<RoomPixels>>
        {
            [DoorDirection.Right] = new(-252, 0),
            [DoorDirection.Left] = new(252, 0),
            [DoorDirection.Down] = new(0, -224),
            [DoorDirection.Up] = new(0, 251),
        });

    /// <summary>
    /// Where the background starts, from its endpoint: as far before it as the setup camera stands before the destination,
    /// except an upward door's, which begins nearly a screen below it. A downward door's first update only readies the row
    /// below the screen.
    /// </summary>
    internal static readonly IdentityTable<DoorDirection, Offset16<BackgroundPixels>> SetupBackgroundOffsets =
        new(new Dictionary<DoorDirection, Offset16<BackgroundPixels>>
        {
            [DoorDirection.Right] = new(-252, 0),
            [DoorDirection.Left] = new(252, 0),
            [DoorDirection.Down] = new(0, -224),
            [DoorDirection.Up] = new(0, 220),
        });

    /// <summary>
    /// The scroll updates that remain after setup: a right or left setup takes the first step before placement, down's
    /// first update only readies a row, and an upward door's placement correction leaves its first moving step to setup.
    /// </summary>
    internal static readonly IdentityTable<DoorDirection, int> MovingScrollUpdates = new(new Dictionary<DoorDirection, int>
    {
        [DoorDirection.Right] = 63,
        [DoorDirection.Left] = 63,
        [DoorDirection.Down] = 56,
        [DoorDirection.Up] = 55,
    });

    /// <summary>
    /// Where the opening's foreground stands from the setup camera: horizontal setups have not yet taken their step, and an
    /// upward door's correction of the source room's top row places the foreground (and the background) a few pixels lower.
    /// </summary>
    internal static readonly IdentityTable<DoorDirection, Offset16<RoomPixels>> OpeningForegroundOffsets =
        new(new Dictionary<DoorDirection, Offset16<RoomPixels>>
        {
            [DoorDirection.Right] = new(-4, 0),
            [DoorDirection.Left] = new(4, 0),
            [DoorDirection.Down] = Offset16<RoomPixels>.Zero,
            [DoorDirection.Up] = new(0, 5),
        });

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

    /// <summary>
    /// The block of its screen, the second, from which the doorway nudge moves a coordinate on past its far edge, off the
    /// screen's near doorway.
    /// </summary>
    internal const int DoorwayNudgeNearBlock = 1;

    /// <summary>
    /// The block of its screen, the fifteenth, from which the doorway nudge moves a coordinate back before its near edge,
    /// off the screen's far doorway.
    /// </summary>
    internal const int DoorwayNudgeFarBlock = 14;

    /// <summary>The pixels the doorway nudge places a coordinate past or before its block.</summary>
    internal const int DoorwayNudgePixels = 8;

    /// <summary>
    /// The pixel within its screen from which the door's camera alignment moves the camera on to the next screen, the
    /// screen's middle; a camera before it moves back to its own screen's edge.
    /// </summary>
    internal const int CameraAlignmentNextScreenFrom = 128;


    /// <summary>The updates of the door's scroll that ran while its destination loaded: one, the loading update's own.</summary>
    internal const int LoadingScrollUpdates = 1;
}
