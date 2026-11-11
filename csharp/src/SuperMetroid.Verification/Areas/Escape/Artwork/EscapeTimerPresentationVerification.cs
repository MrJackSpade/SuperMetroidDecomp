using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Areas.Escape.Artwork;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;

/// <summary>Compares the escape timer's extracted artwork, tile transfers, and digit layout with the cartridge.</summary>
internal static class EscapeTimerPresentationVerification
{
    /// <summary>Checks the extracted timer's VRAM transfers and rendered OAM against native timer assets.</summary>
    internal static void VerifyEscapeTimerPresentationAssets(ISnesAddressSpace bus)
    {
        Suite(nameof(VerifyEscapeTimerPointerDefinitions), () => VerifyEscapeTimerPointerDefinitions(bus));
        byte[] extracted = SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerPresentationExtractor.Extract();
        byte[] extractedTiles = SuperMetroid.AssetExtraction.Areas.Escape.EscapeTimerTileAtlasExtractor.Extract(bus);

        var tileAtlas = EscapeTimerCharacters.Read(new MemoryStream(extractedTiles));
        byte[] nativeFirstTiles = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            EscapeTimerTileSourceConstants.FirstSourceAddress, EscapeTimerTileAtlasFormat.FirstByteCount);
        byte[] nativeSecondTiles = SuperMetroid.AssetExtraction.Cartridge.RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            EscapeTimerTileSourceConstants.SecondSourceAddress, EscapeTimerTileAtlasFormat.SecondByteCount);
        AssertTrue(nativeFirstTiles.SequenceEqual(EscapeTimerTilePageTransfers.Resolve(tileAtlas, InstalledArtworkId.EscapeTimerFirstTiles).Span),
            "first timer PNG page compiles to exact native characters");
        AssertTrue(nativeSecondTiles.SequenceEqual(EscapeTimerTilePageTransfers.Resolve(tileAtlas, InstalledArtworkId.EscapeTimerSecondTiles).Span),
            "second timer PNG page compiles to exact native characters");

        var synchronous = new SpriteArtworkPlacements();
        AssertTrue(tileAtlas.TryRecordNativeTransfer(synchronous,
            EscapeTimerTileSourceConstants.FirstSourceAddress,
            EscapeTimerTileAtlasFormat.FirstByteCount,
            EscapeNativePageWords.TimerFirst),
            "Mother Brain synchronous owner recognizes first timer record");
        AssertTrue(tileAtlas.TryRecordNativeTransfer(synchronous,
            EscapeTimerTileSourceConstants.SecondSourceAddress,
            EscapeTimerTileAtlasFormat.SecondByteCount,
            EscapeNativePageWords.TimerSecond),
            "Mother Brain synchronous owner recognizes second timer record");
        AssertEqual(new SpriteArtworkPlacement(new InstalledArtworkUploadSource(InstalledArtworkId.EscapeTimerFirstTiles),
                (EscapeNativePageWords.TimerFirst - GameplayVramLayoutConstants.ObjectCharacters.Offset) * 2 / SpriteArtworkPlacements.CharacterByteCount,
                EscapeTimerTileAtlasFormat.FirstByteCount / SpriteArtworkPlacements.CharacterByteCount),
            synchronous.Uploads[0], "Mother Brain synchronous first timer transfer shows the installed first page");
        AssertEqual(new SpriteArtworkPlacement(new InstalledArtworkUploadSource(InstalledArtworkId.EscapeTimerSecondTiles),
                (EscapeNativePageWords.TimerSecond - GameplayVramLayoutConstants.ObjectCharacters.Offset) * 2 / SpriteArtworkPlacements.CharacterByteCount,
                EscapeTimerTileAtlasFormat.SecondByteCount / SpriteArtworkPlacements.CharacterByteCount),
            synchronous.Uploads[1], "Mother Brain synchronous second timer transfer shows the installed second page");
        AssertTrue(nativeFirstTiles.SequenceEqual(EscapeTimerTilePageTransfers.Resolve(tileAtlas, InstalledArtworkId.EscapeTimerFirstTiles).ToArray()) &&
                nativeSecondTiles.SequenceEqual(EscapeTimerTilePageTransfers.Resolve(tileAtlas, InstalledArtworkId.EscapeTimerSecondTiles).ToArray()),
            "Mother Brain synchronous timer pages hold the installed artwork");

        var presentation = EscapeTimerPresentationFile.Load(new MemoryStream(extracted));
        SpriteSheet sheet = EscapeTimerDrawChecks.Sheet(bus);
        var timer = new EscapeTimer();
        timer.Clear();
        timer.SetRemaining(EscapeTimerClock.Time(1, 23, 45));
        var native = new OamBuffer();
        native.BeginFrame();
        DrawImportedEscapeTimer(bus, timer, native);
        IReadOnlyList<SpriteDraw> installed = EscapeTimerDrawChecks.Draw(timer, presentation, sheet);
        EscapeTimerDrawChecks.AssertSamePixels(native, installed, tileAtlas.Artwork, bus,
            "installed timer label/digits paint the native OAM sprites' pixels");
        AssertEqual(7, installed.Count, "timer publishes the label and six digits as whole-frame draws");
        string[] frames = ["Label", "Digit.0", "Digit.1", "Digit.2", "Digit.3", "Digit.4", "Digit.5"];
        for (int index = 0; index < frames.Length; index++)
            AssertEqual(sheet.Frame(frames[index]), ((ImageSpriteDraw)installed[index]).Image,
                $"01:23:45 draw {index} shows sheet frame {frames[index]}");

        Console.WriteLine("Escape timer presentation: native-equivalent draws, exact PNG transfers and ROM-free drawing/uploads pass.");
    }

    /// <summary>Checks the compiled digit pointer table and confirms the timer sheet is composed without rereading it.</summary>
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
                System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(nativeSheet.Manifest, SpriteSheetFile.Options).AsSpan()
                    .SequenceEqual(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(guardedSheet.Manifest, SpriteSheetFile.Options)),
            "timer sheet composition selects the same spritemaps without rereading the pointer table");
        Console.WriteLine("Escape timer digit pointers: ten cartridge words and a sheet composed without rereading the pointer table.");
    }

    /// <summary>Verifies the installed timer against import-time cartridge parts.</summary>
    internal static void DrawImportedEscapeTimer(ISnesAddressSpace bus, EscapeTimer timer,
        OamBuffer oam)
    {
        ushort palette = NativeSpriteGraphicsWords.PaletteBits(5);
        Draw(0, EscapeTimerPresentationDefinitions.LabelSpritemap);
        // The cartridge draws each field from its packed-decimal byte.
        Digits(EscapeTimerClock.PackedDecimal(timer.Readout.Minutes), unchecked((short)0xffe4));
        Digits(EscapeTimerClock.PackedDecimal(timer.Readout.Seconds), unchecked((short)0xfffc));
        Digits(EscapeTimerClock.PackedDecimal(timer.Readout.Centiseconds), 0x0014);

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
