using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Translation of the complete bank-$84 n00b-tube PLM coroutine shared by both states of
/// room $CEFB. The implementation follows its ROM list and callbacks instead of keying on
/// the room identity, so the same actor also behaves correctly in faithful test fixtures.
/// </summary>
public sealed partial class RoomPlmSystem
{
    /// <summary>Room-side writer used by the tube's script to set the earthquake type.</summary>
    private Action<ushort>? _writeNoobTubeEarthquakeType;
    /// <summary>Projectile factory that creates the tube crack, debris, and released air bubbles.</summary>
    private Action<NoobTubeProjectileRequest>? _spawnNoobTubeProjectile;
    /// <summary>Room liquid-physics state enabled when the tube breaks and releases water.</summary>
    private RoomLayer3FxState? _noobTubeRoomFx;

    /// <summary>Clears the per-room callbacks and liquid state captured for the tube PLM.</summary>
    private void ResetNoobTubeState()
    {
        _writeNoobTubeEarthquakeType = null;
        _spawnNoobTubeProjectile = null;
        _noobTubeRoomFx = null;
    }

    /// <summary>Runs setup $84:D6CC by replacing the complete origin level word.</summary>
    private static void SetupNoobTubeSlot(RoomLevelData level, PlmSlot slot)
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
    private void RunNoobTubePreInstruction(PlmSlot slot, ushort controllerNewInput)
    {
        if (slot.HeaderPointer != RoomPlmHeaders.NoobTube || slot.PreInstruction == 0)
            return;

        switch (slot.PreInstruction)
        {
            case NoobTubePlmRomData.InactivePreInstruction:
                return;

            case NoobTubePlmRomData.WakeOnPowerBombPreInstruction:
                SamusProjectileFamily projectileFamily =
                    new SamusProjectileTypeWord(slot.LoopTimer).Family;
                if (projectileFamily == SamusProjectileFamily.PowerBomb)
                {
                    slot.InstructionPointer = slot.LinkInstruction;
                    slot.InstructionTimer = 1;
                }
                else if (slot.LoopTimer != 0)
                {
                    _soundRequests.Add(CreateSoundRequest(
                        SoundEffectId.FromCartridge(
                            SoundEffectLibrary.Library2,
                            NoobTubePlmRomData.IneffectiveShotSound),
                        MaximumQueued: 6));
                }
                slot.LoopTimer = 0;
                return;

            case NoobTubePlmRomData.WakeOnAcceptedInputPreInstruction:
                if ((controllerNewInput & NoobTubePlmRomData.AcceptedWakeInputMask) != 0)
                {
                    slot.InstructionPointer = slot.LinkInstruction;
                    slot.InstructionTimer = 1;
                }
                return;

            default:
                throw new InvalidDataException(
                    $"N00b tube has invalid pre-instruction $84:{slot.PreInstruction:X4}.");
        }
    }

