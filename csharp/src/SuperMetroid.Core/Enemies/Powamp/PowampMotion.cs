using static SuperMetroid.Core.Enemies.Powamp.PowampMotionDefinitions;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>A Powamp's wiggle and balloon offsets and its spikes' acceleration.</summary>
public static class PowampMotion
{
    /// <summary>The wiggle offset at <paramref name="phase"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="phase"/> is outside the twelve phases.</exception>
    public static int WiggleOffset(int phase) =>
        (uint)phase < WigglePhaseCount ? WiggleOffsets[phase] : throw new InvalidDataException($"Invalid Powamp wiggle phase {phase}.");

    /// <summary>The balloon's Y offset at inflation pose <paramref name="pose"/> while rising or sinking.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pose"/> is outside the three poses.</exception>
    public static int BalloonOffset(int pose, bool sinking)
    {
        int[] offsets = sinking ? SinkingBalloonOffsets : RisingBalloonOffsets;
        return (uint)pose < (uint)offsets.Length ? offsets[pose] : throw new ArgumentOutOfRangeException(nameof(pose));
    }

    /// <summary>A spike's signed 8.8 acceleration in compass <paramref name="direction"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is outside the eight directions.</exception>
    public static EightEightVector<RoomPixels> SpikeAcceleration(int direction) =>
        (uint)direction < SpikeDirectionCount ? SpikeAccelerations[direction] : throw new InvalidDataException($"Invalid Powamp spike direction {direction}.");
}
