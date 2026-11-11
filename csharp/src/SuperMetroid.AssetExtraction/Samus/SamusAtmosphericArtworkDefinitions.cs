namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The atmospheric effects' frame counts and the pose-half entries whose compositions picture the splash and bubbles.</summary>
internal static class SamusAtmosphericArtworkDefinitions
{
    /// <summary>Four words per authored direct small-OBJ effect list.</summary>
    internal const int DirectAtmosphericFrameCount = 4;
    /// <summary>The pose-half table entry ($92:808D index $18F) whose composition is the diving splash's first frame; later frames follow it.</summary>
    internal const int DivingSplashFirstEntry = 399;
    /// <summary>The pose-half table entry ($92:808D index $186) whose composition is the bubbles' first frame; later frames follow it.</summary>
    internal const int BubblesFirstEntry = 390;
}
