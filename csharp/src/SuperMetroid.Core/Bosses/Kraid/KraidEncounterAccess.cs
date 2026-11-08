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

    /// <summary>
    /// Requests one of Kraid's sounds at the end of the enemy update. A later Kraid sound in the same update replaces
    /// it: the update-end rule of <see cref="EnemyAudioQueue.RequestSoundAtUpdateEnd"/>.
    /// </summary>
    internal void RequestKraidSound(SoundEffectId sound) =>
        _audio.RequestSoundAtUpdateEnd(EnemySoundSource.Kraid, sound, SoundRequestPriority.Normal);

    /// <summary>Requests Kraid's music change at the end of the enemy update.</summary>
    internal void RequestKraidMusic(MusicRequest request) =>
        _audio.RequestMusicAtUpdateEnd(EnemyMusicSource.Kraid, new DelayedMusicRequest(request, MusicQueueDelay.Shortest));

    /// <summary>Marks a Kraid part dead: preserves its upper flags while installing the dead properties.</summary>
    internal static void MarkKraidPartDead(RoomEnemySlot slot) =>
        slot.Properties = slot.Properties.Replace(
            EnemyProperties.SolidToSamus |
                EnemyProperties.RunsAnimationScript |
                EnemyProperties.ProcessOffScreen |
                EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.Deleted |
                EnemyProperties.Invisible,
            EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.Deleted |
                EnemyProperties.Invisible);

    /// <summary>Installs <paramref name="program"/> as the part's program, as a list-pointer store does; the caller sets the timer.</summary>
    internal void SetKraidPartProgram(RoomEnemySlot part, KraidPartProgram program) =>
        _parts.StateOf(RequireKraidState(part), part).Cursor = AnimationProgramCursor<KraidPartProgram>.Start(program);

    /// <summary>Sets a part's program and runs its first instruction on the next frame.</summary>
    internal void StartKraidPartProgram(RoomEnemySlot part, KraidPartProgram program)
    {
        SetKraidPartProgram(part, program);
        part.FrameHold = WrappingCountdown.Start(1);
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

    /// <summary>Installs a part's state with its slot, so a kill empties its record values.</summary>
    internal void AttachKraidPart(RoomEnemySlot slot, KraidEnemyState state)
    {
        KraidPartState part = _parts.StateOf(state, slot);
        slot.SetSpeciesState(part, part.ClearWithRecord);
    }
}
