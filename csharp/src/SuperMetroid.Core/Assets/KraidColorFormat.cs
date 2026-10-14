namespace SuperMetroid.Core.Assets;

/// <summary>Installed-resource identity and schema revision for Kraid's five editable color sources.</summary>
public static class KraidColorFormat
{
    /// <summary>JSON resource filename containing Kraid's single-band and flash/health-band RGB5 sources.</summary>
    public const string FileName = "kraid-colors.json";
    /// <summary>Supported schema revision, one, with three sixteen-color sources and two nine-band, 144-color sources.</summary>
    public const int Version = 1;
}
