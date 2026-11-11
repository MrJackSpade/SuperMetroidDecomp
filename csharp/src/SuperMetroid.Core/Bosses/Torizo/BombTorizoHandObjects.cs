using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>The Bomb Torizo statue's hand: its setup, the fragments and music it requests, and the identity search Bomb Torizo makes.</summary>
[RoomLifetime]
internal sealed class BombTorizoHandObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;
    /// <summary>The room-object slots searched for active headers.</summary>
    private readonly RoomObjectSlotTable _slotTable;

    /// <summary>Connects the statue-hand interpreter to the current room and room-object slot table.</summary>
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

    /// <summary>Statue fragment projectiles emitted during the latest room-object pass.</summary>
    public IReadOnlyList<BombTorizoStatueProjectileRequest>
        BombTorizoStatueProjectileRequests => _bombTorizoStatueProjectileRequests;

    /// <summary>Delayed music commands emitted during the latest room-object pass.</summary>
    public IReadOnlyList<DelayedMusicRequest> MusicRequests => _musicRequests;

    /// <summary>
    /// Tests the room-object headers exactly as enemy code such as
    /// Bomb Torizo's awakening check does. This is intentionally generic: the enemy does not know which
    /// family owns a slot, only whether one of forty headers matches.
    /// </summary>
    public bool HasActiveHeader(RoomObjectHeaderId header) =>
        _slotTable.Slots.Any(slot => slot.Active && slot.Header == header);

    /// <summary>Runs the hand room object's setup after the generic room allocator has installed the header.</summary>
    internal void SetupBombTorizoHandSlot(
        RoomObjectSlot slot,
        IAreaBossFlagReader bosses)
    {
        if (bosses.IsDefeated(BossBits.AreaTorizo))
        {
            // Setup deletes synchronously, so the next population record may reuse this ID.
            slot.Clear();
            return;
        }

        slot.Become(new BombTorizoHandObject(this));
    }

    /// <summary>Discards the previous room-object pass's fragment and music requests before the next pass.</summary>
    internal void BeginBombTorizoHandFrame()
    {
        _bombTorizoStatueProjectileRequests.Clear();
        _musicRequests.Clear();
    }

    /// <summary>Whether Samus has Bombs, which wakes the resting hand.</summary>
    internal bool SamusHasBombs() =>
        (_samus?.Current ?? throw new InvalidOperationException(
            "A live Bomb Torizo hand has no active Samus inventory owner.")).CollectedItems.HasAny(SamusEquipmentFlags.Bombs);
    /// <summary>Spawns statue fragment <paramref name="fragment"/> at the hand's block.</summary>
    internal void SpawnFragment(RoomObjectSlot slot, int fragment) =>
        _bombTorizoStatueProjectileRequests.Add(new BombTorizoStatueProjectileRequest(
            fragment,
            // The fragment spawn carries both block coordinates as bytes.
            slot.Block));

    /// <summary>
    /// Queues the loaded set's second song with the shared eight-frame delay.
    /// </summary>
    internal void QueueMusic() =>
        _musicRequests.Add(new DelayedMusicRequest(MusicRequest.SelectTrack(MusicTrack.SecondSetSong), MusicQueueDelay.Shortest));

}
