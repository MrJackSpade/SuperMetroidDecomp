
namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>A line of an escape typewriter message.</summary>
/// <param name="Message">The message.</param>
/// <param name="Index">The index.</param>
internal readonly record struct EscapeTypewriterLineKey(EscapeTypewriterMessageId Message, int Index);
