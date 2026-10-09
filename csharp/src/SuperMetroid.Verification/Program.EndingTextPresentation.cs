using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>
    /// Verifies installed ending text panels and typewriter sequences against cartridge data,
    /// including editable text and font assets without runtime reads of the guarded text stream.
    /// </summary>
    /// <param name="romPath">Path to the retail ROM used to extract and compare ending assets.</param>
    private static void VerifyEndingTextPresentation(string romPath)
    {
        ISnesAddressSpace nativeBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.EndingTextExtractor.Extract(nativeBus);
        EndingTextPresentation presentation = EndingTextPresentation.Load(
            new MemoryStream(extracted, writable: false));
        byte[] fontPng = SuperMetroid.AssetExtraction.EndingFontAtlasExtractor.Extract(nativeBus);
        EndingFontAtlas font = EndingFontAtlas.Load(new MemoryStream(fontPng, writable: false));
        byte[] nativeFont = RomDataReader.Decompress(SuperMetroid.Core.Rom.CartridgeImportSource.Require(nativeBus),
            EndingCreditsRomData.Assets.EndingFontCharacters,
            EndingCreditsRomData.Rendering.Mode7Bytes);
        AssertTrue(font.Transfer.Span.SequenceEqual(
            nativeFont.AsSpan(0, EndingFontAtlasFormat.ByteCount)),
            "installed ending-font PNG compiles to exact native planar bytes");
        ISnesAddressSpace installedBus = new EndingTextReadGuard(nativeBus);

        AssertTrue(presentation.BuildResultPanel().AsSpan().SequenceEqual(
            ReadEndingWords(nativeBus, EndingTextDefinitions.Native.ResultPanel,
                EndingTextDefinitions.ResultPanelRows * EndingTextDefinitions.TilemapWidth)),
            "installed producer panel matches native tilemap");
        AssertTrue(presentation.BuildCopyrightPanel().AsSpan().SequenceEqual(
            ReadEndingWords(nativeBus, EndingTextDefinitions.Native.CopyrightPanel,
                EndingTextDefinitions.CopyrightRows * EndingTextDefinitions.TilemapWidth)),
            "installed copyright panel matches native tilemap");

        int comparedFrames = 0;
        foreach (EndingTextSequence sequence in Enum.GetValues<EndingTextSequence>())
        {
            ushort[] installedTilemap = Enumerable.Repeat(
                EndingCreditsRomData.Rendering.BlankTile,
                EndingCreditsRomData.Rendering.TilemapWords).ToArray();
            ushort pointer = sequence == EndingTextSequence.ItemPercentage
                ? EndingCreditsRomData.Instructions.ItemPercentageText
                : EndingCreditsRomData.Instructions.SeeYouNextMissionText;
            // The installed sequence runs to completion with its text streams guarded.
            var installed = new EndingBackgroundTextState(installedBus, installedTilemap, pointer,
                default, japaneseText: true, presentation: presentation,
                installedSequence: sequence);
            var installedVram = new SnesVram();
            for (int frame = 0; !installed.Completed; frame++)
            {
                if (frame == 2048)
                    throw new InvalidOperationException($"Ending {sequence} did not terminate.");
                installed.Step(installedVram);
                comparedFrames++;
            }
            AssertTrue(installedTilemap.Any(word => word != EndingCreditsRomData.Rendering.BlankTile),
                $"installed ending {sequence} draws its text");
        }

        JsonObject editedDocument = JsonNode.Parse(extracted)!.AsObject();
        editedDocument["percentageHeading"] = "TEST";
        editedDocument["finalMessage"] = "TEST";
        editedDocument["resultPanel"]!["text"] = "TEST";
        byte[] editedBytes = Encoding.UTF8.GetBytes(
            editedDocument.ToJsonString(MapPresentationFormat.JsonOptions));
        EndingTextPresentation edited = EndingTextPresentation.Load(
            new MemoryStream(editedBytes, writable: false));
        AssertEqual(EndingTextDefinitions.CompileGlyph('T', EndingTextStyle.ResultSmall),
            edited.BuildResultPanel()[EndingTextDefinitions.ResultProducedBy.Column],
            "edited producer text reaches its bounded panel");
        AssertEqual(EndingTextDefinitions.ResultBlankWord,
            edited.BuildResultPanel()[EndingTextDefinitions.ResultProducedBy.Column + 4],
            "shorter producer text clears the remainder of its bounded panel");
        AssertEqual(EndingTextDefinitions.CompileGlyph('T', EndingTextStyle.PercentageSmall),
            edited.Compile(EndingTextSequence.ItemPercentage)[0].TopWord,
            "edited percentage text reaches its live program");

        var saved = new EndingBackgroundTextState(nativeBus,
            Enumerable.Repeat(EndingCreditsRomData.Rendering.BlankTile,
                EndingCreditsRomData.Rendering.TilemapWords).ToArray(),
            EndingCreditsRomData.Instructions.SeeYouNextMissionText,
            default, false, presentation: presentation,
            installedSequence: EndingTextSequence.FinalMessage);
        saved.Step(new SnesVram());
        using var snapshot = new MemoryStream();
        SuperMetroid.Desktop.DebuggerObjectGraphSerializer.Serialize(snapshot, saved);
        snapshot.Position = 0;
        var restored = SuperMetroid.Desktop.DebuggerObjectGraphSerializer
            .Deserialize<EndingBackgroundTextState>(snapshot);
        AssertThrows<InvalidOperationException>(() => restored.Step(new SnesVram()),
            "restored ending text fails loudly until host content is rebound");
        restored.BindPresentation(presentation);
        restored.Step(new SnesVram());

        editedDocument["finalMessage"] = "lowercase";
        AssertThrows<InvalidDataException>(() => EndingTextPresentation.Load(
            new MemoryStream(Encoding.UTF8.GetBytes(
                editedDocument.ToJsonString(MapPresentationFormat.JsonOptions)), writable: false)),
            "ending text rejects glyphs absent from its documented font mapping");
        IndexedPngImage editableFont = IndexedPng.Read(
            new MemoryStream(fontPng, writable: false),
            EndingFontAtlasFormat.Width, EndingFontAtlasFormat.Height);
        editableFont.Pixels[0] ^= 1;
        using var editedFontPng = new MemoryStream();
        IndexedPng.Write(editedFontPng, editableFont.Width, editableFont.Height,
            editableFont.Pixels, editableFont.Palette);
        editedFontPng.Position = 0;
        EndingFontAtlas editedFont = EndingFontAtlas.Load(editedFontPng);
        AssertTrue(!editedFont.Transfer.Span.SequenceEqual(font.Transfer.Span),
            "ending-font PNG edit changes compiled VRAM bytes");
        Console.WriteLine($"Ending text: exact producer/copyright panels and {comparedFrames} native typewriter frames match with bank-$8C text reads forbidden; edits, restore and validation pass.");
        Console.WriteLine("Ending font: 160 PNG tiles compile to exact native 4-bpp bytes; an indexed-pixel edit changes installed VRAM content.");
    }

    /// <summary>Reads a contiguous run of little-endian tilemap words from the ending-text bank.</summary>
    /// <param name="bus">Address space supplying the cartridge bytes.</param>
    /// <param name="pointer">Bank-local address of the first tilemap word.</param>
    /// <param name="count">Number of adjacent words to copy.</param>
    /// <returns>Tilemap words in the same order as their addresses.</returns>
    private static ushort[] ReadEndingWords(ISnesAddressSpace bus, ushort pointer, int count)
    {
        var result = new ushort[count];
        int address = (int)new SnesAddress(EndingTextDefinitions.Native.Bank, pointer);
        for (int index = 0; index < count; index++)
            result[index] = unchecked((ushort)(bus.ReadByte(address + index * 2) |
                bus.ReadByte(address + index * 2 + 1) << 8));
        return result;
    }

    /// <summary>Checks deterministic stock text extraction, override compilation and identity, and rejection of corrupt JSON.</summary>
    /// <param name="bus">Retail cartridge address space used for extraction.</param>
    /// <param name="stockDirectory">Directory containing the installed stock ending-text document.</param>
    /// <param name="overrideDirectory">Directory used for the temporary text override.</param>
    /// <param name="stockCatalog">Unmodified catalog used to compare content identity after editing.</param>
    private static void VerifyEndingTextAssets(
        ISnesAddressSpace bus,
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentationCatalog stockCatalog)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.EndingTextExtractor.Extract(bus);
        AssertTrue(deterministic.AsSpan().SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, EndingTextDefinitions.FileName))),
            "installed ending text is the deterministic cartridge extraction");
        Directory.CreateDirectory(overrideDirectory);
        JsonObject document = JsonNode.Parse(deterministic)!.AsObject();
        document["finalMessage"] = "TEST";
        string replacement = Path.Combine(overrideDirectory, EndingTextDefinitions.FileName);
        File.WriteAllText(replacement, document.ToJsonString(MapPresentationFormat.JsonOptions));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(
            stockDirectory, overrideDirectory);
        AssertEqual(EndingTextDefinitions.CompileGlyph('T', EndingTextStyle.FinalLarge),
            edited.EndingText.Compile(EndingTextSequence.FinalMessage)[0].TopWord,
            "catalog override changes installed ending text");
        AssertTrue(stockCatalog.ContentIdentity != edited.ContentIdentity,
            "ending-text override changes catalog content identity");
        File.WriteAllText(replacement, "{ broken ending text");
        AssertThrows<InvalidDataException>(() =>
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory),
            "corrupt ending-text override fails loudly");
        Console.WriteLine("Ending-text catalog: deterministic stock, override identity and corruption failure pass.");
    }

    /// <summary>Checks deterministic font extraction, PNG override compilation and identity, and rejection of corrupt images.</summary>
    /// <param name="bus">Retail cartridge address space used for extraction.</param>
    /// <param name="stockDirectory">Directory containing the installed stock font atlas.</param>
    /// <param name="overrideDirectory">Directory used for the temporary indexed-PNG override.</param>
    /// <param name="stockCatalog">Unmodified catalog used to compare transfer bytes and content identity.</param>
    private static void VerifyEndingFontAssets(
        ISnesAddressSpace bus,
        string stockDirectory,
        string overrideDirectory,
        AreaMapPresentationCatalog stockCatalog)
    {
        byte[] deterministic = SuperMetroid.AssetExtraction.EndingFontAtlasExtractor.Extract(bus);
        AssertTrue(deterministic.AsSpan().SequenceEqual(File.ReadAllBytes(
            Path.Combine(stockDirectory, EndingFontAtlasFormat.FileName))),
            "installed ending font is the deterministic cartridge extraction");
        IndexedPngImage image = IndexedPng.Read(new MemoryStream(deterministic, writable: false),
            EndingFontAtlasFormat.Width, EndingFontAtlasFormat.Height);
        image.Pixels[0] ^= 1;
        Directory.CreateDirectory(overrideDirectory);
        string replacement = Path.Combine(overrideDirectory, EndingFontAtlasFormat.FileName);
        using (var output = File.Create(replacement))
            IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(!edited.EndingFont.Transfer.Span.SequenceEqual(
            stockCatalog.EndingFont.Transfer.Span),
            "ending-font override changes installed VRAM bytes");
        AssertTrue(stockCatalog.ContentIdentity != edited.ContentIdentity,
            "ending-font override changes catalog content identity");
        File.WriteAllText(replacement, "not a PNG");
        AssertThrows<InvalidDataException>(() =>
            AreaMapPresentationCatalog.Load(stockDirectory, overrideDirectory),
            "corrupt ending-font override fails loudly");
        Console.WriteLine("Ending-font catalog: deterministic stock, PNG override identity and corruption failure pass.");
    }

    /// <summary>Prevents installed ending-text execution from falling back to native text-stream reads.</summary>
    /// <param name="source">Underlying address space for cartridge reads outside the guarded ending-text range.</param>
    private sealed class EndingTextReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes import reads through the guarded byte-read path.</summary>
        /// <param name="address">Cartridge byte address requested by the importer.</param>
        /// <returns>The underlying byte when the address is not part of the guarded text stream.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from native ending text streams and forwards other byte reads.</summary>
        /// <param name="address">Cartridge byte address requested by the caller.</param>
        /// <returns>The wrapped address-space byte for an allowed address.</returns>
        public byte ReadByte(int address)
        {
            int bank = address >> 16;
            int offset = address & 0xffff;
            if (bank == EndingTextDefinitions.Native.Bank &&
                offset is >= EndingTextDefinitions.Native.ResultPanel and <= 0xe1e8)
                throw new InvalidOperationException($"Installed ending text read cartridge address ${address:X6}.");
            return source.ReadByte(address);
        }
        /// <summary>Forwards a byte write unchanged to the wrapped address space.</summary>
        /// <param name="address">Cartridge byte address to update.</param>
        /// <param name="value">Byte value to store at that address.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
