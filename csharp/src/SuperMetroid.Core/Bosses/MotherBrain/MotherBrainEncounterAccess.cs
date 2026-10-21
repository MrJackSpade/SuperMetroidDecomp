using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's state lookups, body and brain program setters, timers and PLM requests.</summary>
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

    /// <summary>Requests a bank-$84 PLM spawn at a foreground room-block coordinate for this pass.</summary>
    internal void RequestMotherBrainPlm(Position16<RoomBlocks> block, PlmHeaderId header) =>
        _frameResults.MotherBrainPlms.Add(new PlmSpawnRequest(block, header));

    /// <summary>Ports <c>SetMotherBrainHeadInstList</c> ($A9:C447): restarts the brain on <paramref name="program"/>.</summary>
    internal static void SetMotherBrainBrainProgram(MotherBrainEnemyState state, MotherBrainHeadProgram program)
    {
        state.BrainCursor = AnimationProgramCursor<MotherBrainHeadProgram>.Start(program);
        state.BrainFrameTimer = 1;
    }

    /// <summary>Requires the active rainbow attack sequence for live phase transitions.</summary>
    internal static MotherBrainRainbowBeamAttackSequence RequireLiveMotherBrainRainbowSequence(
        MotherBrainEnemyState state) =>
        state.RainbowBeamSequence ??
        throw new InvalidDataException(
            "Mother Brain's brain program requires the live rainbow/Baby sequence.");

    /// <summary>Requires a body and head pair initialized in their native enemy slots.</summary>
    internal MotherBrainEnemyState RequireCompleteMotherBrainState(RoomEnemySlot slot)
    {
        MotherBrainEnemyState state = _motherBrainEncounter.MotherBrain ??
            throw new InvalidDataException("Mother Brain record has no shared encounter state.");
        if (!ReferenceEquals(slot, state.Body) && !ReferenceEquals(slot, state.Head))
            throw new InvalidDataException("A non-Mother-Brain slot entered the Mother Brain dispatcher.");
        return state;
    }

    /// <summary>
    /// Native timers expire only after unsigned decrement produces a negative signed word.
    /// A zero timer therefore expires immediately, while a value N remains live for N ticks.
    /// </summary>
    internal static bool DecrementMotherBrainTimerPastZero(MotherBrainEnemyState state)
    {
        CountdownStep timer = state.FunctionTimer.Decrement();
        state.FunctionTimer = timer.Remaining;
        return timer.IsNegative;
    }

    /// <summary>Ports <c>$A9:C42D</c>: restarts the body enemy on <paramref name="program"/> with its initial timer.</summary>
    internal static void SetMotherBrainBodyProgram(MotherBrainEnemyState state, MotherBrainBodyProgram program)
    {
        state.BodyCursor = AnimationProgramCursor<MotherBrainBodyProgram>.Start(program);
        state.Body.InstructionTimer = WrappingCountdown.Start(1);
        state.Body.Timer = WrappingCountdown.Start(0);
    }
}
