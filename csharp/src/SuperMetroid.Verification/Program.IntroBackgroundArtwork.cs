using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Exercises the installed PNG through the real opening-cinematic VRAM loader.</summary>
    private static void VerifyIntroBackgroundArtwork(string sourceRom)
    {
        string root = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "intro-background-" + Guid.NewGuid().ToString("N")));
        try
        {
            GameInstallation installation = GameAssetInstaller.Install(sourceRom, root);
            SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(sourceRom);
            byte[] nativeBytes = RomDataReader.Decompress(bus,
                IntroCinematicRomData.Assets.BackgroundCharacters,
                maximumOutputBytes: IntroBackgroundAtlasFormat.NativeByteCount);
            IntroBackgroundAtlas stock = installation.LoadIntroBackground();
            AssertTrue(stock.Transfer.Span.SequenceEqual(nativeBytes),
                "installed intro PNG round-trips every cartridge background-character byte");

            var native = new IntroCinematicState(bus);
            var guarded = new IntroBackgroundSourceReadGuard(bus);
            var installed = new IntroCinematicState(guarded, backgroundArtwork: stock);
            AssertTrue(installed.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(
                    native.CaptureTranslatedRenderSnapshot().Memory.Vram),
                "installed stock PNG creates exact native opening-cinematic VRAM");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "installed cinematic never reads the compressed background source");

            string stockPath = Path.Combine(installation.IntroBackgroundDirectory,
                IntroBackgroundAtlasFormat.FileName);
            string overridePath = Path.Combine(installation.IntroBackgroundOverrideDirectory,
                IntroBackgroundAtlasFormat.FileName);
            Directory.CreateDirectory(installation.IntroBackgroundOverrideDirectory);
            IndexedPngImage image;
            using (var input = File.OpenRead(stockPath))
                image = IndexedPng.Read(input, 256, 256);
            image.Pixels[0] = (byte)((image.Pixels[0] + 1) & 15);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);

            IntroBackgroundAtlas edited = installation.LoadIntroBackground();
            var editedState = new IntroCinematicState(guarded, backgroundArtwork: edited);
            ReadOnlySpan<byte> nativeVram = native.CaptureTranslatedRenderSnapshot().Memory.Vram;
            ReadOnlySpan<byte> editedVram = editedState.CaptureTranslatedRenderSnapshot().Memory.Vram;
            AssertTrue(!editedVram.SequenceEqual(nativeVram) &&
                    editedVram[..IntroBackgroundAtlasFormat.NativeByteCount].SequenceEqual(edited.Transfer.Span) &&
                    editedVram[IntroBackgroundAtlasFormat.NativeByteCount..].SequenceEqual(
                        nativeVram[IntroBackgroundAtlasFormat.NativeByteCount..]),
                "edited intro PNG changes only its real background-character VRAM transfer");
            installed.BindBackgroundArtwork(edited);
            AssertTrue(installed.CaptureTranslatedRenderSnapshot().Memory.Vram.SequenceEqual(editedVram),
                "restored cinematic state rebinds the current installed artwork");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "edited and rebound cinematic avoid the compressed cartridge source");

            File.WriteAllBytes(stockPath, [0]);
            AssertThrows<InvalidDataException>(() => installation.LoadIntroBackground(),
                "an override cannot hide corrupt stock cinematic artwork");
            GameInstallation repaired = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Installed intro content disappeared during repair.");
            AssertTrue(repaired.LoadIntroBackground().Transfer.Span.SequenceEqual(edited.Transfer.Span),
                "stock repair preserves the external intro PNG override");
            File.Delete(stockPath);
            AssertThrows<FileNotFoundException>(() => repaired.LoadIntroBackground(),
                "an override cannot hide missing stock cinematic artwork");
            repaired = GameAssetInstaller.EnsureInstalled(root)
                ?? throw new InvalidOperationException("Installed intro content disappeared during missing-file repair.");
            AssertTrue(repaired.LoadIntroBackground().Transfer.Span.SequenceEqual(edited.Transfer.Span),
                "missing-stock repair also preserves the external intro PNG override");
            File.Delete(overridePath);
            IntroBackgroundAtlas restored = repaired.LoadIntroBackground();
            AssertTrue(restored.Transfer.Span.SequenceEqual(nativeBytes),
                "removing an override restores exact cartridge intro artwork");
            File.WriteAllBytes(overridePath, [0]);
            AssertThrows<InvalidDataException>(() => repaired.LoadIntroBackground(),
                "malformed selected intro PNG fails instead of silently falling back");
            Console.WriteLine(
                "Intro background artwork: 32 KiB PNG round-trip, exact cinematic VRAM, live edit/rebind, repair and strict failures pass.");
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

    private sealed class IntroBackgroundSourceReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address == IntroCinematicRomData.Assets.BackgroundCharacters)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Cinematic reread compressed background source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
