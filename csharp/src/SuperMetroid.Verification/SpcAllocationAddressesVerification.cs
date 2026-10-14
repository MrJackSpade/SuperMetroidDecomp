using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Audio;

/// <summary>Checks SPC sound effect allocation offsets against the pinned upstream player layout.</summary>
internal static class SpcAllocationAddressesVerification
{
    /// <summary>Verifies each library's field base, per-channel serialization, and rejection of invalid selectors.</summary>
    internal static void VerifySpcAllocationAddresses()
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

    /// <summary>Runs the focused allocation-address verification from the command line.</summary>
    [VerificationCommand("--lookup-spc-allocation-addresses")]
    internal static int LookupSpcAllocationAddressesCommand()
    {
        Suite(nameof(VerifySpcAllocationAddresses), () => VerifySpcAllocationAddresses());
        Console.WriteLine("SPC allocation layout: all original field bases, channel addresses and rejected indices pass.");
        return 0;
    }
}
