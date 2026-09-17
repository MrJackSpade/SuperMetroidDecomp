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
        if (ProgramId != EscapeTypewriterProgramId.None)
        {
            EscapeTypewriterProgram program = installedProgram ??
                throw new InvalidOperationException(
                    $"Escape typewriter program {ProgramId} was restored without host content rebind.");
            return StepInstalled(program, vram);
        }
        // The reload precedes command parsing, so a new delay first affects the next call.
        DelayTimer = Delay;
        for (int commandCount = 0; commandCount < 256; commandCount++)
        {
            ushort command = Word(bus, Pointer);
            if (command == EscapeTypewriterRomData.End) return Completed = true;
            if (command == EscapeTypewriterRomData.Delay)
            {
                Delay = Word(bus, Pointer + 2);
                Advance(4);
                continue;
            }
            if (command == EscapeTypewriterRomData.Destination)
            {
                Destination = Word(bus, Pointer + 2);
                Advance(4);
                continue;
            }
            byte character = (byte)command;
            Advance(1);
            if (character != (byte)' ')
            {
                if (character == (byte)'!') character = EscapeTypewriterRomData.ExclamationGlyph;
                ushort tile = unchecked((ushort)(tileBase + character - EscapeTypewriterRomData.FirstLetter));
                vram.ExecuteWordTransfer([tile], Destination, 1);
                GlyphsWritten++;
                ClickRequested = GlyphsWritten % 2 == 0;
            }
            Destination++;
            return false;
        }
        throw new InvalidDataException("Escape typewriter did not reach a character or terminator in 256 commands.");
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

    private void Advance(int count) => Pointer = (Pointer & ~ushort.MaxValue) | unchecked((ushort)(Pointer + count));
    private static ushort Word(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte((address & ~ushort.MaxValue) | unchecked((ushort)(address + 1))) << 8);
}
