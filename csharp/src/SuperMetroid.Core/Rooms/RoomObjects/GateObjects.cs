using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The downward gates and their shot-block triggers: their setup, the shots that set them off, and their sounds and projectile requests.</summary>
[RoomLifetime]
internal sealed class GateObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>Fixed room-object pool used for temporary shot triggers and resident gates.</summary>
    private readonly RoomObjectSlotTable _slotTable;
    /// <summary>Sound queue receiving accepted gate wake and rejected-shot effects.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Binds the gate room-object owner to the active room, slot pool, and sound queue.</summary>
    internal GateObjects(IRoomSamusAccess? samus, RoomObjectSlotTable slotTable, RoomObjectSoundQueue sounds)
    {
        _samus = samus;
        _slotTable = slotTable;
        _sounds = sounds;
    }
    /// <summary>Spawn and close requests published by gate room objects for the projectile system.</summary>
    private readonly List<DownwardGateProjectileRequest> _downwardGateProjectileRequests = new();

    /// <summary>
    /// Returns and clears projectile work published by gate setup and sequences. Room load
    /// consumes the initial closed actor after enemy loading clears the enemy-projectile pool; normal
    /// gameplay consumes later close and wake requests immediately after the room-object step.
    /// </summary>
    public IReadOnlyList<DownwardGateProjectileRequest> TakeDownwardGateProjectileRequests()
    {
        DownwardGateProjectileRequest[] requests = _downwardGateProjectileRequests.ToArray();
        _downwardGateProjectileRequests.Clear();
        return requests;
    }

    /// <summary>Marks the gate's five vertical cells' behaviour and requests its initial closed projectile.</summary>
    internal void SetupDownwardGateSlot(RoomLevelData level, RoomObjectSlot slot)
    {
        slot.Become(new DownwardGateObject(this));

        // The setup writes the closed-gate behaviour to the five vertically adjacent gate cells without replacing their
        // authored terrain.
        for (int row = 0; row < DownwardGateDefinitions.GateHeightInBlocks; row++)
        {
            Position16<RoomBlocks> block = slot.Block + new Offset16<RoomBlocks>(0, row);
            _ = level.GetCollisionBlock(block);
            level.SetBehavior(block, DownwardGateDefinitions.ClosedGateBehavior);
        }

        _downwardGateProjectileRequests.Add(new DownwardGateProjectileRequest(
            DownwardGateProjectileOperation.Spawn,
            DownwardGateProjectileKind.Closed,
            slot.Block));
    }

    /// <summary>Installs a side trigger's selected gate and its authored neighboring shot block.</summary>
    internal static void SetupDownwardGateShotBlock(RoomLevelData level, RoomObjectSlot slot)
    {
        DownwardGateShotBlockDefinition definition = (slot.Argument as GateTriggerArgument
            ?? throw new InvalidDataException($"Downward gate shot block at block {slot.Block} was placed without its trigger.")).ShotBlock;
        slot.Become(new TerrainChangeObject(definition.Change));

        // The setup performs independent left and right writes even though every authored row installs exactly one
        // side. Preserve that ordering instead of deriving the side from the trigger.
        if (definition.LeftCollision is { } left)
            WriteDownwardGateShotBlock(level, level.ObjectBlockStep(slot.Block, new Offset16<RoomBlocks>(-1, 0)), left, definition.Trigger);
        if (definition.RightCollision is { } right)
            WriteDownwardGateShotBlock(level, level.ObjectBlockStep(slot.Block, new Offset16<RoomBlocks>(1, 0)), right, definition.Trigger);
    }

    /// <summary>
    /// Gives one block inside the room a trigger's collision kind and its gate-trigger behaviour, keeping what the block
    /// shows (#1580): the trigger's shot-block draw covers it on the object's first update.
    /// </summary>
    /// <remarks>The trigger's reaction selects its color and side.</remarks>
    private static void WriteDownwardGateShotBlock(RoomLevelData level, Position16<RoomBlocks> block, RoomCollisionType collision,
        DownwardGateTriggerBehavior trigger)
    {
        _ = level.GetCollisionBlock(block);
        RoomObjectLevelWrites.WriteObjectCollisionAndBehavior(level, block, collision,
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.DownwardGateTriggers.ReactionFor(trigger)));
    }

    /// <summary>Runs the gate-trigger shot step and wakes its adjacent gate.</summary>
    public bool TrySpawnDownwardGateTrigger(
        RoomLevelData level,
        Position16<RoomBlocks> position,
        RoomBlockBehavior bts,
        SamusProjectileType projectileType)
    {
        Ensure.NotNull(level);
        if (!bts.TryGetDownwardGateTrigger(out DownwardGateTriggerBehavior trigger))
            return false;

        // Spawning the trigger still probes the fixed room-object pool even though every trigger setup deletes its
        // temporary slot synchronously. A saturated pool therefore drops the collision just
        // as the game does.
        if (_slotTable.Spawn() is not { } temporary)
            return true;
        temporary.Block = position;

        bool accepted = GateTriggerAcceptsProjectile(trigger, projectileType);
        if (!accepted)
        {
            if (trigger is not DownwardGateTriggerBehavior.BlueLeft and
                not DownwardGateTriggerBehavior.BlueRight)
            {
                // The trigger's setup queues the rejection at once, inside the shot's collision (#1620).
                _sounds.Queue(DownwardGateDefinitions.RejectedShotSound, Priority: SoundRequestPriority.Normal);
            }
            temporary.Delete();
            return true;
        }

        Position16<RoomBlocks> gateBlock = level.ObjectBlockStep(temporary.Block, new Offset16<RoomBlocks>(IsLeftTrigger(trigger) ? 1 : -1, 0));
        DownwardGateObject? gate = null;
        foreach (RoomObjectSlot candidate in _slotTable.InProcessingOrder)
        {
            if (candidate.Active && candidate.Behaviour is DownwardGateObject resident &&
                candidate.Block == gateBlock)
            {
                gate = resident;
                break;
            }
        }
        if (gate is null)
        {
            temporary.Delete();
            throw new InvalidDataException(
                $"Downward gate trigger block {position} has no adjacent resident gate at {gateBlock}.");
        }

        // Repeated accepted hits before the gate wakes do not accumulate extra close cycles.
        gate.Trigger();
        temporary.Delete();
        return true;
    }

    /// <summary>Requests the moving gate projectile that slides the gate down from the slot's block.</summary>
    internal void SpawnMovingGateProjectile(RoomObjectSlot slot) =>
        _downwardGateProjectileRequests.Add(new DownwardGateProjectileRequest(
            DownwardGateProjectileOperation.Spawn, DownwardGateProjectileKind.Moving, slot.Block));

    /// <summary>
    /// Wakes the gate projectile spawned for this gate's cell, so it slides back up.
    /// </summary>
    internal void WakeGateProjectile(RoomObjectSlot slot) =>
        _downwardGateProjectileRequests.Add(new DownwardGateProjectileRequest(
            DownwardGateProjectileOperation.Wake, Kind: null, slot.Block));

    /// <summary>Tests Samus's block position against the resident gate's five-cell vertical column.</summary>
    internal bool IsSamusInsideDownwardGateColumn(RoomObjectSlot slot)
    {
        IRoomSamus? samus = _samus?.Current;
        if (samus is null)
            return false;
        int gateX = slot.Block.X;
        int gateY = slot.Block.Y;
        return RoomGridConversions.PixelToBlock(samus.Position.X.Whole) == gateX &&
            CoordinateAxis.Wrap(RoomGridConversions.PixelToBlock(samus.Position.Y.Whole) - gateY) <
                DownwardGateDefinitions.GateHeightInBlocks;
    }

    /// <summary>Whether <paramref name="trigger"/> is a left-side trigger.</summary>
    private static bool IsLeftTrigger(DownwardGateTriggerBehavior trigger) =>
        trigger is DownwardGateTriggerBehavior.BlueLeft or DownwardGateTriggerBehavior.RedLeft or
            DownwardGateTriggerBehavior.GreenLeft or DownwardGateTriggerBehavior.YellowLeft;

    /// <summary>Applies projectile-family rules, including the right-yellow acceptance asymmetry.</summary>
    private static bool GateTriggerAcceptsProjectile(
        DownwardGateTriggerBehavior trigger,
        SamusProjectileType projectileType)
    {
        bool plainMissile = projectileType.IsFamily(SamusProjectileFamily.Missile);
        bool plainSuper = projectileType.IsFamily(SamusProjectileFamily.SuperMissile);
        bool plainPowerBomb = projectileType.IsFamily(SamusProjectileFamily.PowerBomb);
        return trigger switch
        {
            DownwardGateTriggerBehavior.GreenLeft or DownwardGateTriggerBehavior.GreenRight => plainSuper,
            DownwardGateTriggerBehavior.RedLeft or DownwardGateTriggerBehavior.RedRight => plainMissile || plainSuper,
            DownwardGateTriggerBehavior.BlueLeft or DownwardGateTriggerBehavior.BlueRight => !plainPowerBomb,
            DownwardGateTriggerBehavior.YellowLeft => plainPowerBomb,

            // The retail right-yellow gate triggers on everything but a power bomb, the opposite of the
            // left one. Preserve that asymmetry even though it looks like a likely original bug.
            DownwardGateTriggerBehavior.YellowRight => !plainPowerBomb,
            _ => throw new InvalidOperationException($"Undefined gate trigger {trigger}."),
        };
    }
}
