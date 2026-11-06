namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The motion limits of each physical slot of Crocomire's falling spike wall.</summary>
internal static class CrocomireSpikeMotion
{
    /// <summary>The acceleration delta of <paramref name="slot"/>, in 1/65536 pixels/frame cubed.</summary>
    internal static int AccelerationDelta(int slot) => CrocomireSpikeMotionDefinitions.AccelerationDeltas[ValidateSlot(slot)];

    /// <summary>The maximum acceleration of <paramref name="slot"/>, in 1/65536 pixels/frame squared.</summary>
    internal static int MaximumAcceleration(int slot) =>
        AccelerationDelta(slot) << CrocomireSpikeMotionDefinitions.MaximumAccelerationShift;

    /// <summary>The fastest whole-pixel horizontal speed of <paramref name="slot"/>, in pixels per frame.</summary>
    internal static int MaximumVelocity(int slot) => CrocomireSpikeMotionDefinitions.MaximumVelocities[ValidateSlot(slot)];

    /// <summary>Rejects positions outside the eighteen physical spike slots shared by the motion tables.</summary>
    private static int ValidateSlot(int slot) =>
        (uint)slot < (uint)CrocomireSpikeMotionDefinitions.AccelerationDeltas.Length
            ? slot
            : throw new ArgumentOutOfRangeException(nameof(slot), slot, "Crocomire spike physical slot must be zero through seventeen.");
}
