using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySpcSoundLibrary2Pointers()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SuperMetroid.AssetExtraction.SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "SPC stream oracle revision");
        Suite(nameof(VerifySpcSoundStream2), () => VerifySpcSoundStream2(rom));
    }

    private static void VerifySpcSoundStream1(ISnesAddressSpace rom) => VerifySpcSoundStreams(rom, 0, 66, 0xcf96f5);
    private static void VerifySpcSoundStream2(ISnesAddressSpace rom) => VerifySpcSoundStreams(rom, 1, 127, 0xcfa5bb);
    private static void VerifySpcSoundStream3(ISnesAddressSpace rom) => VerifySpcSoundStreams(rom, 2, 47, 0xcfba97);

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
