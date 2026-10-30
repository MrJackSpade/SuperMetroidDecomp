using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The Maridia glass tube (N00b tube) PLM: its crack and shatter, earthquake, layer-3 FX and projectile spawns.</summary>
[RoomLifetime]
internal sealed class NoobTubePlms
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The game's event flags.</summary>
    private readonly IGlobalEventFlags? _events;
    /// <summary>The room's layer-3 FX.</summary>
    private readonly IRoomWaterPhysics? _waterPhysics;
    /// <summary>The room's earthquake.</summary>
    private readonly IRoomEarthquakeWriter? _earthquake;
    /// <summary>The enemy effects room objects trigger.</summary>
    private readonly IRoomPlmEnemyEffects? _enemies;
    /// <summary>Queue used for ineffective shot feedback while the tube waits.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Shares the room services and sound queue used by the tube PLM interpreter.</summary>
    internal NoobTubePlms(IRoomSamusAccess? samus, IGlobalEventFlags? events, IRoomWaterPhysics? waterPhysics, IRoomEarthquakeWriter? earthquake, IRoomPlmEnemyEffects? enemies, PlmSoundQueue sounds)
    {
        _samus = samus;
        _events = events;
        _waterPhysics = waterPhysics;
        _earthquake = earthquake;
        _enemies = enemies;
        _sounds = sounds;
    }

    /// <summary>Runs the tube's setup by replacing its whole origin block.</summary>
    internal static void SetupNoobTubeSlot(RoomLevelData level, PlmSlot slot)
    {
        level.SetTerrain(
            slot.BlockIndex,
            NoobTubePlmDefinitions.SolidProjectileTrigger);
        level.SetBehavior(
            slot.BlockIndex,
            RoomBlockBehaviorValues.ResidentPlmProjectileTrigger);
        slot.StartAnimation(new NoobTubePlmAnimation(NoobTubeProgram.Main));
    }

    /// <summary>Runs either of the two callbacks that can own the sleeping tube actor.</summary>
    internal void RunNoobTubePreInstruction(PlmSlot slot, NoobTubePlmPreInstruction preInstruction, ControllerButton controllerNewInput)
    {
        switch (preInstruction)
        {
            case NoobTubePlmPreInstruction.WakeOnPowerBomb:
                SamusProjectileFamily projectileFamily =
                    new SamusProjectileTypeWord(checked((ushort)slot.LoopTimer)).Family;
                if (projectileFamily == SamusProjectileFamily.PowerBomb)
                {
                    slot.WakeAtLink();
                }
                else if (slot.LoopTimer != 0)
                {
                    _sounds.Queue(
                        NoobTubePlmDefinitions.IneffectiveShotSound,
                        MaximumQueued: SoundQueueLimit.Max6);
                }
                slot.LoopTimer = 0;
                return;

            case NoobTubePlmPreInstruction.WakeOnAcceptedInput:
                if ((controllerNewInput & NoobTubePlmDefinitions.AcceptedWakeInputMask) != ControllerButton.None)
                {
                    slot.WakeAtLink();
                }
                return;

            case NoobTubePlmPreInstruction.Inactive:
            default:
                throw new InvalidOperationException($"Undefined n00b-tube pre-instruction {preInstruction}.");
        }
    }

    /// <summary>Whether the tube was broken on an earlier visit (its event is set).</summary>
    internal bool IsBroken() => RequireNoobTubeEvents().Has(NoobTubePlmDefinitions.BrokenEvent);

    /// <summary>Locks Samus in place for the break sequence.</summary>
    internal void LockSamus() => RequireNoobTubeSamus().Control.LockStationary();

    /// <summary>Releases Samus once the break sequence ends.</summary>
    internal void UnlockSamus() => RequireNoobTubeSamus().Control.Unlock();

    /// <summary>Spawns the crack projectile at the tube.</summary>
    internal void SpawnCrack(PlmSlot slot) => SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.Crack, 0);

    /// <summary>Spawns the ten glass shards and six released-air bubbles, each with its even native parameter.</summary>
    internal void SpawnShardsAndBubbles(PlmSlot slot)
    {
        for (int shard = 0; shard < NoobTubePlmDefinitions.ShardCount; shard++)
            SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.Shard, shard * 2);
        for (int bubble = 0; bubble < NoobTubePlmDefinitions.BubbleCount; bubble++)
            SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.ReleasedAirBubble, bubble * 2);
    }

    /// <summary>Starts the break earthquake.</summary>
    internal void TriggerEarthquake()
    {
        (_earthquake ?? throw new InvalidOperationException(
            "N00b tube has no earthquake-type writer.")).SetType(NoobTubePlmDefinitions.EarthquakeType);
        _earthquake.SetTimer(NoobTubePlmDefinitions.EarthquakeTimer);
    }

    /// <summary>Sets the tube's broken event.</summary>
    internal void SetBrokenEvent() =>
        (_events ?? throw new InvalidOperationException(
            "N00b tube has no event writer.")).Set(NoobTubePlmDefinitions.BrokenEvent);

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
    private void SpawnNoobTubeProjectile(PlmSlot slot, NoobTubeProjectileDefinition definition, int parameter) =>
        (_enemies ?? throw new InvalidOperationException(
            "N00b tube has no enemy-projectile spawner.")).SpawnNoobTubeProjectile(
            new NoobTubeProjectileRequest(definition, parameter, slot.BlockIndex));
}
