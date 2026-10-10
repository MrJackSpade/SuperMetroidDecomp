using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The permanent in-world item family in bank <c>$84:EED7-$EFD1</c>.
/// </summary>
/// <remarks>
/// Retail defines twenty-one item kinds in three parallel header tables: exposed,
/// Chozo-orb, and shot-block. The four-byte stride and three contiguous 21-entry ranges
/// are part of the cartridge ABI, so this translation derives identity from the header
/// instead of maintaining a room-name switch.
/// </remarks>
public sealed partial class RoomPlmSystem
{
    // Instruction $8764 rotates through four $100-byte character allocations. Its table
    // offsets are words into TileTable at $7E:A000 and destinations are VRAM word addresses.
    private const int DynamicBlockDefinitionFirstWord = 0x0470 / 2;
    private const int DynamicVramFirstByte = 0x3e00 * 2;

    private readonly List<CollectiblePickupEvent> _collectiblePickupEvents = new();
    private Bank80SystemState? _collectibleSystem;
    private Func<SamusState?>? _collectibleSamus;
    private int _nextCollectibleGraphicsSlot;
    private CollectiblePickupEvent? _lastCollectiblePickup;
    private bool _collectibleFanfareRequested;
    private bool _pendingSpeedBoosterPickupContinuation;

    /// <summary>Whether the PLM handler is waiting for its synchronous pickup message.</summary>
    internal bool HasPendingCollectibleMessage
    {
        get
        {
            foreach (PlmSlot slot in _slots)
                if (slot.Active && slot.Item?.Phase == CollectiblePhase.AwaitingMessage)
                    return true;
            return false;
        }
    }

    /// <summary>
    /// The bank-$85 message returns into the item's suspended instruction list, which draws
    /// the empty block and deletes itself within the same PLM_Handler call. Returns that
    /// draw's tilemap updates.
    /// </summary>
    internal IReadOnlyList<PlmTilemapUpdate> CompleteCollectibleMessage(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        _tilemapUpdates.Clear();
        foreach (PlmSlot slot in _slots)
        {
            if (!slot.Active || slot.Item?.Phase != CollectiblePhase.AwaitingMessage)
                continue;
            slot.Item.Phase = CollectiblePhase.ResumeAfterMessage;
            FinishCollectiblePickup(bus, level, streamer, slot, layer1XPosition, layer1YPosition, bg1XOffset);
        }
        CompleteSpeedBoosterPickupContinuation();
        return _tilemapUpdates;
    }

