using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks compiled quadratic speed words and their Polyp, Beetom, Hopper, KiHunter, and Maw consumers.</summary>
internal static class EnemyQuadraticSpeedsVerification
{
    /// <summary>Compares native word windows and focused production integrations, including odd offsets and wrap.</summary>
    internal static void VerifyCompiledQuadraticEnemySpeeds(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        for (int offset = 0; offset <= 758; offset++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0cbc7 + offset), EnemyQuadraticSpeeds.ReadWord(offset), "quadratic projectile table mirror");
        for (int offset = 0; offset <= 758; offset++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + offset), EnemyQuadraticSpeeds.ReadWord(offset), "quadratic native word window");
        for (int offset = 0; offset <= 756; offset++)
            AssertEqual(unchecked((int)SixteenSixteen.Compose(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + (offset + 2)), CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + offset))),
                EnemyQuadraticSpeeds.ReadDisplacement(offset), "quadratic native displacement window");
        AssertThrows<InvalidDataException>(() => EnemyQuadraticSpeeds.ReadWord(-1), "quadratic negative offset");
        AssertThrows<InvalidDataException>(() => EnemyQuadraticSpeeds.ReadWord(759), "quadratic partial word");
        AssertThrows<InvalidDataException>(() => EnemyQuadraticSpeeds.ReadDisplacement(757), "quadratic partial displacement");
        if (definitionsOnly) return;
        var shared = new Func<int, bool, int>(EnemyMath.ReadQuadraticEnemySpeed);
        var addRock = new Action<RoomEnemyProjectileSlot, int, QuadraticSpeedHalf>(PolypBehavior.AddPolypRockQuadraticStep);
        var rock = new RoomEnemyProjectileSlot(0);
        foreach (uint origin in new uint[] { 0, 0x0080ff37, 0xffffff37 })
        for (ushort index = 0; index < 95; index++)
        foreach (bool negative in new[] { false, true })
        {
            int offset = index * 8 + (negative ? 4 : 0);
            uint expected = unchecked(origin + (SixteenSixteen.Compose(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0cbc7 + (offset + 2)), CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0cbc7 + offset))));
            rock.Position = rock.Position.WithY(new(FixedCoordinate.WholeOf(unchecked((int)origin)), unchecked((ushort)origin)));
            addRock(rock, index, negative ? QuadraticSpeedHalf.Negative : QuadraticSpeedHalf.Positive);
            AssertEqual(expected, rock.Position.Y.RawValue, "Polyp real split-word carry/wrap");
        }
        AssertThrows<InvalidDataException>(() => addRock(rock, 95, QuadraticSpeedHalf.Positive), "Polyp invalid speed remains loud");
        for (ushort index = 0; index < 95; index++)
        foreach (bool negative in new[] { false, true })
        {
            int offset = index * 8 + (negative ? 4 : 0);
            AssertEqual(unchecked((int)SixteenSixteen.Compose(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + (offset + 2)), CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + offset))), shared(index, negative),
                "quadratic shared production reader");
        }
        var beetom = new Func<int, int, int>(BeetomBehavior.CalculateInitialBeetomYSpeedIndex);
        var hopper = new Func<int, int, int>(HoppersBehavior.CalculateInitialHopperYSpeedTableIndex);
        int initialIndexCases = 0;
        foreach (ushort delta in new ushort[] { 1, 2, 4 })
        for (int height = 0; height <= ushort.MaxValue; height++)
        {
            ushort accumulated = 0;
            for (int index = delta; index < 95; index += delta)
            {
                accumulated = unchecked((ushort)(accumulated + CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + (index * 8 + 1))));
                if (unchecked((short)(accumulated - height)) < 0)
                    continue;
                AssertEqual(index, beetom((ushort)height, delta), "Beetom native odd-word hop integration");
                AssertEqual(index, hopper((ushort)height, delta), "Hopper native odd-word hop integration");
                initialIndexCases++;
                break;
            }
        }
        // Only report inputs that the native loop resolves inside the authored
        // table. Out-of-table instruction-byte reads are not a fabricated math API.
        AssertTrue(initialIndexCases > 100000, "wide signed-height hop corpus is exercised");

        var wings = new Func<int, QuadraticSpeedHalf, ushort>(KiHunterBehavior.ReadKiHunterQuadraticAngleDelta);
        for (int index = 0; index < 95; index++)
        for (int low = 0; low < 256; low++)
        foreach (bool negative in new[] { false, true })
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + (index * 8 + (negative ? 5 : 1))), wings((ushort)(index * 256 + low), negative ? QuadraticSpeedHalf.Negative : QuadraticSpeedHalf.Positive),
                "KiHunter high-byte index and odd-word angle delta");

        var addMaw = new Action<YappingMawEnemyState>(YappingMawBehavior.AddYappingMawQuadraticSpeed);
        var maw = new YappingMawEnemyState(new RoomEnemySystem(new EnemySession()).SlotTable.Slots[0]);
        int mawAdditions = 0;
        foreach (uint start in new uint[] { 0, 0x0080ff37, 0xffffff37 })
        for (int record = 0; record < EnemyQuadraticSpeedDefinitions.RecordCount; record++)
        foreach (bool negative in new[] { false, true })
        {
            int offset = record * EnemyQuadraticSpeedDefinitions.RecordSize + (negative ? 4 : 0);
            maw.Extension = SignedSixteenSixteen.FromRaw(unchecked((int)start));
            maw.QuadraticSpeedRecord = record;
            maw.QuadraticSpeedNegative = negative;
            mawAdditions++;
            uint expected = unchecked(start + (SixteenSixteen.Compose(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + (offset + 2)), CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + offset))));
            addMaw(maw);
            AssertEqual(expected, unchecked((uint)maw.Extension.RawValue), "Yapping Maw real split-word integration");
            AssertEqual(new VerifyCompiledQuadraticEnemySpeedsCase(record + 1, negative), new VerifyCompiledQuadraticEnemySpeedsCase(maw.QuadraticSpeedRecord, maw.QuadraticSpeedNegative), "Yapping Maw record advance");
        }
        Console.WriteLine("Compiled quadratic speeds: all 759 word windows and 757 displacement windows match the authored NTSC bytes, including odd offsets and native discontinuities.");
        Console.WriteLine("Projectile quadratic mirror: all 759 word windows and 570 real Polyp integrations match, including scratch writes and subpixel carry/wrap, without a loaded bus.");
        Console.WriteLine($"Quadratic production paths: {initialIndexCases} hop inputs per Beetom/Hopper, 48640 KiHunter angle inputs, and {mawAdditions} Yapping Maw additions match cartridge bytes without a loaded bus.");
    }
}
