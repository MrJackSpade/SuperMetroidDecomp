using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's articulated neck joints, indexed from the body (joint 0) to the brain (joint 4), as
/// <c>$A9:91B8-$92AA</c> places them: the lower joints hang from the body reference on the lower-neck angle, and the
/// upper joints hang from the upper-neck anchor on the upper-neck angle.
/// </summary>
internal static class MotherBrainNeckJointDefinitions
{
    /// <summary>The number of neck joints.</summary>
    internal const int Count = 5;

    /// <summary>The number of lower joints, 0 through 2, placed from the body reference.</summary>
    internal const int LowerCount = 3;

    /// <summary>Joint 2: the last lower joint, from which the upper joints hang.</summary>
    internal const int UpperAnchor = 2;

    /// <summary>Joint 4: the articulated head follows its X and its Y less twenty-one pixels.</summary>
    internal const int BrainAnchor = 4;

    /// <summary>The joints the neck hitboxes test: joint 0 is buried in the body and joint 4 is covered by the brain rectangles.</summary>
    internal static Range CollisionJoints { get; } = 1..4;

    /// <summary>Each joint's pixel distance from its anchor as <c>$A9:903F</c> initializes it, indexed by joint.</summary>
    internal static ImmutableArray<int> InitialDistances { get; } = [2, 10, 20, 10, 20];
}
