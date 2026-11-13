using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Text.Json;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresFlightSpriteArtworkVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks installed Ceres flight Mode 7, sprite, actor, and palette art against retail sources.</summary>
internal static class CeresFlightArtworkVerification
{
    /// <summary>Checks native front and rear transfers and frames, actor placements and sprites against the cartridge.</summary>
    internal static void VerifyCeresFlightArtwork(GameInstallation installation,
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        ImportIntroCinematicArtwork intro = IntroCinematicArtworkFiles.LoadForImport(installation.IntroCinematicDirectory, SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.IntroCinematicDirectoryName));
        CeresFlightCharacters stock = intro.FlightCharacters;
        AssertTrue(stock.FloorCharacters.Span.SequenceEqual(RomDataReader.Decompress(bus,
                CeresCinematicSourceDefinitions.FlightFloorCharacters,
                maximumOutputBytes: CeresFlightArtworkFileFormat.FloorByteCount)),
            "extracted Ceres Mode-7 page preserves every native character byte");
        AssertTrue(stock.ObjectCharacters.Span.SequenceEqual(RomDataReader.Decompress(bus,
                CeresCinematicSourceDefinitions.FlightObjectCharacters,
                maximumOutputBytes: CeresFlightArtworkFileFormat.ObjectByteCount)),
            "extracted Ceres OBJ page preserves every native character byte");
        byte[] nativeMaps = RomDataReader.Decompress(bus, CeresCinematicSourceDefinitions.CeresTilemaps,
            maximumOutputBytes: 0x1000);
        AssertTrue(stock.FloorMaps.Span.SequenceEqual(nativeMaps.AsSpan(0,
                CeresFlightArtworkFileFormat.MapCellCount)),
            "installed front/rear Ceres maps preserve every consumed native byte");
        byte[] nativePalette = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            CeresCinematicSourceDefinitions.FlightPalette, CgramPalettes.ByteCount);
        // The installed colours in the cartridge's colour order, compared with the native palette colour by colour.
        byte[] installedWords = ScreenColourWords.Words(intro.FlightPalette.BackgroundColours.AsSpan(),
            intro.FlightPalette.SpriteColours.AsSpan());
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            AssertEqual(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(nativePalette.AsSpan(index * sizeof(ushort))),
                System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(installedWords.AsSpan(index * sizeof(ushort))),
                "installed Ceres palette preserves every native color");

        var guard = NewIntroArtworkSourceReadGuard(bus, IntroArtworkSources.CeresFlightSprites);
        var referenceFiles = CeresFlightArtworkExtractor.Extract(bus);
        using var referenceCharacters = new MemoryStream(referenceFiles[CeresFlightArtworkFileFormat.FloorFileName]);
        using var referenceMaps = new MemoryStream(referenceFiles[CeresFlightArtworkFileFormat.MapFileName]);
        using var referenceObjects = new MemoryStream(referenceFiles[CeresFlightArtworkFileFormat.ObjectFileName]);
        using var referencePalette = new MemoryStream(referenceFiles[CeresFlightPaletteFormat.FileName]);
        using var referenceSprites = new MemoryStream(referenceFiles[CeresFlightSpriteFormat.FileName]);
        using var referenceActors = new MemoryStream(referenceFiles[CeresFlightActorLayoutFormat.FileName]);
        CeresFlightCharacters reference = CeresFlightCharacters.Read(referenceCharacters, referenceObjects,
            CeresFlightSourceArtwork.Read(referenceMaps, CeresFlightPaletteFile.Read(referencePalette).Backdrop, referenceSprites, referenceActors));
        var native = new IntroCeresFlightState(AudioQueues.Standalone(), reference.BoundCatalog, sheets: CinematicSheetFixtures.WithFlight(reference));
        var installed = new IntroCeresFlightState(AudioQueues.Standalone(), stock.BoundCatalog, sheets: CinematicSheetFixtures.WithFlight(stock));
        var phases = new HashSet<IntroCeresFlightPhase>();
        for (int tick = 0; tick < 4000 && !native.Finished; tick++)
        {
            AssertEqual(native.Phase, installed.Phase,
                $"installed Ceres flight preserves native phase at tick {tick}");
            bool enteredPhase = phases.Add(native.Phase);
            if (enteredPhase || tick % 97 == 0)
            {
                LayeredScene original = native.CaptureRenderSnapshot();
                byte[] originalVram = native.ShownVram();
                LayeredScene compiled = installed.CaptureRenderSnapshot();
                byte[] compiledVram = installed.ShownVram();
                AssertTrue(compiled.Brightness == original.Brightness &&
                        compiledVram.SequenceEqual(originalVram) &&
                        compiled.Paint.Backdrop == original.Paint.Backdrop &&
                        ScenePixels.Same(compiled, original),
                    $"installed Ceres flight matches native pixels and PPU state at tick {tick}");
            }
            native.StepFrame();
            installed.StepFrame();
        }
        AssertTrue(native.Finished && installed.Finished &&
                phases.Contains(IntroCeresFlightPhase.FlyingTowardCeres) &&
                phases.Contains(IntroCeresFlightPhase.SpaceColonyTitle),
            "stock Ceres PNG/JSON art preserves front, rear and SPACE COLONY phases");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed flight never reads its cartridge art, palette, or spritemap sources");

        Suite(nameof(VerifyCeresFlightSpriteArtwork), () => VerifyCeresFlightSpriteArtwork(installation, bus, stock));
        Suite(nameof(VerifyCeresFlightActorLayout), () => VerifyCeresFlightActorLayout(installation, bus));

        Console.WriteLine("Ceres flight art: native front/rear frames and five ROM-sourced rear actor placements pass.");
    }

    /// <summary>Checks five native rear-view actor positions and the layout codec's identity rejection.</summary>
    private static void VerifyCeresFlightActorLayout(GameInstallation installation,
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        string name = CeresFlightActorLayoutFormat.FileName;
        string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
        CeresFlightActorLayoutDocument document =
            JsonSerializer.Deserialize<CeresFlightActorLayoutDocument>(
                File.ReadAllBytes(stockPath), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock Ceres flight actor layout is empty.");
        for (int index = 0; index < document.Actors.Length; index++)
        {
            var source = CeresCinematicSourceDefinitions.RearViewPlacements[index];
            AssertEqual(source.Id, document.Actors[index].Id,
                $"Ceres flight actor {index} retains its native role");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresCinematicSourceDefinitions.PlacementBank | source.FirstOperand),
                document.Actors[index].X,
                $"Ceres flight actor {index} X matches the cartridge operand");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresCinematicSourceDefinitions.PlacementBank | source.SecondOperand),
                document.Actors[index].Y,
                $"Ceres flight actor {index} Y matches the cartridge operand");
        }

        document.Actors[0] = document.Actors[0] with { Id = "wrong-actor" };
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            CeresFlightActorLayoutFile.Write(invalid, document);
            _ = CeresFlightActorLayoutFile.Read(new MemoryStream(invalid.ToArray(), writable: false));
        }, "Ceres flight layout rejects missing or reordered actor identities on read");
    }
}
