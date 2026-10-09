using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Compares Alcoon's three vertical launch velocities with the ROM table and checks rejected selectors.</summary>
    /// <param name="rom">Retail ROM supplying the independent launch-table values.</param>
    private static void VerifyAlcoonVerticalLaunchMapping(SuperMetroidAddressSpace rom)
    {
        for (ushort offset = 0; offset <= 4; offset += 2)
            AssertEqual(FireballLaunchOracleWord(rom, 0x869ef9 + offset),
                EnemyFireballLaunchDefinitions.AlcoonYVelocity(offset), "Alcoon native signed8.8 launch");
        foreach (ushort offset in new ushort[] { 1, 3, 5, 6, 0x100, 0x102, 0x104, 0xffff })
            AssertThrows<InvalidDataException>(() => EnemyFireballLaunchDefinitions.AlcoonYVelocity(offset),
                "Alcoon rejects odd, out-of-range and high-byte selectors");
    }

    /// <summary>Runs the NamiFune launch-field checks for the left-facing horizontal velocity.</summary>
    /// <param name="rom">Retail ROM used as the reference for the encoded velocity words.</param>
    private static void VerifyNamiFuneLeftLaunchMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNamiFuneLaunchField), () => VerifyNamiFuneLaunchField(rom, left: true));

    /// <summary>Runs the NamiFune launch-field checks for the right-facing horizontal velocity.</summary>
    /// <param name="rom">Retail ROM used as the reference for the encoded velocity words.</param>
    private static void VerifyNamiFuneRightLaunchMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNamiFuneLaunchField), () => VerifyNamiFuneLaunchField(rom, left: false));

    /// <summary>Checks all NamiFune low-byte selectors and their high-byte aliases against the directional ROM fields.</summary>
    /// <param name="rom">Retail ROM supplying the expected left and right velocity words.</param>
    /// <param name="left">Selects the left-facing field when <see langword="true"/>; otherwise selects the right-facing field.</param>
    private static void VerifyNamiFuneLaunchField(SuperMetroidAddressSpace rom, bool left)
    {
        for (int high = 0; high < 256; high++)
        {
            for (int index = 0; index < 8; index++)
            {
                var pair = EnemyFireballLaunchDefinitions.NamiFuneVelocities((ushort)((high << 8) | index));
                ushort expected = FireballLaunchOracleWord(rom, (left ? 0x86deb6 : 0x86deb8) + index * 4);
                AssertEqual(expected, left ? pair.Left : pair.Right,
                    left ? "NamiFune native negative horizontal speed and high-byte aliases" : "NamiFune native positive horizontal speed and high-byte aliases");
            }
            foreach (int index in new[] { 8, 9, 127, 255 })
                AssertThrows<InvalidDataException>(() => EnemyFireballLaunchDefinitions.NamiFuneVelocities((ushort)((high << 8) | index)),
                    "NamiFune rejects invalid low-byte selectors regardless of high byte");
        }
    }

    /// <summary>Reads a little-endian velocity word from the cartridge's launch table.</summary>
    /// <param name="rom">ROM address space containing the reference bytes.</param>
    /// <param name="address">Address of the low-order byte.</param>
    /// <returns>The decoded 16-bit table word.</returns>
    private static ushort FireballLaunchOracleWord(SuperMetroidAddressSpace rom, int address) =>
        (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);

    /// <summary>Runs the independent Alcoon and NamiFune launch-mapping checks and reports their combined coverage.</summary>
    /// <param name="rom">Retail ROM used to derive expected launch values.</param>
    private static void VerifyCompiledEnemyFireballLaunches(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyAlcoonVerticalLaunchMapping), () => VerifyAlcoonVerticalLaunchMapping(rom));
        Suite(nameof(VerifyNamiFuneLeftLaunchMapping), () => VerifyNamiFuneLeftLaunchMapping(rom));
        Suite(nameof(VerifyNamiFuneRightLaunchMapping), () => VerifyNamiFuneRightLaunchMapping(rom));
        Console.WriteLine("Enemy fireball launches: three independent mappings, 19 native words, 2048 valid parameter aliases and invalid selectors pass.");
    }
}
