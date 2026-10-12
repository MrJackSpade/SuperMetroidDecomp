using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled quadratic enemy speeds")]
    private static void VerifyCompiledQuadraticEnemySpeeds(SuperMetroidAddressSpace rom, bool definitionsOnly = false)
    {
        for (int offset = 0; offset <= 758; offset++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0cbc7 + offset), EnemyQuadraticSpeedDefinitions.ReadWord(offset), "quadratic projectile table mirror");
        for (int offset = 0; offset <= 758; offset++)
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + offset), EnemyQuadraticSpeedDefinitions.ReadWord(offset), "quadratic native word window");
        for (int offset = 0; offset <= 756; offset++)
            AssertEqual((unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + (offset + 2))) << 16) | CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + offset),
                EnemyQuadraticSpeedDefinitions.ReadDisplacement(offset), "quadratic native displacement window");
        AssertThrows<InvalidDataException>(() => EnemyQuadraticSpeedDefinitions.ReadWord(-1), "quadratic negative offset");
        AssertThrows<InvalidDataException>(() => EnemyQuadraticSpeedDefinitions.ReadWord(759), "quadratic partial word");
        AssertThrows<InvalidDataException>(() => EnemyQuadraticSpeedDefinitions.ReadDisplacement(757), "quadratic partial displacement");
        if (definitionsOnly) return;
        T Method<T>(string name) where T : Delegate => typeof(RoomEnemySystem)
            .GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic)!.CreateDelegate<T>();
        var shared = Method<Func<ushort, bool, int>>("ReadQuadraticEnemySpeed");
        var addRock = Method<Action<RoomEnemyProjectileSlot, ushort, bool>>("AddPolypRockQuadraticStep");
        var rock = new RoomEnemyProjectileSlot(0);
        foreach (uint origin in new uint[] { 0, 0x0080ff37, 0xffffff37 })
        for (ushort index = 0; index < 95; index++)
        foreach (bool negative in new[] { false, true })
        {
            int offset = index * 8 + (negative ? 4 : 0);
            uint expected = unchecked(origin + ((uint)CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0cbc7 + (offset + 2)) << 16 | CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0cbc7 + offset)));
            rock.Position = rock.Position.WithY(new(SnesFixedPosition.WholeOf(unchecked((int)origin)), unchecked((ushort)origin)));
            rock.Variable1 = 0xdead;
            addRock(rock, index, negative);
            AssertEqual(expected, rock.Position.Y.RawValue, "Polyp real split-word carry/wrap");
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0cbc7 + (offset + 2)), rock.Variable1, "Polyp final native scratch whole word");
        }
        AssertThrows<InvalidDataException>(() => addRock(rock, 95, false), "Polyp invalid speed remains loud");
        for (ushort index = 0; index < 95; index++)
        foreach (bool negative in new[] { false, true })
        {
            int offset = index * 8 + (negative ? 4 : 0);
            AssertEqual((unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + (offset + 2))) << 16) | CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + offset), shared(index, negative),
                "quadratic shared production reader");
        }
        var beetom = Method<Func<ushort, ushort, ushort>>("CalculateInitialBeetomYSpeedIndex");
        var hopper = Method<Func<ushort, ushort, ushort>>("CalculateInitialHopperYSpeedTableIndex");
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

        var wings = Method<Func<ushort, bool, ushort>>("ReadKiHunterQuadraticAngleDelta");
        for (int index = 0; index < 95; index++)
        for (int low = 0; low < 256; low++)
        foreach (bool negative in new[] { false, true })
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + (index * 8 + (negative ? 5 : 1))), wings((ushort)(index * 256 + low), negative),
                "KiHunter high-byte index and odd-word angle delta");

        var addMaw = Method<Action<YappingMawEnemyState>>("AddYappingMawQuadraticSpeed");
        var maw = new YappingMawEnemyState(new RoomEnemySystem().Slots[0]);
        foreach (uint start in new uint[] { 0, 0x0080ff37, 0xffffff37 })
        for (int offset = 0; offset <= 756; offset++)
        {
            maw.ExtensionWhole = SnesFixedPosition.WholeOf(unchecked((int)start));
            maw.ExtensionFraction = unchecked((ushort)start);
            maw.QuadraticSpeedByteOffset = (ushort)offset;
            uint expected = unchecked(start + ((uint)CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + (offset + 2)) << 16 | CartridgeImportSource.Require(rom).ReadCartridgeWord(0xa0838f + offset)));
            addMaw(maw);
            AssertEqual(expected, new SnesFixedPosition(maw.ExtensionWhole, maw.ExtensionFraction).RawValue, "Yapping Maw real split-word integration");
            AssertEqual(offset + 8, maw.QuadraticSpeedByteOffset, "Yapping Maw byte offset advance");
        }
        Console.WriteLine("Compiled quadratic speeds: all 759 word windows and 757 displacement windows match the authored NTSC bytes, including odd offsets and native discontinuities.");
        Console.WriteLine("Projectile quadratic mirror: all 759 word windows and 570 real Polyp integrations match, including scratch writes and subpixel carry/wrap, without a loaded bus.");
        Console.WriteLine($"Quadratic production paths: {initialIndexCases} hop inputs per Beetom/Hopper, 48640 KiHunter angle inputs, and 2271 Yapping Maw additions match cartridge bytes without a loaded bus.");
    }
}
