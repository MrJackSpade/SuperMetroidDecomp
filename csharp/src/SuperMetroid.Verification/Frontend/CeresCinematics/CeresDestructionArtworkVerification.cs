using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks installed Ceres destruction and Zebes reveal art against cartridge transfers and live cinematic rendering.</summary>
internal static class CeresDestructionArtworkVerification
{
    /// <summary>Confirms native maps, characters, OAM and actor placements, and the layout and sprite codecs' rejections.</summary>
    internal static void VerifyCeresDestructionArtwork(GameInstallation installation,
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        // The Zebes reveal's characters are import data no installation holds: the cartridge-derived extraction's (#1636).
        ArgumentNullException.ThrowIfNull(installation);
        ImportIntroCinematicArtwork stock = InstalledFixtures.IntroArtwork;
        byte[] nativeMaps = RomDataReader.Decompress(bus,
            CeresCinematicSourceDefinitions.CeresTilemaps,
            maximumOutputBytes: CeresCinematicSourceDefinitions.CompressedTilemapLimit);
        AssertTrue(stock.CeresDestructionMaps.Views.Span.SequenceEqual(nativeMaps.AsSpan(
                2 * CeresFlightArtworkFileFormat.MapCellsPerView,
                CeresDestructionMapFormat.MapByteCount)),
            "installed destruction JSON preserves the three remaining Ceres map slices");
        byte[] nativeZebesMap = RomDataReader.Decompress(bus,
            CeresCinematicSourceDefinitions.ZebesTilemap,
            maximumOutputBytes: CeresCinematicSourceDefinitions.CompressedTilemapLimit);
        AssertTrue(stock.ZebesReveal.Map.Bytes.Span.SequenceEqual(
                nativeZebesMap.AsSpan(0, ZebesRevealArtworkFormat.MapByteCount)),
            "installed Zebes reveal JSON preserves every transferred BG word");
        AssertTrue(stock.ZebesReveal.Characters.Characters.Span.SequenceEqual(
                RomDataReader.Decompress(bus,
                    CeresCinematicSourceDefinitions.ZebesCharacters,
                    maximumOutputBytes: ZebesRevealArtworkFormat.CharacterByteCount)),
            "extracted Zebes reveal characters preserve every transferred character byte");

        foreach (CeresDestructionSpriteFrameDefinition frame in
            CeresDestructionSpriteFrameDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    (short)y >= 0;
                int source = (int)new SnesAddress(
                    IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(frame.Pose));
                var nativeOam = new OamBuffer();
                nativeOam.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800);
                else
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800, originIsOnScreen: false);
                nativeOam.FinalizeFrame();
                var installedOam = ScreenSpritesFixtures.Blank();
                installedOam.BeginFrame();
                CinematicSheetFixtures.Drawer(stock.CeresDestructionSprites).Draw(frame.Pose, installedOam,
                    new(120, y), CeresShades.Drawn(CeresSpriteColouring.SpaceObjects));
                installedOam.FinalizeFrame();
                AssertTrue(installedOam.ImageEquals(nativeOam) &&
                        installedOam.LastFinalizedSpriteCount == nativeOam.LastFinalizedSpriteCount,
                    $"Ceres destruction {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }

        var guard = NewIntroArtworkSourceReadGuard(bus, IntroArtworkSources.CeresFlightSprites | IntroArtworkSources.CeresDestructionSprites);
        var native = CreateRetailDestructionFixture();
        var installed = CreateRetailDestructionFixture(artwork: stock);
        var phases = new HashSet<CeresDestructionPhase>();
        for (int frame = 0; frame < 5000 && !native.Finished; frame++)
        {
            AssertEqual(native.Phase, installed.Phase,
                $"installed destruction artwork preserves phase at frame {frame}");
            bool firstPhaseFrame = phases.Add(native.Phase);
            if (firstPhaseFrame || frame % 211 == 0)
            {
                LayeredScene expected = native.CaptureRenderSnapshot();
                byte[] expectedVram = native.ShownVram();
                LayeredScene actual = installed.CaptureRenderSnapshot();
                byte[] actualVram = installed.ShownVram();
                AssertTrue(actual.Brightness == expected.Brightness &&
                        actualVram.SequenceEqual(expectedVram) &&
                        actual.Paint.Backdrop == expected.Paint.Backdrop &&
                        SceneShownColours.Same(SceneShownColours.Of(actual), SceneShownColours.Of(expected)) &&
                        ScenePixels.Same(actual, expected),
                    $"installed Ceres/Zebes art preserves native visible pixels at frame {frame}");
            }
            native.Step();
            installed.Step();
        }
        AssertTrue(native.Finished && installed.Finished &&
                phases.Contains(CeresDestructionPhase.FlyingAwayFromExplosion) &&
                phases.Contains(CeresDestructionPhase.FadeInZebes),
            "installed art retains destruction, Zebes reveal and state-six handoff");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed destruction/reveal never rereads its cartridge art sources");

        string actorsName = CeresRevealActorLayoutFormat.FileName;
        string actorsPath = Path.Combine(installation.IntroCinematicDirectory, actorsName);
        CeresRevealActorLayoutDocument actorsDocument =
            JsonSerializer.Deserialize<CeresRevealActorLayoutDocument>(
                File.ReadAllBytes(actorsPath), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock Zebes reveal actor layout is empty.");
        for (int index = 0; index < actorsDocument.Actors.Length; index++)
        {
            var source = CeresCinematicSourceDefinitions.ZebesPlacements[index];
            AssertEqual(source.Id, actorsDocument.Actors[index].Id,
                $"Zebes reveal actor {index} retains its native identity");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresCinematicSourceDefinitions.PlacementBank | source.FirstOperand),
                actorsDocument.Actors[index].X,
                $"Zebes reveal actor {index} X matches the cartridge operand");
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresCinematicSourceDefinitions.PlacementBank | source.SecondOperand),
                actorsDocument.Actors[index].Y,
                $"Zebes reveal actor {index} Y matches the cartridge operand");
        }
        actorsDocument.Actors[0] = actorsDocument.Actors[0] with { Id = "wrong-actor" };
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            CeresRevealActorLayoutFile.Write(invalid, actorsDocument);
            _ = CeresRevealActorLayoutFile.Read(new MemoryStream(invalid.ToArray(), writable: false));
        }, "Zebes reveal layout rejects missing or reordered actor identities on read");

        string destructionActorsName = CeresDestructionActorLayoutFormat.FileName;
        string destructionActorsPath = Path.Combine(installation.IntroCinematicDirectory,
            destructionActorsName);
        CeresDestructionActorLayoutDocument destructionActorsDocument =
            JsonSerializer.Deserialize<CeresDestructionActorLayoutDocument>(
                File.ReadAllBytes(destructionActorsPath), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock Ceres destruction actor layout is empty.");
        for (int index = 0; index < destructionActorsDocument.Actors.Length; index++)
        {
            CinematicPlacementImmediates source = CeresCinematicSourceDefinitions.DestructionPlacements[index];
            AssertEqual(CeresDestructionActors.InitialPlacementId(index),
                destructionActorsDocument.Actors[index].Id,
                $"Ceres destruction actor {index} retains its native role");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(CeresCinematicSourceDefinitions.PlacementBank | source.FirstOperand),
                destructionActorsDocument.Actors[index].X, $"Ceres destruction actor {index} X is its initializer's immediate");
            AssertEqual(CartridgeImportSource.Require(bus).ReadCartridgeWord(CeresCinematicSourceDefinitions.PlacementBank | source.SecondOperand),
                destructionActorsDocument.Actors[index].Y, $"Ceres destruction actor {index} Y is its initializer's immediate");
        }
        destructionActorsDocument.Actors[0] = destructionActorsDocument.Actors[0] with
        {
            Id = "wrong-actor",
        };
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            CeresDestructionActorLayoutFile.Write(invalid, destructionActorsDocument);
            _ = CeresDestructionActorLayoutFile.Read(new MemoryStream(invalid.ToArray(), writable: false));
        }, "Ceres destruction layout rejects missing or reordered actor identities on read");

        string spritesName = CeresDestructionSpriteFormat.FileName;
        string spritesPath = Path.Combine(installation.IntroCinematicDirectory, spritesName);
        CeresDestructionSpriteDocument spriteDocument =
            JsonSerializer.Deserialize<CeresDestructionSpriteDocument>(
                File.ReadAllBytes(spritesPath), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock Ceres destruction sprites are empty.");
        string removedFrame = "station-under-attack-large-asteroid";
        spriteDocument.Frames.Remove(removedFrame);
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            CeresDestructionSpritePresentation.Write(invalid, spriteDocument);
            _ = CeresDestructionSpritePresentation.Load(new MemoryStream(invalid.ToArray(), writable: false));
        }, "Ceres destruction sprites reject a missing named frame on read");
        Console.WriteLine("Ceres destruction art: 23 native OAM frames, three destruction and six ROM-sourced reveal actor placements, exact PNG/JSON transfers and codec rejections pass.");
    }
}
