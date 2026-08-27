using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Frame-steppable room PLM owner translating movement-triggered breakable terrain and its
/// ROM-authored bank-$84 instruction lists.
/// </summary>
/// <remarks>
/// A PLM is not a Samus animation. It is an independent room object which keeps running
/// after the rope disconnects, owns collision/BTS mutation, and requests a BG1 redraw when
/// an instruction changes its level word. Keeping this state outside <c>SamusGrappleState</c>
/// is essential for the respawning block: Samus is long gone when its original word returns.
///
/// The retail allocation contains 40 word-indexed slots ($00 through $4E) and searches from
/// the highest slot downward. The C# array uses logical indices 0..39 but preserves that
/// search and handler order. The translated opcode surface is deliberately limited to the
/// collision-bomb, projectile-reaction bomb, and breakable-grapple lists. Encountering any
/// other pointer fails instead of silently inventing an effect for a still-untranslated PLM
/// family.
/// </remarks>
public sealed class RoomPlmSystem
{
    private const int SlotCount = 40;
    private const ushort RespawningInstructionList = 0xcd6a;
    private const ushort NonRespawningInstructionList = 0xcda9;
    private const ushort DeleteInstruction = 0x86bc;
    private const ushort DrawPlmBlockInstruction = 0x8b17;
    private const ushort QueueSoundLibrary2Maximum6Instruction = 0x8c10;
    private const ushort QueueSoundLibrary2Maximum3Instruction = 0x8c46;
    private const ushort GotoInstruction = 0x8724;
    private const ushort SetPlmBtsTo1Instruction = 0xcd93;
    private const ushort DeleteInstructionList = 0xaae3;

    // `$94:936B` selects these eight entry IDs from BTS 0..7. Their setup pointer is common,
    // so storing the post-setup instruction-list pointer is sufficient after we reproduce
    // `$84:CE83-$CED9` synchronously in TrySpawnCollisionBombBlock.
    private static readonly ushort[] CollisionBombInstructionLists =
    [
        0xcc35, // BTS 0: 1x1, respawning
        0xcc5f, // BTS 1: 2x1, respawning
        0xcc8b, // BTS 2: 1x2, respawning
        0xccb7, // BTS 3: 2x2, respawning
        0xcce3, // BTS 4: 1x1, permanent
        0xccff, // BTS 5: 2x1, permanent
        0xcd1b, // BTS 6: 1x2, permanent
        0xcd37, // BTS 7: 2x2, permanent
    ];

    // `$94:A012` selects these bank-$84 entry IDs for both type-$7 bombable air and type-$F
    // bombable blocks. All eight entries share setup `$84:CEDA`; these are the instruction
    // list pointers installed by Spawn_PLM before that setup examines the projectile type.
    private static readonly ushort[] ReactionBombInstructionLists =
    [
        0xcc3c, // BTS 0: 1x1, respawning
        0xcc66, // BTS 1: 2x1, respawning
        0xcc92, // BTS 2: 1x2, respawning
        0xccbe, // BTS 3: 2x2, respawning
        0xccea, // BTS 4: 1x1, permanent
        0xcd06, // BTS 5: 2x1, permanent
        0xcd22, // BTS 6: 1x2, permanent
        0xcd3e, // BTS 7: 2x2, permanent
    ];

    private readonly PlmSlot[] _slots = Enumerable
        .Range(0, SlotCount)
        .Select(_ => new PlmSlot())
        .ToArray();
    private readonly List<PlmSoundRequest> _soundRequests = new();
    private readonly List<PlmTilemapUpdate> _tilemapUpdates = new();

    /// <summary>Sound commands emitted during the most recent handler pass.</summary>
    public IReadOnlyList<PlmSoundRequest> SoundRequests => _soundRequests;

    /// <summary>Visible BG1 mutations emitted during the most recent handler pass.</summary>
    public IReadOnlyList<PlmTilemapUpdate> TilemapUpdates => _tilemapUpdates;

    /// <summary>Number of occupied native-equivalent PLM slots.</summary>
    public int ActiveCount => _slots.Count(slot => slot.Active);

