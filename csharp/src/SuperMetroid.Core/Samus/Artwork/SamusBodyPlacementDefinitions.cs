namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Calculated pose-origin relationships for Samus's landing rows.</summary>
internal static class SamusBodyPlacementDefinitions
{
    /// <summary>Normal/spin landing, each with right/left four-byte rows.</summary>
    internal const int LandingPoseBytes = 4;
    /// <summary>The sixteen landing bytes.</summary>
    internal const int LandingDataBytes = 4 * LandingPoseBytes;
}
