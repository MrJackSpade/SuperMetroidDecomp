namespace SuperMetroid.Core.Audio;

/// <summary>
/// Verification's record of the requests a <see cref="GameplaySoundRequests"/> or <see cref="GameplayAudioRequests"/> makes
/// (#2137, #2499): production keeps no record, so a check records the requests between the queues and the systems that
/// make them (<see cref="RecordingSoundQueue"/>, <see cref="RecordingMusicQueue"/>) and begins each update it inspects.
/// </summary>
internal static class GameplayAudioRequestsAccess2
{
    /// <summary>The private field that holds the sound queues the requests go on into.</summary>
    private const string SoundsField = "sounds";

    /// <summary>The private field that holds the music queue the requests go on into.</summary>
    private const string MusicField = "music";

    /// <summary>Records the sound requests and reads the record.</summary>
    extension(GameplaySoundRequests self)
    {
        /// <summary>
        /// Begins a check's update of the requests: from now on every request is recorded, and the requests made before
        /// leave the record. The first call puts the record between the requests and their queues: the sound queues, and
        /// the music queue of requests that also make music.
        /// </summary>
        internal void BeginRecordedUpdate()
        {
            if (PrivateState.Field<GameplaySoundRequests, IGameplaySoundQueue>(self, SoundsField) is RecordingSoundQueue sounds)
                sounds.BeginUpdate();
            else
                PrivateState.SetField(self, SoundsField,
                    new RecordingSoundQueue(PrivateState.Field<GameplaySoundRequests, IGameplaySoundQueue>(self, SoundsField)));
            if (self is not GameplayAudioRequests audio)
                return;
            if (PrivateState.Field<GameplayAudioRequests, IGameplayMusicQueue>(audio, MusicField) is RecordingMusicQueue music)
                music.BeginUpdate();
            else
                PrivateState.SetField(audio, MusicField,
                    new RecordingMusicQueue(PrivateState.Field<GameplayAudioRequests, IGameplayMusicQueue>(audio, MusicField)));
        }

        /// <summary>The sound requests made since the check's update began, in the order the sound queues receive them.</summary>
        internal IReadOnlyList<SoundRequest> SoundRequests =>
            (PrivateState.Field<GameplaySoundRequests, IGameplaySoundQueue>(self, SoundsField) as RecordingSoundQueue ?? throw NotRecorded()).Sounds;
    }

    /// <summary>Reads the recorded music requests.</summary>
    extension(GameplayAudioRequests self)
    {
        /// <summary>The music requests made since the check's update began, in the order the music queue receives them.</summary>
        internal IReadOnlyList<GameplayMusicRequest> MusicRequests =>
            (PrivateState.Field<GameplayAudioRequests, IGameplayMusicQueue>(self, MusicField) as RecordingMusicQueue ?? throw NotRecorded()).Music;
    }

    /// <summary>The failure of a read of requests no check has begun recording.</summary>
    private static InvalidOperationException NotRecorded() =>
        new("These audio requests are not recorded: begin the check's update with BeginRecordedUpdate before the requests it reads.");
}
