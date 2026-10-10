using System.Reflection;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction;

internal static partial class Program
{
    /// <summary>Verifies native SPC FIR address calculation for all byte presets, including discarded carry, mutable RAM reads, and F7 operand consumption.</summary>
    private static void VerifySpcFirAddressing()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "FIR address oracle revision");
        byte Read(int spc) => rom.ReadByte(0xcf6c08 + spc);
        AssertEqual((byte)0x8d, Read(0x1a94), "native MOV Y immediate");
        AssertEqual((byte)0xcf, Read(0x1a96), "native MUL YA");
        AssertEqual((byte)0x5d, Read(0x1a97), "native MOV X A takes only product low byte");
        AssertEqual((byte)0xf5, Read(0x1a9a), "native MOV A absolute plus X");
        AssertEqual((byte)0x3d, Read(0x1aa0), "native INC X");
        int nativeBase = Read(0x1a9b) | Read(0x1a9c) << 8;
        var player = new ManagedSpcPlayer();
        var ram = (byte[])(typeof(ManagedSpcPlayer).GetField("ram", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(player)
            ?? throw new InvalidOperationException("Missing SPC RAM fixture access."));
        var method = typeof(ManagedSpcPlayer).GetMethod("HandleEffect", BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Missing real music-effect handler.");
        var effect = method.CreateDelegate<Action<ManagedSpcMusicChannel, byte>>(player);
        // Distinguish low-byte aliases from incorrectly carrying into later RAM pages.
        for (int address = nativeBase; address < nativeBase + 2048; address++)
            ram[address] = (byte)((address >> 8) ^ (address & 255));
        for (int preset = 32; preset < 288; preset++)
        {
            byte commandPreset = (byte)preset;
            ram[0x6000] = 2;
            ram[0x6001] = 10;
            ram[0x6002] = commandPreset;
            var channel = new ManagedSpcMusicChannel { PatternOrderPointer = 0x6000 };
            effect(channel, (byte)SpcMusicEffect.ConfigureEcho);
            // Execute the original MUL YA / MOV X,A semantics; discarded Y cannot carry.
            ushort ya = (ushort)(Read(0x1a95) * commandPreset);
            byte x = (byte)ya;
            for (int tap = 0; tap < 8; tap++, x++)
                AssertEqual(ram[nativeBase + x], player.ReadDspRegisterForVerification((byte)(0x0f + 16 * tap)),
                    $"native FIR addressing preset{commandPreset} tap{tap}");
            AssertEqual((ushort)0x6003, channel.PatternOrderPointer, "F7 consumes exactly three operands");
        }
        Console.WriteLine("SPC FIR: all256 byte presets preserve native low-byte indexing and mutable RAM reads.");
    }
}
