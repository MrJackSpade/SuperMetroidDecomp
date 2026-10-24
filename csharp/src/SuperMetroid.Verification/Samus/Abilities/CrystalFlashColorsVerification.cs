using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Frontend.Map;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks editable Crystal Flash body and bubble colors against native palette updates.</summary>
internal static class CrystalFlashColorsVerification
{
    /// <summary>Compares cartridge source words, then checks edited CGRAM, unchanged timers, and stock repair.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Crystal Flash body and bubble colors")]
    internal static void VerifyCrystalFlashColorOverride(string stockDirectory,
        string overrideDirectory, AreaMapPresentationCatalog original, ISnesAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Samus.CrystalFlashColorExtractor.Extract(rom);
        CrystalFlashColorCatalog native = CrystalFlashColorCatalog.Load(
            new MemoryStream(extracted, writable: false));
        for (int frame = 0; frame < CrystalFlashColorFormat.BodyFrameCount; frame++)
        {
            ushort pointer = ReadWord(rom, SamusSourceDefinitions.CrystalFlashBodyRecords.Value +
                frame * SamusPaletteDefinitions.CrystalFlash.BodyRecordByteCount);
            for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                        SamusSourceDefinitions.PaletteBank.Value | (pointer + color * 2)),
                    native.ResolveBody(frame, color),
                    $"Crystal Flash body frame {frame}, color {color} matches cartridge");
        }
        for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
        {
            ushort pointer = ReadWord(rom, SamusSourceDefinitions.BubblePointers.Value +
                frame * sizeof(ushort));
            for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                        SamusSourceDefinitions.PaletteBank.Value | (pointer + color * 2)),
                    native.ResolveBubble(frame, color),
                    $"Crystal Flash bubble frame {frame}, color {color} matches cartridge");
        }

        CrystalFlashColorDocument document = JsonSerializer.Deserialize<CrystalFlashColorDocument>(
            File.ReadAllBytes(Path.Combine(stockDirectory, CrystalFlashColorFormat.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Crystal Flash color JSON is null.");
        Paint(document.Body[0]);
        Paint(document.Bubble[0]);
        string replacement = Path.Combine(overrideDirectory, CrystalFlashColorFormat.FileName);
        File.WriteAllBytes(replacement, CrystalFlashColorCatalog.Write(document));
        AreaMapPresentationCatalog edited = MapPresentationFiles.Load(stockDirectory, overrideDirectory);
        AssertTrue(edited.ContentIdentity != original.ContentIdentity,
            "Crystal Flash color edit changes installed-content identity");
        AssertTrue(edited.CrystalFlashColors.ResolveBody(0, 1) !=
            original.CrystalFlashColors.ResolveBody(0, 1),
            "body color edit differs from stock");
        AssertTrue(edited.CrystalFlashColors.ResolveBubble(0, 1) !=
            original.CrystalFlashColors.ResolveBubble(0, 1),
            "bubble color edit differs from stock");

        SamusState samus = StartCrystalFlash();
        SamusState nativeSamus = StartCrystalFlash();

        var guard = NewCrystalFlashColorReadGuard(rom);
        var cgram = new SamusColourRig();
        var nativeCgram = new SamusColourRig();
        int duration = CrystalFlashPaletteTimingDefinitions.AuthoredDuration;
        int calls = CrystalFlashColorFormat.BodyFrameCount * duration;
        for (int call = 0; call < calls; call++)
        {
            AssertTrue(samus.CrystalFlash.UpdatePalette(cgram.Samus, samus,
                    palettes: null, colors: edited.CrystalFlashColors),
                $"Crystal Flash call {call} retains palette ownership");
            AssertTrue(nativeSamus.CrystalFlash.UpdatePalette(nativeCgram.Samus, nativeSamus,
                    colors: native),
                $"native Crystal Flash call {call} retains palette ownership");
            AssertEqual(nativeSamus.CrystalFlash.CrystalPaletteTimer.RawValue,
                samus.CrystalFlash.CrystalPaletteTimer.RawValue,
                $"color edit preserves body timer on call {call}");
            AssertEqual(nativeSamus.CrystalFlash.SpecialPaletteTimer.RawValue,
                samus.CrystalFlash.SpecialPaletteTimer.RawValue,
                $"color edit preserves bubble timer on call {call}");
            AssertEqual(nativeSamus.CrystalFlash.BodyPaletteRecordOffset,
                samus.CrystalFlash.BodyPaletteRecordOffset,
                $"color edit preserves body cursor on call {call}");
            AssertEqual(nativeSamus.CrystalFlash.SpecialPaletteFrame,
                samus.CrystalFlash.SpecialPaletteFrame,
                $"color edit preserves bubble cursor on call {call}");
            int bodyFrame = call / duration;
            int bubbleFrame = call / 5 % CrystalFlashColorFormat.BubbleFrameCount;
            for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
                AssertEqual(edited.CrystalFlashColors.ResolveBody(bodyFrame, color),
                    cgram.Colors[SamusPaletteDefinitionsCrystalFlashConstants.BodyCgramStart + color],
                    $"Crystal Flash body call {call}, color {color} reaches CGRAM");
            for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
                AssertEqual(edited.CrystalFlashColors.ResolveBubble(bubbleFrame, color),
                    cgram.Colors[SamusPaletteDefinitionsCrystalFlashConstants.BubbleCgramStart + color],
                    $"Crystal Flash bubble call {call}, color {color} reaches CGRAM");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Crystal Flash cycle avoids pointer and source-color ROM reads");

        PaletteRgb5 previous = document.Body[0][1];
        document.Body[0][1] = previous with { Red = 32 };
        AssertThrows<InvalidDataException>(() => CrystalFlashColorCatalog.Load(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
                document, ArtworkJson.Options), writable: false)),
            "Crystal Flash colors reject out-of-range RGB5 channels");

        string replacementStock = Path.Combine(
            Path.GetDirectoryName(stockDirectory) ?? throw new InvalidOperationException("Stock maps have no parent."),
            "crystal-color-reextract");
        byte[] overrideBeforeRepair = File.ReadAllBytes(replacement);
        SuperMetroid.AssetExtraction.Frontend.Map.MapPresentationExtractor.Extract(rom, replacementStock, "test-provenance");
        AreaMapPresentationCatalog afterRepair = MapPresentationFiles.Load(
            replacementStock, overrideDirectory);
        AssertEqual(edited.ContentIdentity, afterRepair.ContentIdentity,
            "stock re-extraction preserves Crystal Flash override");
        AssertTrue(overrideBeforeRepair.SequenceEqual(File.ReadAllBytes(replacement)),
            "stock re-extraction never rewrites user Crystal Flash colors");

        File.Delete(replacement);
        AreaMapPresentationCatalog restored = MapPresentationFiles.Load(stockDirectory, overrideDirectory);
        AssertEqual(original.ContentIdentity, restored.ContentIdentity,
            "removing Crystal Flash override restores stock identity");
        Console.WriteLine("Crystal Flash colors: 136 stock words, 100 independent body/bubble calls, edited CGRAM, ROM guard, strict values and stock repair pass.");

        static void Paint(PaletteRgb5[] colors)
        {
            PaletteRgb5 originalColor = colors[1];
            colors[1] = originalColor with
            {
                Blue = originalColor.Blue == 31 ? 30 : originalColor.Blue + 1,
            };
        }

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);

        SamusState StartCrystalFlash()
        {
            var started = new SamusState
            {
                Pose = SamusPoseId.FacingRightNormalPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
                Health = 1,
                MaxHealth = 99,
                Missiles = 10,
                SuperMissiles = 10,
                PowerBombs = 10,
            };
            SamusPoseMetadata.RefreshCollisionRadii(started);
            started.HorizontalSpeed.SpecialPaletteTimer = WrappingCountdown.Start(0);
            AssertTrue(started.CrystalFlash.TryBegin(started, controllerInput: 0,
                inputCheck: CrystalFlashInputCheck.Skipped), "start real Crystal Flash palette owner");
            return started;
        }
    }

    /// <summary>Blocks all native body and bubble pointer and color bytes during installed palette updates.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Crystal Flash palette pointers and colors the installed cycle must not reread")]
    private static CartridgeReadGuard NewCrystalFlashColorReadGuard(ISnesAddressSpace source)
    {
        var forbidden = new HashSet<int>();
        for (int frame = 0; frame < CrystalFlashColorFormat.BodyFrameCount; frame++)
        {
            int address = SamusSourceDefinitions.CrystalFlashBodyRecords.Value +
                frame * SamusPaletteDefinitions.CrystalFlash.BodyRecordByteCount;
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), address);
            forbidden.Add(address);
            forbidden.Add(address + 1);
            BlockColors(pointer, CrystalFlashColorFormat.BodyColorCount);
        }
        for (int frame = 0; frame < CrystalFlashColorFormat.BubbleFrameCount; frame++)
        {
            int address = SamusSourceDefinitions.BubblePointers.Value +
                frame * sizeof(ushort);
            ushort pointer = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(source), address);
            forbidden.Add(address);
            forbidden.Add(address + 1);
            BlockColors(pointer, CrystalFlashColorFormat.BubbleColorCount);
        }
        return new CartridgeReadGuard(source).Forbid(forbidden.Contains,
            address => $"Crystal Flash reread palette data ${address:X6}.");

        void BlockColors(ushort pointer, int count)
        {
            int address = SamusSourceDefinitions.PaletteBank.Value | pointer;
            for (int offset = 0; offset < count * sizeof(ushort); offset++)
                forbidden.Add(address + offset);
        }
    }
}
