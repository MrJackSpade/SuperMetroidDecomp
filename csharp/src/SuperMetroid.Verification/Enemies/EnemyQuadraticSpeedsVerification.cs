using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks the quadratic speed rows and their Polyp, Beetom, Hopper, KiHunter, and Maw consumers.</summary>
internal static class EnemyQuadraticSpeedsVerification
{
    /// <summary>
    /// Compares every row's speed, its stored negation with the speed negated at use (#2157), and its coarse speeds with
    /// the cartridge, then focused production integrations.
    /// </summary>
    internal static void VerifyCompiledQuadraticEnemySpeeds(CartridgeImportAddressSpace rom, bool definitionsOnly = false)
    {
        IImportCartridgeSource cartridge = CartridgeImportSource.Require(rom);
        EnemySpeedSign[] signs = [EnemySpeedSign.Positive, EnemySpeedSign.Negative];
        for (int row = 0; row < QuadraticSpeedCurveDefinitions.MarkCount; row++)
        {
            QuadraticCurveSpeed speeds = QuadraticSpeedCurve.AtMark(row);
            foreach (int table in (int[])[EnemySpeedTableCodec.QuadraticTable, EnemySpeedTableCodec.QuadraticProjectileCopy])
            {
                (SignedSixteenSixteen positive, SignedSixteenSixteen negative) = EnemySpeedTableCodec.QuadraticRow(cartridge, table, row);
                AssertEqual(speeds.Speed, positive, $"quadratic speed row {row} at {table:X6}");
                AssertEqual(speeds.Toward(EnemySpeedSign.Negative), negative,
                    $"quadratic speed row {row} at {table:X6}: stored negation is the speed negated at use");
            }
            foreach (EnemySpeedSign sign in signs)
                AssertEqual(EnemySpeedTableCodec.QuadraticCoarseSpeed(cartridge, row, sign), (int)unchecked((ushort)speeds.CoarseToward(sign)), "quadratic coarse speed");
        }
        AssertThrows<InvalidDataException>(() => QuadraticSpeedCurve.AtMark(-1), "negative quadratic row");
        AssertThrows<InvalidDataException>(() => QuadraticSpeedCurve.AtMark(QuadraticSpeedCurveDefinitions.MarkCount), "quadratic row past the table");
        if (definitionsOnly) return;
        var addRock = new Action<EnemyProjectile, QuadraticSpeedCurvePlace, EnemySpeedSign>(PolypBehavior.AddPolypRockQuadraticStep);
        var rock = new EnemyProjectile(0, default);
        foreach (uint origin in new uint[] { 0, 0x0080ff37, 0xffffff37 })
        for (int index = 0; index < QuadraticSpeedCurveDefinitions.MarkCount; index++)
        foreach (EnemySpeedSign sign in signs)
        {
            int speed = EnemySpeedTableCodec.QuadraticSpeedToward(cartridge, EnemySpeedTableCodec.QuadraticProjectileCopy, index, sign).RawValue;
            uint expected = unchecked(origin + (uint)speed);
            rock.Position = rock.Position.WithY(SixteenSixteenWords.ToCoordinate(origin));
            addRock(rock, QuadraticSpeedCurvePlace.FromMarks(index), sign);
            AssertEqual(SixteenSixteenWords.ToCoordinate(expected), rock.Position.Y, "Polyp real split-word carry/wrap");
        }
        AssertThrows<InvalidDataException>(() => addRock(rock, QuadraticSpeedCurvePlace.FromMarks(QuadraticSpeedCurveDefinitions.MarkCount), EnemySpeedSign.Positive), "Polyp invalid speed remains loud");
        // The Beetom's and the hoppers' climbs (#2026): for every height up to the largest climb, at one, two and four
        // marks a pass, the climb starts at the mark the original's sixteen-bit sum, compared with the height by the sign
        // of their difference, settles on; a height above the largest climb or below nothing is refused.
        QuadraticSpeedCurvePlace Climb(int height, int marks) =>
            new QuadraticCurveClimb(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(height), QuadraticSpeedCurvePlace.FromMarks(marks)).StartPlace;
        int largestClimb = QuadraticSpeedCurveDefinitions.LargestClimbHeight * 256;
        int initialIndexCases = 0;
        foreach (ushort delta in new ushort[] { 1, 2, 4 })
        for (int height = 0; height <= largestClimb; height++)
        {
            ushort accumulated = 0;
            for (int index = delta; index < QuadraticSpeedCurveDefinitions.MarkCount; index += delta)
            {
                accumulated = unchecked((ushort)(accumulated + EnemySpeedTableCodec.QuadraticCoarseSpeed(cartridge, index, EnemySpeedSign.Positive)));
                if (unchecked((short)(accumulated - height)) < 0)
                    continue;
                AssertEqual(QuadraticSpeedCurvePlace.FromMarks(index), Climb(height, delta), "Beetom and hopper coarse-speed climb");
                initialIndexCases++;
                break;
            }
        }
        // At four marks a pass the curve's coarse speeds sum to more than the largest climb, so every height settles.
        AssertEqual(3 * (largestClimb + 1), initialIndexCases, "every climb height up to the largest settles inside the curve");
        AssertThrows<ArgumentOutOfRangeException>(() => Climb(largestClimb + 1, 1), "a climb above the largest is refused");
        AssertThrows<ArgumentOutOfRangeException>(() => Climb(-1, 1), "a climb below nothing is refused");
        // #1627: the climb is bounded by the table's rows, not a pass count. Fifty rows at a time, a climb row fifty settles
        // is taken there; one 256th taller runs on past the table's last row and is rejected; a delta of zero settles on
        // its first pass or never.
        int fiftiethRow = EnemySpeedTableCodec.QuadraticCoarseSpeed(cartridge, 50, EnemySpeedSign.Positive);
        AssertEqual(QuadraticSpeedCurvePlace.FromMarks(50), Climb(fiftiethRow, 50), "a climb settled by row fifty ends there");
        AssertThrows<InvalidDataException>(() => Climb(fiftiethRow + 1, 50), "a climb past the table's last row is rejected");
        AssertEqual(QuadraticSpeedCurvePlace.Start, Climb(0, 0), "a climb of no height with no delta settles on the first row");
        AssertThrows<InvalidDataException>(() => Climb(1, 0), "a climb with no delta never reaches a height");
        AssertEqual(Climb(48 * 256, 4), BeetomLeapDefinitions.ShortLeap.StartPlace, "the Beetom's short leap climbs 48 pixels at four marks a pass");
        AssertEqual(Climb(64 * 256, 5), BeetomLeapDefinitions.LongLeap.StartPlace, "the Beetom's long leap climbs 64 pixels at five marks a pass");
        AssertEqual(Climb(48 * 256, 3), BeetomLeapDefinitions.Lunge.StartPlace, "the Beetom's lunge climbs 48 pixels at three marks a pass");
        AssertEqual(Climb(16 * 256, 3), HopperHopDefinitions.SmallHop.StartPlace, "a hopper's small hop climbs 16 pixels at three marks a pass");
        AssertEqual(Climb(48 * 256, 4), HopperHopDefinitions.BigHop.StartPlace, "a hopper's big hop climbs 48 pixels at four marks a pass");
        // #2065: the Ki-Hunter's detached wings reset to the place the original's sixteen-bit sum of coarse speeds, stepping
        // a mark and a half a pass from the curve's start and compared with 32 pixels by the sign of their difference,
        // settles on.
        int wingPlace = 0;
        ushort wingSum = 0;
        do
        {
            wingPlace += 384;
            wingSum = unchecked((ushort)(wingSum + EnemySpeedTableCodec.QuadraticCoarseSpeed(cartridge, wingPlace / 256, EnemySpeedSign.Positive)));
        }
        while (unchecked((short)(wingSum - 32 * 256)) < 0);
        AssertEqual(QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(wingPlace), KiHunterMotionDefinitions.DetachedWingClimb.StartPlace,
            "the Ki-Hunter's detached wings climb 32 pixels at a mark and a half a pass");

        var wings = new Func<QuadraticSpeedCurvePlace, EnemySpeedSign, int>(KiHunterBehavior.DetachedWingCoarseSpeed);
        for (int index = 0; index < QuadraticSpeedCurveDefinitions.MarkCount; index++)
        for (int low = 0; low < 256; low++)
        foreach (EnemySpeedSign sign in signs)
            AssertEqual(EnemySpeedTableCodec.QuadraticCoarseSpeed(cartridge, index, sign), (int)unchecked((ushort)wings(QuadraticSpeedCurvePlace.FromTwoHundredFiftySixthsOfMark(index * 256 + low), sign)),
                "KiHunter high-byte index and coarse wing orbit speed");

        var addMaw = new Action<YappingMawEnemyState>(YappingMawBehavior.AddYappingMawQuadraticSpeed);
        var maw = new YappingMawEnemyState();
        int mawAdditions = 0;
        foreach (uint start in new uint[] { 0, 0x0080ff37, 0xffffff37 })
        for (int row = 0; row < QuadraticSpeedCurveDefinitions.MarkCount; row++)
        foreach (EnemySpeedSign sign in signs)
        {
            int speed = EnemySpeedTableCodec.QuadraticSpeedToward(cartridge, EnemySpeedTableCodec.QuadraticTable, row, sign).RawValue;
            maw.Extension = SixteenSixteenWords.ToDisplacement(unchecked((int)start));
            maw.SpeedCurvePlace = QuadraticSpeedCurvePlace.FromMarks(row);
            maw.QuadraticSpeedSign = sign;
            mawAdditions++;
            uint expected = unchecked(start + (uint)speed);
            addMaw(maw);
            AssertEqual(expected, unchecked((uint)maw.Extension.RawValue), "Yapping Maw real split-word integration");
            AssertEqual(QuadraticSpeedCurvePlace.FromMarks(row + 1), maw.SpeedCurvePlace, "Yapping Maw row advance");
            AssertEqual(sign, maw.QuadraticSpeedSign, "Yapping Maw speed sign held across the addition");
        }
        Console.WriteLine("Compiled quadratic speeds: all 95 rows and their coarse speeds match both cartridge copies.");
        Console.WriteLine("Projectile quadratic copy: 570 real Polyp integrations match, including subpixel carry/wrap, without a loaded bus.");
        Console.WriteLine($"Quadratic production paths: {initialIndexCases} hop inputs per Beetom/Hopper, 48640 KiHunter angle inputs, and {mawAdditions} Yapping Maw additions match cartridge bytes without a loaded bus.");
    }
}
