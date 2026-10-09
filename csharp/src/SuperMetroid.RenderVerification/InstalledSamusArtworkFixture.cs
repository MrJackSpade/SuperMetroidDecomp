using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

/// <summary>
/// Copies only already-extracted Samus presentation into a disposable installation.
/// No ROM/import routine, player override, setting or save is opened or modified.
/// </summary>
internal sealed class InstalledSamusArtworkFixture : IDisposable
{
    /// <summary>Disposable test installation root that owns the copied and edited PNG files.</summary>
    private readonly string root;

    /// <summary>Stock Samus body artwork loaded from the existing installation before fixture edits.</summary>
    internal SamusBodyArtworkCatalog Stock { get; }

    /// <summary>Samus body artwork reloaded from the fixture installation after PNG overrides are written.</summary>
    internal SamusBodyArtworkCatalog Edited { get; }

    /// <summary>Number of body, arm-cannon, and death-atlas PNG files overridden by this fixture.</summary>
    internal int EditedPngCount { get; }

    /// <summary>Installation descriptor for the fixture's temporary data directories.</summary>
    internal GameInstallation Installation { get; }

    /// <summary>Creates an isolated artwork installation and changes nontransparent character indices in its PNG overrides.</summary>
    /// <param name="installationRoot">Existing installation used as the source of extracted Samus artwork.</param>
    /// <param name="editLayout">Whether to also write the fixture's body-layout override.</param>
    internal InstalledSamusArtworkFixture(string installationRoot, bool editLayout = false)
    {
        string source = new GameInstallation(Path.GetFullPath(installationRoot)).SamusBodyDirectory;
        // Validate before copying: an absent/outdated installation must not silently
        // regenerate its data from the cartridge to make this verification pass.
        Stock = SamusBodyArtworkFiles.Load(source, null);
        root = Path.GetFullPath(Path.Combine("csharp", "test-temp",
            "installed-samus-artwork-" + Guid.NewGuid().ToString("N")));
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
                IndexedPngImage image = IndexedPng.Read(input, width, height);
                for (int i = 0; i < image.Pixels.Length; i++)
                    image.Pixels[i] = Remap(image.Pixels[i]);
                using var output = File.Create(Path.Combine(installation.SamusBodyOverrideDirectory, name));
                // Preserve transparent zero, PNG palette, dimensions and unused cells.
                // Only four-bit character indices are changed, on disk, then reloaded.
                IndexedPng.Write(output, width, height, image.Pixels, image.Palette);
            }
        }
        catch { Dispose(); throw; }
    }

    /// <summary>Maps a nonzero four-bit character index to a different index while retaining transparency at zero.</summary>
    /// <param name="index">Source indexed-PNG pixel value.</param>
    /// <returns>Zero for transparent pixels; otherwise a value in the range 1 through 15.</returns>
    internal static byte Remap(byte index) => index == 0 ? (byte)0 : (byte)(index % 15 + 1);

    /// <summary>Removes the temporary installation after verifying that its path is inside the fixture directory.</summary>
    /// <exception cref="InvalidOperationException">The recorded root is not a recognized installed-artwork fixture path.</exception>
    public void Dispose()
    {
        string owner = Path.GetFullPath(Path.Combine("csharp", "test-temp")) + Path.DirectorySeparatorChar;
        if (!root.StartsWith(owner, StringComparison.OrdinalIgnoreCase) ||
            !Path.GetFileName(root).StartsWith("installed-samus-artwork-", StringComparison.Ordinal))
            throw new InvalidOperationException("Refusing to remove a non-fixture artwork directory.");
        if (Directory.Exists(root)) Directory.Delete(root, true);
    }
}
