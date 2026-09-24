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
                "Intro art: three indexed PNGs and six tilemaps, exact VRAM, phase-aware rebind, repair and strict failures pass.");
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
                IntroCinematicRomData.Assets.ObjectCharacters)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread character source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
