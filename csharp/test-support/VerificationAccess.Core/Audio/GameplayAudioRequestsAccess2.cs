namespace SuperMetroid.Core.Audio;

/// <summary>Verification access to <see cref="GameplayAudioRequests"/> members production does not use.</summary>
internal static class GameplayAudioRequestsAccess2
{
    /// <summary>Exposes <see cref="GameplayAudioRequests"/> state that only verification reads.</summary>
    extension(GameplayAudioRequests self)
    {
        /// <summary>The update's sound requests, in the order the sound queues receive them.</summary>
        internal IReadOnlyList<SoundRequest> SoundRequests => self.soundRequests;
        /// <summary>The update's music requests, in the order the music queue receives them.</summary>
        internal IReadOnlyList<GameplayMusicRequest> MusicRequests => self.musicRequests;
    }
}
