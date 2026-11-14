using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Audio;

/// <summary>
/// Verification reads of an update's <see cref="GameplayAudioRequests"/>, and of the requests each system has queued into
/// the record it joins: its own, or, once a runtime points it there, the runtime's, which every system shares (#1620).
/// </summary>
internal static class GameplayAudioRequestsAccess
{
    /// <summary>Builds a record whose requests go straight on into a check's queues.</summary>
    extension(GameplayAudioRequests)
    {
        /// <summary>A record whose requests go straight on into <paramref name="queues"/>, as a selected game's do.</summary>
        internal static GameplayAudioRequests Reaching(AudioQueues queues)
        {
            var requests = new GameplayAudioRequests();
            requests.AttachTo(new SessionGameplayAudioQueues(queues, NoAttractDemo.Instance));
            return requests;
        }
    }

    /// <summary>Reads the update's music requests by kind and counts its sounds.</summary>
    extension(GameplayAudioRequests self)
    {
        /// <summary>The update's scheduled music requests, in request order.</summary>
        internal IReadOnlyList<DelayedMusicRequest> ScheduledMusic =>
            [.. self.MusicRequests.OfType<GameplayMusicRequest.Scheduled>().Select(scheduled => scheduled.Request)];

        /// <summary>How many permanent-item fanfares the update started.</summary>
        internal int PermanentItemFanfares => self.MusicRequests.OfType<GameplayMusicRequest.PermanentItemFanfare>().Count();

        /// <summary>How many times the update requested <paramref name="sound"/>.</summary>
        internal int CountOf(SoundEffectId sound) => self.SoundRequests.Count(request => request.SoundEffect == sound);
    }

    /// <summary>Reads the requests the enemies have queued.</summary>
    extension(EnemyAudioQueue self)
    {
        /// <summary>The update's sound requests in the record the enemies' queue joins, in request order.</summary>
        internal IReadOnlyList<SoundRequest> SoundRequests => self.Requests.SoundRequests;

        /// <summary>The update's scheduled music requests in the record the enemies' queue joins, in request order.</summary>
        internal IReadOnlyList<DelayedMusicRequest> MusicRequests => self.Requests.ScheduledMusic;

        /// <summary>Begins a check's update of the enemies' audio: the earlier requests leave the record.</summary>
        internal void BeginFrame() => self.Requests.BeginUpdate();
    }

    /// <summary>Begins an update of a room's enemy audio.</summary>
    extension(RoomEnemySystem self)
    {
        /// <summary>Begins a check's update of the enemies' audio: the earlier requests leave the record their queue joins.</summary>
        internal void BeginEnemySoundRequestFrame() => self.Audio.Requests.BeginUpdate();
    }

    /// <summary>Reads the requests the room objects have queued.</summary>
    extension(RoomObjectSoundQueue self)
    {
        /// <summary>The update's sound requests in the record the room objects' queue joins, in request order.</summary>
        internal IReadOnlyList<SoundRequest> SoundRequests => self.Requests.SoundRequests;

        /// <summary>The update's scheduled music requests in the record the room objects' queue joins, in request order.</summary>
        internal IReadOnlyList<DelayedMusicRequest> MusicRequests => self.Requests.ScheduledMusic;
    }

    /// <summary>Reads the effect overlay's sound requests.</summary>
    extension(RoomFxOverlayState self)
    {
        /// <summary>The update's sound requests in the record the effect's sounds join, in request order.</summary>
        internal IReadOnlyList<SoundRequest> SoundRequests => self.AudioRequests.SoundRequests;
    }

    /// <summary>Reads the colour effects' sound requests.</summary>
    extension(ColourEffectSystem self)
    {
        /// <summary>The update's sound requests in the record the colour effects' sounds join, in request order.</summary>
        internal IReadOnlyList<SoundRequest> SoundRequests => self.AudioRequests.SoundRequests;
    }

    /// <summary>Reads Samus's movement and liquid sound requests and begins a check's update of them.</summary>
    extension(SamusLiquidPhysicsState self)
    {
        /// <summary>The update's sound requests in the record Samus's sounds join, in request order.</summary>
        internal IReadOnlyList<SoundRequest> SoundRequests => self.AudioRequests.SoundRequests;

        /// <summary>
        /// Begins a check's update of Samus's audio, as an animation call outside the runtime does: the earlier requests
        /// leave her record.
        /// </summary>
        internal void BeginFrameSoundRequests() => self.AudioRequests.BeginUpdate();
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
