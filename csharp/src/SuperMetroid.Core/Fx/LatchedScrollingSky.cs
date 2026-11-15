namespace SuperMetroid.Core.Fx;

/// <summary>
/// The scrolling sky as an accepted frame shows it: the room row the sky's background shows at the camera, which follows
/// the camera. The bands' sideways drift is read while the frame is drawn and is not part of this latch.
/// </summary>
/// <param name="CameraRow">The room row at the camera when the sky last moved; a camera above the room's top is at a negative row.</param>
public readonly record struct LatchedScrollingSky(int CameraRow)
{
    /// <summary>The sky as it stands, or null when the room has no scrolling sky.</summary>
    public static LatchedScrollingSky? Capture(ScrollingSkyState? sky) =>
        sky is null ? null : new(sky.CameraRow);
}
