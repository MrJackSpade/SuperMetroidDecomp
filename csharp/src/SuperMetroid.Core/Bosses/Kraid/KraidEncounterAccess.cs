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

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidEncounterAccess(EnemyRoomBinding room, EnemyAudioQueue audio)
    {
        _room = room;
        _audio = audio;
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

    /// <summary>Rejects a body part loaded outside its cartridge-defined slot index.</summary>
    internal static void EnsureKraidSlot(RoomEnemySlot slot, int expectedSlot, string part)
    {
        if (slot.SlotIndex != expectedSlot)
        {
            throw new InvalidDataException(
                $"Kraid {part} requires native slot {expectedSlot}, not {slot.SlotIndex}.");
        }
    }

    /// <summary>Ports `$A7:A943`: preserve raw upper flags while installing `$0700`.</summary>
    internal static void MarkKraidPartDead(RoomEnemySlot slot) =>
        slot.Properties = slot.Properties.Replace(
            EnemyProperties.SolidToSamus |
                EnemyProperties.ProcessInstructions |
                EnemyProperties.ProcessOffScreen |
                EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.Deleted |
                EnemyProperties.Invisible,
            EnemyProperties.IgnoreSamusCollision |
                EnemyProperties.Deleted |
                EnemyProperties.Invisible);

    /// <summary>Installs <paramref name="program"/> as the part's program, as a native list-pointer store does; the caller sets the timer.</summary>
    internal void SetKraidPartProgram(RoomEnemySlot part, KraidPartProgram program) =>
        RequireKraidState(part).Parts[part.SlotIndex].Cursor = AnimationProgramCursor<KraidPartProgram>.Start(program);

    /// <summary>Sets a part's program and runs its first instruction on the next frame.</summary>
    internal void StartKraidPartProgram(RoomEnemySlot part, KraidPartProgram program)
    {
        SetKraidPartProgram(part, program);
        part.InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>Consumes one value from the room RNG for Kraid's next decision.</summary>
    internal int ReadKraidRandomNumber() =>
        _room.Random.Read();

    /// <summary>Counts a Kraid function timer down and installs its next state at zero.</summary>
    internal static void TickKraidFunctionTimer(KraidPartState part)
    {
        WrappingCountdown functionTimer = part.FunctionTimer;
        if (functionTimer.IsZero)
            return;
        CountdownStep step = functionTimer.Decrement();
        part.FunctionTimer = step.Remaining;
        if (step.IsZero)
            part.Function = part.NextFunction;
    }

    /// <summary>Active typed state when the loaded room owns retail Kraid slot zero.</summary>
    public KraidEnemyState? Kraid { get; internal set; }

    /// <summary>
    /// Kraid's room state, created by the body's initializer and kept for the room. Parts
    /// read slot zero's words directly, so they keep running after the dead-room body is
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
    internal KraidPartState Part(RoomEnemySlot slot) => RequireKraidState(slot).Parts[slot.SlotIndex];

    /// <summary>Installs a part's state with its slot, so a kill empties its record values.</summary>
    internal static void AttachKraidPart(RoomEnemySlot slot, KraidEnemyState state)
    {
        KraidPartState part = state.Parts[slot.SlotIndex];
        slot.SetSpeciesState(part, part.ClearWithRecord);
    }
}
