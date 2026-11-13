using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's state lookups, body and brain program setters, timers and room-object requests.</summary>
[RoomLifetime]
internal sealed class MotherBrainEncounterAccess
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;
    /// <summary>Holds the shared body/head state for the current Mother Brain room.</summary>
    private readonly MotherBrainEncounter _motherBrainEncounter;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainEncounterAccess(BossFrameResultBuilder frameResults, MotherBrainEncounter motherBrainEncounter)
    {
        _frameResults = frameResults;
        _motherBrainEncounter = motherBrainEncounter;
    }

    /// <summary>Requests one of Mother Brain's room mutations at a foreground room block for this pass.</summary>
    internal void RequestMotherBrainObject(Position16<RoomBlocks> block, MotherBrainRoomMutation mutation) =>
        _frameResults.MotherBrainObjects.Add(new RoomObjectSpawnRequest<MotherBrainRoomMutation>(block, mutation));

    /// <summary>Restarts the brain on <paramref name="program"/>.</summary>
    internal static void SetMotherBrainBrainProgram(MotherBrainEnemyState state, MotherBrainHeadProgram program)
    {
        // The brain's place rests on its next beat: the program's first, entered at once.
        state.BrainPlayback = new(MotherBrainAnimations.HeadStartingWith(program), AnimationPlace.StartOf(0));
        state.BrainFrameTimer = 1;
    }

    /// <summary>Requires the active rainbow attack sequence for live phase transitions.</summary>
    internal static MotherBrainRainbowBeamAttackSequence RequireLiveMotherBrainRainbowSequence(
        MotherBrainEnemyState state) =>
        state.RainbowBeamSequence ??
        throw new InvalidDataException(
            "Mother Brain's brain program requires the live rainbow/Baby sequence.");

    /// <summary>Requires a body and head pair initialized in their enemy slots.</summary>
    internal MotherBrainEnemyState RequireCompleteMotherBrainState(RoomEnemySlot slot)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ??
            throw new InvalidDataException("Mother Brain record has no shared encounter state.");
        if (!ReferenceEquals(slot, state.Body) && !ReferenceEquals(slot, state.Head))
            throw new InvalidDataException("A non-Mother-Brain slot entered the Mother Brain dispatcher.");
        return state;
    }

    /// <summary>
    /// Timers expire only after an unsigned decrement produces a negative signed value.
    /// A zero timer therefore expires immediately, while a value N remains live for N ticks.
    /// </summary>
    internal static bool DecrementMotherBrainTimerPastZero(MotherBrainEnemyState state)
    {
        return --state.FunctionTimer < 0;
    }

    /// <summary>Restarts the body enemy on <paramref name="program"/> with its initial timer.</summary>
    internal static void SetMotherBrainBodyProgram(MotherBrainEnemyState state, MotherBrainBodyProgram program)
    {
        state.BodyPlayback = EnemyProgramAnimation.Start(state.Body, MotherBrainAnimations.BodyStartingWith(program));
    }
}
