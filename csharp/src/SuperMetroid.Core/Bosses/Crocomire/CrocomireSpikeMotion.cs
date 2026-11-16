namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The motion limits of each piece of Crocomire's falling spike wall, by its place from the top piece.</summary>
internal static class CrocomireSpikeMotion
{
    /// <summary>How much the sideways acceleration of the piece at <paramref name="piece"/> grows each update.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="piece"/> is not one of the wall's pieces.</exception>
    internal static SignedSixteenSixteen AccelerationDelta(int piece) => Ensure.Element(CrocomireSpikeMotionDefinitions.AccelerationDeltas, piece);

    /// <summary>The maximum sideways acceleration of the piece at <paramref name="piece"/>, in pixels per update per update.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="piece"/> is not one of the wall's pieces.</exception>
    internal static SignedSixteenSixteen MaximumAcceleration(int piece) =>
        AccelerationDelta(piece) * CrocomireSpikeMotionDefinitions.MaximumAccelerationPerIncrement;

    /// <summary>The fastest whole-pixel horizontal speed of the piece at <paramref name="piece"/>, in pixels per frame.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="piece"/> is not one of the wall's pieces.</exception>
    internal static int MaximumVelocity(int piece) => Ensure.Element(CrocomireSpikeMotionDefinitions.MaximumVelocities, piece);
}
