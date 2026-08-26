using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Frame-steppable room PLM owner, currently translating the two breakable-grapple entries
/// at <c>$84:D0DC/$D0E0</c> and their ROM-authored instruction lists.
/// </summary>
/// <remarks>
/// A PLM is not a Samus animation. It is an independent room object which keeps running
/// after the rope disconnects, owns collision/BTS mutation, and requests a BG1 redraw when
/// an instruction changes its level word. Keeping this state outside <c>SamusGrappleState</c>
/// is essential for the respawning block: Samus is long gone when its original word returns.
///
/// The retail allocation contains 40 word-indexed slots ($00 through $4E) and searches from
/// the highest slot downward. The C# array uses logical indices 0..39 but preserves that
/// search and handler order. Only the instruction opcodes reachable from these two exact
/// entries are admitted; encountering another pointer fails instead of guessing its effect.
/// </remarks>
public sealed class RoomPlmSystem
{
    private const int SlotCount = 40;
    private const ushort RespawningInstructionList = 0xcd6a;
    private const ushort NonRespawningInstructionList = 0xcda9;
    private const ushort DeleteInstruction = 0x86bc;
    private const ushort DrawPlmBlockInstruction = 0x8b17;
    private const ushort QueueSoundLibrary2Maximum6Instruction = 0x8c10;
    private const ushort SetPlmBtsTo1Instruction = 0xcd93;

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
            slot.OriginalLevelWord = block.LevelWord;
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
                        slot.OriginalLevelWord,
                        layer1XPosition,
                        layer1YPosition,
                        bg1XOffset);
                    return;

                case DeleteInstruction:
                    slot.Active = false;
                    return;

                default:
                    throw new InvalidDataException(
                        $"Breakable grapple PLM reached unsupported bank-$84 instruction ${instruction:X4}.");
            }
        }

        throw new InvalidDataException("Breakable grapple PLM instruction chain did not reach a timer.");
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
        // Each $A4F9-$A511 list contains exactly one horizontal block and a zero terminator.
        // Validate both structural words from the cartridge before consuming the level word;
        // this keeps an incorrect pointer from silently becoming plausible terrain.
        ushort blockCount = ReadBank84Word(bus, drawPointer);
        ushort levelWord = ReadBank84Word(bus, unchecked((ushort)(drawPointer + 2)));
        ushort terminator = ReadBank84Word(bus, unchecked((ushort)(drawPointer + 4)));
        if (blockCount != 1 || terminator != 0)
        {
            throw new InvalidDataException(
                $"Breakable grapple draw list ${drawPointer:X4} is not one horizontal block.");
        }

        DrawLevelWord(
            level,
            streamer,
            blockIndex,
            levelWord,
            layer1XPosition,
            layer1YPosition,
            bg1XOffset);
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
        public ushort OriginalLevelWord { get; set; }
        public ushort InstructionPointer { get; set; }
        public ushort InstructionTimer { get; set; }
    }
}

/// <summary>Observable call to one of the cartridge's three queued-sound libraries.</summary>
public readonly record struct PlmSoundRequest(byte Library, byte SoundId, byte MaximumQueued);
