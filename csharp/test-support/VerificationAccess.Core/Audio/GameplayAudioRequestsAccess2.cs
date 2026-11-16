namespace SuperMetroid.Core.Audio;

/// <summary>
/// Verification's record of the requests a <see cref="GameplayAudioRequests"/> makes (#2137): production keeps no record,
/// so a check records the requests between the queues and the systems that make them (<see cref="RecordingAudioQueues"/>)
/// and begins each update it inspects.
/// </summary>
internal static class GameplayAudioRequestsAccess2
{
    /// <summary>Records the requests and reads the record.</summary>
    extension(GameplayAudioRequests self)
    {
        /// <summary>
        /// Begins a check's update of the requests: from now on every request is recorded, and the requests made before
        /// leave the record. The first call puts the record between the requests and their queues.
        /// </summary>
        internal void BeginRecordedUpdate()
        {
            if (PrivateState.Field<GameplayAudioRequests, IGameplayAudioQueues>(self, "queues") is RecordingAudioQueues recording)
                recording.BeginUpdate();
            else
                PrivateState.SetField(self, "queues",
                    new RecordingAudioQueues(PrivateState.Field<GameplayAudioRequests, IGameplayAudioQueues>(self, "queues")));
        }

        /// <summary>The sound requests made since the check's update began, in the order the sound queues receive them.</summary>
        internal IReadOnlyList<SoundRequest> SoundRequests => Recording(self).Sounds;

        /// <summary>The music requests made since the check's update began, in the order the music queue receives them.</summary>
        internal IReadOnlyList<GameplayMusicRequest> MusicRequests => Recording(self).Music;
    }

    /// <summary>The record between <paramref name="requests"/> and their queues.</summary>
    /// <exception cref="InvalidOperationException">No check has begun recording these requests.</exception>
    private static RecordingAudioQueues Recording(GameplayAudioRequests requests) =>
        PrivateState.Field<GameplayAudioRequests, IGameplayAudioQueues>(requests, "queues") as RecordingAudioQueues
            ?? throw new InvalidOperationException(
                "These audio requests are not recorded: begin the check's update with BeginRecordedUpdate before the requests it reads.");
}
