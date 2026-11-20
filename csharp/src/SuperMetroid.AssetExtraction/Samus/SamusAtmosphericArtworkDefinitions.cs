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
    /// <summary>The cartridge's number of the atmospheric type whose list the footstep splash draws.</summary>
    internal const int FootstepType = 1;
    /// <summary>The cartridge's number of the first atmospheric type whose shared list the lava spray and dust draw.</summary>
    internal const int LavaDustType = 4;
    /// <summary>The cartridge's number of the second atmospheric type that draws the shared lava and dust list.</summary>
    internal const int SharedLavaDustType = 6;
    /// <summary>The cartridge's number of the third atmospheric type that draws the shared lava and dust list.</summary>
    internal const int LastSharedLavaDustType = 7;
}
