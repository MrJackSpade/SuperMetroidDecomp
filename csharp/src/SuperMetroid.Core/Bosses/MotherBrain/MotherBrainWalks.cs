namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's walks toward a target (#2471): the game has one forward walk and one backward walk, which her second
/// phase's bomb and hand-beam retreats and every phase of her late fight ask for with their own target and speed.
/// </summary>
internal static class MotherBrainWalks
{
    /// <summary>
    /// Walks her body forwards toward <paramref name="targetX"/> at <paramref name="speed"/>. An overshoot of the target
    /// reports it reached first; at equality the walk is still requested. A body still mid-pose then reports it waiting,
    /// ahead of the arena's front limit, which also reports the target reached. Only a standing body inside both limits
    /// starts the walk, whose animation plays later in the enemy update.
    /// </summary>
    /// <returns>Whether she has reached the walk's target or the arena's front limit.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="speed"/> is not a walking speed.</exception>
    internal static bool Forwards(MotherBrainEnemyState encounter, int targetX, MotherBrainWalkSpeed speed)
    {
        int bodyX = encounter.Body.Position.X.Whole;
        if (bodyX > targetX)
            return true;
        if (encounter.Pose != MotherBrainBodyPose.Standing)
            return false;
        if (bodyX >= MotherBrainFightDefinitions.FrontLimitX)
            return true;
        MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(encounter, GaitOf(speed).Forwards);
        return false;
    }

    /// <summary>
    /// Walks her body backwards toward <paramref name="targetX"/> at <paramref name="speed"/>. Reaching or passing the
    /// target reports it reached first, so a walk can complete its phase on the update its movement step arrives. A body
    /// still mid-pose then reports it waiting, ahead of the arena's back limit, which also reports the target reached and
    /// so stops a walk toward a target behind it there. Only a standing body inside both limits starts the walk.
    /// </summary>
    /// <returns>Whether she has reached the walk's target or the arena's back limit.</returns>
    /// <exception cref="InvalidOperationException"><paramref name="speed"/> is not a walking speed.</exception>
    internal static bool Backwards(MotherBrainEnemyState encounter, int targetX, MotherBrainWalkSpeed speed)
    {
        int bodyX = encounter.Body.Position.X.Whole;
        if (bodyX <= targetX)
            return true;
        if (encounter.Pose != MotherBrainBodyPose.Standing)
            return false;
        if (bodyX < MotherBrainFightDefinitions.BackLimitX)
            return true;
        MotherBrainEncounterAccess.SetMotherBrainBodyAnimation(encounter, GaitOf(speed).Backwards);
        return false;
    }

    /// <summary>The animations <paramref name="speed"/> walks in.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="speed"/> is not a walking speed.</exception>
    private static MotherBrainWalkGait GaitOf(MotherBrainWalkSpeed speed) =>
        MotherBrainWalkDefinitions.Gaits.TryGetValue(speed, out MotherBrainWalkGait gait)
            ? gait
            : throw new InvalidOperationException($"Undefined {nameof(MotherBrainWalkSpeed)} {speed}.");
}
