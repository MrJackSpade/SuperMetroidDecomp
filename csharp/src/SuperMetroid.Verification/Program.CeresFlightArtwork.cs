using SuperMetroid.Core.Audio;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Exercises the actual front/rear Mode-7 transfers and restored Ceres flight.</summary>
    /// <summary>
    /// Standalone frames from a new flight into its visible front approach: $8B:BCA0's
    /// dispatch and $8B:BDE4's 24-dispatch music wait precede the 32-dispatch approach
    /// (13% capture updates 6999-7055).
    /// </summary>
    private const int CeresFrontApproachTicks = 30;

    /// <summary>Verifies installed Ceres flight art against ROM assets, then exercises rendering, rebinding and override handling.</summary>
    /// <param name="installation">Game installation supplying stock artwork and user override paths.</param>
    /// <param name="bus">Cartridge address space used to extract native reference assets and guard migrated reads.</param>
    private static void VerifyCeresFlightArtwork(GameInstallation installation,
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        CeresFlightArtworkCatalog stock = installation.LoadIntroCinematicArt().CeresFlight;
        AssertTrue(stock.Mode7Characters.Span.SequenceEqual(RomDataReader.Decompress(bus,
                CeresFlightRomData.Assets.Mode7Characters,
                maximumOutputBytes: CeresFlightRomData.Vram.Mode7CharacterByteCount)),
            "installed Ceres Mode-7 PNG preserves every native character byte");
        AssertTrue(stock.ObjectCharacters.Span.SequenceEqual(RomDataReader.Decompress(bus,
                CeresFlightRomData.Assets.ObjectCharacters,
                maximumOutputBytes: CeresFlightRomData.Vram.ObjectCharacterByteCount)),
            "installed Ceres OBJ PNG preserves every native character byte");
        byte[] nativeMaps = RomDataReader.Decompress(bus, CeresFlightRomData.Assets.Mode7Maps,
            maximumOutputBytes: 0x1000);
        AssertTrue(stock.Mode7Maps.Span.SequenceEqual(nativeMaps.AsSpan(0,
                CeresFlightRomData.Vram.Mode7MapByteCount)),
            "installed front/rear Ceres maps preserve every consumed native byte");
        byte[] nativePalette = RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus),
            CeresFlightRomData.Assets.Palette, SnesCgram.ByteCount);
        for (int index = 0; index < SnesCgram.ColorCount; index++)
            AssertEqual(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(nativePalette.AsSpan(index * sizeof(ushort))),
                stock.Palette.ColorAt(index), "installed Ceres palette preserves every native color");

        var guard = new IntroArtworkSourceReadGuard(bus, blockCeresFlightSprites: true);
        var referenceFiles = CeresFlightArtworkExtractor.Extract(bus);
        using var referenceCharacters = new MemoryStream(referenceFiles[CeresFlightArtworkFormat.Mode7FileName]);
        using var referenceMaps = new MemoryStream(referenceFiles[CeresFlightArtworkFormat.MapFileName]);
        using var referenceObjects = new MemoryStream(referenceFiles[CeresFlightArtworkFormat.ObjectFileName]);
        using var referencePalette = new MemoryStream(referenceFiles[CeresFlightPaletteFormat.FileName]);
        using var referenceSprites = new MemoryStream(referenceFiles[CeresFlightSpriteFormat.FileName]);
        using var referenceActors = new MemoryStream(referenceFiles[CeresFlightActorLayoutFormat.FileName]);
        var native = new IntroCeresFlightState(bus, new CartridgeAudioState(), CeresFlightArtworkCatalog.Load(
            referenceCharacters, referenceMaps, referenceObjects, referencePalette,
            referenceSprites, referenceActors));
        var installed = new IntroCeresFlightState(guard, new CartridgeAudioState(), stock);
        var phases = new HashSet<IntroCeresFlightPhase>();
        for (int tick = 0; tick < 4000 && !native.Finished; tick++)
        {
            AssertEqual(native.Phase, installed.Phase,
                $"installed Ceres flight preserves native phase at tick {tick}");
            bool enteredPhase = phases.Add(native.Phase);
            if (enteredPhase || tick % 97 == 0)
            {
                LayeredRenderSnapshot original = native.CaptureRenderSnapshot();
                LayeredRenderSnapshot compiled = installed.CaptureRenderSnapshot();
                AssertTrue(compiled.Brightness == original.Brightness &&
                        compiled.Memory.Vram.SequenceEqual(original.Memory.Vram) &&
                        compiled.Memory.Cgram.SequenceEqual(original.Memory.Cgram) &&
                        SoftwareLayeredSnapshotRenderer.Render(compiled).AsSpan().SequenceEqual(
                            SoftwareLayeredSnapshotRenderer.Render(original)),
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

        Suite(nameof(VerifyCeresFlightSpriteArtwork), () => VerifyCeresFlightSpriteArtwork(installation, bus, stock, guard));
        Suite(nameof(VerifyCeresFlightActorLayout), () => VerifyCeresFlightActorLayout(installation, bus, stock, guard));

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string[] names =
        [
            CeresFlightArtworkFormat.Mode7FileName,
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
                        MapPresentationFormat.JsonOptions)
                        ?? throw new InvalidDataException("Ceres Mode-7 map is empty.");
                map.FrontTiles[0] = (map.FrontTiles[0] + 1) & 255;
                map.RearTiles[0] = (map.RearTiles[0] + 1) & 255;
                using var output = File.Create(overridePath);
                CeresFlightArtworkCatalog.WriteMap(output, map);
            }
            else
            {
                bool mode7 = name == CeresFlightArtworkFormat.Mode7FileName;
                IndexedPngImage image;
                using (var input = File.OpenRead(stockPath))
                    image = IndexedPng.Read(input,
                        mode7 ? CeresFlightArtworkFormat.Mode7Width : CeresFlightArtworkFormat.ObjectWidth,
                        mode7 ? CeresFlightArtworkFormat.Mode7Height : CeresFlightArtworkFormat.ObjectHeight);
                image.Pixels[0] = (byte)((image.Pixels[0] + 1) & (mode7 ? 255 : 15));
                using var output = File.Create(overridePath);
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            }

            CeresFlightArtworkCatalog edited = installation.LoadIntroCinematicArt().CeresFlight;
            bool isRearMap = name == CeresFlightArtworkFormat.MapFileName;
            var fresh = new IntroCeresFlightState(guard, new CartridgeAudioState(), edited);
            AssertTrue(fresh.CaptureRenderSnapshot().Memory.Vram.SequenceEqual(
                    ExpectedCeresFlightVram(edited, rear: false)),
                $"edited {name} reaches exact front-view VRAM without changing other bytes");
            var restored = new IntroCeresFlightState(guard, new CartridgeAudioState(), stock);
            for (int tick = 0; tick < 100 && restored.Phase != IntroCeresFlightPhase.FlyingTowardCeres; tick++)
                restored.StepFrame();
            AssertEqual(IntroCeresFlightPhase.FlyingTowardCeres, restored.Phase,
                "restored Ceres fixture reaches the rear-view map handoff");
            // The desktop/Android restore path rebinds the parent cinematic, not
            // this nested flight directly. Exercise that real handoff here.
            var parent = CreateRetailIntroFixture(guard,
                characterArtwork: IntroCinematicArtworkFiles.Load(
                    installation.IntroCinematicDirectory, null));
            typeof(IntroCinematicState).GetField("ceresFlight",
                BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(parent, restored);
            parent.BindCharacterArtwork(installation.LoadIntroCinematicArt());
            AssertTrue(restored.CaptureRenderSnapshot().Memory.Vram.SequenceEqual(
                    ExpectedCeresFlightVram(edited, rear: true)),
                $"restored rear view rebinds edited {name} with native map ordering");
            if (isRearMap)
                AssertTrue(!edited.Mode7Maps.Span.SequenceEqual(stock.Mode7Maps.Span),
                    "front and rear JSON edits both compile into the active map stream");
            File.Delete(overridePath);
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "edited Ceres artwork and debugger rebind never read cartridge art sources");
        Suite(nameof(VerifyCeresVisibleOverrides), () => VerifyCeresVisibleOverrides(installation, bus));
        Suite(nameof(VerifyCeresFlightPaletteOverride), () => VerifyCeresFlightPaletteOverride(installation, new IntroArtworkSourceReadGuard(bus), stock));

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
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Ceres Mode-7 map is empty.");
        persistentMap.FrontTiles[0] = (persistentMap.FrontTiles[0] + 1) & 255;
        using (var output = File.Create(invalidMap))
            CeresFlightArtworkCatalog.WriteMap(output, persistentMap);
        CeresFlightArtworkCatalog selected = installation.LoadIntroCinematicArt().CeresFlight;
        string stockMode7 = Path.Combine(installation.IntroCinematicDirectory,
            CeresFlightArtworkFormat.Mode7FileName);
        File.WriteAllBytes(stockMode7, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
            "a Ceres map override cannot hide a corrupt stock PNG");
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Ceres artwork vanished during stock repair.");
        AssertTrue(repaired.LoadIntroCinematicArt().CeresFlight.Mode7Maps.Span.SequenceEqual(
                selected.Mode7Maps.Span),
            "stock Ceres art repair preserves the player's external map override");
        File.Delete(invalidMap);
        Console.WriteLine("Ceres flight art: native front/rear frames and five ROM-sourced rear actor placements, independent PNG/JSON/palette edits, rebind and strict failures pass.");
    }

    /// <summary>Checks that editable rear-view actor placements retain native identities and affect the intended flight layout.</summary>
    /// <param name="installation">Installation containing the stock actor document and override directory.</param>
    /// <param name="bus">Cartridge source for the native placement reference data.</param>
    /// <param name="stock">Stock Ceres flight artwork used as the unedited comparison.</param>
    /// <param name="guardedBus">Address space that rejects reads of artwork already supplied by the catalog.</param>
    private static void VerifyCeresFlightActorLayout(GameInstallation installation,
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus, CeresFlightArtworkCatalog stock,
        ISnesAddressSpace guardedBus)
    {
        string name = CeresFlightActorLayoutFormat.FileName;
        string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        CeresFlightActorLayoutDocument document =
            JsonSerializer.Deserialize<CeresFlightActorLayoutDocument>(
                File.ReadAllBytes(stockPath), MapPresentationFormat.JsonOptions) ??
            throw new InvalidDataException("Stock Ceres flight actor layout is empty.");
        for (int index = 0; index < document.Actors.Length; index++)
        {
            var source = CeresFlightActorDefinitions.RearViewPlacementSource(index);
            AssertEqual(source.Id, document.Actors[index].Id,
                $"Ceres flight actor {index} retains its native role");
            AssertEqual((int)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresFlightActorDefinitions.NativeBank | source.XAddress),
                document.Actors[index].X,
                $"Ceres flight actor {index} X matches the cartridge operand");
            AssertEqual((int)RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    CeresFlightActorDefinitions.NativeBank | source.YAddress),
                document.Actors[index].Y,
                $"Ceres flight actor {index} Y matches the cartridge operand");
        }

        int stockAsteroidX = document.Actors[0].X;
        document.Actors[0] = document.Actors[0] with { X = stockAsteroidX + 16 };
        using (var output = File.Create(overridePath))
            CeresFlightActorLayout.Write(output, document);
        CeresFlightArtworkCatalog edited = installation.LoadIntroCinematicArt().CeresFlight;
        var original = new IntroCeresFlightState(guardedBus, new CartridgeAudioState(), stock);
        var changed = new IntroCeresFlightState(guardedBus, new CartridgeAudioState(), edited);
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
        var actorField = typeof(IntroCeresFlightState).GetField("rearViewActors",
            BindingFlags.Instance | BindingFlags.NonPublic) ??
            throw new InvalidOperationException("Ceres rear-view actor array is unavailable.");
        var nativeActors = (IntroDiscoverySprite[]?)actorField.GetValue(original) ??
            throw new InvalidOperationException("Stock Ceres rear-view actors did not spawn.");
        var editedActors = (IntroDiscoverySprite[]?)actorField.GetValue(changed) ??
            throw new InvalidOperationException("Edited Ceres rear-view actors did not spawn.");
        AssertEqual(unchecked((ushort)(nativeActors[0].XPosition + 16)),
            editedActors[0].XPosition,
            "editable layout moves the real rear-view asteroid actor by one tile");
        for (int frame = 0; frame < 20; frame++)
        {
            original.StepFrame();
            changed.StepFrame();
        }
        AssertTrue(!original.CaptureRenderSnapshot().Memory.Oam.SequenceEqual(
                changed.CaptureRenderSnapshot().Memory.Oam),
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

    /// <summary>Confirms palette overrides reach flight and destruction CGRAM, survive rebinding, and reject malformed data.</summary>
    /// <param name="installation">Installation supplying the stock palette and override file locations.</param>
    /// <param name="guardedBus">Address space that fails if the states reread migrated palette data from the cartridge.</param>
    /// <param name="stock">Unmodified Ceres flight catalog used to compare palette rendering and restore behavior.</param>
    private static void VerifyCeresFlightPaletteOverride(GameInstallation installation,
        ISnesAddressSpace guardedBus, CeresFlightArtworkCatalog stock)
    {
        string stockPath = Path.Combine(installation.IntroCinematicDirectory,
            CeresFlightPaletteFormat.FileName);
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory,
            CeresFlightPaletteFormat.FileName);
        CeresFlightPaletteDocument document = JsonSerializer.Deserialize<CeresFlightPaletteDocument>(
            File.ReadAllBytes(stockPath), MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock Ceres flight palette is empty.");
        document.Colors[0] = document.Colors[0] with
        {
            Red = (document.Colors[0].Red + 17) & 31,
        };
        using (var output = File.Create(overridePath))
            CeresFlightPalette.Write(output, document);

        CeresFlightArtworkCatalog edited = installation.LoadIntroCinematicArt().CeresFlight;
        var native = new IntroCeresFlightState(guardedBus, new CartridgeAudioState(), stock);
        var changed = new IntroCeresFlightState(guardedBus, new CartridgeAudioState(), edited);
        for (int tick = 0; tick < CeresFrontApproachTicks; tick++)
        {
            native.StepFrame();
            changed.StepFrame();
        }
        LayeredRenderSnapshot stockFrame = native.CaptureRenderSnapshot();
        LayeredRenderSnapshot editedFrame = changed.CaptureRenderSnapshot();
        AssertTrue(stockFrame.Memory.Vram.SequenceEqual(editedFrame.Memory.Vram),
            "Ceres palette edit does not change graphics VRAM");
        AssertTrue(!stockFrame.Memory.Cgram.SequenceEqual(editedFrame.Memory.Cgram),
            "Ceres palette edit reaches active CGRAM");
        AssertTrue(!SoftwareLayeredSnapshotRenderer.Render(stockFrame).AsSpan().SequenceEqual(
                SoftwareLayeredSnapshotRenderer.Render(editedFrame)),
            "Ceres palette edit changes visible approach pixels");

        var restored = new IntroCeresFlightState(guardedBus, new CartridgeAudioState(), stock);
        restored.BindArtwork(edited);
        AssertTrue(restored.CaptureRenderSnapshot().Memory.Cgram.SequenceEqual(
                new IntroCeresFlightState(guardedBus, new CartridgeAudioState(), edited).CaptureRenderSnapshot().Memory.Cgram),
            "restored Ceres flight rebinds current palette without restarting phase");

        IntroCinematicArtworkCatalog stockParent = IntroCinematicArtworkFiles.Load(
            installation.IntroCinematicDirectory, null);
        IntroCinematicArtworkCatalog editedParent = installation.LoadIntroCinematicArt();
        var restoredDestruction = new CeresDestructionCinematicState(guardedBus,
            artwork: stockParent);
        var editedDestruction = new CeresDestructionCinematicState(guardedBus,
            artwork: editedParent);
        AssertTrue(!restoredDestruction.CaptureRenderSnapshot().Memory.Cgram.SequenceEqual(
                editedDestruction.CaptureRenderSnapshot().Memory.Cgram),
            "shared Ceres palette edit reaches destruction CGRAM");
        restoredDestruction.BindArtwork(editedParent);
        AssertTrue(restoredDestruction.CaptureRenderSnapshot().Memory.Cgram.SequenceEqual(
                editedDestruction.CaptureRenderSnapshot().Memory.Cgram),
            "restored destruction rebinds its shared palette before engine FX owns CGRAM");
        File.Delete(overridePath);

        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
            "malformed Ceres flight palette fails instead of using stock silently");
        File.Delete(overridePath);
    }

    /// <summary>Verifies edited Mode-7 maps and character images change visible front or rear flight pixels.</summary>
    /// <param name="installation">Installation whose stock and override artwork files are exercised.</param>
    /// <param name="bus">Cartridge address space for constructing native-art comparison states.</param>
    private static void VerifyCeresVisibleOverrides(GameInstallation installation,
        SuperMetroid.AssetExtraction.CartridgeImportAddressSpace bus)
    {
        foreach (string name in new[]
            {
                CeresFlightArtworkFormat.Mode7FileName,
                CeresFlightArtworkFormat.ObjectFileName,
            })
        {
            bool mode7 = name == CeresFlightArtworkFormat.Mode7FileName;
            string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
            string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
            IndexedPngImage image;
            using (var input = File.OpenRead(stockPath))
                image = IndexedPng.Read(input,
                    mode7 ? CeresFlightArtworkFormat.Mode7Width : CeresFlightArtworkFormat.ObjectWidth,
                    mode7 ? CeresFlightArtworkFormat.Mode7Height : CeresFlightArtworkFormat.ObjectHeight);
            Array.Fill(image.Pixels, (byte)0);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            CeresFlightArtworkCatalog edited = installation.LoadIntroCinematicArt().CeresFlight;
            var stockFlight = new IntroCeresFlightState(bus, new CartridgeAudioState(),
                IntroCinematicArtworkFiles.Load(installation.IntroCinematicDirectory, null).CeresFlight);
            var editedFlight = new IntroCeresFlightState(bus, new CartridgeAudioState(), edited);
            for (int tick = 0; tick < CeresFrontApproachTicks; tick++)
            {
                stockFlight.StepFrame();
                editedFlight.StepFrame();
            }
            AssertEqual(IntroCeresFlightPhase.FlyingIntoCamera, stockFlight.Phase,
                "visible Ceres edit fixture reaches the front flight phase");
            AssertTrue(!SoftwareLayeredSnapshotRenderer.Render(stockFlight.CaptureRenderSnapshot())
                    .AsSpan().SequenceEqual(RenderForComparison(
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
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Ceres Mode-7 map is empty.");
        Array.Fill(map.FrontTiles, 0);
        Array.Fill(map.RearTiles, 0);
        using (var output = File.Create(mapOverride))
            CeresFlightArtworkCatalog.WriteMap(output, map);
        var original = new IntroCeresFlightState(bus, new CartridgeAudioState(),
            IntroCinematicArtworkFiles.Load(installation.IntroCinematicDirectory, null).CeresFlight);
        var changed = new IntroCeresFlightState(bus, new CartridgeAudioState(),
            installation.LoadIntroCinematicArt().CeresFlight);
        for (int tick = 0; tick < CeresFrontApproachTicks; tick++)
        {
            original.StepFrame();
            changed.StepFrame();
        }
        AssertTrue(!SoftwareLayeredSnapshotRenderer.Render(original.CaptureRenderSnapshot())
                .AsSpan().SequenceEqual(RenderForComparison(
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
        AssertTrue(!SoftwareLayeredSnapshotRenderer.Render(original.CaptureRenderSnapshot())
                .AsSpan().SequenceEqual(RenderForComparison(
                    changed.CaptureRenderSnapshot())),
            "edited rear Ceres Mode-7 map changes visible approach pixels");
        File.Delete(mapOverride);
    }

    /// <summary>Builds the VRAM image expected after the flight uploads its character data and selected map view.</summary>
    /// <param name="artwork">Compiled Ceres characters, object graphics and front/rear map bytes.</param>
    /// <param name="rear"><see langword="true"/> to place the rear map slice; otherwise uses the front slice.</param>
    /// <returns>Complete expected VRAM contents, including Mode-7 tile/map interleaving and object graphics.</returns>
    private static byte[] ExpectedCeresFlightVram(CeresFlightArtworkCatalog artwork, bool rear)
    {
        var expected = new byte[SnesVram.ByteCount];
        for (int word = 0; word < CeresFlightRomData.Vram.Mode7MapFillWordCount; word++)
        {
            expected[word * 2] = CeresFlightRomData.Vram.Mode7BlankMapTile;
            expected[word * 2 + 1] = artwork.Mode7Characters.Span[word];
        }
        int mapOffset = rear ? CeresFlightRomData.Vram.Mode7MapSliceByteCount : 0;
        for (int word = 0; word < CeresFlightRomData.Vram.Mode7MapSliceByteCount; word++)
            expected[word * 2] = artwork.Mode7Maps.Span[mapOffset + word];
        artwork.ObjectCharacters.Span.CopyTo(expected.AsSpan(
            CeresFlightRomData.Vram.ObjectCharacterDestinationByte));
        return expected;
    }
}
