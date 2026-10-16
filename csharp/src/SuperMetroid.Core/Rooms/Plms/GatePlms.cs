using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Gate PLMs: the shot-switched vertical gates and the downward gate with its sounds and projectile requests.</summary>
internal sealed class GatePlms : IPlmFamilySource
{
    /// <summary>Registers the DownwardGate family, which has no step of its own: its slots take the shared path.</summary>
    public void RegisterPlmFamilies(PlmFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Register(PlmFamily.DownwardGate, static (_, _) => PlmStepOutcome.Shared);
    }

    /// <summary>Current room and Samus state used by gate-column proximity checks.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Fixed PLM pool used for temporary shot triggers and resident gates.</summary>
    private readonly PlmSlotTable _slotTable;
    /// <summary>Sound queue receiving accepted gate wake and rejected-shot effects.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Binds the gate PLM owner to the active room, slot pool, and sound queue.</summary>
    internal GatePlms(PlmRoomBindings bindings, PlmSlotTable slotTable, PlmSoundQueue sounds)
    {
        _bindings = bindings;
        _slotTable = slotTable;
        _sounds = sounds;
    }
    /// <summary>Spawn and close requests published by gate PLMs for the projectile system.</summary>
    internal readonly List<DownwardGateProjectileRequest> _downwardGateProjectileRequests = new();
    /// <summary>Rejected-shot sounds buffered until the room's sound publication stage.</summary>
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

    /// <summary>Marks the gate's five vertical BTS cells and requests its initial closed projectile.</summary>
    internal void SetupDownwardGateSlot(RoomLevelData level, PlmSlot slot)
    {
        slot.Family = new DownwardGatePlmState();
        slot.StartAnimation(new DownwardGatePlmAnimation(DownwardGateProgram.Closed));

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

    /// <summary>Installs a side trigger's selected instruction list and authored neighboring shot-block words.</summary>
    internal static void SetupDownwardGateShotBlock(RoomLevelData level, PlmSlot slot)
    {
        DownwardGateShotBlockDefinition definition =
            DownwardGateShotBlockDefinitions.Resolve(slot.RoomArgument);
        slot.StartAnimation(new DownwardGatePlmAnimation(definition.Program));

        // Setup $C6E0 performs independent left and right writes even though every authored
        // row installs exactly one side. Preserve that ordering instead of deriving the side
        // from the trigger's low bit.
        if (definition.LeftBlockWord != 0)
            WriteDownwardGateShotBlock(level, slot.BlockIndex - 1, definition.LeftBlockWord);
        if (definition.RightBlockWord != 0)
            WriteDownwardGateShotBlock(level, slot.BlockIndex + 1, definition.RightBlockWord);
    }

    /// <summary>Writes a trigger's foreground word and its low-byte behavior to one collision block.</summary>
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
            temporary.Clear();
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
            temporary.Clear();
            return true;
        }

        int gateBlockIndex = IsLeftTrigger(trigger) ? blockIndex + 1 : blockIndex - 1;
        PlmSlot? gate = null;
        for (int index = _slotTable._slots.Length - 1; index >= 0; index--)
        {
            PlmSlot candidate = _slotTable._slots[index];
            if (candidate.Active && candidate.Family is DownwardGatePlmState &&
                candidate.BlockIndex == gateBlockIndex)
            {
                gate = candidate;
                break;
            }
        }
        if (gate is null)
        {
            temporary.Clear();
            throw new InvalidDataException(
                $"Downward gate trigger block {blockIndex} has no adjacent resident gate at {gateBlockIndex}.");
        }

        // Trigger setup increments only a zero timer. Repeated accepted hits before the
        // resident pre-instruction runs do not accumulate extra close cycles.
        if (gate.LoopTimer == 0)
            gate.LoopTimer = 1;
        temporary.Clear();
        return true;
    }

    /// <summary><c>$84:BBE1</c>: requests the moving gate projectile that slides the gate down from the slot's block.</summary>
    internal void SpawnMovingGateProjectile(PlmSlot slot) =>
        _downwardGateProjectileRequests.Add(new DownwardGateProjectileRequest(
            DownwardGateProjectileOperation.Spawn, (ushort)RoomEnemyProjectileKind.DownwardGateMoving, slot.BlockIndex));

    /// <summary>
    /// <c>$84:BBF0</c>: wakes the gate projectile whose variable E holds this PLM's block, so it slides back up.
    /// </summary>
    internal void WakeGateProjectile(PlmSlot slot) =>
        _downwardGateProjectileRequests.Add(new DownwardGateProjectileRequest(
            DownwardGateProjectileOperation.Wake, DefinitionPointer: 0, slot.BlockIndex));

    /// <summary>Moves buffered rejected-shot sounds into the room sound queue in emission order.</summary>
    internal void PublishPendingDownwardGateSounds()
    {
        _sounds._soundRequests.AddRange(_pendingDownwardGateSounds);
        _pendingDownwardGateSounds.Clear();
    }

    /// <summary>Wakes a resident gate when shot or when Samus occupies the column for the appropriate pre-instruction.</summary>
    internal void RunDownwardGatePreInstruction(PlmSlot slot)
    {
        if (slot.Family is not DownwardGatePlmState)
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

        slot.WakePastSleep();
        slot.PreInstruction = (ushort)DownwardGatePreInstruction.Inert;
    }

    /// <summary>Tests Samus's block position against the resident gate's five-cell vertical column.</summary>
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

    /// <summary>Distinguishes left-side trigger codes using their native low direction bit.</summary>
    private static bool IsLeftTrigger(DownwardGateTriggerBehavior trigger) =>
        ((byte)trigger & 1) == 0;

    /// <summary>Maps a color-and-side trigger behavior to the matching temporary PLM header.</summary>
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

    /// <summary>Applies cartridge projectile-family rules, including the right-yellow acceptance asymmetry.</summary>
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

    /// <summary>Marker attached to the resident PLM slot so adjacent shot triggers can find its gate.</summary>
    internal sealed class DownwardGatePlmState : PlmFamilyState
    {
        /// <inheritdoc/>
        internal override PlmFamily Family => PlmFamily.DownwardGate;
    }
}
