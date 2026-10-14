namespace SuperMetroid.Core.Game;

/// <summary>Named equivalents of the three timer/hack pointer values relevant to draining.</summary>
public enum DrainedGetUpHandler
{
    /// <summary>The drained Up-input callback is no longer installed, either after a successful stand request or because another timer/hack owner replaced it.</summary>
    Inactive,
    /// <summary>$90:E09B accepts a fresh Up press only in left crouching pose $E9 at animation index eight or later, starts index thirteen with timer one, then disables itself.</summary>
    AbleToStand,
    /// <summary>$90:E0C5 accepts a fresh Up press at animation indices eight through eleven, starts failed-stand index eighteen with timer one, and stays installed for later attempts.</summary>
    UnableToStand,
}
