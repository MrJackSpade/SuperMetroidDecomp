using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's state lookups, body and brain animation setters, timers and room-object requests.</summary>
[RoomLifetime]
internal sealed class MotherBrainEncounterAccess
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly BossFrameResultBuilder _frameResults;



    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainEncounterAccess(BossFrameResultBuilder frameResults)
    {
        _frameResults = frameResults;
    }

    /// <summary>Requests one of Mother Brain's room mutations at a foreground room block for this pass.</summary>
    internal void RequestMotherBrainObject(Position16<RoomBlocks> block, MotherBrainRoomMutation mutation) =>
        _frameResults.MotherBrainObjects.Add(new RoomObjectSpawnRequest<MotherBrainRoomMutation>(block, mutation));

    /// <summary>Restarts the brain on <paramref name="segment"/>.</summary>
    internal static void SetMotherBrainBrainAnimation(MotherBrainEnemyState state, MotherBrainHeadSegment segment)
    {
        // The brain's place rests on its next beat: the animation's first, entered at once.
        state.BrainPlayback = new(MotherBrainAnimations.HeadStartingWith(segment), AnimationPlace.StartOf(0));
        state.BrainFrameTimer = 1;
    }

    /// <summary>Requires the active rainbow attack sequence for live phase transitions.</summary>
    internal static MotherBrainRainbowBeamAttackSequence RequireLiveMotherBrainRainbowSequence(
        MotherBrainEnemyState state) =>
        state.RainbowBeamSequence ??
        throw new InvalidDataException(
            "Mother Brain's brain needs the live rainbow beam and Baby sequence.");

    /// <summary>
    /// Timers expire only once a decrement takes them below zero.
    /// A zero timer therefore expires immediately, while a value N remains live for N ticks.
    /// </summary>
    internal static bool DecrementMotherBrainTimerPastZero(MotherBrainEnemyState state)
    {
        return --state.PhaseTimer < 0;
    }

    /// <summary>Restarts the body enemy on <paramref name="segment"/> with its initial timer.</summary>
    internal static void SetMotherBrainBodyAnimation(MotherBrainEnemyState state, MotherBrainBodySegment segment)
    {
        state.BodyPlayback = EnemyAnimationPlayback.Start(state.Body, MotherBrainAnimations.BodyStartingWith(segment));
    }
}
