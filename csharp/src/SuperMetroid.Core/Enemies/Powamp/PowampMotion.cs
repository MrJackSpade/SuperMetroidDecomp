using static SuperMetroid.Core.Enemies.Powamp.PowampMotionDefinitions;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>A Powamp's wiggle and balloon offsets and its spikes' acceleration.</summary>
public static class PowampMotion
{
    /// <summary>The wiggle offset at <paramref name="phase"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="phase"/> is outside the twelve phases.</exception>
    public static int WiggleOffset(int phase) =>
        (uint)phase < WigglePhaseCount ? WiggleOffsets[phase] : throw new InvalidDataException($"Invalid Powamp wiggle phase {phase}.");

    /// <summary>How far above the body the balloon sits in <paramref name="pose"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pose"/> is not one of the three poses.</exception>
    public static int BalloonHeight(PowampBalloonPose pose) =>
        BalloonHeights.TryGetValue(pose, out int height) ? height : throw new ArgumentOutOfRangeException(nameof(pose), pose, "Not a Powamp balloon pose.");

    /// <summary>The signed 1/256-pixel acceleration of a spike flying in <paramref name="direction"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is not one of the eight directions.</exception>
    public static EightEightVector<RoomPixels> SpikeAcceleration(CompassDirection direction) =>
        SpikeAccelerations.TryGetValue(direction, out EightEightVector<RoomPixels> acceleration)
            ? acceleration
            : throw new InvalidDataException($"Invalid Powamp spike direction {direction}.");
}
