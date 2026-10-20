using SuperMetroid.Core.Input;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The Maridia glass tube (N00b tube) PLM: its crack and shatter, earthquake, layer-3 FX and projectile spawns.</summary>
[RoomLifetime]
internal sealed class NoobTubePlms
{
    /// <summary>Room services receiving tube event, Samus, FX, and projectile effects.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Queue used for ineffective shot feedback while the tube waits.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Shares the room service bindings and sound queue used by the tube PLM interpreter.</summary>
    internal NoobTubePlms(PlmRoomBindings bindings, PlmSoundQueue sounds)
    {
        _bindings = bindings;
        _sounds = sounds;
    }

    /// <summary>Runs setup $84:D6CC by replacing the complete origin level word.</summary>
    internal static void SetupNoobTubeSlot(RoomLevelData level, PlmSlot slot)
    {
        level.SetForegroundEntry(
            slot.BlockIndex,
            NoobTubePlmRomData.SolidProjectileTriggerLevelWord);
        level.SetBehavior(
            slot.BlockIndex,
            RoomBlockBehaviorValues.ResidentPlmProjectileTrigger);
        slot.StartAnimation(new NoobTubePlmAnimation(NoobTubeProgram.Main));
    }

    /// <summary>Runs either of the two callbacks that can own the sleeping tube actor.</summary>
    internal void RunNoobTubePreInstruction(PlmSlot slot, NoobTubePlmPreInstruction preInstruction, SnesButton controllerNewInput)
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
                        SoundEffectId.FromCartridge(
                            SoundEffectLibrary.Library2,
                            NoobTubePlmRomData.IneffectiveShotSound),
                        MaximumQueued: 6);
                }
                slot.LoopTimer = 0;
                return;

            case NoobTubePlmPreInstruction.WakeOnAcceptedInput:
                if ((controllerNewInput & NoobTubePlmRomData.AcceptedWakeInputMask) != 0)
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
    internal bool IsBroken() => RequireNoobTubeEvents().Has(NoobTubePlmRomData.BrokenEvent);

    /// <summary>Locks or unlocks Samus for the break sequence.</summary>
    internal void SetSamusLocked(bool locked)
    {
        SamusControl control = RequireNoobTubeSamus().Control;
        if (locked)
            control.LockStationary();
        else
            control.Unlock();
    }

    /// <summary>Spawns the crack projectile at the tube.</summary>
    internal void SpawnCrack(PlmSlot slot) => SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.Crack, 0);

    /// <summary>Spawns the ten glass shards and six released-air bubbles, each with its even native parameter.</summary>
    internal void SpawnShardsAndBubbles(PlmSlot slot)
    {
        for (int shard = 0; shard < NoobTubePlmRomData.ShardCount; shard++)
            SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.Shard, (ushort)(shard * 2));
        for (int bubble = 0; bubble < NoobTubePlmRomData.BubbleCount; bubble++)
            SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.ReleasedAirBubble, (ushort)(bubble * 2));
    }

    /// <summary>Starts the break earthquake.</summary>
    internal void TriggerEarthquake()
    {
        (_bindings.Earthquake ?? throw new InvalidOperationException(
            "N00b tube has no earthquake-type writer.")).SetType(NoobTubePlmRomData.EarthquakeType);
        (_bindings.Earthquake ?? throw new InvalidOperationException(
            "N00b tube has no earthquake-timer writer.")).SetTimer(NoobTubePlmRomData.EarthquakeTimer);
    }

    /// <summary>Sets the tube's broken event.</summary>
    internal void SetBrokenEvent() =>
        (_bindings.Events ?? throw new InvalidOperationException(
            "N00b tube has no event writer.")).Set(NoobTubePlmRomData.BrokenEvent);

    /// <summary>Enables the room's water physics and applies them to Samus.</summary>
    internal void EnableWaterPhysics()
    {
        RoomLayer3FxState roomFx = _bindings.RoomFx
            ?? throw new InvalidOperationException(
                "N00b tube has no room-FX owner for its water-physics write.");
        roomFx.EnableWaterPhysics();
        roomFx.ApplyToSamusLiquidPhysics(RequireNoobTubeSamus().LiquidPhysics);
    }

    /// <summary>Requires the room event owner before testing the broken-tube event.</summary>
    private IGlobalEventFlags RequireNoobTubeEvents() =>
        _bindings.Events ?? throw new InvalidOperationException("N00b tube has no event reader.");

    /// <summary>Requires a live Samus actor for tube script locks or water-physics publication.</summary>
    private SamusState RequireNoobTubeSamus() =>
        _bindings.Samus?.Current ?? throw new InvalidOperationException(
            "N00b tube has no active Samus actor.");

    /// <summary>Routes a crack, shard, or bubble spawn to the enemy system at the tube's block.</summary>
    private void SpawnNoobTubeProjectile(PlmSlot slot, NoobTubeProjectileDefinition definition, ushort parameter) =>
        (_bindings.Enemies ?? throw new InvalidOperationException(
            "N00b tube has no enemy-projectile spawner.")).SpawnNoobTubeProjectile(
            new NoobTubeProjectileRequest(definition, parameter, slot.BlockIndex));
}
