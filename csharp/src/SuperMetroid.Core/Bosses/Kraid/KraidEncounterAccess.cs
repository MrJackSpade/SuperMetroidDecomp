using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's part animations and states, random numbers and his sound and music requests.</summary>
[RoomLifetime]
internal sealed class KraidEncounterAccess
{
    /// <summary>Room services used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Receives Kraid's sound and music requests.</summary>
    private readonly EnemyAudioQueue _audio;
    /// <summary>Kraid's eight parts in his room, which name each part's workspace.</summary>
    private readonly KraidParts _parts;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidEncounterAccess(EnemyRoomBinding room, EnemyAudioQueue audio, KraidParts parts)
    {
        _room = room;
        _audio = audio;
        _parts = parts;
    }

    /// <summary>Queues one of Kraid's sounds as Kraid makes it, after the update's earlier requests.</summary>
    internal void RequestKraidSound(SoundEffectId sound) =>
        _audio.QueueEnemySound(sound, SoundRequestPriority.Normal);

    /// <summary>Queues Kraid's music change as Kraid makes it.</summary>
    internal void RequestKraidMusic(MusicRequest request) =>
        _audio.QueueEnemyMusic(new DelayedMusicRequest(request, MusicQueueDelay.Shortest));

    /// <summary>
    /// Marks a Kraid part dead: it stops being solid, ignores Samus, stops its animation and off-screen processing,
    /// and is hidden and deleted; its other properties are kept.
    /// </summary>
    internal static void MarkKraidPartDead(RoomEnemyPlace place)
    {
        place.Contact = place.Contact.Replace(EnemyContact.SolidToSamus | EnemyContact.IgnoresSamus, EnemyContact.IgnoresSamus);
        place.Processing = place.Processing.Without(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
        place.Presence = place.Presence.With(EnemyPresence.Deleted | EnemyPresence.Hidden);
    }

    /// <summary>Installs <paramref name="segment"/> as <paramref name="kraid"/>'s part's animation, from its start; the part's timer is set separately.</summary>
    internal void SetKraidPartAnimation(KraidEnemyState kraid, RoomEnemyPlace part, KraidPartSegment segment) =>
        _parts.StateOf(kraid, part).Playback =
            new AnimationPlayback<KraidPartSegment>(KraidAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);

    /// <summary>Sets a part's animation and runs its first step on the next frame.</summary>
    internal void StartKraidPartAnimation(KraidEnemyState kraid, RoomEnemyPlace part, KraidPartSegment segment)
    {
        SetKraidPartAnimation(kraid, part, segment);
        part.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>The room's current random number, which Kraid's next decision reads without advancing it.</summary>
    internal int ReadKraidRandomNumber() =>
        _room.Random.Read();

    /// <summary>
    /// The colours Kraid's room backdrop shows (#1512). They belong to the room, not to Kraid: they last
    /// the room's lifetime, so a re-initialization of Kraid, which begins a new encounter, keeps what his growth
    /// fade showed.
    /// </summary>
    internal KraidBackdropColours Backdrop { get; } = new();

    /// <summary>Installs a part's state for the part's occupant of its place; the encounter keeps it across the part's kills and respawns.</summary>
    internal void AttachKraidPart(RoomEnemyPlace place, KraidEnemyState state)
    {
        KraidPartState part = _parts.StateOf(state, place);
        place.SetSpeciesState(part);
    }
}
