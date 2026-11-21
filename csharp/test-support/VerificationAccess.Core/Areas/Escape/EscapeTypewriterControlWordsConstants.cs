namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>Cartridge values and helpers of <see cref="EscapeTypewriterControlWords"/> that only verification reads.</summary>
internal static class EscapeTypewriterControlWordsConstants
{
    /// <summary>The control word the cartridge writes <paramref name="command"/> as.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="command"/> is not a command.</exception>
    internal static int Word(EscapeTypewriterCommand command) => EscapeTypewriterControlWords.Words.TryGetValue(command, out int word)
        ? word
        : throw new ArgumentOutOfRangeException(nameof(command), command, "Undefined escape typewriter command.");
}
