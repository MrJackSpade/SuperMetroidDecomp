using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The Bomb Torizo statue's hand PLM: its setup, per-frame state and the instructions that wake the statue.</summary>
[RoomLifetime]
internal sealed class BombTorizoHandObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>Physical PLM slots searched for active header pointers.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Connects the statue-hand interpreter to the current room and PLM slot table.</summary>
    internal BombTorizoHandObjects(IRoomSamusAccess? samus, RoomObjectSlotTable slotTable)
    {
        _samus = samus;
        _slotTable = slotTable;
    }
    /// <summary>Statue-fragment spawns queued by the hand's breaking instruction for this frame.</summary>
    private readonly List<BombTorizoStatueProjectileRequest>
        _bombTorizoStatueProjectileRequests = [];
    /// <summary>Delayed track-selection commands queued by the hand program for this frame.</summary>
    private readonly List<DelayedMusicRequest> _musicRequests = [];

    /// <summary>Statue fragment projectiles emitted during the latest PLM pass.</summary>
    public IReadOnlyList<BombTorizoStatueProjectileRequest>
        BombTorizoStatueProjectileRequests => _bombTorizoStatueProjectileRequests;

    /// <summary>Delayed music commands emitted during the latest PLM pass.</summary>
    public IReadOnlyList<DelayedMusicRequest> MusicRequests => _musicRequests;

    /// <summary>
    /// Tests the physical PLM header array exactly as enemy code such as
    /// Bomb Torizo's awakening check does. This is intentionally generic: the enemy does not know which
    /// translated family owns a slot, only whether one of forty header words matches.
    /// </summary>
    public bool HasActiveHeader(RoomObjectHeaderId header) =>
        _slotTable.Slots.Any(slot => slot.Active && slot.Header == header);

    /// <summary>Runs the hand PLM's setup after the generic room allocator has installed the header.</summary>
    internal void SetupBombTorizoHandSlot(
        RoomLevelData level,
        RoomObjectSlot slot,
        IAreaBossFlagReader bosses)
    {
        if (bosses.IsDefeated(BossBits.AreaTorizo))
        {
            // Setup deletes synchronously, so the next population record may reuse this ID.
            slot.Clear();
            return;
        }

        slot.StartAnimation(new BombTorizoHandObjectAnimation());
        _bombTorizoHandRoomWidth = level.Size.X;
    }

    /// <summary>Discards the previous PLM pass's fragment and music requests before the next pass.</summary>
    internal void BeginBombTorizoHandFrame()
    {
        _bombTorizoStatueProjectileRequests.Clear();
        _musicRequests.Clear();
    }

    /// <summary>
    /// Runs the hand's frame behaviour before the ordinary PLM timer pass.
    /// </summary>
    internal void RunBombTorizoHandFrameBehavior(RoomObjectSlot slot)
    {
        IRoomSamus samus = _samus?.Current
            ?? throw new InvalidOperationException(
                "A live Bomb Torizo hand PLM has no active Samus inventory owner.");
        if (!samus.CollectedItems.HasAny(SamusEquipmentFlags.Bombs))
            return;

        // The sleeping step leaves the script position at itself. The frame behaviour skips past it,
        // restores the frame hold to one, and replaces its frame behaviour with an empty one.
        slot.WakePastSleep();
        slot.FrameBehavior = RoomObjectFrameBehaviors.Inert;
    }
    /// <summary>Spawns statue fragment <paramref name="fragment"/> at the hand's block.</summary>
    internal void SpawnFragment(RoomObjectSlot slot, int fragment) =>
        _bombTorizoStatueProjectileRequests.Add(new BombTorizoStatueProjectileRequest(
            fragment,
            // The fragment spawn carries both block coordinates as bytes.
            new(slot.BlockIndex % _bombTorizoHandRoomWidth,
                slot.BlockIndex / _bombTorizoHandRoomWidth)));

    /// <summary>
    /// Queues the loaded set's second song with the shared eight-frame delay.
    /// </summary>
    internal void QueueMusic() =>
        _musicRequests.Add(new DelayedMusicRequest(MusicRequest.SelectTrack(MusicTrack.SecondSetSong), MusicQueueDelay.Shortest));

    /// <summary>Room width in blocks used to turn the hand's linear PLM block index into fragment spawn coordinates.</summary>
    private int _bombTorizoHandRoomWidth;

    /// <summary>Removes the hand's header and wake pre-instruction once its PLM has been deleted.</summary>
    internal static void MarkBombTorizoHandDeleted(RoomObjectSlot slot)
    {
        if (slot.Header != RoomObjectHeaderId.BombTorizoHand)
            return;
        slot.Header = 0;
        slot.FrameBehavior = RoomObjectFrameBehaviors.Inert;
    }
}
