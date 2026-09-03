using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>Bank-$84's complete downward-gate PLM family.</summary>
public sealed partial class RoomPlmSystem
{
    private readonly List<DownwardGateProjectileRequest> _downwardGateProjectileRequests = new();
    private readonly List<PlmSoundRequest> _pendingDownwardGateSounds = new();
    private Func<SamusState?>? _downwardGateSamus;
    private int _downwardGateRoomWidth;

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

    private void SetupDownwardGateSlot(RoomLevelData level, PlmSlot slot)
    {
        slot.Gate = new DownwardGatePlmState();

        // Setup $C784 writes BTS $10 to the five vertically adjacent gate cells without
        // replacing their authored level words. C# block indexes are words, whereas the
        // native PLM index is a byte offset, so one room width here equals native +2*width.
        for (int row = 0; row < DownwardGatePlmRomData.GateHeightInBlocks; row++)
        {
            int blockIndex = checked(slot.BlockIndex + row * level.WidthInBlocks);
            _ = level.GetCollisionBlockByIndex(blockIndex);
            level.SetBehavior(blockIndex, DownwardGatePlmRomData.ClosedGateBts);
        }

        _downwardGateProjectileRequests.Add(new DownwardGateProjectileRequest(
            DownwardGateProjectileOperation.Spawn,
            (ushort)RoomEnemyProjectileKind.DownwardGateClosed,
            slot.BlockIndex));
    }

    private static void SetupDownwardGateShotBlock(ISnesAddressSpace bus, RoomLevelData level, PlmSlot slot)
    {
        if ((slot.RoomArgument & 1) != 0 ||
            slot.RoomArgument > DownwardGatePlmRomData.LastShotBlockTableByteOffset)
        {
            throw new InvalidDataException(
                $"Downward gate shot-block argument ${slot.RoomArgument:X4} is not an even table offset from $84:C70A.");
        }

        slot.InstructionPointer = ReadBank84Word(
            bus,
            unchecked((ushort)(DownwardGatePlmRomData.ShotBlockInstructionListTable + slot.RoomArgument)));
        ushort leftWord = ReadBank84Word(
            bus,
            unchecked((ushort)(DownwardGatePlmRomData.LeftShotBlockWordTable + slot.RoomArgument)));
        ushort rightWord = ReadBank84Word(
            bus,
            unchecked((ushort)(DownwardGatePlmRomData.RightShotBlockWordTable + slot.RoomArgument)));

        // Setup $C7B1 installs at most one side for every retail table row. Retaining two
        // independent writes nevertheless mirrors the routine and makes malformed ROM data
        // observable rather than silently choosing a side.
        if (leftWord != 0)
            WriteDownwardGateShotBlock(level, slot.BlockIndex - 1, leftWord);
        if (rightWord != 0)
            WriteDownwardGateShotBlock(level, slot.BlockIndex + 1, rightWord);
    }

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
        ArgumentNullException.ThrowIfNull(level);
        if (!bts.TryGetDownwardGateTrigger(out DownwardGateTriggerBehavior trigger))
            return false;

