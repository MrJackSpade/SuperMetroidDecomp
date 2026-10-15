using SuperMetroid.AssetExtraction.Audio.Synthesis;
using SuperMetroid.AssetExtraction.Installation;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// The installation's extracted audio catalog for the synthesizer's verifiers (#1305). The game
/// plays the installed clips; only the offline renderer and its checks read the extracted catalog.
/// </summary>
internal static class GameInstallationAudioAccess
{
    /// <summary>Loads the stock extracted audio the clips are rendered from.</summary>
    extension(GameInstallation self)
    {
        /// <summary>The stock extracted audio catalog beneath the installation's audio directory.</summary>
        internal ExtractedAudioAssetCatalog LoadAudio() => ExtractedAudioAssetCatalog.Load(self.AudioDirectory);
    }
}
