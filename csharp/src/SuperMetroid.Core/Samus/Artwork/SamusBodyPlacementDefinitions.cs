namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Calculated pose-origin relationships for Samus's landing rows.</summary>
internal static class SamusBodyPlacementDefinitions
{
    /// <summary>$90:8D28..8D37: normal/spin landing, each with right/left four-byte rows.</summary>
    internal const int LandingPoseBytes = 4;
    /// <summary>$90:8D28..8D37: sixteen true landing bytes; $90:8D38 PLB is the separate narrowly retained observed byte.</summary>
    internal const int LandingDataBytes = 4 * LandingPoseBytes;

    /// <summary>$90:8D38: exact PLB instruction encoding observed as the high byte of the final admitted unaligned word.</summary>
    /// <remarks>Only this one byte is retained as unrelated native instruction content; no memory beyond the17-byte window is modeled.</remarks>
    internal const int AdjacentLandingPlb = 0xAB;
}
