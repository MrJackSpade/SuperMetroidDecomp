namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// What the fight asks of <see cref="PhantoonBodyVisibilityEffect"/> while Phantoon is not translucent (#1933): whether his
/// body shows.
/// </summary>
public enum PhantoonBodyVisibilityRequest
{
    /// <summary>Hide his body: the request every part's start and his fatal hit leave.</summary>
    Hide,

    /// <summary>Show his body as the room shows it, as the wavy fade-in and the first round ask.</summary>
    Show,

    /// <summary>Hide his body and end the visibility effect for good, as clearing his body on death asks.</summary>
    HideForGood,
}
