namespace SuperMetroid.Core.Fx;

/// <summary>
/// The scrolling sky as an accepted frame shows it: the room row the sky's background shows at the camera, which follows
/// the camera. The bands' sideways drift is read while the frame is drawn and is not part of this latch.
/// </summary>
public readonly partial record struct LatchedScrollingSky
{
    /// <summary>A value of its parts; only its own operations make one.</summary>
    /// <param name="cameraRow">The room row at the camera when the sky last moved; a camera above the room's top is at a negative row.</param>
    internal LatchedScrollingSky(int cameraRow)
    {
        CameraRow = cameraRow;
    }

    /// <summary>The room row at the camera when the sky last moved; a camera above the room's top is at a negative row.</summary>
    public int CameraRow { get; init; }

    /// <summary>The sky as it stands, or null when the room has no scrolling sky.</summary>
    public static LatchedScrollingSky? Capture(ScrollingSkyState? sky) =>
        sky is null ? null : new(sky.CameraRow);
}
