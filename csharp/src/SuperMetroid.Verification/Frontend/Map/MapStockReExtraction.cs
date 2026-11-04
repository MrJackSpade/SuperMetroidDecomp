using System.Runtime.CompilerServices;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>
/// Re-extracts the stock map presentation beside an edited override, for the override checks that confirm a stock
/// replacement neither rewrites nor outranks the user's file.
/// </summary>
internal static class MapStockReExtraction
{
    /// <summary>
    /// Each cartridge's extracted stock files by their path under the stock directory (#1532). The extractor reads only the
    /// cartridge and writes only its own directory, so every later replacement from the same cartridge holds these bytes.
    /// </summary>
    private static readonly ConditionalWeakTable<ISnesAddressSpace, StockFiles> extracted = new();

    /// <summary>
    /// Extracts the stock map presentation from <paramref name="bus"/> into <paramref name="rebuiltDirectory"/> and loads
    /// it under <paramref name="overrideDirectory"/>, so the caller can compare the selected content and the override's
    /// bytes with those it held before the extraction. The first replacement from a cartridge runs the extractor; each later
    /// one writes the files that extraction wrote, which are the bytes the extractor writes from that cartridge.
    /// </summary>
    /// <param name="bus">Cartridge the stock map presentation is extracted from.</param>
    /// <param name="rebuiltDirectory">Fresh directory the extractor writes the replacement stock into.</param>
    /// <param name="overrideDirectory">Override directory holding the edit under test.</param>
    /// <returns>The map presentation loaded from the replacement stock and the override directory.</returns>
    /// <exception cref="IOException">The replacement directory already holds files.</exception>
    [PrivateInstallation("Runs MapPresentationExtractor into a fresh stock directory beside an edited map override, so its callers check that a stock extraction leaves the override's bytes alone and the override still outranks the newly written stock; reading the installed maps never runs the extractor.")]
    internal static AreaMapPresentation ReExtractStock(ISnesAddressSpace bus, string rebuiltDirectory, string overrideDirectory)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (extracted.TryGetValue(bus, out StockFiles? files))
            files.WriteTo(rebuiltDirectory);
        else
        {
            MapPresentationExtractor.Extract(bus, rebuiltDirectory, "test-provenance");
            extracted.AddOrUpdate(bus, StockFiles.Read(rebuiltDirectory));
        }
        return MapPresentationFiles.Load(rebuiltDirectory, overrideDirectory);
    }

    /// <summary>The files of one extracted stock directory.</summary>
    /// <param name="Files">Each file's bytes by its path relative to the directory.</param>
    private sealed record StockFiles(IReadOnlyDictionary<string, byte[]> Files)
    {
        /// <summary>Reads every file under <paramref name="directory"/>.</summary>
        internal static StockFiles Read(string directory) => new(Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
            .ToDictionary(file => Path.GetRelativePath(directory, file), File.ReadAllBytes, StringComparer.Ordinal));

        /// <summary>Writes the files into <paramref name="directory"/>, which must hold none, as the extractor requires.</summary>
        /// <exception cref="IOException">The directory already holds files.</exception>
        internal void WriteTo(string directory)
        {
            if (Directory.Exists(directory) && Directory.EnumerateFileSystemEntries(directory).Any())
                throw new IOException($"Replacement stock directory {directory} already holds files.");
            foreach ((string relative, byte[] bytes) in Files)
            {
                string path = Path.Combine(directory, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException($"Stock file {path} has no directory."));
                File.WriteAllBytes(path, bytes);
            }
        }
    }
}
