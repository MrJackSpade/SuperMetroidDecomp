using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>In-world collectible PLMs: item and tank setup, pickup, fanfare and the dynamic collectible graphics slots.</summary>
internal sealed class CollectiblePlms : IPlmFamilySource
{
    /// <summary>Registers the Collectible family's step.</summary>
    public void RegisterPlmFamilies(PlmFamilyTable table)
    {
        Ensure.NotNull(table);
        table.Register(PlmFamily.Collectible, (slot, context) =>
            TryStepCollectible(context.Level, context.Streamer, slot, context.View) ? PlmStepOutcome.Consumed : PlmStepOutcome.Shared);
    }

    /// <summary>Supplies Samus, persistence, and room-FX owners at pickup time.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Updates level blocks and queues their tilemap draws.</summary>
    private readonly PlmBlockDrawing _drawing;
    /// <summary>Enumerates active item PLMs in native slot order.</summary>
    private readonly PlmSlotTable _slotTable;
    /// <summary>Receives item instruction sound requests.</summary>
    private readonly PlmSoundQueue _sounds;

    /// <summary>Connects permanent-item PLMs to room state, drawing, and sound services.</summary>
    internal CollectiblePlms(PlmRoomBindings bindings, PlmBlockDrawing drawing, PlmSlotTable slotTable, PlmSoundQueue sounds)
    {
        _bindings = bindings;
        _drawing = drawing;
        _slotTable = slotTable;
        _sounds = sounds;
    }

    /// <summary>Pickup events published by the current PLM handler pass.</summary>
    private readonly List<CollectiblePickupEvent> _collectiblePickupEvents = new();
    /// <summary>Next of four $100-byte dynamic collectible character allocations.</summary>
    private int _nextCollectibleGraphicsSlot;
    /// <summary>Most recent pickup retained across the synchronous message wait.</summary>
    private CollectiblePickupEvent? _lastCollectiblePickup;
    /// <summary>One-shot fanfare edge independent of the retained pickup event list.</summary>
    private bool _collectibleFanfareRequested;
    /// <summary>Delays the Speed Booster Chozo lava motion write until message return.</summary>
    private bool _pendingSpeedBoosterPickupContinuation;

    /// <summary>Whether the PLM handler is waiting for its synchronous pickup message.</summary>
    internal bool HasPendingCollectibleMessage
    {
        get
        {
            foreach (PlmSlot slot in _slotTable._slots)
                if (slot.Active && (slot.Family as CollectiblePlmState)?.Phase == CollectiblePhase.AwaitingMessage)
                    return true;
            return false;
        }
    }

    /// <summary>
    /// The bank-$85 message returns into the item's suspended instruction list, which draws
    /// the empty block and deletes itself within the same PLM_Handler call. Returns that
    /// draw's tilemap updates.
    /// </summary>
    internal IReadOnlyList<PlmTilemapUpdate> CompleteCollectibleMessage(RoomLevelData level, BackgroundTilemapStreamer streamer, PlmView view)
    {
        _drawing._tilemapUpdates.Clear();
        foreach (PlmSlot slot in _slotTable._slots)
        {
            if (!slot.Active || slot.Family is not CollectiblePlmState { Phase: CollectiblePhase.AwaitingMessage } item)
                continue;
            item.Phase = CollectiblePhase.ResumeAfterMessage;
            FinishCollectiblePickup(level, streamer, slot, view);
        }
        CompleteSpeedBoosterPickupContinuation();
        return _drawing._tilemapUpdates;
    }

    /// <summary>Applies the delayed lava-rise velocity after the Speed Booster message returns.</summary>
    private void CompleteSpeedBoosterPickupContinuation()
    {
        if (!_pendingSpeedBoosterPickupContinuation)
            return;
        RoomLayer3FxState fx = _bindings.RoomFx
            ?? throw new InvalidOperationException("Speed Booster pickup has no room-FX owner.");
        fx.ApplyCartridgeMotionWrites(
            packedYVelocity: SpeedBoosterPickupDefinitions.LavaRiseVelocity);
        _pendingSpeedBoosterPickupContinuation = false;
    }

