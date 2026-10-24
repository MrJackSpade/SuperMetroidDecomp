using SuperMetroid.Tooling;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Escape.Artwork;
namespace SuperMetroid.Verification.Areas.Escape.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;

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
        byte[] extracted = SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerPresentationExtractor.Extract();
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
        AssertEqual(EscapeNativePageWords.TimerFirst, queued.Entries[0].Destination.Encoded,
            "timer PNG first record retains native destination");
        AssertEqual(EscapeNativePageWords.TimerSecond, queued.Entries[1].Destination.Encoded,
            "timer PNG second record retains native destination");
        var timerVram = new SnesVram();
        queued.DrainTo(timerVram, original, null, BgArtworkFixtures.For(timerVram));
        AssertTrue(nativeFirstTiles.SequenceEqual(ReadVram(timerVram,
            EscapeNativePageWords.TimerFirst * 2, nativeFirstTiles.Length)),
            "typed first timer record publishes exact stock VRAM");
        AssertTrue(nativeSecondTiles.SequenceEqual(ReadVram(timerVram,
            EscapeNativePageWords.TimerSecond * 2, nativeSecondTiles.Length)),
            "typed second timer record publishes exact stock VRAM");
        var synchronous = new ObjCharacterUploads();
        AssertTrue(tileAtlas.TryRecordNativeTransfer(synchronous,
            EscapeTimerTileRomData.FirstSourceAddress,
            EscapeTimerTileAtlasFormat.FirstByteCount,
            EscapeNativePageWords.TimerFirst),
            "Mother Brain synchronous owner recognizes first timer record");
        AssertTrue(tileAtlas.TryRecordNativeTransfer(synchronous,
            EscapeTimerTileRomData.SecondSourceAddress,
            EscapeTimerTileAtlasFormat.SecondByteCount,
            EscapeNativePageWords.TimerSecond),
            "Mother Brain synchronous owner recognizes second timer record");
        AssertEqual(new ObjCharacterUpload((EscapeNativePageWords.TimerFirst - GameplayVramLayout.ObjectCharacters.Offset) * 2,
                EscapeTimerTileAtlasFormat.FirstByteCount, new AssetObjCharacters(VramAssetId.EscapeTimerFirstTiles), 0),
            synchronous.Uploads[0], "Mother Brain synchronous first timer transfer shows the installed first page");
        AssertEqual(new ObjCharacterUpload((EscapeNativePageWords.TimerSecond - GameplayVramLayout.ObjectCharacters.Offset) * 2,
                EscapeTimerTileAtlasFormat.SecondByteCount, new AssetObjCharacters(VramAssetId.EscapeTimerSecondTiles), 0),
            synchronous.Uploads[1], "Mother Brain synchronous second timer transfer shows the installed second page");
        AssertTrue(nativeFirstTiles.SequenceEqual(tileAtlas.Resolve(VramAssetId.EscapeTimerFirstTiles).ToArray()) &&
                nativeSecondTiles.SequenceEqual(tileAtlas.Resolve(VramAssetId.EscapeTimerSecondTiles).ToArray()),
            "Mother Brain synchronous timer pages hold the installed artwork");

        var restoredQueueRuntime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(new SuperMetroid.Core.Gameplay.HostGameplayOverrides(), initialPaletteArt: initialPalettes);
        restoredQueueRuntime.VramWrites.Enqueue(EscapeTimerTileAtlasFormat.FirstByteCount,
            EscapeTimerTileRomData.FirstSourceAddress, VramDmaDestination.FromNative(EscapeNativePageWords.TimerFirst));
        restoredQueueRuntime.VramWrites.Enqueue(EscapeTimerTileAtlasFormat.SecondByteCount,
            EscapeTimerTileRomData.SecondSourceAddress, VramDmaDestination.FromNative(EscapeNativePageWords.TimerSecond));
        restoredQueueRuntime.Artwork.MapPresentation = original;
        AssertEqual(VramAssetId.EscapeTimerFirstTiles, restoredQueueRuntime.VramWrites.Entries[0].AssetId,
            "content rebind upgrades pending first timer transfer");
        AssertEqual(VramAssetId.EscapeTimerSecondTiles, restoredQueueRuntime.VramWrites.Entries[1].AssetId,
            "content rebind upgrades pending second timer transfer");

        var presentation = EscapeTimerPresentation.Load(new MemoryStream(extracted));
        SpriteSheet sheet = EscapeTimerDrawChecks.Sheet(bus);
        var timer = new EscapeTimer();
        timer.Clear();
        timer.SetTime(PackedBcd.FromDecimal(1), PackedBcd.FromDecimal(23), PackedBcd.FromDecimal(45));
        var native = new OamBuffer();
        native.BeginFrame();
        DrawImportedEscapeTimer(bus, timer, native);
        IReadOnlyList<SpriteDraw> installed = EscapeTimerDrawChecks.Draw(timer, presentation, sheet);
        EscapeTimerDrawChecks.AssertSamePixels(native, installed, tileAtlas.Artwork, initialPalettes,
            "installed timer label/digits paint the native OAM sprites' pixels");
        AssertEqual(7, installed.Count, "timer publishes the label and six digits as whole-frame draws");
        string[] frames = ["Label", "Digit.0", "Digit.1", "Digit.2", "Digit.3", "Digit.4", "Digit.5"];
        for (int index = 0; index < frames.Length; index++)
            AssertEqual(sheet.Frame(frames[index]), ((ImageSpriteDraw)installed[index]).Image,
                $"01:23:45 draw {index} shows sheet frame {frames[index]}");

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

        IReadOnlyList<SpriteDraw> moved = EscapeTimerDrawChecks.Draw(timer, edited.EscapeTimer, sheet);
        // Draw 0 is the TIME label; draws 1 and 2 are the minute tens and ones.
        AssertEqual(installed[0].Origin, moved[0].Origin, "editing minute anchor leaves TIME label untouched");
        AssertEqual(installed[1].Origin.X + 1, (int)moved[1].Origin.X,
            "edited minute anchor reaches the minute tens draw");
        AssertEqual(installed[2].Origin.X + 1, (int)moved[2].Origin.X,
            "edited minute anchor reaches the minute ones draw");

        File.WriteAllText(replacement, "{ broken timer JSON");
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
            "corrupt timer override fails loudly");
        using (var output = File.Create(replacement))
            EscapeTimerPresentation.Write(output, document);

        AssertThrows<InvalidDataException>(() => EscapeTimerPresentation.Write(
            Stream.Null, document with { DigitSpacing = 256 }), "timer resource rejects invalid spacing");

        IndexedPngImage image;
        using (var input = File.OpenRead(stockTilePath))
            image = IndexedPng.Read(input, new(EscapeTimerTileAtlasFormat.Width, EscapeTimerTileAtlasFormat.Height));
        image.Pixels[0] ^= 1;
        string tileReplacement = Path.Combine(overrides, EscapeTimerTileAtlasFormat.FileName);
        using (var output = File.Create(tileReplacement))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
        var editedArtwork = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(!original.EscapeTimerTiles.Resolve(VramAssetId.EscapeTimerFirstTiles).Span.SequenceEqual(
            editedArtwork.EscapeTimerTiles.Resolve(VramAssetId.EscapeTimerFirstTiles).Span),
            "timer PNG edit changes the first compiled transfer");
        AssertTrue(original.EscapeTimerTiles.Resolve(VramAssetId.EscapeTimerSecondTiles).Span.SequenceEqual(
            editedArtwork.EscapeTimerTiles.Resolve(VramAssetId.EscapeTimerSecondTiles).Span),
            "timer PNG edit leaves the second compiled transfer untouched");

        using (var output = File.Create(tileReplacement))
            IndexedPng.Write(output, new(EscapeTimerTileAtlasFormat.Width - 8, EscapeTimerTileAtlasFormat.Height), image.Pixels.AsSpan(0, image.Pixels.Length - 64), image.Colours);
        AssertThrows<InvalidDataException>(() => AreaMapPresentationCatalog.Load(stock, overrides),
            "wrong-size timer PNG fails loudly");

        Console.WriteLine("Escape timer presentation: native-equivalent draws, exact PNG transfers, ROM-free drawing/uploads, edits, rebind and strict failures pass.");

        static byte[] ReadVram(SnesVram vram, int byteAddress, int count)
        {
            var result = new byte[count];
            for (int index = 0; index < count; index++)
                result[index] = vram.ReadByte(byteAddress + index);
            return result;
        }
    }

    /// <summary>Checks the compiled digit pointer table and confirms the timer sheet is composed without rereading it.</summary>
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

        ComposedSpriteSheet nativeSheet = EscapeTimerSheetComposer.Compose(bus);
        ComposedSpriteSheet guardedSheet = EscapeTimerSheetComposer.Compose(
            new CartridgeReadGuard(bus).Forbid(
                address => address >= EscapeTimerPresentationDefinitions.DigitPointerTable &&
                    address < EscapeTimerPresentationDefinitions.DigitPointerTable + 10 * sizeof(ushort),
                address => $"Escape timer reread compiled digit pointer at ${address:X6}."));
        AssertTrue(nativeSheet.Pixels.AsSpan().SequenceEqual(guardedSheet.Pixels) &&
                SpriteSheet.Write(nativeSheet.Manifest).AsSpan().SequenceEqual(SpriteSheet.Write(guardedSheet.Manifest)),
            "timer sheet composition selects the same spritemaps without rereading the pointer table");
        Console.WriteLine("Escape timer digit pointers: ten cartridge words and a sheet composed without rereading the pointer table.");
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
