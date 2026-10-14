using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapPresentationVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationOracles;
namespace SuperMetroid.Verification.Areas.Escape.Artwork;
using SuperMetroid.Verification.Harness;

/// <summary>Compares installed escape-timer artwork, tile transfers, and digit layout with the cartridge.</summary>
internal static class EscapeTimerPresentationVerification
{
    /// <summary>Checks extraction, installed overrides, VRAM transfers, and rendered OAM against native timer assets.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables, native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the installed escape-timer characters")]
    internal static void VerifyEscapeTimerPresentationAssets(ISnesAddressSpace bus, string stock,
        string overrides, AreaMapPresentationCatalog original,
        GameplayBasePaletteCatalog initialPalettes)
    {
        Suite(nameof(VerifyEscapeTimerPointerDefinitions), () => VerifyEscapeTimerPointerDefinitions(bus));

        string stockPath = Path.Combine(stock, EscapeTimerPresentationDefinitions.FileName);
        string stockTilePath = Path.Combine(stock, EscapeTimerTileAtlasFormat.FileName);
        byte[] extracted = SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerPresentationExtractor.Extract(bus);
        AssertTrue(extracted.SequenceEqual(File.ReadAllBytes(stockPath)),
            "installed timer JSON is the deterministic cartridge extraction");
        byte[] extractedTiles = SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerTileAtlasExtractor.Extract(bus);
        AssertTrue(extractedTiles.SequenceEqual(File.ReadAllBytes(stockTilePath)),
            "installed timer PNG is the deterministic cartridge extraction");

        var tileAtlas = EscapeTimerTileAtlas.Load(new MemoryStream(extractedTiles));
        byte[] nativeFirstTiles = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            EscapeTimerTileRomData.FirstSourceAddress, EscapeTimerTileAtlasFormat.FirstByteCount);
        byte[] nativeSecondTiles = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            EscapeTimerTileRomData.SecondSourceAddress, EscapeTimerTileAtlasFormat.SecondByteCount);
        AssertTrue(nativeFirstTiles.SequenceEqual(tileAtlas.Resolve(VramAssetId.EscapeTimerFirstTiles).Span),
            "first timer PNG page compiles to exact native characters");
        AssertTrue(nativeSecondTiles.SequenceEqual(tileAtlas.Resolve(VramAssetId.EscapeTimerSecondTiles).Span),
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
        queued.DrainTo(timerVram, ReferenceMutableMemory.From(NewForbiddenMapBus()), original);
        AssertTrue(nativeFirstTiles.SequenceEqual(ReadVram(timerVram,
            EscapeTimerTileAtlasFormat.FirstDestinationWord * 2, nativeFirstTiles.Length)),
            "typed first timer record publishes exact stock VRAM");
        AssertTrue(nativeSecondTiles.SequenceEqual(ReadVram(timerVram,
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
        AssertTrue(nativeFirstTiles.SequenceEqual(ReadVram(synchronousVram,
            EscapeTimerTileAtlasFormat.FirstDestinationWord * 2, nativeFirstTiles.Length)),
            "Mother Brain synchronous first timer transfer uses installed artwork");
        AssertTrue(nativeSecondTiles.SequenceEqual(ReadVram(synchronousVram,
            EscapeTimerTileAtlasFormat.SecondDestinationWord * 2, nativeSecondTiles.Length)),
            "Mother Brain synchronous second timer transfer uses installed artwork");

        var restoredQueueRuntime = new SuperMetroid.Core.Runtime.SuperMetroidRuntime(bus, new SuperMetroid.Core.Gameplay.HostGameplayOverrides(), initialPaletteArt: initialPalettes);
        restoredQueueRuntime.VramWrites.Enqueue(EscapeTimerTileAtlasFormat.FirstByteCount,
            EscapeTimerTileRomData.FirstSourceAddress, VramDmaDestination.FromNative(EscapeTimerTileAtlasFormat.FirstDestinationWord));
        restoredQueueRuntime.VramWrites.Enqueue(EscapeTimerTileAtlasFormat.SecondByteCount,
            EscapeTimerTileRomData.SecondSourceAddress, VramDmaDestination.FromNative(EscapeTimerTileAtlasFormat.SecondDestinationWord));
        restoredQueueRuntime.Artwork.MapPresentation = original;
        AssertEqual(VramAssetId.EscapeTimerFirstTiles, restoredQueueRuntime.VramWrites.Entries[0].AssetId,
            "content rebind upgrades pending first timer transfer");
        AssertEqual(VramAssetId.EscapeTimerSecondTiles, restoredQueueRuntime.VramWrites.Entries[1].AssetId,
            "content rebind upgrades pending second timer transfer");

        var presentation = EscapeTimerPresentation.Load(new MemoryStream(extracted));
        var timer = new EscapeTimer();
        timer.Clear();
        timer.SetTime(PackedBcd.FromDecimal(1), PackedBcd.FromDecimal(23), PackedBcd.FromDecimal(45));
        var native = new OamBuffer();
        var installed = new OamBuffer();
        native.BeginFrame();
        installed.BeginFrame();
        DrawImportedEscapeTimer(bus, timer, native);
        EscapeTimerRenderer.Draw(timer, installed, presentation);
        AssertEqual(native.NextByteOffset, installed.NextByteOffset,
            "installed timer emits the native number of OAM parts");
        AssertTrue(native.LowTable.SequenceEqual(installed.LowTable),
            "installed timer label/digits exactly match native low OAM");
        AssertTrue(native.HighTable.SequenceEqual(installed.HighTable),
            "installed timer label/digits exactly match native high OAM");

        var document = RequiredJson.Deserialize<EscapeTimerPresentationDocument>(extracted,
            ArtworkJson.Options);
        document.Anchors["Minutes"] = document.Anchors["Minutes"] with
        {
            X = (short)(document.Anchors["Minutes"].X + 1),
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
        EscapeTimerRenderer.Draw(timer, moved, edited.EscapeTimer);
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
            image = IndexedPng.Read(input, new(EscapeTimerTileAtlasFormat.Width, EscapeTimerTileAtlasFormat.Height));
        image.Pixels[0] ^= 1;
        string tileReplacement = Path.Combine(overrides, EscapeTimerTileAtlasFormat.FileName);
        using (var output = File.Create(tileReplacement))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        var editedArtwork = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(!original.EscapeTimerTiles.Resolve(VramAssetId.EscapeTimerFirstTiles).Span.SequenceEqual(
            editedArtwork.EscapeTimerTiles.Resolve(VramAssetId.EscapeTimerFirstTiles).Span),
            "timer PNG edit changes the first compiled transfer");
        AssertTrue(original.EscapeTimerTiles.Resolve(VramAssetId.EscapeTimerSecondTiles).Span.SequenceEqual(
            editedArtwork.EscapeTimerTiles.Resolve(VramAssetId.EscapeTimerSecondTiles).Span),
            "timer PNG edit leaves the second compiled transfer untouched");

        using (var output = File.Create(tileReplacement))
            IndexedPng.Write(output, new(EscapeTimerTileAtlasFormat.Width - 8, EscapeTimerTileAtlasFormat.Height), image.Pixels.AsSpan(0, image.Pixels.Length - 64), image.Palette);
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

    /// <summary>Checks the compiled digit pointer table and confirms timer extraction and drawing no longer read it at runtime.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled escape-timer digit pointers")]
    internal static void VerifyEscapeTimerPointerDefinitions(ISnesAddressSpace bus)
    {
        for (int digit = 0; digit < 10; digit++)
        {
            ushort nativePointer = RomDataReader.ReadWordFixedBank(
                CartridgeImportSource.Require(bus),
                EscapeTimerPresentationDefinitions.DigitPointerTable + digit * sizeof(ushort));
            AssertEqual(nativePointer,
                EscapeTimerPresentationDefinitions.DigitSpritemapPointer(digit),
                $"compiled escape-timer digit pointer {digit}");
        }
        AssertThrows<ArgumentOutOfRangeException>(
            () => EscapeTimerPresentationDefinitions.DigitSpritemapPointer(10),
            "escape-timer digit pointer rejects non-decimal values");

        byte[] nativeExtraction = SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerPresentationExtractor.Extract(bus);
        byte[] guardedExtraction = SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerPresentationExtractor.Extract(
            new CartridgeReadGuard(bus).Forbid(
                address => address >= EscapeTimerPresentationDefinitions.DigitPointerTable &&
                    address < EscapeTimerPresentationDefinitions.DigitPointerTable + 10 * sizeof(ushort),
                address => $"Escape timer reread compiled digit pointer at ${address:X6}."));
        AssertTrue(nativeExtraction.SequenceEqual(guardedExtraction),
            "timer extraction selects the same visual records without rereading the pointer table");

        var timer = new EscapeTimer();
        timer.Clear();
        timer.SetTime(PackedBcd.FromDecimal(1), PackedBcd.FromDecimal(23), PackedBcd.FromDecimal(45));
        var native = new OamBuffer();
        var guarded = new OamBuffer();
        native.BeginFrame();
        guarded.BeginFrame();
        DrawImportedEscapeTimer(bus, timer, native);
        EscapeTimerRenderer.Draw(timer, guarded,
            EscapeTimerPresentation.Load(new MemoryStream(guardedExtraction)));
        AssertEqual(native.NextByteOffset, guarded.NextByteOffset,
            "compiled timer pointers preserve native OAM part count");
        AssertTrue(native.ImageEquals(guarded),
            "native timer draw no longer rereads the immutable digit pointer table");
        Console.WriteLine("Escape timer digit pointers: ten cartridge words, exact extraction and OAM, no runtime pointer-table reads.");
    }

    /// <summary>Verifies the installed timer against import-time cartridge parts.</summary>
    internal static void DrawImportedEscapeTimer(ISnesAddressSpace bus, EscapeTimer timer,
        OamBuffer oam)
    {
        ushort palette = EnemyPaletteBits.Palette5;
        Draw(0, EscapeTimerPresentationDefinitions.LabelSpritemap);
        Digits(timer.Minutes.Raw, unchecked((short)0xffe4));
        Digits(timer.Seconds.Raw, unchecked((short)0xfffc));
        Digits(timer.Centiseconds.Raw, 0x0014);

        void Digits(byte bcd, short offset)
        {
            int tens = bcd >> 4;
            int ones = bcd & 15;
            if (tens > 9 || ones > 9)
                throw new InvalidDataException($"Timer oracle received invalid BCD ${bcd:X2}.");
            Draw(offset, EscapeTimerPresentationDefinitions.DigitSpritemapPointer(tens));
            Draw(unchecked((short)(offset + 8)),
                EscapeTimerPresentationDefinitions.DigitSpritemapPointer(ones));
        }

        void Draw(short offset, int address)
        {
            int fullAddress = address <= ushort.MaxValue ? 0x800000 | address : address;
            DrawImportedSpritemap(bus, oam, fullAddress,
                new(unchecked((ushort)(timer.PixelPosition.X + offset)), timer.PixelPosition.Y), palette);
        }
    }

    /// <summary>Runs the retail cartridge comparison for compiled escape-timer digit pointers.</summary>
    [VerificationCommand("--escape-timer-pointer-definitions")]
    internal static int EscapeTimerPointerDefinitionsCommand()
    {
        string timerRom = RepositoryRomPath;
        Suite(nameof(VerifyEscapeTimerPointerDefinitions), () => VerifyEscapeTimerPointerDefinitions(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath(timerRom))));
        return 0;
    }
}
