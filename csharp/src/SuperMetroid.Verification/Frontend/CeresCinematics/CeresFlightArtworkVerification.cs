using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresFlightSpriteArtworkVerification;
using static SuperMetroid.Verification.Harness.GraphDigestVerification;
using static SuperMetroid.Verification.Frontend.Intro.IntroCinematicArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.Core.Rooms.Backgrounds;
namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks installed Ceres flight Mode 7, sprite, actor, and palette art against retail sources.</summary>
internal static class CeresFlightArtworkVerification
{
    /// <summary>
    /// Standalone frames from a new flight into its visible front approach: $8B:BCA0's
    /// dispatch and $8B:BDE4's 24-dispatch music wait precede the 32-dispatch approach
    /// (13% capture updates 6999-7055).
    /// </summary>
    internal const int CeresFrontApproachTicks = 30;

    /// <summary>Checks native front and rear transfers, visible edits, restored bindings, and stock repair.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ceres flight Mode 7 characters, maps, object characters and palette")]
    internal static void VerifyCeresFlightArtwork(GameInstallation installation,
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus)
    {
        CeresFlightArtworkCatalog stock = installation.LoadIntroCinematicArt().CeresFlight;
        AssertTrue(stock.FloorCharacters.Span.SequenceEqual(RomDataReader.Decompress(bus,
                CeresCinematicSourceDefinitions.FlightFloorCharacters,
                maximumOutputBytes: CeresFlightArtworkFormat.FloorByteCount)),
            "installed Ceres Mode-7 PNG preserves every native character byte");
        AssertTrue(stock.ObjectCharacters.Span.SequenceEqual(RomDataReader.Decompress(bus,
                CeresCinematicSourceDefinitions.FlightObjectCharacters,
                maximumOutputBytes: CeresFlightArtworkFormat.ObjectByteCount)),
            "installed Ceres OBJ PNG preserves every native character byte");
        byte[] nativeMaps = RomDataReader.Decompress(bus, CeresCinematicSourceDefinitions.CeresTilemaps,
            maximumOutputBytes: 0x1000);
        AssertTrue(stock.FloorMaps.Span.SequenceEqual(nativeMaps.AsSpan(0,
                CeresFlightArtworkFormat.MapCellCount)),
            "installed front/rear Ceres maps preserve every consumed native byte");
        byte[] nativePalette = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            CeresCinematicSourceDefinitions.FlightPalette, CgramPalettes.ByteCount);
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            AssertEqual(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(nativePalette.AsSpan(index * sizeof(ushort))),
                stock.Palette.ColorAt(index), "installed Ceres palette preserves every native color");

