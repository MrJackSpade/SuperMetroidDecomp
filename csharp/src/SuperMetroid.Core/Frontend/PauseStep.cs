namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Which step of the pause (<see cref="Progression.SuperMetroidGameState.Pause"/>) runs without the pause screen; the
/// steps that show it are <see cref="PauseMenuStep"/>.
/// </summary>
internal enum PauseStep
{
    /// <summary>Ordinary gameplay keeps running while the screen darkens for the pause.</summary>
    DarkeningGameplay,
    /// <summary>The pause screen is prepared on a blank display and gameplay's sound effects are cancelled.</summary>
    PreparingPauseScreen,
    /// <summary>Gameplay is restored on a blank display, reconciling the equipment the pause changed.</summary>
    RestoringGameplay,
    /// <summary>Ordinary gameplay resumes while the screen brightens, then <see cref="Progression.SuperMetroidGameState.MainGameplay"/>.</summary>
    BrighteningGameplay,
}
