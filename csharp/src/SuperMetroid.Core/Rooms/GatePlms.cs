using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Gate PLMs: the shot-switched vertical gates and the downward gate with its sounds and projectile requests.</summary>
internal sealed class GatePlms
{
    /// <summary>Stores bindings.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Stores slot table.</summary>
    private readonly PlmSlotTable _slotTable;
    /// <summary>Stores sounds.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Initializes a new gate plms instance.</summary>
    internal GatePlms(PlmRoomBindings bindings, PlmSlotTable slotTable, PlmSoundQueue sounds)
    {
        _bindings = bindings;
        _slotTable = slotTable;
        _sounds = sounds;
    }
    /// <summary>Stores downward gate projectile requests.</summary>
    internal readonly List<DownwardGateProjectileRequest> _downwardGateProjectileRequests = new();
    /// <summary>Stores pending downward gate sounds.</summary>
    internal readonly List<PlmSoundRequest> _pendingDownwardGateSounds = new();

    /// <summary>
    /// Returns and clears projectile work published by gate setup/list execution. Room load
    /// consumes the initial closed actor after enemy loading clears bank $86's pool; normal
    /// gameplay consumes later close/wake commands immediately after the PLM handler.
    /// </summary>
    public IReadOnlyList<DownwardGateProjectileRequest> TakeDownwardGateProjectileRequests()
    {
        DownwardGateProjectileRequest[] requests = _downwardGateProjectileRequests.ToArray();
        _downwardGateProjectileRequests.Clear();
        return requests;
    }

    /// <summary>Sets up downward gate slot.</summary>
    internal void SetupDownwardGateSlot(RoomLevelData level, PlmSlot slot)
    {
        slot.Gate = new DownwardGatePlmState();

        // Setup $C784 writes BTS $10 to the five vertically adjacent gate cells without
        // replacing their authored level words. C# block indexes are words, whereas the
        // native PLM index is a byte offset, so one room width here equals native +2*width.
        for (int row = 0; row < DownwardGatePlmRomData.GateHeightInBlocks; row++)
        {
            int blockIndex = checked(slot.BlockIndex + row * level.Size.X);
            _ = level.GetCollisionBlockByIndex(blockIndex);
            level.SetBehavior(blockIndex, DownwardGatePlmRomData.ClosedGateBts);
        }

        _downwardGateProjectileRequests.Add(new DownwardGateProjectileRequest(
            DownwardGateProjectileOperation.Spawn,
            (ushort)RoomEnemyProjectileKind.DownwardGateClosed,
            slot.BlockIndex));
    }

    /// <summary>Sets up downward gate shot block.</summary>
    internal static void SetupDownwardGateShotBlock(RoomLevelData level, PlmSlot slot)
    {
        DownwardGateShotBlockDefinition definition =
            DownwardGateShotBlockDefinitions.Resolve(slot.RoomArgument);
        slot.InstructionPointer = definition.InstructionList;

        // Setup $C6E0 performs independent left and right writes even though every authored
        // row installs exactly one side. Preserve that ordering instead of deriving the side
        // from the trigger's low bit.
        if (definition.LeftBlockWord != 0)
            WriteDownwardGateShotBlock(level, slot.BlockIndex - 1, definition.LeftBlockWord);
        if (definition.RightBlockWord != 0)
            WriteDownwardGateShotBlock(level, slot.BlockIndex + 1, definition.RightBlockWord);
    }

    /// <summary>Writes downward gate shot block.</summary>
    private static void WriteDownwardGateShotBlock(RoomLevelData level, int blockIndex, ushort word)
    {
        _ = level.GetCollisionBlockByIndex(blockIndex);
        level.SetForegroundEntry(blockIndex, word);
        level.SetBehavior(blockIndex, unchecked((byte)word));
    }

    /// <summary>Runs the shootable BTS $46-$4D dispatcher and wakes its adjacent gate.</summary>
    public bool TrySpawnDownwardGateTrigger(
        RoomLevelData level,
        int blockIndex,
        RoomBlockBehavior bts,
        SamusProjectileTypeWord projectileType)
    {
        Ensure.NotNull(level);
        if (!bts.TryGetDownwardGateTrigger(out DownwardGateTriggerBehavior trigger))
            return false;

        // Spawn_PLM still probes the fixed pool even though every trigger setup deletes its
        // temporary slot synchronously. A saturated pool therefore drops the collision just
        // as the cartridge does.
        PlmSlot? temporary = null;
        for (int index = _slotTable._slots.Length - 1; index >= 0; index--)
        {
            if (_slotTable._slots[index].Active)
                continue;
            temporary = _slotTable._slots[index];
            PlmPopulationLoader.ClearSlot(temporary);
            temporary.Active = true;
            temporary.HeaderPointer = HeaderForGateTrigger(trigger);
            temporary.BlockIndex = blockIndex;
            temporary.LoopTimer = projectileType.Raw;
            break;
        }
        if (temporary is null)
            return true;

        bool accepted = GateTriggerAcceptsProjectile(trigger, projectileType);
        if (!accepted)
        {
            if (trigger is not DownwardGateTriggerBehavior.BlueLeft and
                not DownwardGateTriggerBehavior.BlueRight)
            {
                _pendingDownwardGateSounds.Add(_sounds.CreateSoundRequest(
                    SoundEffectId.FromCartridge(
                        SoundEffectLibrary.Library2,
                        DownwardGatePlmRomData.RejectedShotSound),
                    MaximumQueued: 6));
            }
            PlmPopulationLoader.ClearSlot(temporary);
            return true;
        }

        int gateBlockIndex = IsLeftTrigger(trigger) ? blockIndex + 1 : blockIndex - 1;
        PlmSlot? gate = null;
        for (int index = _slotTable._slots.Length - 1; index >= 0; index--)
        {
            PlmSlot candidate = _slotTable._slots[index];
            if (candidate.Active && candidate.Gate is not null &&
                candidate.BlockIndex == gateBlockIndex)
            {
                gate = candidate;
                break;
            }
        }
        if (gate is null)
        {
            PlmPopulationLoader.ClearSlot(temporary);
            throw new InvalidDataException(
                $"Downward gate trigger block {blockIndex} has no adjacent resident gate at {gateBlockIndex}.");
        }

        // Trigger setup increments only a zero timer. Repeated accepted hits before the
        // resident pre-instruction runs do not accumulate extra close cycles.
        if (gate.LoopTimer == 0)
            gate.LoopTimer = 1;
        PlmPopulationLoader.ClearSlot(temporary);
        return true;
    }