    /// <summary>Pickup publications produced during the most recent PLM handler pass.</summary>
    public IReadOnlyList<CollectiblePickupEvent> CollectiblePickupEvents =>
        _collectiblePickupEvents;

    /// <summary>
    /// Consumes the one-shot music request published by a permanent-item instruction.
    /// </summary>
    /// <remarks>
    /// The synchronous bank-$85 message pauses PLM_Handler, so the diagnostic pickup list
    /// intentionally remains visible during many accepted NMIs. Audio must not infer an
    /// edge from that retained list or it will clear/requeue the fanfare every frame.
    /// </remarks>
    public bool ConsumeCollectibleFanfareRequest()
    {
        bool requested = _collectibleFanfareRequested;
        _collectibleFanfareRequested = false;
        return requested;
    }

    /// <summary>Every currently allocated permanent-item PLM in native slot order.</summary>
    public IReadOnlyList<CollectiblePlmSnapshot> Collectibles => _slotTable._slots
        .Where(slot => slot.Active)
        .SelectMany(slot => slot.Family is CollectiblePlmState { } item
            ? [new CollectiblePlmSnapshot(slot.HeaderPointer, slot.BlockIndex, slot.RoomArgument, item.Kind, item.GraphicsSlot)]
            : Array.Empty<CollectiblePlmSnapshot>())
        .ToArray();

