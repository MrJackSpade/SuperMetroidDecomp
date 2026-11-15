namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// One step of a title card: a timed frame showing the card's text typed so far (or nothing), or a cue (a scene trigger
/// or the card's removal).
/// </summary>
/// <param name="Duration">The frame's duration in title updates, one through 120; zero for a cue.</param>
/// <param name="Typed">The card's text as far as the frame shows it typed, or null for a blank frame or a cue.</param>
/// <param name="Cue">The cue the step runs, or null for a timed frame.</param>
internal readonly record struct TitleCardStep(int Duration, TitleCardTyping? Typed, TitleCardCue? Cue = null)
{
    /// <summary>A timed frame showing <paramref name="typed"/>, or nothing when null, for <paramref name="duration"/> updates.</summary>
    internal static TitleCardStep Show(int duration, TitleCardTyping? typed) => new(duration, typed);

    /// <summary>A step running <paramref name="cue"/>.</summary>
    internal static TitleCardStep Run(TitleCardCue cue) => new(0, null, cue);
}
