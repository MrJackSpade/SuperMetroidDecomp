using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Mother Brain's state lookups, body and brain animation setters, timers and room-object requests.</summary>
internal sealed partial class MotherBrainEncounterAccess
{
    /// <summary>The enemy pass's results, which this behavior writes.</summary>
    private readonly EnemyPassEvents _passEvents;



    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal MotherBrainEncounterAccess(EnemyPassEvents passEvents)
    {
        _passEvents = passEvents;
    }

    /// <summary>Requests one of Mother Brain's room mutations at a foreground room block for this pass.</summary>
    internal void RequestMotherBrainObject(Position16<RoomBlocks> block, MotherBrainRoomMutation mutation) =>
        _passEvents.Publish(new MotherBrainMutationRequest(block, mutation));

    /// <summary>Requests the reveal of <paramref name="row"/> of the mural behind her tank, at the row's own block, for this pass.</summary>
    internal void RequestMuralRowReveal(MotherBrainMuralRow row) =>
        _passEvents.Publish(new MotherBrainMuralRowReveal(row));

    /// <summary>Starts the brain animation that begins at <paramref name="segment"/>.</summary>
    internal static void SetMotherBrainBrainAnimation(MotherBrainEnemyState state, MotherBrainHeadSegment segment) =>
        state.Brain.Start(MotherBrainAnimations.HeadStartingWith(segment));

    /// <summary>Restarts the body enemy on <paramref name="segment"/> with its initial timer.</summary>
    internal static void SetMotherBrainBodyAnimation(MotherBrainEnemyState state, MotherBrainBodySegment segment)
    {
        state.BodyPlayback = EnemyAnimationPlayback.Start(state.Body, MotherBrainAnimations.BodyStartingWith(segment));
    }
}
