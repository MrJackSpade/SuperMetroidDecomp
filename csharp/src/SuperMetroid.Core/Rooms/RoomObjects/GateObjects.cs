using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The downward gates and their shot-block triggers: their setup, the shots that set them off, and their sounds and projectile requests.</summary>
[RoomLifetime]
internal sealed class GateObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess _samus;
    /// <summary>Fixed room-object pool used for temporary shot triggers and resident gates.</summary>
    private readonly RoomObjectPlaceList _places;
    /// <summary>Sound queue receiving accepted gate wake and rejected-shot effects.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Binds the gate room-object owner to the active room, its room-object places, and the sound queue.</summary>
    internal GateObjects(IRoomSamusAccess samus, RoomObjectPlaceList places, RoomObjectSoundQueue sounds)
    {
        _samus = samus;
        _places = places;
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
    internal void SetupDownwardGate(RoomLevelData level, RoomObjectPlace place)
    {
        place.Become(new DownwardGateObject(this));

        // The setup writes the closed-gate behaviour to the five vertically adjacent gate cells without replacing their
        // authored terrain.
        for (int row = 0; row < DownwardGateDefinitions.GateHeightInBlocks; row++)
        {
            Position16<RoomBlocks> block = place.Block + new Offset16<RoomBlocks>(0, row);
            level.SetBehavior(block, DownwardGateDefinitions.ClosedGateBehavior);
        }

        _downwardGateProjectileRequests.Add(new DownwardGateProjectileSpawn(DownwardGateProjectileKind.Closed, place.Block));
    }

    /// <summary>
    /// Installs the trigger its placement (<paramref name="placed"/>) selects: its shot-block draw, and the block on the
    /// trigger's own side of the gate made a shootable block whose reaction sets the trigger off. Only the block's collision
    /// kind and behaviour change, so it keeps its picture and what it shows (#1580, #2496): the trigger's shot-block draw
    /// covers it on the object's first update.
    /// </summary>
    internal static void SetupDownwardGateShotBlock(RoomLevelData level, RoomObjectPlace place, GateTriggerArgument placed)
    {
        DownwardGateTrigger trigger = Ensure.NotNull(placed).Trigger;
        place.Become(new TerrainChangeObject(DownwardGateTriggerDefinitions.Triggers[trigger].Change));
        // The trigger's block must lie in the room's grid.
        Position16<RoomBlocks> triggerBlock = level.GridBlock(level.ObjectBlockStep(place.Block, new Offset16<RoomBlocks>(ColumnsFromGate(trigger), 0)));
        level.SetObjectCollisionAndBehavior(triggerBlock, DownwardGateDefinitions.TriggerCollision,
            RoomBlockBehavior.Reacting(BlockReactionDefinitions.DownwardGateTriggers.ReactionFor(trigger)));
    }

    /// <summary>Runs the gate-trigger shot step and wakes its adjacent gate.</summary>
    public void SpawnDownwardGateTrigger(
        RoomLevelData level,
        Position16<RoomBlocks> position,
        RoomBlockBehavior behavior,
        SamusProjectileType projectileType)
    {
        Ensure.NotNull(level);
        if (!behavior.TryGetDownwardGateTrigger(out DownwardGateTrigger trigger))
            return;
        DownwardGateTriggerDefinition definition = DownwardGateTriggerDefinitions.Triggers[trigger];

        // Spawning the trigger still takes a room-object place even though every trigger setup frees it again
        // at once. A full room therefore drops the collision just
        // as the game does.
        if (_places.Spawn() is not { } temporary)
            return;
        temporary.Block = position;

        if (!definition.SetOffBy.Accepts(projectileType.Family))
        {
            // The trigger's setup queues the rejection at once, inside the shot's collision (#1620).
            if (definition.RejectionSound is { } rejection)
                _sounds.Queue(rejection, Priority: SoundRequestPriority.Normal);
            temporary.Delete();
            return;
        }

        Position16<RoomBlocks> gateBlock = level.ObjectBlockStep(temporary.Block, new Offset16<RoomBlocks>(-ColumnsFromGate(trigger), 0));
        DownwardGateObject? gate = null;
        foreach (RoomObjectPlace candidate in _places.InUpdateOrder)
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
    }

    /// <summary>Requests the moving gate projectile that slides the gate down from the place's block.</summary>
    internal void SpawnMovingGateProjectile(RoomObjectPlace place) =>
        _downwardGateProjectileRequests.Add(new DownwardGateProjectileSpawn(DownwardGateProjectileKind.Moving, place.Block));

    /// <summary>
    /// Wakes the gate projectile spawned for this gate's cell, so it slides back up.
    /// </summary>
    internal void WakeGateProjectile(RoomObjectPlace place) =>
        _downwardGateProjectileRequests.Add(new DownwardGateProjectileWake(place.Block));

    /// <summary>Tests Samus's block position against the resident gate's five-cell vertical column.</summary>
    internal bool IsSamusInsideDownwardGateColumn(RoomObjectPlace place)
    {
        IRoomSamus samus = _samus.Current;
        int rowsBelowGateTop = RoomGridConversions.PixelToBlock(samus.Position.Y.Whole) - place.Block.Y;
        return RoomGridConversions.PixelToBlock(samus.Position.X.Whole) == place.Block.X &&
            rowsBelowGateTop >= 0 && rowsBelowGateTop < DownwardGateDefinitions.GateHeightInBlocks;
    }

    /// <summary>
    /// The columns from a gate to its <paramref name="trigger"/>'s block: one to the left for a trigger on the gate's left
    /// side, one to the right for one on its right (#2128).
    /// </summary>
    /// <exception cref="InvalidOperationException">The trigger's side is undefined.</exception>
    internal static int ColumnsFromGate(DownwardGateTrigger trigger) => trigger.Side switch
    {
        HorizontalDirection.Left => -1,
        HorizontalDirection.Right => 1,
        _ => throw new InvalidOperationException($"Undefined {nameof(HorizontalDirection)} {trigger.Side}."),
    };
}
