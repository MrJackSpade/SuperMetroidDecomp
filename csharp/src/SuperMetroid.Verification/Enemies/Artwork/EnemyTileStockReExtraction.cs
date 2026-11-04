using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using System.Runtime.CompilerServices;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Re-extracts the stock enemy artwork over an edited enemy colour override's private copy, for the override checks that
/// confirm a stock replacement neither rewrites nor outranks the user's file.
/// </summary>
internal static class EnemyTileStockReExtraction
{
    /// <summary>
    /// Extracts the stock enemy artwork from <paramref name="bus"/> into <paramref name="stockDirectory"/>, the private
    /// copy's stock directory beside its override, so the caller can compare the override's bytes with those it held
    /// before the extraction and load the replacement stock under the override.
    /// </summary>
    /// <param name="bus">Cartridge the stock enemy artwork is extracted from.</param>
    /// <param name="stockDirectory">The private copy's stock enemy-tile directory, rewritten in place.</param>
    [PrivateInstallation("Runs EnemyTileArtworkFiles.Extract in place over a private copy's stock enemy artwork beside an edited Norfair Ridley, Tourian statue, Zebetite or Ninja Pirate colour override, so its callers check that a stock extraction into that directory leaves the override's bytes alone and the override still outranks the newly written stock; reading the installed enemy artwork never runs the extractor.")]
    internal static void ReExtractStock(ISnesAddressSpace bus, string stockDirectory)
    {
        foreach ((string file, byte[] bytes) in extracted.GetValue(bus, Extract))
        {
            string path = Path.Combine(stockDirectory, file);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException($"{path} has no directory."));
            File.WriteAllBytes(path, bytes);
        }
    }

    /// <summary>
    /// Each cartridge's stock extraction, by file: the extraction writes the same files from the same cartridge every
    /// time, so it runs once per cartridge and every re-extraction writes its files again.
    /// </summary>
    private static readonly ConditionalWeakTable<ISnesAddressSpace, IReadOnlyDictionary<string, byte[]>> extracted = new();

    /// <summary>The files the stock enemy-art extraction from <paramref name="bus"/> writes, by path beneath its directory.</summary>
    [PrivateInstallation("Extracts the stock enemy artwork once per cartridge into a scratch directory so ReExtractStock can write those exact files over a private copy's stock beside an override without running the extractor again; the shared installation is never written.")]
    private static IReadOnlyDictionary<string, byte[]> Extract(ISnesAddressSpace bus)
    {
        using var scratch = new TestScratchDirectory("enemy-tile-stock");
        EnemyTileArtworkFiles.Extract(bus, scratch.Root, SupportedCartridge.Sha256);
        return Directory.GetFiles(scratch.Root, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(scratch.Root, path), File.ReadAllBytes, StringComparer.Ordinal);
    }
}
