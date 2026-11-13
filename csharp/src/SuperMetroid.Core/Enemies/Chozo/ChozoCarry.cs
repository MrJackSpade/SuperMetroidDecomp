using static SuperMetroid.Core.Enemies.Chozo.ChozoCarryMotionDefinitions;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>A carrying Chozo statue's step: its velocity and where it holds Samus, derived from its planted-foot stride.</summary>
public static class ChozoCarry
{
    /// <summary>
    /// The phase of the four-step stride <paramref name="pose"/> is, or null for the poses that take and hold Samus
    /// standing still.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pose"/> is not a defined pose.</exception>
    private static int? StridePhase(ChozoCarryPose pose) => pose switch
    {
        ChozoCarryPose.Reaching or ChozoCarryPose.Closing or ChozoCarryPose.Gripping or ChozoCarryPose.Holding => null,
        ChozoCarryPose.StrideWeightShift => 0,
        ChozoCarryPose.StrideLift => 1,
        ChozoCarryPose.StrideSwing => 2,
        ChozoCarryPose.StridePlant => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(pose), pose, $"Undefined {nameof(ChozoCarryPose)}."),
    };

    /// <summary>Derives the authored statue stride burst, in whole pixels, from adjacent planted-foot positions.</summary>
    private static int StridePixels(int phase) => phase == 0
        ? WeightShiftAdvance
        : ChozoStride.SupportFootX(phase) - ChozoStride.SupportFootX(phase - 1) + (phase == 3 ? FinalPlantSlide : 0);

    /// <summary>
    /// Samus's vertical hand offset, shared by both facings. The two acquisition poses place Samus 32 and 25 pixels above
    /// the statue. Gripped and held she is 23 pixels above, with a two-pixel triangular bob through the stride's four
    /// steps. The carrying movement cues traverse the stride; the release cues reverse the acquisition poses.
    /// </summary>
    private static int SamusYOffset(ChozoCarryPose pose) => pose switch
    {
        ChozoCarryPose.Reaching => -32,
        ChozoCarryPose.Closing => -25,
        ChozoCarryPose.Gripping or ChozoCarryPose.Holding => -23,
        ChozoCarryPose.StrideWeightShift or ChozoCarryPose.StrideLift or ChozoCarryPose.StrideSwing or ChozoCarryPose.StridePlant =>
            -23 - StrideBob(StridePhase(pose) ?? throw new InvalidOperationException($"{pose} is a stride step without a phase.")),
        _ => throw new ArgumentOutOfRangeException(nameof(pose), pose, $"Undefined {nameof(ChozoCarryPose)}."),
    };

    /// <summary>How far Samus bobs up at stride phase <paramref name="phase"/>: a triangle rising two pixels and back.</summary>
    private static int StrideBob(int phase) => 2 - Math.Abs(2 - phase);

    /// <summary>Samus's horizontal distance from the statue in its hand, in pixels toward the way it faces.</summary>
    private static int SamusReach(ChozoCarryPose pose) => pose switch
    {
        ChozoCarryPose.Reaching => 28,
        ChozoCarryPose.Closing => 30,
        ChozoCarryPose.Gripping or ChozoCarryPose.Holding or ChozoCarryPose.StrideWeightShift or ChozoCarryPose.StrideLift
            or ChozoCarryPose.StrideSwing or ChozoCarryPose.StridePlant => 32,
        _ => throw new ArgumentOutOfRangeException(nameof(pose), pose, $"Undefined {nameof(ChozoCarryPose)}."),
    };

    /// <summary>The statue's velocity and Samus's place in its hand at <paramref name="step"/>.</summary>
    /// <remarks>
    /// The velocity is the stride's burst in pixels an update, toward the way the statue faces, and zero for the poses
    /// that take and hold Samus. The caller adds the signed whole-pixel hand offset after moving the statue, wrapping
    /// the position past 65535. Investigation: #625 / #666.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The step names an undefined facing or pose.</exception>
    public static ChozoCarryStepDefinition Read(ChozoCarryStep step)
    {
        int sign = step.Facing switch
        {
            HorizontalDirection.Left => -1,
            HorizontalDirection.Right => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(step), step, $"Undefined {nameof(HorizontalDirection)}."),
        };
        int pixels = StridePhase(step.Pose) is { } phase ? StridePixels(phase) : 0;
        return new(sign * CoordinateAxis.AsSigned(EightEight.FromWhole(pixels)),
            new(CoordinateAxis.AsSigned(sign * SamusReach(step.Pose)), SamusYOffset(step.Pose)));
    }
}