    private void CompleteSpeedBoosterPickupContinuation()
    {
        if (!_pendingSpeedBoosterPickupContinuation)
            return;
        RoomLayer3FxState fx = _speedBoosterEscapeFx
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
    public IReadOnlyList<CollectiblePlmSnapshot> Collectibles => _slots
        .Where(slot => slot.Active && slot.Item is not null)
        .Select(slot => new CollectiblePlmSnapshot(
            slot.HeaderPointer,
            slot.BlockIndex,
            slot.RoomArgument,
            slot.Item!.Kind,
            slot.Item.GraphicsSlot))
        .ToArray();

    /// <summary>
    /// Publishes Samus contact with a visible type-$B/BTS-$45 item. Native setup
    /// <c>$84:EEAB</c> finds the item PLM sharing this block and writes <c>$00FF</c> to its
    /// trigger timer; acquisition remains owned by the following PLM handler pass.
    /// </summary>
    public bool TryNotifyCollectibleTouch(int blockIndex)
    {
        foreach (PlmSlot slot in _slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.Item is null)
                continue;
            // The pickup message retains both the visible block and its physical owner.
            // On message return, movement can touch that block before PLM_Handler runs
            // the pending empty draw/delete. Native $EEAB matches the block index without
            // filtering the instruction phase; acknowledge this owner without collecting
            // again or disturbing its message-return continuation.
            if (slot.Item.Phase is CollectiblePhase.AwaitingMessage or CollectiblePhase.ResumeAfterMessage or
                CollectiblePhase.EmptyAwaitingDelete)
                return true;
            if (slot.Item.Phase is not (
                    CollectiblePhase.Visible or CollectiblePhase.ShotBlockVisible))
                return false;
            slot.Item.Triggered = true;
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
        int blockIndex,
        SamusProjectileTypeWord projectileType)
    {
        foreach (PlmSlot slot in _slots)
        {
            if (!slot.Active || slot.BlockIndex != blockIndex || slot.Item is null)
                continue;
            if (slot.Item.Phase is not (
                    CollectiblePhase.ChozoOrb or
                    CollectiblePhase.ShotBlock or
                    CollectiblePhase.CollectedShotBlock))
            {
                return false;
            }
            slot.Item.LastTriggerProjectileType = projectileType;
            slot.Item.Triggered = true;
            return true;
        }
        return false;
    }

    private void BeginCollectibleFrame() => _collectiblePickupEvents.Clear();

    private void ResetCollectibleState()
    {
        _collectiblePickupEvents.Clear();
        _collectibleSystem = null;
        _collectibleSamus = null;
        _nextCollectibleGraphicsSlot = 0;
        _lastCollectiblePickup = null;
        _collectibleFanfareRequested = false;
        _pendingSpeedBoosterPickupContinuation = false;
    }

    /// <summary>
    /// Runs one permanent-item setup on the physical slot selected by the shared room
    /// loader. The setup remains table-driven by the cartridge header and presentation.
    /// </summary>
    private void SetupCollectibleSlot(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        SnesVram vram,
        Bank80SystemState system,
        PlmSlot slot,
        InWorldCollectibleKind kind,
        CollectiblePresentation presentation,
        RoomPlmDynamicCollectibleGraphic? suppliedGraphic)
    {
        RoomCollisionBlock original = level.GetCollisionBlockByIndex(slot.BlockIndex);
        bool collected = unchecked((short)slot.RoomArgument) >= 0 &&
            system.HasCollectedItemBit(slot.RoomArgument);
        int graphicsSlot = kind >= InWorldCollectibleKind.Bombs
            ? LoadDynamicCollectibleGraphics(
                level, streamer, vram, kind, suppliedGraphic)
            : -1;

        ushort setupWord = unchecked((ushort)(original.LevelWord & 0x0fff));
        if (presentation == CollectiblePresentation.ShotBlock)
            setupWord |= 0xc000;
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
        slot.Item = new CollectiblePlmState(kind, presentation, graphicsSlot, phase);
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
            kind = (InWorldCollectibleKind)(byteOffset / 4);
            return true;
        }
        kind = default;
        return false;
    }

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
            DynamicCollectibleArt?.Resolve(kind) ??
            RoomPlmDynamicCollectibleGraphicsDefinitions.Get(kind);
        ReadOnlyMemory<byte> graphics = definition.Tiles;
        ReadOnlyMemory<byte> paletteOffsets = definition.PaletteOffsets;

        int graphicsSlot = _nextCollectibleGraphicsSlot;
        _nextCollectibleGraphicsSlot = (_nextCollectibleGraphicsSlot + 1) & 3;

        // The 256 bytes contain two 16x16 animation frames, four 4bpp tiles apiece.
        // Word-addressed SNES VRAM destinations become byte offsets in SnesVram.
        vram.LoadBytes(DynamicVramFirstByte + graphicsSlot * 0x100, graphics.Span);

        int startingTileNumber = 0x03e0 + graphicsSlot * 8;
        int firstDefinitionWord = DynamicBlockDefinitionFirstWord + graphicsSlot * 8;
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

