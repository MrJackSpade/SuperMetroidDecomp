namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The motion limits of each piece of Crocomire's falling spike wall, by its place from the top piece.</summary>
internal static class CrocomireSpikeMotion
{
    /// <summary>The acceleration delta of the piece at <paramref name="piece"/>, in 1/65536 pixels/frame cubed.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="piece"/> is not one of the wall's pieces.</exception>
    internal static int AccelerationDelta(int piece) => Ensure.Element(CrocomireSpikeMotionDefinitions.AccelerationDeltas, piece);

    /// <summary>The maximum acceleration of the piece at <paramref name="piece"/>, in 1/65536 pixels/frame squared.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="piece"/> is not one of the wall's pieces.</exception>
    internal static int MaximumAcceleration(int piece) =>
        AccelerationDelta(piece) * CrocomireSpikeMotionDefinitions.MaximumAccelerationPerIncrement;

    /// <summary>The fastest whole-pixel horizontal speed of the piece at <paramref name="piece"/>, in pixels per frame.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="piece"/> is not one of the wall's pieces.</exception>
    internal static int MaximumVelocity(int piece) => Ensure.Element(CrocomireSpikeMotionDefinitions.MaximumVelocities, piece);
}
