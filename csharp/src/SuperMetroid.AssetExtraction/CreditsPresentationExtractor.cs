using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>
/// Converts the retail credits row program into bounded, editable UTF-8 content.
/// </summary>
public static class CreditsPresentationExtractor
{
    /// <summary>Interprets the bounded native credits row program, verifies authored blank-row spacing and glyph coverage, and serializes editable UTF-8 line content.</summary>
    /// <param name="bus">Supported-cartridge address space containing the compressed credits tilemap and row instruction program.</param>
    /// <returns>UTF-8 JSON bytes for the versioned credits presentation document.</returns>
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

    /// <summary>Interprets the bounded cartridge row program and resolves its tilemap row references.</summary>
    /// <param name="bus">Cartridge source containing credits instructions and compressed tilemap data.</param>
    /// <returns>Compiled native rows in their displayed order.</returns>
    internal static ushort[][] ReadNativeRows(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte[] source = RomDataReader.Decompress(CartridgeImportSource.Require(bus), CreditsPresentationDefinitions.Native.Tilemap);
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

    /// <summary>Decodes one authored credits line from its top row and optional large-font lower row.</summary>
    /// <param name="rows">Native tilemap rows produced by <see cref="ReadNativeRows"/>.</param>
    /// <param name="rowIndex">Index of the line's first row; advanced past all rows consumed.</param>
    /// <param name="definition">Line identity, font style, and layout metadata.</param>
    /// <returns>Editable text with its native column and palette selection.</returns>
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

    /// <summary>Rejects content outside the decoded horizontal bounds of a credits line.</summary>
    /// <param name="top">Top tilemap row for the line.</param>
    /// <param name="bottom">Optional lower row for a large-font line.</param>
    /// <param name="column">Column that must be blank in every supplied row.</param>
    /// <param name="identity">Line identifier used in validation errors.</param>
    private static void RequireBlankColumn(
        ushort[] top, ushort[]? bottom, int column, string identity)
    {
        if (!IsBlank(top[column]) || bottom is not null && !IsBlank(bottom[column]))
        {
            throw new InvalidDataException(
                $"Native credits line '{identity}' has content outside its decoded bounds.");
        }
    }

    /// <summary>Verifies the required blank vertical gap in the compiled credits tilemap.</summary>
    /// <param name="rows">All decoded native credits rows.</param>
    /// <param name="start">First row in the gap.</param>
    /// <param name="count">Number of rows required to be blank.</param>
    /// <param name="identity">Adjacent line or gap identity used in validation errors.</param>
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

    /// <summary>Determines whether a credits tilemap word uses the catalogued blank tile.</summary>
    /// <param name="word">Raw tilemap word.</param>
    /// <returns><see langword="true"/> when the tile-number bits identify the blank cell.</returns>
    private static bool IsBlank(ushort word) =>
        (word & 0x03ff) == CreditsPresentationDefinitions.BlankWord;

    /// <summary>Reads one little-endian instruction word from the credits program bank.</summary>
    /// <param name="bus">Cartridge source containing the native program.</param>
    /// <param name="pointer">Bank-relative instruction pointer.</param>
    /// <returns>The instruction or operand at that pointer.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        RomDataReader.ReadWordFixedBank(
            CartridgeImportSource.Require(bus),
            new SnesAddress(CreditsPresentationDefinitions.Native.InstructionBank, pointer));

    /// <summary>Advances a native 16-bit credits program pointer with address-width wraparound.</summary>
    /// <param name="pointer">Current bank-relative pointer.</param>
    /// <param name="bytes">Byte displacement to add.</param>
    /// <returns>The wrapped 16-bit pointer.</returns>
    private static ushort Add(ushort pointer, int bytes) =>
        unchecked((ushort)(pointer + bytes));
}