    /// <summary>Publish pending downward gate sounds.</summary>
    internal void PublishPendingDownwardGateSounds()
    {
        _sounds._soundRequests.AddRange(_pendingDownwardGateSounds);
        _pendingDownwardGateSounds.Clear();
    }

    /// <summary>Runs downward gate pre instruction.</summary>
    internal void RunDownwardGatePreInstruction(PlmSlot slot)
    {
        if (slot.Gate is null)
            return;

        DownwardGatePreInstruction preInstruction = ClosedNativeWords.Decode<DownwardGatePreInstruction>(
            slot.PreInstruction, "downward gate pre-instruction");
        bool wake = preInstruction switch
        {
            DownwardGatePreInstruction.None or DownwardGatePreInstruction.Inert => false,
            DownwardGatePreInstruction.WakeIfTriggered => slot.LoopTimer != 0,
            DownwardGatePreInstruction.WakeIfTriggeredOrSamusBelow =>
                slot.LoopTimer != 0 || IsSamusInsideDownwardGateColumn(slot),
            _ => throw new InvalidOperationException(
                $"Undefined downward gate pre-instruction {preInstruction}."),
        };
        if (!wake)
            return;

        slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
        slot.InstructionTimer = 1;
        slot.PreInstruction = (ushort)DownwardGatePreInstruction.Inert;
    }

    /// <summary>Checks whether samus inside downward gate column.</summary>
    private bool IsSamusInsideDownwardGateColumn(PlmSlot slot)
    {
        SamusState? samus = _bindings.Samus?.Current;
        if (samus is null)
            return false;
        if (_bindings.RoomWidth <= 0)
            throw new InvalidOperationException("Downward gate has no active room width.");
        int gateX = slot.BlockIndex % _bindings.RoomWidth;
        int gateY = slot.BlockIndex / _bindings.RoomWidth;
        return (samus.Position.X.Whole >> 4) == gateX &&
            unchecked((ushort)((samus.Position.Y.Whole >> 4) - gateY)) <
                DownwardGatePlmRomData.GateHeightInBlocks;
    }

    /// <summary>Checks whether left trigger.</summary>
    private static bool IsLeftTrigger(DownwardGateTriggerBehavior trigger) =>
        ((byte)trigger & 1) == 0;

    /// <summary>Header for gate trigger.</summary>
    private static PlmHeaderId HeaderForGateTrigger(DownwardGateTriggerBehavior trigger) => trigger switch
    {
        DownwardGateTriggerBehavior.GreenLeft => PlmHeaderId.LeftGreenGateTrigger,
        DownwardGateTriggerBehavior.GreenRight => PlmHeaderId.RightGreenGateTrigger,
        DownwardGateTriggerBehavior.RedLeft => PlmHeaderId.LeftRedGateTrigger,
        DownwardGateTriggerBehavior.RedRight => PlmHeaderId.RightRedGateTrigger,
        DownwardGateTriggerBehavior.BlueLeft => PlmHeaderId.LeftBlueGateTrigger,
        DownwardGateTriggerBehavior.BlueRight => PlmHeaderId.RightBlueGateTrigger,
        DownwardGateTriggerBehavior.YellowLeft => PlmHeaderId.LeftYellowGateTrigger,
        DownwardGateTriggerBehavior.YellowRight => PlmHeaderId.RightYellowGateTrigger,
        _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null),
    };

    /// <summary>Gate trigger accepts projectile.</summary>
    private static bool GateTriggerAcceptsProjectile(
        DownwardGateTriggerBehavior trigger,
        SamusProjectileTypeWord projectileType)
    {
        bool plainMissile = projectileType.HasPlainFamilyPayload(SamusProjectileFamily.Missile);
        bool plainSuper = projectileType.HasPlainFamilyPayload(SamusProjectileFamily.SuperMissile);
        bool plainPowerBomb = projectileType.HasPlainFamilyPayload(SamusProjectileFamily.PowerBomb);
        return trigger switch
        {
            DownwardGateTriggerBehavior.GreenLeft or DownwardGateTriggerBehavior.GreenRight => plainSuper,
            DownwardGateTriggerBehavior.RedLeft or DownwardGateTriggerBehavior.RedRight => plainMissile || plainSuper,
            DownwardGateTriggerBehavior.BlueLeft or DownwardGateTriggerBehavior.BlueRight => !plainPowerBomb,
            DownwardGateTriggerBehavior.YellowLeft => plainPowerBomb,

            // The retail right-yellow routine branches on BNE rather than BEQ. Preserve
            // that cartridge asymmetry even though it looks like a likely original bug.
            DownwardGateTriggerBehavior.YellowRight => !plainPowerBomb,
            _ => throw new InvalidOperationException($"Undefined gate trigger {trigger}."),
        };
    }

    /// <summary>Represents downward gate plm state.</summary>
    internal sealed class DownwardGatePlmState;
}
