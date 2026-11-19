namespace SuperMetroid.Core.Hud;

/// <summary>What a gameplay message asks the player to confirm, which decides how its box reads the controller and closes (#2542).</summary>
public enum GameplayMessageConfirmation
{
    /// <summary>The message asks nothing: once it has been shown long enough, any held button closes it.</summary>
    None,

    /// <summary>
    /// The message asks whether to save: the box takes input as soon as its window opens, moves a YES/NO cursor, and hands
    /// the chosen answer to what waited on it as it closes.
    /// </summary>
    Save,

    /// <summary>
    /// The message asks whether to save, as <see cref="Save"/> does, and an accepted save keeps the box through the saving
    /// sound and a completion notice of its own, handing YES over only once that notice has closed.
    /// </summary>
    SaveThenCompletionNotice,
}
