using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's articulated neck (#2314): its lower joints hang from her body on the lower-neck angle, and its upper
/// joints hang from the last lower joint on the upper-neck angle; her brain sits on the last upper joint.
/// </summary>
internal static partial class MotherBrainNeckJointDefinitions
{
    /// <summary>
    /// Where the lower joints hang from: thirty-two pixels right of and fifty above her body's position.
    /// </summary>
    internal static readonly Offset16<RoomPixels> LowerNeckRoot = new(32, -50);

    /// <summary>
    /// The lower joints, from the body up, each its pixel distance from <see cref="LowerNeckRoot"/>: the first lies inside
    /// her body.
    /// </summary>
    internal static ImmutableArray<MotherBrainNeckJointDefinition> LowerJoints { get; } =
        [new(2, MotherBrainNeckJointExposure.InsideBody), new(10, MotherBrainNeckJointExposure.Exposed), new(20, MotherBrainNeckJointExposure.Exposed)];

    /// <summary>
    /// The upper joints, from the last lower joint up to the brain, each its pixel distance from that joint: her brain's hit
    /// area covers the last.
    /// </summary>
    internal static ImmutableArray<MotherBrainNeckJointDefinition> UpperJoints { get; } =
        [new(10, MotherBrainNeckJointExposure.Exposed), new(20, MotherBrainNeckJointExposure.UnderBrain)];

    /// <summary>Where the articulated head sits on the neck's last joint: twenty-one pixels above it.</summary>
    internal static readonly Offset16<RoomPixels> BrainAboveAnchor = new(0, -21);

    /// <summary>The lower-neck angle the main tube's landing gives the neck.</summary>
    internal static readonly RotationAngle InitialLowerAngle = RotationAngle.FromSteps(72);

    /// <summary>The upper-neck angle the main tube's landing gives the neck.</summary>
    internal static readonly RotationAngle InitialUpperAngle = RotationAngle.FromSteps(80);

    /// <summary>The height above which her brain turns her neck's bob up back down.</summary>
    internal const int BobUpHighestBrainY = 60;

    /// <summary>How far below her brain Samus may be for her neck's bob down to turn both halves back up.</summary>
    internal const int BobDownSamusBelowBrain = 4;

}
