namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening cinematic's step (#2554): each phase its own case, carrying what it runs on. The opening lines run before
/// the narration pages exist; every page step carries the pages' narration actors; the crossfade into the discovery
/// carries its scene, and the flight to Ceres its flight.
/// </summary>
internal abstract record IntroCinematicStep
{
    /// <summary>Closes the hierarchy to the steps declared here.</summary>
    private IntroCinematicStep()
    {
    }

    /// <summary>The phase the step is, which diagnostics name.</summary>
    internal abstract IntroCinematicPhase Phase { get; }

    /// <summary>A step of the narration pages, run with their narration actors.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal abstract record Narrated(IntroNarrationActors Narration) : IntroCinematicStep
    {
        /// <summary>The narration actors the pages run with.</summary>
        internal IntroNarrationActors Narration { get; } = Ensure.NotNull(Narration);
    }

    /// <summary>The opening's <see cref="IntroCinematicPhase.Initial"/> step, before the narration pages exist.</summary>
    internal sealed record Initial : IntroCinematicStep
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.Initial;
    }

    /// <summary>The opening's <see cref="IntroCinematicPhase.WaitForInitialMusicQueue"/> step, before the narration pages exist.</summary>
    internal sealed record WaitForInitialMusicQueue : IntroCinematicStep
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.WaitForInitialMusicQueue;
    }

    /// <summary>The opening's <see cref="IntroCinematicPhase.FadeInFirstNarration"/> step, before the narration pages exist.</summary>
    internal sealed record FadeInFirstNarration : IntroCinematicStep
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.FadeInFirstNarration;
    }

    /// <summary>The opening's <see cref="IntroCinematicPhase.LastMetroidIsInCaptivity"/> step, before the narration pages exist.</summary>
    internal sealed record LastMetroidIsInCaptivity : IntroCinematicStep
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.LastMetroidIsInCaptivity;
    }

    /// <summary>The opening's <see cref="IntroCinematicPhase.GalaxyIsAtPeace"/> step, before the narration pages exist.</summary>
    internal sealed record GalaxyIsAtPeace : IntroCinematicStep
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.GalaxyIsAtPeace;
    }

    /// <summary>The opening's <see cref="IntroCinematicPhase.WaitForSecondMusicQueue"/> step, before the narration pages exist.</summary>
    internal sealed record WaitForSecondMusicQueue : IntroCinematicStep
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.WaitForSecondMusicQueue;
    }

    /// <summary>The opening's <see cref="IntroCinematicPhase.FourSecondHold"/> step, before the narration pages exist.</summary>
    internal sealed record FourSecondHold : IntroCinematicStep
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.FourSecondHold;
    }

    /// <summary>The opening's <see cref="IntroCinematicPhase.FadeOutFirstNarration"/> step, before the narration pages exist.</summary>
    internal sealed record FadeOutFirstNarration : IntroCinematicStep
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.FadeOutFirstNarration;
    }

    /// <summary>The opening's <see cref="IntroCinematicPhase.SetupFirstPage"/> step, before the narration pages exist.</summary>
    internal sealed record SetupFirstPage : IntroCinematicStep
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.SetupFirstPage;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.WaitForFirstPageMusicQueue"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record WaitForFirstPageMusicQueue(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.WaitForFirstPageMusicQueue;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.FadeInFirstPage"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record FadeInFirstPage(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.FadeInFirstPage;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.PageText"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record PageText(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.PageText;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.PageAwaitingInput"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record PageAwaitingInput(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.PageAwaitingInput;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.MotherBrainCrossfade"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record MotherBrainCrossfade(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.MotherBrainCrossfade;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.MotherBrainFlashback"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record MotherBrainFlashback(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.MotherBrainFlashback;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.PageCrossfade"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record PageCrossfade(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.PageCrossfade;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.BabyDiscovery"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record BabyDiscovery(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.BabyDiscovery;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.BabyMetroidDeliveryCrossfade"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record BabyMetroidDeliveryCrossfade(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.BabyMetroidDeliveryCrossfade;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.BabyMetroidDelivery"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record BabyMetroidDelivery(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.BabyMetroidDelivery;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.BabyMetroidExaminationCrossfade"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record BabyMetroidExaminationCrossfade(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.BabyMetroidExaminationCrossfade;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.BabyMetroidExamination"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record BabyMetroidExamination(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.BabyMetroidExamination;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.StartFinalPage"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record StartFinalPage(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.StartFinalPage;
    }

    /// <summary>The pages' <see cref="IntroCinematicPhase.IntroFadeOut"/> step, run with the pages' <paramref name="Narration"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    internal sealed record IntroFadeOut(IntroNarrationActors Narration) : Narrated(Narration)
    {
        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.IntroFadeOut;
    }

    /// <summary>The crossfade from the narration into the SR388 discovery, showing <paramref name="Discovery"/>.</summary>
    /// <param name="Narration">The narration actors the pages run with.</param>
    /// <param name="Discovery">The discovery scene the crossfade shows.</param>
    internal sealed record BabyDiscoveryCrossfade(IntroNarrationActors Narration, IntroBabyDiscoveryState Discovery) : Narrated(Narration)
    {
        /// <summary>The discovery scene the crossfade shows.</summary>
        internal IntroBabyDiscoveryState Discovery { get; } = Ensure.NotNull(Discovery);

        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.BabyDiscoveryCrossfade;
    }

    /// <summary>The flight to the space colony after the narrated pages, running <paramref name="Flight"/>.</summary>
    /// <param name="Flight">The flight scene.</param>
    internal sealed record CeresFlight(IntroCeresFlightState Flight) : IntroCinematicStep
    {
        /// <summary>The flight scene.</summary>
        internal IntroCeresFlightState Flight { get; } = Ensure.NotNull(Flight);

        /// <inheritdoc/>
        internal override IntroCinematicPhase Phase => IntroCinematicPhase.CeresFlight;
    }
}
