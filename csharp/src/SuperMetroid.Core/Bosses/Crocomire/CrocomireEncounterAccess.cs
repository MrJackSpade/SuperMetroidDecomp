using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's state lookups, body program start, room objects, dust and random numbers, and its sound and music requests.</summary>
[RoomLifetime]
internal sealed class CrocomireEncounterAccess
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Accesses the active room, enemy slots, and shared RNG.</summary>
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

    /// <summary>Crocomire's state while the current room contains Crocomire.</summary>
    public CrocomireEnemyState? Crocomire { get; internal set; }

    /// <summary>Typed extended state used by Crocomire's bridge/melting/skeleton graph.</summary>
    public CrocomireDeathState? CrocomireDeath { get; internal set; }

    /// <summary>Queues one of Crocomire's sounds as Crocomire makes it, after the update's earlier requests.</summary>
    internal void RequestCrocomireSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues Crocomire's music change as Crocomire makes it, after the update's earlier music requests.</summary>
    internal void RequestCrocomireMusic(MusicRequest request, MusicQueueDelay delay) =>
        _audio.QueueEnemyMusic(new DelayedMusicRequest(request, delay));

    /// <summary>Starts <paramref name="program"/> on the body from its first step on the next update, as a list install does.</summary>
    internal static void StartCrocomireBodyAnimation(CrocomireEnemyState state, CrocomireBodySegment program)
    {
        state.BodyPlayback = EnemyAnimationPlayback.Start(state.Body, CrocomireBodyAnimations.StartingWith(program));
    }

    /// <summary>Finds the Crocomire encounter owning an enemy slot, failing if its room state is absent.</summary>
    internal CrocomireEnemyState RequireCrocomire(RoomEnemySlot slot) =>
        Crocomire is { } state && ReferenceEquals(state.Body, slot)
            ? state
            : throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} has no initialized Crocomire body state.");

    /// <summary>Requires the active death-sequence state before processing skeleton or wall instructions.</summary>
    internal CrocomireDeathState RequireCrocomireDeath() =>
        CrocomireDeath ?? throw new InvalidOperationException(
            "Crocomire death extension is not initialized for the current room.");

    /// <summary>Queues a room-object block change for later room processing.</summary>
    internal void PublishCrocomireObject(Position16<RoomBlocks> block, CrocomireArenaMutation mutation) =>
        _frameResults.CrocomireObjects.Add(new RoomObjectSpawnRequest<CrocomireArenaMutation>(block, mutation));

    /// <summary>Draws the room's next random number for Crocomire effects and choices.</summary>
    internal int ReadCrocomireRandom() => _room.Random.Read();

    /// <summary>The session's installed gradient the volley's last shot reads past the authored nine.</summary>
    internal CrocomireTenthShotGradient TenthShotGradient => _room.BossOverreads.CrocomireTenthShotGradient;

    /// <summary>Places a foot dust puff at the body offset with an additional three-bit RNG jitter.</summary>
    internal void SpawnCrocomireDust(CrocomireEnemyState state, int xOffset)
    {
        int random = ReadCrocomireRandom();
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(CoordinateAxis.Wrap(state.Body.Position.X.Whole + xOffset + CrocomireRandomDefinitions.DustJitter.Of(random)), CoordinateAxis.Wrap(state.Body.Position.Y.Whole + state.Body.Radius.Y - 16)),
            MiscDustAnimation.BigDustCloud);
    }
}
