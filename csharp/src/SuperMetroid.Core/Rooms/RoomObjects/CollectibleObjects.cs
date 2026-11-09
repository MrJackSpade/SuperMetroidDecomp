using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>In-world collectibles: item and tank setup, pickup, fanfare, and each collectible's picture where it lies.</summary>
[RoomLifetime]
internal sealed class CollectibleObjects : IRoomObjectFamilySource
{
    /// <summary>Registers the Collectible family's step.</summary>
    public void AddObjectFamilies(RoomObjectFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Add(RoomObjectFamily.Collectible, (slot, context) =>
            TryStepCollectible(context.Level, slot) ? RoomObjectStepOutcome.Consumed : RoomObjectStepOutcome.Shared);
    }

    /// <summary>The room's system and event state.</summary>
    private readonly SharedGameState? _system;
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The room's effect overlay FX.</summary>
    private readonly IRoomFxMotion? _roomFx;
    /// <summary>Updates level blocks and queues their tilemap draws.</summary>
    private readonly IRoomObjectDrawing _drawing;
    /// <summary>Enumerates active item PLMs by slot ordinal.</summary>
    private readonly RoomObjectSlotTable _slotTable;
    /// <summary>Receives item instruction sound requests.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Connects permanent-item PLMs to room state, drawing, and sound services.</summary>
    internal CollectibleObjects(SharedGameState? system, IRoomSamusAccess? samus, IRoomFxMotion? roomFx, IRoomObjectDrawing drawing, RoomObjectSlotTable slotTable, RoomObjectSoundQueue sounds)
    {
        _system = system;
        _samus = samus;
        _roomFx = roomFx;
        _drawing = drawing;
        _slotTable = slotTable;
        _sounds = sounds;
    }

    /// <summary>Pickup events published by the current PLM handler pass.</summary>
    private readonly List<CollectiblePickupEvent> _collectiblePickupEvents = new();
    /// <summary>The terrain block pairs the room's collectibles that show their own picture take in turn.</summary>
    private readonly CollectibleBlockPairs _blockPairs = new();
    /// <summary>One-shot fanfare edge independent of the retained pickup event list.</summary>
    private bool _collectibleFanfareRequested;
    /// <summary>Delays the Speed Booster Chozo lava motion write until message return.</summary>
    private bool _pendingSpeedBoosterPickupContinuation;

    /// <summary>Whether the PLM handler is waiting for its synchronous pickup message.</summary>
    internal bool HasPendingCollectibleMessage
    {
        get
        {
            foreach (RoomObjectSlot slot in _slotTable.Slots)
                if (slot.Active && (slot.Family as CollectibleObjectState)?.Phase == CollectiblePhase.AwaitingMessage)
                    return true;
            return false;
        }
    }

