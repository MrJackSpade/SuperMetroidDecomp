using SuperMetroid.AssetExtraction.Artwork;
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
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks installed Ceres destruction and Zebes reveal art against cartridge transfers and live cinematic rendering.</summary>
internal static class CeresDestructionArtworkVerification
{
    /// <summary>Confirms native maps, characters, and OAM, then exercises editable actor, sprite, map, and PNG overrides through the running scene.</summary>
    internal static void VerifyCeresDestructionArtwork(GameInstallation installation,
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        IntroCinematicArtwork stock = installation.LoadIntroCinematicArt();
        byte[] nativeMaps = RomDataReader.Decompress(bus,
            CeresCinematicSourceDefinitions.CeresTilemaps,
            maximumOutputBytes: CeresCinematicSourceDefinitions.CompressedTilemapLimit);
        AssertTrue(stock.CeresDestruction.CeresMaps.Span.SequenceEqual(nativeMaps.AsSpan(
                2 * CeresFlightArtworkFormat.MapCellsPerView,
                CeresDestructionArtworkFormat.MapByteCount)),
            "installed destruction JSON preserves the three remaining Ceres map slices");
        byte[] nativeZebesMap = RomDataReader.Decompress(bus,
            CeresCinematicSourceDefinitions.ZebesTilemap,
            maximumOutputBytes: CeresCinematicSourceDefinitions.CompressedTilemapLimit);
        AssertTrue(IntroCinematicSourceArtwork.Of(stock).ZebesReveal.Map.Bytes.Span.SequenceEqual(
                nativeZebesMap.AsSpan(0, ZebesRevealArtworkFormat.MapByteCount)),
            "installed Zebes reveal JSON preserves every transferred BG word");
        AssertTrue(IntroCinematicSourceArtwork.Of(stock).ZebesReveal.Characters.Characters.Span.SequenceEqual(
                RomDataReader.Decompress(bus,
                    CeresCinematicSourceDefinitions.ZebesCharacters,
                    maximumOutputBytes: ZebesRevealArtworkFormat.CharacterByteCount)),
            "installed Zebes reveal PNG preserves every transferred character byte");

        foreach (CeresDestructionSpriteFrameDefinition frame in
            CeresDestructionSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    y < CinematicSpriteDrawDefinitions.OnScreenOriginYLimit;
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
                CinematicSheetFixtures.Drawer(IntroCinematicSourceArtwork.Of(stock).CeresDestructionSprites).Draw(frame.Pose, installedOam,
                    new(120, y), ScreenColourGroup.CeresFlightSpaceObjects, onScreen);
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
                        actual.Paint.Colors.SequenceEqual(expected.Paint.Colors) &&
                        DisplayRendering.Render(actual).SequenceEqual(
                            DisplayRendering.Render(expected)),
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

        string overrideRoot = installation.IntroCinematicOverrideDirectory;
        Directory.CreateDirectory(overrideRoot);
        string actorsName = CeresRevealActorLayoutFormat.FileName;
        string actorsPath = Path.Combine(installation.IntroCinematicDirectory, actorsName);
        string actorsOverride = Path.Combine(overrideRoot, actorsName);
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
        int stockPlanetX = actorsDocument.Actors[0].X;
        actorsDocument.Actors[0] = actorsDocument.Actors[0] with { X = stockPlanetX + 16 };
        using (var output = File.Create(actorsOverride))
            CeresRevealActorPlacements.Write(output, actorsDocument);
        IntroCinematicArtwork movedPlanet = installation.LoadIntroCinematicArt();
        var stockReveal = CreateRetailDestructionFixture(artwork: stock);
        var movedReveal = CreateRetailDestructionFixture(artwork: movedPlanet);
        for (int frame = 0; frame < 4000 &&
            stockReveal.Phase != CeresDestructionPhase.PlanetZebesTitle; frame++)
        {
            stockReveal.Step();
            movedReveal.Step();
        }
        AssertEqual(CeresDestructionPhase.PlanetZebesTitle, stockReveal.Phase,
            "Zebes placement fixture reaches the planet reveal");
        AssertEqual(stockReveal.Phase, movedReveal.Phase,
            "visual placement edit preserves cinematic handoff timing");
        var stockPlanet = stockReveal.ZebesPlanetActor ??
            throw new InvalidOperationException("Stock Zebes planet actor did not spawn.");
        var editedPlanet = movedReveal.ZebesPlanetActor ??
            throw new InvalidOperationException("Edited Zebes planet actor did not spawn.");
        AssertEqual(unchecked((ushort)(stockPlanet.WholePosition.X + 16)), editedPlanet.WholePosition.X,
            "edited scene position moves the actual planet actor by one tile");
        for (int frame = 0; frame < 20; frame++)
        {
            stockReveal.Step();
            movedReveal.Step();
        }
        AssertTrue(!stockReveal.CaptureRenderSnapshot().Paint.SpriteParts().SequenceEqual(
                movedReveal.CaptureRenderSnapshot().Paint.SpriteParts()),
            "edited planet placement changes production OAM, not just JSON parsing");
        File.WriteAllBytes(actorsPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
            "a valid actor override cannot conceal damaged stock layout");
        GameInstallation repairedActors = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Ceres reveal actor repair lost installation.");
        AssertEqual(stockPlanetX + 16,
            repairedActors.LoadIntroCinematicArt().CeresDestruction.RevealActors[0].X,
            "actor layout override survives stock repair");
        actorsDocument.Actors[0] = actorsDocument.Actors[0] with { Id = "wrong-actor" };
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            CeresRevealActorPlacements.Write(invalid, actorsDocument);
        }, "Zebes reveal layout rejects missing or reordered actor identities");
        File.Delete(actorsOverride);

