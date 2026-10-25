namespace SuperMetroid.Core.Spatial;

/// <summary>Verification-only room-screen index of a pixel coordinate (#223): a room screen is 256 pixels.</summary>
[EncodingOwner("room screen index")]
internal static class RoomScreenOf
{
    /// <summary>The screen index containing <paramref name="pixel"/> along one axis.</summary>
    public static int Pixel(int pixel) => pixel >> 8;
}
