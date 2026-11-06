using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks compiled Alcoon and NamiFune fireball launch velocities against native words.</summary>
internal static class EnemyFireballLaunchDefinitionsVerification
{
    /// <summary>Checks Alcoon's upward, level and downward launches against the three authored vertical velocities.</summary>
    private static void VerifyAlcoonVerticalLaunchMapping(SuperMetroidAddressSpace rom)
    {
        AlcoonFireballLaunch[] launches =
            [AlcoonFireballLaunchDefinitions.Upward, AlcoonFireballLaunchDefinitions.Level, AlcoonFireballLaunchDefinitions.Downward];
        for (int record = 0; record < launches.Length; record++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x869ef9 + record * 2),
                launches[record].YVelocity.RawValue, "Alcoon launch vertical velocity");
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
        Console.WriteLine("Enemy fireball launches: three independent mappings, 19 native words, 2048 valid parameter aliases and invalid NamiFune selectors pass.");
    }
}