    /// <summary>Executes a recognized bank-$84 tube instruction and advances its program cursor.</summary>
    /// <param name="bus">Address space used to read operands from the active instruction stream.</param>
    /// <param name="slot">Tube PLM slot whose instruction pointer and gameplay effects may be updated.</param>
    /// <param name="instruction">Opcode already fetched from the slot's current instruction pointer.</param>
    /// <returns><see langword="true"/> if this method handled the opcode; otherwise leaves it for another instruction handler.</returns>
    private bool TryExecuteNoobTubeInstruction(
        ISnesAddressSpace bus,
        PlmSlot slot,
        ushort instruction)
    {
        if (slot.HeaderPointer != RoomPlmHeaders.NoobTube)
            return false;

        ushort cursor = slot.InstructionPointer;
        switch (instruction)
        {
            case RoomPlmInstructionCodes.GotoIfEventSet:
                ushort eventNumber = ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                if (eventNumber != (ushort)NoobTubePlmRomData.BrokenEvent)
                {
                    throw new InvalidDataException(
                        $"N00b tube referenced event ${eventNumber:X4}, not event $000B.");
                }
                ushort target = ReadProgramWord(bus, unchecked((ushort)(cursor + 4)));
                slot.InstructionPointer = RequireNoobTubeEventReader()(NoobTubePlmRomData.BrokenEvent)
                    ? target
                    : unchecked((ushort)(cursor + 6));
                return true;

            case RoomPlmInstructionCodes.LinkInstruction:
                slot.LinkInstruction = ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            case RoomPlmInstructionCodes.ClearPreInstruction:
                slot.PreInstruction = NoobTubePlmRomData.InactivePreInstruction;
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstructionCodes.LockSamus:
                RequireNoobTubeSamus().SetStationaryScriptControlLock(true);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstructionCodes.UnlockSamus:
                RequireNoobTubeSamus().SetStationaryScriptControlLock(false);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstructionCodes.SpawnNoobTubeCrack:
                SpawnNoobTubeProjectile(slot, NoobTubePlmRomData.CrackProjectile, 0);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstructionCodes.SpawnNoobTubeShardsAndBubbles:
                for (ushort parameter = 0; parameter <= 0x12; parameter += 2)
                    SpawnNoobTubeProjectile(slot, NoobTubePlmRomData.ShardProjectile, parameter);
                for (ushort parameter = 0; parameter <= 0x0a; parameter += 2)
                    SpawnNoobTubeProjectile(slot, NoobTubePlmRomData.ReleasedAirBubbleProjectile, parameter);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstructionCodes.TriggerNoobTubeEarthquake:
                (_writeNoobTubeEarthquakeType ?? throw new InvalidOperationException(
                    "N00b tube has no earthquake-type writer."))(NoobTubePlmRomData.EarthquakeType);
                (_writeEarthquakeTimer ?? throw new InvalidOperationException(
                    "N00b tube has no earthquake-timer writer."))(NoobTubePlmRomData.EarthquakeTimer);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstructionCodes.SetEvent:
                ushort setEvent = ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                if (setEvent != (ushort)NoobTubePlmRomData.BrokenEvent)
                {
                    throw new InvalidDataException(
                        $"N00b tube attempted to set event ${setEvent:X4}, not event $000B.");
                }
                (_setEvent ?? throw new InvalidOperationException(
                    "N00b tube has no event writer."))(NoobTubePlmRomData.BrokenEvent);
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            case RoomPlmInstructionCodes.EnableNoobTubeWaterPhysics:
                RoomLayer3FxState roomFx = _noobTubeRoomFx
                    ?? throw new InvalidOperationException(
                        "N00b tube has no room-FX owner for its water-physics write.");
                roomFx.EnableWaterPhysics();
                SamusState samus = RequireNoobTubeSamus();
                roomFx.ApplyToSamusLiquidPhysics(samus.LiquidPhysics);
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            default:
                return false;
        }
    }

    /// <summary>Gets the event-state query required to test whether the tube's break event is set.</summary>
    /// <returns>The room event reader installed for the current PLM update.</returns>
    /// <exception cref="InvalidOperationException">No event reader is available for the tube.</exception>
    private Func<EventNumber, bool> RequireNoobTubeEventReader() =>
        _hasEvent ?? throw new InvalidOperationException("N00b tube has no event reader.");

    /// <summary>Gets the active Samus actor whose control or liquid physics the tube script changes.</summary>
    /// <returns>The active Samus state supplied to the room PLM system.</returns>
    /// <exception cref="InvalidOperationException">The tube requires Samus, but no active actor is available.</exception>
    private SamusState RequireNoobTubeSamus() =>
        _collectibleSamus?.Invoke() ?? throw new InvalidOperationException(
            "N00b tube has no active Samus actor.");

    /// <summary>Creates a tube projectile at the block occupied by its PLM slot.</summary>
    /// <param name="slot">PLM slot supplying the projectile's room block origin.</param>
    /// <param name="definition">Cartridge projectile definition selected for the crack, shard, or bubble.</param>
    /// <param name="parameter">Native projectile parameter forwarded unchanged to the projectile factory.</param>
    private void SpawnNoobTubeProjectile(PlmSlot slot, ushort definition, ushort parameter) =>
        (_spawnNoobTubeProjectile ?? throw new InvalidOperationException(
            "N00b tube has no enemy-projectile spawner."))(
            new NoobTubeProjectileRequest(definition, parameter, slot.BlockIndex));

}
