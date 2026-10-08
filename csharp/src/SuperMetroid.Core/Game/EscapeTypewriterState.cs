using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Byte-oriented $A6:C2A7 text interpreter; each accepted call writes at most one glyph.</summary>
public sealed class EscapeTypewriterState
{
    private readonly ushort tileBase;
    [NonSerialized] private EscapeTypewriterProgram? installedProgram;

    /// <summary>Creates an unbound warning-text state retaining the native source identity; attach host text with <see cref="BindProgram"/> before interpreting it.</summary>
    /// <param name="textAddress">Native 24-bit source address retained in <see cref="Pointer"/>; this constructor does not read cartridge memory.</param>
    /// <param name="tileBase">Complete tilemap word for the letter A, including tile index and presentation attributes.</param>
    public EscapeTypewriterState(int textAddress, ushort tileBase)
    {
        Pointer = textAddress;
        this.tileBase = tileBase;
    }

    /// <summary>Creates a warning-text state with its host program bound and its saved identity initialized from that program.</summary>
    /// <param name="program">Installed Ceres or Zebes warning lines and their native source identity.</param>
    /// <param name="tileBase">Complete tilemap word for the letter A, including tile index and presentation attributes.</param>
    /// <exception cref="ArgumentNullException"><paramref name="program"/> is null.</exception>
    public EscapeTypewriterState(EscapeTypewriterProgram program, ushort tileBase)
        : this(program?.SourceAddress ?? throw new ArgumentNullException(nameof(program)), tileBase)
    {
        ProgramId = program.Id;
        BindProgram(program);
    }

    /// <summary>Retained native text source address for compatibility watches; installed text advances the line and character cursors rather than this address.</summary>
    public int Pointer { get; private set; }
    /// <summary>VRAM word address of the next tilemap cell, reset at each line's first character and incremented for both written glyphs and spaces.</summary>
    public ushort Destination { get; private set; }
    /// <summary>Countdown reload in <see cref="Step"/> calls; initially zero and set to the two-call character delay when the program first starts.</summary>
    public ushort Delay { get; private set; }
    /// <summary>Calls still to skip before interpreting another character; a call that decrements it to zero still performs no character work.</summary>
    public ushort DelayTimer { get; private set; }
    /// <summary>Total non-space glyphs transferred to VRAM; spaces advance the destination but do not increment this count.</summary>
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
        ArgumentNullException.ThrowIfNull(program);
        if (ProgramId != EscapeTypewriterProgramId.None && ProgramId != program.Id)
            throw new InvalidDataException(
                $"Cannot bind escape typewriter program {program.Id} to saved {ProgramId} state.");
        ProgramId = program.Id;
        installedProgram = program;
    }

    /// <summary>Removes host content while retaining the saved program identity and cursor.</summary>
    public void UnbindProgram() => installedProgram = null;

    /// <summary>Advances the warning by one gameplay call, writing at most one glyph or consuming one space when the delay has expired.</summary>
    /// <param name="bus">Compatibility address-space argument; installed-program interpretation performs no reads through it.</param>
    /// <param name="vram">VRAM receiving each non-space character's single tilemap word.</param>
    /// <returns>True once all lines have been consumed; false during countdowns and character processing, including the final character's call.</returns>
    /// <remarks>The timer reload uses the previous delay before first-start initialization, so the first two character calls are consecutive; later characters skip two intervening calls.</remarks>
    /// <exception cref="InvalidOperationException">An eligible call requires text, but no host program is bound after construction or restoration.</exception>
    public bool Step(ISnesAddressSpace bus, SnesVram vram)
    {
        ClickRequested = false;
        if (Completed) return true;
        if (DelayTimer != 0) { DelayTimer--; return false; }
        EscapeTypewriterProgram program = installedProgram ??
            throw new InvalidOperationException(
                $"Escape typewriter program {ProgramId} was restored without host content rebind.");
        return StepInstalled(program, vram);
    }

    private bool StepInstalled(EscapeTypewriterProgram program, SnesVram vram)
    {
        DelayTimer = Delay;
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
                Destination = line.Destination;
            byte character = (byte)line.Text[ProgramCharacterIndex++];
            WriteCharacter(character, vram);
            return false;
        }
        return Completed = true;
    }

    private void WriteCharacter(byte character, SnesVram vram)
    {
        if (character != (byte)' ')
        {
            if (character == (byte)'!') character = EscapeTypewriterRomData.ExclamationGlyph;
            ushort tile = unchecked((ushort)(tileBase + character - EscapeTypewriterRomData.FirstLetter));
            vram.ExecuteWordTransfer([tile], Destination, 1);
            GlyphsWritten++;
            ClickRequested = GlyphsWritten % 2 == 0;
        }
        Destination++;
    }

}
