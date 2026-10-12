using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

/// <summary>
/// Copies only already-extracted Samus presentation into a disposable installation.
/// No ROM/import routine, player override, setting or save is opened or modified.
/// </summary>
internal sealed class InstalledSamusArtworkFixture : IDisposable
{
    private readonly TestScratchDirectory scratch;
    private readonly string root;
    internal SamusBodyArtworkCatalog Stock { get; }
    internal SamusBodyArtworkCatalog Edited { get; }
    internal int EditedPngCount { get; }
    internal GameInstallation Installation { get; }

    internal InstalledSamusArtworkFixture(string installationRoot, bool editLayout = false)
    {
        string source = new GameInstallation(Path.GetFullPath(installationRoot)).SamusBodyDirectory;
        // Validate before copying: an absent/outdated installation must not silently
        // regenerate its data from the cartridge to make this verification pass.
        Stock = SamusBodyArtworkFiles.Load(source, null);
        scratch = new TestScratchDirectory("installed-samus-artwork");
        root = scratch.Root;
        try
        {
            var installation = new GameInstallation(root);
            Installation = installation;
            Directory.CreateDirectory(installation.SamusBodyDirectory);
            Directory.CreateDirectory(installation.SamusBodyOverrideDirectory);
            foreach (string path in Directory.EnumerateFiles(source))
                File.Copy(path, Path.Combine(installation.SamusBodyDirectory, Path.GetFileName(path)));
            for (int half = 0; half < 2; half++)
            {
                bool top = half == 0;
                int sets = top ? SamusBodyArtworkCatalog.TopSetCount : SamusBodyArtworkCatalog.BottomSetCount;
                for (int set = 0; set < sets; set++)
                    Edit($"{(top ? "top" : "bottom")}-{set:X2}.png", 64,
                        (top ? Stock.TopSet(set) : Stock.BottomSet(set)).Count * 16);
            }
            Edit(SamusArmCannonArtworkFormat.TileFileName,
                SamusArmCannonArtworkFormat.TileSourcePointers.Length * 8, 8);
            Edit(SamusDeathTileAtlasFormat.ArtworkFileName,
                SamusDeathTileAtlasFormat.Width, SamusDeathTileAtlasFormat.Height);
            EditedPngCount = SamusBodyArtworkCatalog.TopSetCount +
                SamusBodyArtworkCatalog.BottomSetCount + 2;
            if (editLayout) SamusIsolationArtworkEdits.WriteBody(installation);
            Edited = installation.LoadSamusBodyArt();
            if (Stock.ContentIdentity == Edited.ContentIdentity)
                throw new InvalidOperationException("PNG replacements did not change selected Samus content.");

            void Edit(string name, int width, int height)
            {
                using var input = File.OpenRead(Path.Combine(installation.SamusBodyDirectory, name));
                IndexedPngImage image = IndexedPng.Read(input, new(checked((ushort)width), checked((ushort)height)));
                for (int i = 0; i < image.Pixels.Length; i++)
                    image.Pixels[i] = Remap(image.Pixels[i]);
                using var output = File.Create(Path.Combine(installation.SamusBodyOverrideDirectory, name));
                // Preserve transparent zero, PNG palette, dimensions and unused cells.
                // Only four-bit character indices are changed, on disk, then reloaded.
                IndexedPng.Write(output, new(checked((ushort)width), checked((ushort)height)), image.Pixels, image.Palette);
            }
        }
        catch { Dispose(); throw; }
    }

    internal static byte Remap(byte index) => index == 0 ? (byte)0 : (byte)(index % 15 + 1);

    public void Dispose() => scratch.Dispose();
}
