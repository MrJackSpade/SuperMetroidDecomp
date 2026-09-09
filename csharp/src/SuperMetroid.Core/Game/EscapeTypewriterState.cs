using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Byte-oriented $A6:C2A7 text interpreter; each accepted call writes at most one glyph.</summary>
public sealed class EscapeTypewriterState(int textAddress, ushort tileBase)
{
    public int Pointer { get; private set; } = textAddress;
    public ushort Destination { get; private set; }
    public ushort Delay { get; private set; }
    public ushort DelayTimer { get; private set; }
    public int GlyphsWritten { get; private set; }
    public bool Completed { get; private set; }
    public bool ClickRequested { get; private set; }

    public bool Step(ISnesAddressSpace bus, SnesVram vram)
    {
        ClickRequested = false;
        if (Completed) return true;
        if (DelayTimer != 0) { DelayTimer--; return false; }
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

    private void Advance(int count) => Pointer = (Pointer & ~ushort.MaxValue) | unchecked((ushort)(Pointer + count));
    private static ushort Word(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | bus.ReadByte((address & ~ushort.MaxValue) | unchecked((ushort)(address + 1))) << 8);
}
