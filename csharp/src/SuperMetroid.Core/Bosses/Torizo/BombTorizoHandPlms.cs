using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The Bomb Torizo statue's hand PLM: its setup, per-frame state and the instructions that wake the statue.</summary>
[RoomLifetime]
internal sealed class BombTorizoHandPlms
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>Physical PLM slots searched for active native header pointers.</summary>
    private readonly PlmSlotTable _slotTable;

    /// <summary>Connects the statue-hand interpreter to the current room and PLM slot table.</summary>
    internal BombTorizoHandPlms(IRoomSamusAccess? samus, PlmSlotTable slotTable)
    {
        _samus = samus;
        _slotTable = slotTable;
    }
    /// <summary>VRAM transfers queued by the hand's copy-from-RAM instruction for this frame.</summary>
    private readonly List<PlmVramWriteRequest> _vramWriteRequests = [];
    /// <summary>Statue-fragment spawns queued by the hand's breaking instruction for this frame.</summary>
    private readonly List<BombTorizoStatueProjectileRequest>
        _bombTorizoStatueProjectileRequests = [];
    /// <summary>Delayed track-selection commands queued by the hand program for this frame.</summary>
    private readonly List<DelayedMusicRequest> _musicRequests = [];

    /// <summary>Raw seven-byte VRAM queue entries emitted during the latest PLM pass.</summary>
    public IReadOnlyList<PlmVramWriteRequest> VramWriteRequests => _vramWriteRequests;

    /// <summary>Bank-$86 statue fragments emitted during the latest PLM pass.</summary>
    public IReadOnlyList<BombTorizoStatueProjectileRequest>
        BombTorizoStatueProjectileRequests => _bombTorizoStatueProjectileRequests;

    /// <summary>Delayed music commands emitted during the latest PLM pass.</summary>
    public IReadOnlyList<DelayedMusicRequest> MusicRequests => _musicRequests;

    /// <summary>
    /// Tests the physical PLM header array exactly as enemy code such as
    /// <c>$AA:C6C6</c> does. This is intentionally generic: the enemy does not know which
    /// translated family owns a slot, only whether one of forty header words matches.
    /// </summary>
    public bool HasActiveHeader(PlmHeaderId header) =>
        _slotTable.Slots.Any(slot => slot.Active && slot.HeaderPointer == header);

    /// <summary>Runs setup $D606 after the generic room allocator has installed the header.</summary>
    internal void SetupBombTorizoHandSlot(
        RoomLevelData level,
        PlmSlot slot,
        IAreaBossFlagReader bosses)
    {
        if (bosses.IsDefeated(BossBits.AreaTorizo))
        {
            // Setup deletes synchronously, so the next population record may reuse this ID.
            slot.Clear();
            return;
        }

        slot.StartAnimation(new BombTorizoHandPlmAnimation());
        _bombTorizoHandRoomWidth = level.Size.X;
    }

    /// <summary>Discards the previous PLM pass's VRAM, fragment, and music requests before the next pass.</summary>
    internal void BeginBombTorizoHandFrame()
    {
        _vramWriteRequests.Clear();
        _bombTorizoStatueProjectileRequests.Clear();
        _musicRequests.Clear();
    }

    /// <summary>
    /// Runs pre-instruction <c>$84:D33B</c> before the ordinary PLM timer pass.
    /// </summary>
    internal void RunBombTorizoHandPreInstruction(PlmSlot slot)
    {
        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException(
                "A live Bomb Torizo hand PLM has no active Samus inventory owner.");
        if (!samus.CollectedItems.HasAny(SamusEquipmentFlags.Bombs))
            return;

        // The sleeping opcode leaves Y pointing at itself. D33B adds two to skip that word,
        // restores the instruction timer to one, and replaces its pre-instruction with RTS.
        slot.WakePastSleep();
        slot.PreInstruction = PlmPreInstructions.Inert;
    }
    /// <summary><c>$84:87E5</c>: copies the Chozo debris tiles to VRAM before the hand crumbles.</summary>
    internal void CopyDebrisTiles() =>
        _vramWriteRequests.Add(new PlmVramWriteRequest(
            checked(TorizoInstructionTileRomData.ChozoDebrisByteCount),
            TorizoInstructionTileRomData.ChozoDebrisSource,
            GameplayVram.ObjectCharactersDma(BombTorizoHandPlmProgramDefinitions.DebrisObjectCharacterWordOffset)));

    /// <summary><c>$84:D357</c>: spawns statue fragment <paramref name="fragment"/> at the hand's block.</summary>
    internal void SpawnFragment(PlmSlot slot, int fragment) =>
        _bombTorizoStatueProjectileRequests.Add(new BombTorizoStatueProjectileRequest(
            fragment,
            // The native fragment spawn carries both block coordinates as bytes.
            new(checked((byte)(slot.BlockIndex % _bombTorizoHandRoomWidth)),
                checked((byte)(slot.BlockIndex / _bombTorizoHandRoomWidth)))));

    /// <summary>
    /// <c>$84:D3C7</c>: despite its name, always queues gameplay track six with the shared eight-frame delay.
    /// </summary>
    internal void QueueMusic() =>
        _musicRequests.Add(new DelayedMusicRequest(MusicCommand.SelectTrack(6), MusicCommandDelay.EightFrames));

    /// <summary>Room width in blocks used to turn the hand's linear PLM block index into fragment spawn coordinates.</summary>
    private int _bombTorizoHandRoomWidth;

    /// <summary>Removes the hand's header and wake pre-instruction once its PLM has been deleted.</summary>
    internal static void MarkBombTorizoHandDeleted(PlmSlot slot)
    {
        if (slot.HeaderPointer != PlmHeaderId.BombTorizoHand)
            return;
        slot.HeaderPointer = 0;
        slot.PreInstruction = PlmPreInstructions.Inert;
    }
}
