using System.Runtime.CompilerServices;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Audio;

/// <summary>
/// Verification's record of what a <see cref="GameplaySoundRequests"/> or <see cref="GameplayAudioRequests"/> makes
/// (#2137, #2499, #2602). Requests a check builds record every request between the requests and their queues
/// (<see cref="RecordingSoundQueue"/>, <see cref="RecordingMusicQueue"/>); a runtime's requests, which the runtime builds
/// over its session's queues, are read where the game puts them: the sounds each library's queue admits.
/// </summary>
internal static class GameplayAudioRequestsAccess2
{
    /// <summary>The session queues' marks a check's update of a runtime's requests began with.</summary>
    private static readonly ConditionalWeakTable<GameplaySoundRequests, SessionMarks> Marks = [];

    /// <summary>Records the sound requests and reads the record.</summary>
    extension(GameplaySoundRequests self)
    {
        /// <summary>
        /// Begins a check's update of the requests: from now on every request is recorded, and the requests made before
        /// leave the record. A runtime's requests mark their session's queues instead, whose admitted sounds the update
        /// then reads.
        /// </summary>
        internal void BeginRecordedUpdate()
        {
            switch (self.sounds)
            {
                case RecordingSoundQueue sounds:
                    sounds.BeginUpdate();
                    if (self is GameplayAudioRequests { music: RecordingMusicQueue music })
                        music.BeginUpdate();
                    return;
                case SessionGameplayAudioQueues session:
                    Marks.AddOrUpdate(self, new SessionMarks(session.queues,
                        [.. Enum.GetValues<SoundEffectLibrary>().Select(session.queues.MarkSoundRequestsForVerification)]));
                    return;
                default:
                    throw new InvalidOperationException($"These audio requests go on into {self.sounds.GetType().Name}, which no check records.");
            }
        }

        /// <summary>The sound requests made since the check's update began, in the order the sound queues receive them.</summary>
        internal IReadOnlyList<SoundRequest> SoundRequests =>
            (self.sounds as RecordingSoundQueue ?? throw new InvalidOperationException(
                "A runtime's requests go on into its session's queues: read the sounds they admit with SoundsQueued.")).Sounds;

        /// <summary>
        /// The sounds the session's queues admitted since the check's update of a runtime's requests began, library by
        /// library in the libraries' order, each library's in the order it received them.
        /// </summary>
        internal IReadOnlyList<SoundEffectId> SoundsQueued => Marks.TryGetValue(self, out SessionMarks? marks)
            ? [.. marks.Libraries.SelectMany(marks.Queues.SoundsQueuedSinceForVerification)]
            : throw NotRecorded();

        /// <summary>The session's queues a runtime's requests go on into.</summary>
        /// <exception cref="InvalidOperationException">The requests go on into no session's queues.</exception>
        internal AudioQueues QueuesReached => self.sounds is SessionGameplayAudioQueues session
            ? session.queues
            : throw new InvalidOperationException("These audio requests go on into no session's queues.");
    }

    /// <summary>Reads the recorded music requests.</summary>
    extension(GameplayAudioRequests self)
    {
        /// <summary>The music requests made since the check's update began, in the order the music queue receives them.</summary>
        internal IReadOnlyList<GameplayMusicRequest> MusicRequests =>
            (self.music as RecordingMusicQueue ?? throw NotRecorded()).Music;
    }

    /// <summary>The failure of a read of requests no check has begun recording.</summary>
    private static InvalidOperationException NotRecorded() =>
        new("These audio requests are not recorded: begin the check's update with BeginRecordedUpdate before the requests it reads.");

    /// <summary>A runtime's session queues and each library's mark at the start of a check's update.</summary>
    /// <param name="Queues">The session's queues.</param>
    /// <param name="Libraries">Each library's mark, in the libraries' order.</param>
    private sealed record SessionMarks(AudioQueues Queues, IReadOnlyList<SoundRequestsMark> Libraries);
}
