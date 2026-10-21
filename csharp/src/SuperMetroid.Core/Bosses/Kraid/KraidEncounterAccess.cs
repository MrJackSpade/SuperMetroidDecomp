using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Kraid's state and part lookups, part programs, function timer, random numbers and the sound he last requested.</summary>
[RoomLifetime]
internal sealed class KraidEncounterAccess
{
    /// <summary>Room scratch memory, services, and tilemap state used throughout the Kraid fight.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal KraidEncounterAccess(EnemyRoomBinding room)
    {
        _room = room;
    }

    /// <summary>Most recent Kraid-private sound request in the current enemy frame.</summary>
    public KraidSoundRequest? LastKraidSoundEffect { get; internal set; }

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
        WrappingCountdown functionTimer = WrappingCountdown.FromRaw(checked((ushort)part.FunctionTimer));
        if (functionTimer.IsZero)
            return;
        CountdownStep step = functionTimer.Decrement();
        part.FunctionTimer = step.Remaining.RawValue;
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
                $"Enemy ${(int)slot.DefinitionId:X4} requires Kraid's body to have initialized this room.");
        }
        return Kraid;
    }

    /// <summary>The state of the Kraid part in <paramref name="slot"/>.</summary>
    internal KraidPartState Part(RoomEnemySlot slot) => RequireKraidState(slot).Parts[slot.SlotIndex];

    /// <summary>Installs a part's state with its slot, so a kill empties its record words.</summary>
    internal static void RegisterKraidPart(RoomEnemySlot slot, KraidEnemyState state)
    {
        KraidPartState part = state.Parts[slot.SlotIndex];
        slot.SetSpeciesState(part, part.ClearWithRecord);
    }
}
