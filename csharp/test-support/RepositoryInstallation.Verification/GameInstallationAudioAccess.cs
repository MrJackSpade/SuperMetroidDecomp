using System.Collections.Concurrent;
using SuperMetroid.AssetExtraction.Audio;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// The installation's extracted audio catalog for the synthesizer's verifiers (#1305). The game plays the installed clips;
/// the catalog they are rendered from is the audio's import data, which no installation holds (#1693), so the
/// verifiers read it extracted from the installation's own cartridge copy into a scratch directory, once per cartridge
/// per process.
/// </summary>
internal static class GameInstallationAudioAccess
{
    /// <summary>Each cartridge copy's extracted audio import data, by the copy's full path.</summary>
    private static readonly ConcurrentDictionary<string, Lazy<string>> importData = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Reads the audio catalog the installation's clips are rendered from.</summary>
    extension(GameInstallation self)
    {
        /// <summary>
        /// The directory holding the audio's import data, the manifest, upload streams and samples, as the installer
        /// extracts them from the installation's cartridge copy; extracted on first use.
        /// </summary>
        internal string AudioImportDirectory() => importData.GetOrAdd(Path.GetFullPath(self.RomPath),
            rom => new Lazy<string>(() => SharedFixtureTiming.Load("Fixture: audio import data", () => ExtractAudio(rom)))).Value;

        /// <summary>The stock extracted audio catalog the installation's clips are rendered from.</summary>
        internal ExtractedAudioAssetCatalog LoadAudio() => ExtractedAudioAssetCatalog.Load(self.AudioImportDirectory());
    }

    /// <summary>Extracts the audio's import data from the cartridge copy at <paramref name="romPath"/> into a new scratch directory.</summary>
    [PrivateInstallation("The audio catalog the clips are rendered from is import data no installation holds (#1693): checks of the catalog and the synthesizer read this cartridge-derived extraction, made once per process.")]
    private static string ExtractAudio(string romPath)
    {
        var scratch = new TestScratchDirectory("audio-import");
        byte[] rom;
        using (Stream source = File.OpenRead(romPath))
            rom = SupportedCartridge.Read(source);
        SpcAudioAssetExtractor.Extract(new CartridgeImportAddressSpace(rom), scratch.Root);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => scratch.Dispose();
        return scratch.Root;
    }
}
