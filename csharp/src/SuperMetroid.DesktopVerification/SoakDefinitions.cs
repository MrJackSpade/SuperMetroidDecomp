/// <summary>Fixed acceptance lengths for the long-running timer and audio soaks.</summary>
internal static class SoakDefinitions
{
    /// <summary>The five-minute per-scene length the renderer migration and #321 acceptance used.</summary>
    public const int SecondsPerScene = 300;

    /// <summary>The short per-scene length the published-desktop gate uses to prove the packaged timer path.</summary>
    public const int PublishSmokeSecondsPerScene = 5;
}
