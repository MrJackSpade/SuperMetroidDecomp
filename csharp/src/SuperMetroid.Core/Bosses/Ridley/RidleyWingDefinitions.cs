namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How fast Ridley's wings beat for the speed he flies at, and where each Ridley's wing beat starts (#2232).</summary>
internal static class RidleyWingDefinitions
{
    /// <summary>
    /// The frames each update takes off the wing animation timer, slowest flight first; a descending or level flight
    /// halves the selected step.
    /// </summary>
    internal static readonly int[] AnimationTimerSteps = [12, 14, 16, 18, 28, 32, 40, 48];

    /// <summary>The wing countdown each beat starts from: the beat steps once the countdown runs below zero.</summary>
    internal const int BeatCountdown = 32;

    /// <summary>The wing countdown, and its step, as Ridley takes off, before his speed selects the step.</summary>
    internal const int TakeOffCountdown = 8;

    /// <summary>The step of his wing beat Ceres Ridley's wings show first.</summary>
    internal const int CeresFirstBeatStep = 5;

    /// <summary>The step of his wing beat Lower Norfair Ridley's wings show first.</summary>
    internal const int NorfairFirstBeatStep = 0;
}
