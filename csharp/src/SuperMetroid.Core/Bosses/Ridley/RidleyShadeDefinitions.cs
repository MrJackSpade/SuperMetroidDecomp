namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The steps the colours of both Ridleys pass through as they fade in (#1413, #2229). The colours themselves, and their
/// shapes, are the import's.
/// </summary>
public static class RidleyShadeDefinitions
{
    /// <summary>The black-to-visible body shades, one shown every two updates of his body fade.</summary>
    public const int BodyFadeShadeCount = 16;

    /// <summary>The updates both Ridleys wait after their last body shade; on the last they begin their opening roar.</summary>
    public const int BeforeRoarUpdates = 5;
}
