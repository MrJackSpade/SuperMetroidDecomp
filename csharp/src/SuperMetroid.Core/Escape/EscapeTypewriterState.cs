using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Escape;

/// <summary>Types the escape warning one character at a time; each accepted update types at most one glyph.</summary>
public sealed partial class EscapeTypewriterState
{
    /// <summary>The installed warning text the typewriter types.</summary>
    private EscapeTypewriterMessage installedMessage;

    /// <summary>Creates a typewriter for <paramref name="message"/>, the installed text it types.</summary>
    /// <param name="message">Installed Ceres or Zebes warning lines and their identity.</param>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is null.</exception>
    public EscapeTypewriterState(EscapeTypewriterMessage message)
    {
        installedMessage = Ensure.NotNull(message);
        MessageId = message.Id;
    }

    /// <summary>
    /// The pixel of the layer the next character's top-left corner is typed at: reset to each line's start at its first
    /// character and advanced right by a glyph's width for both written glyphs and spaces (#1406, #1514, #1601).
    /// </summary>
    internal Position16<PlanePixels> Corner { get; set; }
    /// <summary>
    /// The wait before the typewriter types again (#2368): none after its opening character, then
    /// <see cref="EscapeTypewriterDefinitions.CharacterDelayFrames"/> after each later character.
    /// </summary>
    private readonly UpdateCountdown _characterWait = UpdateCountdown.Ended();
    /// <summary>The updates that have typed a character, space or line end, and the one that found the warning complete.</summary>
    private readonly ElapsedUpdates _typing = new();
    /// <summary>Total non-space glyphs typed, whose count paces the clicks; spaces advance the typing position but are not counted.</summary>
    private int _glyphsWritten;
    /// <summary>True once an update ready to type passes the end of all lines; later updates report completion without typing or requesting a sound.</summary>
    public bool Completed { get; private set; }
    /// <summary>The Ceres or Zebes warning the typewriter types, which any text bound to it again must be.</summary>
    public EscapeTypewriterMessageId MessageId { get; private set; }
    /// <summary>The installed line being typed, from zero; moves past exhausted lines on an eligible update and reaches the line count when typing completes.</summary>
    private int _lineIndex;
    /// <summary>The next character to type in the current line, from zero, including spaces; resets to zero when the typewriter advances to another line.</summary>
    private int _characterIndex;

    /// <summary>Binds the installed text of the warning the typewriter types, as when the host installs new text mid-warning.</summary>
    /// <exception cref="InvalidDataException"><paramref name="message"/> is the other warning's text.</exception>
    public void BindMessage(EscapeTypewriterMessage message)
    {
        Ensure.NotNull(message);
        if (MessageId != message.Id)
            throw new InvalidDataException(
                $"Cannot bind the {message.Id} escape warning to a typewriter typing the {MessageId} warning.");
        installedMessage = message;
    }

    /// <summary>Advances the warning by one gameplay update, typing at most one glyph or consuming one space when its wait is over.</summary>
    /// <returns>
    /// What the update did: typed nothing while waiting or on a space, typed a glyph for the owner to write over its
    /// background, with the key click after every second glyph, or, once all lines have been typed, found the warning
    /// complete. The update that types the final character is a typing update, not the completed one.
    /// </returns>
    /// <remarks>
    /// The opening characters are typed on consecutive updates; each later character follows its predecessor after its
    /// delay's updates that type nothing (<see cref="EscapeTypewriterDefinitions.UndelayedOpeningCharacters"/>,
    /// <see cref="EscapeTypewriterDefinitions.CharacterDelayFrames"/>).
    /// </remarks>
    public EscapeTypewriterStep Step()
    {
        if (Completed) return EscapeTypewriterStep.Complete.Instance;
        if (_characterWait.IsRunning)
        {
            _characterWait.Count();
            return EscapeTypewriterStep.NothingTyped.Instance;
        }
        _characterWait.Restart(_typing.Age < EscapeTypewriterDefinitions.UndelayedOpeningCharacters
            ? 0 : EscapeTypewriterDefinitions.CharacterDelayFrames);
        _typing.Count();
        return StepInstalled(installedMessage);
    }

    /// <summary>Moves on to the next line and character, consuming at most one character.</summary>
    private EscapeTypewriterStep StepInstalled(EscapeTypewriterMessage message)
    {
        while (_lineIndex < message.Lines.Count)
        {
            EscapeTypewriterLine line = message.Lines[_lineIndex];
            if (_characterIndex >= line.Text.Length)
            {
                _lineIndex++;
                _characterIndex = 0;
                continue;
            }
            if (_characterIndex == 0)
                Corner = line.Start;
            return TypeCharacter(line.Text[_characterIndex++]);
        }
        Completed = true;
        return EscapeTypewriterStep.Complete.Instance;
    }

    /// <summary>Types a non-space glyph, clicking every second glyph, and advances the typing position by a glyph's width.</summary>
    private EscapeTypewriterStep TypeCharacter(char character)
    {
        EscapeTypewriterStep step = EscapeTypewriterStep.NothingTyped.Instance;
        if (EscapeTypewriterGlyph.Types(character))
        {
            var glyph = new EscapeTypewriterGlyph(Corner, EscapeTypewriterGlyph.PictureOf(MessageId, character));
            _glyphsWritten++;
            step = EscapeTypewriterDefinitions.GlyphClick.IsDue(_glyphsWritten)
                ? new EscapeTypewriterStep.TypedWithClick(glyph)
                : new EscapeTypewriterStep.Typed(glyph);
        }
        Corner += new Offset16<PlanePixels>(EscapeTypewriterDefinitions.GlyphAdvance, 0);
        return step;
    }
}
