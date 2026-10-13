using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>The Maridia glass tube (N00b tube) PLM: its crack and shatter, earthquake, layer-3 FX and projectile spawns.</summary>
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
        slot.InstructionPointer = RoomPlmInstructionLists.NoobTube;
    }

    /// <summary>Runs either of the two callbacks that can own the sleeping tube actor.</summary>
    internal void RunNoobTubePreInstruction(PlmSlot slot, ushort controllerNewInput)
    {
        if (slot.HeaderPointer != PlmHeaderId.NoobTube || slot.PreInstruction == 0)
            return;

        NoobTubePlmPreInstruction preInstruction = ClosedNativeWords.Decode<NoobTubePlmPreInstruction>(
            slot.PreInstruction, "n00b-tube pre-instruction");
        switch (preInstruction)
        {
            case NoobTubePlmPreInstruction.Inactive:
                return;

            case NoobTubePlmPreInstruction.WakeOnPowerBomb:
                SamusProjectileFamily projectileFamily =
                    new SamusProjectileTypeWord(slot.LoopTimer).Family;
                if (projectileFamily == SamusProjectileFamily.PowerBomb)
                {
                    slot.InstructionPointer = slot.LinkInstruction;
                    slot.InstructionTimer = 1;
                }
                else if (slot.LoopTimer != 0)
                {
                    _sounds._soundRequests.Add(_sounds.CreateSoundRequest(
                        SoundEffectId.FromCartridge(
                            SoundEffectLibrary.Library2,
                            NoobTubePlmRomData.IneffectiveShotSound),
                        MaximumQueued: 6));
                }
                slot.LoopTimer = 0;
                return;

            case NoobTubePlmPreInstruction.WakeOnAcceptedInput:
                if ((controllerNewInput & NoobTubePlmRomData.AcceptedWakeInputMask) != 0)
                {
                    slot.InstructionPointer = slot.LinkInstruction;
                    slot.InstructionTimer = 1;
                }
                return;

            default:
                throw new InvalidOperationException($"Undefined n00b-tube pre-instruction {preInstruction}.");
        }
    }

    /// <summary>Executes tube-only instructions, including crack, shatter, event, water, and control-lock effects.</summary>
    internal bool TryExecuteNoobTubeInstruction(
        ISnesAddressSpace bus,
        PlmSlot slot,
        RoomPlmInstruction instruction)
    {
        if (slot.HeaderPointer != PlmHeaderId.NoobTube)
            return false;

        ushort cursor = slot.InstructionPointer;
        if (instruction is not (
            RoomPlmInstruction.GotoIfEventSet or
            RoomPlmInstruction.LinkInstruction or
            RoomPlmInstruction.ClearPreInstruction or
            RoomPlmInstruction.LockSamus or
            RoomPlmInstruction.UnlockSamus or
            RoomPlmInstruction.SpawnNoobTubeCrack or
            RoomPlmInstruction.SpawnNoobTubeShardsAndBubbles or
            RoomPlmInstruction.TriggerNoobTubeEarthquake or
            RoomPlmInstruction.SetEvent or
            RoomPlmInstruction.EnableNoobTubeWaterPhysics))
            return false;

        switch (instruction)
        {
            case RoomPlmInstruction.GotoIfEventSet:
                ushort eventNumber = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                if (eventNumber != (ushort)NoobTubePlmRomData.BrokenEvent)
                {
                    throw new InvalidDataException(
                        $"N00b tube referenced event ${eventNumber:X4}, not event $000B.");
                }
                ushort target = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 4)));
                slot.InstructionPointer = RequireNoobTubeEvents().Has(NoobTubePlmRomData.BrokenEvent)
                    ? target
                    : unchecked((ushort)(cursor + 6));
                return true;

            case RoomPlmInstruction.LinkInstruction:
                slot.LinkInstruction = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            case RoomPlmInstruction.ClearPreInstruction:
                slot.PreInstruction = (ushort)NoobTubePlmPreInstruction.Inactive;
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstruction.LockSamus:
                RequireNoobTubeSamus().ScriptLocks.SetStationaryScriptControlLock(true);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstruction.UnlockSamus:
                RequireNoobTubeSamus().ScriptLocks.SetStationaryScriptControlLock(false);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstruction.SpawnNoobTubeCrack:
                SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.Crack, 0);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstruction.SpawnNoobTubeShardsAndBubbles:
                for (ushort parameter = 0; parameter <= 0x12; parameter += 2)
                    SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.Shard, parameter);
                for (ushort parameter = 0; parameter <= 0x0a; parameter += 2)
                    SpawnNoobTubeProjectile(slot, NoobTubeProjectileDefinition.ReleasedAirBubble, parameter);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstruction.TriggerNoobTubeEarthquake:
                (_bindings.Earthquake ?? throw new InvalidOperationException(
                    "N00b tube has no earthquake-type writer.")).SetType(NoobTubePlmRomData.EarthquakeType);
                (_bindings.Earthquake ?? throw new InvalidOperationException(
                    "N00b tube has no earthquake-timer writer.")).SetTimer(NoobTubePlmRomData.EarthquakeTimer);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstruction.SetEvent:
                ushort setEvent = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                if (setEvent != (ushort)NoobTubePlmRomData.BrokenEvent)
                {
                    throw new InvalidDataException(
                        $"N00b tube attempted to set event ${setEvent:X4}, not event $000B.");
                }
                (_bindings.Events ?? throw new InvalidOperationException(
                    "N00b tube has no event writer.")).Set(NoobTubePlmRomData.BrokenEvent);
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            case RoomPlmInstruction.EnableNoobTubeWaterPhysics:
                RoomLayer3FxState roomFx = _bindings.RoomFx
                    ?? throw new InvalidOperationException(
                        "N00b tube has no room-FX owner for its water-physics write.");
                roomFx.EnableWaterPhysics();
                SamusState samus = RequireNoobTubeSamus();
                roomFx.ApplyToSamusLiquidPhysics(samus.LiquidPhysics);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            default:
                throw new InvalidOperationException($"Unhandled RoomPlmInstruction {instruction}.");
        }
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
