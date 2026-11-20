namespace SuperMetroid.Core.Frontend;

/// <summary>Which step of the pause (<see cref="Progression.SuperMetroidGameState.Pause"/>) shows the pause screen.</summary>
internal enum PauseMenuStep
{
    /// <summary>The prepared pause screen is drawn and brightens; gameplay stays paused.</summary>
    FadingIn,
    /// <summary>The pause map and equipment screens take input while gameplay stays paused.</summary>
    Menu,
    /// <summary>The pause screen is drawn while it fades to a blank display after Start requests unpause.</summary>
    FadingOut,
}
