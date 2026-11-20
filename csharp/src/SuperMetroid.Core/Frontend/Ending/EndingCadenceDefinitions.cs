namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The cadences of the ending (#1606): the turning floor's and the departing planet's counter cadences, and the paces of the
/// explosion's crossfade and the reward's colours (#2488).
/// </summary>
internal static class EndingCadenceDefinitions
{
    /// <summary>Updates each step of the escape scenes' and the credits' fades takes.</summary>
    internal const int FastFadeUpdatesPerStep = 2;

    /// <summary>Updates the post-credits fade-in waits after the blank hold before its first step.</summary>
    internal const int PostCreditsFadeFirstWait = 1;

    /// <summary>Updates the post-credits fade-in waits between each step and the next.</summary>
    internal const int PostCreditsFadeStepDelay = 3;

    /// <summary>
    /// The colour steps of the post-credits reward's fade over both its halves, half of them in each, undoing the waiting
    /// backdrop's.
    /// </summary>
    internal const int RewardColourFadeSteps = 32;

    /// <summary>
    /// The updates of the reward's halves each colour step of its fade takes: a half steps on the last of each run of
    /// them, and the copyright between the halves takes none.
    /// </summary>
    internal const int RewardColourUpdatesPerStep = 4;

    /// <summary>
    /// The updates on which the turning floor of the ending's atmosphere view turns one step, counted on the cinematic
    /// frame counter at the period's even place.
    /// </summary>
    internal static readonly UpdateCadence AtmosphereFloorTurn = new(2, 0);

    /// <summary>
    /// The updates on which the departing planet, once small enough, turns one step toward its exit angle, counted on the
    /// cinematic frame counter at the period's first place.
    /// </summary>
    internal static readonly UpdateCadence PlanetTurn = new(4, 0);

    /// <summary>
    /// The updates each step of the planet explosion's colour crossfade takes: the crossfade steps on the last of each run
    /// of them, and ends on the update it takes its last step.
    /// </summary>
    internal const int ExplosionCrossfadeUpdatesPerStep = 2;


    /// <summary>The updates each step off the post-credits white flash takes: one, a step fewer each update.</summary>
    internal const int PostCreditsFlashUpdatesPerStep = 1;

    /// <summary>The update of the post-credits white flash its first step off white comes on: the first.</summary>
    internal const int PostCreditsFlashFirstStepUpdate = 1;
}
