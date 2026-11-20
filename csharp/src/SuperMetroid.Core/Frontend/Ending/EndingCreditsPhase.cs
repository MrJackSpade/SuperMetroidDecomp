namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending's phases, from the setup's waits to the final see-you-next-mission screen (#2571): a phase whose turning floor
/// shows a shot names it (<see cref="IEndingFloorPhase"/>), and each phase the ending's cast shows (<see cref="EndingCastPhase"/>) steps itself and enters
/// its successor, so no switch over phase identities runs them. A phase whose scene has actors of its own is run by that
/// scene.
/// </summary>
internal class EndingCreditsPhase
{
    /// <summary>The phase's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>A phase named <paramref name="name"/>.</summary>
    /// <param name="name">The phase's name, for diagnostics.</param>
    private protected EndingCreditsPhase(string name) => _name = name;

    /// <summary>Waits out the setup updates before loading the side-cloud escape.</summary>
    internal static EndingCastPhase SetupEscapeFromZebes { get; } = new(nameof(SetupEscapeFromZebes), ending => ending.CastSetupEscapeFromZebes());

    /// <summary>Holds the cloud scene until the queued escape music finishes loading.</summary>
    internal static EndingFloorCastPhase WaitForEscapeMusic { get; } = new(nameof(WaitForEscapeMusic), EndingFloorShot.SideCloudEscape, ending => ending.CastWaitForEscapeMusic());

    /// <summary>Reveals the side-cloud escape's rotating turning floor.</summary>
    internal static EndingFloorCastPhase FadeInSideCloudEscape { get; } = new(nameof(FadeInSideCloudEscape), EndingFloorShot.SideCloudEscape, ending => ending.CastFadeInSideCloudEscape());

    /// <summary>Continues zooming the side-cloud escape to its authored threshold.</summary>
    internal static EndingFloorCastPhase SideCloudEscape { get; } = new(nameof(SideCloudEscape), EndingFloorShot.SideCloudEscape, ending => ending.CastSideCloudEscape());

    /// <summary>Blanks the side-cloud escape before loading the top-and-bottom-cloud escape.</summary>
    internal static EndingFloorCastPhase FadeOutSideCloudEscape { get; } = new(nameof(FadeOutSideCloudEscape), EndingFloorShot.SideCloudEscape, ending => ending.CastFadeOutSideCloudEscape());

    /// <summary>Reveals the top-and-bottom-cloud escape and its grey-cloud colour effect.</summary>
    internal static EndingFloorCastPhase FadeInTopAndBottomCloudEscape { get; } = new(nameof(FadeInTopAndBottomCloudEscape), EndingFloorShot.TopAndBottomCloudEscape, ending => ending.CastFadeInTopAndBottomCloudEscape());

    /// <summary>Continues the top-and-bottom-cloud escape's zoom toward the planet explosion.</summary>
    internal static EndingFloorCastPhase TopAndBottomCloudEscape { get; } = new(nameof(TopAndBottomCloudEscape), EndingFloorShot.TopAndBottomCloudEscape, ending => ending.CastTopAndBottomCloudEscape());

    /// <summary>Blanks the top-and-bottom-cloud escape before the explosion handoff.</summary>
    internal static EndingFloorCastPhase FadeOutTopAndBottomCloudEscape { get; } = new(nameof(FadeOutTopAndBottomCloudEscape), EndingFloorShot.TopAndBottomCloudEscape, ending => ending.CastFadeOutTopAndBottomCloudEscape());

    /// <summary>Reveals the explosion while the planet's and the sky's colours crossfade.</summary>
    internal static EndingFloorCastPhase FadeInZebesExplosion { get; } = new(nameof(FadeInZebesExplosion), EndingFloorShot.PlanetExplosion, ending => ending.CastFadeInZebesExplosion());

    /// <summary>Completes the explosion's colour crossfade after brightness reaches full.</summary>
    internal static EndingFloorCastPhase ZebesExplosionCrossfade { get; } = new(nameof(ZebesExplosionCrossfade), EndingFloorShot.PlanetExplosion, ending => ending.CastZebesExplosionCrossfade());

    /// <summary>Holds the explosion (<see cref="EndingExplosionFadeDefinitions.HoldUpdates"/>) with the escape backdrop off; the flyaway's floor is ready as it ends.</summary>
    internal static EndingFloorCastPhase ZebesExplosionHold { get; } = new(nameof(ZebesExplosionHold), EndingFloorShot.PlanetExplosion, ending => ending.CastZebesExplosionHold());

    /// <summary>Lets the exploding planet's own animation run the finale.</summary>
    internal static EndingFloorCastPhase ZebesExplosionAnimation { get; } = new(nameof(ZebesExplosionAnimation), EndingFloorShot.Flyaway, ending => ending.CastZebesExplosionAnimation());

    /// <summary>Holds the explosion whiteout before queuing planet-escape music.</summary>
    internal static EndingFloorCastPhase WaitForPlanetEscapeMusic { get; } = new(nameof(WaitForPlanetEscapeMusic), EndingFloorShot.Flyaway, ending => ending.CastWaitForPlanetEscapeMusic());

    /// <summary>Waits for the newly queued track before initializing planet motion.</summary>
    internal static EndingFloorCastPhase WaitForPlanetEscapeMusicQueue { get; } = new(nameof(WaitForPlanetEscapeMusicQueue), EndingFloorShot.Flyaway, ending => ending.CastWaitForPlanetEscapeMusicQueue());

    /// <summary>Runs the first rotating planet flyaway motion pattern.</summary>
    internal static EndingFloorCastPhase PlanetEscapeFast { get; } = new(nameof(PlanetEscapeFast), EndingFloorShot.Flyaway, ending => ending.CastPlanetEscapeFast());

    /// <summary>Runs the slower flyaway pattern and may release the animals' pod.</summary>
    internal static EndingFloorCastPhase PlanetEscapeSlow { get; } = new(nameof(PlanetEscapeSlow), EndingFloorShot.Flyaway, ending => ending.CastPlanetEscapeSlow());

    /// <summary>Accelerates the planet off screen before the gunship completion text.</summary>
    internal static EndingFloorCastPhase PlanetEscapeAccelerating { get; } = new(nameof(PlanetEscapeAccelerating), EndingFloorShot.Flyaway, ending => ending.CastPlanetEscapeAccelerating());

    /// <summary>Shows the completion text until its animation's cue starts the fade to the credits.</summary>
    internal static EndingFloorCastPhase OperationSuccessfulText { get; } = new(nameof(OperationSuccessfulText), EndingFloorShot.Flyaway, ending => ending.CastOperationSuccessfulText());

    /// <summary>Blanks the planet and completion text before staff credits.</summary>
    internal static EndingFloorCastPhase FadeOutToCredits { get; } = new(nameof(FadeOutToCredits), EndingFloorShot.Flyaway, ending => ending.CastFadeOutToCredits());

    /// <summary>Scrolls the staff credits until the roll has scrolled past its end.</summary>
    internal static EndingCreditsPhase Credits { get; } = new(nameof(Credits));

    /// <summary>Holds the first post-credits backdrop blank (<see cref="EndingSequenceDefinitions.Timing.PostCreditsBlankFrames"/>).</summary>
    internal static EndingCastPhase PostCreditsBlank { get; } = new(nameof(PostCreditsBlank), ending => ending.CastPostCreditsBlank());

    /// <summary>Reveals the waiting scene at the slower post-credits brightness cadence.</summary>
    internal static EndingCastPhase PostCreditsFadeIn { get; } = new(nameof(PostCreditsFadeIn), ending => ending.CastPostCreditsFadeIn());

    /// <summary>Fades the shooting-star backdrop toward the waiting colours.</summary>
    internal static EndingCastPhase PostCreditsShootingStars { get; } = new(nameof(PostCreditsShootingStars), ending => ending.CastPostCreditsShootingStars());

    /// <summary>Holds the waiting backdrop before installing the result panel.</summary>
    internal static EndingCastPhase PostCreditsWaitingBackdrop { get; } = new(nameof(PostCreditsWaitingBackdrop), ending => ending.CastPostCreditsWaitingBackdrop());

    /// <summary>Displays the result panel before revealing the chosen Samus reward.</summary>
    internal static EndingCastPhase PostCreditsWaitingSamus { get; } = new(nameof(PostCreditsWaitingSamus), ending => ending.CastPostCreditsWaitingSamus());

    /// <summary>Advances the reward colours and sprite reveal in two timed halves.</summary>
    internal static EndingCreditsPhase PostCreditsReward { get; } = new(nameof(PostCreditsReward));

    /// <summary>Shows the copyright panel while hidden reward sprites continue stepping.</summary>
    internal static EndingCreditsPhase PostCreditsCopyright { get; } = new(nameof(PostCreditsCopyright));

    /// <summary>Runs the selected reward gesture until it requests a jump.</summary>
    internal static EndingCreditsPhase PostCreditsGesture { get; } = new(nameof(PostCreditsGesture));

    /// <summary>Runs the reward jump, which reveals the reward icon as Samus falls back, until the shot.</summary>
    internal static EndingCreditsPhase PostCreditsJump { get; } = new(nameof(PostCreditsJump));

    /// <summary>Runs the post-shot colour fades and picture changes before the white flash.</summary>
    internal static EndingFloorScenePhase PostCreditsShot { get; } = new(nameof(PostCreditsShot), EndingFloorShot.RewardIcon);

    /// <summary>Fades the white flash out while the final text replaces the shot scene.</summary>
    internal static EndingCreditsPhase PostCreditsWhiteFlash { get; } = new(nameof(PostCreditsWhiteFlash));

    /// <summary>Runs the final logo animation before the percentage text.</summary>
    internal static EndingCreditsPhase PostCreditsLogo { get; } = new(nameof(PostCreditsLogo));

    /// <summary>Writes the collected-item percentage into the final panel.</summary>
    internal static EndingCreditsPhase ItemPercentage { get; } = new(nameof(ItemPercentage));

    /// <summary>Scrolls the percentage panel down after its text animation completes.</summary>
    internal static EndingCreditsPhase ItemPercentageScrollDown { get; } = new(nameof(ItemPercentageScrollDown));

    /// <summary>Holds the final see-you-next-mission screen until reset.</summary>
    internal static EndingCreditsPhase SeeYouNextMission { get; } = new(nameof(SeeYouNextMission));

    /// <summary>Names the phase.</summary>
    public override string ToString() => _name;
}