    private bool TryStepCollectible(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        PlmSlot slot,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        CollectiblePlmState? item = slot.Item;
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
                FinishCollectiblePickup(bus, level, streamer, slot,
                    layer1XPosition, layer1YPosition, bg1XOffset);
                return true;

            case CollectiblePhase.EmptyAwaitingDelete:
                // InstList_PLM_EmptyItem's Instruction_PLM_Delete ($84:DFAD).
                slot.Active = false;
                slot.HeaderPointer = 0;
                return true;

            case CollectiblePhase.CollectedEmpty:
                DrawCollectible(
                    bus, level, streamer, slot, RoomPlmCollectibleDrawDefinitions.Empty,
                    layer1XPosition, layer1YPosition, bg1XOffset);
                slot.Active = false;
                slot.HeaderPointer = 0;
                return true;

            case CollectiblePhase.Visible:
                if (item.Triggered)
                {
                    AcquireCollectible(slot);
                    return true;
                }
                DrawCollectible(
                    bus, level, streamer, slot,
                    GetVisibleCollectibleDraw(item),
                    layer1XPosition, layer1YPosition, bg1XOffset);
                item.AnimationIndex ^= 1;
                item.Timer = 4;
                return true;

            case CollectiblePhase.ShotBlockVisible:
                if (item.Triggered)
                {
                    AcquireCollectible(slot);
                    return true;
                }
                DrawCollectible(
                    bus, level, streamer, slot,
                    GetVisibleCollectibleDraw(item),
                    layer1XPosition, layer1YPosition, bg1XOffset);
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
                    DrawCollectible(
                        bus, level, streamer, slot, RoomPlmCollectibleDrawDefinitions.Empty,
                        layer1XPosition, layer1YPosition, bg1XOffset);
                    item.Timer = 3;
                    return true;
                }
                DrawCollectible(
                    bus, level, streamer, slot,
                    RoomPlmCollectibleDrawDefinitions.OrbFrame(item.AnimationIndex),
                    layer1XPosition, layer1YPosition, bg1XOffset);
                item.Timer = item.AnimationIndex is 0 or 2 ? 20 : 10;
                item.AnimationIndex = (item.AnimationIndex + 1) & 3;
                return true;

