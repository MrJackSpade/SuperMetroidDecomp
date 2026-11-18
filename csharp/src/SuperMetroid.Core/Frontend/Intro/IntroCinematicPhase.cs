namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Ordered phases of the opening narration, flashbacks, laboratory scenes, and Ceres flight.</summary>
public enum IntroCinematicPhase
{
    /// <summary>Waits for the initial opening music requests to finish queuing.</summary>
    WaitForInitialMusicQueue,
    /// <summary>Fades in the first narration card.</summary>
    FadeInFirstNarration,
    /// <summary>Holds the “last Metroid” narration card.</summary>
    LastMetroidIsInCaptivity,
    /// <summary>Holds the “galaxy is at peace” narration card.</summary>
    GalaxyIsAtPeace,
    /// <summary>Waits for the Mother Brain flashback music requests.</summary>
    WaitForSecondMusicQueue,
    /// <summary>Holds the first narration card for its authored four seconds.</summary>
    FourSecondHold,
    /// <summary>Fades out the first narration card.</summary>
    FadeOutFirstNarration,
    /// <summary>Waits for the first narration page's music queue.</summary>
    WaitForFirstPageMusicQueue,
    /// <summary>Fades in the first narration page.</summary>
    FadeInFirstPage,
    /// <summary>Types and displays the current narration page (#2489).</summary>
    PageText,
    /// <summary>Waits for the press that turns the current narration page.</summary>
    PageAwaitingInput,
    /// <summary>Crossfades from the narration into the Mother Brain gameplay flashback.</summary>
    MotherBrainCrossfade,
    /// <summary>Runs the Mother Brain gameplay flashback.</summary>
    MotherBrainFlashback,
    /// <summary>Crossfades from an illustration back to the current narration page.</summary>
    PageCrossfade,
    /// <summary>Crossfades from the narration into the baby Metroid discovery flashback.</summary>
    BabyDiscoveryCrossfade,
    /// <summary>Runs the SR388 baby Metroid discovery flashback.</summary>
    BabyDiscovery,
    /// <summary>Crossfades from the narration into the baby Metroid delivery scene.</summary>
    BabyMetroidDeliveryCrossfade,
    /// <summary>Runs the Ceres baby Metroid delivery scene.</summary>
    BabyMetroidDelivery,
    /// <summary>Crossfades from the narration into the baby Metroid examination scene.</summary>
    BabyMetroidExaminationCrossfade,
    /// <summary>Runs the Ceres scientist examination scene.</summary>
    BabyMetroidExamination,
    /// <summary>Fades out the completed narration sequence.</summary>
    IntroFadeOut,
    /// <summary>Runs the SPACE COLONY Ceres approach sequence.</summary>
    CeresFlight,
    /// <summary>The setup update that queues the opening music.</summary>
    Initial,
    /// <summary>The update after the first narration's fade-out that sets up the first page.</summary>
    SetupFirstPage,
    /// <summary>With Japanese text, the update after the page before the last's input that starts the last page.</summary>
    StartFinalPage,
}
