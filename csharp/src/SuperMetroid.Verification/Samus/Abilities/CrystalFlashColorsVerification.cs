using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Text.Json;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks installed Crystal Flash body and bubble colors against native palette updates.</summary>
internal static class CrystalFlashColorsVerification
{
    /// <summary>Compares cartridge source words, then checks the installed colors reach CGRAM with unchanged timers.</summary>
    internal static void VerifyCrystalFlashColors(string stockDirectory,
        AreaMapPresentation original, ISnesAddressSpace rom)
    {
        byte[] extracted = SuperMetroid.AssetExtraction.Samus.CrystalFlashColorExtractor.Extract(rom);
        InstalledCrystalFlashColors native = CrystalFlashColorFile.Read(
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
            File.ReadAllBytes(Path.Combine(stockDirectory, CrystalFlashColorFile.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Crystal Flash color JSON is null.");
        SamusState samus = StartCrystalFlash();

        var guard = NewCrystalFlashColorReadGuard(rom);
        var samusColours = new SamusColours();
        // Crystal Flash names body and bubble shades (#1512); the import resolves them from the installed colours.
        var installedSources = InstalledFixtures.SamusShadeSources with { CrystalFlash = InstalledFixtures.ImportOf(original).SamusColours.CrystalFlash };
        int duration = CrystalFlashPaletteTimingDefinitions.AuthoredDuration;
        int calls = CrystalFlashColorFormat.BodyFrameCount * duration;
        for (int call = 0; call < calls; call++)
        {
            AssertTrue(samus.CrystalFlash.UpdatePalette(samusColours, samus),
                $"Crystal Flash call {call} retains palette ownership");
            int bodyFrame = call / duration;
            int bubbleFrame = call / 5 % CrystalFlashColorFormat.BubbleFrameCount;
            for (int color = 0; color < CrystalFlashColorFormat.BodyColorCount; color++)
                AssertEqual(InstalledFixtures.ImportOf(original).SamusColours.CrystalFlash.ResolveBody(bodyFrame, color),
                    samusColours.ShownBeams(installedSources)[color],
                    $"Crystal Flash body call {call}, color {color} reaches CGRAM");
            for (int color = 0; color < CrystalFlashColorFormat.BubbleColorCount; color++)
                AssertEqual(InstalledFixtures.ImportOf(original).SamusColours.CrystalFlash.ResolveBubble(bubbleFrame, color),
                    samusColours.ShownBeams(installedSources)[SamusPaletteDefinitionsCrystalFlashConstants.BubbleCgramStart - SamusPaletteDefinitionsCrystalFlashConstants.BodyCgramStart + color],
                    $"Crystal Flash bubble call {call}, color {color} reaches CGRAM");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed Crystal Flash cycle avoids pointer and source-color ROM reads");

        PaletteRgb5 previous = document.Body[0][1];
        document.Body[0][1] = previous with { Red = 32 };
        AssertThrows<InvalidDataException>(() => CrystalFlashColorFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
                document, ArtworkJson.Options), writable: false)),
            "Crystal Flash colors reject out-of-range RGB5 channels");

        Console.WriteLine("Crystal Flash colors: 136 stock words, 100 independent body/bubble calls reach CGRAM, ROM guard and strict values pass.");

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);

        SamusState StartCrystalFlash()
        {
            var started = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
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
            started.HorizontalSpeed.SpecialPaletteTimer = 0;
            AssertTrue(started.CrystalFlash.TryBegin(started, controllerInput: 0,
                inputCheck: CrystalFlashInputCheck.Skipped), "start real Crystal Flash palette owner");
            return started;
        }
    }

    /// <summary>Blocks all native body and bubble pointer and color bytes during installed palette updates.</summary>
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
