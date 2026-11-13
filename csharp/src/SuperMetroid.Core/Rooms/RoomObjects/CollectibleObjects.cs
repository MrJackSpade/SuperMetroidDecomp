using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The room's in-world collectibles' service (#1554): item and tank setup, the touches and shots that trigger them, the
/// pickup with its message, fanfare and inventory, and each collectible's picture where it lies. Each item is a typed
/// room object (<see cref="CollectibleObject"/>) that holds and advances its own state.
/// </summary>
[RoomLifetime]
internal sealed class CollectibleObjects
{

    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The room's effect overlay FX.</summary>
    private readonly IRoomFxMotion? _roomFx;
    /// <summary>Updates level blocks and queues their tilemap draws.</summary>
    private readonly IRoomObjectDrawing _drawing;
    /// <summary>Enumerates active item room objects by slot ordinal.</summary>
    private readonly RoomObjectSlotTable _slotTable;
    /// <summary>
    /// Connects the collectibles to room state and drawing, and to the room's block pairs, which its collectibles that
    /// show their own picture take in turn.
    /// </summary>
    internal CollectibleObjects(SharedGameState? system, IRoomSamusAccess? samus, IRoomFxMotion? roomFx, IRoomObjectDrawing drawing, RoomObjectSlotTable slotTable,
        CollectibleBlockPairs blockPairs, RoomObjectSoundQueue sounds)
    {
        _sounds = Ensure.NotNull(sounds);
        _system = system;
        _samus = samus;
        _roomFx = roomFx;
        _drawing = drawing;
        _slotTable = slotTable;
        _blockPairs = Ensure.NotNull(blockPairs);
    }

    /// <summary>Pickup events published by the current room-object handler pass.</summary>
    private readonly List<CollectiblePickupEvent> _collectiblePickupEvents = new();
    /// <summary>The terrain block pairs the room's collectibles that show their own picture take in turn.</summary>
    private readonly CollectibleBlockPairs _blockPairs;
    /// <summary>The room objects' audio requests, which a pickup's fanfare joins.</summary>
    private readonly RoomObjectSoundQueue _sounds;
    /// <summary>Delays the Speed Booster Chozo lava motion write until message return.</summary>
    private bool _pendingSpeedBoosterPickupContinuation;

    /// <summary>Whether the room-object handler is waiting for its synchronous pickup message.</summary>
    internal bool HasPendingCollectibleMessage
    {
        get
        {
            foreach (RoomObjectSlot slot in _slotTable.FromLastPlace)
                if (slot.Active && (slot.Behaviour as CollectibleObject)?.Phase == CollectiblePhase.AwaitingMessage)
                    return true;
            return false;
        }
    }

    /// <summary>
    /// The message returns into the item's suspended script, which draws
    /// the empty block and deletes itself within the same room-object update.
    /// </summary>
    internal void CompleteCollectibleMessage(RoomLevelData level)
    {
        foreach (RoomObjectSlot slot in _slotTable.FromLastPlace)
        {
            if (!slot.Active || slot.Behaviour is not CollectibleObject { Phase: CollectiblePhase.AwaitingMessage } item)
                continue;
            item.Phase = CollectiblePhase.ResumeAfterMessage;
            item.FinishPickup(_drawing, level, slot);
        }
        CompleteSpeedBoosterPickupContinuation();
    }

    /// <summary>Applies the delayed lava-rise velocity after the Speed Booster message returns.</summary>
    private void CompleteSpeedBoosterPickupContinuation()
    {
        if (!_pendingSpeedBoosterPickupContinuation)
            return;
        IRoomFxMotion fx = _roomFx
            ?? throw new InvalidOperationException("Speed Booster pickup has no room-FX owner.");
        fx.SetLiquidMotion(
            yVelocity: SpeedBoosterPickupDefinitions.LavaRiseVelocity);
        _pendingSpeedBoosterPickupContinuation = false;
    }

    /// <summary>Pickup publications produced during the most recent room-object handler pass.</summary>
    public IReadOnlyList<CollectiblePickupEvent> CollectiblePickupEvents =>
        _collectiblePickupEvents;

    /// <summary>Every currently allocated permanent-item room object, by slot ordinal.</summary>
    public IReadOnlyList<CollectibleObjectSnapshot> Collectibles => _slotTable.FromLastPlace
        .Where(slot => slot.Active)
        .SelectMany(slot => slot.Behaviour is CollectibleObject { } item
            ? [new CollectibleObjectSnapshot(slot.Block, (slot.Argument as CollectedItemArgument)?.Location, item.Kind)]
            : Array.Empty<CollectibleObjectSnapshot>())
        .ToArray();

    /// <summary>
    /// Publishes Samus contact with a visible item block. The item's touch setup
    /// finds the item room object sharing this block and arms its trigger timer;
    /// acquisition remains owned by the following room-object handler pass.
    /// </summary>
    public bool TryNotifyCollectibleTouch(Position16<RoomBlocks> block)
    {
        foreach (RoomObjectSlot slot in _slotTable.FromLastPlace)
        {
            if (!slot.Active || slot.Block != block || slot.Behaviour is not CollectibleObject item)
                continue;
            // The pickup message retains both the visible block and its physical owner.
            // On message return, movement can touch that block before the room-object update runs
            // the pending empty draw/delete. The touch setup matches the block index without
            // filtering the instruction phase; acknowledge this owner without collecting
            // again or disturbing its message-return continuation.
            if (item.Phase is CollectiblePhase.AwaitingMessage or CollectiblePhase.ResumeAfterMessage or
                CollectiblePhase.EmptyAwaitingDelete)
                return true;
            if (item.Phase is not (
                    CollectiblePhase.Visible or CollectiblePhase.ShotBlockVisible))
                return false;
            item.Triggered = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Publishes a shot/bomb/grapple reaction at a Chozo orb or concealed item block.
    /// The projectile family is retained for diagnostics; the item's shot setup accepts any
    /// collision producer and reduces it to the same trigger.
    /// </summary>
    public bool TryNotifyCollectibleProjectileHit(
        Position16<RoomBlocks> block)
    {
        foreach (RoomObjectSlot slot in _slotTable.FromLastPlace)
        {
            if (!slot.Active || slot.Block != block || slot.Behaviour is not CollectibleObject item)
                continue;
            if (item.Phase is not (
                    CollectiblePhase.ChozoOrb or
                    CollectiblePhase.ShotBlock or
                    CollectiblePhase.CollectedShotBlock))
            {
                return false;
            }
            item.Triggered = true;
            return true;
        }
        return false;
    }

    /// <summary>Clears pickup publications at the start of a room-object handler frame.</summary>
    internal void BeginCollectibleFrame() => _collectiblePickupEvents.Clear();

    /// <summary>
    /// Runs one permanent-item setup on the slot selected by the shared room
    /// loader. The setup remains table-driven by the original game header and presentation.
    /// </summary>
    internal void SetupCollectibleSlot(
        RoomObjectRoom room,
        RoomObjectSlot slot,
        InWorldCollectibleKind kind,
        CollectiblePresentation presentation)
    {
        RoomLevelData level = room.Level;
        SharedGameState system = room.System;
        RoomCollisionBlock original = level.GetCollisionBlock(slot.Block);
        bool collected = slot.Argument is CollectedItemArgument placedItem &&
            system.HasCollectedItem(placedItem.Location);
        CollectibleBlockPairs.Pair? blockPair = ShowsOwnPicture(kind) ? _blockPairs.Take() : null;

        RoomTerrainBlock setup = original.Terrain.WithCollision(
            presentation == CollectiblePresentation.ShotBlock ? RoomCollisionType.ShootableBlock : RoomCollisionType.Air);
        level.SetTerrain(slot.Block, setup);
        level.SetBehavior(slot.Block, RoomBlockBehavior.Reacting(BlockReactionDefinitions.CollectibleTrigger));
        slot.RestoreTerrain = setup;

        CollectiblePhase phase = collected
            ? presentation == CollectiblePresentation.ShotBlock
                ? CollectiblePhase.CollectedShotBlock
                : CollectiblePhase.CollectedEmpty
            : presentation switch
            {
                CollectiblePresentation.Exposed => CollectiblePhase.Visible,
                // Item orb lists test only the collected-item bit. A shell
                // broken on an earlier visit is rebuilt until the item is picked up.
                CollectiblePresentation.ChozoOrb => CollectiblePhase.ChozoOrb,
                CollectiblePresentation.ShotBlock => CollectiblePhase.ShotBlock,
                _ => throw new ArgumentOutOfRangeException(nameof(presentation)),
            };
        // Both item setup paths increment the saved load counter, even for collected items.
        system.LoadedItemCount = WrappingCounter.IncrementCount(system.LoadedItemCount);
        slot.Become(new CollectibleObject(kind, presentation, blockPair, phase));
    }

    /// <summary>
    /// Whether a collectible shows its own picture where it lies (#1452, #1538): every item but the four tanks, whose
    /// pictures are blocks every room loads.
    /// </summary>
    internal static bool ShowsOwnPicture(InWorldCollectibleKind kind) => kind is not (InWorldCollectibleKind.EnergyTank
        or InWorldCollectibleKind.MissileTank or InWorldCollectibleKind.SuperMissileTank or InWorldCollectibleKind.PowerBombTank);

    /// <summary>
    /// Names the item and presentation of a permanent-item room object; false for every other room object. This public read-only seam
    /// lets audits inventory room populations without duplicating the permanent-item catalog.
    /// </summary>
    public static bool TryIdentifyPermanentCollectible(
        RoomObjectHeaderId header,
        out InWorldCollectibleKind kind,
        out CollectiblePresentation presentation)
    {
        bool found = PermanentCollectibleObjectDefinitions.Items.TryGetValue(header, out PermanentCollectibleObject item);
        kind = item.Kind;
        presentation = item.Presentation;
        return found;
    }

    /// <summary>
    /// Collects <paramref name="item"/>, placed at <paramref name="slot"/>: sets persistence and inventory before publishing
    /// the synchronous message and fanfare request.
    /// </summary>
    internal void Acquire(CollectibleObject item, RoomObjectSlot slot)
    {
        Ensure.NotNull(item);
        Ensure.NotNull(slot);
        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException(
                "A live collectible was triggered before the room owned Samus state.");
        SharedGameState system = _system
            ?? throw new InvalidOperationException("A live collectible has no persistence owner.");

        if (slot.Argument is CollectedItemArgument collectedItem)
            system.CollectItem(collectedItem.Location);
        ApplyCollectibleEffect(samus, item.Kind);

        var pickup = new CollectiblePickupEvent(
            item.Kind,
            GetMessageBoxIndex(item.Kind));
        _collectiblePickupEvents.Add(pickup);
        // The item's instruction starts the fanfare as it runs, before its message box opens.
        _sounds.QueuePermanentItemFanfare();
        _pendingSpeedBoosterPickupContinuation =
            item.Kind == InWorldCollectibleKind.SpeedBooster &&
            item.Presentation == CollectiblePresentation.ChozoOrb;

        // The item's script grants inventory before the message box opens, but its
        // following draw/delete steps cannot run until that synchronous call
        // returns. Keep the existing tile and slot throughout the fanfare.
        item.Triggered = false;
        item.Timer = 0;
        item.Phase = CollectiblePhase.AwaitingMessage;
    }

    /// <summary>Applies tank capacity, equipment, or beam ownership, including Spazer/Plasma exclusion.</summary>
    private static void ApplyCollectibleEffect(
        IRoomSamus samus,
        InWorldCollectibleKind kind)
    {
        switch (kind)
        {
            case InWorldCollectibleKind.EnergyTank:
                samus.MaxHealth += 100;
                samus.Health = samus.MaxHealth;
                return;
            case InWorldCollectibleKind.ReserveTank:
                samus.MaxReserveEnergy += 100;
                if (samus.ReserveTankMode == ReserveTankMode.None)
                    samus.ReserveTankMode = ReserveTankMode.Auto;
                return;
            case InWorldCollectibleKind.MissileTank:
                samus.MaxMissiles += 5;
                samus.Missiles += 5;
                return;
            case InWorldCollectibleKind.SuperMissileTank:
                samus.MaxSuperMissiles += 5;
                samus.SuperMissiles += 5;
                return;
            case InWorldCollectibleKind.PowerBombTank:
                samus.MaxPowerBombs += 5;
                samus.PowerBombs += 5;
                return;
            case InWorldCollectibleKind.Bombs:
            case InWorldCollectibleKind.HiJumpBoots:
            case InWorldCollectibleKind.SpeedBooster:
            case InWorldCollectibleKind.SpringBall:
            case InWorldCollectibleKind.VariaSuit:
            case InWorldCollectibleKind.GravitySuit:
            case InWorldCollectibleKind.XrayScope:
            case InWorldCollectibleKind.GrappleBeam:
            case InWorldCollectibleKind.SpaceJump:
            case InWorldCollectibleKind.ScrewAttack:
            case InWorldCollectibleKind.MorphBall:
                SamusEquipmentFlags equipmentMask = GetEquipmentMask(kind);
                samus.EquippedItems |= equipmentMask;
                samus.CollectedItems |= equipmentMask;
                // Every Varia/Gravity presentation clears the suit-pickup state immediately before
                // The equipment pickup and the synchronous message. It is not part of the later
                // transformation and therefore must already be clear throughout the fanfare.
                if (kind is InWorldCollectibleKind.VariaSuit or InWorldCollectibleKind.GravitySuit)
                    samus.ClearBeamCharge();
                return;
            case InWorldCollectibleKind.ChargeBeam:
            case InWorldCollectibleKind.IceBeam:
            case InWorldCollectibleKind.WaveBeam:
            case InWorldCollectibleKind.SpazerBeam:
            case InWorldCollectibleKind.PlasmaBeam:
                SamusBeamFlags beamMask = GetBeamMask(kind);
                samus.CollectedBeams |= beamMask;
                samus.EquippedBeams |= beamMask;

                // Spazer and Plasma are the only mutually exclusive beam pair: the beam pickup shifts the
                // collected beam onto its partner, so collecting either one unequips the other.
                if ((beamMask & SamusBeamFlags.Spazer) != SamusBeamFlags.None)
                    samus.EquippedBeams &= ~SamusBeamFlags.Plasma;
                if ((beamMask & SamusBeamFlags.Plasma) != SamusBeamFlags.None)
                    samus.EquippedBeams &= ~SamusBeamFlags.Spazer;
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined collectible kind.");
        }
    }

    /// <summary>Maps an equipment pickup to the bit written in equipped and collected items.</summary>
    private static SamusEquipmentFlags GetEquipmentMask(InWorldCollectibleKind kind) => kind switch
    {
        InWorldCollectibleKind.Bombs => SamusEquipmentFlags.Bombs,
        InWorldCollectibleKind.HiJumpBoots => SamusEquipmentFlags.HiJumpBoots,
        InWorldCollectibleKind.SpeedBooster => SamusEquipmentFlags.SpeedBooster,
        InWorldCollectibleKind.SpringBall => SamusEquipmentFlags.SpringBall,
        InWorldCollectibleKind.VariaSuit => SamusEquipmentFlags.VariaSuit,
        InWorldCollectibleKind.GravitySuit => SamusEquipmentFlags.GravitySuit,
        InWorldCollectibleKind.XrayScope => SamusEquipmentFlags.XrayScope,
        InWorldCollectibleKind.GrappleBeam => SamusEquipmentFlags.GrappleBeam,
        InWorldCollectibleKind.SpaceJump => SamusEquipmentFlags.SpaceJump,
        InWorldCollectibleKind.ScrewAttack => SamusEquipmentFlags.ScrewAttack,
        InWorldCollectibleKind.MorphBall => SamusEquipmentFlags.MorphBall,
        InWorldCollectibleKind.EnergyTank or
            InWorldCollectibleKind.MissileTank or
            InWorldCollectibleKind.SuperMissileTank or
            InWorldCollectibleKind.PowerBombTank or
            InWorldCollectibleKind.ChargeBeam or
            InWorldCollectibleKind.IceBeam or
            InWorldCollectibleKind.WaveBeam or
            InWorldCollectibleKind.SpazerBeam or
            InWorldCollectibleKind.PlasmaBeam or
            InWorldCollectibleKind.ReserveTank => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not an equipment collectible."),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not an equipment collectible."),
    };

    /// <summary>Maps a beam pickup to its bit for collected and equipped beams.</summary>
    private static SamusBeamFlags GetBeamMask(InWorldCollectibleKind kind) => kind switch
    {
        InWorldCollectibleKind.ChargeBeam => SamusBeamFlags.Charge,
        InWorldCollectibleKind.IceBeam => SamusBeamFlags.Ice,
        InWorldCollectibleKind.WaveBeam => SamusBeamFlags.Wave,
        InWorldCollectibleKind.SpazerBeam => SamusBeamFlags.Spazer,
        InWorldCollectibleKind.PlasmaBeam => SamusBeamFlags.Plasma,
        InWorldCollectibleKind.EnergyTank or
            InWorldCollectibleKind.MissileTank or
            InWorldCollectibleKind.SuperMissileTank or
            InWorldCollectibleKind.PowerBombTank or
            InWorldCollectibleKind.Bombs or
            InWorldCollectibleKind.HiJumpBoots or
            InWorldCollectibleKind.SpeedBooster or
            InWorldCollectibleKind.SpringBall or
            InWorldCollectibleKind.VariaSuit or
            InWorldCollectibleKind.GravitySuit or
            InWorldCollectibleKind.XrayScope or
            InWorldCollectibleKind.GrappleBeam or
            InWorldCollectibleKind.SpaceJump or
            InWorldCollectibleKind.ScrewAttack or
            InWorldCollectibleKind.MorphBall or
            InWorldCollectibleKind.ReserveTank => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };

    /// <summary>Selects the message shown synchronously after a permanent pickup.</summary>
    private static GameplayMessageId GetMessageBoxIndex(InWorldCollectibleKind kind) => kind switch
    {
        InWorldCollectibleKind.EnergyTank => GameplayMessageIds.EnergyTank,
        InWorldCollectibleKind.MissileTank => GameplayMessageIds.MissileTank,
        InWorldCollectibleKind.SuperMissileTank => GameplayMessageIds.SuperMissileTank,
        InWorldCollectibleKind.PowerBombTank => GameplayMessageIds.PowerBombTank,
        InWorldCollectibleKind.GrappleBeam => GameplayMessageIds.GrappleBeam,
        InWorldCollectibleKind.XrayScope => GameplayMessageIds.XrayScope,
        InWorldCollectibleKind.VariaSuit => GameplayMessageIds.VariaSuit,
        InWorldCollectibleKind.SpringBall => GameplayMessageIds.SpringBall,
        InWorldCollectibleKind.MorphBall => GameplayMessageIds.MorphBall,
        InWorldCollectibleKind.ScrewAttack => GameplayMessageIds.ScrewAttack,
        InWorldCollectibleKind.HiJumpBoots => GameplayMessageIds.HiJumpBoots,
        InWorldCollectibleKind.SpaceJump => GameplayMessageIds.SpaceJump,
        InWorldCollectibleKind.SpeedBooster => GameplayMessageIds.SpeedBooster,
        InWorldCollectibleKind.ChargeBeam => GameplayMessageIds.ChargeBeam,
        InWorldCollectibleKind.IceBeam => GameplayMessageIds.IceBeam,
        InWorldCollectibleKind.WaveBeam => GameplayMessageIds.WaveBeam,
        InWorldCollectibleKind.SpazerBeam => GameplayMessageIds.SpazerBeam,
        InWorldCollectibleKind.PlasmaBeam => GameplayMessageIds.PlasmaBeam,
        InWorldCollectibleKind.Bombs => GameplayMessageIds.Bombs,
        InWorldCollectibleKind.GravitySuit => GameplayMessageIds.GravitySuit,
        InWorldCollectibleKind.ReserveTank => GameplayMessageIds.ReserveTank,
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
