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

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidEncounterAccess(EnemyRoomBinding room, EnemyAudioQueue audio)
    {
        _room = room;
        _audio = audio;
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

    /// <summary>The room's current random number, which Kraid's next decision reads without advancing it.</summary>
    internal int ReadKraidRandomNumber() =>
        _room.Random.Read();

    /// <summary>
    /// The colours Kraid's room backdrop shows (#1512). They belong to the room, not to Kraid: they last
    /// the room's lifetime, so a re-initialization of Kraid, which begins a new encounter, keeps what his growth
    /// fade showed.
    /// </summary>
    internal KraidBackdropColours Backdrop { get; } = new();

    /// <summary>
    /// Deletes a Kraid part as his body sinks away (#2298): it stops being solid, ignores Samus, stops its animation and
    /// off-screen processing, and is deleted; its other properties are kept.
    /// </summary>
    internal static void DeleteKraidPart(RoomEnemyPlace place)
    {
        place.Contact = place.Contact.Replace(EnemyContact.SolidToSamus | EnemyContact.IgnoresSamus, EnemyContact.IgnoresSamus);
        place.Processing = place.Processing.Without(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
        place.Presence = place.Presence.With(EnemyPresence.Deleted);
    }

    /// <summary>
    /// The <typeparamref name="TPart"/> part another of Kraid's parts finds when it reads <paramref name="place"/>
    /// (<see cref="RoomEnemyPlace.FoundEnemy"/>): the part every one of his parts holds, or null when the place reads as
    /// holding none.
    /// </summary>
    /// <exception cref="InvalidDataException">The place holds another kind of Kraid part.</exception>
    internal static TPart? FoundKraidPart<TPart>(RoomEnemyPlace place) where TPart : KraidPartState =>
        Ensure.NotNull(place).FoundEnemy is not KraidEntity found ? null : found.PartState switch
        {
            TPart part => part,
            KraidPartState other => throw new InvalidDataException(
                $"Kraid's part at enemy {place.Ordinal} is a {other.GetType().Name}, not the {typeof(TPart).Name} its neighbour reads."),
        };
}
