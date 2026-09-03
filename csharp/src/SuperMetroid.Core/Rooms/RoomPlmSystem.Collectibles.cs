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
    private const ushort FirstExposedCollectibleHeader = 0xeed7;
    private const ushort FirstChozoCollectibleHeader = 0xef2b;
    private const ushort FirstShotBlockCollectibleHeader = 0xef7f;
    private const int CollectibleKindCount = 21;

    // These draw lists are cartridge-authored one-block records. Routing every visual
    // mutation through DrawRomInstruction retains the exact level word, collision nibble,
    // tile number, flip flags, clipping window, and live BG1 update path.
    private const ushort EmptyCollectibleDraw = 0xa2b5;
    private const ushort ChozoOrbFrame0Draw = 0xa2c7;
    private const ushort ChozoOrbFrame1Draw = 0xa2cd;
    private const ushort ChozoOrbFrame2Draw = 0xa2d3;
    private const ushort ChozoOrbBurstDraw = 0xa2d9;
    private const ushort FirstTankFrame0Draw = 0xa2df;
    private const ushort ShotBlockRevealFrame0Draw = 0xa3dd;
    private const ushort RespawnBlockFrame0Draw = 0xa345;
    private const ushort DynamicItemFrame0Table = 0xe05f;
    private const ushort DynamicItemFrame1Table = 0xe077;
    private const ushort LoadItemGraphicsInstruction = 0x8764;

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

    /// <summary>Pickup publications produced during the most recent PLM handler pass.</summary>
    public IReadOnlyList<CollectiblePickupEvent> CollectiblePickupEvents =>
        _collectiblePickupEvents;

    /// <summary>Most recent pickup retained for debugger watches after its frame ends.</summary>
    public CollectiblePickupEvent? LastCollectiblePickup => _lastCollectiblePickup;

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
            slot.Item.Presentation,
            slot.Item.Phase,
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
    public bool TryNotifyCollectibleProjectileHit(int blockIndex, ushort projectileType)
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
    }

    /// <summary>
    /// Runs one permanent-item setup on the physical slot selected by the shared room
    /// loader. The setup remains table-driven by the cartridge header and presentation.
    /// </summary>
    private void SetupCollectibleSlot(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        SnesVram vram,
        Bank80SystemState system,
        PlmSlot slot,
        InWorldCollectibleKind kind,
        CollectiblePresentation presentation)
    {
        RoomCollisionBlock original = level.GetCollisionBlockByIndex(slot.BlockIndex);
        bool collected = unchecked((short)slot.RoomArgument) >= 0 &&
            system.HasCollectedItemBit(slot.RoomArgument);
        bool chozoOrbOpened = presentation == CollectiblePresentation.ChozoOrb &&
            unchecked((short)slot.RoomArgument) >= 0 &&
            system.HasRoomChozoBit(slot.RoomArgument);
        int graphicsSlot = kind >= InWorldCollectibleKind.Bombs
            ? LoadDynamicCollectibleGraphics(
                bus, level, streamer, vram, slot.HeaderPointer)
            : -1;

        ushort setupWord = unchecked((ushort)(original.LevelWord & 0x0fff));
        if (presentation == CollectiblePresentation.ShotBlock)
            setupWord |= 0xc000;
        level.SetForegroundEntry(slot.BlockIndex, setupWord);
        level.SetBehavior(slot.BlockIndex, 0x45);
        streamer.SetLevelEntry(slot.BlockIndex, setupWord);
        slot.RestoreLevelWord = setupWord;

        CollectiblePhase phase = collected
            ? presentation == CollectiblePresentation.ShotBlock
                ? CollectiblePhase.CollectedShotBlock
                : CollectiblePhase.CollectedEmpty
            : presentation switch
            {
                CollectiblePresentation.Exposed => CollectiblePhase.Visible,
                CollectiblePresentation.ChozoOrb => chozoOrbOpened
                    ? CollectiblePhase.Visible
                    : CollectiblePhase.ChozoOrb,
                CollectiblePresentation.ShotBlock => CollectiblePhase.ShotBlock,
                _ => throw new ArgumentOutOfRangeException(nameof(presentation)),
            };
        slot.Item = new CollectiblePlmState(kind, presentation, graphicsSlot, phase);
    }

    /// <summary>
    /// Decodes one bank-$84 PLM header only when it belongs to one of the three complete
    /// permanent-item tables. This public read-only seam lets cartridge audits inventory
    /// room populations without duplicating the header-range ABI.
    /// </summary>
    public static bool TryIdentifyPermanentCollectible(
        ushort header,
        out InWorldCollectibleKind kind,
        out CollectiblePresentation presentation)
    {
        if (TryDecodeCollectibleRange(
                header,
                FirstExposedCollectibleHeader,
                out kind))
        {
            presentation = CollectiblePresentation.Exposed;
            return true;
        }
        if (TryDecodeCollectibleRange(header, FirstChozoCollectibleHeader, out kind))
        {
            presentation = CollectiblePresentation.ChozoOrb;
            return true;
        }
        if (TryDecodeCollectibleRange(header, FirstShotBlockCollectibleHeader, out kind))
        {
            presentation = CollectiblePresentation.ShotBlock;
            return true;
        }

        kind = default;
        presentation = default;
        return false;
    }

    private static bool TryDecodeCollectibleRange(
        ushort header,
        ushort firstHeader,
        out InWorldCollectibleKind kind)
    {
        int byteOffset = header - firstHeader;
        if (byteOffset >= 0 && byteOffset < CollectibleKindCount * 4 &&
            (byteOffset & 3) == 0)
        {
            kind = (InWorldCollectibleKind)(byteOffset / 4);
            return true;
        }
        kind = default;
        return false;
    }

    private int LoadDynamicCollectibleGraphics(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        SnesVram vram,
        ushort header)
    {
        ushort instructionList = ReadBank84Word(bus, unchecked((ushort)(header + 2)));
        ushort opcode = ReadBank84Word(bus, instructionList);
        if (opcode != LoadItemGraphicsInstruction)
        {
            throw new InvalidDataException(
                $"Collectible header $84:{header:X4} begins with ${opcode:X4}, not item-GFX load $8764.");
        }

        int graphicsSlot = _nextCollectibleGraphicsSlot;
        _nextCollectibleGraphicsSlot = (_nextCollectibleGraphicsSlot + 1) & 3;
        ushort graphicsPointer = ReadBank84Word(
            bus,
            unchecked((ushort)(instructionList + 2)));

        // The source is exactly $100 raw bytes in bank $89: two 16x16 animation frames,
        // four 4bpp tiles apiece. Word-addressed SNES VRAM destinations become byte
        // offsets in SnesVram.
        var graphics = new byte[0x100];
        for (int index = 0; index < graphics.Length; index++)
        {
            graphics[index] = bus.ReadByte(
                0x890000 | unchecked((ushort)(graphicsPointer + index)));
        }
        vram.LoadBytes(DynamicVramFirstByte + graphicsSlot * 0x100, graphics);

        int startingTileNumber = 0x03e0 + graphicsSlot * 8;
        int firstDefinitionWord = DynamicBlockDefinitionFirstWord + graphicsSlot * 8;
        for (int child = 0; child < 8; child++)
        {
            byte palette = bus.ReadByte(
                0x840000 | unchecked((ushort)(instructionList + 4 + child)));
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
            case CollectiblePhase.CollectedEmpty:
                DrawCollectible(
                    bus, level, streamer, slot, EmptyCollectibleDraw,
                    layer1XPosition, layer1YPosition, bg1XOffset);
                slot.Active = false;
                slot.HeaderPointer = 0;
                return true;

            case CollectiblePhase.Visible:
                if (item.Triggered)
                {
                    AcquireCollectible(bus, level, streamer, slot,
                        layer1XPosition, layer1YPosition, bg1XOffset);
                    return true;
                }
                DrawCollectible(
                    bus, level, streamer, slot,
                    GetVisibleCollectibleDraw(bus, item),
                    layer1XPosition, layer1YPosition, bg1XOffset);
                item.AnimationIndex ^= 1;
                item.Timer = 4;
                return true;

            case CollectiblePhase.ShotBlockVisible:
                if (item.Triggered)
                {
                    AcquireCollectible(bus, level, streamer, slot,
                        layer1XPosition, layer1YPosition, bg1XOffset);
                    return true;
                }
                DrawCollectible(
                    bus, level, streamer, slot,
                    GetVisibleCollectibleDraw(bus, item),
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
                    // The native orb list executes $84:8865 as soon as the shell is
                    // broken. Leaving and re-entering before touching the item must
                    // therefore restore the exposed pickup, not rebuild the orb.
                    if (unchecked((short)slot.RoomArgument) >= 0)
                    {
                        (_collectibleSystem ?? throw new InvalidOperationException(
                            "A live Chozo collectible has no persistence owner."))
                            .SetRoomChozoBit(slot.RoomArgument);
                    }
                    item.Triggered = false;
                    item.Phase = CollectiblePhase.ChozoOrbBurst;
                    item.AnimationIndex = 0;
                    DrawCollectible(
                        bus, level, streamer, slot, EmptyCollectibleDraw,
                        layer1XPosition, layer1YPosition, bg1XOffset);
                    item.Timer = 3;
                    return true;
                }
                DrawCollectible(
                    bus, level, streamer, slot,
                    item.AnimationIndex switch
                    {
                        0 => ChozoOrbFrame0Draw,
                        1 => ChozoOrbFrame1Draw,
                        2 => ChozoOrbFrame2Draw,
                        _ => ChozoOrbFrame1Draw,
                    },
                    layer1XPosition, layer1YPosition, bg1XOffset);
                item.Timer = item.AnimationIndex is 0 or 2 ? 20 : 10;
                item.AnimationIndex = (item.AnimationIndex + 1) & 3;
                return true;

            case CollectiblePhase.ChozoOrbBurst:
                item.AnimationIndex++;
                if (item.AnimationIndex == 1)
                {
                    DrawCollectible(
                        bus, level, streamer, slot, ChozoOrbBurstDraw,
                        layer1XPosition, layer1YPosition, bg1XOffset);
                    item.Timer = 3;
                    return true;
                }
                if (item.AnimationIndex == 2)
                {
                    DrawCollectible(
                        bus, level, streamer, slot, EmptyCollectibleDraw,
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
                    bus, level, streamer, slot, ShotBlockRevealFrame0Draw,
                    layer1XPosition, layer1YPosition, bg1XOffset);
                _soundRequests.Add(new PlmSoundRequest(2, 0x0a, 6));
                item.Timer = 4;
                return true;

            case CollectiblePhase.ShotBlockReveal:
                item.AnimationIndex++;
                if (item.AnimationIndex < 3)
                {
                    DrawCollectible(
                        bus, level, streamer, slot,
                        unchecked((ushort)(ShotBlockRevealFrame0Draw + item.AnimationIndex * 6)),
                        layer1XPosition, layer1YPosition, bg1XOffset);
                    item.Timer = 4;
                    return true;
                }
                if (item.WasCollectedBeforeReveal)
                {
                    item.Phase = CollectiblePhase.CollectedShotBlockEmpty;
                    item.AnimationIndex = 0;
                    DrawCollectible(
                        bus, level, streamer, slot, EmptyCollectibleDraw,
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
                    unchecked((ushort)(ShotBlockRevealFrame0Draw + item.AnimationIndex * 6)),
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
                    unchecked((ushort)(RespawnBlockFrame0Draw + item.AnimationIndex * 6)),
                    layer1XPosition, layer1YPosition, bg1XOffset);
                item.Timer = 4;
                return true;

            case CollectiblePhase.CollectedShotBlockRespawn:
                item.AnimationIndex--;
                if (item.AnimationIndex >= 0)
                {
                    DrawCollectible(
                        bus, level, streamer, slot,
                        unchecked((ushort)(RespawnBlockFrame0Draw + item.AnimationIndex * 6)),
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

    private static ushort GetVisibleCollectibleDraw(
        ISnesAddressSpace bus,
        CollectiblePlmState item)
    {
        if (item.Kind <= InWorldCollectibleKind.PowerBombTank)
        {
            return unchecked((ushort)(
                FirstTankFrame0Draw + (int)item.Kind * 12 + item.AnimationIndex * 6));
        }

        ushort table = item.AnimationIndex == 0
            ? DynamicItemFrame0Table
            : DynamicItemFrame1Table;
        return ReadBank84Word(
            bus,
            unchecked((ushort)(table + item.GraphicsSlot * 2)));
    }

    private void DrawCollectible(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        PlmSlot slot,
        ushort drawPointer,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset) =>
        DrawRomInstruction(
            bus,
            level,
            streamer,
            slot.BlockIndex,
            drawPointer,
            layer1XPosition,
            layer1YPosition,
            bg1XOffset);

    private void AcquireCollectible(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        PlmSlot slot,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
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
            item.Presentation,
            slot.RoomArgument,
            GetMessageBoxIndex(item.Kind),
            slot.BlockIndex,
            item.LastTriggerProjectileType);
        _collectiblePickupEvents.Add(pickup);
        _lastCollectiblePickup = pickup;
        _collectibleFanfareRequested = true;

        if (item.Presentation == CollectiblePresentation.ShotBlock)
        {
            // After the message, the shot-block list draws empty for 22 eight-frame
            // iterations before the three four-frame respawn images and saved block.
            item.Triggered = false;
            item.WasCollectedBeforeReveal = true;
            item.Phase = CollectiblePhase.CollectedShotBlockEmpty;
            item.AnimationIndex = 0;
            DrawCollectible(
                bus, level, streamer, slot, EmptyCollectibleDraw,
                layer1XPosition, layer1YPosition, bg1XOffset);
            item.Timer = 8 * 22;
            return;
        }

        DrawCollectible(
            bus, level, streamer, slot, EmptyCollectibleDraw,
            layer1XPosition, layer1YPosition, bg1XOffset);
        slot.Active = false;
        slot.HeaderPointer = 0;
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
        }

        ushort equipmentMask = GetEquipmentMask(kind);
        if (equipmentMask != 0)
        {
            samus.EquippedItems |= equipmentMask;
            samus.CollectedItems |= equipmentMask;
            // Every Varia/Gravity presentation executes `$84:E29D` immediately before
            // PickUpEquipment and the synchronous message. It is not part of the later
            // transformation and therefore must already be clear throughout the fanfare.
            if (kind is InWorldCollectibleKind.VariaSuit or InWorldCollectibleKind.GravitySuit)
                samus.ProjectileFlareCounter = 0;
            return;
        }

        ushort beamMask = GetBeamMask(kind);
        samus.CollectedBeams |= beamMask;
        samus.EquippedBeams |= beamMask;

        // Spazer and Plasma are the only mutually exclusive beam pair. These are the
        // literal shifts used by $84:88B0, retained instead of naming either one as a
        // blanket replacement for all beams.
        samus.EquippedBeams &= unchecked((ushort)~((beamMask << 1) & 0x0008));
        samus.EquippedBeams &= unchecked((ushort)~((beamMask >> 1) & 0x0004));
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
        InWorldCollectibleKind.EnergyTank or InWorldCollectibleKind.ReserveTank or
        InWorldCollectibleKind.MissileTank or InWorldCollectibleKind.SuperMissileTank or
        InWorldCollectibleKind.PowerBombTank or InWorldCollectibleKind.ChargeBeam or
        InWorldCollectibleKind.IceBeam or InWorldCollectibleKind.WaveBeam or
        InWorldCollectibleKind.SpazerBeam or InWorldCollectibleKind.PlasmaBeam => 0,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
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

    private static byte GetMessageBoxIndex(InWorldCollectibleKind kind) => kind switch
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
        public ushort LastTriggerProjectileType { get; set; }
    }
}

/// <summary>Twenty-one entries shared by all three retail permanent-item header tables.</summary>
public enum InWorldCollectibleKind : byte
{
    EnergyTank,
    MissileTank,
    SuperMissileTank,
    PowerBombTank,
    Bombs,
    ChargeBeam,
    IceBeam,
    HiJumpBoots,
    SpeedBooster,
    WaveBeam,
    SpazerBeam,
    SpringBall,
    VariaSuit,
    GravitySuit,
    XrayScope,
    PlasmaBeam,
    GrappleBeam,
    SpaceJump,
    ScrewAttack,
    MorphBall,
    ReserveTank,
}

/// <summary>The three parallel item presentations encoded by bank-$84 header ranges.</summary>
public enum CollectiblePresentation : byte
{
    Exposed,
    ChozoOrb,
    ShotBlock,
}

/// <summary>Debugger-visible phase of one translated permanent-item PLM.</summary>
public enum CollectiblePhase : byte
{
    CollectedEmpty,
    Visible,
    ChozoOrb,
    ChozoOrbBurst,
    ShotBlock,
    ShotBlockReveal,
    ShotBlockVisible,
    ShotBlockReconceal,
    CollectedShotBlock,
    CollectedShotBlockEmpty,
    CollectedShotBlockRespawn,
}

/// <summary>Stable debugger view of one occupied permanent-item PLM slot.</summary>
public readonly record struct CollectiblePlmSnapshot(
    ushort Header,
    int BlockIndex,
    ushort RoomArgument,
    InWorldCollectibleKind Kind,
    CollectiblePresentation Presentation,
    CollectiblePhase Phase,
    int GraphicsSlot);

/// <summary>One cartridge-defined permanent pickup completed by the current PLM pass.</summary>
public readonly record struct CollectiblePickupEvent(
    InWorldCollectibleKind Kind,
    CollectiblePresentation Presentation,
    ushort RoomArgument,
    byte MessageBoxIndex,
    int BlockIndex,
    ushort TriggerProjectileType);
