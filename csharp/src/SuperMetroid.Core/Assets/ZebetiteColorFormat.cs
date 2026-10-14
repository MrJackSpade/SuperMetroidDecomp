namespace SuperMetroid.Core.Assets;

/// <summary>Presentation geometry of Zebetite palette records at $A6:FD87..FDA6.</summary>
public static class ZebetiteColorFormat
{
    /// <summary>Asset filename for the editable Zebetite barrier pulse colors.</summary>
    public const string FileName = "zebetite-colors.json";
    /// <summary>Supported revision of the eight-row, two-color RGB5 JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Eight pulse rows selected by the native shared counter masked with <c>$0007</c>; these are palette steps rather than independent video-refresh timing data.</summary>
    public const int FrameCount = 8;
    /// <summary>Two adjacent barrier-core colors copied per pulse step to OBJ palette 2 colors 12 and 13.</summary>
    public const int ColorsPerFrame = 2;
    /// <summary>Required frame-by-color dimensions of the editable document.</summary>
    internal static FixedGridShape Shape => new(FrameCount, ColorsPerFrame);
}
