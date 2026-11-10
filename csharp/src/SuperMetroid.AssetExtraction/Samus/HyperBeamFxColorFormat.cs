using SuperMetroid.AssetExtraction.Assets;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Presentation geometry of the color payloads at $8D:D906..D9CA.</summary>
public static class HyperBeamFxColorFormat
{
    /// <summary>Asset filename for the Hyper Beam projectile palette colors, separate from Samus's full-body Hyper Beam cycle.</summary>
    public const string FileName = "hyper-beam-fx-colors.json";
    /// <summary>Supported revision of the ten-row RGB5 projectile-color JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Ten palette rows in the native <c>$8D:D900</c> cycle: red, yellow, green, blue and magenta endpoints, with intervening rows between them.</summary>
    public const int FrameCount = SamusColourSets.HyperBeamCycleFrames;
    /// <summary>Eight colors per native palette record, written to OBJ palette 6 colors 1..8; control words and frame duration are not included.</summary>
    public const int ColorsPerFrame = SamusColourSets.HyperBeamColours;
    /// <summary>Required frame-by-color dimensions of the editable document.</summary>
    internal static FixedGridShape Shape => new(FrameCount, ColorsPerFrame);
}
