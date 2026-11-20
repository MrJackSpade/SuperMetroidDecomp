using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Areas.Escape;

/// <summary>
/// The control words of the cartridge's escape typewriter programs (#2556): the words between a program's character bytes
/// that set its delay, start a line or end it. This is the only place the typewriter's commands meet their cartridge words.
/// </summary>
[EncodingOwner("escape typewriter program control words")]
internal static class EscapeTypewriterControlWords
{
    /// <summary>$A6:C2D6: the control word that sets the character delay from the word that follows.</summary>
    internal const int Delay = 1;

    /// <summary>$A6:C2B7: the control word that ends the program.</summary>
    private const int End = 0;

    /// <summary>$A6:C2E8: the control word that starts a line at the VRAM tilemap word that follows.</summary>
    private const int LineStart = 13;

    /// <summary>The command <paramref name="word"/> is, if it is one that ends a line's characters.</summary>
    internal static bool TryDecode(int word, out EscapeTypewriterCommand command)
    {
        switch (word)
        {
            case End:
                command = EscapeTypewriterCommand.End;
                return true;
            case LineStart:
                command = EscapeTypewriterCommand.LineStart;
                return true;
            default:
                command = default;
                return false;
        }
    }

    /// <summary>The control word the cartridge writes <paramref name="command"/> as.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="command"/> is not a command.</exception>
    internal static int Word(EscapeTypewriterCommand command) => command switch
    {
        EscapeTypewriterCommand.End => End,
        EscapeTypewriterCommand.LineStart => LineStart,
        _ => throw new ArgumentOutOfRangeException(nameof(command), command, "Undefined escape typewriter command."),
    };
}
