using System.Buffers.Binary;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Exercises installed opening-scene art and tilemaps through the real VRAM loader.</summary>
    private static void VerifyIntroCinematicArtwork(string sourceRom)
    {
        string root = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "intro-artwork-" + Guid.NewGuid().ToString("N")));
        try
        {
            GameInstallation installation = GameAssetInstaller.Install(sourceRom, root);
            SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
            IntroCinematicArtworkCatalog stock = installation.LoadIntroCinematicArt();
            VerifyFrontendRomFreeStartup(installation, sourceRom);
            AssertTrue(stock.BackgroundCharacters.Transfer.Span.SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.BackgroundCharacters,
                        maximumOutputBytes: IntroCinematicArtworkFormat.BackgroundByteCount)),
                "installed intro BG PNG preserves every native tile byte");
            AssertTrue(stock.IntroObjectCharacters.Transfer.Span.SequenceEqual(
                    RomDataReader.ReadFixedBank(bus, IntroCinematicRomData.Assets.IntroObjectCharacters,
                        IntroCinematicArtworkFormat.IntroObjectByteCount)),
                "installed fixed intro OBJ PNG preserves every native tile byte");
            AssertTrue(stock.CinematicObjectCharacters.Transfer.Span.SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.ObjectCharacters,
                        maximumOutputBytes: IntroCinematicArtworkFormat.CinematicObjectByteCount)),
                "installed compressed cinematic OBJ PNG preserves every native tile byte");
            AssertTrue(stock.BackgroundPages.Span.SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.BackgroundPageTilemaps,
                        maximumOutputBytes: IntroCinematicRomData.Vram.BackgroundPageTilemapBytes)),
                "four installed BG page JSON files preserve every native tilemap word");
            AssertTrue(stock.PortraitTilemap.Span.SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.SamusHeadTilemap,
                        maximumOutputBytes: IntroCinematicRomData.Vram.SamusHeadTilemapBytes)),
                "installed portrait JSON preserves every native tilemap word");
            AssertTrue(stock.InitialNarrationTilemap.Span.SequenceEqual(
                    RomDataReader.Decompress(bus, IntroCinematicRomData.Assets.FirstNarrationTilemap,
                        maximumOutputBytes: IntroCinematicRomData.Vram.NarrationTilemapBytes)),
                "installed initial narration JSON preserves every native tilemap word");
            for (int index = 0; index < IntroFinalLineTilemapFormat.CellCount; index++)
            {
                int source = IntroCinematicRomData.Assets.FinalTextLine + index * sizeof(ushort);
                ushort nativeWord = (ushort)(bus.ReadByte(source) | bus.ReadByte(source + 1) << 8);
                AssertEqual(nativeWord, stock.FinalLine.Words.Span[index],
                    $"opening divider tile {index} matches cartridge source");
            }
            for (int offset = IntroEyeAnimationDefinitions.StartPointer;
                 offset < IntroEyeAnimationDefinitions.EndPointer; offset++)
                AssertEqual(bus.ReadByte((int)new SnesAddress(
                        IntroCinematicRomData.Banks.Spritemaps, (ushort)offset)),
                    IntroEyeAnimationDefinitions.ReadByte((ushort)offset),
                    $"opening eye timing byte {offset:X4} matches cartridge");
            for (int frame = 0; frame < IntroEyeTilemapFormat.FrameCount; frame++)
            {
                int source = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    (ushort)(IntroEyeAnimationDefinitions.FrameStartPointer +
                        frame * IntroEyeAnimationDefinitions.FrameStride));
                for (int cell = 0; cell < IntroEyeTilemapFormat.CellsPerFrame; cell++)
                {
                    int wordSource = source + 4 + cell * sizeof(ushort);
                    ushort nativeWord = (ushort)(bus.ReadByte(wordSource) |
                        bus.ReadByte(wordSource + 1) << 8);
                    AssertEqual(nativeWord, stock.EyeFrames.FrameWords(frame)[cell],
                        $"opening eye frame {frame} cell {cell} matches cartridge");
                }
            }
            VerifyIntroEyeArtwork(bus, stock, installation);
            VerifyIntroCaretSpriteArtwork(bus, stock, installation);
            VerifyIntroMotherBrainCollision(bus, stock);
            VerifyIntroMotherBrainSpriteArtwork(bus, stock, installation);
            VerifyIntroMotherBrainExplosionSpriteArtwork(bus, stock, installation);
            VerifyIntroRinkaSpriteArtwork(bus, stock, installation);
            VerifyIntroEggEffectSpriteArtwork(bus, stock, installation);
            VerifyIntroDiscoveryActorSpriteArtwork(bus, stock, installation);
            VerifyIntroScientistSpriteArtwork(bus, stock, installation);
            VerifyIntroMotherBrainDemoInput(bus, stock);
            AssertTrue(stock.Palette.Transfer.Span.SequenceEqual(
                    RomDataReader.ReadFixedBank(bus, IntroCinematicRomData.Assets.Palette,
                        SnesCgram.ByteCount)),
                "installed opening palette preserves every native RGB5 word");

            var native = new IntroCinematicState(bus);
            var guarded = new IntroArtworkSourceReadGuard(bus);
            var installed = new IntroCinematicState(guarded, characterArtwork: stock);
            byte[] nativeVram = native.CaptureTranslatedRenderSnapshot().Memory.Vram.ToArray();
            AssertTrue(installed.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(nativeVram),
                "three installed PNGs create exact native opening-cinematic VRAM, including overlapping OBJ uploads");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed cinematic never reads cartridge character or BG-page sources");
            for (int frame = 0; frame < 360; frame++)
            {
                native.Step(0);
                installed.Step(0);
                AssertEqual(native.Phase, installed.Phase,
                    $"installed cinematic preserves native phase at frame {frame}");
                if (frame % 60 == 0)
                {
                    var expectedScene = native.CaptureTranslatedRenderSnapshot();
                    var actualScene = installed.CaptureTranslatedRenderSnapshot();
                    AssertTrue(actualScene.Brightness == expectedScene.Brightness &&
                            actualScene.Memory.Vram.SequenceEqual(expectedScene.Memory.Vram) &&
                            actualScene.Memory.Cgram.SequenceEqual(expectedScene.Memory.Cgram),
                        $"installed cinematic preserves native graphics and colors at frame {frame}");
                }
            }
            Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
            string paletteName = IntroCinematicPaletteFormat.FileName;
            string paletteStockPath = Path.Combine(installation.IntroCinematicDirectory, paletteName);
            string paletteOverridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, paletteName);
            IntroCinematicPaletteDocument paletteDocument =
                JsonSerializer.Deserialize<IntroCinematicPaletteDocument>(
                    File.ReadAllBytes(paletteStockPath),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidDataException("Extracted opening palette is empty.");
            PaletteRgb5 originalColor = paletteDocument.Colors[1];
            paletteDocument.Colors[1] = originalColor with
            {
                Red = originalColor.Red == 31 ? 30 : originalColor.Red + 1,
            };
            using (var output = File.Create(paletteOverridePath))
                IntroCinematicPalette.Write(output, paletteDocument);
            IntroCinematicArtworkCatalog editedPalette = installation.LoadIntroCinematicArt();
            var editedPaletteState = new IntroCinematicState(guarded, characterArtwork: editedPalette);
            var stockPaletteState = new IntroCinematicState(guarded, characterArtwork: stock);
            ushort[] originalCgram = stockPaletteState.CaptureTranslatedRenderSnapshot().Memory.Cgram.ToArray();
            ushort[] editedCgram = editedPaletteState.CaptureTranslatedRenderSnapshot().Memory.Cgram.ToArray();
            AssertTrue(!editedCgram.SequenceEqual(originalCgram) &&
                editedCgram.AsSpan(0, 1).SequenceEqual(originalCgram.AsSpan(0, 1)) &&
                editedCgram.AsSpan(2).SequenceEqual(originalCgram.AsSpan(2)),
                "edited opening RGB5 color reaches only its production CGRAM destination");
            stockPaletteState.BindCharacterArtwork(editedPalette);
            AssertTrue(stockPaletteState.CaptureTranslatedRenderSnapshot().Memory.Cgram.SequenceEqual(editedCgram),
                "restored initial narration rebinds the current palette without restarting its phase");
            for (int frame = 0; frame < 120; frame++)
            {
                editedPaletteState.Step(0);
                stockPaletteState.Step(0);
                AssertEqual(editedPaletteState.Phase, stockPaletteState.Phase,
                    $"opening palette edit preserves cinematic phase at frame {frame}");
            }
            // The initial single-color edit proves exact CGRAM targeting; it can be
            // hidden behind the first card's chosen palette indices. A broad color
            // edit proves the selected resource also reaches visible scene pixels.
            using (var output = File.Create(paletteOverridePath))
                IntroCinematicPalette.Write(output, paletteDocument with
                {
                    Colors = paletteDocument.Colors.Select(color => color with
                    {
                        Red = (color.Red + 7) & 31,
                        Green = (color.Green + 11) & 31,
                    }).ToArray(),
                });
            IntroCinematicArtworkCatalog visiblePalette = installation.LoadIntroCinematicArt();
            var stockVisualState = new IntroCinematicState(guarded, characterArtwork: stock);
            var editedVisualState = new IntroCinematicState(guarded, characterArtwork: visiblePalette);
            bool changedVisiblePixel = false;
            for (int frame = 0; frame < 400; frame++)
            {
                stockVisualState.Step(0);
                editedVisualState.Step(0);
                AssertEqual(stockVisualState.Phase, editedVisualState.Phase,
                    $"visible opening palette edit preserves cinematic phase at frame {frame}");
                if (frame % 20 == 0 && !SoftwareLayeredSnapshotRenderer.Render(
                        stockVisualState.CaptureTranslatedRenderSnapshot()).AsSpan().SequenceEqual(
                        SoftwareLayeredSnapshotRenderer.Render(
                            editedVisualState.CaptureTranslatedRenderSnapshot())))
                    changedVisiblePixel = true;
            }
            AssertTrue(changedVisiblePixel,
                "edited opening RGB5 colors change displayed narration pixels");
            AssertThrows<InvalidDataException>(() => IntroCinematicPalette.Load(
                new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(paletteDocument with
                {
                    Colors = paletteDocument.Colors.Take(255).ToArray(),
                }))), "opening palette rejects a truncated color table");
            AssertThrows<InvalidDataException>(() => IntroCinematicPalette.Load(
                new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(paletteDocument with
                {
                    Colors = paletteDocument.Colors.Select((color, index) =>
                        index == 1 ? color with { Red = 32 } : color).ToArray(),
                }))), "opening palette rejects out-of-range RGB5 channels");
            File.Delete(paletteOverridePath);
            AssertTrue(installation.LoadIntroCinematicArt().Palette.Transfer.Span.SequenceEqual(
                    stock.Palette.Transfer.Span),
                "removing opening palette override restores exact native colors");
            File.WriteAllBytes(paletteOverridePath, [0]);
            AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
                "malformed opening palette override fails instead of silently restoring stock");
            File.Delete(paletteOverridePath);
            File.WriteAllBytes(paletteStockPath, [0]);
            AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
                "opening palette stock corruption fails manifest validation");
            _ = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Opening palette stock repair lost the installation.");
            AssertTrue(installation.LoadIntroCinematicArt().Palette.Transfer.Span.SequenceEqual(
                    stock.Palette.Transfer.Span),
                "stock repair restores exact cartridge opening palette colors");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed opening palette never rereads its cartridge source");
            string dividerStockPath = Path.Combine(installation.IntroCinematicDirectory,
                IntroFinalLineTilemapFormat.FileName);
            string dividerOverridePath = Path.Combine(installation.IntroCinematicOverrideDirectory,
                IntroFinalLineTilemapFormat.FileName);
            IntroFinalLineTilemapDocument dividerDocument =
                JsonSerializer.Deserialize<IntroFinalLineTilemapDocument>(
                    File.ReadAllBytes(dividerStockPath),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidDataException("Extracted opening divider is empty.");
            const BindingFlags dividerSetupFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            var nativeDividerState = new IntroCinematicState(bus);
            var stockDividerState = new IntroCinematicState(guarded, characterArtwork: stock);
            foreach (IntroCinematicState scene in new[] { nativeDividerState, stockDividerState })
                typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", dividerSetupFlags)!
                    .Invoke(scene, null);
            byte[] stockDividerVram = stockDividerState.CaptureTranslatedRenderSnapshot().Memory.Vram.ToArray();
            AssertTrue(stockDividerVram.SequenceEqual(
                    nativeDividerState.CaptureTranslatedRenderSnapshot().Memory.Vram),
                "installed opening divider produces exact native illustrated-page VRAM");
            int changedCell = Array.FindIndex(dividerDocument.Cells,
                cell => cell.TileColumn < RoomBackgroundTilemapFormat.TileColumns - 1);
            AssertTrue(changedCell >= 0, "opening divider has an editable tile column");
            RoomBackgroundTilemapCell originalDividerCell = dividerDocument.Cells[changedCell];
            dividerDocument.Cells[changedCell] = originalDividerCell with
            {
                TileColumn = originalDividerCell.TileColumn + 1,
            };
            using (var output = File.Create(dividerOverridePath))
                IntroFinalLineTilemap.Write(output, dividerDocument);
            IntroCinematicArtworkCatalog editedDivider = installation.LoadIntroCinematicArt();
            var editedDividerState = new IntroCinematicState(guarded, characterArtwork: editedDivider);
            typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", dividerSetupFlags)!
                .Invoke(editedDividerState, null);
            byte[] expectedDividerVram = stockDividerVram.ToArray();
            int dividerByte = (IntroCinematicRomData.Layers.NarrationTilemapWord +
                IntroCinematicRomData.Text.FinalLineDestinationStart + changedCell) * sizeof(ushort);
            BinaryPrimitives.WriteUInt16LittleEndian(expectedDividerVram.AsSpan(dividerByte),
                editedDivider.FinalLine.Words.Span[changedCell]);
            AssertTrue(!expectedDividerVram.SequenceEqual(stockDividerVram) &&
                    editedDividerState.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(
                        expectedDividerVram),
                "one edited divider cell changes only its production BG3 tile word");
            stockDividerState.BindCharacterArtwork(editedDivider);
            AssertTrue(stockDividerState.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(
                    expectedDividerVram),
                "restored illustrated page rebinds divider without erasing live text elsewhere");
            using (var output = File.Create(dividerOverridePath))
                IntroFinalLineTilemap.Write(output, dividerDocument with
                {
                    Cells = dividerDocument.Cells.Select(cell => cell with
                    {
                        Palette = (cell.Palette + 3) % RoomBackgroundTilemapFormat.PaletteCount,
                    }).ToArray(),
                });
            IntroCinematicArtworkCatalog visibleDivider = installation.LoadIntroCinematicArt();
            var stockDividerVisual = new IntroCinematicState(guarded, characterArtwork: stock);
            var editedDividerVisual = new IntroCinematicState(guarded, characterArtwork: visibleDivider);
            foreach (IntroCinematicState scene in new[] { stockDividerVisual, editedDividerVisual })
            {
                typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", dividerSetupFlags)!
                    .Invoke(scene, null);
                typeof(IntroCinematicState).GetField("brightness", dividerSetupFlags)!
                    .SetValue(scene, 15);
            }
            AssertEqual(stockDividerVisual.Phase, editedDividerVisual.Phase,
                "visual divider override leaves page phase unchanged");
            AssertTrue(!SoftwareLayeredSnapshotRenderer.Render(
                    stockDividerVisual.CaptureTranslatedRenderSnapshot()).AsSpan().SequenceEqual(
                    SoftwareLayeredSnapshotRenderer.Render(
                        editedDividerVisual.CaptureTranslatedRenderSnapshot())),
                "divider palette edits change displayed illustrated-page pixels");
            File.Delete(dividerOverridePath);
            stockDividerState.BindCharacterArtwork(stock);
            AssertTrue(stockDividerState.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(
                    stockDividerVram),
                "removing opening divider override restores exact native BG3 words");
            AssertThrows<InvalidDataException>(() => IntroFinalLineTilemap.Load(
                new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(dividerDocument with
                {
                    Cells = dividerDocument.Cells.Take(127).ToArray(),
                }))), "opening divider rejects a truncated tilemap");
            File.WriteAllBytes(dividerOverridePath, [0]);
            AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
                "malformed opening divider override fails loudly");
            File.Delete(dividerOverridePath);
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed opening divider never rereads its cartridge source");
            VerifyCeresFlightArtwork(installation, bus);
            VerifyCeresDestructionArtwork(installation, bus);
            VerifyEndingFlyawayArtwork(installation);
            VerifyEndingMode7Artwork(installation);
            VerifyEndingObjectArtwork(installation);
            VerifyEndingPaletteArtwork(installation);

            string[] names =
            [
                IntroCinematicArtworkFormat.BackgroundFileName,
                IntroCinematicArtworkFormat.IntroObjectFileName,
                IntroCinematicArtworkFormat.CinematicObjectFileName,
            ];
            Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
            foreach (string name in names)
            {
                string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
                string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
                int nativeByteCount = name == names[0]
                    ? IntroCinematicArtworkFormat.BackgroundByteCount
                    : name == names[1]
                        ? IntroCinematicArtworkFormat.IntroObjectByteCount
                        : IntroCinematicArtworkFormat.CinematicObjectByteCount;
                int tileRows = nativeByteCount / 32 / IntroCinematicArtworkFormat.TileColumns;
                IndexedPngImage image;
                using (var input = File.OpenRead(stockPath))
                    image = IndexedPng.Read(input, 256, tileRows * 8);
                image.Pixels[0] = (byte)((image.Pixels[0] + 1) & 15);
                using (var output = File.Create(overridePath))
                    IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);

                IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();
                var editedState = new IntroCinematicState(guarded, characterArtwork: edited);
                byte[] expected = nativeVram.ToArray();
                // $8B:A395 copies the fixed OBJ sheet before the compressed sheet;
                // the final $400 bytes of the first transfer are deliberately overwritten.
                edited.BackgroundCharacters.Transfer.Span.CopyTo(expected.AsSpan(
                    IntroCinematicRomData.Vram.BackgroundCharacterDestinationByte));
                edited.BackgroundPages.Span.CopyTo(expected.AsSpan(
                    IntroCinematicRomData.Vram.BackgroundPagesDestinationByte));
                edited.IntroObjectCharacters.Transfer.Span.CopyTo(expected.AsSpan(
                    IntroCinematicRomData.Vram.IntroObjectCharactersDestinationByte));
                edited.CinematicObjectCharacters.Transfer.Span.CopyTo(expected.AsSpan(
                    IntroCinematicRomData.Vram.CinematicObjectCharactersDestinationByte));
                // The final beam DMA wins over the $C600-$C6FF portion of the fixed
                // OBJ sheet, including in a freshly constructed cinematic state.
                nativeVram.AsSpan(BeamTileAtlasDefinitions.DestinationWord * 2,
                    BeamTileAtlasDefinitions.ByteCount).CopyTo(expected.AsSpan(
                        BeamTileAtlasDefinitions.DestinationWord * 2));
                AssertTrue(!expected.AsSpan().SequenceEqual(nativeVram) &&
                        editedState.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(expected),
                    $"edited {name} reaches the exact production VRAM transfer without changing other scene bytes");
                installed.BindCharacterArtwork(edited);
                AssertTrue(installed.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(expected),
                    $"restored cinematic state rebinds edited {name} with native transfer ordering");
                File.Delete(overridePath);
                installed.BindCharacterArtwork(stock);
                AssertTrue(installed.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(nativeVram),
                    $"removing {name} override restores stock cinematic VRAM");
            }
            for (int page = 0; page < IntroCinematicArtworkFormat.BackgroundPageCount; page++)
            {
                string name = IntroCinematicArtworkFormat.BackgroundPageFileName(page);
                string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
                string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
                RoomBackgroundTilemapDocument document;
                using (var input = File.OpenRead(stockPath))
                    document = JsonSerializer.Deserialize<RoomBackgroundTilemapDocument>(input,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? throw new InvalidDataException($"Empty cinematic BG page {name}.");
                // Choose a nonblank source cell so each isolated page edit has a
                // deterministic physical tilemap-word difference.
                int editedCell = Enumerable.Range(RoomBackgroundTilemapFormat.TileColumns,
                        26 * RoomBackgroundTilemapFormat.TileColumns)
                    .First(index => document.Pages[0].Cells[index] is
                        { TileColumn: not 31 } or { TileRow: not 31 });
                RoomBackgroundTilemapCell original = document.Pages[0].Cells[editedCell];
                document.Pages[0].Cells[editedCell] = original with
                {
                    TileColumn = 31,
                    TileRow = 31,
                };
                if (page == 0)
                    // A full-page high-contrast fixture crosses the native actor/text
                    // occlusion, proving the authored tilemap is actually rendered.
                    for (int cell = 0; cell < document.Pages[0].Cells.Length; cell++)
                        document.Pages[0].Cells[cell] = document.Pages[0].Cells[cell] with
                        { TileColumn = 0, TileRow = 0, Palette = 7, Priority = true };
                using (var output = File.Create(overridePath))
                    RoomBackgroundTilemapAtlas.Write(output, document,
                        IntroCinematicArtworkFormat.BackgroundPageByteCount);

                IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();
                byte[] expected = nativeVram.ToArray();
                edited.BackgroundPages.Span.CopyTo(expected.AsSpan(
                    IntroCinematicRomData.Vram.BackgroundPagesDestinationByte));
                AssertTrue(!expected.AsSpan().SequenceEqual(nativeVram) &&
                        new IntroCinematicState(guarded, characterArtwork: edited)
                            .CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(expected),
                    $"edited {name} reaches exactly its production BG page transfer");
                if (page == 0)
                {
                    const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                    var stockScene = new IntroCinematicState(bus, characterArtwork: stock);
                    var editedScene = new IntroCinematicState(bus, characterArtwork: edited);
                    foreach (IntroCinematicState scene in new[] { stockScene, editedScene })
                    {
                        typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", flags)!
                            .Invoke(scene, null);
                        typeof(IntroCinematicState).GetMethod("SetupMotherBrainFlashback", flags)!
                            .Invoke(scene, null);
                    }
                    // The setup begins with gameplay colors cleared; wait for the
                    // cartridge's 128-frame palette crossfade before checking pixels.
                    for (int fadeFrame = 0; fadeFrame < 128; fadeFrame++)
                    {
                        stockScene.Step(0);
                        editedScene.Step(0);
                    }
                    // The full game owns INIDISP during this synthetic setup; force
                    // the already-composed layer to visible brightness for this probe.
                    typeof(IntroCinematicState).GetField("brightness", flags)!
                        .SetValue(stockScene, 15);
                    typeof(IntroCinematicState).GetField("brightness", flags)!
                        .SetValue(editedScene, 15);
                    AssertTrue(!SoftwareLayeredSnapshotRenderer.Render(stockScene.CaptureTranslatedRenderSnapshot())
                            .AsSpan().SequenceEqual(SoftwareLayeredSnapshotRenderer.Render(
                                editedScene.CaptureTranslatedRenderSnapshot())),
                        "edited opening BG page produces visible flashback pixels");
                }
                installed.BindCharacterArtwork(edited);
                AssertTrue(installed.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(expected),
                    $"restored cinematic rebinds edited {name} without changing another page");
                File.Delete(overridePath);
                installed.BindCharacterArtwork(stock);
                AssertTrue(installed.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(nativeVram),
                    $"removing {name} override restores stock BG VRAM");
            }
            foreach ((string name, int destination, Func<IntroCinematicArtworkCatalog, ReadOnlyMemory<byte>> transfer)
                in new (string, int, Func<IntroCinematicArtworkCatalog, ReadOnlyMemory<byte>>)[]
                {
                    (IntroCinematicArtworkFormat.PortraitTilemapFileName,
                        IntroCinematicRomData.Vram.SamusHeadTilemapDestinationByte,
                        artwork => artwork.PortraitTilemap),
                    (IntroCinematicArtworkFormat.InitialNarrationTilemapFileName,
                        IntroCinematicRomData.Vram.NarrationTilemapDestinationByte,
                        artwork => artwork.InitialNarrationTilemap),
                })
            {
                string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
                string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
                RoomBackgroundTilemapDocument document;
                using (var input = File.OpenRead(stockPath))
                    document = JsonSerializer.Deserialize<RoomBackgroundTilemapDocument>(input,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? throw new InvalidDataException($"Empty opening tilemap {name}.");
                RoomBackgroundTilemapCell original = document.Pages[0].Cells[0];
                document.Pages[0].Cells[0] = original with
                {
                    TileColumn = (original.TileColumn + 1) % RoomBackgroundTilemapFormat.TileColumns,
                };
                using (var output = File.Create(overridePath))
                    RoomBackgroundTilemapAtlas.Write(output, document,
                        IntroCinematicArtworkFormat.BackgroundPageByteCount);

                IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();
                byte[] expected = nativeVram.ToArray();
                transfer(edited).Span.CopyTo(expected.AsSpan(destination));
                AssertTrue(!expected.AsSpan().SequenceEqual(nativeVram) &&
                        new IntroCinematicState(guarded, characterArtwork: edited)
                            .CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(expected),
                    $"edited {name} reaches only its initial cinematic VRAM page");
                var restoredEarly = new IntroCinematicState(guarded, characterArtwork: stock);
                restoredEarly.BindCharacterArtwork(edited);
                AssertTrue(restoredEarly.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(expected),
                    $"restored first narration/portrait state receives edited {name}");

                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                var restoredLate = new IntroCinematicState(guarded, characterArtwork: stock);
                typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", flags)!
                    .Invoke(restoredLate, null);
                byte[] lateBefore = restoredLate.CaptureTranslatedRenderSnapshot().Memory.Vram.ToArray();
                restoredLate.BindCharacterArtwork(edited);
                byte[] lateAfter = restoredLate.CaptureTranslatedRenderSnapshot().Memory.Vram.ToArray();
                if (name == IntroCinematicArtworkFormat.InitialNarrationTilemapFileName)
                    AssertTrue(lateAfter.SequenceEqual(lateBefore),
                        "later typewriter map survives rebinding an edited initial narration page");
                else
                {
                    transfer(edited).Span.CopyTo(lateBefore.AsSpan(destination));
                    AssertTrue(lateAfter.SequenceEqual(lateBefore),
                        "later illustrated page receives edited portrait map without changing typewriter text");
                }
                File.Delete(overridePath);
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "edited and rebound cinematic never reads cartridge character or tilemap sources");

            string backgroundStockPath = Path.Combine(installation.IntroCinematicDirectory, names[0]);
            string backgroundOverridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, names[0]);
            using (var input = File.OpenRead(backgroundStockPath))
            {
                IndexedPngImage image = IndexedPng.Read(input, 256, 256);
                image.Pixels[0] = (byte)((image.Pixels[0] + 1) & 15);
                using var output = File.Create(backgroundOverridePath);
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            }
            IntroCinematicArtworkCatalog selected = installation.LoadIntroCinematicArt();
            File.WriteAllBytes(backgroundStockPath, [0]);
            AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
                "an override cannot hide corrupt stock cinematic artwork");
            GameInstallation repaired = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Installed intro content disappeared during repair.");
            AssertTrue(repaired.LoadIntroCinematicArt().BackgroundCharacters.Transfer.Span.SequenceEqual(
                    selected.BackgroundCharacters.Transfer.Span),
                "stock repair preserves the external cinematic PNG override");
            File.Delete(backgroundStockPath);
            AssertThrows<FileNotFoundException>(() => repaired.LoadIntroCinematicArt(),
                "an override cannot hide missing stock cinematic artwork");
            repaired = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Installed intro content disappeared during missing-file repair.");
            AssertTrue(repaired.LoadIntroCinematicArt().BackgroundCharacters.Transfer.Span.SequenceEqual(
                    selected.BackgroundCharacters.Transfer.Span),
                "missing-stock repair also preserves the external cinematic PNG override");
            File.Delete(backgroundOverridePath);
            AssertTrue(repaired.LoadIntroCinematicArt().BackgroundCharacters.Transfer.Span.SequenceEqual(
                    stock.BackgroundCharacters.Transfer.Span),
                "removing an override restores exact cartridge opening artwork");
            string pageStockPath = Path.Combine(installation.IntroCinematicDirectory,
                IntroCinematicArtworkFormat.BackgroundPageFileName(0));
            File.WriteAllBytes(pageStockPath, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroCinematicArt(),
                "a corrupt stock BG page is rejected even without an override");
            repaired = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Installed intro BG pages disappeared during repair.");
            AssertTrue(repaired.LoadIntroCinematicArt().BackgroundPages.Span.SequenceEqual(
                    stock.BackgroundPages.Span),
                "stock repair restores all four native cinematic BG pages");
            File.WriteAllBytes(backgroundOverridePath, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroCinematicArt(),
                "malformed selected intro PNG fails instead of silently falling back");
            File.Delete(backgroundOverridePath);
            string invalidPage = Path.Combine(installation.IntroCinematicOverrideDirectory,
                IntroCinematicArtworkFormat.BackgroundPageFileName(0));
            File.WriteAllBytes(invalidPage, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroCinematicArt(),
                "malformed selected intro BG page fails instead of silently falling back");
            File.Delete(invalidPage);
            string invalidEye = Path.Combine(installation.IntroCinematicOverrideDirectory,
                IntroEyeTilemapFormat.FileName);
            File.WriteAllBytes(invalidEye, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroCinematicArt(),
                "malformed selected opening eye frames fail instead of silently falling back");
            File.Delete(invalidEye);
            string invalidCaret = Path.Combine(installation.IntroCinematicOverrideDirectory,
                IntroCaretSpriteFormat.FileName);
            File.WriteAllBytes(invalidCaret, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroCinematicArt(),
                "malformed selected caret sprites fail instead of silently falling back");
            File.Delete(invalidCaret);
            string invalidMotherBrain = Path.Combine(installation.IntroCinematicOverrideDirectory,
                IntroMotherBrainSpriteFormat.FileName);
            File.WriteAllBytes(invalidMotherBrain, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroCinematicArt(),
                "malformed selected intro Mother Brain sprites fail instead of silently falling back");
            Console.WriteLine(
                "Intro art: three indexed PNGs, seven full tilemaps, eye/caret/Mother Brain/explosion/Rinka/egg-effect/discovery/scientist compositions and full RGB5 palette; native parity, edits, rebind, repair and strict failures pass.");
        }
        finally
        {
            string workspaceTemp = Path.GetFullPath(Path.Combine("csharp", "test-temp")) +
                Path.DirectorySeparatorChar;
            if (!root.StartsWith(workspaceTemp, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Intro artwork test cleanup escaped the workspace temp directory.");
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    private static void VerifyIntroEyeArtwork(SuperMetroidAddressSpace bus,
        IntroCinematicArtworkCatalog stock, GameInstallation installation)
    {
        var guarded = new IntroArtworkSourceReadGuard(bus);
        var nativeVram = new SnesVram();
        var installedVram = new SnesVram();
        var native = new IntroCinematicObjectSystem(bus, nativeVram, new ushort[1024]);
        var installed = new IntroCinematicObjectSystem(guarded, installedVram,
            new ushort[1024], eyeArtwork: stock.EyeFrames);
        native.Step();
        installed.Step();
        AssertTrue(installedVram.Bytes.SequenceEqual(nativeVram.Bytes),
            "installed opening eye frame draws the exact native portrait rectangle");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed opening eye script and frame do not reread ROM");

        string eyePath = Path.Combine(installation.IntroCinematicDirectory,
            IntroEyeTilemapFormat.FileName);
        IntroEyeTilemapDocument document = JsonSerializer.Deserialize<IntroEyeTilemapDocument>(
            File.ReadAllBytes(eyePath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        var firstFrame = document.Frames[0];
        var firstCell = firstFrame.Cells[0];
        firstFrame.Cells[0] = firstCell with
        {
            TileColumn = (firstCell.TileColumn + 1) % RoomBackgroundTilemapFormat.TileColumns,
        };
        using var editedJson = new MemoryStream();
        IntroEyeTilemapPresentation.Write(editedJson, document);
        editedJson.Position = 0;
        IntroEyeTilemapPresentation edited = IntroEyeTilemapPresentation.Load(editedJson);
        var timerField = typeof(IntroCinematicObjectSystem).GetField("eyeInstructionTimer",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        var pointerField = typeof(IntroCinematicObjectSystem).GetField("eyeInstructionPointer",
            BindingFlags.Instance | BindingFlags.NonPublic)!;
        ushort timer = (ushort)timerField.GetValue(installed)!;
        ushort pointer = (ushort)pointerField.GetValue(installed)!;
        installed.BindEyeArtwork(edited);
        AssertEqual(timer, (ushort)timerField.GetValue(installed)!,
            "eye art rebind preserves blink timer");
        AssertEqual(pointer, (ushort)pointerField.GetValue(installed)!,
            "eye art rebind preserves blink script position");
        int packedPosition = IntroEyeAnimationDefinitions.ReadByte(
            (ushort)(IntroEyeAnimationDefinitions.StartPointer + 2)) |
            IntroEyeAnimationDefinitions.ReadByte(
                (ushort)(IntroEyeAnimationDefinitions.StartPointer + 3)) << 8;
        int destination = IntroCinematicRomData.Layers.PortraitTilemapWord +
            (packedPosition >> 8) * IntroCinematicRomData.Layers.TilemapWidth +
            (packedPosition & IntroCinematicRomData.ObjectSystem.PackedPositionXMask);
        AssertEqual(edited.FrameWords(0)[0], installedVram.ReadWord(destination),
            "eye JSON edit reaches the active portrait tile");
        installed.BindEyeArtwork(stock.EyeFrames);
        AssertTrue(installedVram.Bytes.SequenceEqual(nativeVram.Bytes),
            "restoring stock eye art restores exact native VRAM without restarting script");

        for (int frame = 0; frame < 160; frame++)
        {
            native.Step();
            installed.Step();
            AssertTrue(installedVram.Bytes.SequenceEqual(nativeVram.Bytes),
                $"installed normal eye blink preserves native VRAM at frame {frame}");
        }
        native.StartEnglishPageSix();
        installed.StartEnglishPageSix();
        for (int frame = 0; frame < 64; frame++)
        {
            native.Step();
            installed.Step();
            AssertTrue(installedVram.Bytes.SequenceEqual(nativeVram.Bytes),
                $"installed page-six eye blink preserves native VRAM at frame {frame}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed eye lists and all four frames avoid native source reads");
    }

    private static void VerifyIntroCaretSpriteArtwork(SuperMetroidAddressSpace bus,
        IntroCinematicArtworkCatalog stock, GameInstallation installation)
    {
        for (int offset = IntroCaretInstructionDefinitions.StartPointer;
             offset < IntroCaretInstructionDefinitions.EndPointer; offset++)
            AssertEqual(bus.ReadByte((int)new SnesAddress(
                    IntroCinematicRomData.Banks.CinematicCode >> 16, (ushort)offset)),
                IntroCaretInstructionDefinitions.ReadByte((ushort)offset),
                $"opening caret instruction byte {offset:X4} matches cartridge");
        const ushort originX = 8;
        const ushort originY = 24;
        ushort paletteBits = IntroCinematicRomData.Objects.ScientistPalette.Raw;
        foreach (IntroCaretFrameDefinition definition in IntroCaretSpriteDefinitions.Frames)
        {
            var native = new OamBuffer();
            native.BeginFrame();
            native.AddOnScreenSpritemap(bus,
                (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    definition.Pointer), originX, originY, paletteBits);
            native.FinalizeFrame();
            var installed = new OamBuffer();
            installed.BeginFrame();
            stock.CaretSprites.Draw(definition.Pointer, installed,
                originX, originY, paletteBits);
            installed.FinalizeFrame();
            AssertTrue(installed.LowTable.SequenceEqual(native.LowTable) &&
                    installed.HighTable.SequenceEqual(native.HighTable) &&
                    installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                $"opening caret {definition.Name} produces exact native OAM");
        }

        string caretPath = Path.Combine(installation.IntroCinematicDirectory,
            IntroCaretSpriteFormat.FileName);
        IntroCaretSpriteDocument document = JsonSerializer.Deserialize<IntroCaretSpriteDocument>(
            File.ReadAllBytes(caretPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        AssertEqual(1, document.Frames.Count,
            "caret asset excludes neighboring non-caret spritemaps");
        var previousFrames = new Dictionary<string, SpriteVisualPart[]>(StringComparer.Ordinal)
        {
            [IntroCaretSpriteDefinitions.PreviousFrameNames[0]] = document.Frames["caret-visible"],
            [IntroCaretSpriteDefinitions.PreviousFrameNames[1]] = [],
            [IntroCaretSpriteDefinitions.PreviousFrameNames[2]] = [],
            [IntroCaretSpriteDefinitions.PreviousFrameNames[3]] = [],
        };
        using (var previousJson = new MemoryStream())
        {
            IntroCaretSpritePresentation.Write(previousJson, new IntroCaretSpriteDocument
            {
                Version = IntroCaretSpriteFormat.PreviousVersion,
                Frames = previousFrames,
            });
            previousJson.Position = 0;
            IntroCaretSpritePresentation previous = IntroCaretSpritePresentation.Load(previousJson);
            var previousOam = new OamBuffer();
            previousOam.BeginFrame();
            previous.Draw(IntroCaretSpriteDefinitions.Still, previousOam,
                originX, originY, paletteBits);
            previousOam.FinalizeFrame();
            var stockOam = new OamBuffer();
            stockOam.BeginFrame();
            stock.CaretSprites.Draw(IntroCaretSpriteDefinitions.Still, stockOam,
                originX, originY, paletteBits);
            stockOam.FinalizeFrame();
            AssertTrue(previousOam.LowTable.SequenceEqual(stockOam.LowTable) &&
                    previousOam.HighTable.SequenceEqual(stockOam.HighTable),
                "prior four-frame override preserves its actual caret composition");
        }
        SpriteVisualPart part = document.Frames["caret-visible"][0];
        document.Frames["caret-visible"][0] = part with { TileColumn = part.TileColumn + 1 };
        using var editedJson = new MemoryStream();
        IntroCaretSpritePresentation.Write(editedJson, document);
        editedJson.Position = 0;
        IntroCaretSpritePresentation edited = IntroCaretSpritePresentation.Load(editedJson);
        var changed = new OamBuffer();
        changed.BeginFrame();
        edited.Draw(IntroCaretSpriteDefinitions.Still, changed, originX, originY, paletteBits);
        changed.FinalizeFrame();
        var original = new OamBuffer();
        original.BeginFrame();
        stock.CaretSprites.Draw(IntroCaretSpriteDefinitions.Still, original,
            originX, originY, paletteBits);
        original.FinalizeFrame();
        AssertTrue(!changed.LowTable.SequenceEqual(original.LowTable) &&
                changed.LowTable[..2].SequenceEqual(original.LowTable[..2]) &&
                changed.HighTable.SequenceEqual(original.HighTable),
            "caret JSON tile edit changes OAM art without changing sprite placement");

        var guarded = new IntroArtworkSourceReadGuard(bus);
        var nativeState = new IntroCinematicState(bus);
        var installedState = new IntroCinematicState(guarded, characterArtwork: stock);
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", flags)!
            .Invoke(nativeState, null);
        typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", flags)!
            .Invoke(installedState, null);
        var objectField = typeof(IntroCinematicState).GetField("objects", flags)!;
        ((IntroCinematicObjectSystem)objectField.GetValue(nativeState)!).Step();
        ((IntroCinematicObjectSystem)objectField.GetValue(installedState)!).Step();
        var prepare = typeof(IntroCinematicState).GetMethod("PrepareIllustratedPageOam", flags)!;
        var nativeOam = (OamBuffer)prepare.Invoke(nativeState, null)!;
        var installedOam = (OamBuffer)prepare.Invoke(installedState, null)!;
        AssertTrue(installedOam.LowTable.SequenceEqual(nativeOam.LowTable) &&
                installedOam.HighTable.SequenceEqual(nativeOam.HighTable),
            "installed illustrated page draws caret without cartridge spritemap reads");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed caret art avoids the native OAM composition source");

        var nativeBlink = new IntroCinematicObjectSystem(bus, new SnesVram(), new ushort[1024]);
        var installedBlink = new IntroCinematicObjectSystem(guarded, new SnesVram(),
            new ushort[1024], eyeArtwork: stock.EyeFrames);
        var setBlink = typeof(IntroCinematicObjectSystem).GetMethod("SetCaretBlinking", flags)!;
        setBlink.Invoke(nativeBlink, null);
        setBlink.Invoke(installedBlink, null);
        for (int frame = 0; frame < 20; frame++)
        {
            nativeBlink.Step();
            installedBlink.Step();
            ushort expected = (frame / 5) % 2 == 0 ? IntroCaretSpriteDefinitions.Still : (ushort)0;
            AssertEqual(expected, nativeBlink.SpriteMapPointer,
                $"native caret blink selects visible/blank frame at frame {frame}");
            AssertEqual(expected, installedBlink.SpriteMapPointer,
                $"installed caret blink selects visible/blank frame at frame {frame}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed caret blink avoids cartridge instruction and visual reads");
    }

    private static void VerifyIntroMotherBrainCollision(SuperMetroidAddressSpace bus,
        IntroCinematicArtworkCatalog stock)
    {
        byte[] native = RomDataReader.ReadFixedBank(bus,
            IntroCinematicRomData.Assets.MotherBrainLevelData,
            IntroCinematicRomData.Flashback.MotherBrainLevelByteCount);
        AssertTrue(IntroMotherBrainCollisionDefinitions.SourceBytes.SequenceEqual(native),
            "compiled Mother Brain flashback physical level matches all 448 cartridge bytes");
        var guarded = new IntroArtworkSourceReadGuard(bus);
        var state = new IntroCinematicState(guarded, characterArtwork: stock);
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", flags)!
            .Invoke(state, null);
        typeof(IntroCinematicState).GetMethod("SetupMotherBrainFlashback", flags)!
            .Invoke(state, null);
        AssertTrue(state.MotherBrainLevelData is not null &&
                state.MotherBrainLevelData.AsSpan().SequenceEqual(native),
            "flashback setup retains the exact native physical level without ROM reads");
        var level = (RoomLevelData)typeof(IntroCinematicState)
            .GetField("flashbackLevel", flags)!.GetValue(state)!;
        for (int index = 0; index < level.ForegroundEntries.Length; index++)
        {
            ushort expected = index * sizeof(ushort) < native.Length
                ? (ushort)(native[index * 2] | native[index * 2 + 1] << 8)
                : (ushort)0;
            AssertEqual(expected, level.ForegroundEntries.Span[index],
                $"Mother Brain flashback collision block {index} preserves native level word");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "flashback setup never rereads the physical level source");
    }

    private static void VerifyIntroMotherBrainSpriteArtwork(SuperMetroidAddressSpace bus,
        IntroCinematicArtworkCatalog stock, GameInstallation installation)
    {
        foreach (IntroMotherBrainSpriteFrameDefinition definition in
            IntroMotherBrainSpriteDefinitions.Frames)
        {
            var native = new OamBuffer();
            native.BeginFrame();
            native.AddOnScreenSpritemap(bus,
                (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    definition.Pointer),
                IntroMotherBrainSpriteState.XPosition,
                IntroMotherBrainSpriteState.YPosition,
                IntroMotherBrainSpriteState.PaletteBits);
            native.FinalizeFrame();
            var installed = new OamBuffer();
            installed.BeginFrame();
            stock.MotherBrainSprites.Draw(definition.Pointer, installed,
                IntroMotherBrainSpriteState.XPosition,
                IntroMotherBrainSpriteState.YPosition,
                IntroMotherBrainSpriteState.PaletteBits);
            installed.FinalizeFrame();
            AssertTrue(installed.LowTable.SequenceEqual(native.LowTable) &&
                    installed.HighTable.SequenceEqual(native.HighTable) &&
                    installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                $"intro Mother Brain {definition.Name} produces exact native OAM");
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = IntroMotherBrainSpriteFormat.FileName;
        IntroMotherBrainSpriteDocument document = JsonSerializer.Deserialize<IntroMotherBrainSpriteDocument>(
            File.ReadAllBytes(Path.Combine(installation.IntroCinematicDirectory, name)),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        document.Frames["mother-brain-frame-0"] = document.Frames["mother-brain-frame-0"]
            .Select(part => part with { Palette = 0 }).ToArray();
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        using (var output = File.Create(overridePath))
            IntroMotherBrainSpritePresentation.Write(output, document);
        IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();
        var stockOam = new OamBuffer();
        stockOam.BeginFrame();
        stock.MotherBrainSprites.Draw(IntroMotherBrainSpriteDefinitions.FrameZero, stockOam,
            IntroMotherBrainSpriteState.XPosition, IntroMotherBrainSpriteState.YPosition,
            IntroMotherBrainSpriteState.PaletteBits);
        stockOam.FinalizeFrame();
        var editedOam = new OamBuffer();
        editedOam.BeginFrame();
        edited.MotherBrainSprites.Draw(IntroMotherBrainSpriteDefinitions.FrameZero, editedOam,
            IntroMotherBrainSpriteState.XPosition, IntroMotherBrainSpriteState.YPosition,
            IntroMotherBrainSpriteState.PaletteBits);
        editedOam.FinalizeFrame();
        AssertTrue(!editedOam.LowTable.SequenceEqual(stockOam.LowTable) &&
                editedOam.HighTable.SequenceEqual(stockOam.HighTable),
            "intro Mother Brain palette edit changes OAM art but not sprite size or X high bits");
        for (int part = 0; part < IntroMotherBrainSpriteDefinitions.StockPartCount; part++)
            AssertTrue(editedOam.LowTable.Slice(part * 4, 2)
                    .SequenceEqual(stockOam.LowTable.Slice(part * 4, 2)),
                $"intro Mother Brain palette edit preserves part {part} screen position");

        var guarded = new IntroArtworkSourceReadGuard(bus);
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var stockState = new IntroCinematicState(guarded, characterArtwork: stock);
        var editedState = new IntroCinematicState(guarded, characterArtwork: edited);
        foreach (IntroCinematicState state in new[] { stockState, editedState })
        {
            typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", flags)!
                .Invoke(state, null);
            typeof(IntroCinematicState).GetMethod("SetupMotherBrainFlashback", flags)!
                .Invoke(state, null);
            ((IntroMotherBrainSpriteState)typeof(IntroCinematicState)
                .GetField("flashbackMotherBrain", flags)!.GetValue(state)!).Step(guarded);
        }
        var render = typeof(IntroCinematicState).GetMethod("RenderMotherBrainFlashback", flags)!;
        Rgba32[] stockPixels = (Rgba32[])render.Invoke(stockState, null)!;
        Rgba32[] editedPixels = (Rgba32[])render.Invoke(editedState, null)!;
        AssertTrue(!editedPixels.SequenceEqual(stockPixels),
            "intro Mother Brain palette override changes visible production scene pixels");
        stockState.BindCharacterArtwork(edited);
        AssertTrue(((Rgba32[])render.Invoke(stockState, null)!).SequenceEqual(editedPixels),
            "restored intro Mother Brain scene rebinds the selected visual composition");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed intro Mother Brain rendering avoids all three native spritemap records");
        File.Delete(overridePath);
    }

    private static void VerifyIntroMotherBrainExplosionSpriteArtwork(
        SuperMetroidAddressSpace bus, IntroCinematicArtworkCatalog stock,
        GameInstallation installation)
    {
        const ushort x = 120;
        const ushort y = 100;
        ushort palette = IntroCinematicRomData.Objects.ExplosionPalette.Raw;
        foreach (IntroMotherBrainExplosionSpriteFrameDefinition definition in
            IntroMotherBrainExplosionSpriteDefinitions.Frames)
        {
            var native = new OamBuffer();
            native.BeginFrame();
            native.AddOnScreenSpritemap(bus,
                (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    definition.Pointer), x, y, palette);
            native.FinalizeFrame();
            var installed = new OamBuffer();
            installed.BeginFrame();
            stock.MotherBrainExplosionSprites.Draw(definition.Pointer, installed,
                x, y, palette);
            installed.FinalizeFrame();
            AssertTrue(installed.LowTable.SequenceEqual(native.LowTable) &&
                    installed.HighTable.SequenceEqual(native.HighTable) &&
                    installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                $"intro Mother Brain explosion {definition.Name} produces exact native OAM");
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = IntroMotherBrainExplosionSpriteFormat.FileName;
        IntroMotherBrainExplosionSpriteDocument document =
            JsonSerializer.Deserialize<IntroMotherBrainExplosionSpriteDocument>(
                File.ReadAllBytes(Path.Combine(installation.IntroCinematicDirectory, name)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        document.Frames["small-explosion-0"] = document.Frames["small-explosion-0"]
            .Select(part => part with { Palette = 0 }).ToArray();
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        using (var output = File.Create(overridePath))
            IntroMotherBrainExplosionSpritePresentation.Write(output, document);
        IntroCinematicArtworkCatalog edited = installation.LoadIntroCinematicArt();

        var guarded = new IntroArtworkSourceReadGuard(bus,
            blockIntroMotherBrainExplosions: true);
        var explosions = new IntroMotherBrainExplosionSystem();
        explosions.SpawnFourthHitExplosions();
        explosions.Step(guarded, introCrossfadeTimer: 1);
        var stockOam = new OamBuffer();
        stockOam.BeginFrame();
        explosions.Draw(guarded, stockOam, stock.MotherBrainExplosionSprites);
        stockOam.FinalizeFrame();
        var editedOam = new OamBuffer();
        editedOam.BeginFrame();
        explosions.Draw(guarded, editedOam, edited.MotherBrainExplosionSprites);
        editedOam.FinalizeFrame();
        AssertTrue(!editedOam.LowTable.SequenceEqual(stockOam.LowTable) &&
                editedOam.HighTable.SequenceEqual(stockOam.HighTable),
            "intro explosion palette edit changes production OAM attributes, not actor placement");
        for (int part = 0; part < stockOam.LastFinalizedSpriteCount; part++)
            AssertTrue(editedOam.LowTable.Slice(part * 4, 2)
                    .SequenceEqual(stockOam.LowTable.Slice(part * 4, 2)),
                $"intro explosion palette edit preserves native part {part} X/Y");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "installed intro explosion actors never reread the twelve native compositions");

        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var stockState = new IntroCinematicState(guarded, characterArtwork: stock);
        var editedState = new IntroCinematicState(guarded, characterArtwork: edited);
        foreach (IntroCinematicState state in new[] { stockState, editedState })
        {
            typeof(IntroCinematicState).GetMethod("SetupFirstIllustratedPage", flags)!
                .Invoke(state, null);
            typeof(IntroCinematicState).GetMethod("SetupMotherBrainFlashback", flags)!
                .Invoke(state, null);
            typeof(IntroCinematicState)
                .GetField("flashbackMotherBrainExplosions", flags)!
                .SetValue(state, explosions);
        }
        var render = typeof(IntroCinematicState)
            .GetMethod("RenderMotherBrainFlashback", flags)!;
        Rgba32[] stockPixels = (Rgba32[])render.Invoke(stockState, null)!;
        Rgba32[] editedPixels = (Rgba32[])render.Invoke(editedState, null)!;
        AssertTrue(!stockPixels.SequenceEqual(editedPixels),
            "intro explosion art override changes visible production flashback pixels");
        stockState.BindCharacterArtwork(edited);
        AssertTrue(((Rgba32[])render.Invoke(stockState, null)!).SequenceEqual(editedPixels),
            "restored intro explosion scene rebinds the selected visual composition");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "production intro explosion render avoids all twelve native compositions");

        document.Frames.Remove("small-explosion-0");
        AssertThrows<InvalidDataException>(() =>
        {
            using var malformed = new MemoryStream();
            IntroMotherBrainExplosionSpritePresentation.Write(malformed, document);
        }, "intro explosion artwork rejects a missing named frame");
        File.Delete(overridePath);
    }

    private sealed class IntroArtworkSourceReadGuard(ISnesAddressSpace source,
        bool blockIntroMotherBrainExplosions = false,
        bool blockIntroRinkas = false,
        bool blockIntroEggEffects = false,
        bool blockIntroDiscoveryActors = false,
        bool blockIntroScientistSprites = false,
        bool blockCeresFlightSprites = false,
        bool blockCeresDestructionSprites = false) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (blockCeresFlightSprites)
            {
                foreach (CeresFlightSpriteFrameDefinition frame in CeresFlightSpriteDefinitions.Frames)
                {
                    int start = (int)new SnesAddress(
                        IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                    if (address >= start && address < start + 2 + frame.StockPartCount * 5)
                    {
                        ForbiddenReadAttempts++;
                        throw new InvalidOperationException(
                            $"Ceres flight reread installed spritemap ${address:X6}.");
                    }
                }
            }
            if (blockCeresDestructionSprites)
            {
                foreach (CeresDestructionSpriteFrameDefinition frame in
                    CeresDestructionSpriteDefinitions.Frames)
                {
                    int start = (int)new SnesAddress(
                        IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                    if (address >= start && address < start + 2 + frame.StockPartCount * 5)
                    {
                        ForbiddenReadAttempts++;
                        throw new InvalidOperationException(
                            $"Ceres destruction reread installed spritemap ${address:X6}.");
                    }
                }
            }
            if (address is IntroCinematicRomData.Assets.BackgroundCharacters or
                IntroCinematicRomData.Assets.BackgroundPageTilemaps or
                IntroCinematicRomData.Assets.SamusHeadTilemap or
                IntroCinematicRomData.Assets.FirstNarrationTilemap or
                IntroCinematicRomData.Assets.IntroObjectCharacters or
                IntroCinematicRomData.Assets.ObjectCharacters or
                CeresFlightRomData.Assets.Mode7Characters or
                CeresFlightRomData.Assets.Mode7Maps or
                CeresFlightRomData.Assets.ObjectCharacters or
                CeresDestructionRomData.Assets.ZebesTilemap or
                CeresDestructionRomData.Assets.ZebesCharacters or
                CeresDestructionRomData.Assets.SharedObjectCharacters)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread character source ${address:X6}.");
            }
            if (address >= CeresFlightRomData.Assets.Palette &&
                address < CeresFlightRomData.Assets.Palette + SnesCgram.ByteCount)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread Ceres flight palette ${address:X6}.");
            }
            if (address >= IntroCinematicRomData.Assets.Palette &&
                address < IntroCinematicRomData.Assets.Palette + SnesCgram.ByteCount)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread opening palette ${address:X6}.");
            }
            if (address >= IntroCinematicRomData.Assets.FinalTextLine &&
                address < IntroCinematicRomData.Assets.FinalTextLine +
                    IntroFinalLineTilemapFormat.CellCount * sizeof(ushort))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread opening divider ${address:X6}.");
            }
            if (address >= IntroCinematicRomData.Assets.MotherBrainLevelData &&
                address < IntroCinematicRomData.Assets.MotherBrainLevelData +
                    IntroCinematicRomData.Flashback.MotherBrainLevelByteCount)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread Mother Brain collision source ${address:X6}.");
            }
            int eyeScriptStart = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                IntroEyeAnimationDefinitions.StartPointer);
            int caretScriptStart = (int)new SnesAddress(
                IntroCinematicRomData.Banks.CinematicCode >> 16,
                IntroCaretInstructionDefinitions.StartPointer);
            if (address >= caretScriptStart &&
                address < caretScriptStart + IntroCaretInstructionDefinitions.EndPointer -
                    IntroCaretInstructionDefinitions.StartPointer)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread caret instruction ${address:X6}.");
            }
            int eyeFrameStart = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                IntroEyeAnimationDefinitions.FrameStartPointer);
            foreach (IntroCaretFrameDefinition caret in IntroCaretSpriteDefinitions.Frames)
            {
                int sourceAddress = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, caret.Pointer);
                int byteCount = 2 + caret.StockPartCount * 5;
                if (address >= sourceAddress && address < sourceAddress + byteCount)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Cinematic reread caret composition ${address:X6}.");
                }
            }
            int motherBrainSpriteStart = (int)new SnesAddress(
                IntroCinematicRomData.Banks.Spritemaps,
                IntroMotherBrainSpriteDefinitions.FrameZero);
            int motherBrainSpriteEnd = (int)new SnesAddress(
                IntroCinematicRomData.Banks.Spritemaps,
                (ushort)(IntroMotherBrainSpriteDefinitions.FrameTwo +
                    2 + IntroMotherBrainSpriteDefinitions.StockPartCount * 5));
            if (address >= motherBrainSpriteStart && address < motherBrainSpriteEnd)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread intro Mother Brain sprite ${address:X6}.");
            }
            int explosionSpriteStart = (int)new SnesAddress(
                IntroCinematicRomData.Banks.Spritemaps,
                IntroMotherBrainExplosionSpriteDefinitions.SmallStart);
            int explosionSpriteEnd = (int)new SnesAddress(
                IntroCinematicRomData.Banks.Spritemaps,
                IntroMotherBrainExplosionSpriteDefinitions.End);
            if (blockIntroMotherBrainExplosions &&
                address >= explosionSpriteStart && address < explosionSpriteEnd)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread intro Mother Brain explosion sprite ${address:X6}.");
            }
            int rinkaSpriteStart = (int)new SnesAddress(
                IntroCinematicRomData.Banks.Spritemaps, IntroRinkaSpriteDefinitions.First);
            int rinkaSpriteEnd = (int)new SnesAddress(
                IntroCinematicRomData.Banks.Spritemaps, IntroRinkaSpriteDefinitions.End);
            if (blockIntroRinkas &&
                address >= rinkaSpriteStart && address < rinkaSpriteEnd)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread intro Rinka sprite ${address:X6}.");
            }
            int eggEffectSpriteStart = (int)new SnesAddress(
                IntroCinematicRomData.Banks.Spritemaps,
                IntroEggEffectSpriteDefinitions.Start);
            int eggEffectSpriteEnd = (int)new SnesAddress(
                IntroCinematicRomData.Banks.Spritemaps,
                IntroEggEffectSpriteDefinitions.End);
            if (blockIntroEggEffects &&
                address >= eggEffectSpriteStart && address < eggEffectSpriteEnd)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread intro egg effect sprite ${address:X6}.");
            }
            if (blockIntroDiscoveryActors)
            {
                int eggStart = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    IntroDiscoveryActorSpriteDefinitions.EggStart);
                int eggEnd = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    IntroDiscoveryActorSpriteDefinitions.EggEnd);
                int babyStart = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    IntroDiscoveryActorSpriteDefinitions.BabyStart);
                int babySmallEnd = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    IntroDiscoveryActorSpriteDefinitions.BabySmallEnd);
                int babyLarge = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    IntroDiscoveryActorSpriteDefinitions.BabyLarge);
                int babyEnd = (int)new SnesAddress(IntroCinematicRomData.Banks.Spritemaps,
                    IntroDiscoveryActorSpriteDefinitions.BabyEnd);
                if (address >= eggStart && address < eggEnd ||
                    address >= babyStart && address < babySmallEnd ||
                    address >= babyLarge && address < babyEnd)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Cinematic reread intro discovery actor sprite ${address:X6}.");
                }
            }
            int scientistSpriteStart = (int)new SnesAddress(
                IntroCinematicRomData.Banks.Spritemaps,
                IntroScientistSpriteDefinitions.Start);
            int scientistSpriteEnd = (int)new SnesAddress(
                IntroCinematicRomData.Banks.Spritemaps,
                IntroScientistSpriteDefinitions.End);
            if (blockIntroScientistSprites &&
                address >= scientistSpriteStart && address < scientistSpriteEnd)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread intro scientist sprite ${address:X6}.");
            }
            int motherBrainDemoListStart = DemoInputRomData.BankBase |
                IntroMotherBrainInputDefinitions.ListStart;
            int motherBrainDemoListEnd = DemoInputRomData.BankBase |
                IntroMotherBrainInputDefinitions.ListEnd;
            int motherBrainDemoHeaderStart = DemoInputRomData.BankBase |
                IntroMotherBrainInputDefinitions.HeaderStart;
            int motherBrainDemoHeaderEnd = DemoInputRomData.BankBase |
                IntroMotherBrainInputDefinitions.HeaderEnd;
            if (address >= motherBrainDemoListStart && address < motherBrainDemoListEnd ||
                address >= motherBrainDemoHeaderStart && address < motherBrainDemoHeaderEnd)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread intro Mother Brain demo program ${address:X6}.");
            }
            if ((address >= eyeScriptStart &&
                    address < eyeScriptStart + IntroEyeAnimationDefinitions.EndPointer -
                        IntroEyeAnimationDefinitions.StartPointer) ||
                (address >= eyeFrameStart &&
                    address < eyeFrameStart + IntroEyeAnimationDefinitions.FrameCount *
                        IntroEyeAnimationDefinitions.FrameStride))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException($"Cinematic reread opening eye source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
