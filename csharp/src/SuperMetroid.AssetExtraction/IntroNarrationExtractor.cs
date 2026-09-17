using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using System.Text;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts the six safe English narration pages from native bank-$8C scripts.</summary>
public static class IntroNarrationExtractor
{
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

    private static byte ReadByte(ISnesAddressSpace bus, ushort pointer) =>
        bus.ReadByte((int)new SnesAddress(
            IntroNarrationDefinitions.Native.ScriptBank, pointer));

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        unchecked((ushort)(ReadByte(bus, pointer) | ReadByte(bus, Add(pointer, 1)) << 8));

    private static ushort Add(ushort pointer, int bytes) =>
        unchecked((ushort)(pointer + bytes));
}