        // Spawn_PLM still probes the fixed pool even though every trigger setup deletes its
        // temporary slot synchronously. A saturated pool therefore drops the collision just
        // as the cartridge does.
        PlmSlot? temporary = null;
        for (int index = _slots.Length - 1; index >= 0; index--)
        {
            if (_slots[index].Active)
                continue;
            temporary = _slots[index];
            ClearSlot(temporary);
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
                _pendingDownwardGateSounds.Add(new PlmSoundRequest(
                    SoundEffectId.FromCartridge(
                        SoundEffectLibrary.Library2,
                        DownwardGatePlmRomData.RejectedShotSound),
                    MaximumQueued: 6));
            }
            ClearSlot(temporary);
            return true;
        }

        int gateBlockIndex = IsLeftTrigger(trigger) ? blockIndex + 1 : blockIndex - 1;
        PlmSlot? gate = null;
        for (int index = _slots.Length - 1; index >= 0; index--)
        {
            PlmSlot candidate = _slots[index];
            if (candidate.Active && candidate.Gate is not null &&
                candidate.BlockIndex == gateBlockIndex)
            {
                gate = candidate;
                break;
            }
        }
        if (gate is null)
        {
            ClearSlot(temporary);
            throw new InvalidDataException(
                $"Downward gate trigger block {blockIndex} has no adjacent resident gate at {gateBlockIndex}.");
        }

        // Trigger setup increments only a zero timer. Repeated accepted hits before the
        // resident pre-instruction runs do not accumulate extra close cycles.
        if (gate.LoopTimer == 0)
            gate.LoopTimer = 1;
        ClearSlot(temporary);
        return true;
    }

    private void PublishPendingDownwardGateSounds()
    {
        _soundRequests.AddRange(_pendingDownwardGateSounds);
        _pendingDownwardGateSounds.Clear();
    }

    private void RunDownwardGatePreInstruction(PlmSlot slot)
    {
        if (slot.Gate is null)
            return;

        bool wake = slot.PreInstruction switch
        {
            0 or DownwardGatePreInstructionCodes.Inert => false,
            DownwardGatePreInstructionCodes.WakeIfTriggered => slot.LoopTimer != 0,
            DownwardGatePreInstructionCodes.WakeIfTriggeredOrSamusBelow =>
                slot.LoopTimer != 0 || IsSamusInsideDownwardGateColumn(slot),
            _ => throw new InvalidDataException(
                $"Downward gate reached untranslated pre-instruction $84:{slot.PreInstruction:X4}."),
        };
        if (!wake)
            return;

        slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
        slot.InstructionTimer = 1;
        slot.PreInstruction = DownwardGatePreInstructionCodes.Inert;
    }

    private bool IsSamusInsideDownwardGateColumn(PlmSlot slot)
    {
        SamusState? samus = _downwardGateSamus?.Invoke();
        if (samus is null)
            return false;
        if (_downwardGateRoomWidth <= 0)
            throw new InvalidOperationException("Downward gate has no active room width.");
        int gateX = slot.BlockIndex % _downwardGateRoomWidth;
        int gateY = slot.BlockIndex / _downwardGateRoomWidth;
        return (samus.XPosition >> 4) == gateX &&
            unchecked((ushort)((samus.YPosition >> 4) - gateY)) <
                DownwardGatePlmRomData.GateHeightInBlocks;
    }

    private static bool IsLeftTrigger(DownwardGateTriggerBehavior trigger) =>
        ((byte)trigger & 1) == 0;

    private static ushort HeaderForGateTrigger(DownwardGateTriggerBehavior trigger) => trigger switch
    {
        DownwardGateTriggerBehavior.GreenLeft => DownwardGateTriggerPlmHeaders.GreenLeft,
        DownwardGateTriggerBehavior.GreenRight => DownwardGateTriggerPlmHeaders.GreenRight,
        DownwardGateTriggerBehavior.RedLeft => DownwardGateTriggerPlmHeaders.RedLeft,
        DownwardGateTriggerBehavior.RedRight => DownwardGateTriggerPlmHeaders.RedRight,
        DownwardGateTriggerBehavior.BlueLeft => DownwardGateTriggerPlmHeaders.BlueLeft,
        DownwardGateTriggerBehavior.BlueRight => DownwardGateTriggerPlmHeaders.BlueRight,
        DownwardGateTriggerBehavior.YellowLeft => DownwardGateTriggerPlmHeaders.YellowLeft,
        DownwardGateTriggerBehavior.YellowRight => DownwardGateTriggerPlmHeaders.YellowRight,
        _ => throw new ArgumentOutOfRangeException(nameof(trigger), trigger, null),
    };

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
            _ => false,
        };
    }

    private sealed class DownwardGatePlmState;
}
