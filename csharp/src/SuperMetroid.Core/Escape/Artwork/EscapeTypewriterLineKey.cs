
namespace SuperMetroid.Core.Escape.Artwork;

/// <summary>A line of an escape typewriter program.</summary>
/// <param name="Program">The program.</param>
/// <param name="Index">The index.</param>
internal readonly record struct EscapeTypewriterLineKey(EscapeTypewriterProgramId Program, int Index);
