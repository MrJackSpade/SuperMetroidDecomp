using SuperMetroid.Core.Audio;

/// <summary>
/// Verification drivers over <see cref="CartridgeAudioSession"/> for fixtures that feed cartridge
/// port writes and uploads straight to the session's port model and backend (#1304).
/// </summary>
internal static class CartridgeAudioSessionAccess
{
    /// <summary>A session already bound to <paramref name="assets"/>, so every update renders PCM.</summary>
    internal static CartridgeAudioSession CreateSynthesizing(ExtractedAudioAssetCatalog assets)
    {
        var session = new CartridgeAudioSession();
        session.BindSynthesis(assets);
        return session;
    }

    /// <summary>Advances a bound session by one update and copies out its PCM.</summary>
    extension(CartridgeAudioSession self)
    {
        /// <summary>Applies one update's commands and returns a copy of the update's interleaved 48-kHz PCM.</summary>
        internal short[] RenderFrame(IReadOnlyList<CartridgeAudioCommand> commands)
        {
            self.Advance(commands);
            var pcm = new short[CartridgeAudioSession.PcmSamplesPerFrame];
            self.CopyFrame(pcm);
            return pcm;
        }

        /// <summary>The emulated driver the bound backend plays through.</summary>
        internal ManagedSpcPlayer PlayerForVerification =>
            self.player ?? throw new InvalidOperationException("The session has no bound synthesizer.");
    }
}
