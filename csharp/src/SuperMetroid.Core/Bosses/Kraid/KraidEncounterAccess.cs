using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and his sound and music requests.</summary>
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
    internal static void MarkKraidPartDead(RoomEnemySlot slot)
    {
        slot.Contact = slot.Contact.Replace(EnemyContact.SolidToSamus | EnemyContact.IgnoresSamus, EnemyContact.IgnoresSamus);
        slot.Processing = slot.Processing.Without(EnemyProcessing.Animates | EnemyProcessing.OffScreen);
        slot.Presence = slot.Presence.With(EnemyPresence.Deleted | EnemyPresence.Hidden);
    }

    /// <summary>Installs <paramref name="program"/> as the part's program, from its start; the caller sets the timer.</summary>
    internal void SetKraidPartAnimation(RoomEnemySlot part, KraidPartSegment program) =>
        _parts.StateOf(RequireKraidState(part), part).Playback =
            new AnimationPlayback<KraidPartSegment>(KraidAnimations.StartingWith(program), AnimationPlace.StartOfAnimation);

    /// <summary>Sets a part's program and runs its first instruction on the next frame.</summary>
    internal void StartKraidPartAnimation(RoomEnemySlot part, KraidPartSegment program)
    {
        SetKraidPartAnimation(part, program);
        part.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Consumes one value from the room RNG for Kraid's next decision.</summary>
    internal int ReadKraidRandomNumber() =>
        _room.Random.Read();

    /// <summary>Counts a Kraid function timer down and installs its next state at zero.</summary>
    internal static void TickKraidFunctionTimer(KraidPartState part)
    {
        if (part.FunctionTimer == 0)
            return;
        if (--part.FunctionTimer == 0)
            part.Function = part.NextFunction;
    }

    /// <summary>Kraid's encounter state once his body has initialized in the room.</summary>
    public KraidEnemyState? Kraid { get; internal set; }

    /// <summary>
    /// The colours Kraid's room backdrop shows (#1512). They belong to the room, not to Kraid: they last
    /// the room's lifetime, so a re-initialization of Kraid, which replaces <see cref="Kraid"/>, keeps what his growth
    /// fade showed.
    /// </summary>
    internal KraidBackdropColours Backdrop { get; } = new();

    /// <summary>
    /// Kraid's room state, created by the body's initializer and kept for the room. Parts
    /// read the body's state directly, so they keep running after the dead-room body is
    /// deleted, as the fingernails do.
    /// </summary>
    internal KraidEnemyState RequireKraidState(RoomEnemySlot slot)
    {
        if (Kraid is null)
        {
            throw new InvalidOperationException(
                $"Enemy {slot.DefinitionId} requires Kraid's body to have initialized this room.");
        }
        return Kraid;
    }

    /// <summary>The state of the Kraid part in <paramref name="slot"/>.</summary>
    internal KraidPartState Part(RoomEnemySlot slot) => _parts.StateOf(RequireKraidState(slot), slot);

    /// <summary>The state of the Kraid lint in <paramref name="slot"/>.</summary>
    internal KraidLintState LintState(RoomEnemySlot slot) => _parts.LintStateOf(RequireKraidState(slot), slot);

    /// <summary>The state of the Kraid fingernail in <paramref name="slot"/>.</summary>
    internal KraidNailState NailState(RoomEnemySlot slot) => _parts.NailStateOf(RequireKraidState(slot), slot);

    /// <summary>Installs a part's state for the part's occupant of its slot; the encounter keeps it across the part's kills and respawns.</summary>
    internal void AttachKraidPart(RoomEnemySlot slot, KraidEnemyState state)
    {
        KraidPartState part = _parts.StateOf(state, slot);
        slot.SetSpeciesState(part);
    }
}
