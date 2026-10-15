namespace SuperMetroid.Core.Frontend.Title;

/// <summary>
/// One step of a title card: a timed frame showing a bank-$8C spritemap, or a command (a scene trigger or the card's
/// deletion).
/// </summary>
/// <param name="Duration">The frame's duration in title updates; zero for a command.</param>
/// <param name="Spritemap">The bank-$8C spritemap the frame shows; zero for a command.</param>
/// <param name="Command">The command the step runs, or null for a timed frame.</param>
internal readonly record struct TitleCardStep(ushort Duration, ushort Spritemap, TitleSequenceInstruction? Command = null)
{
    /// <summary>A timed frame showing <paramref name="spritemap"/> for <paramref name="duration"/> updates.</summary>
    internal static TitleCardStep Show(ushort duration, ushort spritemap) => new(duration, spritemap);

    /// <summary>A step running <paramref name="command"/>.</summary>
    internal static TitleCardStep Run(TitleSequenceInstruction command) => new(0, 0, command);
}
