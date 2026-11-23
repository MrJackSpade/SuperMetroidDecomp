namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>A carrying Chozo statue's step: its velocity and where it holds Samus, from its pose and the way it faces.</summary>
public static partial class ChozoCarry
{
    /// <summary>The statue's velocity and Samus's place in its hand at <paramref name="step"/>.</summary>
    /// <remarks>
    /// The velocity is the pose's walking speed toward the way the statue faces. Samus's place is the pose's hold
    /// mirrored to the statue's facing, added to the statue's position after it moves.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The step names an undefined facing or pose.</exception>
    public static ChozoCarryStepDefinition Read(ChozoCarryStep step)
    {
        ChozoCarryPoseDefinition pose = ChozoCarryPoseDefinitions.Poses[step.Pose];
        return new(pose.Speed.Toward(step.Facing),
            new(pose.Hold.X.Toward(step.Facing), pose.Hold.Y));
    }
}
