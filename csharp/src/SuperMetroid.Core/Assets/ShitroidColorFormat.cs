namespace SuperMetroid.Core.Assets;

/// <summary>Installed filename and supported schema revision for the encounter's selected pulse and target colors.</summary>
public static class ShitroidColorFormat
{
    /// <summary>Installed editable JSON filename for the eight-frame pulse and three initialization target images.</summary>
    public const string FileName = "shitroid-colors.json";
    /// <summary>Supported color schema revision, requiring the exact native frame and target dimensions.</summary>
    public const int Version = 1;
}
