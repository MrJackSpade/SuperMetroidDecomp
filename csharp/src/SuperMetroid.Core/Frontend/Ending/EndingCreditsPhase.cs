namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's phases, from the setup's waits to the final see-you-next-mission screen.</summary>
internal enum EndingCreditsPhase
{
    /// <summary>Waits out the setup updates before loading the side-cloud escape.</summary>
    SetupEscapeFromZebes,
    /// <summary>Holds the cloud scene until the queued escape music finishes loading.</summary>
    WaitForEscapeMusic,
    /// <summary>Reveals the side-cloud escape's rotating turning floor.</summary>
    FadeInSideCloudEscape,
    /// <summary>Continues zooming the side-cloud escape to its authored threshold.</summary>
    SideCloudEscape,
    /// <summary>Blanks the side-cloud escape before loading the top-and-bottom-cloud escape.</summary>
    FadeOutSideCloudEscape,
    /// <summary>Reveals the top-and-bottom-cloud escape and its grey-cloud colour effect.</summary>
    FadeInTopAndBottomCloudEscape,
    /// <summary>Continues the top-and-bottom-cloud escape's zoom toward the planet explosion.</summary>
    TopAndBottomCloudEscape,
    /// <summary>Blanks the top-and-bottom-cloud escape before the explosion handoff.</summary>
    FadeOutTopAndBottomCloudEscape,
    /// <summary>Reveals the explosion while the planet's and the sky's colours crossfade.</summary>
    FadeInZebesExplosion,
    /// <summary>Completes the explosion's colour crossfade after brightness reaches full.</summary>
    ZebesExplosionCrossfade,
    /// <summary>Holds the explosion (<see cref="EndingExplosionFadeDefinitions.HoldUpdates"/>) with the escape backdrop off; the flyaway's floor is ready as it ends.</summary>
    ZebesExplosionHold,
    /// <summary>Lets the exploding planet's own animation run the finale.</summary>
    ZebesExplosionAnimation,
    /// <summary>Holds the explosion whiteout before queuing planet-escape music.</summary>
    WaitForPlanetEscapeMusic,
    /// <summary>Waits for the newly queued track before initializing planet motion.</summary>
    WaitForPlanetEscapeMusicQueue,
    /// <summary>Runs the first rotating planet flyaway motion pattern.</summary>
    PlanetEscapeFast,
    /// <summary>Runs the slower flyaway pattern and may release the animals' pod.</summary>
    PlanetEscapeSlow,
    /// <summary>Accelerates the planet off screen before the gunship completion text.</summary>
    PlanetEscapeAccelerating,
    /// <summary>Shows the completion text until its animation's cue starts the fade to the credits.</summary>
    OperationSuccessfulText,
    /// <summary>Blanks the planet and completion text before staff credits.</summary>
    FadeOutToCredits,
    /// <summary>Scrolls the staff credits until the roll has scrolled past its end.</summary>
    Credits,
    /// <summary>Holds the first post-credits backdrop blank (<see cref="EndingSequenceDefinitions.Timing.PostCreditsBlankFrames"/>).</summary>
    PostCreditsBlank,
    /// <summary>Reveals the waiting scene at the slower four-update brightness cadence.</summary>
    PostCreditsFadeIn,
    /// <summary>Fades the shooting-star backdrop toward the waiting colours.</summary>
    PostCreditsShootingStars,
    /// <summary>Holds the waiting backdrop before installing the result panel.</summary>
    PostCreditsWaitingBackdrop,
    /// <summary>Displays the result panel before revealing the chosen Samus reward.</summary>
    PostCreditsWaitingSamus,
    /// <summary>Advances the reward colours and sprite reveal in two timed halves.</summary>
    PostCreditsReward,
    /// <summary>Shows the copyright panel while hidden reward sprites continue stepping.</summary>
    PostCreditsCopyright,
    /// <summary>Runs the selected reward gesture until it requests a jump.</summary>
    PostCreditsGesture,
    /// <summary>Runs the reward jump, which reveals the reward icon as Samus falls back, until the shot.</summary>
    PostCreditsJump,
    /// <summary>Runs the post-shot colour fades and picture changes before the white flash.</summary>
    PostCreditsShot,
    /// <summary>Fades the white flash out while the final text replaces the shot scene.</summary>
    PostCreditsWhiteFlash,
    /// <summary>Runs the final logo animation before the percentage text.</summary>
    PostCreditsLogo,
    /// <summary>Writes the collected-item percentage into the final panel.</summary>
    ItemPercentage,
    /// <summary>Scrolls the percentage panel down after its text animation completes.</summary>
    ItemPercentageScrollDown,
    /// <summary>Holds the final see-you-next-mission screen until reset.</summary>
    SeeYouNextMission,
}
