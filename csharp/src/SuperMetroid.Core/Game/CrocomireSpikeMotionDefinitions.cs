namespace SuperMetroid.Core.Game;

/// <summary>Physical-slot motion profiles for Crocomire's falling spike wall.</summary>
internal static class CrocomireSpikeMotionDefinitions
{
    /// <summary>
    /// Native $86:91C3 accelerationDelta, in 1/65536 pixels/frame cubed.
    /// Slots 0 and 1 are stationary; slots 2..9 and 10..17 repeat a descending
    /// eight-step ramp of $0220, saturated at $0FF0. The peak is deliberately
    /// capped rather than extrapolated to $1100. All eighteen words agree with
    /// the pinned NTSC J/U v1.0 definition.
    /// </summary>
    internal static ushort AccelerationDelta(int slot)
    {
        ValidateSlot(slot);
        if (slot < 2)
            return 0;
        int remainingSteps = 8 - ((slot - 2) % 8);
        return (ushort)Math.Min(0x0ff0, remainingSteps * 0x0220);
    }

    /// <summary>
    /// Native $86:91E7 maxAcceleration, in 1/65536 pixels/frame squared.
    /// Each of the eighteen limits is sixteen times its acceleration increment;
    /// the saturated increment keeps the largest limit at $FF00 without wrap.
    /// </summary>
    internal static ushort MaximumAcceleration(int slot) =>
        (ushort)(AccelerationDelta(slot) << 4);

    /// <summary>
    /// Native $86:920B maxVelocity low-byte view, selected by physical slot 0..17.
    /// Slots 0..1 are stationary. Each eight-slot group descends one unit per
    /// pair; the second group's first four slots use the larger single-slot
    /// ramp 6,5,4,3. Equivalently that group takes the maximum of the two ramps.
    /// Native $86:913F/$9144 reads only the low byte; no high-byte view is used.
    /// </summary>
    internal static byte MaximumVelocity(int slot)
    {
        ValidateSlot(slot);
        if (slot < 2)
            return 0;
        int groupSlot = (slot - 2) % 8;
        int pairedRamp = 4 - groupSlot / 2;
        return (byte)(slot < 10 ? pairedRamp : Math.Max(pairedRamp, 6 - groupSlot));
    }

    private static void ValidateSlot(int slot)
    {
        if ((uint)slot >= 18)
            throw new IndexOutOfRangeException("Crocomire spike physical slot must be zero through seventeen.");
    }
}