            case CollectiblePhase.ChozoOrbBurst:
                item.AnimationIndex++;
                if (item.AnimationIndex == 1)
                {
                    DrawCollectible(
                        bus, level, streamer, slot, RoomPlmCollectibleDrawDefinitions.OrbBurst,
                        layer1XPosition, layer1YPosition, bg1XOffset);
                    item.Timer = 3;
                    return true;
                }
                if (item.AnimationIndex == 2)
                {
                    DrawCollectible(
                        bus, level, streamer, slot, RoomPlmCollectibleDrawDefinitions.Empty,
                        layer1XPosition, layer1YPosition, bg1XOffset);
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
                DrawCollectible(
                    bus, level, streamer, slot,
                    RoomPlmCollectibleDrawDefinitions.ShotRevealFrame(0),
                    layer1XPosition, layer1YPosition, bg1XOffset);
                _soundRequests.Add(CreateSoundRequest(SoundEffectLibrary2Sounds.PermanentItemAcquisition, 6));
                item.Timer = 4;
                return true;

            case CollectiblePhase.ShotBlockReveal:
                item.AnimationIndex++;
                if (item.AnimationIndex < 3)
                {
                    DrawCollectible(
                        bus, level, streamer, slot,
                        RoomPlmCollectibleDrawDefinitions.ShotRevealFrame(item.AnimationIndex),
                        layer1XPosition, layer1YPosition, bg1XOffset);
                    item.Timer = 4;
                    return true;
                }
                if (item.WasCollectedBeforeReveal)
                {
                    item.Phase = CollectiblePhase.CollectedShotBlockEmpty;
                    item.AnimationIndex = 0;
                    DrawCollectible(
                        bus, level, streamer, slot, RoomPlmCollectibleDrawDefinitions.Empty,
                        layer1XPosition, layer1YPosition, bg1XOffset);
                    item.Timer = 8 * 22;
                    return true;
                }
                item.Phase = CollectiblePhase.ShotBlockVisible;
                item.AnimationIndex = 0;
                item.VisibleFramePairsRemaining = 22;
                goto case CollectiblePhase.ShotBlockVisible;

            case CollectiblePhase.ShotBlockReconceal:
                DrawCollectible(
                    bus, level, streamer, slot,
                    RoomPlmCollectibleDrawDefinitions.ShotRevealFrame(item.AnimationIndex),
                    layer1XPosition, layer1YPosition, bg1XOffset);
                item.AnimationIndex--;
                if (item.AnimationIndex >= 0)
                {
                    item.Timer = 4;
                    return true;
                }
                DrawLevelWord(
                    level, streamer, slot.BlockIndex, slot.RestoreLevelWord,
                    layer1XPosition, layer1YPosition, bg1XOffset);
                item.Phase = CollectiblePhase.ShotBlock;
                return true;

            case CollectiblePhase.CollectedShotBlockEmpty:
                item.Phase = CollectiblePhase.CollectedShotBlockRespawn;
                item.AnimationIndex = 2;
                DrawCollectible(
                    bus, level, streamer, slot,
                    unchecked((ushort)(RoomPlmShotBlockDrawDefinitions.SingleFrame0 + item.AnimationIndex * 6)),
                    layer1XPosition, layer1YPosition, bg1XOffset);
                item.Timer = 4;
                return true;

            case CollectiblePhase.CollectedShotBlockRespawn:
                item.AnimationIndex--;
                if (item.AnimationIndex >= 0)
                {
                    DrawCollectible(
                        bus, level, streamer, slot,
                        unchecked((ushort)(RoomPlmShotBlockDrawDefinitions.SingleFrame0 + item.AnimationIndex * 6)),
                        layer1XPosition, layer1YPosition, bg1XOffset);
                    item.Timer = 4;
                    return true;
                }
                DrawLevelWord(
                    level, streamer, slot.BlockIndex, slot.RestoreLevelWord,
                    layer1XPosition, layer1YPosition, bg1XOffset);
                item.Phase = CollectiblePhase.CollectedShotBlock;
                return true;

            default:
                throw new InvalidDataException(
                    $"Collectible {item.Kind} reached unsupported phase {item.Phase}.");
        }
    }

    private static ushort GetVisibleCollectibleDraw(CollectiblePlmState item) =>
        RoomPlmCollectibleDrawDefinitions.VisibleFrame(
            item.Kind, item.AnimationIndex, item.GraphicsSlot);

    private void DrawCollectible(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        PlmSlot slot,
        ushort drawPointer,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset) =>
        DrawPlmInstruction(
            bus,
            level,
            streamer,
            slot.BlockIndex,
            drawPointer,
            layer1XPosition,
            layer1YPosition,
            bg1XOffset);

    private void AcquireCollectible(PlmSlot slot)
    {
        CollectiblePlmState item = slot.Item
            ?? throw new InvalidOperationException("A non-item PLM attempted acquisition.");
        SamusState samus = _collectibleSamus?.Invoke()
            ?? throw new InvalidOperationException(
                "A live collectible was triggered before the room owned Samus state.");
        Bank80SystemState system = _collectibleSystem
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

    private void FinishCollectiblePickup(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        PlmSlot slot,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        CollectiblePlmState item = slot.Item
            ?? throw new InvalidOperationException("A non-item PLM resumed a pickup message.");
        if (item.Presentation == CollectiblePresentation.ShotBlock)
        {
            // After the message, the shot-block list draws empty for 22 eight-frame
            // iterations before the three four-frame respawn images and saved block.
            item.Triggered = false;
            item.WasCollectedBeforeReveal = true;
            item.Phase = CollectiblePhase.CollectedShotBlockEmpty;
            item.AnimationIndex = 0;
            DrawCollectible(
                bus, level, streamer, slot, RoomPlmCollectibleDrawDefinitions.Empty,
                layer1XPosition, layer1YPosition, bg1XOffset);
            item.Timer = 8 * 22;
            return;
        }

        // $84:DFA9: InstList_PLM_EmptyItem draws for one frame; its delete runs on the
        // following PLM_Handler pass.
        DrawCollectible(
            bus, level, streamer, slot, RoomPlmCollectibleDrawDefinitions.Empty,
            layer1XPosition, layer1YPosition, bg1XOffset);
        item.Phase = CollectiblePhase.EmptyAwaitingDelete;
        item.Timer = 1;
    }

    private static void ApplyCollectibleEffect(
        SamusState samus,
        InWorldCollectibleKind kind)
    {
        switch (kind)
        {
            case InWorldCollectibleKind.EnergyTank:
                samus.MaxHealth = unchecked((ushort)(samus.MaxHealth + 100));
                samus.Health = samus.MaxHealth;
                return;
            case InWorldCollectibleKind.ReserveTank:
                samus.MaxReserveEnergy = unchecked((ushort)(samus.MaxReserveEnergy + 100));
                if (samus.ReserveTankMode == 0)
                    samus.ReserveTankMode = 1;
                return;
            case InWorldCollectibleKind.MissileTank:
                samus.MaxMissiles = unchecked((ushort)(samus.MaxMissiles + 5));
                samus.Missiles = unchecked((ushort)(samus.Missiles + 5));
                return;
            case InWorldCollectibleKind.SuperMissileTank:
                samus.MaxSuperMissiles = unchecked((ushort)(samus.MaxSuperMissiles + 5));
                samus.SuperMissiles = unchecked((ushort)(samus.SuperMissiles + 5));
                return;
            case InWorldCollectibleKind.PowerBombTank:
                samus.MaxPowerBombs = unchecked((ushort)(samus.MaxPowerBombs + 5));
                samus.PowerBombs = unchecked((ushort)(samus.PowerBombs + 5));
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
                ushort equipmentMask = GetEquipmentMask(kind);
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
                ushort beamMask = GetBeamMask(kind);
                samus.CollectedBeams |= beamMask;
                samus.EquippedBeams |= beamMask;

                // Spazer and Plasma are the only mutually exclusive beam pair. These are the
                // literal shifts used by $84:88B0, retained instead of naming either one as a
                // blanket replacement for all beams.
                samus.EquippedBeams &= unchecked((ushort)~((beamMask << 1) & 0x0008));
                samus.EquippedBeams &= unchecked((ushort)~((beamMask >> 1) & 0x0004));
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Undefined collectible kind.");
        }
    }

    private static ushort GetEquipmentMask(InWorldCollectibleKind kind) => kind switch
    {
        InWorldCollectibleKind.Bombs => (ushort)SamusEquipmentFlags.Bombs,
        InWorldCollectibleKind.HiJumpBoots => (ushort)SamusEquipmentFlags.HiJumpBoots,
        InWorldCollectibleKind.SpeedBooster => (ushort)SamusEquipmentFlags.SpeedBooster,
        InWorldCollectibleKind.SpringBall => (ushort)SamusEquipmentFlags.SpringBall,
        InWorldCollectibleKind.VariaSuit => (ushort)SamusEquipmentFlags.VariaSuit,
        InWorldCollectibleKind.GravitySuit => (ushort)SamusEquipmentFlags.GravitySuit,
        InWorldCollectibleKind.XrayScope => (ushort)SamusEquipmentFlags.XrayScope,
        InWorldCollectibleKind.GrappleBeam => (ushort)SamusEquipmentFlags.GrappleBeam,
        InWorldCollectibleKind.SpaceJump => (ushort)SamusEquipmentFlags.SpaceJump,
        InWorldCollectibleKind.ScrewAttack => (ushort)SamusEquipmentFlags.ScrewAttack,
        InWorldCollectibleKind.MorphBall => (ushort)SamusEquipmentFlags.MorphBall,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not an equipment collectible."),
    };

    private static ushort GetBeamMask(InWorldCollectibleKind kind) => kind switch
    {
        InWorldCollectibleKind.ChargeBeam => (ushort)SamusBeamFlags.Charge,
        InWorldCollectibleKind.IceBeam => (ushort)SamusBeamFlags.Ice,
        InWorldCollectibleKind.WaveBeam => (ushort)SamusBeamFlags.Wave,
        InWorldCollectibleKind.SpazerBeam => (ushort)SamusBeamFlags.Spazer,
        InWorldCollectibleKind.PlasmaBeam => (ushort)SamusBeamFlags.Plasma,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
    };

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

    private sealed class CollectiblePlmState(
        InWorldCollectibleKind kind,
        CollectiblePresentation presentation,
        int graphicsSlot,
        CollectiblePhase phase)
    {
        public InWorldCollectibleKind Kind { get; } = kind;
        public CollectiblePresentation Presentation { get; } = presentation;
        public int GraphicsSlot { get; } = graphicsSlot;
        public CollectiblePhase Phase { get; set; } = phase;
        public int Timer { get; set; }
        public int AnimationIndex { get; set; }
        public int VisibleFramePairsRemaining { get; set; }
        public bool Triggered { get; set; }
        public bool WasCollectedBeforeReveal { get; set; }
        public SamusProjectileTypeWord LastTriggerProjectileType { get; set; }
    }
}

