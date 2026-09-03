using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Bomb Torizo's room-authored Chozo-hand trigger PLM, header <c>$84:D6EA</c>.
/// </summary>
/// <remarks>
/// This innocuous-looking room object is the encounter's real synchronization primitive.
/// Torizo's bank-$AA main AI scans the shared forty-slot PLM header array and remains a
/// tangible statue while this header exists. The item PLM grants Bombs independently;
/// pre-instruction <c>$84:D33B</c> notices that inventory bit, advances past a sleeping
/// instruction, and lets the cartridge's <c>$84:D368</c> list break the hand apart before
/// deleting itself. Translating the resident PLM keeps the item, terrain art, debris, and
/// enemy wake-up coupled by the same observable header that the original game uses.
/// </remarks>
public sealed partial class RoomPlmSystem
{
    private const ushort WakeIfSamusHasBombsPreInstruction = 0xd33b;
    private const ushort BombTorizoStatueBreakingDefinition = 0xa993;

    private readonly List<PlmVramWriteRequest> _vramWriteRequests = [];
    private readonly List<BombTorizoStatueProjectileRequest>
        _bombTorizoStatueProjectileRequests = [];
    private readonly List<PlmMusicRequest> _musicRequests = [];
    private Func<SamusState?>? _bombTorizoSamus;
    private bool _bombTorizoHandWasLoaded;
    private bool _bombTorizoHandWasDeleted;

    /// <summary>Raw seven-byte VRAM queue entries emitted during the latest PLM pass.</summary>
    public IReadOnlyList<PlmVramWriteRequest> VramWriteRequests => _vramWriteRequests;

    /// <summary>Bank-$86 statue fragments emitted during the latest PLM pass.</summary>
    public IReadOnlyList<BombTorizoStatueProjectileRequest>
        BombTorizoStatueProjectileRequests => _bombTorizoStatueProjectileRequests;

    /// <summary>Delayed music commands emitted during the latest PLM pass.</summary>
    public IReadOnlyList<PlmMusicRequest> MusicRequests => _musicRequests;

    /// <summary>Whether this room population contained a live, undefeated hand trigger.</summary>
    public bool BombTorizoHandWasLoaded => _bombTorizoHandWasLoaded;

    /// <summary>Whether instruction <c>$84:86BC</c> consumed the loaded hand trigger.</summary>
    public bool BombTorizoHandWasDeleted => _bombTorizoHandWasDeleted;

    /// <summary>
    /// Tests the physical PLM header array exactly as enemy code such as
    /// <c>$AA:C6C6</c> does. This is intentionally generic: the enemy does not know which
    /// translated family owns a slot, only whether one of forty header words matches.
    /// </summary>
    public bool HasActiveHeader(ushort header) =>
        _slots.Any(slot => slot.Active && slot.HeaderPointer == header);

    /// <summary>Runs setup $D606 after the generic room allocator has installed the header.</summary>
    private void SetupBombTorizoHandSlot(
        RoomLevelData level,
        PlmSlot slot,
        Func<bool> isAreaTorizoDefeated)
    {
        if (isAreaTorizoDefeated())
        {
            // Setup deletes synchronously, so the next population record may reuse this ID.
            ClearSlot(slot);
            return;
        }

        slot.InstructionPointer = RoomPlmInstructionLists.BombTorizoCrumblingChozo;
        _bombTorizoHandRoomWidth = level.WidthInBlocks;
        _bombTorizoHandWasLoaded = true;
        _bombTorizoHandWasDeleted = false;
    }

    private void BeginBombTorizoHandFrame()
    {
        _vramWriteRequests.Clear();
        _bombTorizoStatueProjectileRequests.Clear();
        _musicRequests.Clear();
    }

    private void ResetBombTorizoHandState()
    {
        BeginBombTorizoHandFrame();
        _bombTorizoSamus = null;
        _bombTorizoHandRoomWidth = 0;
        _bombTorizoHandWasLoaded = false;
        _bombTorizoHandWasDeleted = false;
    }

