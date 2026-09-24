using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Exercises all installed opening-scene character sheets through the real VRAM loader.</summary>
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

            var native = new IntroCinematicState(bus);
            var guarded = new IntroArtworkSourceReadGuard(bus);
            var installed = new IntroCinematicState(guarded, characterArtwork: stock);
            byte[] nativeVram = native.CaptureTranslatedRenderSnapshot().Memory.Vram.ToArray();
            AssertTrue(installed.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(nativeVram),
                "three installed PNGs create exact native opening-cinematic VRAM, including overlapping OBJ uploads");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed cinematic never reads any of the three cartridge character sources");

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
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "edited and rebound cinematic never reads cartridge character sources");

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
            File.WriteAllBytes(backgroundOverridePath, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroCinematicArt(),
                "malformed selected intro PNG fails instead of silently falling back");
            Console.WriteLine(
                "Intro art: three indexed PNGs, exact overlapping VRAM, independent edits/rebinds, repair and strict failures pass.");
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