        var guard = NewIntroArtworkSourceReadGuard(bus, IntroArtworkSources.CeresFlightSprites);
        var referenceFiles = CeresFlightArtworkExtractor.Extract(bus);
        using var referenceCharacters = new MemoryStream(referenceFiles[CeresFlightArtworkFormat.FloorFileName]);
        using var referenceMaps = new MemoryStream(referenceFiles[CeresFlightArtworkFormat.MapFileName]);
        using var referenceObjects = new MemoryStream(referenceFiles[CeresFlightArtworkFormat.ObjectFileName]);
        using var referencePalette = new MemoryStream(referenceFiles[CeresFlightPaletteFormat.FileName]);
        using var referenceSprites = new MemoryStream(referenceFiles[CeresFlightSpriteFormat.FileName]);
        using var referenceActors = new MemoryStream(referenceFiles[CeresFlightActorLayoutFormat.FileName]);
        CeresFlightCharacters reference = CeresFlightCharacters.Read(
            referenceCharacters, referenceMaps, referenceObjects, CeresFlightPaletteFile.Read(referencePalette),
            referenceSprites, referenceActors);
        var native = new IntroCeresFlightState(new AudioQueues(), reference, sheets: CinematicSheetFixtures.WithFlight(reference));
        var installed = new IntroCeresFlightState(new AudioQueues(), stock, sheets: CinematicSheetFixtures.WithFlight(stock));
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
                        compiled.Paint.Colors.SequenceEqual(original.Paint.Colors) &&
                        DisplayRendering.Render(compiled).SequenceEqual(
                            DisplayRendering.Render(original)),
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
        Suite(nameof(VerifyCeresFlightActorLayout), () => VerifyCeresFlightActorLayout(installation, bus, stock));

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string[] names =
        [
            CeresFlightArtworkFormat.FloorFileName,
            CeresFlightArtworkFormat.MapFileName,
            CeresFlightArtworkFormat.ObjectFileName,
        ];
        foreach (string name in names)
        {
            string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
            string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
            if (name == CeresFlightArtworkFormat.MapFileName)
            {
                CeresFlightMapDocument map;
                using (var input = File.OpenRead(stockPath))
                    map = JsonSerializer.Deserialize<CeresFlightMapDocument>(input,
                        ArtworkJson.Options)
                        ?? throw new InvalidDataException("Ceres Mode-7 map is empty.");
                map.FrontTiles[0] = unchecked((byte)(map.FrontTiles[0] + 1));
                map.RearTiles[0] = unchecked((byte)(map.RearTiles[0] + 1));
                using var output = File.Create(overridePath);
                CeresFlightArtworkCatalog.WriteMap(output, map);
            }
            else
            {
                bool mode7 = name == CeresFlightArtworkFormat.FloorFileName;
                IndexedPngImage image;
                using (var input = File.OpenRead(stockPath))
                    image = IndexedPng.Read(input,
                        new(checked((ushort)(mode7 ? CeresFlightArtworkFormat.FloorWidth : CeresFlightArtworkFormat.ObjectWidth)), checked((ushort)(mode7 ? CeresFlightArtworkFormat.FloorHeight : CeresFlightArtworkFormat.ObjectHeight))));
                image.Pixels[0] = (byte)((image.Pixels[0] + 1) & (mode7 ? 255 : 15));
                using var output = File.Create(overridePath);
                IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
            }

            CeresFlightArtworkCatalog edited = installation.LoadIntroCinematicArt().CeresFlight;
            bool isRearMap = name == CeresFlightArtworkFormat.MapFileName;
            var fresh = new IntroCeresFlightState(new AudioQueues(), edited, sheets: CinematicSheetFixtures.WithFlight(edited));
            AssertTrue(fresh.ShownVram().SequenceEqual(
                    ExpectedCeresFlightVram(edited, rear: false)),
                $"edited {name} reaches exact front-view VRAM without changing other bytes");
            var restored = new IntroCeresFlightState(new AudioQueues(), stock, sheets: CinematicSheetFixtures.WithFlight(stock));
            for (int tick = 0; tick < 100 && restored.Phase != IntroCeresFlightPhase.FlyingTowardCeres; tick++)
                restored.StepFrame();
            AssertEqual(IntroCeresFlightPhase.FlyingTowardCeres, restored.Phase,
                "restored Ceres fixture reaches the rear-view map handoff");
            // The desktop/Android restore path rebinds the parent cinematic, not
            // this nested flight directly. Exercise that real handoff here.
            var parent = CreateRetailIntroFixture(characterArtwork: IntroCinematicArtworkFiles.Load(
                    installation.IntroCinematicDirectory, null));
            parent.CurrentCeresFlight = restored;
            parent.BindFixtureIntroArtwork(installation.LoadIntroCinematicArt());
            AssertTrue(restored.ShownVram().SequenceEqual(
                    ExpectedCeresFlightVram(edited, rear: true)),
                $"restored rear view rebinds edited {name} with native map ordering");
            if (isRearMap)
                AssertTrue(!edited.FloorMaps.Span.SequenceEqual(stock.FloorMaps.Span),
                    "front and rear JSON edits both compile into the active map stream");
            File.Delete(overridePath);
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "edited Ceres artwork and debugger rebind never read cartridge art sources");
        Suite(nameof(VerifyCeresVisibleOverrides), () => VerifyCeresVisibleOverrides(installation));
        Suite(nameof(VerifyCeresFlightPaletteOverride), () => VerifyCeresFlightPaletteOverride(installation, stock));

        string invalidMap = Path.Combine(installation.IntroCinematicOverrideDirectory,
            CeresFlightArtworkFormat.MapFileName);
        File.WriteAllBytes(invalidMap, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
            "malformed Ceres front/rear map fails instead of silently falling back");
        File.Delete(invalidMap);
        CeresFlightMapDocument persistentMap;
        using (var input = File.OpenRead(Path.Combine(installation.IntroCinematicDirectory,
                   CeresFlightArtworkFormat.MapFileName)))
            persistentMap = JsonSerializer.Deserialize<CeresFlightMapDocument>(input,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Ceres Mode-7 map is empty.");
        persistentMap.FrontTiles[0] = unchecked((byte)(persistentMap.FrontTiles[0] + 1));
        using (var output = File.Create(invalidMap))
            CeresFlightArtworkCatalog.WriteMap(output, persistentMap);
        CeresFlightArtworkCatalog selected = installation.LoadIntroCinematicArt().CeresFlight;
        string stockMode7 = Path.Combine(installation.IntroCinematicDirectory,
            CeresFlightArtworkFormat.FloorFileName);
        File.WriteAllBytes(stockMode7, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
            "a Ceres map override cannot hide a corrupt stock PNG");
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Ceres artwork vanished during stock repair.");
        AssertTrue(repaired.LoadIntroCinematicArt().CeresFlight.FloorMaps.Span.SequenceEqual(
                selected.FloorMaps.Span),
            "stock Ceres art repair preserves the player's external map override");
        File.Delete(invalidMap);
        Console.WriteLine("Ceres flight art: native front/rear frames and five ROM-sourced rear actor placements, independent PNG/JSON/palette edits, rebind and strict failures pass.");
    }

