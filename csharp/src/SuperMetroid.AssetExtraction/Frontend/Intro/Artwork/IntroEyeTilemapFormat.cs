
using SuperMetroid.Core.Frontend.Intro;

namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Stable identities and rectangle shape of the four portrait eye drawings. The installed cells are the drawings; timing and pixels are outside this retained scope.</summary>
public static partial class IntroEyeTilemapFormat
{
    /// <summary>Supported eye-frame document schema revision.</summary>
    public const int Version = 1;
    /// <summary>Number of ordered eye drawings in the blink selector.</summary>
    public const int FrameCount = 4;
    /// <summary>Number of BG cells across each eye rectangle.</summary>
    public const int Columns = 3;
    /// <summary>Number of BG cells down each eye rectangle.</summary>
    public const int Rows = 2;
    /// <summary>Number of row-major tilemap cells in each frame.</summary>
    public const int CellsPerFrame = Columns * Rows;
    /// <summary>JSON filename containing the four editable eye rectangles.</summary>
    public const string FileName = "intro-samus-eye-frames.json";

    /// <summary>The four eye drawings in the order the document lists them.</summary>
    internal static IReadOnlyList<IntroEyeFrame> Eyes { get; } =
        [IntroEyeFrame.Open, IntroEyeFrame.HalfOpen, IntroEyeFrame.Closed, IntroEyeFrame.Deadpan];

    /// <summary>The document's identity of the drawing of <paramref name="eye"/>, spelled from its installed key.</summary>
    internal static string FrameId(IntroEyeFrame eye) => $"eye-{IntroEyeFrameKeys.Installed.Of(eye)}";
}