    /// <summary>
    /// Runs setup <c>$84:CFB5</c> for BTS one or two and installs the corresponding PLM.
    /// </summary>
    /// <returns>False only when all 40 native slots are occupied.</returns>
    public bool TrySpawnBreakableGrappleBlock(
        RoomLevelData level,
        int blockIndex,
        byte behavior)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (behavior is not (1 or 2))
            throw new ArgumentOutOfRangeException(nameof(behavior), "Breakable grapple BTS must be one or two.");

        // Spawn_PLM at $84:84E7 probes $4E,$4C,...,$00. Matching it matters if multiple
        // PLMs mutate the same room on one frame because the handler uses the same order.
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.RestoreLevelWord = block.LevelWord;
            slot.InstructionPointer = behavior == 1
                ? RespawningInstructionList
                : NonRespawningInstructionList;
            slot.InstructionTimer = 1;

            // Setup_CFB5 saves the complete original level word but clears only the low BTS
            // byte. It deliberately leaves collision type E intact until the first PLM pass
            // later in this same gameplay frame draws $E0B7.
            level.SetBehavior(blockIndex, 0);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Spawns the bank-$84 collision PLM selected by type-$F BTS zero through seven.
    /// </summary>
    /// <remarks>
    /// The caller has already satisfied setup <c>$84:CE83</c>'s speed/screw pose gate. Setup
    /// saves <c>(levelWord &amp; $F000) | $0058</c> in <c>PLM_Vars</c>, then clears only the
    /// collision nibble in level data and returns carry clear so Samus continues moving.
    /// BTS 0..3 later redraw a linked 1x1/2x1/1x2/2x2 collision shape after the exact
    /// 384-frame hold; BTS 4..7 delete after their four-frame break animation.
    /// </remarks>
    /// <returns>
    /// True when a native slot was allocated. False preserves <c>Spawn_PLM</c>'s full-pool
    /// behavior: setup never ran, so the level word remains untouched even though bank $94
    /// inherited carry clear and lets the current movement scan continue.
    /// </returns>
    public bool TrySpawnCollisionBombBlock(
        RoomLevelData level,
        int blockIndex,
        byte behavior)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (behavior > 7)
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                "Collision bomb-block BTS must be in the native table range zero through seven.");
        }

        // `$84:84ED-$84F7` searches the same descending slot order used by every other
        // gameplay-spawned PLM. Do not coalesce neighboring pieces: a native collision scan
        // can allocate more than one independently timed object in a single movement call.
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            slot.Active = true;
            slot.BlockIndex = blockIndex;

            // This is not the original visual word. Setup_CE83 deliberately replaces all
            // low twelve bits with visual block `$058`; multi-block restoration lists then
            // add type-$5/$D extension words around this type-$F parent.
            slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0058));
            slot.InstructionPointer = CollisionBombInstructionLists[behavior];
            slot.InstructionTimer = 1;
            level.ClearCollisionType(blockIndex);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Spawns the bank-$84 shot/bombed/grappled-reaction PLM selected by bombable BTS.
    /// </summary>
    /// <remarks>
    /// This is setup <c>$84:CEDA</c>, not the collision setup above. A normal bomb family
    /// (<c>$0500</c>) advances the entry instruction pointer by three bytes, deliberately
    /// skipping its leading sound-$0A opcode because the bomb explosion already owns its
    /// sound. A power bomb (<c>$0300</c>) retains that opcode. Both accepted projectile
    /// families synthesize <c>(levelWord &amp; $F000) | $0058</c> for later restoration and
    /// then apply <c>AND $8FFF</c> to live terrain. Thus a type-$F solid bomb block remains
    /// temporarily type-$8 solid until the same frame's PLM pass draws its first air frame,
    /// while a type-$7 bombable-air parent becomes ordinary air immediately.
    ///
    /// BTS 8..15 point at <c>PLMEntries_nothing</c>. Native code still allocates a slot and
    /// deletes it on the next handler pass, so this implementation retains that otherwise
    /// invisible resource/timing effect. A negative BTS is filtered by bank $94 before this
    /// method is called because it denotes an area-dependent/duplicate path.
    /// </remarks>
    /// <returns>False only when all 40 native slots are occupied.</returns>
    public bool TrySpawnBombReactionBlock(
        RoomLevelData level,
        int blockIndex,
        byte behavior,
        ushort projectileType)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (behavior > 15)
        {
            throw new ArgumentOutOfRangeException(
                nameof(behavior),
                "Bomb-reaction BTS must be in the native table range zero through fifteen.");
        }

        ushort projectileFamily = unchecked((ushort)(projectileType & 0x0f00));
        if (projectileFamily is not (0x0500 or 0x0300))
        {
            throw new ArgumentOutOfRangeException(
                nameof(projectileType),
                "Bomb-reaction setup accepts only normal-bomb or power-bomb projectile families.");
        }

        // `$84:84ED-$84F7` searches from native slot `$4E` toward `$00`. Importantly, the
        // full-pool path never calls setup and therefore must not mutate live level data.
        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (slot.Active)
                continue;

            RoomCollisionBlock block = level.GetCollisionBlockByIndex(blockIndex);
            slot.Active = true;
            slot.BlockIndex = blockIndex;
            slot.InstructionTimer = 1;

            if (behavior >= 8)
            {
                // Table entries 8..15 are all `$84:B62F`, whose setup is a bare RTS and
                // whose instruction list is the one-word delete stream at `$84:AAE3`.
                slot.RestoreLevelWord = 0;
                slot.InstructionPointer = DeleteInstructionList;
                return true;
            }

            // Setup_CEDA discards the original low twelve bits rather than preserving the
            // visible tile number. Dimension-specific final draw lists reconstruct linked
            // extension words; the 1x1 respawn tail uses this exact PLM_Vars value.
            slot.RestoreLevelWord = unchecked((ushort)((block.LevelWord & 0xf000) | 0x0058));
            ushort instructionPointer = ReactionBombInstructionLists[behavior];

            // `$84:CF0C-$CF13` adds three only for normal bombs. The skipped bytes are
            // `{Instruction_PLM_QueueSound_Y_Lib2_Max3, $0A}` in the odd-byte operand form.
            slot.InstructionPointer = projectileFamily == 0x0500
                ? unchecked((ushort)(instructionPointer + 3))
                : instructionPointer;

            ushort temporaryLevelWord = unchecked((ushort)(slot.RestoreLevelWord & 0x8fff));
            level.SetForegroundEntry(blockIndex, temporaryLevelWord);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Executes <c>PLM_Handler</c>'s timer/instruction portion for all translated slots.
    /// </summary>
    /// <remarks>
    /// The caller supplies current layer-1 coordinates because native DrawPLM clips before
    /// queuing VRAM work. Level-data writes always occur; only the PPU-ring update is clipped.
    /// Returned updates are also exposed through <see cref="TilemapUpdates"/> so a debugger
    /// can inspect the exact block and destination before the runtime executes them.
    /// </remarks>
    public IReadOnlyList<PlmTilemapUpdate> Step(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(streamer);
        _soundRequests.Clear();
        _tilemapUpdates.Clear();

        for (int slotIndex = _slots.Length - 1; slotIndex >= 0; slotIndex--)
        {
            PlmSlot slot = _slots[slotIndex];
            if (!slot.Active)
                continue;

            slot.InstructionTimer = unchecked((ushort)(slot.InstructionTimer - 1));
            if (slot.InstructionTimer != 0)
                continue;

            ExecuteInstructionStream(
                bus,
                level,
                streamer,
                slot,
                layer1XPosition,
                layer1YPosition,
                bg1XOffset);
        }

        return _tilemapUpdates;
    }

    private void ExecuteInstructionStream(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        PlmSlot slot,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        // An instruction list may execute multiple negative instruction words before it
        // reaches a positive timer/draw pair. The guard catches corrupt ROM/test data while
        // remaining far above the longest straight-line chain in these two retail lists.
        for (int dispatchCount = 0; dispatchCount < 16; dispatchCount++)
        {
            ushort instruction = ReadBank84Word(bus, slot.InstructionPointer);
            if ((instruction & 0x8000) == 0)
            {
                ushort drawPointer = ReadBank84Word(
                    bus,
                    unchecked((ushort)(slot.InstructionPointer + 2)));
                slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 4));
                slot.InstructionTimer = instruction;
                DrawRomInstruction(
                    bus,
                    level,
                    streamer,
                    slot.BlockIndex,
                    drawPointer,
                    layer1XPosition,
                    layer1YPosition,
                    bg1XOffset);
                return;
            }

            switch (instruction)
            {
                case QueueSoundLibrary2Maximum6Instruction:
                    // $84:8C10 consumes one byte after its pointer. The following timer's
                    // low byte is read as A's harmless high byte by the 16-bit LDA.
                    byte soundId = bus.ReadByte(
                        0x840000 | unchecked((ushort)(slot.InstructionPointer + 2)));
                    _soundRequests.Add(new PlmSoundRequest(Library: 2, soundId, MaximumQueued: 6));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case QueueSoundLibrary2Maximum3Instruction:
                    // `$84:8C46` has the same odd-byte operand layout as `$8C10`, but the
                    // collision-bomb list's crumble sound `$06` uses the stricter queue cap.
                    byte cappedSoundId = bus.ReadByte(
                        0x840000 | unchecked((ushort)(slot.InstructionPointer + 2)));
                    _soundRequests.Add(new PlmSoundRequest(2, cappedSoundId, MaximumQueued: 3));
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 3));
                    continue;

                case GotoInstruction:
                    // `$84:8724` replaces Y with the following little-endian pointer. All
                    // eight collision entry lists use it to share their dimension-specific
                    // respawning/permanent animation tail.
                    slot.InstructionPointer = ReadBank84Word(
                        bus,
                        unchecked((ushort)(slot.InstructionPointer + 2)));
                    continue;

                case SetPlmBtsTo1Instruction:
                    level.SetBehavior(slot.BlockIndex, 1);
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    continue;

                case DrawPlmBlockInstruction:
                    // $84:8B17 restores PLM_Vars to level data, builds a one-block custom
                    // draw list, sets timer one, and exits the handler. Deletion therefore
                    // occurs on the next PLM pass rather than this restoration pass.
                    slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
                    slot.InstructionTimer = 1;
                    DrawLevelWord(
                        level,
                        streamer,
                        slot.BlockIndex,
                        slot.RestoreLevelWord,
                        layer1XPosition,
                        layer1YPosition,
                        bg1XOffset);
                    return;

                case DeleteInstruction:
                    slot.Active = false;
                    return;

                default:
                    throw new InvalidDataException(
                        $"Movement-owned PLM reached unsupported bank-$84 instruction ${instruction:X4}.");
            }
        }

        throw new InvalidDataException("Movement-owned PLM instruction chain did not reach a timer.");
    }

    private void DrawRomInstruction(
        ISnesAddressSpace bus,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        int blockIndex,
        ushort drawPointer,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        // `$84:861E-$86B3` treats each record as a direction/count word followed by complete
        // level words. Bit 15 means a vertical column; a clear bit means a horizontal row.
        // After its words, a signed-byte X/Y pair locates another record relative to the PLM
        // origin. A zero pair terminates. The 2x2 bomb art is consequently two horizontal
        // records: row zero, then `{dx=0,dy=1}` and row one.
        int originX = blockIndex % level.WidthInBlocks;
        int originY = blockIndex / level.WidthInBlocks;
        int entryX = originX;
        int entryY = originY;
        ushort cursor = drawPointer;

        // Retail movement-owned draw lists have at most two records and two words apiece.
        // A generous structural guard makes malformed fixtures/ROM deterministic instead of
        // allowing a missing terminator to wander through all of bank $84.
        for (int entryNumber = 0; entryNumber < 32; entryNumber++)
        {
            ushort directionAndCount = ReadBank84Word(bus, cursor);
            bool vertical = (directionAndCount & 0x8000) != 0;
            int count = directionAndCount & 0x7fff;
            if (count is <= 0 or > 0xff)
            {
                throw new InvalidDataException(
                    $"PLM draw list ${drawPointer:X4} has invalid block count {count}.");
            }
            cursor = unchecked((ushort)(cursor + 2));

            for (int blockOffset = 0; blockOffset < count; blockOffset++)
            {
                int x = entryX + (vertical ? 0 : blockOffset);
                int y = entryY + (vertical ? blockOffset : 0);
                if ((uint)x >= (uint)level.WidthInBlocks ||
                    (uint)y >= (uint)level.HeightInBlocks)
                {
                    throw new InvalidDataException(
                        $"PLM draw list ${drawPointer:X4} targets out-of-room block ({x},{y}).");
                }

                ushort levelWord = ReadBank84Word(bus, cursor);
                cursor = unchecked((ushort)(cursor + 2));
                DrawLevelWord(
                    level,
                    streamer,
                    level.GetBlockIndex(x, y),
                    levelWord,
                    layer1XPosition,
                    layer1YPosition,
                    bg1XOffset);
            }

            byte relativeX = bus.ReadByte(0x840000 | cursor);
            byte relativeY = bus.ReadByte(0x840000 | unchecked((ushort)(cursor + 1)));
            if (relativeX == 0 && relativeY == 0)
                return;

            entryX = originX + unchecked((sbyte)relativeX);
            entryY = originY + unchecked((sbyte)relativeY);
            cursor = unchecked((ushort)(cursor + 2));
        }

        throw new InvalidDataException(
            $"PLM draw list ${drawPointer:X4} did not reach its signed-offset terminator.");
    }

    private void DrawLevelWord(
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        int blockIndex,
        ushort levelWord,
        ushort layer1XPosition,
        ushort layer1YPosition,
        ushort bg1XOffset)
    {
        level.SetForegroundEntry(blockIndex, levelWord);
        streamer.SetLevelEntry(blockIndex, levelWord);

        int blockX = blockIndex % level.WidthInBlocks;
        int blockY = blockIndex / level.WidthInBlocks;
        if (!IsInsideNativeDrawWindow(blockX, blockY, layer1XPosition, layer1YPosition))
            return;

        _tilemapUpdates.Add(streamer.BuildPlmLevelBlockUpdate(blockIndex, bg1XOffset));
    }

    private static bool IsInsideNativeDrawWindow(
        int blockX,
        int blockY,
        ushort layer1XPosition,
        ushort layer1YPosition)
    {
        int topBlock = layer1YPosition >> 4;
        if (blockY < topBlock || blockY > topBlock + 15)
            return false;

        // $84:8DF4 calculates floor((layer1X+15)/16)-1, then admits the following
        // seventeen blocks. In ordinary non-wrapped room coordinates this is the partially
        // visible left block plus the sixteen blocks that can intersect the 256-pixel view.
        int leftBlock = ((layer1XPosition + 15) >> 4) - 1;
        return blockX >= leftBlock && blockX < leftBlock + 17;
    }

    private static ushort ReadBank84Word(ISnesAddressSpace bus, ushort address) =>
        unchecked((ushort)(
            bus.ReadByte(0x840000 | address) |
            (bus.ReadByte(0x840000 | unchecked((ushort)(address + 1))) << 8)));

    private sealed class PlmSlot
    {
        public bool Active { get; set; }
        public int BlockIndex { get; set; }
        /// <summary>
        /// Native <c>PLM_Vars</c>. Grapple setup saves the original word; collision-bomb
        /// setup synthesizes the dimension-parent restoration word <c>$x058</c> instead.
        /// </summary>
        public ushort RestoreLevelWord { get; set; }
        public ushort InstructionPointer { get; set; }
        public ushort InstructionTimer { get; set; }
    }
}

/// <summary>Observable call to one of the cartridge's three queued-sound libraries.</summary>
public readonly record struct PlmSoundRequest(byte Library, byte SoundId, byte MaximumQueued);
