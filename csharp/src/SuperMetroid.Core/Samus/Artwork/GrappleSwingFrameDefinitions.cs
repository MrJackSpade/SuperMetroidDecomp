namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Displayed-frame identities, independent of the physical offset table.</summary>
public static class GrappleSwingFrameDefinitions
{
    /// <summary>Grapple swing art has 32 orientation frames per facing.</summary>
    public const int FrameCount = 32;
    /// <summary>The angle selector rounds to the nearest eight-angle frame, wrapping at 32.</summary>
    public static int FrameForAngle(ByteAngle angle) => WordWrap.LowByte(((angle.RawValue + 4) >> 3) & (FrameCount - 1));
}
