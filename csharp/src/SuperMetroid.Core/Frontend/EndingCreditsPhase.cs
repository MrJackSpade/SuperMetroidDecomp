namespace SuperMetroid.Core.Frontend;

/// <summary>Bank-$8B cinematic coroutine positions, from setup waits to the terminal result display.</summary>
internal enum EndingCreditsPhase
{
    /// <summary>Performs setup NMI waits before loading the first cloud scene.</summary>
    SetupEscapeFromZebes,
    /// <summary>Holds the cloud scene until the queued escape music finishes loading.</summary>
    WaitForEscapeMusic,
    /// <summary>Reveals the first rotating Mode-7 cloud scene.</summary>
    FadeInEscapeSceneA,
    /// <summary>Continues zooming the first cloud scene to its authored threshold.</summary>
    EscapeSceneA,
    /// <summary>Blanks the first cloud scene before loading the second.</summary>
    FadeOutEscapeSceneA,
    /// <summary>Reveals the second cloud scene and its grey-cloud palette effect.</summary>
    FadeInEscapeSceneB,
    /// <summary>Continues the second cloud zoom toward the planet explosion.</summary>
    EscapeSceneB,
    /// <summary>Blanks the second cloud scene before the explosion handoff.</summary>
    FadeOutEscapeSceneB,
    /// <summary>Reveals the explosion while object and background colors crossfade.</summary>
    FadeInZebesExplosion,
    /// <summary>Completes the explosion palette interpolation after brightness reaches full.</summary>
    ZebesExplosionPaletteCrossfade,
    /// <summary>Uploads Ceres-derived flyaway Mode-7 art over sixteen NMI calls.</summary>
    ZebesExplosionTileUpload,
    /// <summary>Lets the explosion actor's private instruction list control the finale.</summary>
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
    /// <summary>Lets completion-text sprite instructions trigger the credits transition.</summary>
    OperationSuccessfulText,
    /// <summary>Blanks the planet and completion text before staff credits.</summary>
    FadeOutToCredits,
    /// <summary>Scrolls the staff tilemap until its control program finishes.</summary>
    Credits,
    /// <summary>Holds the first post-credits backdrop blank for sixty calls.</summary>
    PostCreditsBlank,
    /// <summary>Reveals the waiting scene at the slower four-call brightness cadence.</summary>
    PostCreditsFadeIn,
    /// <summary>Fades the shooting-star backdrop toward the waiting palette.</summary>
    PostCreditsShootingStars,
    /// <summary>Holds the waiting backdrop before installing the result panel.</summary>
    PostCreditsWaitingBackdrop,
    /// <summary>Displays the result panel before revealing the chosen Samus reward.</summary>
    PostCreditsWaitingSamus,
    /// <summary>Advances the reward palette and sprite reveal in two timed halves.</summary>
    PostCreditsReward,
    /// <summary>Shows the copyright panel while hidden reward sprites continue stepping.</summary>
    PostCreditsCopyright,
    /// <summary>Runs the selected reward gesture until it requests a jump.</summary>
    PostCreditsGesture,
    /// <summary>Runs the reward jump and its staged graphics uploads until the shot.</summary>
    PostCreditsJump,
    /// <summary>Runs the post-shot palette and tile replacements before the white flash.</summary>
    PostCreditsShot,
    /// <summary>Fades fixed white while switching to the two-screen final text map.</summary>
    PostCreditsWhiteFlash,
    /// <summary>Runs the final logo animation before the percentage text.</summary>
    PostCreditsLogo,
    /// <summary>Writes the collected-item percentage into the final panel.</summary>
    ItemPercentage,
    /// <summary>Scrolls the percentage panel down after its text program completes.</summary>
    ItemPercentageScrollDown,
    /// <summary>Holds the final see-you-next-mission screen until reset.</summary>
    SeeYouNextMission,
}
