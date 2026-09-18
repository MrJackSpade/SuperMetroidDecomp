using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Converts the retail credits row program into bounded, editable UTF-8 content.
/// </summary>
public static class CreditsPresentationExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ushort[][] nativeRows = ReadNativeRows(bus);
        var lines = new CreditsLineDocument[CreditsPresentationDefinitions.Lines.Count];
        int row = 0;
        for (int index = 0; index < lines.Length; index++)
        {
            CreditsLineDefinition definition = CreditsPresentationDefinitions.Lines[index];
            RequireBlankRows(nativeRows, row, definition.BlankRowsBefore, definition.Id);
            row += definition.BlankRowsBefore;
            lines[index] = DecodeLine(nativeRows, ref row, definition);
        }
        RequireBlankRows(nativeRows, row, CreditsPresentationDefinitions.TrailingBlankRows,
            "trailing credits gap");
        row += CreditsPresentationDefinitions.TrailingBlankRows;
        if (row != nativeRows.Length)
            throw new InvalidDataException($"Credits decoding consumed {row} of {nativeRows.Length} rows.");

        using var output = new MemoryStream();
        CreditsPresentation.Write(output, new()
        {
            Version = CreditsPresentationDefinitions.Version,
            Lines = lines,
        });
        return output.ToArray();
    }

    internal static ushort[][] ReadNativeRows(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte[] source = RomDataReader.Decompress(bus, CreditsPresentationDefinitions.Native.Tilemap);
        if (source.Length < CreditsPresentationDefinitions.Native.TilemapBytes)
        {
            throw new InvalidDataException(
                $"Credits tilemap expanded to ${source.Length:X}, expected at least $2000.");
        }

        var rows = new List<ushort[]>(CreditsPresentationDefinitions.ExpectedCompiledRows);
        ushort pointer = CreditsPresentationDefinitions.Native.InitialInstruction;
        ushort timer = 0;
        for (int operation = 0; operation < CreditsPresentationDefinitions.Native.MaximumOperations; operation++)
        {
            ushort instruction = ReadWord(bus, pointer);
            if ((instruction & CreditsPresentationDefinitions.Native.CommandBit) == 0)
            {
                ushort offset = ReadWord(bus, Add(pointer, 2));
                if ((offset & 0x3f) != 0 ||
                    offset + CreditsPresentationDefinitions.TilemapWidth * sizeof(ushort) > source.Length)
                {
                    throw new InvalidDataException(
                        $"Credits row offset ${offset:X4} is unaligned or outside the source tilemap.");
                }
                var row = new ushort[CreditsPresentationDefinitions.TilemapWidth];
                for (int column = 0; column < row.Length; column++)
                {
                    int byteIndex = offset + column * 2;
                    row[column] = unchecked((ushort)(source[byteIndex] | source[byteIndex + 1] << 8));
                }
                rows.Add(row);
                pointer = Add(pointer, 4);
                continue;
            }

            switch (instruction)
            {
                case CreditsPresentationDefinitions.Native.SetTimer:
                    timer = ReadWord(bus, Add(pointer, 2));
                    pointer = Add(pointer, 4);
                    break;
                case CreditsPresentationDefinitions.Native.DecrementTimerAndGoto:
                    timer = unchecked((ushort)(timer - 1));
                    pointer = timer != 0 ? ReadWord(bus, Add(pointer, 2)) : Add(pointer, 4);
                    break;
                case CreditsPresentationDefinitions.Native.EndCredits:
                case CreditsPresentationDefinitions.Native.Delete:
                    if (rows.Count != CreditsPresentationDefinitions.ExpectedCompiledRows)
                    {
                        throw new InvalidDataException(
                            $"Native credits produced {rows.Count} rows, expected " +
                            $"{CreditsPresentationDefinitions.ExpectedCompiledRows}.");
                    }
                    return rows.ToArray();
                default:
                    throw new InvalidDataException(
                        $"Native credits instruction $8B:{instruction:X4} at " +
                        $"$8C:{pointer:X4} is not catalogued.");
            }
        }
        throw new InvalidDataException("Native credits did not terminate within the bounded operation count.");
    }

    private static CreditsLineDocument DecodeLine(
        ushort[][] rows, ref int rowIndex, CreditsLineDefinition definition)
    {
        ushort[] top = rows[rowIndex++];
        ushort[]? bottom = definition.Style == CreditsLineStyle.Large
            ? rows[rowIndex++]
            : null;
        int first = Array.FindIndex(top, word => !IsBlank(word));
        int last = Array.FindLastIndex(top, word => !IsBlank(word));
        if (first < 0 || last < first)
            throw new InvalidDataException($"Native credits line '{definition.Id}' is empty.");

        for (int column = 0; column < first; column++)
            RequireBlankColumn(top, bottom, column, definition.Id);
        for (int column = last + 1; column < CreditsPresentationDefinitions.TilemapWidth; column++)
            RequireBlankColumn(top, bottom, column, definition.Id);

        int palette = top[first] >> CreditsPresentationDefinitions.PaletteShift &
            CreditsPresentationDefinitions.MaximumPalette;
        var text = new char[last - first + 1];
        for (int column = first; column <= last; column++)
        {
            ushort topWord = top[column];
            if (!IsBlank(topWord) &&
                ((topWord >> CreditsPresentationDefinitions.PaletteShift) &
                    CreditsPresentationDefinitions.MaximumPalette) != palette)
            {
                throw new InvalidDataException(
                    $"Native credits line '{definition.Id}' mixes palettes.");
            }
            char glyph = CreditsPresentationDefinitions.DecodeGlyph(topWord, definition.Style);
            if (bottom is not null)
            {
                char lower = CreditsPresentationDefinitions.DecodeGlyph(
                    bottom[column], definition.Style, bottom: true);
                if (lower != glyph)
                {
                    throw new InvalidDataException(
                        $"Native credits line '{definition.Id}' has mismatched glyph halves.");
                }
            }
            text[column - first] = glyph;
        }
        return new CreditsLineDocument
        {
            Id = definition.Id,
            Text = new string(text),
            Column = first,
            Palette = palette,
        };
    }

    private static void RequireBlankColumn(
        ushort[] top, ushort[]? bottom, int column, string identity)
    {
        if (!IsBlank(top[column]) || bottom is not null && !IsBlank(bottom[column]))
        {
            throw new InvalidDataException(
                $"Native credits line '{identity}' has content outside its decoded bounds.");
        }
    }

    private static void RequireBlankRows(ushort[][] rows, int start, int count, string identity)
    {
        if (start + count > rows.Length)
            throw new InvalidDataException($"Native credits end inside '{identity}'.");
        for (int row = start; row < start + count; row++)
        for (int column = 0; column < CreditsPresentationDefinitions.TilemapWidth; column++)
        {
            if (!IsBlank(rows[row][column]))
                throw new InvalidDataException($"Native credits gap before '{identity}' is not blank.");
        }
    }

    private static bool IsBlank(ushort word) =>
        (word & 0x03ff) == CreditsPresentationDefinitions.BlankWord;

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        RomDataReader.ReadWordFixedBank(
            bus, new SnesAddress(CreditsPresentationDefinitions.Native.InstructionBank, pointer));

    private static ushort Add(ushort pointer, int bytes) =>
        unchecked((ushort)(pointer + bytes));
}
