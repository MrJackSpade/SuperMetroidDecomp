namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The motion limits of each physical slot of Crocomire's falling spike wall.</summary>
internal static class CrocomireSpikeMotion
{
    /// <summary>Native $86:91C3 accelerationDelta of <paramref name="slot"/>, in 1/65536 pixels/frame cubed.</summary>
    internal static int AccelerationDelta(int slot) => CrocomireSpikeMotionDefinitions.AccelerationDeltas[ValidateSlot(slot)];

    /// <summary>Native $86:91E7 maxAcceleration of <paramref name="slot"/>, in 1/65536 pixels/frame squared.</summary>
    internal static int MaximumAcceleration(int slot) =>
        WordWrap.Unsigned(AccelerationDelta(slot) << CrocomireSpikeMotionDefinitions.MaximumAccelerationShift);

    /// <summary>Native $86:920B maxVelocity low-byte view of <paramref name="slot"/>.</summary>
    internal static int MaximumVelocity(int slot) => CrocomireSpikeMotionDefinitions.MaximumVelocities[ValidateSlot(slot)];

    /// <summary>Rejects positions outside the eighteen physical spike slots shared by the motion tables.</summary>
    private static int ValidateSlot(int slot) =>
        (uint)slot < (uint)CrocomireSpikeMotionDefinitions.AccelerationDeltas.Length
            ? slot
            : throw new ArgumentOutOfRangeException(nameof(slot), slot, "Crocomire spike physical slot must be zero through seventeen.");
}
