using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Rooms.Scrolling;

/// <summary>
/// How far ahead of Samus the room's camera aims horizontally (#1874). Every room starts with normal tracking; Kraid
/// and Crocomire request boss tracking for their fights, Crocomire the right edge while his skeleton waits behind the
/// wall, and both normal tracking again once they are done.
/// </summary>
[RoomLifetime]
public sealed class RoomCameraDistance
{
    /// <summary>The distance mode the camera aims by.</summary>
    public CameraDistanceMode Mode { get; private set; } = CameraDistanceMode.NormalTracking;

    /// <summary>Has the camera aim by <paramref name="mode"/> from its next horizontal tracking on.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="mode"/> is not a distance mode.</exception>
    internal void Request(CameraDistanceMode mode) =>
        Mode = Enum.IsDefined(mode) ? mode : throw new ArgumentOutOfRangeException(nameof(mode), mode, "Undefined camera distance mode.");
}