        string destructionActorsName = CeresDestructionActorLayoutFormat.FileName;
        string destructionActorsPath = Path.Combine(installation.IntroCinematicDirectory,
            destructionActorsName);
        string destructionActorsOverride = Path.Combine(overrideRoot, destructionActorsName);
        CeresDestructionActorLayoutDocument destructionActorsDocument =
            JsonSerializer.Deserialize<CeresDestructionActorLayoutDocument>(
                File.ReadAllBytes(destructionActorsPath), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock Ceres destruction actor layout is empty.");
        for (int index = 0; index < destructionActorsDocument.Actors.Length; index++)
        {
            CeresDestructionActorDefinition definition =
                CeresDestructionActors.InitialActor(index);
            AssertEqual(CeresDestructionActors.InitialPlacementId(index),
                destructionActorsDocument.Actors[index].Id,
                $"Ceres destruction actor {index} retains its native role");
            AssertEqual(definition.Position.X, destructionActorsDocument.Actors[index].X,
                $"Ceres destruction actor {index} X matches its verified initializer");
            AssertEqual(definition.Position.Y, destructionActorsDocument.Actors[index].Y,
                $"Ceres destruction actor {index} Y matches its verified initializer");
        }
        int stockAsteroidX = destructionActorsDocument.Actors[0].X;
        destructionActorsDocument.Actors[0] = destructionActorsDocument.Actors[0] with
        {
            X = stockAsteroidX + 16,
        };
        using (var output = File.Create(destructionActorsOverride))
            CeresDestructionActorPlacements.Write(output, destructionActorsDocument);
        IntroCinematicArtwork movedAsteroid = installation.LoadIntroCinematicArt();
        var stockDestruction = CreateRetailDestructionFixture(artwork: stock);
        var movedDestruction = CreateRetailDestructionFixture(artwork: movedAsteroid);
        for (int frame = 0; frame < 36; frame++)
        {
            stockDestruction.Step();
            movedDestruction.Step();
        }
        AssertEqual(stockDestruction.Phase, movedDestruction.Phase,
            "edited Ceres asteroid placement preserves cinematic phase timing");
        var stockDestructionActors = stockDestruction.actors ??
            throw new InvalidOperationException("Stock Ceres destruction actors did not spawn.");
        var movedDestructionActors =
            movedDestruction.actors ??
            throw new InvalidOperationException("Edited Ceres destruction actors did not spawn.");
        AssertEqual(unchecked((ushort)(stockDestructionActors[0].WholePosition.X + 16)),
            movedDestructionActors[0].WholePosition.X,
            "edited destruction layout moves the actual asteroid actor by one tile");
        AssertTrue(!stockDestruction.CaptureRenderSnapshot().Paint.SpriteParts().SequenceEqual(
                movedDestruction.CaptureRenderSnapshot().Paint.SpriteParts()),
            "edited destruction placement changes production OAM");
        File.WriteAllBytes(destructionActorsPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
            "a destruction actor override cannot hide damaged stock layout");
        GameInstallation repairedDestructionActors =
            GameAssetInstaller.EnsureInstalled(installation.Root) ??
            throw new InvalidOperationException("Ceres destruction actor repair lost installation.");
        AssertEqual(stockAsteroidX + 16,
            repairedDestructionActors.LoadIntroCinematicArt().CeresDestruction.DestructionActors[0].X,
            "destruction actor override survives stock repair");
        destructionActorsDocument.Actors[0] = destructionActorsDocument.Actors[0] with
        {
            Id = "wrong-actor",
        };
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            CeresDestructionActorPlacements.Write(invalid, destructionActorsDocument);
        }, "Ceres destruction layout rejects missing or reordered actor identities");
        File.Delete(destructionActorsOverride);

        string spritesName = CeresDestructionSpriteFormat.FileName;
        string spritesPath = Path.Combine(installation.IntroCinematicDirectory, spritesName);
        string spritesOverride = Path.Combine(overrideRoot, spritesName);
        CeresDestructionSpriteDocument spriteDocument =
            JsonSerializer.Deserialize<CeresDestructionSpriteDocument>(
                File.ReadAllBytes(spritesPath), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock Ceres destruction sprites are empty.");
        string changedFrame = "station-under-attack-large-asteroid";
        spriteDocument.Frames[changedFrame] = spriteDocument.Frames[changedFrame]
            .Select(part => part with { OffsetX = part.OffsetX + 16 }).ToArray();
        using (var output = File.Create(spritesOverride))
            CeresDestructionSpritePresentation.Write(output, spriteDocument);
        IntroCinematicArtwork editedSprites = installation.LoadIntroCinematicArt();
        var spriteState = CreateRetailDestructionFixture(artwork: stock);
        for (int frame = 0; frame < 36; frame++) spriteState.Step();
        LayeredScene beforeSprites = spriteState.CaptureRenderSnapshot();
        spriteState.BindFixtureDestructionArtwork(editedSprites);
        LayeredScene afterSprites = spriteState.CaptureRenderSnapshot();
        AssertTrue(!beforeSprites.Paint.SpriteParts().SequenceEqual(afterSprites.Paint.SpriteParts()) &&
                !DisplayRendering.Render(beforeSprites).SequenceEqual(
                    DisplayRendering.Render(afterSprites)),
            "edited Ceres destruction composition changes production OAM and visible pixels");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "rebound Ceres destruction actor never rereads installed spritemaps");
        spriteDocument.Frames.Remove(changedFrame);
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            CeresDestructionSpritePresentation.Write(invalid, spriteDocument);
        }, "Ceres destruction sprites reject a missing named frame");
        File.WriteAllBytes(spritesPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
            "a destruction sprite override cannot hide corrupt stock artwork");
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Ceres destruction sprite repair lost installation.");
        var repairedSpriteState = CreateRetailDestructionFixture(artwork: repaired.LoadIntroCinematicArt());
        for (int frame = 0; frame < 36; frame++) repairedSpriteState.Step();
        AssertTrue(repairedSpriteState.CaptureRenderSnapshot().Paint.SpriteParts().SequenceEqual(
                afterSprites.Paint.SpriteParts()),
            "Ceres destruction sprite override survives stock repair and remains visible");
        File.Delete(spritesOverride);

        string ceresPath = Path.Combine(installation.IntroCinematicDirectory,
            CeresDestructionArtworkFormat.CeresMapFileName);
        string ceresOverride = Path.Combine(overrideRoot,
            CeresDestructionArtworkFormat.CeresMapFileName);
        CeresDestructionMapDocument ceres;
        using (var input = File.OpenRead(ceresPath))
            ceres = JsonSerializer.Deserialize<CeresDestructionMapDocument>(input,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Stock Ceres destruction JSON is empty.");
        foreach (int[] view in ceres.Views) Array.Fill(view, 0);
        using (var output = File.Create(ceresOverride))
            CeresDestructionArtwork.WriteMap(output, ceres);
        IntroCinematicArtwork editedCeres = installation.LoadIntroCinematicArt();
        var ceresState = CreateRetailDestructionFixture(artwork: stock);
        for (int frame = 0; frame < 36; frame++) ceresState.Step();
        LayeredScene beforeCeres = ceresState.CaptureRenderSnapshot();
        byte[] beforeCeresVram = ceresState.ShownVram();
        ceresState.BindFixtureDestructionArtwork(editedCeres);
        LayeredScene afterCeres = ceresState.CaptureRenderSnapshot();
        byte[] afterCeresVram = ceresState.ShownVram();
        AssertTrue(!beforeCeresVram.SequenceEqual(afterCeresVram) &&
                !DisplayRendering.Render(beforeCeres).SequenceEqual(
                    DisplayRendering.Render(afterCeres)),
            "restored Ceres destruction adopts edited map and changes visible pixels");
        File.Delete(ceresOverride);

        string zebesMapPath = Path.Combine(installation.IntroCinematicDirectory,
            ZebesRevealArtworkFormat.MapFileName);
        string zebesMapOverride = Path.Combine(overrideRoot,
            ZebesRevealArtworkFormat.MapFileName);
        RoomBackgroundTilemapDocument zebesMap;
        using (var input = File.OpenRead(zebesMapPath))
            zebesMap = JsonSerializer.Deserialize<RoomBackgroundTilemapDocument>(input,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Stock Zebes reveal JSON is empty.");
        RoomBackgroundTilemapCell originalCell = zebesMap.Pages[0].Cells[0];
        zebesMap.Pages[0].Cells[0] = originalCell with
        {
            TileColumn = (originalCell.TileColumn + 1) % RoomBackgroundTilemapFormat.TileColumns,
        };
        using (var output = File.Create(zebesMapOverride))
            RoomBackgroundTilemapAtlas.Write(output, zebesMap,
                ZebesRevealArtworkFormat.MapByteCount);
        IntroCinematicArtwork editedZebesMap = installation.LoadIntroCinematicArt();
        AssertTrue(!IntroCinematicSourceArtwork.Of(editedZebesMap).ZebesReveal.Map.Bytes.Span.SequenceEqual(
                IntroCinematicSourceArtwork.Of(stock).ZebesReveal.Map.Bytes.Span),
            "independent Zebes tilemap JSON edit compiles into its native transfer");
        File.Delete(zebesMapOverride);

        string zebesPngPath = Path.Combine(installation.IntroCinematicDirectory,
            ZebesRevealArtworkFormat.CharacterFileName);
        string zebesPngOverride = Path.Combine(overrideRoot,
            ZebesRevealArtworkFormat.CharacterFileName);
        IndexedPngImage zebesPng;
        using (var input = File.OpenRead(zebesPngPath))
            zebesPng = IndexedPng.Read(input, new(256, 128));
        zebesPng.Pixels[0] = (byte)((zebesPng.Pixels[0] + 1) & 15);
        using (var output = File.Create(zebesPngOverride))
            IndexedPng.Write(output, new(zebesPng.Size.X, zebesPng.Size.Y),
                zebesPng.Pixels, zebesPng.Colours);
        IntroCinematicArtwork editedZebesPng = installation.LoadIntroCinematicArt();
        File.Delete(zebesPngOverride);

        var zebesState = CreateRetailDestructionFixture(artwork: stock);
        for (int frame = 0; frame < 3000 &&
            zebesState.Phase != CeresDestructionPhase.FadeInZebes; frame++)
            zebesState.Step();
        AssertEqual(CeresDestructionPhase.FadeInZebes, zebesState.Phase,
            "rebind fixture reaches Zebes reveal");
        LayeredScene originalZebes = zebesState.CaptureRenderSnapshot();
        byte[] originalZebesVram = zebesState.ShownVram();
        zebesState.BindFixtureDestructionArtwork(editedZebesMap);
        AssertTrue(!zebesState.ShownVram().SequenceEqual(
                originalZebesVram),
            "restored Zebes reveal adopts edited BG map without resetting phase");
        zebesState.BindFixtureDestructionArtwork(editedZebesPng);
        AssertTrue(!zebesState.ShownVram().SequenceEqual(
                originalZebesVram),
            "restored Zebes reveal adopts edited character PNG without resetting phase");
        AssertEqual(CeresDestructionPhase.FadeInZebes, zebesState.Phase,
            "visual rebind leaves native cinematic phase unchanged");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "edited/rebound destruction art never reads cartridge art sources");

        File.WriteAllBytes(ceresOverride, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
            "malformed Ceres destruction override fails loudly");
        File.Delete(ceresOverride);
        Console.WriteLine("Ceres destruction art: 23 native OAM frames, three destruction and six ROM-sourced reveal actor placements, visible edits, exact PNG/JSON transfers, overrides and state rebind pass.");
    }
}
