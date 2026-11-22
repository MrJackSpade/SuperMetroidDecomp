using static SuperMetroid.Core.Enemies.Powamp.PowampMotionDefinitions;

namespace SuperMetroid.Core.Enemies.Powamp;

/// <summary>A Powamp's balloon offsets and its spikes' acceleration.</summary>
internal static partial class PowampMotion
{
    /// <summary>How far above the body the balloon sits in <paramref name="pose"/>.</summary>
    internal static int BalloonHeight(PowampBalloonPose pose) =>
        BalloonHeights[pose];

    /// <summary>The acceleration of a spike flying in <paramref name="direction"/>.</summary>
    internal static PixelVelocity<RoomPixels> SpikeAcceleration(CompassDirection direction) =>
        SpikeAccelerations[direction];
}
