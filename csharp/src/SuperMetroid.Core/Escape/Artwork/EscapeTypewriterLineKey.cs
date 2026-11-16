
namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>A line of an escape typewriter message.</summary>
/// <param name="Message">The message the line belongs to.</param>
/// <param name="Index">The line's place in the message, the first line zero.</param>
internal readonly record struct EscapeTypewriterLineKey(EscapeTypewriterMessageId Message, int Index);
