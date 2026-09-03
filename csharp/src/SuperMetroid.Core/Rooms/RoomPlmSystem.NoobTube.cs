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
    private Action<ushort>? _writeNoobTubeEarthquakeType;
    private Action<NoobTubeProjectileRequest>? _spawnNoobTubeProjectile;

    private void ResetNoobTubeState()
    {
        _writeNoobTubeEarthquakeType = null;
        _spawnNoobTubeProjectile = null;
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
                    _soundRequests.Add(new PlmSoundRequest(
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
                ushort eventNumber = ReadBank84Word(bus, unchecked((ushort)(cursor + 2)));
                if (eventNumber != (ushort)NoobTubePlmRomData.BrokenEvent)
                {
                    throw new InvalidDataException(
                        $"N00b tube referenced event ${eventNumber:X4}, not event $000B.");
                }
                ushort target = ReadBank84Word(bus, unchecked((ushort)(cursor + 4)));
                slot.InstructionPointer = RequireNoobTubeEventReader()(NoobTubePlmRomData.BrokenEvent)
                    ? target
                    : unchecked((ushort)(cursor + 6));
                return true;

            case RoomPlmInstructionCodes.LinkInstruction:
                slot.LinkInstruction = ReadBank84Word(bus, unchecked((ushort)(cursor + 2)));
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;

            case RoomPlmInstructionCodes.ClearPreInstruction:
                slot.PreInstruction = NoobTubePlmRomData.InactivePreInstruction;
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstructionCodes.LockSamus:
                RequireNoobTubeSamus().InputLocked = true;
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            case RoomPlmInstructionCodes.UnlockSamus:
                RequireNoobTubeSamus().InputLocked = false;
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
                ushort setEvent = ReadBank84Word(bus, unchecked((ushort)(cursor + 2)));
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
                SamusState samus = RequireNoobTubeSamus();
                samus.LiquidPhysics.LiquidOptions = unchecked((ushort)(
                    samus.LiquidPhysics.LiquidOptions &
                    ~NoobTubePlmRomData.WaterPhysicsDisabledMask));
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            default:
                return false;
        }
    }

    private Func<EventNumber, bool> RequireNoobTubeEventReader() =>
        _hasEvent ?? throw new InvalidOperationException("N00b tube has no event reader.");

    private SamusState RequireNoobTubeSamus() =>
        _collectibleSamus?.Invoke() ?? throw new InvalidOperationException(
            "N00b tube has no active Samus actor.");

    private void SpawnNoobTubeProjectile(PlmSlot slot, ushort definition, ushort parameter) =>
        (_spawnNoobTubeProjectile ?? throw new InvalidOperationException(
            "N00b tube has no enemy-projectile spawner."))(
            new NoobTubeProjectileRequest(definition, parameter, slot.BlockIndex));

    private bool TryNotifyNoobTubeProjectileHit(int blockIndex, ushort projectileType)
    {
        PlmSlot? slot = _slots.FirstOrDefault(candidate =>
            candidate.Active &&
            candidate.HeaderPointer == RoomPlmHeaders.NoobTube &&
            candidate.BlockIndex == blockIndex);
        if (slot is null)
            return false;
        slot.LoopTimer = projectileType;
        return true;
    }
}
