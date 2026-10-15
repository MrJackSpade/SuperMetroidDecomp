using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The Bomb Torizo statue's hand PLM: its setup, per-frame state and the instructions that wake the statue.</summary>
internal sealed class BombTorizoHandPlms
{
    /// <summary>Live room bindings used to inspect Samus's collected equipment during the hand's wake check.</summary>
    private readonly PlmRoomBindings _bindings;
    /// <summary>Physical PLM slots searched for active native header pointers.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Connects the statue-hand interpreter to the current room and PLM slot table.</summary>
    internal BombTorizoHandPlms(PlmRoomBindings bindings, PlmSlotTable slotTable)
    {
        _bindings = bindings;
        _slotTable = slotTable;
    }
    /// <summary>Bank-$86 projectile definition $A993 for the statue fragments spawned as Bomb Torizo wakes.</summary>
    private const ushort BombTorizoStatueBreakingDefinition = 0xa993;

    /// <summary>VRAM transfers queued by the hand's copy-from-RAM instruction for this frame.</summary>
    private readonly List<PlmVramWriteRequest> _vramWriteRequests = [];
    /// <summary>Statue-fragment spawns queued by the hand's breaking instruction for this frame.</summary>
    private readonly List<BombTorizoStatueProjectileRequest>
        _bombTorizoStatueProjectileRequests = [];
    /// <summary>Delayed track-selection commands queued by the hand program for this frame.</summary>
    private readonly List<PlmMusicRequest> _musicRequests = [];

    /// <summary>Raw seven-byte VRAM queue entries emitted during the latest PLM pass.</summary>
    public IReadOnlyList<PlmVramWriteRequest> VramWriteRequests => _vramWriteRequests;

    /// <summary>Bank-$86 statue fragments emitted during the latest PLM pass.</summary>
    public IReadOnlyList<BombTorizoStatueProjectileRequest>
        BombTorizoStatueProjectileRequests => _bombTorizoStatueProjectileRequests;

    /// <summary>Delayed music commands emitted during the latest PLM pass.</summary>
    public IReadOnlyList<PlmMusicRequest> MusicRequests => _musicRequests;

    /// <summary>
    /// Tests the physical PLM header array exactly as enemy code such as
    /// <c>$AA:C6C6</c> does. This is intentionally generic: the enemy does not know which
    /// translated family owns a slot, only whether one of forty header words matches.
    /// </summary>
    public bool HasActiveHeader(PlmHeaderId header) =>
        _slotTable._slots.Any(slot => slot.Active && slot.HeaderPointer == header);

    /// <summary>Runs setup $D606 after the generic room allocator has installed the header.</summary>
    internal void SetupBombTorizoHandSlot(
        RoomLevelData level,
        PlmSlot slot,
        IAreaBossFlagReader bosses)
    {
        if (bosses.IsDefeated(BossBits.AreaTorizo))
        {
            // Setup deletes synchronously, so the next population record may reuse this ID.
            PlmPopulationLoader.ClearSlot(slot);
            return;
        }

        slot.InstructionPointer = RoomPlmInstructionLists.BombTorizoCrumblingChozo;
        _bombTorizoHandRoomWidth = level.Size.X;
    }

    /// <summary>Discards the previous PLM pass's VRAM, fragment, and music requests before the next pass.</summary>
    internal void BeginBombTorizoHandFrame()
    {
        _vramWriteRequests.Clear();
        _bombTorizoStatueProjectileRequests.Clear();
        _musicRequests.Clear();
    }

    /// <summary>Clears pending output and forgets the room width captured when the hand was installed.</summary>
    internal void ResetBombTorizoHandState()
    {
        BeginBombTorizoHandFrame();
        _bombTorizoHandRoomWidth = 0;
    }

