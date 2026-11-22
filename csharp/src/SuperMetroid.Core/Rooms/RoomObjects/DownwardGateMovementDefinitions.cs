namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The authored timing of the downward gate (#1554): the open and closed gate each show for one update before the gate
/// rests; closing shows the open gate, then lowers it image by image while its projectile carries the collision down;
/// reopening raises it image by image. The image a movement starts with names that movement (<see cref="DownwardGateImage.Starts"/>).
/// </summary>
internal static partial class DownwardGateMovementDefinitions
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
    internal static DownwardGateImage Open { get; } = new(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.Open), 1);

    /// <summary>The closed gate, shown for one update before it rests closed.</summary>
    internal static DownwardGateImage Closed { get; } = new(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.Closed), 1);

    /// <summary>The closing, in order: the open gate holds, and the lowering starts with the next image.</summary>
    internal static IReadOnlyList<DownwardGateImage> Closing { get; } =
    [
        new(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.Open), CloseDelay),
        new(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.QuarterLowered), MovementFrameDuration, DownwardGateMovement.Lowering),
        new(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.HalfLowered), MovementFrameDuration),
        new(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.ThreeQuartersLowered), MovementFrameDuration),
        new(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.FullyLowered), LastMovementFrameDuration),
    ];

    /// <summary>The reopening, in order: the rising starts with the first image.</summary>
    internal static IReadOnlyList<DownwardGateImage> Reopening { get; } =
    [
        new(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.FullyLowered), MovementFrameDuration, DownwardGateMovement.Rising),
        new(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.ThreeQuartersLowered), MovementFrameDuration),
        new(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.HalfLowered), MovementFrameDuration),
        new(DownwardGateObjectDraws.Family.Of(DownwardGateDraw.QuarterLowered), LastMovementFrameDuration),
    ];
}