/// <summary>Twenty-one entries shared by all three retail permanent-item header tables.</summary>
public enum InWorldCollectibleKind : byte
{
    /// <summary>Header-table entry zero: adds 100 maximum health and refills health to the new maximum.</summary>
    EnergyTank,
    /// <summary>Header-table entry one: adds five missiles to both capacity and current ammunition.</summary>
    MissileTank,
    /// <summary>Header-table entry two: adds five Super Missiles to both capacity and current ammunition.</summary>
    SuperMissileTank,
    /// <summary>Header-table entry three: adds five Power Bombs to both capacity and current ammunition.</summary>
    PowerBombTank,
    /// <summary>Header-table entry four: collects and equips the Bombs equipment bit.</summary>
    Bombs,
    /// <summary>Header-table entry five: collects and equips the Charge beam bit.</summary>
    ChargeBeam,
    /// <summary>Header-table entry six: collects and equips the Ice beam bit.</summary>
    IceBeam,
    /// <summary>Header-table entry seven: collects and equips the Hi-Jump Boots equipment bit.</summary>
    HiJumpBoots,
    /// <summary>Header-table entry eight: collects and equips Speed Booster; its Chozo presentation also resumes lava motion after the pickup message.</summary>
    SpeedBooster,
    /// <summary>Header-table entry nine: collects and equips the Wave beam bit.</summary>
    WaveBeam,
    /// <summary>Header-table entry ten: collects and equips Spazer while unequipping Plasma.</summary>
    SpazerBeam,
    /// <summary>Header-table entry eleven: collects and equips the Spring Ball equipment bit.</summary>
    SpringBall,
    /// <summary>Header-table entry twelve: collects and equips Varia Suit and clears the projectile flare counter before its message.</summary>
    VariaSuit,
    /// <summary>Header-table entry thirteen: collects and equips Gravity Suit and clears the projectile flare counter before its message.</summary>
    GravitySuit,
    /// <summary>Header-table entry fourteen: collects and equips the X-Ray Scope equipment bit.</summary>
    XrayScope,
    /// <summary>Header-table entry fifteen: collects and equips Plasma while unequipping Spazer.</summary>
    PlasmaBeam,
    /// <summary>Header-table entry sixteen: collects and equips the Grapple Beam equipment bit.</summary>
    GrappleBeam,
    /// <summary>Header-table entry seventeen: collects and equips the Space Jump equipment bit.</summary>
    SpaceJump,
    /// <summary>Header-table entry eighteen: collects and equips the Screw Attack equipment bit.</summary>
    ScrewAttack,
    /// <summary>Header-table entry nineteen: collects and equips the Morph Ball equipment bit.</summary>
    MorphBall,
    /// <summary>Header-table entry twenty: adds 100 reserve-energy capacity and selects automatic reserve mode if no mode was set.</summary>
    ReserveTank,
}

