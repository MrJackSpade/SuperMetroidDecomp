namespace SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

/// <summary>The extent of the Ceres approach's OBJ characters.</summary>
public static class CeresFlightArtworkFormat
{
    /// <summary>Exact planar 4-bpp character length, 16384 bytes for 512 characters at 32 bytes each.</summary>
    public const int ObjectByteCount = 512 * ObjectCharacterBytes;
    /// <summary>The planar bytes of one 4-bpp OBJ character.</summary>
    public const int ObjectCharacterBytes = 32;
}
