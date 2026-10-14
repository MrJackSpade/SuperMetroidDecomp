using SuperMetroid.Core.Rom;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts bounded post-credit labels while validating native stream mechanics.</summary>
[CpuBusBoundary("codec: ROM post-credit label streams → ending text")]
public static class EndingTextExtractor
{
    /// <summary>Imports post-credit labels, item-percentage wording, Japanese subtitle cells, and the final message while checking native text-stream mechanics.</summary>
    /// <param name="bus">Non-null cartridge import address space containing ending tilemap panels, glyphs, and instruction streams.</param>
    /// <returns>New UTF-8 JSON bytes containing editable text and its native tilemap templates and character placements.</returns>
    /// <remarks>Instruction opcodes, delays, and panel geometry are validated against compiled definitions rather than exported as editable behavior.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="InvalidDataException">A stream command, delay, placement, glyph, character count, or termination differs from the supported layout.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        EndingTextCell[] result = ReadCells(bus, EndingTextDefinitions.Native.ResultPanel,
            EndingTextDefinitions.ResultPanelRows * EndingTextDefinitions.TilemapWidth);
        string producedBy = DecodeRow(result, EndingTextDefinitions.ResultProducedBy);
        EndingTextCell[] copyright = ReadCells(bus, EndingTextDefinitions.Native.CopyrightPanel,
            EndingTextDefinitions.CopyrightRows * EndingTextDefinitions.TilemapWidth);
        string year = DecodeRow(copyright, EndingTextDefinitions.CopyrightYear);
        string company = DecodeRow(copyright, EndingTextDefinitions.CopyrightCompany);
        string percentage = ReadStream(bus, EndingTextDefinitions.Native.ItemPercentage,
            EndingTextSequence.ItemPercentage, out EndingTextCharacter[] percentageCharacters);
        int firstLineLength = EndingTextDefinitions.PercentageHeading.Width;
        if (percentageCharacters.Length != firstLineLength + EndingTextDefinitions.PercentageDetail.Width)
            throw new InvalidDataException("Native item-percentage stream has unexpected text dimensions.");
        string final = ReadStream(bus, EndingTextDefinitions.Native.FinalMessage,
            EndingTextSequence.FinalMessage, out _);

        using var output = new MemoryStream();
        EndingTextPresentation.Write(output, new()
        {
            Version = EndingTextDefinitions.Version,
            ResultPanel = new() { Template = result, Text = producedBy },
            CopyrightYear = year,
            CopyrightCompany = company,
            PercentageHeading = percentage[..firstLineLength],
            PercentageDetail = percentage[firstLineLength..],
            JapaneseSubtitle = ReadCells(bus, EndingTextDefinitions.Native.JapaneseSubtitle,
                EndingTextDefinitions.JapaneseSubtitleRows * EndingTextDefinitions.TilemapWidth),
            FinalMessage = final,
        });
        return output.ToArray();
    }

    /// <summary>Decodes a bounded timed glyph stream while validating command order, positions, and glyph dimensions.</summary>
    private static string ReadStream(ISnesAddressSpace bus, ushort pointer,
        EndingTextSequence sequence, out EndingTextCharacter[] characters)
    {
        ushort cursor = pointer;
        RequireMarker(bus, cursor, EndingTextDefinitions.InitialDelayFrames);
        cursor = Add(cursor, 6);
        var text = new List<char>();
        var compiled = new List<EndingTextCharacter>();
        EndingTextRegionDefinition expected = sequence == EndingTextSequence.ItemPercentage
            ? EndingTextDefinitions.PercentageHeading
            : EndingTextDefinitions.FinalMessage;
        for (int record = 0; record < EndingTextDefinitions.Native.MaximumRecords; record++)
        {
            ushort word = ReadWord(bus, cursor);
            if ((word & EndingTextDefinitions.Native.CommandBit) != 0)
            {
                if (sequence == EndingTextSequence.ItemPercentage)
                {
                    RequireWord(word, EndingTextDefinitions.Native.DrawPercentageOpcode, "percentage count opcode");
                    cursor = Add(cursor, 2);
                    RequireWord(ReadWord(bus, cursor), EndingTextDefinitions.Native.DrawSubtitleOpcode, "percentage subtitle opcode");
                    cursor = Add(cursor, 2);
                    RequireMarker(bus, cursor, EndingTextDefinitions.PercentageHoldFrames);
                    cursor = Add(cursor, 6);
                    RequireWord(ReadWord(bus, cursor), EndingTextDefinitions.Native.ClearSubtitleOpcode, "percentage clear opcode");
                    cursor = Add(cursor, 2);
                }
                RequireWord(ReadWord(bus, cursor), EndingTextDefinitions.Native.DeleteOpcode, "ending-text delete opcode");
                characters = compiled.ToArray();
                return new string(text.ToArray());
            }
            RequireWord(word, EndingTextDefinitions.CharacterDelayFrames, "ending-text character delay");
            ushort packed = ReadWord(bus, Add(cursor, 2));
            int column = packed & EndingTextDefinitions.Native.PackedPositionXMask;
            int row = SnesLittleEndian.HighByte(packed);
            EndingTextStyle style = sequence == EndingTextSequence.ItemPercentage
                ? EndingTextStyle.PercentageSmall : EndingTextStyle.FinalLarge;
            if (sequence == EndingTextSequence.ItemPercentage && compiled.Count == expected.Width)
                expected = EndingTextDefinitions.PercentageDetail;
            int expectedColumn = expected.Column + compiled.Count -
                (expected == EndingTextDefinitions.PercentageDetail ? EndingTextDefinitions.PercentageHeading.Width : 0);
            if (row != expected.Row || column != expectedColumn)
                throw new InvalidDataException($"Native {sequence} record {record} is at ({column},{row}), expected ({expectedColumn},{expected.Row}).");
            ushort data = ReadWord(bus, Add(cursor, 4));
            RequireWord(ReadWord(bus, data), EndingTextDefinitions.Native.DrawFunction, "ending-text draw function");
            int height = ReadByte(bus, Add(data, 3));
            if (ReadByte(bus, Add(data, 2)) != EndingTextDefinitions.Native.GlyphWidth ||
                height != (style == EndingTextStyle.FinalLarge
                    ? EndingTextDefinitions.Native.LargeGlyphHeight
                    : EndingTextDefinitions.Native.SmallGlyphHeight))
                throw new InvalidDataException($"Native {sequence} glyph has unexpected dimensions.");
            ushort top = ReadWord(bus, Add(data, 4));
            char glyph = EndingTextDefinitions.DecodeGlyph(top, style);
            ushort? bottom = height == EndingTextDefinitions.Native.LargeGlyphHeight
                ? ReadWord(bus, Add(data, 6)) : null;
            if (bottom is { } bottomWord && EndingTextDefinitions.DecodeGlyph(bottomWord, style, bottom: true) != glyph)
                throw new InvalidDataException($"Native {sequence} glyph halves disagree.");
            text.Add(glyph);
            compiled.Add(new(row, column, top, bottom));
            cursor = Add(cursor, 6);
        }
        throw new InvalidDataException($"Native {sequence} did not terminate within the bounded record count.");
    }

    /// <summary>Decodes a fixed panel row and confirms both halves of large copyright glyphs agree.</summary>
    private static string DecodeRow(EndingTextCell[] cells, EndingTextRegionDefinition region)
    {
        var text = new char[region.Width];
        for (int index = 0; index < text.Length; index++)
        {
            int cell = region.Row * EndingTextDefinitions.TilemapWidth + region.Column + index;
            text[index] = EndingTextDefinitions.DecodeGlyph(cells[cell].Raw, region.Style);
            if (region.Style == EndingTextStyle.CopyrightLarge &&
                EndingTextDefinitions.DecodeGlyph(cells[cell + EndingTextDefinitions.TilemapWidth].Raw,
                    region.Style, bottom: true) != text[index])
                throw new InvalidDataException("Native copyright glyph halves disagree.");
        }
        return new string(text).TrimEnd();
    }

    /// <summary>Copies a fixed count of native tilemap words into editable ending-text cells.</summary>
    private static EndingTextCell[] ReadCells(ISnesAddressSpace bus, ushort pointer, int count)
    {
        var result = new EndingTextCell[count];
        for (int index = 0; index < count; index++)
            result[index] = new() { Raw = ReadWord(bus, Add(pointer, index * 2)) };
        return result;
    }

    /// <summary>Checks the no-op timing marker's duration, packed position, payload, and function.</summary>
    private static void RequireMarker(ISnesAddressSpace bus, ushort pointer, ushort duration)
    {
        RequireWord(ReadWord(bus, pointer), duration, "ending-text marker duration");
        RequireWord(ReadWord(bus, Add(pointer, 2)), EndingTextDefinitions.Native.MarkerPackedPosition,
            "ending-text marker position");
        RequireWord(ReadWord(bus, Add(pointer, 4)), EndingTextDefinitions.Native.MarkerData, "ending-text marker payload");
        RequireWord(ReadWord(bus, EndingTextDefinitions.Native.MarkerData), EndingTextDefinitions.Native.DoNothingFunction, "ending-text marker function");
    }

    /// <summary>Reports a native ending-text word that differs from its compiled control value.</summary>
    private static void RequireWord(ushort actual, ushort expected, string identity)
    {
        if (actual != expected) throw new InvalidDataException(
            $"Native {identity} is ${actual:X4}, expected ${expected:X4}.");
    }

    /// <summary>Reads one bank-local glyph or stream byte from the import cartridge.</summary>
    private static byte ReadByte(ISnesAddressSpace bus, ushort pointer) =>
        bus.ReadCartridgeByte((int)new SnesAddress(EndingTextDefinitions.Native.Bank, pointer));
    /// <summary>Reads one little-endian bank-local ending-text word from the import cartridge.</summary>
    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer) =>
        CartridgeImportSource.Require(bus).ReadCartridgeWordFixedBank((int)new SnesAddress(EndingTextDefinitions.Native.Bank, pointer));
    /// <summary>Advances a native 16-bit stream pointer by a byte count with cartridge wrap semantics.</summary>
    private static ushort Add(ushort pointer, int bytes) => unchecked((ushort)(pointer + bytes));
}
