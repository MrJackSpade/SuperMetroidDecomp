using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Gameplay;

/// <summary>The room's background plane (#1346), refreshed from its level data as the background scrolls.</summary>
public sealed record RoomPlaneBackground : ShownRoomBackground
{
    /// <summary>Keeps the background plane with its pictures.</summary>
    /// <param name="plane">The background plane with its pictures.</param>
    public RoomPlaneBackground(RoomBackgroundLayer plane) => Plane = Ensure.NotNull(plane);

    /// <summary>The background plane with its pictures.</summary>
    internal RoomBackgroundLayer Plane { get; }

    /// <inheritdoc/>
    public override TResult Map<TResult>(IShownRoomBackgroundMapping<TResult> mapping) => Ensure.NotNull(mapping).RoomPlane(Plane);
}
