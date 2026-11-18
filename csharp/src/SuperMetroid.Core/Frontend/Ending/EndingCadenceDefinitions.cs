namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The cadences of the ending (#1606): the turning floor's and the departing planet's counter cadences, and the paces of the
/// explosion's crossfade and the reward's colours (#2488).
/// </summary>
internal static class EndingCadenceDefinitions
{
    /// <summary>Updates each step of the escape scenes' and the credits' fades takes: one level every second update.</summary>
    internal const int FastFadeUpdatesPerStep = 2;

    /// <summary>Updates the post-credits fade-in waits after the blank hold before its first step.</summary>
    internal const int PostCreditsFadeFirstWait = 1;

    /// <summary>Updates the post-credits fade-in waits between each step and the next: one level every fourth update.</summary>
    internal const int PostCreditsFadeStepDelay = 3;

    /// <summary>
    /// The colour steps of the post-credits reward's fade over its two 64-update halves: sixteen in each, undoing the
    /// waiting backdrop's thirty-two.
    /// </summary>
    internal const int RewardColourFadeSteps = 32;

    /// <summary>
    /// The updates of the reward's halves each colour step of its fade takes: a half steps on its fourth update and every
    /// fourth update after, and the copyright between the halves takes none.
    /// </summary>
    internal const int RewardColourUpdatesPerStep = 4;

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
    /// The updates each step of the planet explosion's colour crossfade takes: the crossfade steps on its second update and
    /// every second update after, and ends on the update it takes its last step.
    /// </summary>
    internal const int ExplosionCrossfadeUpdatesPerStep = 2;
}
