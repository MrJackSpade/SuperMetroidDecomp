using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyCeresDestructionArtwork(GameInstallation installation,
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        IntroCinematicArtworkCatalog stock = installation.LoadIntroCinematicArt();
        byte[] nativeMaps = RomDataReader.Decompress(bus,
            CeresDestructionRomData.Assets.CeresTilemaps,
            maximumOutputBytes: CeresDestructionRomData.Vram.CompressedTilemapLimit);
        AssertTrue(stock.CeresDestruction.CeresMaps.Span.SequenceEqual(nativeMaps.AsSpan(
                2 * CeresFlightArtworkFormat.MapCellsPerView,
                CeresDestructionArtworkFormat.MapByteCount)),
            "installed destruction JSON preserves the three remaining Ceres map slices");
        byte[] nativeZebesMap = RomDataReader.Decompress(bus,
            CeresDestructionRomData.Assets.ZebesTilemap,
            maximumOutputBytes: CeresDestructionRomData.Vram.CompressedTilemapLimit);
        AssertTrue(stock.CeresDestruction.ZebesMap.Transfer.Span.SequenceEqual(
                nativeZebesMap.AsSpan(0, CeresDestructionArtworkFormat.ZebesMapByteCount)),
            "installed Zebes reveal JSON preserves every transferred BG word");
        AssertTrue(stock.CeresDestruction.ZebesCharacters.Transfer.Span.SequenceEqual(
                RomDataReader.Decompress(bus,
                    CeresDestructionRomData.Assets.ZebesCharacters,
                    maximumOutputBytes: CeresDestructionArtworkFormat.ZebesCharacterByteCount)),
            "installed Zebes reveal PNG preserves every transferred character byte");

        foreach (CeresDestructionSpriteFrameDefinition frame in
            CeresDestructionSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    (y & CinematicSpriteDrawDefinitions.OriginYHighByteMask) == 0;
                int source = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                var nativeOam = new OamBuffer();
                nativeOam.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, nativeOam, source, 120, y, 0x0800);
                else
                    DrawImportedSpritemap(bus, nativeOam, source, 120, y, 0x0800, originIsOnScreen: false);
                nativeOam.FinalizeFrame();
                var installedOam = new OamBuffer();
                installedOam.BeginFrame();
                stock.CeresDestruction.Sprites.Draw(frame.Pointer, installedOam,
                    120, y, 0x0800, onScreen);
                installedOam.FinalizeFrame();
                AssertTrue(installedOam.LowTable.SequenceEqual(nativeOam.LowTable) &&
                        installedOam.HighTable.SequenceEqual(nativeOam.HighTable) &&
                        installedOam.LastFinalizedSpriteCount == nativeOam.LastFinalizedSpriteCount,
                    $"Ceres destruction {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }

        var guard = new IntroArtworkSourceReadGuard(bus,
            blockCeresFlightSprites: true, blockCeresDestructionSprites: true);
        var native = new CeresDestructionCinematicState(bus);
        var installed = new CeresDestructionCinematicState(guard, artwork: stock);
        var phases = new HashSet<CeresDestructionPhase>();
        for (int frame = 0; frame < 5000 && !native.Finished; frame++)
        {
            AssertEqual(native.Phase, installed.Phase,
                $"installed destruction artwork preserves phase at frame {frame}");
            bool firstPhaseFrame = phases.Add(native.Phase);
            if (firstPhaseFrame || frame % 211 == 0)
            {
                LayeredRenderSnapshot expected = native.CaptureRenderSnapshot();
                LayeredRenderSnapshot actual = installed.CaptureRenderSnapshot();
                AssertTrue(actual.Brightness == expected.Brightness &&
                        actual.Memory.Vram.SequenceEqual(expected.Memory.Vram) &&
                        actual.Memory.Cgram.SequenceEqual(expected.Memory.Cgram) &&
                        SoftwareLayeredSnapshotRenderer.Render(actual).AsSpan().SequenceEqual(
                            SoftwareLayeredSnapshotRenderer.Render(expected)),
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
                File.ReadAllBytes(actorsPath), MapPresentationFormat.JsonOptions) ??
            throw new InvalidDataException("Stock Zebes reveal actor layout is empty.");
        for (int index = 0; index < actorsDocument.Actors.Length; index++)
        {
            var source = CeresDestructionActorDefinitions.ZebesPlacementSource(index);
            AssertEqual(source.Id, actorsDocument.Actors[index].Id,
                $"Zebes reveal actor {index} retains its native identity");
            AssertEqual((int)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresDestructionActorDefinitions.NativeBank | source.XAddress),
                actorsDocument.Actors[index].X,
                $"Zebes reveal actor {index} X matches the cartridge operand");
            AssertEqual((int)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresDestructionActorDefinitions.NativeBank | source.YAddress),
                actorsDocument.Actors[index].Y,
                $"Zebes reveal actor {index} Y matches the cartridge operand");
        }
        int stockPlanetX = actorsDocument.Actors[0].X;
        actorsDocument.Actors[0] = actorsDocument.Actors[0] with { X = stockPlanetX + 16 };
        using (var output = File.Create(actorsOverride))
            CeresRevealActorLayout.Write(output, actorsDocument);
        IntroCinematicArtworkCatalog movedPlanet = installation.LoadIntroCinematicArt();
        var stockReveal = new CeresDestructionCinematicState(guard, artwork: stock);
        var movedReveal = new CeresDestructionCinematicState(guard, artwork: movedPlanet);
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
        var planetField = typeof(CeresDestructionCinematicState).GetField("zebesPlanetActor",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) ??
            throw new InvalidOperationException("Zebes planet actor field is unavailable.");
        var stockPlanet = (IntroDiscoverySprite?)planetField.GetValue(stockReveal) ??
            throw new InvalidOperationException("Stock Zebes planet actor did not spawn.");
        var editedPlanet = (IntroDiscoverySprite?)planetField.GetValue(movedReveal) ??
            throw new InvalidOperationException("Edited Zebes planet actor did not spawn.");
        AssertEqual(unchecked((ushort)(stockPlanet.XPosition + 16)), editedPlanet.XPosition,
            "edited scene position moves the actual planet actor by one tile");
        for (int frame = 0; frame < 20; frame++)
        {
            stockReveal.Step();
            movedReveal.Step();
        }
        AssertTrue(!stockReveal.CaptureRenderSnapshot().Memory.Oam.SequenceEqual(
                movedReveal.CaptureRenderSnapshot().Memory.Oam),
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
            CeresRevealActorLayout.Write(invalid, actorsDocument);
        }, "Zebes reveal layout rejects missing or reordered actor identities");
        File.Delete(actorsOverride);

        string destructionActorsName = CeresDestructionActorLayoutFormat.FileName;
        string destructionActorsPath = Path.Combine(installation.IntroCinematicDirectory,
            destructionActorsName);
        string destructionActorsOverride = Path.Combine(overrideRoot, destructionActorsName);
        CeresDestructionActorLayoutDocument destructionActorsDocument =
            JsonSerializer.Deserialize<CeresDestructionActorLayoutDocument>(
                File.ReadAllBytes(destructionActorsPath), MapPresentationFormat.JsonOptions) ??
            throw new InvalidDataException("Stock Ceres destruction actor layout is empty.");
        for (int index = 0; index < destructionActorsDocument.Actors.Length; index++)
        {
            CeresDestructionActorDefinition definition =
                CeresDestructionActorDefinitions.InitialActor(index);
            AssertEqual(CeresDestructionActorDefinitions.InitialPlacementId(index),
                destructionActorsDocument.Actors[index].Id,
                $"Ceres destruction actor {index} retains its native role");
            AssertEqual((int)definition.X, destructionActorsDocument.Actors[index].X,
                $"Ceres destruction actor {index} X matches its verified initializer");
            AssertEqual((int)definition.Y, destructionActorsDocument.Actors[index].Y,
                $"Ceres destruction actor {index} Y matches its verified initializer");
        }
        int stockAsteroidX = destructionActorsDocument.Actors[0].X;
        destructionActorsDocument.Actors[0] = destructionActorsDocument.Actors[0] with
        {
            X = stockAsteroidX + 16,
        };
        using (var output = File.Create(destructionActorsOverride))
            CeresDestructionActorLayout.Write(output, destructionActorsDocument);
        IntroCinematicArtworkCatalog movedAsteroid = installation.LoadIntroCinematicArt();
        var stockDestruction = new CeresDestructionCinematicState(guard, artwork: stock);
        var movedDestruction = new CeresDestructionCinematicState(guard, artwork: movedAsteroid);
        for (int frame = 0; frame < 36; frame++)
        {
            stockDestruction.Step();
            movedDestruction.Step();
        }
        AssertEqual(stockDestruction.Phase, movedDestruction.Phase,
            "edited Ceres asteroid placement preserves cinematic phase timing");
        var destructionActorsField = typeof(CeresDestructionCinematicState).GetField("actors",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) ??
            throw new InvalidOperationException("Ceres destruction actor list is unavailable.");
        var stockDestructionActors =
            (List<IntroDiscoverySprite>?)destructionActorsField.GetValue(stockDestruction) ??
            throw new InvalidOperationException("Stock Ceres destruction actors did not spawn.");
        var movedDestructionActors =
            (List<IntroDiscoverySprite>?)destructionActorsField.GetValue(movedDestruction) ??
            throw new InvalidOperationException("Edited Ceres destruction actors did not spawn.");
        AssertEqual(unchecked((ushort)(stockDestructionActors[0].XPosition + 16)),
            movedDestructionActors[0].XPosition,
            "edited destruction layout moves the actual asteroid actor by one tile");
        AssertTrue(!stockDestruction.CaptureRenderSnapshot().Memory.Oam.SequenceEqual(
                movedDestruction.CaptureRenderSnapshot().Memory.Oam),
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
            CeresDestructionActorLayout.Write(invalid, destructionActorsDocument);
        }, "Ceres destruction layout rejects missing or reordered actor identities");
        File.Delete(destructionActorsOverride);

        string spritesName = CeresDestructionSpriteFormat.FileName;
        string spritesPath = Path.Combine(installation.IntroCinematicDirectory, spritesName);
        string spritesOverride = Path.Combine(overrideRoot, spritesName);
        CeresDestructionSpriteDocument spriteDocument =
            JsonSerializer.Deserialize<CeresDestructionSpriteDocument>(
                File.ReadAllBytes(spritesPath), MapPresentationFormat.JsonOptions) ??
            throw new InvalidDataException("Stock Ceres destruction sprites are empty.");
        string changedFrame = "station-under-attack-large-asteroid";
        spriteDocument.Frames[changedFrame] = spriteDocument.Frames[changedFrame]
            .Select(part => part with { OffsetX = part.OffsetX + 16 }).ToArray();
        using (var output = File.Create(spritesOverride))
            CeresDestructionSpritePresentation.Write(output, spriteDocument);
        IntroCinematicArtworkCatalog editedSprites = installation.LoadIntroCinematicArt();
        var spriteState = new CeresDestructionCinematicState(guard, artwork: stock);
        for (int frame = 0; frame < 36; frame++) spriteState.Step();
        LayeredRenderSnapshot beforeSprites = spriteState.CaptureRenderSnapshot();
        spriteState.BindArtwork(editedSprites);
        LayeredRenderSnapshot afterSprites = spriteState.CaptureRenderSnapshot();
        AssertTrue(!beforeSprites.Memory.Oam.SequenceEqual(afterSprites.Memory.Oam) &&
                !SoftwareLayeredSnapshotRenderer.Render(beforeSprites).AsSpan().SequenceEqual(
                    SoftwareLayeredSnapshotRenderer.Render(afterSprites)),
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
        var repairedSpriteState = new CeresDestructionCinematicState(guard,
            artwork: repaired.LoadIntroCinematicArt());
        for (int frame = 0; frame < 36; frame++) repairedSpriteState.Step();
        AssertTrue(repairedSpriteState.CaptureRenderSnapshot().Memory.Oam.SequenceEqual(
                afterSprites.Memory.Oam),
            "Ceres destruction sprite override survives stock repair and remains visible");
        File.Delete(spritesOverride);

        string ceresPath = Path.Combine(installation.IntroCinematicDirectory,
            CeresDestructionArtworkFormat.CeresMapFileName);
        string ceresOverride = Path.Combine(overrideRoot,
            CeresDestructionArtworkFormat.CeresMapFileName);
        CeresDestructionMapDocument ceres;
        using (var input = File.OpenRead(ceresPath))
            ceres = JsonSerializer.Deserialize<CeresDestructionMapDocument>(input,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Stock Ceres destruction JSON is empty.");
        foreach (int[] view in ceres.Views) Array.Fill(view, 0);
        using (var output = File.Create(ceresOverride))
            CeresDestructionArtworkCatalog.WriteMap(output, ceres);
        IntroCinematicArtworkCatalog editedCeres = installation.LoadIntroCinematicArt();
        var ceresState = new CeresDestructionCinematicState(guard, artwork: stock);
        for (int frame = 0; frame < 36; frame++) ceresState.Step();
        LayeredRenderSnapshot beforeCeres = ceresState.CaptureRenderSnapshot();
        ceresState.BindArtwork(editedCeres);
        LayeredRenderSnapshot afterCeres = ceresState.CaptureRenderSnapshot();
        AssertTrue(!beforeCeres.Memory.Vram.SequenceEqual(afterCeres.Memory.Vram) &&
                !SoftwareLayeredSnapshotRenderer.Render(beforeCeres).AsSpan().SequenceEqual(
                    SoftwareLayeredSnapshotRenderer.Render(afterCeres)),
            "restored Ceres destruction adopts edited map and changes visible pixels");
        File.Delete(ceresOverride);

        string zebesMapPath = Path.Combine(installation.IntroCinematicDirectory,
            CeresDestructionArtworkFormat.ZebesMapFileName);
        string zebesMapOverride = Path.Combine(overrideRoot,
            CeresDestructionArtworkFormat.ZebesMapFileName);
        RoomBackgroundTilemapDocument zebesMap;
        using (var input = File.OpenRead(zebesMapPath))
            zebesMap = JsonSerializer.Deserialize<RoomBackgroundTilemapDocument>(input,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Stock Zebes reveal JSON is empty.");
        RoomBackgroundTilemapCell originalCell = zebesMap.Pages[0].Cells[0];
        zebesMap.Pages[0].Cells[0] = originalCell with
        {
            TileColumn = (originalCell.TileColumn + 1) % RoomBackgroundTilemapFormat.TileColumns,
        };
        using (var output = File.Create(zebesMapOverride))
            RoomBackgroundTilemapAtlas.Write(output, zebesMap,
                CeresDestructionArtworkFormat.ZebesMapByteCount);
        IntroCinematicArtworkCatalog editedZebesMap = installation.LoadIntroCinematicArt();
        AssertTrue(!editedZebesMap.CeresDestruction.ZebesMap.Transfer.Span.SequenceEqual(
                stock.CeresDestruction.ZebesMap.Transfer.Span),
            "independent Zebes tilemap JSON edit compiles into its native transfer");
        File.Delete(zebesMapOverride);

        string zebesPngPath = Path.Combine(installation.IntroCinematicDirectory,
            CeresDestructionArtworkFormat.ZebesCharacterFileName);
        string zebesPngOverride = Path.Combine(overrideRoot,
            CeresDestructionArtworkFormat.ZebesCharacterFileName);
        IndexedPngImage zebesPng;
        using (var input = File.OpenRead(zebesPngPath))
            zebesPng = IndexedPng.Read(input, 256, 128);
        zebesPng.Pixels[0] = (byte)((zebesPng.Pixels[0] + 1) & 15);
        using (var output = File.Create(zebesPngOverride))
            IndexedPng.Write(output, zebesPng.Width, zebesPng.Height,
                zebesPng.Pixels, zebesPng.Palette);
        IntroCinematicArtworkCatalog editedZebesPng = installation.LoadIntroCinematicArt();
        File.Delete(zebesPngOverride);

        var zebesState = new CeresDestructionCinematicState(guard, artwork: stock);
        for (int frame = 0; frame < 3000 &&
            zebesState.Phase != CeresDestructionPhase.FadeInZebes; frame++)
            zebesState.Step();
        AssertEqual(CeresDestructionPhase.FadeInZebes, zebesState.Phase,
            "rebind fixture reaches Zebes reveal");
        LayeredRenderSnapshot originalZebes = zebesState.CaptureRenderSnapshot();
        zebesState.BindArtwork(editedZebesMap);
        AssertTrue(!zebesState.CaptureRenderSnapshot().Memory.Vram.SequenceEqual(
                originalZebes.Memory.Vram),
            "restored Zebes reveal adopts edited BG map without resetting phase");
        zebesState.BindArtwork(editedZebesPng);
        AssertTrue(!zebesState.CaptureRenderSnapshot().Memory.Vram.SequenceEqual(
                originalZebes.Memory.Vram),
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