    /// <summary>
    /// The message returns into the item's suspended instruction list, which draws
    /// the empty block and deletes itself within the same room-object update.
    /// </summary>
    internal void CompleteCollectibleMessage(RoomLevelData level)
    {
        foreach (RoomObjectSlot slot in _slotTable.Slots)
        {
            if (!slot.Active || slot.Family is not CollectibleObjectState { Phase: CollectiblePhase.AwaitingMessage } item)
                continue;
            item.Phase = CollectiblePhase.ResumeAfterMessage;
            FinishCollectiblePickup(level, slot);
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

    /// <summary>Pickup publications produced during the most recent PLM handler pass.</summary>
    public IReadOnlyList<CollectiblePickupEvent> CollectiblePickupEvents =>
        _collectiblePickupEvents;

    /// <summary>
    /// Consumes the one-shot music request published by a permanent-item instruction.
    /// </summary>
    /// <remarks>
    /// The synchronous message pauses the room-object update, so the diagnostic pickup list
    /// intentionally remains visible during many frames. Audio must not infer an
    /// edge from that retained list or it will clear/requeue the fanfare every frame.
    /// </remarks>
    public bool ConsumeCollectibleFanfareRequest()
    {
        bool requested = _collectibleFanfareRequested;
        _collectibleFanfareRequested = false;
        return requested;
    }

    /// <summary>Every currently allocated permanent-item PLM, by slot ordinal.</summary>
    public IReadOnlyList<CollectibleObjectSnapshot> Collectibles => _slotTable.Slots
        .Where(slot => slot.Active)
        .SelectMany(slot => slot.Family is CollectibleObjectState { } item
            ? [new CollectibleObjectSnapshot(slot.BlockIndex, (slot.Argument as CollectedItemArgument)?.Location, item.Kind)]
            : Array.Empty<CollectibleObjectSnapshot>())
        .ToArray();

    /// <summary>
    /// Publishes Samus contact with a visible item block. The item's touch setup
    /// finds the item PLM sharing this block and arms its trigger timer;
    /// acquisition remains owned by the following PLM handler pass.
    /// </summary>
    public bool TryNotifyCollectibleTouch(int blockIndex)
    {
        foreach (RoomObjectSlot slot in _slotTable.Slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.Family is not CollectibleObjectState item)
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
        int blockIndex)
    {
        foreach (RoomObjectSlot slot in _slotTable.Slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.Family is not CollectibleObjectState item)
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

    /// <summary>Clears pickup publications at the start of a PLM handler frame.</summary>
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
        RoomCollisionBlock original = level.GetCollisionBlockByIndex(slot.BlockIndex);
        bool collected = slot.Argument is CollectedItemArgument placedItem &&
            system.HasCollectedItem(placedItem.Location);
        int? blockPair = ShowsOwnPicture(kind) ? _blockPairs.Take() : null;

        RoomTerrainBlock setup = original.Terrain.WithCollision(
            presentation == CollectiblePresentation.ShotBlock ? RoomCollisionType.ShootableBlock : RoomCollisionType.Air);
        level.SetTerrain(slot.BlockIndex, setup);
        level.SetBehavior(slot.BlockIndex, RoomBlockBehavior.Reacting(BlockReactionDefinitions.CollectibleTrigger));
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
        system.LoadedItemCount = WrappingCounter.IncrementWord(system.LoadedItemCount);
        slot.Family = new CollectibleObjectState(kind, presentation, blockPair, phase);
    }

    /// <summary>
    /// Whether a collectible shows its own picture where it lies (#1452, #1538): every item but the four tanks, whose
    /// pictures are blocks every room loads.
    /// </summary>
    internal static bool ShowsOwnPicture(InWorldCollectibleKind kind) => kind is not (InWorldCollectibleKind.EnergyTank
        or InWorldCollectibleKind.MissileTank or InWorldCollectibleKind.SuperMissileTank or InWorldCollectibleKind.PowerBombTank);

    /// <summary>
    /// Names the item and presentation of a permanent-item PLM; false for every other PLM. This public read-only seam
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

    /// <summary>Advances one item's reveal, pickup, message wait, or shot-block respawn list.</summary>
    internal bool TryStepCollectible(RoomLevelData level, RoomObjectSlot slot)
    {
        CollectibleObjectState? item = slot.Family as CollectibleObjectState;
        if (item is null)
            return false;

        // Collision runs before the room-object update. The shared item trigger written by setup
        // for touch and shot must therefore pre-empt an outstanding animation timer in this same
        // handler pass; otherwise touching an item can leave Samus embedded in its solid
        // type-$B word for up to four extra frames.
        bool triggerPreemptsTimer = item.Triggered && item.Phase is
            CollectiblePhase.Visible or
            CollectiblePhase.ShotBlockVisible or
            CollectiblePhase.ChozoOrb or
            CollectiblePhase.ShotBlock or
            CollectiblePhase.CollectedShotBlock;
        if (!triggerPreemptsTimer && item.Timer > 0)
        {
            item.Timer--;
            if (item.Timer > 0)
                return true;
        }

        switch (item.Phase)
        {
            case CollectiblePhase.AwaitingMessage:
                return true;

            case CollectiblePhase.ResumeAfterMessage:
                FinishCollectiblePickup(level, slot);
                return true;

            case CollectiblePhase.EmptyAwaitingDelete:
                // The empty item's program ends by deleting the item.
                slot.Delete();
                slot.Header = 0;
                return true;

            case CollectiblePhase.CollectedEmpty:
                DrawCollectible(level, slot, RoomObjectCollectibleDrawDefinitions.Empty);
                slot.Delete();
                slot.Header = 0;
                return true;

            case CollectiblePhase.Visible:
                if (item.Triggered)
                {
                    AcquireCollectible(slot);
                    return true;
                }
                DrawVisibleCollectible(level, slot, item);
                item.AnimationIndex ^= 1;
                item.Timer = 4;
                return true;

            case CollectiblePhase.ShotBlockVisible:
                if (item.Triggered)
                {
                    AcquireCollectible(slot);
                    return true;
                }
                DrawVisibleCollectible(level, slot, item);
                if (item.AnimationIndex == 1 && --item.VisibleFramePairsRemaining == 0)
                {
                    item.Phase = CollectiblePhase.ShotBlockReconceal;
                    item.AnimationIndex = 2;
                    item.Timer = 4;
                    return true;
                }
                item.AnimationIndex ^= 1;
                item.Timer = 4;
                return true;

            case CollectiblePhase.ChozoOrb:
                if (item.Triggered)
                {
                    // Breaking the shell persists nothing. The chozo-block-destroyed step
                    // belongs only to the unused chozo-block PLMs.
                    item.Triggered = false;
                    item.Phase = CollectiblePhase.ChozoOrbBurst;
                    item.AnimationIndex = 0;
                    DrawCollectible(level, slot, RoomObjectCollectibleDrawDefinitions.Empty);
                    item.Timer = 3;
                    return true;
                }
                DrawCollectible(level, slot, CollectibleObjectDraws.OrbFrame(item.AnimationIndex));
                item.Timer = item.AnimationIndex is 0 or 2 ? 20 : 10;
                item.AnimationIndex = (item.AnimationIndex + 1) & 3;
                return true;

            case CollectiblePhase.ChozoOrbBurst:
                item.AnimationIndex++;
                if (item.AnimationIndex == 1)
                {
                    DrawCollectible(level, slot, RoomObjectCollectibleDrawDefinitions.OrbBurst);
                    item.Timer = 3;
                    return true;
                }
                if (item.AnimationIndex == 2)
                {
                    DrawCollectible(level, slot, RoomObjectCollectibleDrawDefinitions.Empty);
                    item.Timer = 3;
                    return true;
                }
                item.Phase = CollectiblePhase.Visible;
                item.AnimationIndex = 0;
                goto case CollectiblePhase.Visible;

            case CollectiblePhase.ShotBlock:
            case CollectiblePhase.CollectedShotBlock:
                if (!item.Triggered)
                    return true;
                item.Triggered = false;
                item.WasCollectedBeforeReveal = item.Phase == CollectiblePhase.CollectedShotBlock;
                item.Phase = CollectiblePhase.ShotBlockReveal;
                item.AnimationIndex = 0;
                DrawCollectible(level, slot, CollectibleObjectDraws.ShotRevealFrame(0));
                _sounds.Queue(EnemySound.BlockCrumble, SoundRequestPriority.Normal);
                item.Timer = 4;
                return true;

            case CollectiblePhase.ShotBlockReveal:
                item.AnimationIndex++;
                if (item.AnimationIndex < 3)
                {
                    DrawCollectible(level, slot, CollectibleObjectDraws.ShotRevealFrame(item.AnimationIndex));
                    item.Timer = 4;
                    return true;
                }
                if (item.WasCollectedBeforeReveal)
                {
                    item.Phase = CollectiblePhase.CollectedShotBlockEmpty;
                    item.AnimationIndex = 0;
                    DrawCollectible(level, slot, RoomObjectCollectibleDrawDefinitions.Empty);
                    item.Timer = 8 * 22;
                    return true;
                }
                item.Phase = CollectiblePhase.ShotBlockVisible;
                item.AnimationIndex = 0;
                item.VisibleFramePairsRemaining = 22;
                goto case CollectiblePhase.ShotBlockVisible;

            case CollectiblePhase.ShotBlockReconceal:
                DrawCollectible(level, slot, CollectibleObjectDraws.ShotRevealFrame(item.AnimationIndex));
                item.AnimationIndex--;
                if (item.AnimationIndex >= 0)
                {
                    item.Timer = 4;
                    return true;
                }
                RoomObjectLevelWrites.DrawTerrain(level, slot.BlockIndex, slot.RestoreTerrain);
                item.Phase = CollectiblePhase.ShotBlock;
                return true;

            case CollectiblePhase.CollectedShotBlockEmpty:
                item.Phase = CollectiblePhase.CollectedShotBlockRespawn;
                item.AnimationIndex = 2;
                DrawCollectible(level, slot, ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, item.AnimationIndex));
                item.Timer = 4;
                return true;

            case CollectiblePhase.CollectedShotBlockRespawn:
                item.AnimationIndex--;
                if (item.AnimationIndex >= 0)
                {
                    DrawCollectible(level, slot, ShotBlockObjectDraws.Breakup(RoomObjectShotBlockDrawDefinitions.Shape.Single, item.AnimationIndex));
                    item.Timer = 4;
                    return true;
                }
                RoomObjectLevelWrites.DrawTerrain(level, slot.BlockIndex, slot.RestoreTerrain);
                item.Phase = CollectiblePhase.CollectedShotBlock;
                return true;

            default:
                throw new InvalidDataException(
                    $"Collectible {item.Kind} reached unsupported phase {item.Phase}.");
        }
    }

    /// <summary>
    /// Draws an item's visible frame: the terrain its kind and block pair write, and over it, for an item that shows its
    /// own picture, its kind's picture of that frame (#1538).
    /// </summary>
    private void DrawVisibleCollectible(RoomLevelData level, RoomObjectSlot slot, CollectibleObjectState item)
    {
        DrawCollectible(level, slot, CollectibleObjectDraws.VisibleFrame(item.Kind, item.AnimationIndex, item.BlockPair));
        if (item.BlockPair is not null)
            level.ShowCollectible(slot.BlockIndex, new CollectiblePicture(item.Kind, item.AnimationIndex));
    }

    /// <summary>Executes a compiled item draw instruction against the level.</summary>
    private void DrawCollectible(RoomLevelData level, RoomObjectSlot slot, RoomObjectDraw draw) =>
        _drawing.Draw(level, slot.BlockIndex, draw);

    /// <summary>Sets persistence and inventory before publishing the synchronous message and fanfare request.</summary>
    private void AcquireCollectible(RoomObjectSlot slot)
    {
        CollectibleObjectState item = (slot.Family as CollectibleObjectState)
            ?? throw new InvalidOperationException("A non-item PLM attempted acquisition.");
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
        _collectibleFanfareRequested = true;
        _pendingSpeedBoosterPickupContinuation =
            item.Kind == InWorldCollectibleKind.SpeedBooster &&
            item.Presentation == CollectiblePresentation.ChozoOrb;

        // The item instruction grants inventory before DisplayMessageBox, but its
        // following draw/delete instructions cannot run until that synchronous call
        // returns. Keep the existing tile and slot throughout the fanfare.
        item.Triggered = false;
        item.Timer = 0;
        item.Phase = CollectiblePhase.AwaitingMessage;
    }

    /// <summary>Draws the empty item after message return or begins the shot block's delayed respawn.</summary>
    private void FinishCollectiblePickup(RoomLevelData level, RoomObjectSlot slot)
    {
        CollectibleObjectState item = (slot.Family as CollectibleObjectState)
            ?? throw new InvalidOperationException("A non-item PLM resumed a pickup message.");
        if (item.Presentation == CollectiblePresentation.ShotBlock)
        {
            // After the message, the shot-block list draws empty for 22 eight-frame
            // iterations before the three four-frame respawn images and saved block.
            item.Triggered = false;
            item.WasCollectedBeforeReveal = true;
            item.Phase = CollectiblePhase.CollectedShotBlockEmpty;
            item.AnimationIndex = 0;
            DrawCollectible(level, slot, RoomObjectCollectibleDrawDefinitions.Empty);
            item.Timer = 8 * 22;
            return;
        }

        // The empty item draws for one frame; its delete runs on the
        // following room-object update.
        DrawCollectible(level, slot, RoomObjectCollectibleDrawDefinitions.Empty);
        item.Phase = CollectiblePhase.EmptyAwaitingDelete;
        item.Timer = 1;
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
                // PickUpEquipment and the synchronous message. It is not part of the later
                // transformation and therefore must already be clear throughout the fanfare.
                if (kind is InWorldCollectibleKind.VariaSuit or InWorldCollectibleKind.GravitySuit)
                    samus.BeamChargeCount.Clear();
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

    /// <summary>Tracks one physical item's presentation, terrain block pair, and instruction phase.</summary>
    internal sealed class CollectibleObjectState(
        InWorldCollectibleKind kind,
        CollectiblePresentation presentation,
        int? blockPair,
        CollectiblePhase phase) : RoomObjectFamilyState
    {
        /// <inheritdoc/>
        internal override RoomObjectFamily Family => RoomObjectFamily.Collectible;

        /// <summary>Inventory or capacity item granted when this PLM is acquired.</summary>
        public InWorldCollectibleKind Kind { get; } = kind;
        /// <summary>Exposed, Chozo orb, or shot-block reveal behavior.</summary>
        public CollectiblePresentation Presentation { get; } = presentation;
        /// <summary>
        /// The terrain block pair an item that shows its own picture writes its frames into (<see cref="CollectibleBlockPairs"/>);
        /// null for a tank, whose frames are its kind's own blocks.
        /// </summary>
        public int? BlockPair { get; } = blockPair;
        /// <summary>Current reveal, visible, message, empty, or respawn instruction phase.</summary>
        public CollectiblePhase Phase { get; set; } = phase;
        /// <summary>Frames remaining in the current item instruction hold.</summary>
        public int Timer { get; set; }
        /// <summary>Current visible or respawn animation frame index.</summary>
        [IntegerQuantity("animation frame index")]
        public int AnimationIndex { get; set; }
        /// <summary>Remaining paired visible frames in the item's animation loop.</summary>
        public int VisibleFramePairsRemaining { get; set; }
        /// <summary>Contact or projectile reaction pending for the next PLM handler pass.</summary>
        public bool Triggered { get; set; }
        /// <summary>Distinguishes a previously acquired concealed item during its reveal list.</summary>
        public bool WasCollectedBeforeReveal { get; set; }
    }
}
