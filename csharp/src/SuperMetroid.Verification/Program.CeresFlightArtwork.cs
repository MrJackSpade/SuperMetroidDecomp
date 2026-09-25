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
    private static void VerifyCeresFlightArtwork(GameInstallation installation,
        SuperMetroidAddressSpace bus)
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
        AssertTrue(stock.Palette.Transfer.Span.SequenceEqual(RomDataReader.ReadFixedBank(bus,
                CeresFlightRomData.Assets.Palette, SnesCgram.ByteCount)),
            "installed Ceres palette preserves all 256 native colors");

        var guard = new IntroArtworkSourceReadGuard(bus);
        var native = new IntroCeresFlightState(bus);
        var installed = new IntroCeresFlightState(guard, stock);
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
            native.Step();
            installed.Step();
        }
        AssertTrue(native.Finished && installed.Finished &&
                phases.Contains(IntroCeresFlightPhase.FlyingTowardCeres) &&
                phases.Contains(IntroCeresFlightPhase.SpaceColonyTitle),
            "stock Ceres PNG/JSON art preserves front, rear and SPACE COLONY phases");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed flight never reads its cartridge art or palette sources");

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
            var fresh = new IntroCeresFlightState(guard, edited);
            AssertTrue(fresh.CaptureRenderSnapshot().Memory.Vram.SequenceEqual(
                    ExpectedCeresFlightVram(edited, rear: false)),
                $"edited {name} reaches exact front-view VRAM without changing other bytes");
            var restored = new IntroCeresFlightState(guard, stock);
            for (int tick = 0; tick < 100 && restored.Phase != IntroCeresFlightPhase.FlyingTowardCeres; tick++)
                restored.Step();
            AssertEqual(IntroCeresFlightPhase.FlyingTowardCeres, restored.Phase,
                "restored Ceres fixture reaches the rear-view map handoff");
            // The desktop/Android restore path rebinds the parent cinematic, not
            // this nested flight directly. Exercise that real handoff here.
            var parent = new IntroCinematicState(guard,
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
        VerifyCeresVisibleOverrides(installation, bus);
        VerifyCeresFlightPaletteOverride(installation, guard, stock);

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
        Console.WriteLine("Ceres flight art: native front/rear frames, independent PNG/JSON/palette edits, rebind and strict failures pass.");
    }

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
        var native = new IntroCeresFlightState(guardedBus, stock);
        var changed = new IntroCeresFlightState(guardedBus, edited);
        for (int tick = 0; tick < 20; tick++)
        {
            native.Step();
            changed.Step();
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

        var restored = new IntroCeresFlightState(guardedBus, stock);
        restored.BindArtwork(edited);
        AssertTrue(restored.CaptureRenderSnapshot().Memory.Cgram.SequenceEqual(
                new IntroCeresFlightState(guardedBus, edited).CaptureRenderSnapshot().Memory.Cgram),
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

    private static void VerifyCeresVisibleOverrides(GameInstallation installation,
        SuperMetroidAddressSpace bus)
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
            var stockFlight = new IntroCeresFlightState(bus,
                IntroCinematicArtworkFiles.Load(installation.IntroCinematicDirectory, null).CeresFlight);
            var editedFlight = new IntroCeresFlightState(bus, edited);
            for (int tick = 0; tick < 20; tick++)
            {
                stockFlight.Step();
                editedFlight.Step();
            }
            AssertEqual(IntroCeresFlightPhase.FlyingIntoCamera, stockFlight.Phase,
                "visible Ceres edit fixture reaches the front flight phase");
            AssertTrue(!SoftwareLayeredSnapshotRenderer.Render(stockFlight.CaptureRenderSnapshot())
                    .AsSpan().SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(
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
        var original = new IntroCeresFlightState(bus,
            IntroCinematicArtworkFiles.Load(installation.IntroCinematicDirectory, null).CeresFlight);
        var changed = new IntroCeresFlightState(bus,
            installation.LoadIntroCinematicArt().CeresFlight);
        for (int tick = 0; tick < 20; tick++)
        {
            original.Step();
            changed.Step();
        }
        AssertTrue(!SoftwareLayeredSnapshotRenderer.Render(original.CaptureRenderSnapshot())
                .AsSpan().SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(
                    changed.CaptureRenderSnapshot())),
            "edited front Ceres Mode-7 map changes visible approach pixels");
        for (int tick = 0; tick < 100 &&
            original.Phase != IntroCeresFlightPhase.FlyingTowardCeres; tick++)
        {
            original.Step();
            changed.Step();
        }
        AssertEqual(IntroCeresFlightPhase.FlyingTowardCeres, original.Phase,
            "visible Ceres map fixture reaches the rear view");
        // The handoff starts with a saturated white fixed-color flash. Allow it
        // to fade before comparing the rear map's actually visible pixels.
        for (int fadeTick = 0; fadeTick < 40; fadeTick++)
        {
            original.Step();
            changed.Step();
        }
        AssertTrue(!SoftwareLayeredSnapshotRenderer.Render(original.CaptureRenderSnapshot())
                .AsSpan().SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(
                    changed.CaptureRenderSnapshot())),
            "edited rear Ceres Mode-7 map changes visible approach pixels");
        File.Delete(mapOverride);
    }

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
