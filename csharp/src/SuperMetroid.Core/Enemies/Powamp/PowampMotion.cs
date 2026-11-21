using static SuperMetroid.Core.Enemies.Powamp.PowampMotionDefinitions;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>A Powamp's balloon offsets and its spikes' acceleration.</summary>
internal static class PowampMotion
{
    /// <summary>How far above the body the balloon sits in <paramref name="pose"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="pose"/> is not one of the three poses.</exception>
    internal static int BalloonHeight(PowampBalloonPose pose) =>
        BalloonHeights[pose];

    /// <summary>The acceleration of a spike flying in <paramref name="direction"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="direction"/> is not one of the eight directions.</exception>
    internal static PixelVelocity<RoomPixels> SpikeAcceleration(CompassDirection direction) =>
        SpikeAccelerations[direction];
}
