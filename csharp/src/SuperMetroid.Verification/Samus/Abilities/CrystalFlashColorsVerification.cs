using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Samus.Artwork;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>Checks installed Crystal Flash body and bubble colors against the cartridge and the palette owner.</summary>
internal static class CrystalFlashColorsVerification
{
    /// <summary>Compares cartridge source words, checks the palette owner holds the palette for the whole body cycle, and rejects invalid RGB5.</summary>
    internal static void VerifyCrystalFlashColors(ISnesAddressSpace rom)
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

        SamusState samus = StartCrystalFlash();

        var samusColours = new SamusColours();
        int duration = CrystalFlashPaletteTimingDefinitions.AuthoredDuration;
        int calls = CrystalFlashColorFormat.BodyFrameCount * duration;
        for (int call = 0; call < calls; call++)
        {
            AssertTrue(samus.CrystalFlash.UpdatePalette(samusColours, samus),
                $"Crystal Flash call {call} retains palette ownership");
        }

        CrystalFlashColorDocument document = RequiredJson.Deserialize<CrystalFlashColorDocument>(extracted, ArtworkJson.Options);
        PaletteRgb5 previous = document.Body[0][1];
        document.Body[0][1] = previous with { Red = 32 };
        AssertThrows<InvalidDataException>(() => CrystalFlashColorFile.Read(
            new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(
                document, ArtworkJson.Options), writable: false)),
            "Crystal Flash colors reject out-of-range RGB5 channels");

        Console.WriteLine("Crystal Flash colors: 136 stock words, 100 body/bubble palette calls and strict values pass.");

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);

        SamusState StartCrystalFlash()
        {
            var started = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence())
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
}