    /// <summary>
    /// Publishes Samus contact with a visible type-$B/BTS-$45 item. Native setup
    /// <c>$84:EEAB</c> finds the item PLM sharing this block and writes <c>$00FF</c> to its
    /// trigger timer; acquisition remains owned by the following PLM handler pass.
    /// </summary>
    public bool TryNotifyCollectibleTouch(int blockIndex)
    {
        foreach (PlmSlot slot in _slotTable._slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.Family is not CollectiblePlmState item)
                continue;
            // The pickup message retains both the visible block and its physical owner.
            // On message return, movement can touch that block before PLM_Handler runs
            // the pending empty draw/delete. Native $EEAB matches the block index without
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
    /// The projectile family is retained for diagnostics; header $EED3 itself accepts the
    /// collision producer generically and reduces it to the same $00FF trigger word.
    /// </summary>
    public bool TryNotifyCollectibleProjectileHit(
        int blockIndex)
    {
        foreach (PlmSlot slot in _slotTable._slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.Family is not CollectiblePlmState item)
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

    /// <summary>Clears dynamic graphics allocation and pickup/message state for a new room.</summary>
    internal void ResetCollectibleState()
    {
        _collectiblePickupEvents.Clear();
        _nextCollectibleGraphicsSlot = 0;
        _lastCollectiblePickup = null;
        _collectibleFanfareRequested = false;
        _pendingSpeedBoosterPickupContinuation = false;
    }

    /// <summary>
    /// Runs one permanent-item setup on the physical slot selected by the shared room
    /// loader. The setup remains table-driven by the cartridge header and presentation.
    /// </summary>
    internal void SetupCollectibleSlot(
        PlmRoom room,
        PlmSlot slot,
        InWorldCollectibleKind kind,
        CollectiblePresentation presentation,
        RoomPlmDynamicCollectibleGraphic? suppliedGraphic)
    {
        (RoomLevelData level, BackgroundTilemapStreamer streamer, SnesVram vram, Bank80SystemState system) =
            (room.Level, room.Streamer, room.Vram, room.System);
        RoomCollisionBlock original = level.GetCollisionBlockByIndex(slot.BlockIndex);
        bool collected = unchecked((short)slot.RoomArgument) >= 0 &&
            system.HasCollectedItemBit(slot.RoomArgument);
        int graphicsSlot = kind >= InWorldCollectibleKind.Bombs
            ? LoadDynamicCollectibleGraphics(
                level, streamer, vram, kind, suppliedGraphic)
            : -1;

        ushort setupWord = new RoomLevelWord(original.LevelWord).WithCollisionType(
            presentation == CollectiblePresentation.ShotBlock ? RoomCollisionType.ShootableBlock : RoomCollisionType.Air).Raw;
        level.SetForegroundEntry(slot.BlockIndex, setupWord);
        level.SetBehavior(slot.BlockIndex, RoomBlockBehaviorValues.CollectibleTrigger);
        streamer.SetLevelEntry(slot.BlockIndex, setupWord);
        slot.RestoreLevelWord = setupWord;

        CollectiblePhase phase = collected
            ? presentation == CollectiblePresentation.ShotBlock
                ? CollectiblePhase.CollectedShotBlock
                : CollectiblePhase.CollectedEmpty
            : presentation switch
            {
                CollectiblePresentation.Exposed => CollectiblePhase.Visible,
                // Item orb lists test only the collected-item bit ($84:887C). A shell
                // broken on an earlier visit is rebuilt until the item is picked up.
                CollectiblePresentation.ChozoOrb => CollectiblePhase.ChozoOrb,
                CollectiblePresentation.ShotBlock => CollectiblePhase.ShotBlock,
                _ => throw new ArgumentOutOfRangeException(nameof(presentation)),
            };
        // Both native item setup paths increment the saved load counter, even for collected items.
        system.LoadedItemCount = unchecked((ushort)(system.LoadedItemCount + 1));
        slot.Family = new CollectiblePlmState(kind, presentation, graphicsSlot, phase);
    }

    /// <summary>
    /// Decodes one bank-$84 PLM header only when it belongs to one of the three complete
    /// permanent-item tables. This public read-only seam lets cartridge audits inventory
    /// room populations without duplicating the header-range ABI.
    /// </summary>
    public static bool TryIdentifyPermanentCollectible(
        PlmHeaderId header,
        out InWorldCollectibleKind kind,
        out CollectiblePresentation presentation)
    {
        if (TryDecodeCollectibleRange(
                header,
                PlmHeaderId.ExposedEnergyTank,
                out kind))
        {
            presentation = CollectiblePresentation.Exposed;
            return true;
        }
        if (TryDecodeCollectibleRange(header, PlmHeaderId.ChozoEnergyTank, out kind))
        {
            presentation = CollectiblePresentation.ChozoOrb;
            return true;
        }
        if (TryDecodeCollectibleRange(header, PlmHeaderId.ShotBlockEnergyTank, out kind))
        {
            presentation = CollectiblePresentation.ShotBlock;
            return true;
        }

        kind = default;
        presentation = default;
        return false;
    }

    /// <summary>Decodes an aligned four-byte entry in one permanent-item header table.</summary>
    private static bool TryDecodeCollectibleRange(
        PlmHeaderId header,
        PlmHeaderId firstHeader,
        out InWorldCollectibleKind kind)
    {
        int byteOffset = (int)header - (int)firstHeader;
        if (byteOffset >= 0 &&
            byteOffset < RoomPlmHeaders.PermanentCollectibleKindCount * 4 &&
            (byteOffset & 3) == 0)
        {
            kind = InWorldCollectibleKinds.AtHeaderEntry(byteOffset / 4);
            return true;
        }
        kind = default;
        return false;
    }

    /// <summary>Uploads two animation frames to the next VRAM slot and updates live block-definition words.</summary>
    private int LoadDynamicCollectibleGraphics(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        SnesVram vram,
        InWorldCollectibleKind kind,
        RoomPlmDynamicCollectibleGraphic? suppliedGraphic)
    {
        // Retail's presentation lists share one installed upload per kind. A
        // constructed population may supply an explicitly decoded graphic; neither
        // source grants the handler a bank-$84/$89 reader.
        RoomPlmDynamicCollectibleGraphic definition = suppliedGraphic ??
            _drawing.DynamicCollectibleArt?.Resolve(kind) ??
            RoomPlmDynamicCollectibleGraphicsDefinitions.Get(kind);
        ReadOnlyMemory<byte> graphics = definition.Tiles;
        ReadOnlyMemory<byte> paletteOffsets = definition.PaletteOffsets;

        int graphicsSlot = _nextCollectibleGraphicsSlot;
        _nextCollectibleGraphicsSlot = (_nextCollectibleGraphicsSlot + 1) & 3;

        // The 256 bytes contain two 16x16 animation frames, four 4bpp tiles apiece.
        new RoomGraphicsVram(vram).CollectibleSlot(graphicsSlot).Load(graphics.Span);

        int startingTileNumber = CollectibleDynamicGraphicsDefinitions.FirstTileNumber + graphicsSlot * 8;
        int firstDefinitionWord = CollectibleDynamicGraphicsDefinitions.FirstBlockDefinitionWord + graphicsSlot * 8;
        for (int child = 0; child < 8; child++)
        {
            byte palette = paletteOffsets.Span[child];
            ushort tilemapWord = unchecked((ushort)(
                startingTileNumber + child + (palette << 10)));
            level.SetBlockDefinitionWord(firstDefinitionWord + child, tilemapWord);
            // BackgroundTilemapStreamer owns the native staging source used by both the
            // immediate PLM draw and future camera rows/columns. It was constructed before
            // room PLM setup, so the cartridge's live TileTable write must reach that copy
            // in the same instruction—not merely RoomLevelData's debugger-facing array.
            streamer.SetBlockDefinitionWord(firstDefinitionWord + child, tilemapWord);
        }
        return graphicsSlot;
    }

    /// <summary>Advances one item's reveal, pickup, message wait, or shot-block respawn list.</summary>
    internal bool TryStepCollectible(RoomLevelData level, BackgroundTilemapStreamer streamer, PlmSlot slot, PlmView view)
    {
        CollectiblePlmState? item = slot.Family as CollectiblePlmState;
        if (item is null)
            return false;

        // Collision runs before PLM_Handler. The shared item trigger written by setup
        // $EEAB/$EED3 must therefore pre-empt an outstanding animation timer in this same
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
                FinishCollectiblePickup(level, streamer, slot, view);
                return true;

            case CollectiblePhase.EmptyAwaitingDelete:
                // InstList_PLM_EmptyItem's Instruction_PLM_Delete ($84:DFAD).
                slot.Active = false;
                slot.HeaderPointer = 0;
                return true;

            case CollectiblePhase.CollectedEmpty:
                DrawCollectible(level, streamer, slot, RoomPlmCollectibleDrawDefinitions.Empty, view);
                slot.Active = false;
                slot.HeaderPointer = 0;
                return true;

            case CollectiblePhase.Visible:
                if (item.Triggered)
                {
                    AcquireCollectible(slot);
                    return true;
                }
                DrawCollectible(level, streamer, slot, GetVisibleCollectibleDraw(item), view);
                item.AnimationIndex ^= 1;
                item.Timer = 4;
                return true;

            case CollectiblePhase.ShotBlockVisible:
                if (item.Triggered)
                {
                    AcquireCollectible(slot);
                    return true;
                }
                DrawCollectible(level, streamer, slot, GetVisibleCollectibleDraw(item), view);
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
                    // Breaking the shell persists nothing. $84:8865 (chozo block destroyed)
                    // belongs only to the unused chozo-block PLMs $D700/$D708.
                    item.Triggered = false;
                    item.Phase = CollectiblePhase.ChozoOrbBurst;
                    item.AnimationIndex = 0;
                    DrawCollectible(level, streamer, slot, RoomPlmCollectibleDrawDefinitions.Empty, view);
                    item.Timer = 3;
                    return true;
                }
                DrawCollectible(level, streamer, slot, RoomPlmCollectibleDrawDefinitions.OrbFrame(item.AnimationIndex), view);
                item.Timer = item.AnimationIndex is 0 or 2 ? 20 : 10;
                item.AnimationIndex = (item.AnimationIndex + 1) & 3;
                return true;

            case CollectiblePhase.ChozoOrbBurst:
                item.AnimationIndex++;
                if (item.AnimationIndex == 1)
                {
                    DrawCollectible(level, streamer, slot, RoomPlmCollectibleDrawDefinitions.OrbBurst, view);
                    item.Timer = 3;
                    return true;
                }
                if (item.AnimationIndex == 2)
                {
                    DrawCollectible(level, streamer, slot, RoomPlmCollectibleDrawDefinitions.Empty, view);
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
                DrawCollectible(level, streamer, slot, RoomPlmCollectibleDrawDefinitions.ShotRevealFrame(0), view);
                _sounds._soundRequests.Add(_sounds.CreateSoundRequest(SoundEffectLibrary2Sounds.PermanentItemAcquisition, 6));
                item.Timer = 4;
                return true;

            case CollectiblePhase.ShotBlockReveal:
                item.AnimationIndex++;
                if (item.AnimationIndex < 3)
                {
                    DrawCollectible(level, streamer, slot, RoomPlmCollectibleDrawDefinitions.ShotRevealFrame(item.AnimationIndex), view);
                    item.Timer = 4;
                    return true;
                }
                if (item.WasCollectedBeforeReveal)
                {
                    item.Phase = CollectiblePhase.CollectedShotBlockEmpty;
                    item.AnimationIndex = 0;
                    DrawCollectible(level, streamer, slot, RoomPlmCollectibleDrawDefinitions.Empty, view);
                    item.Timer = 8 * 22;
                    return true;
                }
                item.Phase = CollectiblePhase.ShotBlockVisible;
                item.AnimationIndex = 0;
                item.VisibleFramePairsRemaining = 22;
                goto case CollectiblePhase.ShotBlockVisible;

            case CollectiblePhase.ShotBlockReconceal:
                DrawCollectible(level, streamer, slot, RoomPlmCollectibleDrawDefinitions.ShotRevealFrame(item.AnimationIndex), view);
                item.AnimationIndex--;
                if (item.AnimationIndex >= 0)
                {
                    item.Timer = 4;
                    return true;
                }
                _drawing.DrawLevelWord(level, streamer, slot.BlockIndex, slot.RestoreLevelWord, view);
                item.Phase = CollectiblePhase.ShotBlock;
                return true;

            case CollectiblePhase.CollectedShotBlockEmpty:
                item.Phase = CollectiblePhase.CollectedShotBlockRespawn;
                item.AnimationIndex = 2;
                DrawCollectible(level, streamer, slot, unchecked((ushort)(RoomPlmShotBlockDrawDefinitions.SingleFrame0 + item.AnimationIndex * 6)), view);
                item.Timer = 4;
                return true;

            case CollectiblePhase.CollectedShotBlockRespawn:
                item.AnimationIndex--;
                if (item.AnimationIndex >= 0)
                {
                    DrawCollectible(level, streamer, slot, unchecked((ushort)(RoomPlmShotBlockDrawDefinitions.SingleFrame0 + item.AnimationIndex * 6)), view);
                    item.Timer = 4;
                    return true;
                }
                _drawing.DrawLevelWord(level, streamer, slot.BlockIndex, slot.RestoreLevelWord, view);
                item.Phase = CollectiblePhase.CollectedShotBlock;
                return true;

            default:
                throw new InvalidDataException(
                    $"Collectible {item.Kind} reached unsupported phase {item.Phase}.");
        }
    }

    /// <summary>Selects the visible frame for an item's kind, animation index, and dynamic art slot.</summary>
    private static ushort GetVisibleCollectibleDraw(CollectiblePlmState item) =>
        RoomPlmCollectibleDrawDefinitions.VisibleFrame(
            item.Kind, item.AnimationIndex, item.GraphicsSlot);

    /// <summary>Executes a compiled item draw instruction against the level and background streamer.</summary>
    private void DrawCollectible(RoomLevelData level, BackgroundTilemapStreamer streamer, PlmSlot slot, ushort drawPointer, PlmView view) =>
        _drawing.DrawPlmInstruction(level, streamer, slot.BlockIndex, drawPointer, view);

    /// <summary>Sets persistence and inventory before publishing the synchronous message and fanfare request.</summary>
    private void AcquireCollectible(PlmSlot slot)
    {
        CollectiblePlmState item = (slot.Family as CollectiblePlmState)
            ?? throw new InvalidOperationException("A non-item PLM attempted acquisition.");
        SamusState samus = _bindings.Samus?.Current
            ?? throw new InvalidOperationException(
                "A live collectible was triggered before the room owned Samus state.");
        Bank80SystemState system = _bindings.System
            ?? throw new InvalidOperationException("A live collectible has no persistence owner.");

        if (unchecked((short)slot.RoomArgument) >= 0)
            system.SetCollectedItemBit(slot.RoomArgument);
        ApplyCollectibleEffect(samus, item.Kind);

        var pickup = new CollectiblePickupEvent(
            item.Kind,
            GetMessageBoxIndex(item.Kind));
        _collectiblePickupEvents.Add(pickup);
        _lastCollectiblePickup = pickup;
        _collectibleFanfareRequested = true;
        _pendingSpeedBoosterPickupContinuation =
            item.Kind == InWorldCollectibleKind.SpeedBooster &&
            item.Presentation == CollectiblePresentation.ChozoOrb;

        // The item instruction grants inventory before DisplayMessageBox, but its
        // following draw/delete instructions cannot run until that synchronous call
        // returns. Keep the existing tile and physical slot throughout the fanfare.
        item.Triggered = false;
        item.Timer = 0;
        item.Phase = CollectiblePhase.AwaitingMessage;
    }

    /// <summary>Draws the empty item after message return or begins the shot block's delayed respawn.</summary>
    private void FinishCollectiblePickup(RoomLevelData level, BackgroundTilemapStreamer streamer, PlmSlot slot, PlmView view)
    {
        CollectiblePlmState item = (slot.Family as CollectiblePlmState)
            ?? throw new InvalidOperationException("A non-item PLM resumed a pickup message.");
        if (item.Presentation == CollectiblePresentation.ShotBlock)
        {
            // After the message, the shot-block list draws empty for 22 eight-frame
            // iterations before the three four-frame respawn images and saved block.
            item.Triggered = false;
            item.WasCollectedBeforeReveal = true;
            item.Phase = CollectiblePhase.CollectedShotBlockEmpty;
            item.AnimationIndex = 0;
            DrawCollectible(level, streamer, slot, RoomPlmCollectibleDrawDefinitions.Empty, view);
            item.Timer = 8 * 22;
            return;
        }

        // $84:DFA9: InstList_PLM_EmptyItem draws for one frame; its delete runs on the
        // following PLM_Handler pass.
        DrawCollectible(level, streamer, slot, RoomPlmCollectibleDrawDefinitions.Empty, view);
        item.Phase = CollectiblePhase.EmptyAwaitingDelete;
        item.Timer = 1;
    }

    /// <summary>Applies tank capacity, equipment, or beam ownership, including Spazer/Plasma exclusion.</summary>
    private static void ApplyCollectibleEffect(
        SamusState samus,
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
                if (samus.ReserveTankMode == 0)
                    samus.ReserveTankMode = 1;
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
                // Every Varia/Gravity presentation executes `$84:E29D` immediately before
                // PickUpEquipment and the synchronous message. It is not part of the later
                // transformation and therefore must already be clear throughout the fanfare.
                if (kind is InWorldCollectibleKind.VariaSuit or InWorldCollectibleKind.GravitySuit)
                    samus.ProjectileFlareCounter = 0;
                return;
            case InWorldCollectibleKind.ChargeBeam:
            case InWorldCollectibleKind.IceBeam:
            case InWorldCollectibleKind.WaveBeam:
            case InWorldCollectibleKind.SpazerBeam:
            case InWorldCollectibleKind.PlasmaBeam:
                SamusBeamFlags beamMask = GetBeamMask(kind);
                samus.CollectedBeams |= beamMask;
                samus.EquippedBeams |= beamMask;

                // Spazer and Plasma are the only mutually exclusive beam pair: $84:88B0 shifts the
                // collected beam onto its partner, so collecting either one unequips the other.
                if ((beamMask & SamusBeamFlags.Spazer) != 0)
                    samus.EquippedBeams &= ~SamusBeamFlags.Plasma;
                if ((beamMask & SamusBeamFlags.Plasma) != 0)
                    samus.EquippedBeams &= ~SamusBeamFlags.Spazer;
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined collectible kind.");
        }
    }

