using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The Maridia glass tube (N00b tube) PLM: its crack and shatter, earthquake, effect overlay FX and projectile spawns.</summary>
[RoomLifetime]
internal sealed class NoobTubeObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>The room's effect overlay FX.</summary>
    private readonly IRoomWaterPhysics? _waterPhysics;
    /// <summary>The room's earthquake.</summary>
    private readonly IRoomEarthquakeWriter? _earthquake;
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomObjectEnemyEffects? _enemies;
    /// <summary>Queue used for ineffective shot feedback while the tube waits.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Shares the room services and sound queue used by the tube PLM interpreter.</summary>
    internal NoobTubeObjects(IRoomSamusAccess? samus, IGlobalEventFlags? events, IRoomWaterPhysics? waterPhysics, IRoomEarthquakeWriter? earthquake, IRoomObjectEnemyEffects? enemies, RoomObjectSoundQueue sounds)
    {
        _samus = samus;
        _events = events;
        _waterPhysics = waterPhysics;
        _earthquake = earthquake;
        _enemies = enemies;
        _sounds = sounds;
    }

    /// <summary>Runs the tube's setup by replacing its whole origin block.</summary>
    internal static void SetupNoobTubeSlot(RoomLevelData level, RoomObjectSlot slot)
    {
        level.SetTerrain(
            slot.BlockIndex,
            NoobTubeObjectDefinitions.SolidProjectileTrigger);
        level.SetBehavior(
            slot.BlockIndex,
            RoomBlockBehaviorValues.ResidentObjectProjectileTrigger);
        slot.StartAnimation(new NoobTubeObjectAnimation(NoobTubeProgram.Main));
    }

    /// <summary>Runs either of the two callbacks that can own the sleeping tube actor.</summary>
    internal void RunNoobTubeFrameBehavior(RoomObjectSlot slot, NoobTubeObjectFrameBehavior frameBehavior, ControllerButton controllerNewInput)
    {
        switch (frameBehavior)
        {
            case NoobTubeObjectFrameBehavior.WakeOnPowerBomb:
                if (slot.ProjectileHit?.Family == SamusProjectileFamily.PowerBomb)
                {
                    slot.WakeAtLink();
                }
                else if (slot.ProjectileHit is not null)
                {
                    _sounds.Queue(
                        NoobTubeObjectDefinitions.IneffectiveShotSound,
                        Priority: SoundRequestPriority.Normal);
                }
                slot.ProjectileHit = null;
                return;

            case NoobTubeObjectFrameBehavior.WakeOnAcceptedInput:
                if ((controllerNewInput & NoobTubeObjectDefinitions.AcceptedWakeInputMask) != ControllerButton.None)
                {
                    slot.WakeAtLink();
                }
                return;

            case NoobTubeObjectFrameBehavior.Inactive:
            default:
                throw new InvalidOperationException($"Undefined n00b-tube pre-instruction {frameBehavior}.");
        }
    }

    /// <summary>Whether the tube was broken on an earlier visit (its event is set).</summary>
    internal bool IsBroken() => RequireNoobTubeEvents().Has(NoobTubeObjectDefinitions.BrokenEvent);

    /// <summary>Locks Samus in place for the break sequence.</summary>
    internal void LockSamus() => RequireNoobTubeSamus().Control.LockStationary();

    /// <summary>Releases Samus once the break sequence ends.</summary>
    internal void UnlockSamus() => RequireNoobTubeSamus().Control.Unlock();

    /// <summary>Spawns the crack projectile at the tube.</summary>
    internal void SpawnCrack(RoomObjectSlot slot) => SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.Crack, piece: null);

    /// <summary>Spawns the ten glass shards and then the six released-air bubbles, each naming its place in the burst.</summary>
    internal void SpawnShardsAndBubbles(RoomObjectSlot slot)
    {
        for (int shard = 0; shard < NoobTubeObjectDefinitions.ShardCount; shard++)
            SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.Shard, shard);
        for (int bubble = 0; bubble < NoobTubeObjectDefinitions.BubbleCount; bubble++)
            SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.ReleasedAirBubble, bubble);
    }

    /// <summary>Starts the break earthquake.</summary>
    internal void TriggerEarthquake()
    {
        (_earthquake ?? throw new InvalidOperationException(
            "N00b tube has no earthquake-type writer.")).SetType(NoobTubeObjectDefinitions.EarthquakeType);
        _earthquake.SetTimer(NoobTubeObjectDefinitions.EarthquakeTimer);
    }

    /// <summary>Sets the tube's broken event.</summary>
    internal void SetBrokenEvent() =>
        (_events ?? throw new InvalidOperationException(
            "N00b tube has no event writer.")).Set(NoobTubeObjectDefinitions.BrokenEvent);

    /// <summary>Enables the room's water physics, Samus's included.</summary>
    internal void EnableWaterPhysics() =>
        (_waterPhysics ?? throw new InvalidOperationException(
            "N00b tube has no room water physics for its water-physics write.")).EnableWaterPhysics();

    /// <summary>Requires the room event owner before testing the broken-tube event.</summary>
    private IGlobalEventFlags RequireNoobTubeEvents() =>
        _events ?? throw new InvalidOperationException("N00b tube has no event reader.");

    /// <summary>Requires a live Samus actor for tube script locks or water-physics publication.</summary>
    private IRoomSamus RequireNoobTubeSamus() =>
        _samus?.Current ?? throw new InvalidOperationException(
            "N00b tube has no active Samus actor.");

    /// <summary>Routes a crack, shard, or bubble spawn to the enemy system at the tube's block.</summary>
    private void SpawnNoobTubeProjectile(RoomObjectSlot slot, NoobTubeProjectileDefinition definition, int? piece) =>
        (_enemies ?? throw new InvalidOperationException(
            "N00b tube has no enemy-projectile spawner.")).SpawnNoobTubeProjectile(
            new NoobTubeProjectileRequest(definition, piece, slot.BlockIndex));
}
