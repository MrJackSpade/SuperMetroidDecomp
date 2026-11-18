namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The steps of Lower Norfair Ridley's arena reveal (#1413, #2409) and how long he waits before his own. The colours
/// themselves, and their shapes, are the import's.
/// </summary>
public static class NorfairRidleyRevealDefinitions
{
    /// <summary>The arena reveal's steps, one shown every third reveal update; the reveal ends after the last.</summary>
    public const int RevealStepCount = 15;

    /// <summary>The updates each step of the arena reveal shows; the reveal ends once the last step's have passed.</summary>
    public const int RevealStepUpdates = 3;

    /// <summary>The updates Lower Norfair Ridley waits once Samus's door has finished; on the last his eyes begin to fade in.</summary>
    public const int RevealDelayUpdates = 171;

    /// <summary>The updates of the hold that starts with his roar: his arena's reveal takes its first step on the hold's first update.</summary>
    public const int BeforeFirstStepUpdates = 1;
}
