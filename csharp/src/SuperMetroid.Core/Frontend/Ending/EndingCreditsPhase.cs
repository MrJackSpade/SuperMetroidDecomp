namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's phases, from the setup's waits to the final see-you-next-mission screen (#2571): each phase names the shot
/// its turning floor shows, and each phase the ending's cast shows steps itself and enters its successor, so no switch over
/// phase identities runs them.
/// </summary>
internal sealed class EndingCreditsPhase
{
    /// <summary>The phase's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>Runs one update of a phase the cast shows, or null for a phase whose scene has actors of its own.</summary>
    private readonly Func<EndingCreditsState, bool>? _castStep;

    /// <summary>A phase named <paramref name="name"/> whose floor shows <paramref name="floorShot"/>.</summary>
    /// <param name="name">The phase's name, for diagnostics.</param>
    /// <param name="floorShot">The shot the turning floor shows in the phase.</param>
    /// <param name="castStep">Runs one update of a phase the cast shows, or null for a phase whose scene has actors of its own.</param>
    private EndingCreditsPhase(string name, EndingFloorShot floorShot, Func<EndingCreditsState, bool>? castStep = null)
    {
        _name = name;
        FloorShot = floorShot;
        _castStep = castStep;
    }

    /// <summary>Waits out the setup updates before loading the side-cloud escape.</summary>
    internal static EndingCreditsPhase SetupEscapeFromZebes { get; } = new(nameof(SetupEscapeFromZebes), EndingFloorShot.None, ending => ending.CastSetupEscapeFromZebes());

    /// <summary>Holds the cloud scene until the queued escape music finishes loading.</summary>
    internal static EndingCreditsPhase WaitForEscapeMusic { get; } = new(nameof(WaitForEscapeMusic), EndingFloorShot.SideCloudEscape, ending => ending.CastWaitForEscapeMusic());

    /// <summary>Reveals the side-cloud escape's rotating turning floor.</summary>
    internal static EndingCreditsPhase FadeInSideCloudEscape { get; } = new(nameof(FadeInSideCloudEscape), EndingFloorShot.SideCloudEscape, ending => ending.CastFadeInSideCloudEscape());

    /// <summary>Continues zooming the side-cloud escape to its authored threshold.</summary>
    internal static EndingCreditsPhase SideCloudEscape { get; } = new(nameof(SideCloudEscape), EndingFloorShot.SideCloudEscape, ending => ending.CastSideCloudEscape());

    /// <summary>Blanks the side-cloud escape before loading the top-and-bottom-cloud escape.</summary>
    internal static EndingCreditsPhase FadeOutSideCloudEscape { get; } = new(nameof(FadeOutSideCloudEscape), EndingFloorShot.SideCloudEscape, ending => ending.CastFadeOutSideCloudEscape());

    /// <summary>Reveals the top-and-bottom-cloud escape and its grey-cloud colour effect.</summary>
    internal static EndingCreditsPhase FadeInTopAndBottomCloudEscape { get; } = new(nameof(FadeInTopAndBottomCloudEscape), EndingFloorShot.TopAndBottomCloudEscape, ending => ending.CastFadeInTopAndBottomCloudEscape());

    /// <summary>Continues the top-and-bottom-cloud escape's zoom toward the planet explosion.</summary>
    internal static EndingCreditsPhase TopAndBottomCloudEscape { get; } = new(nameof(TopAndBottomCloudEscape), EndingFloorShot.TopAndBottomCloudEscape, ending => ending.CastTopAndBottomCloudEscape());

    /// <summary>Blanks the top-and-bottom-cloud escape before the explosion handoff.</summary>
    internal static EndingCreditsPhase FadeOutTopAndBottomCloudEscape { get; } = new(nameof(FadeOutTopAndBottomCloudEscape), EndingFloorShot.TopAndBottomCloudEscape, ending => ending.CastFadeOutTopAndBottomCloudEscape());

    /// <summary>Reveals the explosion while the planet's and the sky's colours crossfade.</summary>
    internal static EndingCreditsPhase FadeInZebesExplosion { get; } = new(nameof(FadeInZebesExplosion), EndingFloorShot.PlanetExplosion, ending => ending.CastFadeInZebesExplosion());

    /// <summary>Completes the explosion's colour crossfade after brightness reaches full.</summary>
    internal static EndingCreditsPhase ZebesExplosionCrossfade { get; } = new(nameof(ZebesExplosionCrossfade), EndingFloorShot.PlanetExplosion, ending => ending.CastZebesExplosionCrossfade());

    /// <summary>Holds the explosion (<see cref="EndingExplosionFadeDefinitions.HoldUpdates"/>) with the escape backdrop off; the flyaway's floor is ready as it ends.</summary>
    internal static EndingCreditsPhase ZebesExplosionHold { get; } = new(nameof(ZebesExplosionHold), EndingFloorShot.PlanetExplosion, ending => ending.CastZebesExplosionHold());

    /// <summary>Lets the exploding planet's own animation run the finale.</summary>
    internal static EndingCreditsPhase ZebesExplosionAnimation { get; } = new(nameof(ZebesExplosionAnimation), EndingFloorShot.Flyaway, ending => ending.CastZebesExplosionAnimation());

    /// <summary>Holds the explosion whiteout before queuing planet-escape music.</summary>
    internal static EndingCreditsPhase WaitForPlanetEscapeMusic { get; } = new(nameof(WaitForPlanetEscapeMusic), EndingFloorShot.Flyaway, ending => ending.CastWaitForPlanetEscapeMusic());

    /// <summary>Waits for the newly queued track before initializing planet motion.</summary>
    internal static EndingCreditsPhase WaitForPlanetEscapeMusicQueue { get; } = new(nameof(WaitForPlanetEscapeMusicQueue), EndingFloorShot.Flyaway, ending => ending.CastWaitForPlanetEscapeMusicQueue());

    /// <summary>Runs the first rotating planet flyaway motion pattern.</summary>
    internal static EndingCreditsPhase PlanetEscapeFast { get; } = new(nameof(PlanetEscapeFast), EndingFloorShot.Flyaway, ending => ending.CastPlanetEscapeFast());

    /// <summary>Runs the slower flyaway pattern and may release the animals' pod.</summary>
    internal static EndingCreditsPhase PlanetEscapeSlow { get; } = new(nameof(PlanetEscapeSlow), EndingFloorShot.Flyaway, ending => ending.CastPlanetEscapeSlow());

    /// <summary>Accelerates the planet off screen before the gunship completion text.</summary>
    internal static EndingCreditsPhase PlanetEscapeAccelerating { get; } = new(nameof(PlanetEscapeAccelerating), EndingFloorShot.Flyaway, ending => ending.CastPlanetEscapeAccelerating());

    /// <summary>Shows the completion text until its animation's cue starts the fade to the credits.</summary>
    internal static EndingCreditsPhase OperationSuccessfulText { get; } = new(nameof(OperationSuccessfulText), EndingFloorShot.Flyaway, ending => ending.CastOperationSuccessfulText());

    /// <summary>Blanks the planet and completion text before staff credits.</summary>
    internal static EndingCreditsPhase FadeOutToCredits { get; } = new(nameof(FadeOutToCredits), EndingFloorShot.Flyaway, ending => ending.CastFadeOutToCredits());

    /// <summary>Scrolls the staff credits until the roll has scrolled past its end.</summary>
    internal static EndingCreditsPhase Credits { get; } = new(nameof(Credits), EndingFloorShot.None);

    /// <summary>Holds the first post-credits backdrop blank (<see cref="EndingSequenceDefinitions.Timing.PostCreditsBlankFrames"/>).</summary>
    internal static EndingCreditsPhase PostCreditsBlank { get; } = new(nameof(PostCreditsBlank), EndingFloorShot.None, ending => ending.CastPostCreditsBlank());

    /// <summary>Reveals the waiting scene at the slower four-update brightness cadence.</summary>
    internal static EndingCreditsPhase PostCreditsFadeIn { get; } = new(nameof(PostCreditsFadeIn), EndingFloorShot.None, ending => ending.CastPostCreditsFadeIn());

    /// <summary>Fades the shooting-star backdrop toward the waiting colours.</summary>
    internal static EndingCreditsPhase PostCreditsShootingStars { get; } = new(nameof(PostCreditsShootingStars), EndingFloorShot.None, ending => ending.CastPostCreditsShootingStars());

    /// <summary>Holds the waiting backdrop before installing the result panel.</summary>
    internal static EndingCreditsPhase PostCreditsWaitingBackdrop { get; } = new(nameof(PostCreditsWaitingBackdrop), EndingFloorShot.None, ending => ending.CastPostCreditsWaitingBackdrop());

    /// <summary>Displays the result panel before revealing the chosen Samus reward.</summary>
    internal static EndingCreditsPhase PostCreditsWaitingSamus { get; } = new(nameof(PostCreditsWaitingSamus), EndingFloorShot.None, ending => ending.CastPostCreditsWaitingSamus());

    /// <summary>Advances the reward colours and sprite reveal in two timed halves.</summary>
    internal static EndingCreditsPhase PostCreditsReward { get; } = new(nameof(PostCreditsReward), EndingFloorShot.None);

    /// <summary>Shows the copyright panel while hidden reward sprites continue stepping.</summary>
    internal static EndingCreditsPhase PostCreditsCopyright { get; } = new(nameof(PostCreditsCopyright), EndingFloorShot.None);

    /// <summary>Runs the selected reward gesture until it requests a jump.</summary>
    internal static EndingCreditsPhase PostCreditsGesture { get; } = new(nameof(PostCreditsGesture), EndingFloorShot.None);

    /// <summary>Runs the reward jump, which reveals the reward icon as Samus falls back, until the shot.</summary>
    internal static EndingCreditsPhase PostCreditsJump { get; } = new(nameof(PostCreditsJump), EndingFloorShot.None);

    /// <summary>Runs the post-shot colour fades and picture changes before the white flash.</summary>
    internal static EndingCreditsPhase PostCreditsShot { get; } = new(nameof(PostCreditsShot), EndingFloorShot.RewardIcon);

    /// <summary>Fades the white flash out while the final text replaces the shot scene.</summary>
    internal static EndingCreditsPhase PostCreditsWhiteFlash { get; } = new(nameof(PostCreditsWhiteFlash), EndingFloorShot.None);

    /// <summary>Runs the final logo animation before the percentage text.</summary>
    internal static EndingCreditsPhase PostCreditsLogo { get; } = new(nameof(PostCreditsLogo), EndingFloorShot.None);

    /// <summary>Writes the collected-item percentage into the final panel.</summary>
    internal static EndingCreditsPhase ItemPercentage { get; } = new(nameof(ItemPercentage), EndingFloorShot.None);

    /// <summary>Scrolls the percentage panel down after its text animation completes.</summary>
    internal static EndingCreditsPhase ItemPercentageScrollDown { get; } = new(nameof(ItemPercentageScrollDown), EndingFloorShot.None);

    /// <summary>Holds the final see-you-next-mission screen until reset.</summary>
    internal static EndingCreditsPhase SeeYouNextMission { get; } = new(nameof(SeeYouNextMission), EndingFloorShot.None);

    /// <summary>
    /// The shot the turning floor shows in the phase: each escape scene's clouds from its setup through its fade-out, the
    /// exploding planet from its fade-in through its hold, the gunship flying away from the explosion's animation through
    /// the fade to the credits, the reward icon while the reward's shot turns into the camera, and nothing before the first
    /// scene, through the credits and the post-credits scenes that show no floor.
    /// </summary>
    internal EndingFloorShot FloorShot { get; }

    /// <summary>Runs one update of a phase the ending's cast shows.</summary>
    /// <param name="ending">The ending.</param>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    /// <exception cref="InvalidOperationException">The phase's scene has actors of its own, which run it.</exception>
    internal bool StepCast(EndingCreditsState ending) => (_castStep ?? throw new InvalidOperationException(
        $"The ending's cast runs no {_name} phase, whose scene has actors of its own."))(ending);

    /// <summary>Names the phase.</summary>
    public override string ToString() => _name;
}
