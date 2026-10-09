using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using System.Text;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the six safe English narration pages from native bank-$8C scripts.</summary>
public static class IntroNarrationExtractor
{
    /// <summary>Decodes all six English opening narration pages from checked bank-$8C character scripts into editable text lines.</summary>
    /// <param name="bus">Cartridge source for the page instruction streams and their single-tile glyph payloads.</param>
    /// <returns>A new UTF-8 JSON buffer with page-name-keyed line arrays, native tile-row positions, and decoded text in row order.</returns>
    /// <remarks>Verifies begin/end/delete commands, marker records, five-update character delays, contiguous columns, and the final-page caret/hold sequence. Those script mechanics remain compiled rather than becoming editable JSON fields.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A script opcode, marker, timing, glyph payload, coordinate sequence, or bounded termination differs from the supported format.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var pages = new Dictionary<string, IntroNarrationPage>(StringComparer.Ordinal);
        foreach (IntroNarrationNativePage source in IntroNarrationDefinitions.Pages)
            pages.Add(source.Id.ToString(), ExtractPage(bus, source));

        using var output = new MemoryStream();
        IntroNarrationPresentation.Write(output, new()
        {
            Version = IntroNarrationDefinitions.Version,
            Pages = pages,
        });
        return output.ToArray();
    }

    /// <summary>Decodes one bounded narration page and checks its native script sequence.</summary>
    /// <param name="bus">Cartridge address space containing the bank-$8C stream and glyph records.</param>
    /// <param name="page">Compiled page identity, instruction pointer, and expected command opcodes.</param>
    /// <returns>Editable lines decoded from the page's native glyph placements.</returns>
    private static IntroNarrationPage ExtractPage(
        ISnesAddressSpace bus,
        IntroNarrationNativePage page)
    {
        ushort cursor = page.InstructionPointer;
        RequireWord(bus, cursor, page.BeginOpcode, $"{page.Id} begin opcode");
        cursor = Add(cursor, 2);
        RequireRecord(bus, cursor, IntroNarrationDefinitions.InitialMarkerDelayFrames,
            IntroNarrationDefinitions.Native.MarkerPackedPosition,
            IntroNarrationDefinitions.Native.MarkerDataPointer,
            IntroNarrationDefinitions.Native.DoNothingFunction);
        cursor = Add(cursor, 6);

        var rows = new SortedDictionary<int, StringBuilder>();
        int previousRow = -1;
        int previousColumn = 0;
        for (int record = 0; record < IntroNarrationDefinitions.Native.MaximumRecords; record++)
        {
            ushort word = ReadWord(bus, cursor);
            if ((word & IntroNarrationDefinitions.Native.InstructionCommandBit) != 0)
            {
                if (page.Id == IntroNarrationPageId.Page6 &&
                    word == IntroNarrationDefinitions.Native.SetCaretBlinkingOpcode)
                {
                    cursor = Add(cursor, 2);
                    RequireRecord(bus, cursor, IntroNarrationDefinitions.FinalPageHoldFrames,
                        IntroNarrationDefinitions.Native.MarkerPackedPosition,
                        IntroNarrationDefinitions.Native.MarkerDataPointer,
                        IntroNarrationDefinitions.Native.DoNothingFunction);
                    cursor = Add(cursor, 6);
                    word = ReadWord(bus, cursor);
                }
                if (word != page.FinishOpcode || ReadWord(bus, Add(cursor, 2)) !=
                    IntroNarrationDefinitions.Native.DeleteOpcode)
                {
                    throw new InvalidDataException(
                        $"Opening-narration {page.Id} ended with unexpected opcode ${word:X4} at $8C:{cursor:X4}.");
                }
                return new()
                {
                    Lines = rows.Select(pair => new IntroNarrationLine
                    {
                        Row = pair.Key,
                        Text = pair.Value.ToString(),
                    }).ToArray(),
                };
            }

            if (word != IntroNarrationDefinitions.CharacterDelayFrames)
                throw new InvalidDataException(
                    $"Opening-narration {page.Id} character delay was {word}, expected 5.");
            ushort packedPosition = ReadWord(bus, Add(cursor, 2));
            ushort dataPointer = ReadWord(bus, Add(cursor, 4));
            int column = packedPosition &
                IntroNarrationDefinitions.Native.PackedPositionXMask;
            int row = packedPosition >> 8;
            ushort drawFunction = ReadWord(bus, dataPointer);
            int dimensions = ReadByte(bus, Add(dataPointer, 2)) |
                ReadByte(bus, Add(dataPointer, 3)) << 8;
            if (drawFunction != IntroNarrationDefinitions.Native.DrawCharacterFunction ||
                dimensions != IntroNarrationDefinitions.Native.SingleTileDimensions)
                throw new InvalidDataException(
                    $"Opening-narration {page.Id} character record at $8C:{cursor:X4} has an invalid glyph payload.");
            if (row != previousRow)
            {
                if (rows.ContainsKey(row) || column != IntroNarrationDefinitions.FirstColumn)
                    throw new InvalidDataException(
                        $"Opening-narration {page.Id} starts row {row} at invalid column {column}.");
                rows.Add(row, new StringBuilder());
                previousRow = row;
                previousColumn = 0;
            }
            if (column != previousColumn + 1)
                throw new InvalidDataException(
                    $"Opening-narration {page.Id} row {row} skips from column {previousColumn} to {column}.");
            rows[row].Append(IntroNarrationDefinitions.DecodeGlyph(
                ReadWord(bus, Add(dataPointer, 4))));
            previousColumn = column;
            cursor = Add(cursor, 6);
        }

        throw new InvalidDataException(
            $"Opening-narration {page.Id} did not terminate within " +
            $"{IntroNarrationDefinitions.Native.MaximumRecords} records.");
    }

    /// <summary>Checks one native narration marker's delay, position, data pointer, and draw function.</summary>
    /// <param name="bus">Cartridge address space containing the marker and marker payload.</param>
    /// <param name="pointer">Bank-local address of the marker record.</param>
    /// <param name="duration">Required marker delay.</param>
    /// <param name="packedPosition">Required encoded row and column.</param>
    /// <param name="dataPointer">Required marker payload address.</param>
    /// <param name="drawFunction">Required marker callback word.</param>
    private static void RequireRecord(
        ISnesAddressSpace bus,
        ushort pointer,
        ushort duration,
        ushort packedPosition,
        ushort dataPointer,
        ushort drawFunction)
    {
        if (ReadWord(bus, pointer) != duration ||
            ReadWord(bus, Add(pointer, 2)) != packedPosition ||
            ReadWord(bus, Add(pointer, 4)) != dataPointer ||
            ReadWord(bus, dataPointer) != drawFunction)
        {
            throw new InvalidDataException(
                $"Opening-narration marker at $8C:{pointer:X4} does not match the retail stream.");
        }
    }

    /// <summary>Rejects a native instruction word that differs from the page contract.</summary>
    /// <param name="bus">Cartridge address space containing the word.</param>
    /// <param name="pointer">Bank-local address to inspect.</param>
    /// <param name="expected">Required opcode or operand.</param>
    /// <param name="identity">Instruction identity included in mismatch diagnostics.</param>
    private static void RequireWord(
        ISnesAddressSpace bus,
        ushort pointer,
        ushort expected,
        string identity)
    {
        ushort actual = ReadWord(bus, pointer);
        if (actual != expected)
            throw new InvalidDataException(
                $"Opening-narration {identity} is ${actual:X4}, expected ${expected:X4}.");
    }

    /// <summary>Reads one byte from the opening narration script bank.</summary>
    /// <param name="bus">Cartridge address space supplying the byte.</param>
    /// <param name="pointer">Bank-local address to read.</param>
    /// <returns>The native byte.</returns>
    private static byte ReadByte(ISnesAddressSpace bus, ushort pointer) =>
        bus.ReadCartridgeByte((int)new SnesAddress(
            IntroNarrationDefinitions.Native.ScriptBank, pointer));

    /// <summary>Reads a little-endian word from the opening narration script bank.</summary>
    /// <param name="bus">Cartridge address space supplying the bytes.</param>
    /// <param name="pointer">Bank-local address of the low byte.</param>
    /// <returns>The decoded word.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        unchecked((ushort)(ReadByte(bus, pointer) | ReadByte(bus, Add(pointer, 1)) << 8));

    /// <summary>Adds a byte displacement with the bank-local address width used by the script.</summary>
    /// <param name="pointer">Starting bank-local address.</param>
    /// <param name="bytes">Byte displacement.</param>
    /// <returns>The wrapped 16-bit address.</returns>
    private static ushort Add(ushort pointer, int bytes) =>
        unchecked((ushort)(pointer + bytes));
}
