using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Audio.Synthesis;
namespace SuperMetroid.Verification.Audio;
using SuperMetroid.Verification.Harness;

/// <summary>Checks native SPC FIR preset indexing through the production music-effect handler.</summary>
internal static class SpcFirAddressingVerification
{
    /// <summary>Confirms MUL YA drops the high byte before indexing eight FIR taps, so all 256 byte presets alias only within the selected RAM page.</summary>
    [DspRegisterBoundary("exact register test: checks S-DSP register behaviour")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for SPC FIR addressing")]
    [SpcMemoryBoundary("exact APU memory test: fills APU RAM around the echo FIR preset table")]
    internal static void VerifySpcFirAddressing()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        AssertEqual(SupportedCartridge.Sha256.ToUpperInvariant(),
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(rom.Rom)), "FIR address oracle revision");
        byte Read(int spc) => rom.ReadCartridgeByte(0xcf6c08 + spc);
        AssertEqual((byte)0x8d, Read(0x1a94), "native MOV Y immediate");
        AssertEqual((byte)0xcf, Read(0x1a96), "native MUL YA");
        AssertEqual((byte)0x5d, Read(0x1a97), "native MOV X A takes only product low byte");
        AssertEqual((byte)0xf5, Read(0x1a9a), "native MOV A absolute plus X");
        AssertEqual((byte)0x3d, Read(0x1aa0), "native INC X");
        int nativeBase = SnesLittleEndian.Word(Read(0x1a9b), Read(0x1a9c));
        var player = new ManagedSpcPlayer();
        byte[] ram = player.ram;
        Action<ManagedSpcMusicChannel, byte> effect = player.Music.HandleEffect;
        // Distinguish low-byte aliases from incorrectly carrying into later RAM pages.
        for (int address = nativeBase; address < nativeBase + 2048; address++)
            ram[address] = (byte)(SnesLittleEndian.HighByte(unchecked((ushort)address)) ^ unchecked((byte)address));
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
            AssertEqual(0x6003, channel.PatternOrderPointer, "F7 consumes exactly three operands");
        }
        Console.WriteLine("SPC FIR: all256 byte presets preserve native low-byte indexing and mutable RAM reads.");
    }

    /// <summary>Runs the focused SPC FIR low-byte address calculation check.</summary>
    [VerificationCommand("--lookup-spc-fir-addressing")]
    internal static int LookupSpcFirAddressingCommand()
    {
        Suite(nameof(VerifySpcFirAddressing), () => VerifySpcFirAddressing());
        return 0;
    }
}
