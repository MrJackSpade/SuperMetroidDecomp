namespace SuperMetroid.Core.Spatial;

/// <summary>
/// Where the gameplay screen shows the room and its actors, stated once (#2573): the camera places every actor, an actor
/// at the camera's row showing on the screen's top line, and every actor's picture is drawn to stand a row lower against
/// the room than the room row its place names. So the screen shows the room from the row below the camera's, and actors
/// stand on their floors as their pictures are drawn to.
/// </summary>
public static partial class RoomView
{
    /// <summary>The room row the screen's top line shows against a camera on the signed row <paramref name="cameraRow"/>.</summary>
    public static int ShownRow(int cameraRow) => cameraRow + 1;

    /// <summary>The room pixel the screen's top-left corner shows against a camera at the signed position <paramref name="camera"/>.</summary>
    /// <typeparam name="TSpace">The layer's pixels.</typeparam>
    public static Offset16<TSpace> Shown<TSpace>(Offset16<TSpace> camera)
        where TSpace : ISpatialSpace => camera.WithY(ShownRow(camera.Y));
}
