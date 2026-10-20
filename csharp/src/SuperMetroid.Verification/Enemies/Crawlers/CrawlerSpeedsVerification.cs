using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Yard;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Verification.Enemies.Crawlers;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks compiled crawler and Yard velocity definitions against
/// native speed tables and production sign application.</summary>
internal static class CrawlerSpeedsVerification
{
    /// <summary>Compares all authored speeds, preserve-sentinel behavior,
    /// and Yard direction transforms across the effective ushort input domain.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled crawler speeds")]
    internal static void VerifyCompiledCrawlerSpeeds(SuperMetroidAddressSpace rom)
    {
        var reset = new Func<RoomEnemySlot, int, EightEightVector<RoomPixels>, EightEightVector<RoomPixels>>(
            CrawlersBehavior.ResetCrawlerVelocitiesFromProperties);
        var setYard = new Action<YardEnemyState, int>(YardEnemyBehavior.SetYardCrawlingVelocities);
        var slot = new RoomEnemySystem(new EnemySession()).SlotTable.Slots[0];
        var yard = new YardEnemyState();
        int speedParameter = 0;
        for (ushort parameter = 0; parameter < 32; parameter++)
        {
            ushort expected = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3e5f0 + parameter * 2));
            AssertEqual(expected, CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa3cca2 + parameter * 2)), "Yard/crawler native speed mirror");
            AssertEqual(expected, CrawlerSpeedDefinitions.ForParameter(parameter), "Compiled crawling speed");
            speedParameter = parameter;
            yard.CrawlingSpeedIndex = parameter;
            for (ushort property = 0; property < 4; property++)
            {
                slot.Properties = EnemyPropertyWord.FromNativeWord((ushort)(0xa000 | property));
                EightEightVector<RoomPixels> velocity = reset(slot, speedParameter, new(new SnesSignedEightEight(123), new SnesSignedEightEight(456)));
                AssertEqual(property == 0 ? unchecked((ushort)-expected) : expected, velocity.X.RawValue, "Real crawler X reset without bus");
                AssertEqual(property == 2 ? unchecked((ushort)-expected) : expected, velocity.Y.RawValue, "Real crawler Y reset without bus");
            }
            for (ushort direction = 0; direction < 8; direction++)
            {
                setYard(yard, direction);
                int address = 0xa3cd82 + direction * 8;
                AssertEqual(unchecked((ushort)((expected ^ CartridgeImportSource.Require(rom).ReadCartridgeWord(address)) + CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2)))), yard.CrawlingVelocity.X.RawValue, "Real Yard X reset without bus");
                AssertEqual(unchecked((ushort)((expected ^ CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4))) + CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6)))), yard.CrawlingVelocity.Y.RawValue, "Real Yard Y reset without bus");
            }
        }
        // The preserve sentinel skips the table, not the subsequent native sign application.
        speedParameter = CrawlerSpeedDefinitions.PreserveVelocity;
        for (int magnitude = 0; magnitude <= ushort.MaxValue; magnitude++)
        {
            for (ushort property = 0; property < 4; property++)
            {
                slot.Properties = EnemyPropertyWord.FromNativeWord(property);
                EightEightVector<RoomPixels> velocity = reset(slot, speedParameter,
                    new(new SnesSignedEightEight((ushort)magnitude), new SnesSignedEightEight((ushort)~magnitude)));
                AssertEqual(property == 0 ? unchecked((ushort)-magnitude) : (ushort)magnitude, velocity.X.RawValue, "Crawler sentinel X sign");
                AssertEqual(property == 2 ? unchecked((ushort)-(ushort)~magnitude) : (ushort)~magnitude, velocity.Y.RawValue, "Crawler sentinel Y sign");
            }
            for (ushort direction = 0; direction < 8; direction++)
            {
                int address = 0xa3cd82 + direction * 8;
                var actual = YardVelocityDefinitions.Apply((ushort)magnitude, direction);
                AssertEqual(unchecked((ushort)((magnitude ^ CartridgeImportSource.Require(rom).ReadCartridgeWord(address)) + CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2)))), actual.X.RawValue, "Yard exhaustive native X sign");
                AssertEqual(unchecked((ushort)((magnitude ^ CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4))) + CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6)))), actual.Y.RawValue, "Yard exhaustive native Y sign");
            }
        }
        AssertThrows<InvalidDataException>(() => CrawlerSpeedDefinitions.ForParameter(32), "Crawler invalid speed");
        AssertThrows<InvalidDataException>(() => YardVelocityDefinitions.Apply(1, 8), "Yard invalid direction");
        Console.WriteLine("Compiled crawling speeds: both 32-word copies, 384 real resets, 262144 sentinel resets and 524288 Yard sign pairs match native values without a bus.");
    }

    /// <summary>Runs the focused compiled crawler speed check.</summary>
    [VerificationCommand("--compiled-crawler-speeds")]
    internal static int CompiledCrawlerSpeedsCommand()
    {
        SuiteTimingVerification.Suite(nameof(VerifyCompiledCrawlerSpeeds), () => VerifyCompiledCrawlerSpeeds(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
