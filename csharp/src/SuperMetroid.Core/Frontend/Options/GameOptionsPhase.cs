namespace SuperMetroid.Core.Frontend.Options;

/// <summary>Identifies the current stage of the game-options menu state machine.</summary>
public enum GameOptionsPhase
{
    /// <summary>Increase screen brightness to the menu maximum.</summary>
    FadeIn,
    /// <summary>Handle the primary options-page selection.</summary>
    Main,
    /// <summary>Fade the current page to black before changing pages.</summary>
    DissolveOut,
    /// <summary>Fade the newly loaded page in from black.</summary>
    DissolveIn,
    /// <summary>Handle controller-binding rows and commands.</summary>
    ControllerSettings,
    /// <summary>Handle icon-cancel and moonwalk settings.</summary>
    SpecialSettings,
    /// <summary>Scroll the controller page toward its lower rows.</summary>
    ScrollControllerDown,
    /// <summary>Scroll the controller page toward its upper rows.</summary>
    ScrollControllerUp,
    /// <summary>Fade to black before starting the opening sequence.</summary>
    FadeOutToIntro,
    /// <summary>Fade to black before returning to file select.</summary>
    FadeOutToFileSelect,
    /// <summary>Publish the opening-sequence request after the preceding fade completes.</summary>
    StartGame,
    /// <summary>Native index zero: finish the previous menu's fade-out at black.</summary>
    FinishFadingOut,
    /// <summary>Native index one: load the menu pages and spawn the selection missile.</summary>
    LoadingMenu,
}