/// <summary>The three parallel item presentations encoded by bank-$84 header ranges.</summary>
public enum CollectiblePresentation : byte
{
    /// <summary>The item begins visible and can be acquired by Samus contact.</summary>
    Exposed,
    /// <summary>The item begins inside a projectile-triggered Chozo orb; breaking the shell does not persist between room visits.</summary>
    ChozoOrb,
    /// <summary>The item begins in a concealed shot block that temporarily reveals the item and later restores its saved block.</summary>
    ShotBlock,
}

/// <summary>Debugger-visible phase of one translated permanent-item PLM.</summary>
public enum CollectiblePhase : byte
{
    /// <summary>A previously collected exposed or orb item awaits its empty draw and immediate slot deletion.</summary>
    CollectedEmpty,
    /// <summary>The exposed item alternates its two visible frames and accepts a touch trigger for acquisition.</summary>
    Visible,
    /// <summary>The intact Chozo orb cycles its four shell frames while waiting for a projectile trigger.</summary>
    ChozoOrb,
    /// <summary>The triggered Chozo orb runs its timed empty/burst draws before revealing the item.</summary>
    ChozoOrbBurst,
    /// <summary>An uncollected item is concealed in its saved shot block and waits for a projectile trigger.</summary>
    ShotBlock,
    /// <summary>The shot block runs three reveal frames before showing either the item or its already-collected empty space.</summary>
    ShotBlockReveal,
    /// <summary>The revealed item accepts touch acquisition during twenty-two pairs of four-frame animation draws.</summary>
    ShotBlockVisible,
    /// <summary>An uncollected revealed item runs the reverse reveal sequence and restores its concealed shot block.</summary>
    ShotBlockReconceal,
    /// <summary>A previously collected item's restored shot block waits for another projectile trigger.</summary>
    CollectedShotBlock,
    /// <summary>The collected shot block remains empty for twenty-two eight-frame intervals before respawning.</summary>
    CollectedShotBlockEmpty,
    /// <summary>The collected shot block runs three four-frame respawn draws before restoring its saved level word.</summary>
    CollectedShotBlockRespawn,
    /// <summary>Inventory and persistence have been granted; the physical slot and drawn tile remain while the synchronous pickup message runs.</summary>
    AwaitingMessage,
    /// <summary>The pickup message has returned and the suspended item list is ready for its empty draw or shot-block continuation.</summary>
    ResumeAfterMessage,
    /// <summary>The empty draw has run; the instruction list deletes the PLM next pass.</summary>
    EmptyAwaitingDelete,
}

/// <summary>Stable debugger view of one occupied permanent-item PLM slot.</summary>
/// <param name="Header">The slot's native bank-$84 PLM header offset, retaining its kind and presentation identity.</param>
/// <param name="BlockIndex">The item owner's foreground block index in room level data.</param>
/// <param name="RoomArgument">The native room-population argument; nonnegative signed values select persistent collected-item bits.</param>
/// <param name="Kind">The permanent item kind decoded from the header's position in its presentation table.</param>
/// <param name="GraphicsSlot">The rotating dynamic-art allocation from zero through three, or -1 for an item using static art.</param>
public readonly record struct CollectiblePlmSnapshot(
    PlmHeaderId Header,
    int BlockIndex,
    ushort RoomArgument,
    InWorldCollectibleKind Kind,
    int GraphicsSlot);

/// <summary>One cartridge-defined permanent pickup completed by the current PLM pass.</summary>
/// <param name="Kind">The item whose inventory effect and collected-item persistence have been applied.</param>
/// <param name="MessageBoxIndex">The native gameplay-message identity requested for the synchronous pickup notification.</param>
public readonly record struct CollectiblePickupEvent(
    InWorldCollectibleKind Kind,
    GameplayMessageId MessageBoxIndex);
