using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Drawing;

/// <summary>Which splash or footstep effect Samus's movement and room produce.</summary>
internal static partial class SamusAtmosphericEffects
{
    /// <summary>
    /// The splash <paramref name="movementType"/> raises entering water (<see cref="SamusMovementTypeDefinitions"/>):
    /// grounded postures produce the paired surface splash; other movement dives.
    /// </summary>
    internal static WaterSplashKind WaterSplashFor(SamusMovementType movementType) =>
        SamusMovementTypeDefinitions.Movements.TryGetValue(movementType, out SamusMovementTypeDefinition? definition)
            ? definition.Splash
            : throw new InvalidDataException($"Water-splash movement type {movementType} is outside the movement types.");

    /// <summary>
    /// Whether <paramref name="strideFrame"/>, a frame of the run's stride, is one of its two foot contacts
    /// (<see cref="SamusAnimationFrames.RunFirstFootContact"/>, <see cref="SamusAnimationFrames.RunSecondFootContact"/>).
    /// </summary>
    /// <exception cref="InvalidDataException">The frame is not one of the stride's pictures.</exception>
    internal static bool IsRunningFootContact(SamusAnimationFrame strideFrame)
    {
        if (strideFrame.Segment != SamusAnimationSegmentName.Run ||
            strideFrame.Frame < 0 ||
            strideFrame.Frame >= SamusAnimationDelayDefinitions.RunningAnimation.Segment(SamusAnimationSegmentName.Run).Delays.Count)
            throw new InvalidDataException($"Running animation frame {strideFrame} is outside the run's stride.");
        return strideFrame == SamusAnimationFrames.RunFirstFootContact || strideFrame == SamusAnimationFrames.RunSecondFootContact;
    }
}