    /// <summary>Checks five native rear-view actor positions and an edited asteroid's live OAM result.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Ceres flight actor spritemap layout")]
    private static void VerifyCeresFlightActorLayout(GameInstallation installation,
        SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace bus, CeresFlightArtworkCatalog stock)
    {
        string name = CeresFlightActorLayoutFormat.FileName;
        string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
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

        int stockAsteroidX = document.Actors[0].X;
        document.Actors[0] = document.Actors[0] with { X = stockAsteroidX + 16 };
        using (var output = File.Create(overridePath))
            CeresFlightActorLayout.Write(output, document);
        CeresFlightArtworkCatalog edited = installation.LoadIntroCinematicArt().CeresFlight;
        var original = new IntroCeresFlightState(new AudioQueues(), stock, sheets: CinematicSheetFixtures.WithFlight(stock));
        var changed = new IntroCeresFlightState(new AudioQueues(), edited, sheets: CinematicSheetFixtures.WithFlight(edited));
        for (int frame = 0; frame < 4000 &&
            original.Phase != IntroCeresFlightPhase.FlyingTowardCeres; frame++)
        {
            original.StepFrame();
            changed.StepFrame();
        }
        AssertEqual(IntroCeresFlightPhase.FlyingTowardCeres, original.Phase,
            "Ceres actor-layout fixture reaches the rear-view handoff");
        AssertEqual(original.Phase, changed.Phase,
            "edited rear-view position preserves cinematic phase timing");
        var nativeActors = original.RearViewActors ??
            throw new InvalidOperationException("Stock Ceres rear-view actors did not spawn.");
        var editedActors = changed.RearViewActors ??
            throw new InvalidOperationException("Edited Ceres rear-view actors did not spawn.");
        AssertEqual(unchecked((ushort)(nativeActors[0].WholePosition.X + 16)),
            editedActors[0].WholePosition.X,
            "editable layout moves the real rear-view asteroid actor by one tile");
        for (int frame = 0; frame < 20; frame++)
        {
            original.StepFrame();
            changed.StepFrame();
        }
        AssertTrue(!original.CaptureRenderSnapshot().Paint.SpriteParts().SequenceEqual(
                changed.CaptureRenderSnapshot().Paint.SpriteParts()),
            "edited rear-view placement changes production OAM");

        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
            "a Ceres flight actor override cannot hide a corrupt stock layout");
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root) ??
            throw new InvalidOperationException("Ceres flight actor repair lost installation.");
        AssertEqual(stockAsteroidX + 16,
            repaired.LoadIntroCinematicArt().CeresFlight.Actors[0].X,
            "rear-view actor override survives stock installation repair");
        document.Actors[0] = document.Actors[0] with { Id = "wrong-actor" };
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            CeresFlightActorLayout.Write(invalid, document);
        }, "Ceres flight layout rejects missing or reordered actor identities");
        File.Delete(overridePath);
    }

    /// <summary>Checks an edited palette in approach and destruction scenes, including restoration.</summary>
    private static void VerifyCeresFlightPaletteOverride(GameInstallation installation,
        CeresFlightArtworkCatalog stock)
    {
        string stockPath = Path.Combine(installation.IntroCinematicDirectory,
            CeresFlightPaletteFormat.FileName);
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory,
            CeresFlightPaletteFormat.FileName);
        CeresFlightPaletteDocument document = JsonSerializer.Deserialize<CeresFlightPaletteDocument>(
            File.ReadAllBytes(stockPath), ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Ceres flight palette is empty.");
        document.Colors[0] = document.Colors[0] with
        {
            Red = (document.Colors[0].Red + 17) & 31,
        };
        using (var output = File.Create(overridePath))
            CeresFlightPaletteFile.Write(output, document);

        CeresFlightArtworkCatalog edited = installation.LoadIntroCinematicArt().CeresFlight;
        var native = new IntroCeresFlightState(new AudioQueues(), stock, sheets: CinematicSheetFixtures.WithFlight(stock));
        var changed = new IntroCeresFlightState(new AudioQueues(), edited, sheets: CinematicSheetFixtures.WithFlight(edited));
        for (int tick = 0; tick < CeresFrontApproachTicks; tick++)
        {
            native.StepFrame();
            changed.StepFrame();
        }
        LayeredScene stockFrame = native.CaptureRenderSnapshot();
        byte[] stockFrameVram = native.ShownVram();
        LayeredScene editedFrame = changed.CaptureRenderSnapshot();
        byte[] editedFrameVram = changed.ShownVram();
        AssertTrue(stockFrameVram.SequenceEqual(editedFrameVram),
            "Ceres palette edit does not change graphics VRAM");
        AssertTrue(!stockFrame.Paint.Colors.SequenceEqual(editedFrame.Paint.Colors),
            "Ceres palette edit reaches active CGRAM");
        AssertTrue(!DisplayRendering.Render(stockFrame).SequenceEqual(
                DisplayRendering.Render(editedFrame)),
            "Ceres palette edit changes visible approach pixels");

        var restored = new IntroCeresFlightState(new AudioQueues(), stock, sheets: CinematicSheetFixtures.WithFlight(stock));
        restored.BindFixtureFlightArtwork(edited);
        AssertTrue(restored.CaptureRenderSnapshot().Paint.Colors.SequenceEqual(
                new IntroCeresFlightState(new AudioQueues(), edited, sheets: CinematicSheetFixtures.WithFlight(edited)).CaptureRenderSnapshot().Paint.Colors),
            "restored Ceres flight rebinds current palette without restarting phase");

        IntroCinematicArtworkCatalog stockParent = IntroCinematicArtworkFiles.Load(
            installation.IntroCinematicDirectory, null);
        IntroCinematicArtworkCatalog editedParent = installation.LoadIntroCinematicArt();
        var restoredDestruction = new CeresDestructionCinematicState(artwork: stockParent,
            spriteSheets: CinematicSheetFixtures.WithIntro(stockParent));
        var editedDestruction = new CeresDestructionCinematicState(artwork: editedParent,
            spriteSheets: CinematicSheetFixtures.WithIntro(editedParent));
        // $8B:C11B loads the shared Ceres palette only after its initial NMI waits.
        for (int update = 0; update <= CeresDestructionSceneDefinitions.InitialFrameWaits; update++)
        {
            restoredDestruction.Step();
            editedDestruction.Step();
        }
        AssertTrue(!restoredDestruction.CaptureRenderSnapshot().Paint.Colors.SequenceEqual(
                editedDestruction.CaptureRenderSnapshot().Paint.Colors),
            "shared Ceres palette edit reaches destruction CGRAM");
        restoredDestruction.BindFixtureDestructionArtwork(editedParent);
        AssertTrue(restoredDestruction.CaptureRenderSnapshot().Paint.Colors.SequenceEqual(
                editedDestruction.CaptureRenderSnapshot().Paint.Colors),
            "restored destruction rebinds its shared palette before engine FX owns CGRAM");
        File.Delete(overridePath);

        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
            "malformed Ceres flight palette fails instead of using stock silently");
        File.Delete(overridePath);
    }

    /// <summary>Confirms each PNG and both Mode 7 map views can alter rendered flight pixels.</summary>
    private static void VerifyCeresVisibleOverrides(GameInstallation installation)
    {
        foreach (string name in new[]
            {
                CeresFlightArtworkFormat.FloorFileName,
                CeresFlightArtworkFormat.ObjectFileName,
            })
        {
            bool mode7 = name == CeresFlightArtworkFormat.FloorFileName;
            string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
            string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
            IndexedPngImage image;
            using (var input = File.OpenRead(stockPath))
                image = IndexedPng.Read(input,
                    new(checked((ushort)(mode7 ? CeresFlightArtworkFormat.FloorWidth : CeresFlightArtworkFormat.ObjectWidth)), checked((ushort)(mode7 ? CeresFlightArtworkFormat.FloorHeight : CeresFlightArtworkFormat.ObjectHeight))));
            Array.Fill(image.Pixels, (byte)0);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
            CeresFlightArtworkCatalog edited = installation.LoadIntroCinematicArt().CeresFlight;
            CeresFlightArtworkCatalog stockArtwork = IntroCinematicArtworkFiles.Load(installation.IntroCinematicDirectory, null).CeresFlight;
            var stockFlight = new IntroCeresFlightState(new AudioQueues(),
                stockArtwork, sheets: CinematicSheetFixtures.WithFlight(stockArtwork));
            var editedFlight = new IntroCeresFlightState(new AudioQueues(), edited, sheets: CinematicSheetFixtures.WithFlight(edited));
            for (int tick = 0; tick < CeresFrontApproachTicks; tick++)
            {
                stockFlight.StepFrame();
                editedFlight.StepFrame();
            }
            AssertEqual(IntroCeresFlightPhase.FlyingIntoCamera, stockFlight.Phase,
                "visible Ceres edit fixture reaches the front flight phase");
            AssertTrue(!DisplayRendering.Render(stockFlight.CaptureRenderSnapshot())
                    .SequenceEqual(RenderForComparison(
                        editedFlight.CaptureRenderSnapshot())),
                $"edited {name} changes visible Ceres approach pixels");
            File.Delete(overridePath);
        }

        string mapStock = Path.Combine(installation.IntroCinematicDirectory,
            CeresFlightArtworkFormat.MapFileName);
        string mapOverride = Path.Combine(installation.IntroCinematicOverrideDirectory,
            CeresFlightArtworkFormat.MapFileName);
        CeresFlightMapDocument map;
        using (var input = File.OpenRead(mapStock))
            map = JsonSerializer.Deserialize<CeresFlightMapDocument>(input,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Ceres Mode-7 map is empty.");
        Array.Fill(map.FrontTiles, 0);
        Array.Fill(map.RearTiles, 0);
        using (var output = File.Create(mapOverride))
            CeresFlightArtworkCatalog.WriteMap(output, map);
        CeresFlightArtworkCatalog originalArtwork = IntroCinematicArtworkFiles.Load(installation.IntroCinematicDirectory, null).CeresFlight;
        CeresFlightArtworkCatalog changedArtwork = installation.LoadIntroCinematicArt().CeresFlight;
        var original = new IntroCeresFlightState(new AudioQueues(),
            originalArtwork, sheets: CinematicSheetFixtures.WithFlight(originalArtwork));
        var changed = new IntroCeresFlightState(new AudioQueues(),
            changedArtwork, sheets: CinematicSheetFixtures.WithFlight(changedArtwork));
        for (int tick = 0; tick < CeresFrontApproachTicks; tick++)
        {
            original.StepFrame();
            changed.StepFrame();
        }
        AssertTrue(!DisplayRendering.Render(original.CaptureRenderSnapshot())
                .SequenceEqual(RenderForComparison(
                    changed.CaptureRenderSnapshot())),
            "edited front Ceres Mode-7 map changes visible approach pixels");
        for (int tick = 0; tick < 100 &&
            original.Phase != IntroCeresFlightPhase.FlyingTowardCeres; tick++)
        {
            original.StepFrame();
            changed.StepFrame();
        }
        AssertEqual(IntroCeresFlightPhase.FlyingTowardCeres, original.Phase,
            "visible Ceres map fixture reaches the rear view");
        // The handoff starts with a saturated white fixed-color flash. Allow it
        // to fade before comparing the rear map's actually visible pixels.
        for (int fadeTick = 0; fadeTick < 40; fadeTick++)
        {
            original.StepFrame();
            changed.StepFrame();
        }
        AssertTrue(!DisplayRendering.Render(original.CaptureRenderSnapshot())
                .SequenceEqual(RenderForComparison(
                    changed.CaptureRenderSnapshot())),
            "edited rear Ceres Mode-7 map changes visible approach pixels");
        File.Delete(mapOverride);
    }

    /// <summary>Constructs the exact front or rear VRAM image from selected map and character art.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native PPU geometry, native VRAM words")]
    private static byte[] ExpectedCeresFlightVram(CeresFlightArtworkCatalog artwork, bool rear)
    {
        var expected = new byte[SnesVram.ByteCount];
        for (int word = 0; word < AffineFloor.CellCount; word++)
        {
            expected[word * 2] = CeresFlightSceneDefinitions.BlankFloorCharacter;
            expected[word * 2 + 1] = artwork.FloorCharacters.Span[word];
        }
        int mapOffset = rear ? CeresFlightArtworkFormat.MapCellsPerView : 0;
        for (int word = 0; word < CeresFlightArtworkFormat.MapCellsPerView; word++)
            expected[word * 2] = artwork.FloorMaps.Span[mapOffset + word];
        artwork.ObjectCharacters.Span.CopyTo(expected.AsSpan(
            (CeresVramLayout.ObjectCharacters.Offset * 2)));
        return expected;
    }
}
