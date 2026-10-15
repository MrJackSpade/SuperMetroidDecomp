using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Core.Audio;

/// <summary>
/// Verification drivers for fixtures that feed cartridge port writes and uploads through the
/// session's port model to the emulated driver (#1304). Since #1305 the game session plays
/// clips; synthesis fixtures compose the same event source with the offline synthesizer.
/// </summary>
internal static class CartridgeAudioSessionAccess
{
    /// <summary>A synthesizing harness over <paramref name="assets"/>, so every update renders PCM.</summary>
    internal static SynthesizingAudioSession CreateSynthesizing(ExtractedAudioAssetCatalog assets) => new(assets);
}
