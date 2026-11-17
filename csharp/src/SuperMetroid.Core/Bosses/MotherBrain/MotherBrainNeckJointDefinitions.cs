using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's articulated neck joints, indexed from the body (joint 0) to the brain (joint 4), as
/// the neck placement places them: the lower joints hang from the body reference on the lower-neck angle, and the
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

    /// <summary>Joint 4: the articulated head follows it, raised by <see cref="BrainAboveAnchor"/>.</summary>
    internal const int BrainAnchor = 4;

    /// <summary>Where the articulated head sits on <see cref="BrainAnchor"/>: twenty-one pixels above it.</summary>
    internal static readonly Offset16<RoomPixels> BrainAboveAnchor = new(0, -21);

    /// <summary>The lower-neck angle the main tube's landing gives the neck.</summary>
    internal static readonly RotationAngle InitialLowerAngle = RotationAngle.FromSteps(72);

    /// <summary>The upper-neck angle the main tube's landing gives the neck.</summary>
    internal static readonly RotationAngle InitialUpperAngle = RotationAngle.FromSteps(80);

    /// <summary>The height above which her brain turns her neck's bob up back down.</summary>
    internal const int BobUpHighestBrainY = 60;

    /// <summary>How far below her brain Samus may be for her neck's bob down to turn both halves back up.</summary>
    internal const int BobDownSamusBelowBrain = 4;

    /// <summary>The neck's reference point from her body, from which <see cref="LowerAnchorFromReference"/> reaches the lower joints' anchor.</summary>
    internal static readonly Offset16<RoomPixels> ReferenceFromBody = new(-80, 46);

    /// <summary>The lower joints' anchor from the neck's reference point: body+(32,-50) in all.</summary>
    internal static readonly Offset16<RoomPixels> LowerAnchorFromReference = new(112, -96);

    /// <summary>
    /// The farthest a joint lies from its anchor, in pixels (#1627): the game multiplies the sine by the distance as a
    /// signed count of whole pixels, which goes no further, and the neck's distances are 2, 10 and 20, so a farther one is
    /// rejected rather than given a product the game never forms.
    /// </summary>
    internal const int FarthestJointDistance = 127;

    /// <summary>The joints the neck hitboxes test: joint 0 is buried in the body and joint 4 is covered by the brain rectangles.</summary>
    internal static Range CollisionJoints { get; } = 1..4;

    /// <summary>Each joint's pixel distance from its anchor as the main tube's landing initializes it, indexed by joint.</summary>
    internal static ImmutableArray<int> InitialDistances { get; } = [2, 10, 20, 10, 20];
}
