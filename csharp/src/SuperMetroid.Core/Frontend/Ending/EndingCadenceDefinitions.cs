namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The counter cadences of the ending (#1606): the turning floor, the departing planet, the explosion's fade and the reward's colours.</summary>
internal static class EndingCadenceDefinitions
{
    /// <summary>Updates each step of the escape scenes' and the credits' fades takes: one level every second update.</summary>
    internal const int FastFadeUpdatesPerStep = 2;

    /// <summary>Updates the post-credits fade-in waits after the blank hold before its first step.</summary>
    internal const int PostCreditsFadeFirstWait = 1;

    /// <summary>Updates the post-credits fade-in waits between each step and the next: one level every fourth update.</summary>
    internal const int PostCreditsFadeStepDelay = 3;

    /// <summary>
    /// The phase-timer counts on which the post-credits reward's fade takes a colour step: every fourth count, at place
    /// one of the period, so a 64-update fade half steps on its fourth update and every fourth update after.
    /// </summary>
    internal static readonly UpdateCadence RewardColourStepEveryFourthUpdate = new(4, 1);

    /// <summary>
    /// The updates on which the turning floor of the ending's atmosphere view turns one step: every second update of the
    /// cinematic frame counter, on its even place.
    /// </summary>
    internal static readonly UpdateCadence AtmosphereFloorTurnEverySecondUpdate = new(2, 0);

    /// <summary>
    /// The updates on which the departing planet, once small enough, turns one step toward its exit angle: every fourth
    /// update of the cinematic frame counter, on its first place.
    /// </summary>
    internal static readonly UpdateCadence PlanetTurnEveryFourthUpdate = new(4, 0);

    /// <summary>
    /// The phase-timer counts on which the planet's explosion crossfades its colours one step: every second count, on
    /// its even place.
    /// </summary>
    internal static readonly UpdateCadence ExplosionFadeStepEverySecondUpdate = new(2, 0);
}