    /// <summary>
    /// Runs pre-instruction <c>$84:D33B</c> before the ordinary PLM timer pass.
    /// </summary>
    private void RunBombTorizoHandPreInstruction(PlmSlot slot)
    {
        if (slot.HeaderPointer != RoomPlmHeaders.BombTorizoHand || slot.PreInstruction == 0)
            return;
        if (slot.PreInstruction != WakeIfSamusHasBombsPreInstruction)
        {
            throw new InvalidDataException(
                $"Bomb Torizo hand has invalid pre-instruction $84:{slot.PreInstruction:X4}.");
        }

        SamusState samus = _bombTorizoSamus?.Invoke()
            ?? throw new InvalidOperationException(
                "A live Bomb Torizo hand PLM has no active Samus inventory owner.");
        if (!samus.CollectedItems.HasAny(SamusEquipmentFlags.Bombs))
            return;

        // The sleeping opcode leaves Y pointing at itself. D33B adds two to skip that word,
        // restores the instruction timer to one, and replaces its pre-instruction with RTS.
        slot.InstructionTimer = 1;
        slot.InstructionPointer = unchecked((ushort)(slot.InstructionPointer + 2));
        slot.PreInstruction = 0;
    }

    private bool TryExecuteBombTorizoHandInstruction(
        ISnesAddressSpace bus,
        PlmSlot slot,
        ushort instruction)
    {
        if (slot.HeaderPointer != RoomPlmHeaders.BombTorizoHand)
            return false;

        ushort cursor = slot.InstructionPointer;
        switch (instruction)
        {
            case RoomPlmInstructionCodes.CopyFromRamToVram:
            {
                // $87E5 consumes seven deliberately unaligned bytes after its opcode:
                // u16 size, u16 source offset, u8 bank, then u16 encoded VRAM destination.
                ushort size = ReadBank84Word(bus, unchecked((ushort)(cursor + 2)));
                ushort sourceOffset = ReadBank84Word(bus, unchecked((ushort)(cursor + 4)));
                byte sourceBank = bus.ReadByte(Bank84(unchecked((ushort)(cursor + 6))));
                ushort destination = ReadBank84Word(bus, unchecked((ushort)(cursor + 7)));
                _vramWriteRequests.Add(new PlmVramWriteRequest(
                    size,
                    (sourceBank << 16) | sourceOffset,
                    destination));
                slot.InstructionPointer = unchecked((ushort)(cursor + 9));
                return true;
            }

            case RoomPlmInstructionCodes.SpawnTorizoStatueBreaking:
            {
                ushort parameter = ReadBank84Word(bus, unchecked((ushort)(cursor + 2)));
                _bombTorizoStatueProjectileRequests.Add(
                    new BombTorizoStatueProjectileRequest(
                        BombTorizoStatueBreakingDefinition,
                        parameter,
                        checked((byte)(slot.BlockIndex % _bombTorizoHandRoomWidth)),
                        checked((byte)(slot.BlockIndex / _bombTorizoHandRoomWidth))));
                slot.InstructionPointer = unchecked((ushort)(cursor + 4));
                return true;
            }

            case RoomPlmInstructionCodes.QueueSongOneMusicTrack:
                // Despite its name, this routine always queues literal gameplay track six
                // with the shared eight-frame delay and consumes no inline operand.
                _musicRequests.Add(new PlmMusicRequest(
                    MusicCommand.SelectTrack(6),
                    MusicCommandDelay.EightFrames));
                slot.InstructionPointer = unchecked((ushort)(cursor + 2));
                return true;

            default:
                return false;
        }
    }

    private int _bombTorizoHandRoomWidth;

    private void MarkBombTorizoHandDeleted(PlmSlot slot)
    {
        if (slot.HeaderPointer != RoomPlmHeaders.BombTorizoHand)
            return;
        _bombTorizoHandWasDeleted = true;
        slot.HeaderPointer = 0;
        slot.PreInstruction = 0;
    }
}

/// <summary>One packed VRAM write-table record emitted by bank-$84 PLM bytecode.</summary>
public readonly record struct PlmVramWriteRequest(
    ushort SizeInBytes,
    int SourceAddress,
    ushort EncodedVramDestination);

/// <summary>One <c>SpawnEprojWithRoomGfx($A993, parameter)</c> hand fragment.</summary>
public readonly record struct BombTorizoStatueProjectileRequest(
    ushort DefinitionPointer,
    ushort Parameter,
    byte PlmBlockX,
    byte PlmBlockY);

/// <summary>One delayed music request made by a room PLM instruction.</summary>
public readonly record struct PlmMusicRequest(
    MusicCommand Command,
    MusicCommandDelay Delay);
