namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Calculated pose-origin relationships for Samus's landing rows.</summary>
internal static class SamusBodyPlacementDefinitions
{
    /// <summary>Normal/spin landing, each with right/left four-byte rows.</summary>
    internal const int LandingPoseBytes = 4;
    /// <summary>Sixteen true landing bytes; the byte after them is the separate narrowly retained observed byte.</summary>
    internal const int LandingDataBytes = 4 * LandingPoseBytes;

    /// <summary>The byte after the landing table, observed as the high byte of the final admitted unaligned word.</summary>
    /// <remarks>Only this one byte is retained as unrelated content; nothing beyond the17-byte window is modeled.</remarks>
    internal const int AdjacentLandingPlb = 171;
}
