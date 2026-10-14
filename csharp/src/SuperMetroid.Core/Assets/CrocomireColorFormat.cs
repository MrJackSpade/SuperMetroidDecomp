namespace SuperMetroid.Core.Assets;

/// <summary>Installed filename and supported schema revision for Crocomire's bounded editable palette-transfer payloads.</summary>
public static class CrocomireColorFormat
{
    /// <summary>Installed editable JSON filename for fight-body, initial-wall/projectile, skeleton-arm, and wall-spike colors.</summary>
    public const string FileName = "crocomire-colors.json";
    /// <summary>Supported color schema revision, requiring transfer lengths of 8, 17, 17, 16, and 16 RGB5 words.</summary>
    public const int Version = 1;
}
