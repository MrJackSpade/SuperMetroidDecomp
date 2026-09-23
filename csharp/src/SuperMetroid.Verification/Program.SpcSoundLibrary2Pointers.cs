using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySpcSoundLibrary2Pointers()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  SPC sound library 2 pointers: cartridge comparison skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        ushort[] pointers = SpcSoundEffectTables.StreamPointerTables[1];
        AssertEqual(127, pointers.Length, "sound library 2 has 127 authored command pointers");
        const int source = 0xcfa5bb;
        for (int index = 0; index < pointers.Length; index++)
        {
            int address = source + index * sizeof(ushort);
            ushort native = unchecked((ushort)(rom.ReadByte(address) |
                (rom.ReadByte(address + 1) << 8)));
            AssertEqual(native, pointers[index], $"sound library 2 command {index + 1:X2}");
        }

        short[] pcm = new short[SpcDriverData.HostStereoFramesPerVideoFrame * 2];
        var zero = new ManagedSpcPlayer();
        zero.WritePort(AudioRomData.Apu.FirstSoundPort + 1, 0);
        zero.GenerateFrame(pcm);
        AssertEqual((byte)0, zero.ReadPort(AudioRomData.Apu.FirstSoundPort + 1),
            "zero sound command is a valid acknowledged no-sound sentinel");

        var invalid = new ManagedSpcPlayer();
        invalid.WritePort(AudioRomData.Apu.FirstSoundPort + 1, 128);
        AssertThrows<InvalidDataException>(() => invalid.GenerateFrame(pcm),
            "library 2 command 128 is rejected before indexing the pointer table");
        Console.WriteLine("  SPC sound library 2: all 127 native pointers and both command boundaries pass.");
    }
}
