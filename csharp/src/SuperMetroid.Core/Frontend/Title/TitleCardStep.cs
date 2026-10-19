namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// One step of a title card: a timed frame showing a title composition (or nothing), or a command (a scene trigger or
/// the card's deletion).
/// </summary>
/// <param name="Duration">The frame's duration in title updates, one through 120; zero for a command.</param>
/// <param name="Sprite">The composition the frame shows, or null for a blank frame or a command.</param>
/// <param name="Command">The command the step runs, or null for a timed frame.</param>
internal readonly record struct TitleCardStep(int Duration, TitleSprite? Sprite, TitleSequenceInstruction? Command = null)
{
    /// <summary>A timed frame showing <paramref name="sprite"/>, or nothing when null, for <paramref name="duration"/> updates.</summary>
    internal static TitleCardStep Show(int duration, TitleSprite? sprite) => new(duration, sprite);

    /// <summary>A step running <paramref name="command"/>.</summary>
    internal static TitleCardStep Run(TitleSequenceInstruction command) => new(0, null, command);
}
