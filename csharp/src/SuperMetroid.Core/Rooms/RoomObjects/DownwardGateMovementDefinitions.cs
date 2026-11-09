namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The authored timing of the downward gate (#1554): the open and closed gate each show for one update before the gate
/// rests; closing shows the open gate, then lowers it image by image while its projectile carries the collision down;
/// reopening raises it image by image.
/// </summary>
internal static class DownwardGateMovementDefinitions
{
    /// <summary>How long the open gate shows before it starts lowering.</summary>
    internal const int CloseDelay = 16;

    /// <summary>How long each lowering or raising image shows.</summary>
    internal const int MovementFrameDuration = 16;

    /// <summary>How long the last lowering or raising image shows.</summary>
    internal const int LastMovementFrameDuration = 24;

    /// <summary>The movement sound's request priority.</summary>
    internal static readonly SoundRequestPriority MovementSoundPriority = SoundRequestPriority.Normal;

    /// <summary>The open gate, shown for one update before it rests open.</summary>
    internal static RoomObjectDraw OpenImage { get; } = RoomObjectDraw.Of(DownwardGateDraw.Open);

    /// <summary>The closed gate, shown for one update before it rests closed.</summary>
    internal static RoomObjectDraw ClosedImage { get; } = RoomObjectDraw.Of(DownwardGateDraw.Closed);

    /// <summary>The closing, in order; the gate's movement starts with <see cref="ClosingMovementStart"/>.</summary>
    internal static IReadOnlyList<TerrainChangeFrame> Closing { get; } =
    [
        new(RoomObjectDraw.Of(DownwardGateDraw.Open), CloseDelay),
        new(RoomObjectDraw.Of(DownwardGateDraw.Lowered1), MovementFrameDuration),
        new(RoomObjectDraw.Of(DownwardGateDraw.Lowered2), MovementFrameDuration),
        new(RoomObjectDraw.Of(DownwardGateDraw.Lowered3), MovementFrameDuration),
        new(RoomObjectDraw.Of(DownwardGateDraw.Lowered4), LastMovementFrameDuration),
    ];

    /// <summary>The closing image the gate's moving projectile and movement sound start with.</summary>
    internal const int ClosingMovementStart = 1;

    /// <summary>The reopening, in order; the gate's projectile is woken and the movement sound plays with the first image.</summary>
    internal static IReadOnlyList<TerrainChangeFrame> Reopening { get; } =
    [
        new(RoomObjectDraw.Of(DownwardGateDraw.Lowered4), MovementFrameDuration),
        new(RoomObjectDraw.Of(DownwardGateDraw.Lowered3), MovementFrameDuration),
        new(RoomObjectDraw.Of(DownwardGateDraw.Lowered2), MovementFrameDuration),
        new(RoomObjectDraw.Of(DownwardGateDraw.Lowered1), LastMovementFrameDuration),
    ];
}
