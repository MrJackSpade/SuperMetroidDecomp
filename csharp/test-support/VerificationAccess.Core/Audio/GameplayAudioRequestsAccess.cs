using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Audio;

/// <summary>
/// Verification reads of the requests a check records (<see cref="GameplayAudioRequestsAccess2"/>), and of the requests each
/// system has made through the requests it joins: its own, or, once a runtime points it there, the runtime's, which every
/// system shares (#1620).
/// </summary>
internal static class GameplayAudioRequestsAccess
{
    /// <summary>Builds requests that go straight on into a check's queues, recorded from the start.</summary>
    extension(GameplayAudioRequests)
    {
        /// <summary>
        /// Requests that go straight on into <paramref name="queues"/>, as a selected game's do, recorded from the start
        /// until the check begins an update.
        /// </summary>
        internal static GameplayAudioRequests Reaching(AudioQueues queues)
        {
            var session = new SessionGameplayAudioQueues(queues, NoAttractDemo.Instance);
            return new(new RecordingSoundQueue(session), new RecordingMusicQueue(session));
        }

        /// <summary>
        /// Requests of a check's own, which go straight on into standalone queues (#2057): what a system a check builds
        /// outside a runtime joins.
        /// </summary>
        internal static GameplayAudioRequests Standalone() => Reaching(AudioQueues.Standalone());
    }

    /// <summary>Reads the recorded music requests by kind.</summary>
    extension(GameplayAudioRequests self)
    {
        /// <summary>The update's scheduled music requests, in request order.</summary>
        internal IReadOnlyList<DelayedMusicRequest> ScheduledMusic =>
            [.. self.MusicRequests.OfType<GameplayMusicRequest.Scheduled>().Select(scheduled => scheduled.Request)];

        /// <summary>How many permanent-item fanfares the update started.</summary>
        internal int PermanentItemFanfares => self.MusicRequests.OfType<GameplayMusicRequest.PermanentItemFanfare>().Count();
    }

    /// <summary>Counts the recorded sounds.</summary>
    extension(GameplaySoundRequests self)
    {
        /// <summary>How many times the update requested <paramref name="sound"/>.</summary>
        internal int CountOf(SoundEffectId sound) => self.SoundRequests.Count(request => request.SoundEffect == sound);
    }

    /// <summary>Reads the requests the enemies have queued.</summary>
    extension(EnemyAudioQueue self)
    {
        /// <summary>The recorded sound requests of the requests the enemies' queue joins, in request order.</summary>
        internal IReadOnlyList<SoundRequest> SoundRequests => self.Requests.SoundRequests;

        /// <summary>The recorded scheduled music requests of the requests the enemies' queue joins, in request order.</summary>
        internal IReadOnlyList<DelayedMusicRequest> MusicRequests => self.Requests.ScheduledMusic;

        /// <summary>Begins a check's recorded update of the enemies' audio: the earlier requests leave the record.</summary>
        internal void BeginFrame() => self.Requests.BeginRecordedUpdate();
    }

    /// <summary>Begins an update of a room's enemy audio.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>Begins a check's recorded update of the enemies' audio: the earlier requests leave the record.</summary>
        internal void BeginEnemySoundRequestFrame() => self.Audio.Requests.BeginRecordedUpdate();
    }

    /// <summary>Reads the requests the room objects have queued.</summary>
    extension(RoomObjectSoundQueue self)
    {
        /// <summary>The recorded sound requests of the requests the room objects' queue joins, in request order.</summary>
        internal IReadOnlyList<SoundRequest> SoundRequests => self.Requests.SoundRequests;

        /// <summary>The recorded scheduled music requests of the requests the room objects' queue joins, in request order.</summary>
        internal IReadOnlyList<DelayedMusicRequest> MusicRequests => self.Requests.ScheduledMusic;
    }

    /// <summary>Reads Samus's movement and liquid sound requests and begins a check's update of them.</summary>
    extension(SamusLiquidPhysicsState self)
    {
        /// <summary>The recorded sound requests of the requests Samus's sounds join, in request order.</summary>
        internal IReadOnlyList<SoundRequest> SoundRequests => self.AudioRequests.SoundRequests;

        /// <summary>Begins a check's recorded update of Samus's audio: the earlier requests leave the record.</summary>
        internal void BeginFrameSoundRequests() => self.AudioRequests.BeginRecordedUpdate();
    }

    /// <summary>The state of a check's game, which no attract demo drives.</summary>
    private sealed class NoAttractDemo : IAttractDemoState
    {
        /// <summary>The one instance.</summary>
        internal static NoAttractDemo Instance { get; } = new();

        /// <inheritdoc/>
        public bool IsAttractDemo => false;
    }
}
