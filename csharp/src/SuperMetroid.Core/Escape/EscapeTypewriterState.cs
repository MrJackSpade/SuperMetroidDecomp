using SuperMetroid.Core.Escape.Artwork;

namespace SuperMetroid.Core.Escape;

/// <summary>Types the escape warning one character at a time; each accepted call types at most one glyph.</summary>
public sealed class EscapeTypewriterState
{
    /// <summary>The installed warning text the typewriter types.</summary>
    private EscapeTypewriterMessage? installedMessage;

    /// <summary>Creates an unbound warning-text state for <paramref name="programId"/>; attach host text with <see cref="BindMessage"/> before typing it.</summary>
    /// <param name="programId">The warning program the state will show.</param>
    public EscapeTypewriterState(EscapeTypewriterMessageId programId) => MessageId = programId;

    /// <summary>Creates a warning-text state with its host program bound and its saved identity initialized from that program.</summary>
    /// <param name="program">Installed Ceres or Zebes warning lines and their source identity.</param>
    /// <exception cref="ArgumentNullException"><paramref name="program"/> is null.</exception>
    public EscapeTypewriterState(EscapeTypewriterMessage program)
    {
        Ensure.NotNull(program);
        MessageId = program.Id;
        BindMessage(program);
    }

    /// <summary>
    /// The pixel of the layer the next character's top-left corner is typed at: reset to each line's start at its first
    /// character and advanced right by a glyph's width for both written glyphs and spaces (#1406, #1514, #1601).
    /// </summary>
    public Position16<PlanePixels> Corner { get; private set; }
    /// <summary>Countdown reload in <see cref="Step"/> calls: zero, then the two-call character delay once the program starts.</summary>
    public int Delay { get; private set; }
    /// <summary>Calls still to skip before typing another character; a call that decrements it to zero still performs no character work.</summary>
    public int DelayTimer { get; private set; }
    /// <summary>Total non-space glyphs typed; spaces advance the typing position but do not increment this count.</summary>
    public int GlyphsWritten { get; private set; }
    /// <summary>True after an eligible typing call passes the end of all lines; subsequent calls return completion without writing or requesting sound.</summary>
    public bool Completed { get; private set; }
    /// <summary>One-call sound request raised after every second non-space glyph, cleared at the start of every <see cref="Step"/>; the caller selects the Ceres or Zebes click sound.</summary>
    public bool ClickRequested { get; private set; }
    /// <summary>Saved Ceres or Zebes program identity used to validate content rebinding; the program the state was created for.</summary>
    public EscapeTypewriterMessageId MessageId { get; private set; }
    /// <summary>Zero-based installed line cursor; advances past exhausted lines on an eligible call and reaches the line count when typing completes.</summary>
    public int MessageLineIndex { get; private set; }
    /// <summary>Zero-based next-character cursor within the current line, including spaces; resets to zero when the typewriter advances to another line.</summary>
    public int MessageCharacterIndex { get; private set; }
    /// <summary>Whether the first eligible installed-program call has initialized the character delay; retained across host-content unbinding and restoration.</summary>
    public bool MessageStarted { get; private set; }

    /// <summary>Reattaches the installed typewriter text when a saved game state is restored.</summary>
    public void BindMessage(EscapeTypewriterMessage program)
    {
        Ensure.NotNull(program);
        if (MessageId != EscapeTypewriterMessageId.None && MessageId != program.Id)
            throw new InvalidDataException(
                $"Cannot bind escape typewriter program {program.Id} to saved {MessageId} state.");
        MessageId = program.Id;
        installedMessage = program;
    }

    /// <summary>Removes host content while retaining the saved program identity and cursor.</summary>
    public void UnbindMessage() => installedMessage = null;

    /// <summary>Advances the warning by one gameplay call, typing at most one glyph or consuming one space when the delay has expired.</summary>
    /// <param name="glyph">Receives the glyph typed this call, for the owner to write over its background; null otherwise.</param>
    /// <returns>True once all lines have been consumed; false during countdowns and character processing, including the final character's call.</returns>
    /// <remarks>The timer reload uses the previous delay before first-start initialization, so the first two character calls are consecutive; later characters skip two intervening calls.</remarks>
    /// <exception cref="InvalidOperationException">An eligible call requires text, but no installed program is bound.</exception>
    public bool Step(out EscapeTypewriterGlyph? glyph)
    {
        ClickRequested = false;
        glyph = null;
        if (Completed) return true;
        if (DelayTimer != 0) { DelayTimer--; return false; }
        EscapeTypewriterMessage program = installedMessage ??
            throw new InvalidOperationException(
                $"Escape typewriter program {MessageId} was restored without host content rebind.");
        return StepInstalled(program, out glyph);
    }

    /// <summary>Advances line and character cursors after the delay expires, consuming at most one character.</summary>
    private bool StepInstalled(EscapeTypewriterMessage program, out EscapeTypewriterGlyph? glyph)
    {
        glyph = null;
        DelayTimer = Delay;
        if (!MessageStarted)
        {
            Delay = EscapeTypewriterDefinitions.CharacterDelayFrames;
            MessageStarted = true;
        }

        while (MessageLineIndex < program.Lines.Count)
        {
            EscapeTypewriterLine line = program.Lines[MessageLineIndex];
            if (MessageCharacterIndex >= line.Text.Length)
            {
                MessageLineIndex++;
                MessageCharacterIndex = 0;
                continue;
            }
            if (MessageCharacterIndex == 0)
                Corner = line.Start;
            glyph = TypeCharacter(line.Text[MessageCharacterIndex++]);
            return false;
        }
        return Completed = true;
    }

    /// <summary>Types a non-space glyph, requests a click every second glyph, and advances the typing position by a glyph's width.</summary>
    private EscapeTypewriterGlyph? TypeCharacter(char character)
    {
        EscapeTypewriterGlyph? glyph = null;
        if (EscapeTypewriterGlyph.Types(character))
        {
            glyph = new(Corner, EscapeTypewriterGlyph.PictureOf(MessageId, character));
            GlyphsWritten++;
            ClickRequested = EscapeTypewriterDefinitions.ClickEverySecondGlyph.IsDue(GlyphsWritten);
        }
        Corner += new Offset16<PlanePixels>(EscapeTypewriterDefinitions.GlyphAdvance, 0);
        return glyph;
    }
}
