using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's state lookups, body program start, PLMs, dust and random numbers, and the sound and music it last requested.</summary>
[RoomLifetime]
internal sealed class CrocomireEncounterAccess
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyFrameResultBuilder _frameResults;
    /// <summary>Accesses the active room, enemy slots, and shared RNG.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Spawns Crocomire's attacks, dust, smoke, and post-death drops.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal CrocomireEncounterAccess(EnemyFrameResultBuilder frameResults, EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _frameResults = frameResults;
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>Debugger-visible Crocomire owner while the current room contains $DDBF.</summary>
    public CrocomireEnemyState? Crocomire { get; internal set; }

    /// <summary>Typed WRAM extension used by Crocomire's bridge/melting/skeleton graph.</summary>
    public CrocomireDeathState? CrocomireDeath { get; internal set; }

    /// <summary>Delayed music publication from the current Crocomire frame.</summary>
    public DelayedMusicRequest? LastCrocomireMusicRequest { get; internal set; }

    /// <summary>Most recent library-two sound publication of the current frame.</summary>
    public SoundEffectId? LastCrocomireSoundEffect { get; internal set; }

    /// <summary>Starts <paramref name="program"/> on the body from its first step on the next update, as a native list install does.</summary>
    internal static void StartCrocomireBodyProgram(CrocomireEnemyState state, CrocomireBodyProgram program)
    {
        state.BodyCursor = AnimationProgramCursor<CrocomireBodyProgram>.Start(program);
        state.Body.InstructionTimer = NativeCountdown.Start(1);
        state.Body.Timer = NativeCountdown.Start(0);
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

    /// <summary>Queues a native PLM block change for later room processing.</summary>
    internal void PublishCrocomirePlm(Position16<RoomBlocks> block, PlmHeaderId header) =>
        _frameResults.CrocomirePlms.Add(new PlmSpawnRequest(block, header));

    /// <summary>Consumes the room's next RNG word for Crocomire effects and choices.</summary>
    internal int ReadCrocomireRandom() => _room.Random.Read();

    /// <summary>Places a foot dust puff at the body offset with an additional three-bit RNG jitter.</summary>
    internal void SpawnCrocomireDust(CrocomireEnemyState state, int xOffset)
    {
        int random = ReadCrocomireRandom();
        _projectiles.SpawnRoomGraphicsDustExplosion(
            new(unchecked((ushort)(state.Body.Position.X.Whole + xOffset + (random & 7))), unchecked((ushort)(state.Body.Position.Y.Whole + state.Body.Radius.Y - 16))),
            MiscDustAnimation.BigDustCloud);
    }
}
