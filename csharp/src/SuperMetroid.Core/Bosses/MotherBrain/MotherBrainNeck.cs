namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's neck (#2314): three lower joints hanging from her body and two upper joints hanging from the last
/// lower joint, each at its own distance; her brain sits on the last upper joint. Made with her encounter.
/// </summary>
public sealed class MotherBrainNeck
{
    /// <summary>Makes the neck's joints at their distances.</summary>
    internal MotherBrainNeck()
    {
        Lower = [.. MotherBrainNeckJointDefinitions.LowerJoints.Select(definition => new MotherBrainNeckJoint(definition))];
        Upper = [.. MotherBrainNeckJointDefinitions.UpperJoints.Select(definition => new MotherBrainNeckJoint(definition))];
        Joints = [.. Lower, .. Upper];
        ExposedJoints = [.. Joints.Where(joint => joint.Exposure == MotherBrainNeckJointExposure.Exposed)];
    }

    /// <summary>The lower joints, from the body up, which hang from her body on the lower-neck angle.</summary>
    public IReadOnlyList<MotherBrainNeckJoint> Lower { get; }

    /// <summary>The upper joints, from the last lower joint up, which hang from it on the upper-neck angle.</summary>
    public IReadOnlyList<MotherBrainNeckJoint> Upper { get; }

    /// <summary>The last lower joint, from which the upper joints hang.</summary>
    public MotherBrainNeckJoint UpperAnchor => Lower[^1];

    /// <summary>The last upper joint, on which her brain sits.</summary>
    public MotherBrainNeckJoint BrainAnchor => Upper[^1];

    /// <summary>Every joint from the body to the brain.</summary>
    public IReadOnlyList<MotherBrainNeckJoint> Joints { get; }

    /// <summary>
    /// The joints Samus can touch, from the body to the brain: each joint's definition says whether it is
    /// (<see cref="MotherBrainNeckJointExposure"/>).
    /// </summary>
    public IReadOnlyList<MotherBrainNeckJoint> ExposedJoints { get; }
}
