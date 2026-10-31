namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Native displayed-frame identities, independent of the physical offset table.</summary>
public static class GrappleSwingFrameDefinitions
{
    /// <summary>Asset filename for the editable angle-to-displayed-orientation mapping.</summary>
    public const string FileName = "grapple-swing-frames.json";
    /// <summary>Supported revision of the 256-entry displayed-frame JSON schema.</summary>
    public const int Version = 1;
    /// <summary>$9B:C1C2..C2C1: one displayed animation frame for each high angle byte.</summary>
    public const int AngleCount = 256;
    /// <summary>$9B:BD95: Grapple swing art has 32 orientation frames per facing.</summary>
    public const int FrameCount = 32;
    /// <summary>$9B:C1C2 angle selector rounds to the nearest eight-angle frame, wrapping at 32.</summary>
    public static int FrameForAngle(ByteAngle angle) => WordWrap.LowByte(((angle.RawValue + 4) >> 3) & (FrameCount - 1));
}
