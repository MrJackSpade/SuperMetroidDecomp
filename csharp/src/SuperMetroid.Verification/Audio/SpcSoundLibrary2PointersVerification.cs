using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks SPC sound-effect stream pointer tables and invalid-command handling.</summary>
internal static class SpcSoundLibrary2PointersVerification
{
    /// <summary>Loads the supported retail revision and runs the second sound-library pointer audit.</summary>
    internal static void VerifySpcSoundLibrary2Pointers()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.Cartridge.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC stream oracle revision");
        Suite(nameof(VerifySpcSoundStream2), () => VerifySpcSoundStream2(rom));
    }

    /// <summary>Checks all 66 stream pointers in the first sound-effect library.</summary>
    internal static void VerifySpcSoundStream1(ISnesAddressSpace rom) => VerifySpcSoundStreams(rom, 0, 66, 0xcf96f5);
    /// <summary>Checks all 127 stream pointers in the second sound-effect library.</summary>
    internal static void VerifySpcSoundStream2(ISnesAddressSpace rom) => VerifySpcSoundStreams(rom, 1, 127, 0xcfa5bb);
    /// <summary>Checks all 47 stream pointers in the third sound-effect library.</summary>
    internal static void VerifySpcSoundStream3(ISnesAddressSpace rom) => VerifySpcSoundStreams(rom, 2, 47, 0xcfba97);

    /// <summary>Compares native pointers and command bounds, then checks driver acknowledgement and rejection timing.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for SPC sound streams")]
    private static void VerifySpcSoundStreams(ISnesAddressSpace rom, int library, int count, int source)
    {
        AssertEqual(count, SpcSoundEffectTables.CommandCount(library), "native sound command extent");
        for (int command = 1; command <= count; command++)
        {
            int address = source + (command - 1) * sizeof(ushort);
            ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(address);
            AssertEqual(native, SpcSoundEffectTables.StreamPointer(library, command), "original sound stream dispatch");
        }
        foreach (int invalid in new[] { int.MinValue, -1, 0, count + 1, 256, int.MaxValue })
            AssertThrows<IndexOutOfRangeException>(() => SpcSoundEffectTables.StreamPointer(library, invalid), "unsupported stream command");
        foreach (int invalid in new[] { int.MinValue, -1, 3, int.MaxValue })
        {
            AssertThrows<IndexOutOfRangeException>(() => SpcSoundEffectTables.StreamPointer(invalid, 1), "unsupported stream library");
            AssertThrows<IndexOutOfRangeException>(() => SpcSoundEffectTables.CommandCount(invalid), "unsupported command-count library");
        }

        short[] pcm = new short[SpcDriverData.HostStereoFramesPerVideoFrame * 2];
        var zero = new ManagedSpcPlayer();
        zero.WritePort(ApuPorts.ForSoundLibrary(library), 0);
        zero.GenerateFrame(pcm);
        AssertEqual((byte)0, zero.ReadPort(ApuPorts.ForSoundLibrary(library)),
            "zero sound command is a valid acknowledged no-sound sentinel");

        // #1269: the driver reads a request on one service and dispatches it on the next, so
        // the undefined command is rejected on the second frame, before its stream dispatch.
        var invalidPlayer = new ManagedSpcPlayer();
        invalidPlayer.WritePort(ApuPorts.ForSoundLibrary(library), (byte)(count + 1));
        invalidPlayer.GenerateFrame(pcm);
        AssertThrows<InvalidDataException>(() => invalidPlayer.GenerateFrame(pcm),
            "runtime rejects the first undefined sound command before stream dispatch");
    }

    /// <summary>Runs the focused retail sound-library pointer verification command.</summary>
    [VerificationCommand("--spc-sound-library-2-pointers")]
    internal static int SpcSoundLibrary2PointersCommand()
    {
        Suite(nameof(VerifySpcSoundLibrary2Pointers), () => VerifySpcSoundLibrary2Pointers());
        return 0;
    }
}
