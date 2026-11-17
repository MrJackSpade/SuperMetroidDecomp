using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's body animation start, room objects, dust and random numbers, and its sound and music requests.</summary>
[RoomLifetime]
internal sealed class CrocomireEncounterAccess
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Accesses the active room, enemy places, and shared RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Spawns Crocomire's attacks, dust, smoke, and post-death drops.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Receives Crocomire's sound and music requests.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireEncounterAccess(BossFrameResultBuilder frameResults, EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _frameResults = frameResults;
        _room = room;
        _projectiles = projectiles;
        _audio = audio;
    }

    /// <summary>Queues one of Crocomire's sounds as Crocomire makes it, after the update's earlier requests.</summary>
    internal void RequestCrocomireSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues Crocomire's music change as Crocomire makes it, after the update's earlier music requests.</summary>
    internal void RequestCrocomireMusic(MusicRequest request, MusicQueueDelay delay) =>
        _audio.QueueEnemyMusic(new DelayedMusicRequest(request, delay));

    /// <summary>Starts <paramref name="segment"/> on the body from its first step on the next update, as starting an animation does.</summary>
    internal static void StartCrocomireBodyAnimation(CrocomireEnemyState state, CrocomireBodySegment segment)
    {
        state.BodyPlayback = EnemyAnimationPlayback.Start(state.Body, CrocomireBodyAnimations.StartingWith(segment));
    }

    /// <summary>Queues a room-object block change for later room processing.</summary>
    internal void PublishCrocomireObject(Position16<RoomBlocks> block, CrocomireArenaMutation mutation) =>
        _frameResults.CrocomireObjects.Add(new RoomObjectSpawnRequest<CrocomireArenaMutation>(block, mutation));

    /// <summary>Draws the room's next random number for Crocomire effects and choices.</summary>
    internal int ReadCrocomireRandom() => _room.Random.Read();

    /// <summary>The session's installed gradient the volley's last shot reads past the authored nine.</summary>
    internal CrocomireTenthShotGradient TenthShotGradient => _room.Session.BossOverreads.CrocomireTenthShotGradient;

    /// <summary>Places a foot dust puff at the body offset with an additional three-bit RNG jitter.</summary>
    internal void SpawnCrocomireDust(CrocomireEnemyState state, int xOffset)
    {
        int random = ReadCrocomireRandom();
        _projectiles.SpawnDustOrExplosion(
            new(state.Body.Position.X.Whole + xOffset + CrocomireRandomDefinitions.DustJitter.Of(random), state.Body.Position.Y.Whole + state.Body.Radius.Y - CrocomireFightDefinitions.DustAboveBodyBottom),
            MiscDustAnimation.BigDustCloud);
    }
}
