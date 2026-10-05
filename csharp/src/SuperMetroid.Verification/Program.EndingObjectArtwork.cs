using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingObjectFragmentMetadata(ISnesAddressSpace bus)
    {
        string[] names = ["ending-explosion-fragment-70.png", "ending-explosion-fragment-74.png",
            "ending-explosion-fragment-78.png", "ending-explosion-fragment-7c.png"];
        AssertEqual(names.Length, EndingObjectArtworkFormat.FragmentCount, "ending fragment count");
        for (int index = 0; index < names.Length; index++)
        {
            int load = 0x8bd5af + 17 * index;
            AssertEqual((byte)0xa9, bus.ReadByte(load), "native fragment bank LDA");
            AssertEqual((byte)0xa9, bus.ReadByte(load + 5), "native fragment address LDA");
            int source = bus.ReadByte(load + 2) << 16 | bus.ReadByte(load + 6) | bus.ReadByte(load + 7) << 8;
            AssertEqual(source, EndingCreditsRomData.Assets.ObjectFragmentSource((EndingObjectFragmentId)index), "native fragment compressed source");
            AssertEqual(names[index], EndingObjectArtworkFormat.FragmentFileName(index), "published fragment filename");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 4, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => EndingObjectArtworkFormat.FragmentFileName(invalid), "fragment filename bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => EndingCreditsRomData.Assets.ObjectFragmentSource((EndingObjectFragmentId)invalid), "fragment source bounds");
        }
    }

    private static void VerifyEndingCloudDefinitions(ISnesAddressSpace bus)
    {
        for (int pointer = EndingCloudInstructionDefinitions.Start;
             pointer < EndingCloudInstructionDefinitions.End; pointer += sizeof(ushort))
        {
            int address = (int)new SnesAddress(0x8b, (ushort)pointer);
            ushort nativeWord = (ushort)(bus.ReadByte(address) |
                bus.ReadByte(address + 1) << 8);
            AssertEqual(nativeWord, EndingCloudInstructionDefinitions.ReadWord((ushort)pointer),
                $"ending cloud instruction $8B:{pointer:X4} matches cartridge");
        }
        AssertThrows<InvalidDataException>(() =>
            EndingCloudInstructionDefinitions.ReadWord(
                EndingCloudInstructionDefinitions.End),
            "ending cloud reader cannot escape its six lists");
        string[] names = ["scene-b-upper-a", "scene-b-upper-b", "scene-b-lower-a", "scene-b-lower-b", "scene-a-right", "scene-a-left"];
        var catalog = EndingCloudSpriteDefinitions.Frames;
        AssertEqual(6, catalog.Count, "cloud catalog count");
        for (int index = 0; index < catalog.Count; index++)
        {
            int operand = 0x8becef + index * 8;
            ushort map = (ushort)(bus.ReadByte(operand) | bus.ReadByte(operand + 1) << 8);
            int header = 0x8c0000 | map;
            int count = bus.ReadByte(header) | bus.ReadByte(header + 1) << 8;
            AssertEqual(map, catalog[index].Pointer, "cloud native pointer operand");
            AssertEqual(count, catalog[index].StockPartCount, "cloud native OAM count");
            AssertEqual(names[index], catalog[index].Name, "cloud published asset key");
            SpriteVisualPart[] visual = IntroCinematicSpriteFrameExtractor.Extract(bus, map, count, names[index]);
            SpriteComposition supplied = IntroCinematicSpriteCompiler.Compile(visual, names[index]);
            var grid = new EndingCloudGridParts((EndingCloudSpriteDefinitions.Role)index);
            SpriteComposition calculated = supplied.CalculateIfMatching(grid);
            AssertTrue(!ReferenceEquals(supplied, calculated), "all original cloud parts use calculated grid");
            string Identity(SpriteComposition value) => SelectedPresentationHash.Create("cloud", value.AppendIdentity);
            AssertEqual(Identity(supplied), Identity(calculated), "cloud grid complete visual identity");
            foreach (ushort y in new ushort[] { 72, 0xfff8 })
            {
                var originalOam = new OamBuffer();
                var calculatedOam = new OamBuffer();
                originalOam.BeginFrame(); calculatedOam.BeginFrame();
                DrawImportedSpritemap(bus, originalOam, header, 120, y, 0x0800, originIsOnScreen: y == 72);
                if (y == 72) calculated.DrawOnScreen(calculatedOam, 120, y, 0x0800);
                else calculated.DrawOffScreen(calculatedOam, 120, y, 0x0800);
                originalOam.FinalizeFrame(); calculatedOam.FinalizeFrame();
                AssertTrue(originalOam.LowTable.SequenceEqual(calculatedOam.LowTable) && originalOam.HighTable.SequenceEqual(calculatedOam.HighTable),
                    "cloud grid preserves native OAM and clipping");
            }
            SpriteVisualPart first = visual[0];
            foreach (SpriteVisualPart edit in new[]
            {
                first with { OffsetX = first.OffsetX + 1 }, first with { OffsetY = first.OffsetY + 1 },
                first with { TileColumn = first.TileColumn + 1 }, first with { TileRow = first.TileRow + 1 },
                first with { Size = 16 }, first with { Priority = 2 }, first with { Palette = 3 },
                first with { FlipX = !first.FlipX }, first with { FlipY = !first.FlipY },
            })
            {
                visual[0] = edit;
                SpriteComposition edited = IntroCinematicSpriteCompiler.Compile(visual, "edited cloud");
                AssertTrue(ReferenceEquals(edited, edited.CalculateIfMatching(grid)), "independent cloud edits stay supplied");
            }
            foreach (int invalid in new[] { int.MinValue, -1, count, int.MaxValue })
                AssertThrows<ArgumentOutOfRangeException>(() => _ = grid[invalid], "cloud grid part bounds");
            ushort list = unchecked((ushort)(EndingCloudInstructionDefinitions.Start + index * 8));
            var nativeActor = new IntroDiscoverySprite(120, 72, 0x0800, list);
            var installedActor = new IntroDiscoverySprite(120, 72, 0x0800, list);
            for (int frame = 0; frame < 120; frame++)
            {
                nativeActor.Step(bus, instructionWord: pointer => (ushort)(
                    bus.ReadByte(0x8b0000 | pointer) | bus.ReadByte(0x8b0000 | (pointer + 1)) << 8));
                installedActor.Step(bus, instructionWord: EndingCloudInstructionDefinitions.ReadWord);
                AssertEqual(nativeActor.InstructionPointer, installedActor.InstructionPointer,
                    $"ending cloud {index} list cursor at frame {frame}");
                AssertEqual(nativeActor.SpriteMapPointer, installedActor.SpriteMapPointer,
                    $"ending cloud {index} selected visual frame at frame {frame}");
            }
        }
        AssertTrue(catalog.Select(frame => frame.Name).SequenceEqual(names), "cloud catalog enumeration order");
        foreach (int invalid in new[] { int.MinValue, -1, 6, int.MaxValue })
        {
            AssertThrows<ArgumentOutOfRangeException>(() => _ = catalog[invalid], "cloud catalog bounds");
            AssertThrows<ArgumentOutOfRangeException>(() => _ = new EndingCloudGridParts((EndingCloudSpriteDefinitions.Role)invalid), "cloud grid role bounds");
        }
        foreach (ushort invalid in new ushort[] { 0, 0xecec, 0xecee, 0xed1d, 0xffff })
            AssertThrows<InvalidDataException>(() => EndingCloudInstructionDefinitions.ReadWord(invalid), "cloud program bounds");
    }

    private static void VerifyEndingObjectArtwork(GameInstallation installation)
    {
        EndingObjectArtworkCatalog stock = installation.LoadEndingObjectArt();
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        VerifyEndingObjectFragmentMetadata(bus);
        AssertSheet(stock.Clouds, EndingCreditsRomData.Assets.EscapeCloudCharacters,
            EndingObjectArtworkFormat.CloudByteCount, "clouds");
        AssertSheet(stock.Explosion, EndingCreditsRomData.Assets.EndingObjectCharacters,
            EndingObjectArtworkFormat.ExplosionByteCount, "explosion");
        for (int index = 0; index < EndingObjectArtworkFormat.FragmentCount; index++)
            AssertSheet(stock.Fragment((EndingObjectFragmentId)index),
                EndingCreditsRomData.Assets.ObjectFragmentSource((EndingObjectFragmentId)index), EndingObjectArtworkFormat.FragmentByteCount,
                $"explosion fragment {index}");
        AssertSheet(stock.WaitingSamus,
            EndingCreditsRomData.Assets.WaitingForCreditsCharacters,
            EndingObjectArtworkFormat.RewardByteCount, "waiting Samus");
        AssertSheet(stock.ShootingScreen,
            EndingCreditsRomData.Assets.ShootingScreenCharacters,
            EndingObjectArtworkFormat.RewardByteCount, "shooting screen");
        AssertSheet(stock.SuitlessSamus,
            EndingCreditsRomData.Assets.SuitlessSamusCharacters,
            EndingObjectArtworkFormat.RewardByteCount, "suitless Samus");
        byte[] nativeWaitingMap = RomDataReader.Decompress(bus,
            EndingCreditsRomData.Assets.WaitingForCreditsTilemap,
            EndingCreditsRomData.Rendering.ObjectFragmentLimit);
        AssertTrue(stock.WaitingTilemap.Transfer.Span.SequenceEqual(
                nativeWaitingMap.AsSpan(0, EndingObjectArtworkFormat.WaitingTilemapByteCount)),
            "installed waiting-Samus map preserves all native BG2 tile words");
        AssertSheet(stock.PostCreditsFragmentA,
            EndingCreditsRomData.Assets.PostCreditsTileFragmentA,
            EndingObjectArtworkFormat.PostCreditsFragmentAByteCount,
            "post-credits fragment A");
        AssertSheet(stock.PostCreditsFragmentB,
            EndingCreditsRomData.Assets.PostCreditsTileFragmentB,
            EndingObjectArtworkFormat.PostCreditsFragmentBByteCount,
            "post-credits fragment B");
        AssertSheet(stock.PostShotLogoTiles, EndingPostShotDefinitions.LogoTiles,
            EndingObjectArtworkFormat.PostShotLogoTileByteCount,
            "post-shot logo tiles");
        byte[] nativeLogoMap = RomDataReader.Decompress(bus,
            EndingPostShotDefinitions.LogoMap,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        AssertTrue(stock.PostShotLogoMap.Transfer.Span.SequenceEqual(
                nativeLogoMap.AsSpan(0, EndingObjectArtworkFormat.PostShotLogoMapByteCount)),
            "installed post-shot logo map preserves all native BG2 tile words");

        VerifyEndingCloudDefinitions(bus);
        for (int index = 0; index < EndingCloudSpriteDefinitions.Frames.Count; index++)
        {
            EndingCloudSpriteFrameDefinition definition = EndingCloudSpriteDefinitions.Frames[index];
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    (y & CinematicSpriteDrawDefinitions.OriginYHighByteMask) == 0;
                int source = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, definition.Pointer);
                var nativeOam = new OamBuffer();
                nativeOam.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, nativeOam, source, 120, y, 0x0800);
                else
                    DrawImportedSpritemap(bus, nativeOam, source, 120, y, 0x0800, originIsOnScreen: false);
                nativeOam.FinalizeFrame();
                var installedOam = new OamBuffer();
                installedOam.BeginFrame();
                stock.CloudSprites.Draw(definition.Pointer, installedOam,
                    120, y, 0x0800, onScreen);
                installedOam.FinalizeFrame();
                AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                        nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
                        nativeOam.LastFinalizedSpriteCount == installedOam.LastFinalizedSpriteCount,
                    $"ending cloud {definition.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }
        VerifyEndingExplosionActorArtwork(installation, bus, stock);
        VerifyEndingCompletionTextInstructions(bus);
        VerifyEndingCompletionTextSpriteArtwork(bus, stock);
        VerifyEndingRewardInstructions(bus);
        VerifyEndingRewardSpriteArtwork(bus, stock);
        VerifyEndingLogoSpriteArtwork(bus, stock);

        var guard = new EndingObjectSourceReadGuard(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc"));
        var nativeAudio = new CartridgeAudioState();
        var installedAudio = new CartridgeAudioState();
        var native = new EndingCreditsState(bus, nativeAudio, 0, 0);
        var installed = new EndingCreditsState(guard, installedAudio, 0, 0);
        installed.BindObjectArtwork(stock);
        var phases = new HashSet<EndingCreditsPhase>();
        for (int frame = 0; frame < 12000 &&
            native.Phase != EndingCreditsPhase.FadeOutToCredits; frame++)
        {
            AssertEqual(native.Phase, installed.Phase,
                $"installed ending OBJ art preserves phase at frame {frame}");
            if (phases.Add(native.Phase) || frame % 211 == 0)
            {
                LayeredRenderSnapshot expected = native.CaptureRenderSnapshot();
                LayeredRenderSnapshot actual = installed.CaptureRenderSnapshot();
                AssertTrue(actual.Memory.Vram.SequenceEqual(expected.Memory.Vram) &&
                        actual.Memory.Cgram.SequenceEqual(expected.Memory.Cgram) &&
                        SoftwareLayeredSnapshotRenderer.Render(actual).AsSpan().SequenceEqual(
                            SoftwareLayeredSnapshotRenderer.Render(expected)),
                    $"ending OBJ PNGs preserve native VRAM/pixels at frame {frame}");
            }
            native.Step();
            installed.Step();
            nativeAudio.AdvanceFrame(bus, default);
            installedAudio.AdvanceFrame(guard, default);
        }
        AssertEqual(EndingCreditsPhase.FadeOutToCredits, native.Phase,
            "ending OBJ fixture reaches the completed-time credits handoff");
        AssertEqual(EndingCreditsPhase.FadeOutToCredits, installed.Phase,
            "installed OBJ art retains the native completed-time credits handoff");
        AssertTrue(phases.Contains(EndingCreditsPhase.WaitForEscapeMusic) &&
                phases.Contains(EndingCreditsPhase.FadeInEscapeSceneB) &&
                phases.Contains(EndingCreditsPhase.FadeInZebesExplosion) &&
                phases.Contains(EndingCreditsPhase.PlanetEscapeFast) &&
                phases.Contains(EndingCreditsPhase.OperationSuccessfulText),
            "ending clouds, explosion and completion text were exercised in their actual scenes");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "installed ending OBJ scenes never reread compiled instructions, sprite maps or character sources");

        VerifyEndingExplosionVisualOverride(installation, stock, guard);
        VerifyEndingCompletionTextVisualOverride(installation, stock, guard);
        VerifyEndingRewardVisualOverride(installation, stock, guard);

        string cloudSpriteName = EndingCloudSpriteFormat.FileName;
        Directory.CreateDirectory(installation.EndingObjectOverrideDirectory);
        string cloudSpritePath = Path.Combine(installation.EndingObjectDirectory, cloudSpriteName);
        string cloudSpriteOverride = Path.Combine(
            installation.EndingObjectOverrideDirectory, cloudSpriteName);
        EndingCloudSpriteDocument cloudDocument =
            JsonSerializer.Deserialize<EndingCloudSpriteDocument>(
                File.ReadAllBytes(cloudSpritePath), MapPresentationFormat.JsonOptions) ??
            throw new InvalidDataException("Stock ending cloud sprite JSON is empty.");
        cloudDocument.Frames["scene-a-right"] = cloudDocument.Frames["scene-a-right"]
            .Select(part => part with { OffsetX = part.OffsetX + 8 }).ToArray();
        using (var output = File.Create(cloudSpriteOverride))
            EndingCloudSpritePresentation.Write(output, cloudDocument);
        byte[] editedCloudSpriteBytes = File.ReadAllBytes(cloudSpriteOverride);
        EndingObjectArtworkCatalog editedCloudSprites = installation.LoadEndingObjectArt();
        var cloudAudio = new CartridgeAudioState();
        var cloudScene = new EndingCreditsState(guard, cloudAudio, 0, 0);
        cloudScene.BindObjectArtwork(stock);
        for (int frame = 0; frame < 2000 &&
            cloudScene.Phase != EndingCreditsPhase.EscapeSceneA; frame++)
        {
            cloudScene.Step();
            cloudAudio.AdvanceFrame(guard, default);
        }
        AssertEqual(EndingCreditsPhase.EscapeSceneA, cloudScene.Phase,
            "ending cloud edit reaches visible first atmospheric scene");
        bool visibleCloudEdit = false;
        for (int frame = 0; frame < 700 &&
            cloudScene.Phase < EndingCreditsPhase.FadeInEscapeSceneB &&
            !visibleCloudEdit; frame++)
        {
            cloudScene.BindObjectArtwork(stock);
            LayeredRenderSnapshot beforeCloudEdit = cloudScene.CaptureRenderSnapshot();
            cloudScene.BindObjectArtwork(editedCloudSprites);
            LayeredRenderSnapshot afterCloudEdit = cloudScene.CaptureRenderSnapshot();
            visibleCloudEdit = !beforeCloudEdit.Memory.Oam.SequenceEqual(
                    afterCloudEdit.Memory.Oam) &&
                !SoftwareLayeredSnapshotRenderer.Render(beforeCloudEdit).AsSpan().SequenceEqual(
                    SoftwareLayeredSnapshotRenderer.Render(afterCloudEdit));
            if (!visibleCloudEdit)
            {
                cloudScene.Step();
                cloudAudio.AdvanceFrame(guard, default);
            }
        }
        AssertTrue(visibleCloudEdit,
            "edited cloud composition changes live OAM and visible ending pixels");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "cloud art rebind never rereads installed bank-$8C sprite maps");
        cloudDocument.Frames.Remove("scene-a-right");
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            EndingCloudSpritePresentation.Write(invalid, cloudDocument);
        }, "ending cloud sprites reject missing named art");
        File.Delete(cloudSpriteOverride);

        Directory.CreateDirectory(installation.EndingObjectOverrideDirectory);
        string[] names =
        [
            EndingObjectArtworkFormat.CloudFileName,
            EndingObjectArtworkFormat.ExplosionFileName,
            .. Enumerable.Range(0, EndingObjectArtworkFormat.FragmentCount)
                .Select(EndingObjectArtworkFormat.FragmentFileName),
        ];
        for (int resource = 0; resource < names.Length; resource++)
        {
            string name = names[resource];
            int byteCount = resource switch
            {
                0 => EndingObjectArtworkFormat.CloudByteCount,
                1 => EndingObjectArtworkFormat.ExplosionByteCount,
                _ => EndingObjectArtworkFormat.FragmentByteCount,
            };
            var sceneBus = new EndingObjectSourceReadGuard(
                SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc"));
            var audio = new CartridgeAudioState();
            var scene = new EndingCreditsState(sceneBus, audio, 0, 0);
            scene.BindObjectArtwork(stock);
            EndingCreditsPhase target = resource == 0
                ? EndingCreditsPhase.WaitForEscapeMusic :
                EndingCreditsPhase.FadeInZebesExplosion;
            for (int frame = 0; frame < 10000 && scene.Phase != target; frame++)
            {
                scene.Step();
                audio.AdvanceFrame(sceneBus, default);
            }
            AssertEqual(target, scene.Phase, $"ending {name} reaches its actual scene");
            for (int frame = 0; frame < 1200 && scene.Brightness == 0; frame++)
            {
                scene.Step();
                audio.AdvanceFrame(sceneBus, default);
            }
            LayeredRenderSnapshot baseline = scene.CaptureRenderSnapshot();
            EndingCreditsPhase baselinePhase = scene.Phase;
            AssertTrue(baseline.Brightness > 0,
                $"ending {name} reaches a visible override frame");

            int tiles = byteCount / RoomCharacterAtlasFormat.BytesPerTile;
            int rows = tiles / RoomCharacterAtlasFormat.TileColumns;
            string overridePath = Path.Combine(installation.EndingObjectOverrideDirectory, name);
            IndexedPngImage image;
            using (var input = File.OpenRead(Path.Combine(installation.EndingObjectDirectory, name)))
                image = IndexedPng.Read(input,
                    RoomCharacterAtlasFormat.TileColumns * 8, rows * 8);
            Array.Fill(image.Pixels, (byte)0);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            EndingObjectArtworkCatalog edited = installation.LoadEndingObjectArt();
            scene.BindObjectArtwork(edited);
            LayeredRenderSnapshot changed = scene.CaptureRenderSnapshot();
            AssertTrue(!baseline.Memory.Vram.SequenceEqual(changed.Memory.Vram) &&
                    baseline.Memory.Vram[..EndingCreditsRomData.Rendering.ObjectCharactersDestination]
                        .SequenceEqual(changed.Memory.Vram
                            [..EndingCreditsRomData.Rendering.ObjectCharactersDestination]) &&
                    baseline.Memory.Cgram.SequenceEqual(changed.Memory.Cgram) &&
                    scene.Phase == baselinePhase,
                $"edited {name} changes OBJ VRAM without touching Mode-7 graphics or CGRAM");
            bool visibleEdit = false;
            for (int frame = 0; frame < 2400 && !visibleEdit &&
                scene.Phase < (resource == 0
                    ? EndingCreditsPhase.FadeInZebesExplosion :
                    EndingCreditsPhase.PlanetEscapeFast); frame++)
            {
                if (frame % 6 == 0)
                {
                    scene.BindObjectArtwork(stock);
                    LayeredRenderSnapshot nativeArt = scene.CaptureRenderSnapshot();
                    scene.BindObjectArtwork(edited);
                    LayeredRenderSnapshot editedArt = scene.CaptureRenderSnapshot();
                    visibleEdit = !SoftwareLayeredSnapshotRenderer.Render(nativeArt)
                        .AsSpan().SequenceEqual(
                            SoftwareLayeredSnapshotRenderer.Render(editedArt));
                }
                scene.Step();
                audio.AdvanceFrame(sceneBus, default);
            }
            AssertTrue(visibleEdit, $"edited {name} changes visible ending pixels");
            File.Delete(overridePath);
            AssertEqual(0, sceneBus.ForbiddenReadAttempts,
                $"edited/rebound {name} does not reread cartridge OBJ sources");
        }

        string invalidPath = Path.Combine(installation.EndingObjectOverrideDirectory,
            EndingObjectArtworkFormat.CloudFileName);
        File.WriteAllBytes(invalidPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadEndingObjectArt(),
            "malformed ending OBJ override fails loudly");
        File.Delete(invalidPath);
        IndexedPngImage persistentClouds;
        using (var input = File.OpenRead(Path.Combine(installation.EndingObjectDirectory,
                   EndingObjectArtworkFormat.CloudFileName)))
            persistentClouds = IndexedPng.Read(input, 256, 128);
        persistentClouds.Pixels[0] = (byte)((persistentClouds.Pixels[0] + 1) & 15);
        using (var output = File.Create(invalidPath))
            IndexedPng.Write(output, persistentClouds.Width, persistentClouds.Height,
                persistentClouds.Pixels, persistentClouds.Palette);
        byte[] editedClouds = installation.LoadEndingObjectArt().Clouds.Transfer.ToArray();
        string stockExplosion = Path.Combine(installation.EndingObjectDirectory,
            EndingObjectArtworkFormat.ExplosionFileName);
        File.WriteAllBytes(stockExplosion, [0]);
        File.WriteAllBytes(cloudSpriteOverride, editedCloudSpriteBytes);
        File.WriteAllBytes(cloudSpritePath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadEndingObjectArt(),
            "valid ending OBJ overrides cannot hide corrupt stock art or sprite JSON");
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Ending OBJ content vanished during stock repair.");
        AssertTrue(repaired.LoadEndingObjectArt().Clouds.Transfer.Span.SequenceEqual(editedClouds) &&
                repaired.LoadEndingObjectArt().Explosion.Transfer.Span.SequenceEqual(
                    stock.Explosion.Transfer.Span),
            "stock OBJ repair restores native pixels and preserves the player's cloud override");
        var preservedCloudOam = new OamBuffer();
        preservedCloudOam.BeginFrame();
        repaired.LoadEndingObjectArt().CloudSprites.Draw(0xb83b, preservedCloudOam,
            120, 72, 0x0800, originIsOnScreen: true);
        preservedCloudOam.FinalizeFrame();
        var editedCloudOam = new OamBuffer();
        editedCloudOam.BeginFrame();
        editedCloudSprites.CloudSprites.Draw(0xb83b, editedCloudOam,
            120, 72, 0x0800, originIsOnScreen: true);
        editedCloudOam.FinalizeFrame();
        AssertTrue(preservedCloudOam.LowTable.SequenceEqual(editedCloudOam.LowTable) &&
                preservedCloudOam.HighTable.SequenceEqual(editedCloudOam.HighTable),
            "ending cloud sprite override survives stock repair with edited OAM");
        File.Delete(invalidPath);
        File.Delete(cloudSpriteOverride);
        VerifyPostCreditsCharacterArtwork(repaired);
        Console.WriteLine("Ending/credits art: compiled cloud, explosion, completion-text, reward and logo actors, editable cloud/explosion/text/reward/logo OAM, twelve native sheets and two BG maps, visible independent edits, guarded runtime and stock repair pass.");

        void AssertSheet(RoomCharacterAtlas sheet, int source, int bytes, string name)
        {
            byte[] native = RomDataReader.Decompress(bus, source,
                EndingCreditsRomData.Rendering.DecompressionLimit);
            AssertTrue(sheet.Transfer.Span.SequenceEqual(native.AsSpan(0, bytes)),
                $"installed ending {name} PNG preserves every native OBJ byte");
        }
    }

    private sealed class EndingObjectSourceReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, ISnesMutableMemory, IImportCartridgeSource
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address >= (int)new SnesAddress(0x8b,
                    EndingCloudInstructionDefinitions.Start) &&
                address < (int)new SnesAddress(0x8b,
                    EndingCloudInstructionDefinitions.End))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ending cloud reread compiled instruction ${address:X6}.");
            }
            if (address >= (int)new SnesAddress(0x8b,
                    EndingExplosionInstructionDefinitions.Start) &&
                address < (int)new SnesAddress(0x8b,
                    EndingExplosionInstructionDefinitions.End))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ending explosion reread compiled instruction ${address:X6}.");
            }
            if (address >= (int)new SnesAddress(0x8b,
                    EndingCompletionTextInstructionDefinitions.Start) &&
                address < (int)new SnesAddress(0x8b,
                    EndingCompletionTextInstructionDefinitions.End))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ending completion text reread compiled instruction ${address:X6}.");
            }
            if (address >= (int)new SnesAddress(0x8b,
                    EndingRewardInstructionDefinitions.Start) &&
                address < (int)new SnesAddress(0x8b,
                    EndingRewardInstructionDefinitions.End))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ending reward reread compiled instruction ${address:X6}.");
            }
            if (address >= (int)new SnesAddress(0x8b,
                    EndingLogoInstructionDefinitions.Start) &&
                address < (int)new SnesAddress(0x8b,
                    EndingLogoInstructionDefinitions.End))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ending logo reread compiled instruction ${address:X6}.");
            }
            foreach (EndingCloudSpriteFrameDefinition frame in EndingCloudSpriteDefinitions.Frames)
            {
                int start = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                if (address >= start && address < start + 2 + frame.StockPartCount * 5)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Ending cloud reread installed spritemap ${address:X6}.");
                }
            }
            foreach (EndingExplosionSpriteFrameDefinition frame in EndingExplosionSpriteDefinitions.Frames)
            {
                int start = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                if (address >= start && address < start + 2 + frame.StockPartCount * 5)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Ending explosion reread installed spritemap ${address:X6}.");
                }
            }
            foreach (EndingCompletionTextSpriteFrameDefinition frame in
                EndingCompletionTextSpriteDefinitions.Frames)
            {
                int start = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                if (address >= start && address < start + 2 + frame.StockPartCount * 5)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Ending completion text reread installed spritemap ${address:X6}.");
                }
            }
            foreach (EndingRewardSpriteFrameDefinition frame in EndingRewardSpriteDefinitions.Frames)
            {
                int start = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                if (address >= start && address < start + 2 + frame.StockPartCount * 5)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Ending reward reread installed spritemap ${address:X6}.");
                }
            }
            foreach (EndingLogoSpriteFrameDefinition frame in EndingLogoSpriteDefinitions.Frames)
            {
                int start = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                if (address >= start && address < start + 2 + frame.StockPartCount * 5)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Ending logo reread installed spritemap ${address:X6}.");
                }
            }
            if (address is EndingCreditsRomData.Assets.EscapeCloudCharacters or
                EndingCreditsRomData.Assets.EndingObjectCharacters or
                EndingCreditsRomData.Assets.EndingObjectCharacters70 or
                EndingCreditsRomData.Assets.EndingObjectCharacters74 or
                EndingCreditsRomData.Assets.EndingObjectCharacters78 or
                EndingCreditsRomData.Assets.EndingObjectCharacters7C or
                EndingCreditsRomData.Assets.WaitingForCreditsCharacters or
                EndingCreditsRomData.Assets.ShootingScreenCharacters or
                EndingCreditsRomData.Assets.SuitlessSamusCharacters or
                EndingCreditsRomData.Assets.WaitingForCreditsTilemap or
                EndingCreditsRomData.Assets.PostCreditsTileFragmentA or
                EndingCreditsRomData.Assets.PostCreditsTileFragmentB or
                EndingCreditsRomData.Assets.PostCreditsMode7Characters or
                EndingPostShotDefinitions.LogoTiles or
                EndingPostShotDefinitions.LogoMap)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException($"Ending OBJ reread cartridge source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        public byte ReadCartridgeByte(int address)
        {
            _ = ReadByte(address);
            return (source as IImportCartridgeSource ?? throw new InvalidOperationException(
                "Ending OBJ guard requires a cartridge import source."))
                .ReadCartridgeByte(address);
        }

        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Ending OBJ guard requires WRAM."))
            .ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Ending OBJ guard requires SRAM."))
            .ReadSaveRamByte(address);
    }
}
