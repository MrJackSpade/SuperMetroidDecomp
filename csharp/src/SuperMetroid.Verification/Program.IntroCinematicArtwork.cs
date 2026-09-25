using System.Buffers.Binary;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rendering;

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
            Console.WriteLine(
                "Intro art: three indexed PNGs, seven tilemaps and full RGB5 palette; VRAM/CGRAM parity, rebind, repair and strict failures pass.");
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

    private sealed class IntroArtworkSourceReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
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
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
