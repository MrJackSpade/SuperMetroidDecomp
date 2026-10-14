namespace SuperMetroid.Core.Frontend;

/// <summary>Ordered phases of the opening narration, flashbacks, laboratory scenes, and Ceres flight.</summary>
public enum IntroCinematicPhase
{
    /// <summary>Waits for the initial opening music commands to finish queuing.</summary>
    WaitForInitialMusicQueue,
    /// <summary>Fades in the first narration card.</summary>
    FadeInFirstNarration,
    /// <summary>Holds the “last Metroid” narration card.</summary>
    LastMetroidIsInCaptivity,
    /// <summary>Holds the “galaxy is at peace” narration card.</summary>
    GalaxyIsAtPeace,
    /// <summary>Waits for the Mother Brain flashback music commands.</summary>
    WaitForSecondMusicQueue,
    /// <summary>Holds the first narration card for its authored four seconds.</summary>
    FourSecondHold,
    /// <summary>Fades out the first narration card.</summary>
    FadeOutFirstNarration,
    /// <summary>Waits for the first illustrated page's music queue.</summary>
    WaitForPageOneMusicQueue,
    /// <summary>Fades in the first illustrated narration page.</summary>
    FadeInPageOne,
    /// <summary>Types and displays the first illustrated page.</summary>
    PageOneText,
    /// <summary>Waits for input after the first illustrated page.</summary>
    PageOneAwaitingInput,
    /// <summary>Crossfades from page one into the Mother Brain gameplay flashback.</summary>
    MotherBrainCrossfade,
    /// <summary>Runs the Mother Brain gameplay flashback.</summary>
    MotherBrainFlashback,
    /// <summary>Crossfades from the Mother Brain flashback to narration page two.</summary>
    PageTwoCrossfade,
    /// <summary>Types and displays narration page two.</summary>
    PageTwoText,
    /// <summary>Waits for input after narration page two.</summary>
    PageTwoAwaitingInput,
    /// <summary>Crossfades from page two into the baby Metroid discovery flashback.</summary>
    BabyDiscoveryCrossfade,
    /// <summary>Runs the SR388 baby Metroid discovery flashback.</summary>
    BabyDiscovery,
    /// <summary>Crossfades from the discovery flashback to narration page three.</summary>
    PageThreeCrossfade,
    /// <summary>Types and displays narration page three.</summary>
    PageThreeText,
    /// <summary>Waits for input after narration page three.</summary>
    PageThreeAwaitingInput,
    /// <summary>Crossfades from page three into the baby Metroid delivery scene.</summary>
    BabyMetroidDeliveryCrossfade,
    /// <summary>Runs the Ceres baby Metroid delivery scene.</summary>
    BabyMetroidDelivery,
    /// <summary>Crossfades from the delivery scene to narration page four.</summary>
    PageFourCrossfade,
    /// <summary>Types and displays narration page four.</summary>
    PageFourText,
    /// <summary>Waits for input after narration page four.</summary>
    PageFourAwaitingInput,
    /// <summary>Crossfades from page four into the baby Metroid examination scene.</summary>
    BabyMetroidExaminationCrossfade,
    /// <summary>Runs the Ceres scientist examination scene.</summary>
    BabyMetroidExamination,
    /// <summary>Crossfades from the examination scene to narration page five.</summary>
    PageFiveCrossfade,
    /// <summary>Types and displays narration page five.</summary>
    PageFiveText,
    /// <summary>Waits for input after narration page five.</summary>
    PageFiveAwaitingInput,
    /// <summary>Types and displays the final narration page.</summary>
    PageSixText,
    /// <summary>Fades out the completed narration sequence.</summary>
    IntroFadeOut,
    /// <summary>Runs the SPACE COLONY Ceres approach sequence.</summary>
    CeresFlight,
    /// <summary>$8B:A395: the setup dispatch that queues the opening music.</summary>
    /// <remarks>Debugger states store phases numerically, so this follows the existing members.</remarks>
    Initial,
    /// <summary>$8B:A66F: the dispatch after the first narration's fade-out that sets up page one.</summary>
    SetupPageOne,
    /// <summary>$8B:B207: with Japanese text, the dispatch after page five's input that starts page six.</summary>
    StartPageSix,
}
