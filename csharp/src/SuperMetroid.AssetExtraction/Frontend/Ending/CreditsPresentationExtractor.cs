using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>
/// Converts the retail credits row program into bounded, editable UTF-8 content.
/// </summary>
[EncodingOwner("cartridge credits instruction words")]
public static class CreditsPresentationExtractor
{
    /// <summary>Interprets the bounded native credits row program, verifies authored blank-row spacing and glyph coverage, and serializes editable UTF-8 line content.</summary>
    /// <param name="bus">Supported-cartridge address space containing the compressed credits tilemap and row instruction program.</param>
    /// <returns>UTF-8 JSON bytes for the versioned credits presentation document.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ushort[][] nativeRows = ReadNativeRows(bus);
        var lines = new CreditsLineDocument[CreditsRoles.Lines.Count];
        int row = 0;
        for (int index = 0; index < lines.Length; index++)
        {
            CreditsLineDefinition definition = CreditsRoles.Lines[index];
            RequireBlankRows(nativeRows, row, definition.BlankRowsBefore, definition.Id);
            row += definition.BlankRowsBefore;
            lines[index] = DecodeLine(nativeRows, ref row, definition);
        }
        RequireBlankRows(nativeRows, row, CreditsRollDefinitions.TrailingBlankRows,
            "trailing credits gap");
        row += CreditsRollDefinitions.TrailingBlankRows;
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

    /// <summary>Interprets bank 8C row instructions into bounded 32-word tilemap rows.</summary>
    internal static ushort[][] ReadNativeRows(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        byte[] source = RomDataReader.Decompress(CartridgeImportSource.Require(bus), EndingSourceDefinitions.Credits.Tilemap);
        if (source.Length < EndingSourceDefinitions.Credits.TilemapBytes)
        {
            throw new InvalidDataException(
                $"Credits tilemap expanded to ${source.Length:X}, expected at least $2000.");
        }

        var rows = new List<ushort[]>(CreditsRollDefinitions.ExpectedCompiledRows);
        int pointer = EndingSourceDefinitions.Credits.InitialInstruction;
        WrappingCountdown timer = WrappingCountdown.Start(0);
        for (int operation = 0; operation < EndingSourceDefinitions.Credits.MaximumOperations; operation++)
        {
            int instruction = ReadWord(bus, pointer);
            if ((instruction & EndingSourceDefinitions.Credits.CommandBit) == 0)
            {
                int offset = ReadWord(bus, Add(pointer, 2));
                if (offset % (CreditsPresentationDefinitions.TilemapWidth * sizeof(ushort)) != 0 ||
                    offset + CreditsPresentationDefinitions.TilemapWidth * sizeof(ushort) > source.Length)
                {
                    throw new InvalidDataException(
                        $"Credits row offset ${offset:X4} is unaligned or outside the source tilemap.");
                }
                var row = new ushort[CreditsPresentationDefinitions.TilemapWidth];
                for (int column = 0; column < row.Length; column++)
                {
                    int byteIndex = offset + column * 2;
                    row[column] = unchecked(LittleEndianBytes.Word(source[byteIndex], source[byteIndex + 1]));
                }
                rows.Add(row);
                pointer = Add(pointer, 4);
                continue;
            }

            var command = (CreditsInstruction)instruction;
            if (!Enum.IsDefined(command))
            {
                throw new InvalidDataException(
                    $"Native credits instruction $8B:{instruction:X4} at " +
                    $"$8C:{pointer:X4} is not catalogued.");
            }
            switch (command)
            {
                case CreditsInstruction.SetTimer:
                    timer = WrappingCountdown.FromRaw(ReadWord(bus, Add(pointer, 2)));
                    pointer = Add(pointer, 4);
                    break;
                case CreditsInstruction.DecrementTimerAndGoto:
                    timer = timer.Decrement().Remaining;
                    pointer = !timer.IsZero ? ReadWord(bus, Add(pointer, 2)) : Add(pointer, 4);
                    break;
                case CreditsInstruction.EndCredits:
                case CreditsInstruction.Delete:
                    if (rows.Count != CreditsRollDefinitions.ExpectedCompiledRows)
                    {
                        throw new InvalidDataException(
                            $"Native credits produced {rows.Count} rows, expected " +
                            $"{CreditsRollDefinitions.ExpectedCompiledRows}.");
                    }
                    return rows.ToArray();
                default:
                    throw new InvalidOperationException($"Undefined CreditsInstruction {command}.");
            }
        }
        throw new InvalidDataException("Native credits did not terminate within the bounded operation count.");
    }

    /// <summary>Decodes one line's tile bounds, palette, and matching halves into editable text.</summary>
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

        int palette = BackgroundTileWords.ToTile(top[first]).ColourVariant;
        var text = new char[last - first + 1];
        for (int column = first; column <= last; column++)
        {
            BackgroundTile topTile = BackgroundTileWords.ToTile(top[column]);
            if (!IsBlank(top[column]) && topTile.ColourVariant != palette)
            {
                throw new InvalidDataException(
                    $"Native credits line '{definition.Id}' mixes palettes.");
            }
            char glyph = CreditsGlyphs.DecodeGlyph(topTile, definition.Style);
            if (bottom is not null)
            {
                char lower = CreditsGlyphs.DecodeGlyph(
                    BackgroundTileWords.ToTile(bottom[column]), definition.Style, bottom: true);
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

    /// <summary>Rejects tile content outside the decoded horizontal bounds of a credits line.</summary>
    private static void RequireBlankColumn(
        ushort[] top, ushort[]? bottom, int column, string identity)
    {
        if (!IsBlank(top[column]) || bottom is not null && !IsBlank(bottom[column]))
        {
            throw new InvalidDataException(
                $"Native credits line '{identity}' has content outside its decoded bounds.");
        }
    }

    /// <summary>Checks the authored empty-row gap before a line or after the final line.</summary>
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

    /// <summary>Recognizes the credits blank tile while ignoring its colour variant, priority and mirroring.</summary>
    private static bool IsBlank(int word) =>
        BackgroundTileWords.ToTile(word).Tile == CreditsPresentationDefinitions.BlankTile.Tile;

    /// <summary>Reads one instruction word from the credits program's native bank.</summary>
    private static ushort ReadWord(ISnesAddressSpace bus, int pointer) =>
        RomDataReader.ReadWordFixedBank(
            CartridgeImportSource.Require(bus),
            SnesAddress.InBank(EndingSourceDefinitions.Credits.InstructionBank, pointer));

    /// <summary>Advances a bank-local credits instruction pointer with native 16-bit wrapping.</summary>
    private static int Add(int pointer, int bytes) =>
        unchecked((ushort)(pointer + bytes));
}
