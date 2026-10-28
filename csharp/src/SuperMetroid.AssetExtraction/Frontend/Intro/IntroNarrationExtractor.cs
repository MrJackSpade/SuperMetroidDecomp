using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using System.Text;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Frontend.Intro;

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
        Ensure.NotNull(bus);
        var pages = new Dictionary<string, IntroNarrationPage>(StringComparer.Ordinal);
        foreach (IntroNarrationScriptPage source in IntroNarrationScriptSources.Pages)
            pages.Add(source.Id.ToString(), ExtractPage(bus, source));

        using var output = new MemoryStream();
        IntroNarrationPresentation.Write(output, new()
        {
            Version = IntroNarrationDefinitions.Version,
            Pages = pages,
        });
        return output.ToArray();
    }

    /// <summary>Walks one bounded bank-$8C page script and groups consecutive single-tile glyphs by row.</summary>
    private static IntroNarrationPage ExtractPage(
        ISnesAddressSpace bus,
        IntroNarrationScriptPage page)
    {
        ushort cursor = checked((ushort)page.InstructionPointer);
        RequireWord(bus, cursor, page.BeginOpcode, $"{page.Id} begin opcode");
        cursor = Add(cursor, 2);
        RequireRecord(bus, cursor, IntroNarrationDefinitions.InitialMarkerDelayFrames,
            IntroNarrationScriptDefinitions.MarkerPackedPosition,
            IntroNarrationScriptDefinitions.MarkerDataPointer,
            IntroNarrationScriptDefinitions.DoNothingFunction);
        cursor = Add(cursor, 6);

        var rows = new SortedDictionary<int, StringBuilder>();
        int previousRow = -1;
        int previousColumn = 0;
        for (int record = 0; record < IntroNarrationScriptDefinitions.MaximumRecords; record++)
        {
            ushort word = ReadWord(bus, cursor);
            if ((word & IntroNarrationScriptDefinitions.InstructionCommandBit) != 0)
            {
                if (page.Id == IntroNarrationPageId.Page6 &&
                    word == IntroNarrationScriptDefinitions.SetCaretBlinkingOpcode)
                {
                    cursor = Add(cursor, 2);
                    RequireRecord(bus, cursor, IntroNarrationDefinitions.FinalPageHoldFrames,
                        IntroNarrationScriptDefinitions.MarkerPackedPosition,
                        IntroNarrationScriptDefinitions.MarkerDataPointer,
                        IntroNarrationScriptDefinitions.DoNothingFunction);
                    cursor = Add(cursor, 6);
                    word = ReadWord(bus, cursor);
                }
                if (word != page.FinishOpcode || ReadWord(bus, Add(cursor, 2)) !=
                    IntroNarrationScriptDefinitions.DeleteOpcode)
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
                IntroNarrationScriptDefinitions.PackedPositionXMask;
            int row = SnesLittleEndian.HighByte(packedPosition);
            ushort drawFunction = ReadWord(bus, dataPointer);
            int dimensions = SnesLittleEndian.Word(ReadByte(bus, Add(dataPointer, 2)), ReadByte(bus, Add(dataPointer, 3)));
            if (drawFunction != IntroNarrationScriptDefinitions.DrawCharacterFunction ||
                dimensions != IntroNarrationScriptDefinitions.SingleTileDimensions)
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
            $"{IntroNarrationScriptDefinitions.MaximumRecords} records.");
    }

    /// <summary>Checks a marker record's duration, packed position, data pointer, and draw callback.</summary>
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

    /// <summary>Requires an exact native opcode at a page-script address.</summary>
    private static void RequireWord(
        ISnesAddressSpace bus,
        ushort pointer,
        int expected,
        string identity)
    {
        ushort actual = ReadWord(bus, pointer);
        if (actual != expected)
            throw new InvalidDataException(
                $"Opening-narration {identity} is ${actual:X4}, expected ${expected:X4}.");
    }

    /// <summary>Reads one script byte from the fixed narration bank.</summary>
    private static byte ReadByte(ISnesAddressSpace bus, ushort pointer) =>
        bus.ReadCartridgeByte((int)new SnesAddress(
            IntroNarrationScriptDefinitions.ScriptBank, pointer));

    /// <summary>Reads a little-endian narration word with bank-local address wrapping.</summary>
    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank((int)new SnesAddress(IntroNarrationScriptDefinitions.ScriptBank, pointer));

    /// <summary>Advances a script pointer with native 16-bit bank-local wrapping.</summary>
    private static ushort Add(ushort pointer, int bytes) =>
        unchecked((ushort)(pointer + bytes));
}
