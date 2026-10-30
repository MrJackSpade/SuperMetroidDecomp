using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks the quadratic speed rows and their Polyp, Beetom, Hopper, KiHunter, and Maw consumers.</summary>
internal static class EnemyQuadraticSpeedsVerification
{
    /// <summary>Compares every row and its coarse speeds with the cartridge, then focused production integrations.</summary>
    internal static void VerifyCompiledQuadraticEnemySpeeds(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        EnemySpeedSign[] signs = [EnemySpeedSign.Positive, EnemySpeedSign.Negative];
        for (int row = 0; row < EnemyQuadraticSpeedDefinitions.RowCount; row++)
        {
            EnemyQuadraticSpeedRow speeds = EnemyQuadraticSpeeds.Row(row);
            AssertEqual(EnemySpeedTableCodec.QuadraticRow(cartridge, EnemySpeedTableCodec.QuadraticTable, row), speeds, "quadratic speed row");
            AssertEqual(EnemySpeedTableCodec.QuadraticRow(cartridge, EnemySpeedTableCodec.QuadraticProjectileCopy, row), speeds, "quadratic projectile copy row");
            foreach (EnemySpeedSign sign in signs)
                AssertEqual(EnemySpeedTableCodec.QuadraticCoarseSpeed(cartridge, row, sign), speeds.CoarseToward(sign), "quadratic coarse speed");
        }
        AssertThrows<InvalidDataException>(() => EnemyQuadraticSpeeds.Row(-1), "negative quadratic row");
        AssertThrows<InvalidDataException>(() => EnemyQuadraticSpeeds.Row(EnemyQuadraticSpeedDefinitions.RowCount), "quadratic row past the table");
        if (definitionsOnly) return;
        var addRock = new Action<RoomEnemyProjectileSlot, int, EnemySpeedSign>(PolypBehavior.AddPolypRockQuadraticStep);
        var rock = new RoomEnemyProjectileSlot(0);
        foreach (uint origin in new uint[] { 0, 0x0080ff37, 0xffffff37 })
        for (int index = 0; index < EnemyQuadraticSpeedDefinitions.RowCount; index++)
        foreach (EnemySpeedSign sign in signs)
        {
            int speed = EnemySpeedTableCodec.QuadraticRow(cartridge, EnemySpeedTableCodec.QuadraticProjectileCopy, index).Toward(sign);
            uint expected = unchecked(origin + (uint)speed);
            rock.Position = rock.Position.WithY(new(FixedCoordinate.WholeOf(unchecked((int)origin)), unchecked((ushort)origin)));
            addRock(rock, index, sign);
            AssertEqual(expected, rock.Position.Y.RawValue, "Polyp real split-word carry/wrap");
        }
        AssertThrows<InvalidDataException>(() => addRock(rock, EnemyQuadraticSpeedDefinitions.RowCount, EnemySpeedSign.Positive), "Polyp invalid speed remains loud");
        var beetom = new Func<int, int, int>(BeetomBehavior.CalculateInitialBeetomYSpeedIndex);
        var hopper = new Func<int, int, int>(HoppersBehavior.CalculateInitialHopperYSpeedTableIndex);
        int initialIndexCases = 0;
        foreach (ushort delta in new ushort[] { 1, 2, 4 })
        for (int height = 0; height <= ushort.MaxValue; height++)
        {
            ushort accumulated = 0;
            for (int index = delta; index < EnemyQuadraticSpeedDefinitions.RowCount; index += delta)
            {
                accumulated = unchecked((ushort)(accumulated + EnemySpeedTableCodec.QuadraticCoarseSpeed(cartridge, index, EnemySpeedSign.Positive)));
                if (unchecked((short)(accumulated - height)) < 0)
                    continue;
                AssertEqual(index, beetom((ushort)height, delta), "Beetom coarse-speed hop integration");
                AssertEqual(index, hopper((ushort)height, delta), "Hopper coarse-speed hop integration");
                initialIndexCases++;
                break;
            }
        }
        // Only report inputs that the native loop resolves inside the authored
        // table. Out-of-table instruction-byte reads are not a fabricated math API.
        AssertTrue(initialIndexCases > 100000, "wide signed-height hop corpus is exercised");

        var wings = new Func<int, EnemySpeedSign, ushort>(KiHunterBehavior.ReadKiHunterQuadraticAngleDelta);
        for (int index = 0; index < EnemyQuadraticSpeedDefinitions.RowCount; index++)
        for (int low = 0; low < 256; low++)
        foreach (EnemySpeedSign sign in signs)
            AssertEqual(EnemySpeedTableCodec.QuadraticCoarseSpeed(cartridge, index, sign), (int)wings((ushort)(index * 256 + low), sign),
                "KiHunter high-byte index and coarse angle delta");

        var addMaw = new Action<YappingMawEnemyState>(YappingMawBehavior.AddYappingMawQuadraticSpeed);
        var maw = new YappingMawEnemyState(new RoomEnemySystem(new EnemySession()).SlotTable.Slots[0]);
        int mawAdditions = 0;
        foreach (uint start in new uint[] { 0, 0x0080ff37, 0xffffff37 })
        for (int record = 0; record < EnemyQuadraticSpeedDefinitions.RowCount; record++)
        foreach (bool negative in new[] { false, true })
        {
            int speed = EnemySpeedTableCodec.QuadraticRow(cartridge, EnemySpeedTableCodec.QuadraticTable, record)
                .Toward(negative ? EnemySpeedSign.Negative : EnemySpeedSign.Positive);
            maw.Extension = SignedSixteenSixteen.FromRaw(unchecked((int)start));
            maw.QuadraticSpeedRecord = record;
            maw.QuadraticSpeedNegative = negative;
            mawAdditions++;
            uint expected = unchecked(start + (uint)speed);
            addMaw(maw);
            AssertEqual(expected, unchecked((uint)maw.Extension.RawValue), "Yapping Maw real split-word integration");
            AssertEqual(new VerifyCompiledQuadraticEnemySpeedsCase(record + 1, negative), new VerifyCompiledQuadraticEnemySpeedsCase(maw.QuadraticSpeedRecord, maw.QuadraticSpeedNegative), "Yapping Maw record advance");
        }
        Console.WriteLine("Compiled quadratic speeds: all 95 rows and their coarse speeds match both cartridge copies.");
        Console.WriteLine("Projectile quadratic copy: 570 real Polyp integrations match, including subpixel carry/wrap, without a loaded bus.");
        Console.WriteLine($"Quadratic production paths: {initialIndexCases} hop inputs per Beetom/Hopper, 48640 KiHunter angle inputs, and {mawAdditions} Yapping Maw additions match cartridge bytes without a loaded bus.");
    }
}
