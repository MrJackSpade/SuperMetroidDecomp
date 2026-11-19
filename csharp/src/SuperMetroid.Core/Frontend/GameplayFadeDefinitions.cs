namespace SuperMetroid.Core.Frontend;

/// <summary>The cadence of the fade into gameplay and of the main gameplay's own fades (#2575).</summary>
internal static class GameplayFadeDefinitions
{
    /// <summary>
    /// The updates the fade into Landing Site or Ceres arrival waits before each brightness step, the first included: one,
    /// so it steps on every second update.
    /// </summary>
    internal const int FadeInWaitBeforeEachStep = 1;

    /// <summary>The updates each step of the shared screen fade takes once main gameplay begins: one, a step every update.</summary>
    internal const int MainGameplayUpdatesPerStep = 1;
}
