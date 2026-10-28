using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.RenderVerification.Samus;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Copies only already-extracted Samus presentation into a disposable installation.
/// No ROM/import routine, player override, setting or save is opened or modified.
/// </summary>
internal sealed class InstalledSamusArtworkFixture : IDisposable
{
    /// <summary>Owns the temporary copy and its replacement PNG files for the fixture lifetime.</summary>
    private readonly TestScratchDirectory scratch;
    /// <summary>Root of the disposable installation used to load edited artwork.</summary>
    private readonly string root;
    /// <summary>Original body artwork loaded from the supplied installation before any files are copied.</summary>
    internal InstalledSamusBodyArtwork Stock { get; }
    /// <summary>Artwork loaded from the scratch installation after its replacement PNGs are written.</summary>
    internal InstalledSamusBodyArtwork Edited { get; }
    /// <summary>Number of replacement sheets written across body sets, arm cannon, and death tiles.</summary>
    internal int EditedPngCount { get; }
    /// <summary>Disposable installation whose override directory supplies the edited artwork.</summary>
    internal GameInstallation Installation { get; }

    /// <summary>Copies installed Samus artwork into scratch storage, remaps nontransparent tile indices, and reloads it through the installation loader.</summary>
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
                int sets = top ? InstalledSamusBodyArtwork.TopSetCount : InstalledSamusBodyArtwork.BottomSetCount;
                for (int set = 0; set < sets; set++)
                    Edit($"{(top ? "top" : "bottom")}-{set:X2}.png", 64,
                        (top ? Stock.TopSet(set) : Stock.BottomSet(set)).Count * 16);
            }
            Edit(SamusArmCannonArtworkFormat.TileFileName,
                SamusArmCannonArtworkFormat.CoverTileCount * 8, 8);
            Edit(SamusDeathTileAtlasFormat.ArtworkFileName,
                SamusDeathTileAtlasFormat.Width, SamusDeathTileAtlasFormat.Height);
            EditedPngCount = InstalledSamusBodyArtwork.TopSetCount +
                InstalledSamusBodyArtwork.BottomSetCount + 2;
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
                IndexedPng.Write(output, new(checked((ushort)width), checked((ushort)height)), image.Pixels, image.Colours);
            }
        }
        catch { Dispose(); throw; }
    }

    /// <summary>Cycles four-bit character indices while preserving transparent index zero.</summary>
    internal static byte Remap(byte index) => index == 0 ? (byte)0 : (byte)(index % 15 + 1);

    /// <summary>Deletes the scratch installation and all replacement sheets.</summary>
    public void Dispose() => scratch.Dispose();
}
