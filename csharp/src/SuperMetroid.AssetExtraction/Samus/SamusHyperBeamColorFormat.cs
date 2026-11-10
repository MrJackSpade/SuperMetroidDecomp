using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.AssetExtraction.Assets;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Installed-resource identity and row geometry for the full-body Hyper Beam RGB5 cycle.</summary>
public static class SamusHyperBeamColorFormat
{
    /// <summary>JSON resource filename containing the ten editable full-body Hyper Beam palettes.</summary>
    public const string FileName = "samus-hyper-beam-colors.json";
    /// <summary>Supported schema revision, one, requiring all ten complete RGB5 palette rows.</summary>
    public const int Version = 1;
    /// <summary>Ten authored palette phases; this count does not specify the number of gameplay updates between phases.</summary>
    public const int FrameCount = SamusPaletteDefinitions.FullBodyCycles.HyperBeamPaletteCount;
    /// <summary>Sixteen colors in each complete OBJ palette, including transparent index zero and fifteen opaque ink slots.</summary>
    public const int ColorsPerFrame = SamusColourSets.ColoursPerSet;
    /// <summary>Required frame-by-color dimensions of the editable document.</summary>
    internal static FixedGridShape Shape => new(FrameCount, ColorsPerFrame);
}
