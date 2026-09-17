using System.Text;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEndingTextPresentation(string romPath)
    {
        ISnesAddressSpace nativeBus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        byte[] extracted = SuperMetroid.AssetExtraction.EndingTextExtractor.Extract(nativeBus);
        EndingTextPresentation presentation = EndingTextPresentation.Load(
            new MemoryStream(extracted, writable: false));
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
            ushort[] nativeTilemap = Enumerable.Repeat(
                EndingCreditsRomData.Rendering.BlankTile,
                EndingCreditsRomData.Rendering.TilemapWords).ToArray();
            ushort[] installedTilemap = nativeTilemap.ToArray();
            ushort pointer = sequence == EndingTextSequence.ItemPercentage
                ? EndingCreditsRomData.Instructions.ItemPercentageText
                : EndingCreditsRomData.Instructions.SeeYouNextMissionText;
            var native = new EndingBackgroundTextState(nativeBus, nativeTilemap, pointer,
                default, japaneseText: true);
            var installed = new EndingBackgroundTextState(installedBus, installedTilemap, pointer,
                default, japaneseText: true, presentation: presentation,
                installedSequence: sequence);
            var nativeVram = new SnesVram();
            var installedVram = new SnesVram();
            for (int frame = 0; frame < 2048; frame++)
            {
                native.Step(nativeVram);
                installed.Step(installedVram);
                AssertTrue(nativeTilemap.AsSpan().SequenceEqual(installedTilemap),
                    $"installed ending {sequence} tilemap frame {frame}");
                AssertEqual(native.Completed, installed.Completed,
                    $"installed ending {sequence} completion frame {frame}");
                AssertEqual(native.RequestedItemPercentageScroll,
                    installed.RequestedItemPercentageScroll,
                    $"installed ending {sequence} scroll request frame {frame}");
                comparedFrames++;
                if (native.Completed) break;
                if (frame == 2047)
                    throw new InvalidOperationException($"Ending {sequence} did not terminate.");
            }
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
        Console.WriteLine($"Ending text: exact producer/copyright panels and {comparedFrames} native typewriter frames match with bank-$8C text reads forbidden; edits, restore and validation pass.");
    }

    private static ushort[] ReadEndingWords(ISnesAddressSpace bus, ushort pointer, int count)
    {
        var result = new ushort[count];
        int address = (int)new SnesAddress(EndingTextDefinitions.Native.Bank, pointer);
        for (int index = 0; index < count; index++)
            result[index] = unchecked((ushort)(bus.ReadByte(address + index * 2) |
                bus.ReadByte(address + index * 2 + 1) << 8));
        return result;
    }

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

    private sealed class EndingTextReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            int bank = address >> 16;
            int offset = address & 0xffff;
            if (bank == EndingTextDefinitions.Native.Bank &&
                offset is >= EndingTextDefinitions.Native.ResultPanel and <= 0xe1e8)
                throw new InvalidOperationException($"Installed ending text read cartridge address ${address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
