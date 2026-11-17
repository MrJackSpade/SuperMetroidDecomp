namespace SuperMetroid.Core.Frontend;

/// <summary>Which step of the pause (<see cref="Progression.SuperMetroidGameState.Pause"/>) runs.</summary>
internal enum PauseStep
{
    /// <summary>Ordinary gameplay keeps running while the screen darkens for the pause.</summary>
    DarkeningGameplay,
    /// <summary>The pause screen is prepared on a blank display and gameplay's sound effects are cancelled.</summary>
    PreparingPauseScreen,
    /// <summary>The prepared pause screen is drawn and brightens; gameplay stays paused.</summary>
    FadingIn,
    /// <summary>The pause map and equipment screens take input while gameplay stays paused.</summary>
    Menu,
    /// <summary>The pause screen is drawn while it fades to a blank display after Start requests unpause.</summary>
    FadingOut,
    /// <summary>Gameplay is restored on a blank display, reconciling the equipment the pause changed.</summary>
    RestoringGameplay,
    /// <summary>Ordinary gameplay resumes while the screen brightens, then <see cref="Progression.SuperMetroidGameState.MainGameplay"/>.</summary>
    BrighteningGameplay,
}
