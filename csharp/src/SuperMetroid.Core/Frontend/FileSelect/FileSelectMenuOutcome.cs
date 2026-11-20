namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>How the file-select menu hands off to its host (#2562), which it keeps once it reaches it.</summary>
public enum FileSelectMenuOutcome
{
    /// <summary>A selected slot, empty or occupied, finished its helmet turn and fade-out: the host shows the game options.</summary>
    NewGame = 1,
    /// <summary>Exit or B completed the fade back toward reset: the host starts the title again.</summary>
    Title = 2,
}
