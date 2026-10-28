using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks compiled Alcoon and NamiFune fireball launch velocities against native words.</summary>
internal static class EnemyFireballLaunchDefinitionsVerification
{
    /// <summary>Checks Alcoon's three signed launch words and rejects odd or out-of-range offsets.</summary>
    private static void VerifyAlcoonVerticalLaunchMapping(SuperMetroidAddressSpace rom)
    {
        for (ushort offset = 0; offset <= 4; offset += 2)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0x869ef9 + offset)),
                EnemyFireballLaunches.AlcoonYVelocity(offset), "Alcoon native signed8.8 launch");
        foreach (ushort offset in new ushort[] { 1, 3, 5, 6, 0x100, 0x102, 0x104, 0xffff })
            AssertThrows<InvalidDataException>(() => EnemyFireballLaunches.AlcoonYVelocity(offset),
                "Alcoon rejects odd, out-of-range and high-byte selectors");
    }

    /// <summary>Checks NamiFune's leftward velocity member across all high-byte aliases.</summary>
    private static void VerifyNamiFuneLeftLaunchMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNamiFuneLaunchField), () => VerifyNamiFuneLaunchField(rom, left: true));

    /// <summary>Checks NamiFune's rightward velocity member across all high-byte aliases.</summary>
    private static void VerifyNamiFuneRightLaunchMapping(SuperMetroidAddressSpace rom) =>
        Suite(nameof(VerifyNamiFuneLaunchField), () => VerifyNamiFuneLaunchField(rom, left: false));

    /// <summary>Compares eight indexed velocity pairs with native words and rejects invalid low-byte selectors.</summary>
    private static void VerifyNamiFuneLaunchField(SuperMetroidAddressSpace rom, bool left)
    {
        for (int high = 0; high < 256; high++)
        {
            for (int index = 0; index < 8; index++)
            {
                var pair = EnemyFireballLaunches.NamiFuneVelocities((ushort)((high << 8) | index));
                ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord(((left ? 0x86deb6 : 0x86deb8) + index * 4));
                AssertEqual(expected, left ? pair.Left : pair.Right,
                    left ? "NamiFune native negative horizontal speed and high-byte aliases" : "NamiFune native positive horizontal speed and high-byte aliases");
            }
            foreach (int index in new[] { 8, 9, 127, 255 })
                AssertThrows<InvalidDataException>(() => EnemyFireballLaunches.NamiFuneVelocities((ushort)((high << 8) | index)),
                    "NamiFune rejects invalid low-byte selectors regardless of high byte");
        }
    }

    /// <summary>Runs the independent Alcoon and NamiFune launch mapping checks.</summary>
    internal static void VerifyCompiledEnemyFireballLaunches(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyAlcoonVerticalLaunchMapping), () => VerifyAlcoonVerticalLaunchMapping(rom));
        Suite(nameof(VerifyNamiFuneLeftLaunchMapping), () => VerifyNamiFuneLeftLaunchMapping(rom));
        Suite(nameof(VerifyNamiFuneRightLaunchMapping), () => VerifyNamiFuneRightLaunchMapping(rom));
        Console.WriteLine("Enemy fireball launches: three independent mappings, 19 native words, 2048 valid parameter aliases and invalid selectors pass.");
    }
}
