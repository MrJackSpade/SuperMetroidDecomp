
namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>A line of an escape typewriter program.</summary>
/// <param name="Message">The program.</param>
/// <param name="Index">The index.</param>
internal readonly record struct EscapeTypewriterLineKey(EscapeTypewriterMessageId Message, int Index);