    /// <summary>
    /// Runs pre-instruction <c>$84:D33B</c> before the ordinary PLM timer pass.
    /// </summary>
    internal void RunBombTorizoHandPreInstruction(PlmSlot slot)
    {
        if (slot.HeaderPointer != PlmHeaderId.BombTorizoHand || slot.PreInstruction == 0)
            return;
        if (slot.PreInstruction != BombTorizoHandPlmProgramDefinitions.WakeIfSamusHasBombsPreInstruction)
        {
            throw new InvalidDataException(
                $"Bomb Torizo hand has invalid pre-instruction $84:{slot.PreInstruction:X4}.");
        }

        SamusState samus = _bindings.Samus?.Current
            ?? throw new InvalidOperationException(
                "A live Bomb Torizo hand PLM has no active Samus inventory owner.");
        if (!samus.CollectedItems.HasAny(SamusEquipmentFlags.Bombs))
            return;

        // The sleeping opcode leaves Y pointing at itself. D33B adds two to skip that word,
        // restores the instruction timer to one, and replaces its pre-instruction with RTS.
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
        slot.PreInstruction = 0;
    }

    /// <summary>Handles the hand's VRAM copy, statue-fragment spawn, and fixed track-six opcodes, advancing the program cursor only when handled.</summary>
    internal bool TryExecuteBombTorizoHandInstruction(
        ISnesAddressSpace bus,
        PlmSlot slot,
        RoomPlmInstruction instruction)
    {
        if (slot.HeaderPointer != PlmHeaderId.BombTorizoHand)
            return false;

        ushort cursor = slot.InstructionPointer;
        if (instruction is not (
            RoomPlmInstruction.CopyFromRamToVram or
            RoomPlmInstruction.SpawnTorizoStatueBreaking or
            RoomPlmInstruction.QueueSongOneMusicTrack))
            return false;

        switch (instruction)
        {
            case RoomPlmInstruction.CopyFromRamToVram:
            {
                // $87E5 consumes seven deliberately unaligned bytes after its opcode:
                // u16 size, u16 source offset, u8 bank, then u16 encoded VRAM destination.
                ushort size = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                ushort sourceOffset = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 4)));
                byte sourceBank = RoomPlmSystem.ReadProgramByte(bus, unchecked((ushort)(cursor + 6)));
                ushort destination = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 7)));
                _vramWriteRequests.Add(new PlmVramWriteRequest(
                    size,
                    ((int)new SnesAddress(sourceBank, sourceOffset)),
                    destination));
                slot.InstructionPointer = unchecked((ushort)(cursor + 9));
                return true;
            }

            case RoomPlmInstruction.SpawnTorizoStatueBreaking:
            {
                ushort parameter = RoomPlmSystem.ReadProgramWord(bus, unchecked((ushort)(cursor + 2)));
                _bombTorizoStatueProjectileRequests.Add(
                    new BombTorizoStatueProjectileRequest(
                        BombTorizoStatueBreakingDefinition,
                        parameter,
                        // The native fragment spawn carries both block coordinates as bytes.
                        new(checked((byte)(slot.BlockIndex % _bombTorizoHandRoomWidth)),
                            checked((byte)(slot.BlockIndex / _bombTorizoHandRoomWidth)))));
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;
            }

            case RoomPlmInstruction.QueueSongOneMusicTrack:
                // Despite its name, this routine always queues literal gameplay track six
                // with the shared eight-frame delay and consumes no inline operand.
                _musicRequests.Add(new PlmMusicRequest(
                    MusicCommand.SelectTrack(6),
                    MusicCommandDelay.EightFrames));
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            default:
                throw new InvalidOperationException($"Unhandled RoomPlmInstruction {instruction}.");
        }
    }

    /// <summary>Room width in blocks used to turn the hand's linear PLM block index into fragment spawn coordinates.</summary>
    private int _bombTorizoHandRoomWidth;

    /// <summary>Removes the hand's header and wake pre-instruction once its PLM has been deleted.</summary>
    internal static void MarkBombTorizoHandDeleted(PlmSlot slot)
    {
        if (slot.HeaderPointer != PlmHeaderId.BombTorizoHand)
            return;
        slot.HeaderPointer = 0;
        slot.PreInstruction = 0;
    }
}
