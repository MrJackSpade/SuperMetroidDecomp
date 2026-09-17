using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyEscapeTimerPresentationAssets(ISnesAddressSpace bus, string stock,
        string overrides, AreaMapPresentationCatalog original)
    {
        string stockPath = Path.Combine(stock, EscapeTimerPresentationDefinitions.FileName);
        string stockTilePath = Path.Combine(stock, EscapeTimerTileAtlasFormat.FileName);
        byte[] extracted = SuperMetroid.AssetExtraction.EscapeTimerPresentationExtractor.Extract(bus);
        AssertTrue(extracted.AsSpan().SequenceEqual(File.ReadAllBytes(stockPath)),
            "installed timer JSON is the deterministic cartridge extraction");
        byte[] extractedTiles = SuperMetroid.AssetExtraction.EscapeTimerTileAtlasExtractor.Extract(bus);
        AssertTrue(extractedTiles.AsSpan().SequenceEqual(File.ReadAllBytes(stockTilePath)),
            "installed timer PNG is the deterministic cartridge extraction");

        var tileAtlas = EscapeTimerTileAtlas.Load(new MemoryStream(extractedTiles));
        byte[] nativeFirstTiles = SuperMetroid.Core.Rom.RomDataReader.ReadFixedBank(bus,
            EscapeTimerTileRomData.FirstSourceAddress, EscapeTimerTileAtlasFormat.FirstByteCount);
        byte[] nativeSecondTiles = SuperMetroid.Core.Rom.RomDataReader.ReadFixedBank(bus,
            EscapeTimerTileRomData.SecondSourceAddress, EscapeTimerTileAtlasFormat.SecondByteCount);
        AssertTrue(nativeFirstTiles.AsSpan().SequenceEqual(tileAtlas.Resolve(VramAssetId.EscapeTimerFirstTiles).Span),
            "first timer PNG page compiles to exact native characters");
        AssertTrue(nativeSecondTiles.AsSpan().SequenceEqual(tileAtlas.Resolve(VramAssetId.EscapeTimerSecondTiles).Span),
            "second timer PNG page compiles to exact native characters");

        var queued = new VramWriteQueue();
        tileAtlas.QueueTo(queued);
        AssertEqual(2, queued.Entries.Count, "timer PNG retains two native transfer records");
        AssertEqual(VramAssetId.EscapeTimerFirstTiles, queued.Entries[0].AssetId,
            "timer PNG first record uses typed installed artwork");
        AssertEqual(VramAssetId.EscapeTimerSecondTiles, queued.Entries[1].AssetId,
            "timer PNG second record uses typed installed artwork");
        AssertEqual(EscapeTimerTileAtlasFormat.FirstDestinationWord, queued.Entries[0].EncodedVramDestination,
            "timer PNG first record retains native destination");
        AssertEqual(EscapeTimerTileAtlasFormat.SecondDestinationWord, queued.Entries[1].EncodedVramDestination,
            "timer PNG second record retains native destination");
        var timerVram = new SnesVram();
        queued.DrainTo(timerVram, new ForbiddenMapBus(), original);
        AssertTrue(nativeFirstTiles.AsSpan().SequenceEqual(ReadVram(timerVram,
            EscapeTimerTileAtlasFormat.FirstDestinationWord * 2, nativeFirstTiles.Length)),
            "typed first timer record publishes exact stock VRAM");
        AssertTrue(nativeSecondTiles.AsSpan().SequenceEqual(ReadVram(timerVram,
            EscapeTimerTileAtlasFormat.SecondDestinationWord * 2, nativeSecondTiles.Length)),
            "typed second timer record publishes exact stock VRAM");
        var synchronousVram = new SnesVram();
        AssertTrue(tileAtlas.TryLoadNativeTransfer(synchronousVram,
            EscapeTimerTileRomData.FirstSourceAddress,
            EscapeTimerTileAtlasFormat.FirstByteCount,
            EscapeTimerTileAtlasFormat.FirstDestinationWord),
            "Mother Brain synchronous owner recognizes first timer record");
        AssertTrue(tileAtlas.TryLoadNativeTransfer(synchronousVram,
            EscapeTimerTileRomData.SecondSourceAddress,
            EscapeTimerTileAtlasFormat.SecondByteCount,
            EscapeTimerTileAtlasFormat.SecondDestinationWord),
            "Mother Brain synchronous owner recognizes second timer record");
        AssertTrue(nativeFirstTiles.AsSpan().SequenceEqual(ReadVram(synchronousVram,
            EscapeTimerTileAtlasFormat.FirstDestinationWord * 2, nativeFirstTiles.Length)),
            "Mother Brain synchronous first timer transfer uses installed artwork");
        AssertTrue(nativeSecondTiles.AsSpan().SequenceEqual(ReadVram(synchronousVram,
            EscapeTimerTileAtlasFormat.SecondDestinationWord * 2, nativeSecondTiles.Length)),
            "Mother Brain synchronous second timer transfer uses installed artwork");

        var restoredQueueRuntime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus);
        restoredQueueRuntime.VramWrites.Enqueue(EscapeTimerTileAtlasFormat.FirstByteCount,
            EscapeTimerTileRomData.FirstSourceAddress, EscapeTimerTileAtlasFormat.FirstDestinationWord);
        restoredQueueRuntime.VramWrites.Enqueue(EscapeTimerTileAtlasFormat.SecondByteCount,
            EscapeTimerTileRomData.SecondSourceAddress, EscapeTimerTileAtlasFormat.SecondDestinationWord);
        restoredQueueRuntime.MapPresentation = original;
        AssertEqual(VramAssetId.EscapeTimerFirstTiles, restoredQueueRuntime.VramWrites.Entries[0].AssetId,
            "content rebind upgrades pending first timer transfer");
        AssertEqual(VramAssetId.EscapeTimerSecondTiles, restoredQueueRuntime.VramWrites.Entries[1].AssetId,
            "content rebind upgrades pending second timer transfer");

        var presentation = EscapeTimerPresentation.Load(new MemoryStream(extracted));
        var timer = new EscapeTimer();
        timer.Clear();
        timer.SetTime(0x01, 0x23, 0x45);
        var native = new OamBuffer();
        var installed = new OamBuffer();
        native.BeginFrame();
        installed.BeginFrame();
        EscapeTimerRenderer.Draw(timer, native, bus);
        EscapeTimerRenderer.Draw(timer, installed, new ForbiddenMapBus(), presentation);
        AssertEqual(native.NextByteOffset, installed.NextByteOffset,
            "installed timer emits the native number of OAM parts");
        AssertTrue(native.LowTable.SequenceEqual(installed.LowTable),
            "installed timer label/digits exactly match native low OAM");
        AssertTrue(native.HighTable.SequenceEqual(installed.HighTable),
            "installed timer label/digits exactly match native high OAM");

        var document = JsonSerializer.Deserialize<EscapeTimerPresentationDocument>(extracted,
            MapPresentationFormat.JsonOptions)!;
        document.Anchors["Minutes"] = document.Anchors["Minutes"] with
        {
            X = document.Anchors["Minutes"].X + 1,
        };
        Directory.CreateDirectory(overrides);
        string replacement = Path.Combine(overrides, EscapeTimerPresentationDefinitions.FileName);
        using (var output = File.Create(replacement))
            EscapeTimerPresentation.Write(output, document);
        var edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "timer override changes catalog identity");

        var moved = new OamBuffer();
        moved.BeginFrame();
        EscapeTimerRenderer.Draw(timer, moved, new ForbiddenMapBus(), edited.EscapeTimer);
        // Label occupies the first five OAM entries. The next two parts are the minute tens.
        AssertTrue(native.LowTable[..20].SequenceEqual(moved.LowTable[..20]),
            "editing minute anchor leaves TIME label untouched");
        AssertEqual(unchecked((byte)(native.LowTable[20] + 1)), moved.LowTable[20],
            "edited minute anchor reaches the first minute OAM part");
        AssertEqual(unchecked((byte)(native.LowTable[24] + 1)), moved.LowTable[24],
            "edited minute anchor reaches the second minute OAM part");

        File.WriteAllText(replacement, "{ broken timer JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
            "corrupt timer override fails loudly");
        using (var output = File.Create(replacement))
            EscapeTimerPresentation.Write(output, document);

        var missingFrame = document with
        {
            Frames = document.Frames.Where(pair => pair.Key != "Digit.9")
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
        };
        AssertThrows<InvalidDataException>(() => EscapeTimerPresentation.Write(
            Stream.Null, missingFrame), "timer resource rejects missing named digits");
        AssertThrows<InvalidDataException>(() => EscapeTimerPresentation.Write(
            Stream.Null, document with { Palette = 8 }), "timer resource rejects invalid palette");
        AssertThrows<InvalidDataException>(() => EscapeTimerPresentation.Write(
            Stream.Null, document with { DigitSpacing = 256 }), "timer resource rejects invalid spacing");

        IndexedPngImage image;
        using (var input = File.OpenRead(stockTilePath))
            image = IndexedPng.Read(input, EscapeTimerTileAtlasFormat.Width, EscapeTimerTileAtlasFormat.Height);
        image.Pixels[0] ^= 1;
        string tileReplacement = Path.Combine(overrides, EscapeTimerTileAtlasFormat.FileName);
        using (var output = File.Create(tileReplacement))
            IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
        var editedArtwork = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(!original.EscapeTimerTiles.Resolve(VramAssetId.EscapeTimerFirstTiles).Span.SequenceEqual(
            editedArtwork.EscapeTimerTiles.Resolve(VramAssetId.EscapeTimerFirstTiles).Span),
            "timer PNG edit changes the first compiled transfer");
        AssertTrue(original.EscapeTimerTiles.Resolve(VramAssetId.EscapeTimerSecondTiles).Span.SequenceEqual(
            editedArtwork.EscapeTimerTiles.Resolve(VramAssetId.EscapeTimerSecondTiles).Span),
            "timer PNG edit leaves the second compiled transfer untouched");

        using (var output = File.Create(tileReplacement))
            IndexedPng.Write(output, EscapeTimerTileAtlasFormat.Width - 8,
                EscapeTimerTileAtlasFormat.Height, image.Pixels.AsSpan(0, image.Pixels.Length - 64), image.Palette);
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
            "wrong-size timer PNG fails loudly");

        Console.WriteLine("Escape timer presentation: exact stock OAM/PNG transfers, ROM-free drawing/uploads, edits, rebind and strict failures pass.");

        static byte[] ReadVram(SnesVram vram, int byteAddress, int count)
        {
            var result = new byte[count];
            for (int index = 0; index < count; index++)
                result[index] = vram.ReadByte(byteAddress + index);
            return result;
        }
    }
}
