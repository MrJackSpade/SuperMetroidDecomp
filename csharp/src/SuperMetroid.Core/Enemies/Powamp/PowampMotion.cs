using static SuperMetroid.Core.Enemies.Powamp.PowampMotionDefinitions;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>A Powamp's wiggle and balloon offsets and its spikes' acceleration.</summary>
public static class PowampMotion
{
    /// <summary>The wiggle offset at <paramref name="phase"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="phase"/> is outside the twelve phases.</exception>
    public static short WiggleOffset(int phase) =>
        (uint)phase < WigglePhaseCount ? WiggleOffsets[phase] : throw new InvalidDataException($"Invalid Powamp wiggle phase {phase}.");

    /// <summary>The balloon's Y offset at inflation pose <paramref name="pose"/> while rising or sinking.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pose"/> is outside the three poses.</exception>
    public static short BalloonOffset(int pose, bool sinking)
    {
        short[] offsets = sinking ? SinkingBalloonOffsets : RisingBalloonOffsets;
        return (uint)pose < (uint)offsets.Length ? offsets[pose] : throw new ArgumentOutOfRangeException(nameof(pose));
    }

    /// <summary>A spike's signed 8.8 X acceleration in compass <paramref name="direction"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is outside the eight directions.</exception>
    public static short SpikeXAcceleration(int direction) =>
        (uint)direction < SpikeDirectionCount ? SpikeAccelerations[direction].X.SignedRawValue : throw new InvalidDataException($"Invalid Powamp spike direction {direction}.");

    /// <summary>A spike's signed 8.8 Y acceleration in compass <paramref name="direction"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is outside the eight directions.</exception>
    public static short SpikeYAcceleration(int direction) =>
        (uint)direction < SpikeDirectionCount ? SpikeAccelerations[direction].Y.SignedRawValue : throw new InvalidDataException($"Invalid Powamp spike direction {direction}.");
}
