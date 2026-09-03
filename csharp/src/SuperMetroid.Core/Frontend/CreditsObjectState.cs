using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The single credits object at <c>$8B:9932-$8B:99FD</c> and its bank-$8C instruction
/// stream beginning at <c>$8C:D91B</c>.
/// </summary>
/// <remarks>
/// Credit text is never represented as host strings. The cartridge's compressed row
/// tilemap is copied through the same circular 32-row staging buffer used by the SNES, so
/// regional spelling, omissions, spacing, and the intentionally rearranged sections all
/// remain ROM-authored.
/// </remarks>
internal sealed class CreditsObjectState
{
    private const int RowByteCount =
        EndingCreditsRomData.Rendering.TilemapWidth * sizeof(ushort);

    private readonly ISnesAddressSpace bus;
    private readonly byte[] sourceRows;
    private readonly ushort[] tilemap =
        new ushort[EndingCreditsRomData.Rendering.TilemapWords];
    private ushort instructionPointer = EndingCreditsRomData.Instructions.CreditsInitial;
    private ushort instructionTimer;
    private ushort scrollWhole;
    private ushort scrollSubposition;
    private ushort previousCopiedScroll;
    private int destinationRow;

    public CreditsObjectState(ISnesAddressSpace bus)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        sourceRows = RomDataReader.Decompress(bus, EndingCreditsRomData.Assets.CreditsTilemap);
        if (sourceRows.Length < EndingCreditsRomData.Rendering.CreditsSourceBytes)
        {
            throw new InvalidDataException(
                $"Credits tilemap expanded to ${sourceRows.Length:X}, expected at least $2000.");
        }
        Array.Fill(tilemap, EndingCreditsRomData.Rendering.BlankTile);
    }

    public bool Enabled => instructionPointer != 0;
    public bool Finished { get; private set; }
    public ushort VerticalScroll => scrollWhole;
    public int DestinationRow => destinationRow;
    public ushort InstructionPointer => instructionPointer;
    public ushort InstructionTimer => instructionTimer;
    public ReadOnlySpan<ushort> Tilemap => tilemap;

    /// <summary>Runs one <c>CreditsObject_Process</c> call.</summary>
    public CreditsObjectStepResult Step()
    {
        if (!Enabled)
            return new CreditsObjectStepResult(false, Finished, destinationRow, scrollWhole);

        // AddToHiLo($198F,$198D,$00008000): half a pixel per accepted frame.
        uint fixedScroll = ((uint)scrollWhole << 16) | scrollSubposition;
        fixedScroll = unchecked(
            fixedScroll + EndingCreditsRomData.Motion.CreditsScrollDelta16Point16);
        scrollWhole = unchecked((ushort)(fixedScroll >> 16));
        scrollSubposition = unchecked((ushort)fixedScroll);

        // A new source row is interpreted whenever the scroll advances eight whole pixels,
        // i.e. every sixteen NTSC frames. The signed modular comparison is intentional.
        if ((short)unchecked((ushort)(scrollWhole - previousCopiedScroll - 8)) < 0)
            return new CreditsObjectStepResult(false, Finished, destinationRow, scrollWhole);

        previousCopiedScroll = scrollWhole;
        bool copied = InterpretUntilRowOrStop();
        return new CreditsObjectStepResult(copied, Finished, destinationRow, scrollWhole);
    }

    /// <summary>Uploads the live circular tilemap to native BG1 base word $4800.</summary>
    public void UploadTilemap(SnesVram vram)
    {
        ArgumentNullException.ThrowIfNull(vram);
        vram.ExecuteWordTransfer(
            tilemap,
            EndingCreditsRomData.Rendering.CreditsTilemapWord,
            wordIncrement: 1);
    }

    private bool InterpretUntilRowOrStop()
    {
        // A valid retail row can be preceded by a timer and a timer-loop branch. Bound the
        // loop so a damaged stream fails at the opcode that caused it instead of hanging.
        for (int operation = 0; operation < 32; operation++)
        {
            ushort word = ReadInstructionWord(instructionPointer);
            if ((word & CinematicCodePointers.InstructionCommandBit) == 0)
            {
                ushort sourceOffset = ReadInstructionWord(
                    unchecked((ushort)(instructionPointer + 2)));
                CopySourceRow(sourceOffset);
                instructionPointer = unchecked((ushort)(instructionPointer + 4));
                destinationRow = (destinationRow + 1) &
                    (EndingCreditsRomData.Rendering.TilemapHeight - 1);
                return true;
            }

            switch (word)
            {
                case CinematicCodePointers.CreditsObject_Instruction_SetTimer:
                    instructionTimer = ReadInstructionWord(
                        unchecked((ushort)(instructionPointer + 2)));
                    instructionPointer = unchecked((ushort)(instructionPointer + 4));
                    break;

                case CinematicCodePointers.CreditsObject_Instruction_DecrementTimerAndGoto:
                    instructionTimer = unchecked((ushort)(instructionTimer - 1));
                    instructionPointer = instructionTimer != 0
                        ? ReadInstructionWord(unchecked((ushort)(instructionPointer + 2)))
                        : unchecked((ushort)(instructionPointer + 4));
                    break;

                case CinematicCodePointers.CreditsObject_Instruction_EndCredits:
                    // $8B:F6FE disables the object, forces blank, installs the post-credit
                    // palette, and arms the following cinematic. The outer ending owner
                    // performs those cross-system effects; this interpreter publishes the
                    // exact boundary and stops before consuming the following delete word.
                    Finished = true;
                    instructionPointer = 0;
                    return false;

                case CinematicCodePointers.CreditsObject_Instruction_Delete:
                    instructionPointer = 0;
                    return false;

                default:
                    throw new InvalidDataException(
                        $"Credits opcode $8B:{word:X4} at $8C:{instructionPointer:X4} is not valid.");
            }
        }

        throw new InvalidDataException(
            $"Credits stream at $8C:{instructionPointer:X4} did not reach a row within 32 operations.");
    }

    private void CopySourceRow(ushort sourceOffset)
    {
        if (sourceOffset + RowByteCount > sourceRows.Length)
        {
            throw new InvalidDataException(
                $"Credits row offset ${sourceOffset:X4} exceeds the $2000-byte source tilemap.");
        }

        int destination = destinationRow * EndingCreditsRomData.Rendering.TilemapWidth;
        for (int column = 0; column < EndingCreditsRomData.Rendering.TilemapWidth; column++)
        {
            int source = sourceOffset + column * 2;
            tilemap[destination + column] = unchecked((ushort)(
                sourceRows[source] | (sourceRows[source + 1] << 8)));
        }
    }

    private ushort ReadInstructionWord(ushort pointer) =>
        RomDataReader.ReadWordFixedBank(
            bus,
            EndingCreditsRomData.Instructions.Bank.AddWithinBank(pointer));
}

internal readonly record struct CreditsObjectStepResult(
    bool CopiedRow,
    bool Finished,
    int NextDestinationRow,
    ushort VerticalScroll);
