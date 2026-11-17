using System.Collections.Immutable;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's articulated neck (#2314): its lower joints hang from her body on the lower-neck angle, and its upper
/// joints hang from the last lower joint on the upper-neck angle; her brain sits on the last upper joint.
/// </summary>
internal static class MotherBrainNeckJointDefinitions
{
    /// <summary>
    /// Where the lower joints hang from: thirty-two pixels right of and fifty above her body's position.
    /// </summary>
    internal static readonly Offset16<RoomPixels> LowerNeckRoot = new(32, -50);

    /// <summary>Each lower joint's pixel distance from <see cref="LowerNeckRoot"/>, from the body up.</summary>
    internal static ImmutableArray<int> LowerJointDistances { get; } = [2, 10, 20];

    /// <summary>Each upper joint's pixel distance from the last lower joint, from it up to the brain.</summary>
    internal static ImmutableArray<int> UpperJointDistances { get; } = [10, 20];

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
