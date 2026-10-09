using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Compares each physical spike slot's acceleration increment with the native $86:91C3 table
    /// and confirms that slots outside the eighteen-entry table are rejected.
    /// </summary>
    /// <param name="rom">Address space containing the native Crocomire spike motion table.</param>
    private static void VerifyCrocomireSpikeAccelerationDelta(SuperMetroidAddressSpace rom)
    {
        for (int slot = 0; slot < 18; slot++)
        {
            int address = 0x8691c3 + 2 * slot;
            ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(expected, CrocomireSpikeMotionDefinitions.AccelerationDelta(slot),
                $"Crocomire spike acceleration increment, physical slot {slot}");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.AccelerationDelta(-1), "negative increment slot");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.AccelerationDelta(18), "increment slot past end");
    }

    /// <summary>
    /// Compares each physical spike slot's acceleration limit with the native $86:91E7 table
    /// and confirms that slots outside the eighteen-entry table are rejected.
    /// </summary>
    /// <param name="rom">Address space containing the native Crocomire spike motion table.</param>
    private static void VerifyCrocomireSpikeMaximumAcceleration(SuperMetroidAddressSpace rom)
    {
        for (int slot = 0; slot < 18; slot++)
        {
            int address = 0x8691e7 + 2 * slot;
            ushort expected = (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
            AssertEqual(expected, CrocomireSpikeMotionDefinitions.MaximumAcceleration(slot),
                $"Crocomire spike acceleration limit, physical slot {slot}");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.MaximumAcceleration(-1), "negative acceleration limit slot");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.MaximumAcceleration(18), "acceleration limit slot past end");
    }

    /// <summary>
    /// Compares each physical spike slot's maximum-velocity low byte with the native $86:920B
    /// table and confirms that slots outside the eighteen-entry table are rejected.
    /// </summary>
    /// <param name="rom">Address space containing the native Crocomire spike motion table.</param>
    private static void VerifyCrocomireSpikeMaximumVelocity(SuperMetroidAddressSpace rom)
    {
        for (int slot = 0; slot < 18; slot++)
            AssertEqual(rom.ReadByte(0x86920b + 2 * slot), CrocomireSpikeMotionDefinitions.MaximumVelocity(slot),
                $"Crocomire spike velocity limit low byte, physical slot {slot}");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.MaximumVelocity(-1), "negative velocity limit slot");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireSpikeMotionDefinitions.MaximumVelocity(18), "velocity limit slot past end");
    }
}
