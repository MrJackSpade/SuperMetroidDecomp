using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Byte-oriented $A6:C2A7 text interpreter; each accepted call writes at most one glyph.</summary>
public sealed class EscapeTypewriterState
{
    private readonly ushort tileBase;
    [NonSerialized] private EscapeTypewriterProgram? installedProgram;

    public EscapeTypewriterState(int textAddress, ushort tileBase)
    {
        Pointer = textAddress;
        this.tileBase = tileBase;
    }

    public EscapeTypewriterState(EscapeTypewriterProgram program, ushort tileBase)
        : this(program?.SourceAddress ?? throw new ArgumentNullException(nameof(program)), tileBase)
    {
        ProgramId = program.Id;
        BindProgram(program);
    }

    public int Pointer { get; private set; }
    public ushort Destination { get; private set; }
    public ushort Delay { get; private set; }
    public ushort DelayTimer { get; private set; }
    public int GlyphsWritten { get; private set; }
    public bool Completed { get; private set; }
    public bool ClickRequested { get; private set; }
    public EscapeTypewriterProgramId ProgramId { get; private set; }
    public int ProgramLineIndex { get; private set; }
    public int ProgramCharacterIndex { get; private set; }
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

        while (ProgramLineIndex < program.Lines.Length)
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
