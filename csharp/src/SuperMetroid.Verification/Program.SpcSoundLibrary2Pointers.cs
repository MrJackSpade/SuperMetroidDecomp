using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    /// <summary>Loads the pinned retail ROM, confirms its revision hash and verifies SPC sound-library 2 stream pointers.</summary>
    private static void VerifySpcSoundLibrary2Pointers()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC stream oracle revision");
        Suite(nameof(VerifySpcSoundStream2), () => VerifySpcSoundStream2(rom));
    }

    /// <summary>Compares all 66 library-1 command pointers with the native table at bank-$CF:$96F5.</summary>
    /// <param name="rom">Address space containing the retail sound pointer table.</param>
    private static void VerifySpcSoundStream1(ISnesAddressSpace rom) => VerifySpcSoundStreams(rom, 0, 66, 0xcf96f5);

    /// <summary>Compares all 127 library-2 command pointers with the native table at bank-$CF:$A5BB.</summary>
    /// <param name="rom">Address space containing the retail sound pointer table.</param>
    private static void VerifySpcSoundStream2(ISnesAddressSpace rom) => VerifySpcSoundStreams(rom, 1, 127, 0xcfa5bb);

    /// <summary>Compares all 47 library-3 command pointers with the native table at bank-$CF:$BA97.</summary>
    /// <param name="rom">Address space containing the retail sound pointer table.</param>
    private static void VerifySpcSoundStream3(ISnesAddressSpace rom) => VerifySpcSoundStreams(rom, 2, 47, 0xcfba97);

    /// <summary>Checks a sound library's indexed pointers against ROM and verifies command bounds and zero-command behavior.</summary>
    /// <param name="rom">Address space used to read the native pointer table.</param>
    /// <param name="library">Zero-based sound-library index used by <see cref="SpcSoundEffectTables"/>.</param>
    /// <param name="count">Number of defined commands in the library.</param>
    /// <param name="source">Full SNES address of the first native command pointer.</param>
    private static void VerifySpcSoundStreams(ISnesAddressSpace rom, int library, int count, int source)
    {
        AssertEqual(count, SpcSoundEffectTables.CommandCount(library), "native sound command extent");
        for (int command = 1; command <= count; command++)
        {
            int address = source + (command - 1) * sizeof(ushort);
            ushort native = unchecked((ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
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
        zero.WritePort(AudioRomData.Apu.FirstSoundPort + library, 0);
        zero.GenerateFrame(pcm);
        AssertEqual((byte)0, zero.ReadPort(AudioRomData.Apu.FirstSoundPort + library),
            "zero sound command is a valid acknowledged no-sound sentinel");

        var invalidPlayer = new ManagedSpcPlayer();
        invalidPlayer.WritePort(AudioRomData.Apu.FirstSoundPort + library, (byte)(count + 1));
        AssertThrows<InvalidDataException>(() => invalidPlayer.GenerateFrame(pcm),
            "runtime rejects the first undefined sound command before stream dispatch");
    }
}
