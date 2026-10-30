using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Escape;

/// <summary>Byte-oriented $A6:C2A7 text interpreter; each accepted call writes at most one glyph.</summary>
public sealed class EscapeTypewriterState
{
    /// <summary>Rebound host text excluded from serialized interpreter state.</summary>
    [NonSerialized] private EscapeTypewriterProgram? installedProgram;

    /// <summary>Creates an unbound warning-text state for <paramref name="programId"/>; attach host text with <see cref="BindProgram"/> before interpreting it.</summary>
    /// <param name="programId">The warning program the state will show.</param>
    public EscapeTypewriterState(EscapeTypewriterProgramId programId) => ProgramId = programId;

    /// <summary>Creates a warning-text state with its host program bound and its saved identity initialized from that program.</summary>
    /// <param name="program">Installed Ceres or Zebes warning lines and their native source identity.</param>
    /// <exception cref="ArgumentNullException"><paramref name="program"/> is null.</exception>
    public EscapeTypewriterState(EscapeTypewriterProgram program)
    {
        Ensure.NotNull(program);
        ProgramId = program.Id;
        BindProgram(program);
    }

    /// <summary>
    /// The overlay cell the next character is typed over: reset at each line's first character and advanced for both
    /// written glyphs and spaces (#1406).
    /// </summary>
    public int Cell { get; private set; }
    /// <summary>Countdown reload in <see cref="Step"/> calls: zero, then the two-call character delay once the program starts.</summary>
    public int Delay { get; private set; }
    /// <summary>Calls still to skip before interpreting another character; a call that decrements it to zero still performs no character work.</summary>
    public WrappingCountdown DelayTimer { get; private set; }
    /// <summary>Total non-space glyphs typed; spaces advance the cell but do not increment this count.</summary>
    public int GlyphsWritten { get; private set; }
    /// <summary>True after an eligible interpretation call passes the end of all lines; subsequent calls return completion without writing or requesting sound.</summary>
    public bool Completed { get; private set; }
    /// <summary>One-call sound request raised after every second non-space glyph, cleared at the start of every <see cref="Step"/>; the caller selects the Ceres or Zebes click sound.</summary>
    public bool ClickRequested { get; private set; }
    /// <summary>Saved Ceres or Zebes program identity used to validate content rebinding; initially None for the native-address-only constructor.</summary>
    public EscapeTypewriterProgramId ProgramId { get; private set; }
    /// <summary>Zero-based installed line cursor; advances past exhausted lines on an eligible call and reaches the line count when interpretation completes.</summary>
    public int ProgramLineIndex { get; private set; }
    /// <summary>Zero-based next-character cursor within the current line, including spaces; resets to zero when the interpreter advances to another line.</summary>
    public int ProgramCharacterIndex { get; private set; }
    /// <summary>Whether the first eligible installed-program call has initialized the character delay; retained across host-content unbinding and restoration.</summary>
    public bool ProgramStarted { get; private set; }

    /// <summary>Reattaches host content after debugger-state restoration.</summary>
    public void BindProgram(EscapeTypewriterProgram program)
    {
        Ensure.NotNull(program);
        if (ProgramId != EscapeTypewriterProgramId.None && ProgramId != program.Id)
            throw new InvalidDataException(
                $"Cannot bind escape typewriter program {program.Id} to saved {ProgramId} state.");
        ProgramId = program.Id;
        installedProgram = program;
    }

    /// <summary>Removes host content while retaining the saved program identity and cursor.</summary>
    public void UnbindProgram() => installedProgram = null;

    /// <summary>Advances the warning by one gameplay call, typing at most one glyph or consuming one space when the delay has expired.</summary>
    /// <param name="glyph">Receives the glyph typed this call, for the owner to write over its background; null otherwise.</param>
    /// <returns>True once all lines have been consumed; false during countdowns and character processing, including the final character's call.</returns>
    /// <remarks>The timer reload uses the previous delay before first-start initialization, so the first two character calls are consecutive; later characters skip two intervening calls.</remarks>
    /// <exception cref="InvalidOperationException">An eligible call requires text, but no host program is bound after construction or restoration.</exception>
    public bool Step(out EscapeTypewriterGlyph? glyph)
    {
        ClickRequested = false;
        glyph = null;
        if (Completed) return true;
        if (!DelayTimer.IsZero) { DelayTimer = DelayTimer.Decrement().Remaining; return false; }
        EscapeTypewriterProgram program = installedProgram ??
            throw new InvalidOperationException(
                $"Escape typewriter program {ProgramId} was restored without host content rebind.");
        return StepInstalled(program, out glyph);
    }

    /// <summary>Advances line and character cursors after the delay expires, consuming at most one character.</summary>
    private bool StepInstalled(EscapeTypewriterProgram program, out EscapeTypewriterGlyph? glyph)
    {
        glyph = null;
        DelayTimer = WrappingCountdown.Start(Delay);
        if (!ProgramStarted)
        {
            Delay = EscapeTypewriterDefinitions.CharacterDelayFrames;
            ProgramStarted = true;
        }

        while (ProgramLineIndex < program.Lines.Count)
        {
            EscapeTypewriterLine line = program.Lines[ProgramLineIndex];
            if (ProgramCharacterIndex >= line.Text.Length)
            {
                ProgramLineIndex++;
                ProgramCharacterIndex = 0;
                continue;
            }
            if (ProgramCharacterIndex == 0)
                Cell = line.Cell;
            int character = (byte)line.Text[ProgramCharacterIndex++];
            glyph = TypeCharacter(character);
            return false;
        }
        return Completed = true;
    }

    /// <summary>Whether a line of <paramref name="length"/> glyphs typed from <paramref name="cell"/> stays inside the overlay.</summary>
    internal static bool LineFitsOverlay(int cell, int length) =>
        cell >= 0 && cell + length <= RoomWrittenOverlay.CellCount;

    /// <summary>Types a non-space glyph, requests a click every second glyph, and advances the cell.</summary>
    private EscapeTypewriterGlyph? TypeCharacter(int character)
    {
        EscapeTypewriterGlyph? glyph = null;
        if (EscapeTypewriterGlyph.Types((char)character))
        {
            glyph = new(Cell, EscapeTypewriterGlyph.PictureOf(ProgramId, (char)character));
            GlyphsWritten++;
            ClickRequested = GlyphsWritten % 2 == 0;
        }
        Cell++;
        return glyph;
    }
}
