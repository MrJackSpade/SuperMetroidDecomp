using SuperMetroid.Core.Audio;

internal static partial class Program
{
    private static void VerifySpcAllocationAddresses()
    {
        // Independent original kSomeAddrs and SpcPlayer member offsets from
        // upstream-sm 578f90b3cc49557bb70060ad033bb90b8cf8ac50/src/spc_player.c.
        ushort[,] original = { { 0x3a6, 0x3aa, 0x3af }, { 0x446, 0x448, 0x44b }, { 0x47e, 0x480, 0x483 } };
        for (int library = 0; library < 3; library++)
        for (int field = 0; field < 3; field++)
        {
            ushort actual = SpcSoundEffectTables.AllocationStateAddress(library, (SpcAllocationField)field);
            AssertEqual(original[library, field], actual, "original allocation field base");
            for (int channel = 0; channel < (library == 0 ? 4 : 2); channel++)
                AssertEqual(original[library, field] + channel, actual + channel, "serialized per-channel address");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => SpcSoundEffectTables.AllocationStateAddress(invalid, SpcAllocationField.VoiceBitset), "invalid library");
            AssertThrows<IndexOutOfRangeException>(() => SpcSoundEffectTables.AllocationStateAddress(0, (SpcAllocationField)invalid), "invalid field");
        }
    }
}
