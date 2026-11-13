using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The Bomb Torizo statue's hand: its setup, the fragments and music it requests, and whether it still stands, which
/// Bomb Torizo waits on.
/// </summary>
[RoomLifetime]
internal sealed class BombTorizoHandObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess? _samus;

    /// <summary>The room's statue hand, from its setup on; null in a room without one or whose Torizo is defeated.</summary>
    private BombTorizoHandObject? _hand;

    /// <summary>The place the hand holds in the room's update order, which it frees as it leaves.</summary>
    private RoomObjectSlot? _handPlace;

    /// <summary>Connects the statue hand to Samus and to the room objects' audio requests.</summary>
    internal BombTorizoHandObjects(IRoomSamusAccess? samus, RoomObjectSoundQueue sounds)
    {
        _samus = samus;
        _sounds = Ensure.NotNull(sounds);
    }
    /// <summary>Statue-fragment spawns queued by the hand's breaking cue for this frame.</summary>
    private readonly List<BombTorizoStatueProjectileRequest>
        _bombTorizoStatueProjectileRequests = [];
    /// <summary>The room objects' audio requests, which the hand's music joins.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>Statue fragment projectiles emitted during the latest room-object pass.</summary>
    public IReadOnlyList<BombTorizoStatueProjectileRequest>
        BombTorizoStatueProjectileRequests => _bombTorizoStatueProjectileRequests;

    /// <summary>
    /// Whether the statue's hand still stands in the room: from its setup until it leaves after its crumble. Bomb Torizo
    /// sleeps while it does.
    /// </summary>
    public bool HandRemains => _handPlace is { Active: true } place && ReferenceEquals(place.Behaviour, _hand);

    /// <summary>Sets up the hand the room's population placed at <paramref name="slot"/>; a defeated Torizo's room has none.</summary>
    internal void SetupBombTorizoHandSlot(
        RoomObjectSlot slot,
        IAreaBossFlagReader bosses)
    {
        Ensure.NotNull(slot);
        Ensure.NotNull(bosses);
        if (bosses.IsDefeated(BossBits.AreaTorizo))
        {
            // The setup removes the hand at once, so the next population record may take its place.
            slot.Delete();
            return;
        }

        var hand = new BombTorizoHandObject(this);
        slot.Become(hand);
        _hand = hand;
        _handPlace = slot;
    }

    /// <summary>Discards the previous room-object pass's fragment requests before the next pass.</summary>
    internal void BeginBombTorizoHandFrame()
    {
        _bombTorizoStatueProjectileRequests.Clear();
    }

    /// <summary>Whether Samus has Bombs, which wakes the resting hand.</summary>
    internal bool SamusHasBombs() =>
        (_samus?.Current ?? throw new InvalidOperationException(
            "A live Bomb Torizo hand has no active Samus inventory owner.")).CollectedItems.HasAny(SamusEquipmentFlags.Bombs);
    /// <summary>Spawns statue fragment <paramref name="fragment"/> at the hand's block.</summary>
    internal void SpawnFragment(RoomObjectSlot slot, int fragment) =>
        _bombTorizoStatueProjectileRequests.Add(new BombTorizoStatueProjectileRequest(
            fragment,
            // The fragment spawn carries both block coordinates, each below 256.
            slot.Block));

    /// <summary>
    /// Queues the loaded set's second song with the shared eight-frame delay, now.
    /// </summary>
    internal void QueueMusic() =>
        _sounds.ScheduleMusic(new DelayedMusicRequest(MusicRequest.SelectTrack(MusicTrack.SecondSetSong), MusicQueueDelay.Shortest));

}
