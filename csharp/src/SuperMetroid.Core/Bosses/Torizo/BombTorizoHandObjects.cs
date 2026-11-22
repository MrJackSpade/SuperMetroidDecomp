using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The Bomb Torizo statue's hand: its setup, the fragments and music it requests, and whether it still stands, which
/// Bomb Torizo waits on.
/// </summary>
internal sealed partial class BombTorizoHandObjects
{
    /// <summary>Samus as the room reads and changes her.</summary>
    private readonly IRoomSamusAccess _samus;

    /// <summary>The room's statue hand, from its setup on; null in a room without one or whose Torizo is defeated.</summary>
    private BombTorizoHandObject? _hand;

    /// <summary>The place the hand holds in the room's update order, which it frees as it leaves.</summary>
    private RoomObjectPlace? _handPlace;

    /// <summary>Connects the statue hand to Samus and to the room objects' audio requests.</summary>
    internal BombTorizoHandObjects(IRoomSamusAccess samus, RoomObjectSoundQueue sounds)
    {
        _samus = samus;
        _sounds = Ensure.NotNull(sounds);
    }
    /// <summary>The room objects' audio requests, which the hand's music joins.</summary>
    private readonly RoomObjectSoundQueue _sounds;

    /// <summary>
    /// Whether the statue's hand still stands in the room: from its setup until it leaves after its crumble. Bomb Torizo
    /// stays dormant while it does.
    /// </summary>
    public bool HandRemains => _handPlace is { Active: true } place && ReferenceEquals(place.Behaviour, _hand);

    /// <summary>
    /// Sets up the hand the room's population placed at <paramref name="place"/>, throwing its fragments among
    /// <paramref name="fragments"/>: the room's enemy projectiles, which the room's enemies built before its room objects are
    /// set up. A defeated Torizo's room has none.
    /// </summary>
    internal void SetupBombTorizoHand(
        RoomObjectPlace place,
        IDefeatedBossesReader bosses,
        IBombTorizoStatueFragmentSpawner fragments)
    {
        Ensure.NotNull(place);
        Ensure.NotNull(bosses);
        if (bosses.IsDefeated(DefeatedBosses.AreaTorizo))
        {
            // The setup removes the hand at once, so the next population entry may take its place.
            place.Delete();
            return;
        }

        var hand = new BombTorizoHandObject(this, fragments);
        place.Become(hand);
        _hand = hand;
        _handPlace = place;
    }

    /// <summary>Whether Samus has Bombs, which wakes the resting hand.</summary>
    internal bool SamusHasBombs() =>
        _samus.Current.CollectedItems.HasAny(SamusEquipmentFlags.Bombs);
    /// <summary>
    /// Throws statue fragment <paramref name="fragment"/> from the hand's block into the step's <paramref name="outcome"/>, to
    /// spawn among <paramref name="spawner"/>.
    /// </summary>
    internal static void ThrowFragment(RoomObjectPlace place, BombTorizoStatueFragment fragment, RoomObjectStepOutcome outcome,
        IBombTorizoStatueFragmentSpawner spawner) =>
        Ensure.NotNull(outcome).Threw(new BombTorizoStatueProjectileRequest(fragment, Ensure.NotNull(place).Block, spawner));

    /// <summary>
    /// Queues the Torizo's awakening music now, after the shortest wait a queued song takes.
    /// </summary>
    internal void QueueMusic() =>
        _sounds.ScheduleMusic(new DelayedMusicRequest(MusicRequest.SelectTrack(MusicTrack.TorizoAwakening), MusicQueueDelay.Shortest));

}