    /// <summary>Maps an equipment pickup to the native bit written in equipped and collected items.</summary>
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
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not an equipment collectible."),
    };

    /// <summary>Maps a beam pickup to its native bit for collected and equipped beams.</summary>
    private static SamusBeamFlags GetBeamMask(InWorldCollectibleKind kind) => kind switch
    {
        InWorldCollectibleKind.ChargeBeam => SamusBeamFlags.Charge,
        InWorldCollectibleKind.IceBeam => SamusBeamFlags.Ice,
        InWorldCollectibleKind.WaveBeam => SamusBeamFlags.Wave,
        InWorldCollectibleKind.SpazerBeam => SamusBeamFlags.Spazer,
        InWorldCollectibleKind.PlasmaBeam => SamusBeamFlags.Plasma,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };

    /// <summary>Selects the bank-$85 message shown synchronously after a permanent pickup.</summary>
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

    /// <summary>Tracks one physical item's presentation, graphics allocation, and instruction phase.</summary>
    internal sealed class CollectiblePlmState(
        InWorldCollectibleKind kind,
        CollectiblePresentation presentation,
        int graphicsSlot,
        CollectiblePhase phase) : PlmFamilyState
    {
        /// <inheritdoc/>
        internal override PlmFamily Family => PlmFamily.Collectible;

        /// <summary>Inventory or capacity item granted when this PLM is acquired.</summary>
        public InWorldCollectibleKind Kind { get; } = kind;
        /// <summary>Exposed, Chozo orb, or shot-block reveal behavior.</summary>
        public CollectiblePresentation Presentation { get; } = presentation;
        /// <summary>Dynamic character slot used by equipment and beam item art.</summary>
        public int GraphicsSlot { get; } = graphicsSlot;
        /// <summary>Current reveal, visible, message, empty, or respawn instruction phase.</summary>
        public CollectiblePhase Phase { get; set; } = phase;
        /// <summary>Frames remaining in the current item instruction hold.</summary>
        public int Timer { get; set; }
        /// <summary>Current visible or respawn animation frame index.</summary>
        public int AnimationIndex { get; set; }
        /// <summary>Remaining paired visible frames in the item's animation loop.</summary>
        public int VisibleFramePairsRemaining { get; set; }
        /// <summary>Contact or projectile reaction pending for the next PLM handler pass.</summary>
        public bool Triggered { get; set; }
        /// <summary>Distinguishes a previously acquired concealed item during its reveal list.</summary>
        public bool WasCollectedBeforeReveal { get; set; }
    }
}
