using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Abilities;

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
    private readonly SharedGameState _system;
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess _samus;
    /// <summary>The room's effect overlay FX.</summary>
    private readonly IRoomFxMotion _roomFx;
    /// <summary>Updates level blocks and queues their draws.</summary>
    private readonly IRoomObjectDrawing _drawing;
    /// <summary>The room's places, searched for the item room objects they hold.</summary>
    private readonly RoomObjectPlaceList _places;
    /// <summary>
    /// Connects the collectibles to room state and drawing, and to the room's block pairs, which its collectibles that
    /// show their own picture take in turn.
    /// </summary>
    internal CollectibleObjects(SharedGameState system, IRoomSamusAccess samus, IRoomFxMotion roomFx, IRoomObjectDrawing drawing, RoomObjectPlaceList places,
        CollectibleBlockPairs blockPairs, RoomObjectSoundQueue sounds)
    {
        _sounds = Ensure.NotNull(sounds);
        _system = system;
        _samus = samus;
        _roomFx = roomFx;
        _drawing = drawing;
        _places = places;
        _blockPairs = Ensure.NotNull(blockPairs);
    }

    /// <summary>Pickup events published by the current room-object step.</summary>
    private readonly List<CollectiblePickupEvent> _collectiblePickupEvents = new();
    /// <summary>The terrain block pairs the room's collectibles that show their own picture take in turn.</summary>
    private readonly CollectibleBlockPairs _blockPairs;
    /// <summary>The room objects' audio requests, which a pickup's fanfare joins.</summary>
    private readonly RoomObjectSoundQueue _sounds;
    /// <summary>Delays the Speed Booster Chozo lava motion write until message return.</summary>
    private bool _pendingSpeedBoosterPickupContinuation;

    /// <summary>Whether the room objects' step is waiting for its synchronous pickup message.</summary>
    internal bool HasPendingCollectibleMessage
    {
        get
        {
            foreach (RoomObjectPlace place in _places.FromLastPlace)
                if (place.Active && (place.Behaviour as CollectibleObject)?.Phase == CollectiblePhase.AwaitingMessage)
                    return true;
            return false;
        }
    }

    /// <summary>
    /// The pickup message has closed: each item awaiting it finishes its pickup within the same room-object update, an item
    /// showing its empty block before it leaves, a shot block's item leaving its block empty.
    /// </summary>
    internal void CompleteCollectibleMessage(RoomLevelData level)
    {
        foreach (RoomObjectPlace place in _places.FromLastPlace)
        {
            if (!place.Active || place.Behaviour is not CollectibleObject { Phase: CollectiblePhase.AwaitingMessage } item)
                continue;
            item.Phase = CollectiblePhase.ResumeAfterMessage;
            item.FinishPickup(_drawing, level, place);
        }
        CompleteSpeedBoosterPickupContinuation();
    }

    /// <summary>Applies the delayed lava-rise velocity after the Speed Booster message returns.</summary>
    private void CompleteSpeedBoosterPickupContinuation()
    {
        if (!_pendingSpeedBoosterPickupContinuation)
            return;
        _roomFx.SetLiquidSpeed(SpeedBoosterPickupDefinitions.LavaRiseVelocity);
        _pendingSpeedBoosterPickupContinuation = false;
    }

    /// <summary>Pickup publications produced during the most recent room-object step.</summary>
    public IReadOnlyList<CollectiblePickupEvent> CollectiblePickupEvents =>
        _collectiblePickupEvents;

    /// <summary>Every permanent-item room object the room holds, from the last place in update order back to the first.</summary>
    public IReadOnlyList<CollectibleObjectSnapshot> Collectibles => _places.FromLastPlace
        .Where(place => place.Active)
        .SelectMany(place => place.Behaviour is CollectibleObject { } item
            ? [new CollectibleObjectSnapshot(place.Block, item.Location, item.Kind)]
            : Array.Empty<CollectibleObjectSnapshot>())
        .ToArray();

    /// <summary>
    /// Publishes Samus contact with a visible item block. The item's touch setup
    /// finds the item room object sharing this block and arms its trigger timer;
    /// acquisition remains owned by the following room-object step.
    /// </summary>
    public bool TryNotifyCollectibleTouch(Position16<RoomBlocks> block)
    {
        foreach (RoomObjectPlace place in _places.FromLastPlace)
        {
            if (!place.Active || place.Block != block || place.Behaviour is not CollectibleObject item)
                continue;
            // The pickup message keeps both the visible block and the item that lies on it. As the message closes,
            // Samus's movement can touch that block before the room objects' update empties and removes the item. A touch
            // finds the item by its block whatever it is doing, so it is acknowledged here without being collected again
            // or disturbing what it does as the message closes.
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
    /// Triggers the Chozo orb or concealed item block at <paramref name="block"/> that a shot, bomb or grapple reached:
    /// whatever reached it, the item responds with the same trigger.
    /// </summary>
    public bool TryNotifyCollectibleProjectileHit(
        Position16<RoomBlocks> block)
    {
        foreach (RoomObjectPlace place in _places.FromLastPlace)
        {
            if (!place.Active || place.Block != block || place.Behaviour is not CollectibleObject item)
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

    /// <summary>Clears pickup publications at the start of a room-object step.</summary>
    internal void BeginCollectibleFrame() => _collectiblePickupEvents.Clear();

    /// <summary>
    /// Runs one permanent-item setup on the place selected by the shared room loader, for the item its placement
    /// (<paramref name="placed"/>) names the location of, as the collectible's definition and its pictures describe it.
    /// </summary>
    internal void SetupCollectible(
        RoomObjectRoom room,
        RoomObjectPlace place,
        InWorldCollectibleKind kind,
        CollectiblePresentation presentation,
        CollectedItemArgument? placed)
    {
        RoomLevelData level = room.Level;
        SharedGameState system = room.System;
        RoomCollisionBlock original = level.GetCollisionBlock(place.Block);
        bool collected = placed is not null && system.HasCollectedItem(placed.Location);
        CollectibleBlockPairs.Pair? blockPair = ShowsOwnPicture(kind) ? _blockPairs.Take() : null;

        RoomTerrainBlock setup = original.Terrain.WithCollision(
            presentation == CollectiblePresentation.ShotBlock ? RoomCollisionType.ShootableBlock : RoomCollisionType.Air);
        level.SetTerrain(place.Block, setup);
        level.SetBehavior(place.Block, RoomBlockBehavior.Reacting(ShootableReaction.CollectibleTrigger));
        place.RestoreTerrain = setup;

        CollectiblePhase phase = collected
            ? presentation == CollectiblePresentation.ShotBlock
                ? CollectiblePhase.CollectedShotBlock
                : CollectiblePhase.CollectedEmpty
            : presentation switch
            {
                CollectiblePresentation.Exposed => CollectiblePhase.Visible,
                // An item orb tests only whether its item was collected. A shell
                // broken on an earlier visit is rebuilt until the item is picked up.
                CollectiblePresentation.ChozoOrb => CollectiblePhase.ChozoOrb,
                CollectiblePresentation.ShotBlock => CollectiblePhase.ShotBlock,
                _ => throw new ArgumentOutOfRangeException(nameof(presentation)),
            };
        place.Become(new CollectibleObject(kind, presentation, blockPair, phase, placed?.Location));
    }

    /// <summary>
    /// Whether a collectible shows its own picture where it lies (#1452, #1538): every item but the four tanks, whose
    /// pictures are blocks every room loads.
    /// </summary>
    internal static bool ShowsOwnPicture(InWorldCollectibleKind kind) => kind is not (InWorldCollectibleKind.EnergyTank
        or InWorldCollectibleKind.MissileTank or InWorldCollectibleKind.SuperMissileTank or InWorldCollectibleKind.PowerBombTank);

    /// <summary>
    /// Collects <paramref name="item"/>, placed at <paramref name="place"/>: sets persistence and inventory before publishing
    /// the synchronous message and fanfare request.
    /// </summary>
    internal void Acquire(CollectibleObject item, RoomObjectPlace place)
    {
        Ensure.NotNull(item);
        Ensure.NotNull(place);
        IRoomSamus samus = _samus.Current
            ?? throw new InvalidOperationException(
                "A live collectible was triggered before the room owned Samus state.");
        SharedGameState system = _system;

        if (item.Location is { } location)
            system.CollectItem(location);
        ApplyCollectibleEffect(samus, item.Kind);

        var pickup = new CollectiblePickupEvent(
            item.Kind,
            PickupMessage(item.Kind),
            PickupConsequence(item.Kind));
        _collectiblePickupEvents.Add(pickup);
        // The item's cue starts the fanfare as it runs, before its message box opens.
        _sounds.QueuePermanentItemFanfare();
        _pendingSpeedBoosterPickupContinuation =
            item.Kind == InWorldCollectibleKind.SpeedBooster &&
            item.Presentation == CollectiblePresentation.ChozoOrb;

        // The item is granted before its message opens; it keeps its block and its place while the message shows, and
        // finishes its pickup once the message closes.
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
                // An energy tank adds its energy to her maximum and fills her health.
                samus.MaxHealth += SamusEnergyDefinitions.EnergyPerTank;
                samus.Health = samus.MaxHealth;
                return;
            case InWorldCollectibleKind.ReserveTank:
                samus.MaxReserveEnergy += SamusEnergyDefinitions.EnergyPerTank;
                if (samus.ReserveTankMode == ReserveTankMode.None)
                    samus.ReserveTankMode = ReserveTankMode.Auto;
                return;
            case InWorldCollectibleKind.MissileTank:
                samus.MaxMissiles += CollectibleCapacityDefinitions.MissileTankMissiles;
                samus.Missiles += CollectibleCapacityDefinitions.MissileTankMissiles;
                return;
            case InWorldCollectibleKind.SuperMissileTank:
                samus.MaxSuperMissiles += CollectibleCapacityDefinitions.SuperMissileTankSuperMissiles;
                samus.SuperMissiles += CollectibleCapacityDefinitions.SuperMissileTankSuperMissiles;
                return;
            case InWorldCollectibleKind.PowerBombTank:
                samus.MaxPowerBombs += CollectibleCapacityDefinitions.PowerBombTankPowerBombs;
                samus.PowerBombs += CollectibleCapacityDefinitions.PowerBombTankPowerBombs;
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
                // A Varia or Gravity Suit pickup clears the beam's charge as the suit is equipped, before its
                // message, so no charge is held through the fanfare or the suit's reveal that follows it.
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

    /// <summary>Maps an equipment pickup to the item it adds to the equipped and collected items.</summary>
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

    /// <summary>Maps a beam pickup to the beam it adds to the collected and equipped beams.</summary>
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
    private static CollectiblePickupConsequence PickupConsequence(InWorldCollectibleKind kind) => kind switch
    {
        InWorldCollectibleKind.ChargeBeam or InWorldCollectibleKind.IceBeam or InWorldCollectibleKind.WaveBeam or
            InWorldCollectibleKind.SpazerBeam or InWorldCollectibleKind.PlasmaBeam => CollectiblePickupConsequence.ShowsEquippedBeams,
        InWorldCollectibleKind.VariaSuit => CollectiblePickupConsequence.TransformsSuit(SamusSuitPickupKind.Varia),
        InWorldCollectibleKind.GravitySuit => CollectiblePickupConsequence.TransformsSuit(SamusSuitPickupKind.Gravity),
        InWorldCollectibleKind.EnergyTank or InWorldCollectibleKind.MissileTank or InWorldCollectibleKind.SuperMissileTank or
            InWorldCollectibleKind.PowerBombTank or InWorldCollectibleKind.ReserveTank or InWorldCollectibleKind.Bombs or
            InWorldCollectibleKind.HiJumpBoots or InWorldCollectibleKind.SpeedBooster or InWorldCollectibleKind.SpringBall or
            InWorldCollectibleKind.XrayScope or InWorldCollectibleKind.GrappleBeam or InWorldCollectibleKind.SpaceJump or
            InWorldCollectibleKind.ScrewAttack or InWorldCollectibleKind.MorphBall => CollectiblePickupConsequence.Nothing,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "No permanent item is of this kind."),
    };

    /// <summary>The message the pickup of <paramref name="kind"/> opens.</summary>
    private static GameplayMessageId PickupMessage(InWorldCollectibleKind kind) => kind switch
    {
        InWorldCollectibleKind.EnergyTank => GameplayMessageId.EnergyTank,
        InWorldCollectibleKind.MissileTank => GameplayMessageId.MissileTank,
        InWorldCollectibleKind.SuperMissileTank => GameplayMessageId.SuperMissileTank,
        InWorldCollectibleKind.PowerBombTank => GameplayMessageId.PowerBombTank,
        InWorldCollectibleKind.GrappleBeam => GameplayMessageId.GrappleBeam,
        InWorldCollectibleKind.XrayScope => GameplayMessageId.XrayScope,
        InWorldCollectibleKind.VariaSuit => GameplayMessageId.VariaSuit,
        InWorldCollectibleKind.SpringBall => GameplayMessageId.SpringBall,
        InWorldCollectibleKind.MorphBall => GameplayMessageId.MorphBall,
        InWorldCollectibleKind.ScrewAttack => GameplayMessageId.ScrewAttack,
        InWorldCollectibleKind.HiJumpBoots => GameplayMessageId.HiJumpBoots,
        InWorldCollectibleKind.SpaceJump => GameplayMessageId.SpaceJump,
        InWorldCollectibleKind.SpeedBooster => GameplayMessageId.SpeedBooster,
        InWorldCollectibleKind.ChargeBeam => GameplayMessageId.ChargeBeam,
        InWorldCollectibleKind.IceBeam => GameplayMessageId.IceBeam,
        InWorldCollectibleKind.WaveBeam => GameplayMessageId.WaveBeam,
        InWorldCollectibleKind.SpazerBeam => GameplayMessageId.SpazerBeam,
        InWorldCollectibleKind.PlasmaBeam => GameplayMessageId.PlasmaBeam,
        InWorldCollectibleKind.Bombs => GameplayMessageId.Bombs,
        InWorldCollectibleKind.GravitySuit => GameplayMessageId.GravitySuit,
        InWorldCollectibleKind.ReserveTank => GameplayMessageId.ReserveTank,
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
