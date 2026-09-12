using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyCompiledQuadraticEnemySpeeds(SuperMetroidAddressSpace rom)
    {
        ushort Word(int offset) => (ushort)(rom.ReadByte(0xa0838f + offset) | rom.ReadByte(0xa08390 + offset) << 8);
        ushort ProjectileWord(int offset) => (ushort)(rom.ReadByte(0xa0cbc7 + offset) | rom.ReadByte(0xa0cbc8 + offset) << 8);
        for (int offset = 0; offset <= 758; offset++)
            AssertEqual(ProjectileWord(offset), EnemyQuadraticSpeedDefinitions.ReadWord(offset), "quadratic projectile table mirror");
        for (int offset = 0; offset <= 758; offset++)
            AssertEqual(Word(offset), EnemyQuadraticSpeedDefinitions.ReadWord(offset), "quadratic native word window");
        for (int offset = 0; offset <= 756; offset++)
            AssertEqual((unchecked((short)Word(offset + 2)) << 16) | Word(offset),
                EnemyQuadraticSpeedDefinitions.ReadDisplacement(offset), "quadratic native displacement window");
        AssertThrows<InvalidDataException>(() => EnemyQuadraticSpeedDefinitions.ReadWord(-1), "quadratic negative offset");
        AssertThrows<InvalidDataException>(() => EnemyQuadraticSpeedDefinitions.ReadWord(759), "quadratic partial word");
        AssertThrows<InvalidDataException>(() => EnemyQuadraticSpeedDefinitions.ReadDisplacement(757), "quadratic partial displacement");
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
            uint expected = unchecked(origin + ((uint)ProjectileWord(offset + 2) << 16 | ProjectileWord(offset)));
            rock.YPosition = (ushort)(origin >> 16);
            rock.YSubposition = unchecked((ushort)origin);
            rock.Variable1 = 0xdead;
            addRock(rock, index, negative);
            AssertEqual(expected, ((uint)rock.YPosition << 16) | rock.YSubposition, "Polyp real split-word carry/wrap");
            AssertEqual(ProjectileWord(offset + 2), rock.Variable1, "Polyp final native scratch whole word");
        }
        AssertThrows<InvalidDataException>(() => addRock(rock, 95, false), "Polyp invalid speed remains loud");
        for (ushort index = 0; index < 95; index++)
        foreach (bool negative in new[] { false, true })
        {
            int offset = index * 8 + (negative ? 4 : 0);
            AssertEqual((unchecked((short)Word(offset + 2)) << 16) | Word(offset), shared(index, negative),
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
                accumulated = unchecked((ushort)(accumulated + Word(index * 8 + 1)));
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
            AssertEqual(Word(index * 8 + (negative ? 5 : 1)), wings((ushort)(index * 256 + low), negative),
                "KiHunter high-byte index and odd-word angle delta");

        var addMaw = Method<Action<YappingMawEnemyState>>("AddYappingMawQuadraticSpeed");
        var maw = new YappingMawEnemyState(new RoomEnemySystem().Slots[0]);
        foreach (uint start in new uint[] { 0, 0x0080ff37, 0xffffff37 })
        for (int offset = 0; offset <= 756; offset++)
        {
            maw.ExtensionWhole = (ushort)(start >> 16);
            maw.ExtensionFraction = unchecked((ushort)start);
            maw.QuadraticSpeedByteOffset = (ushort)offset;
            uint expected = unchecked(start + ((uint)Word(offset + 2) << 16 | Word(offset)));
            addMaw(maw);
            AssertEqual(expected, ((uint)maw.ExtensionWhole << 16) | maw.ExtensionFraction, "Yapping Maw real split-word integration");
            AssertEqual(offset + 8, maw.QuadraticSpeedByteOffset, "Yapping Maw byte offset advance");
        }
        Console.WriteLine("Compiled quadratic speeds: all 759 word windows and 757 displacement windows match the authored NTSC bytes, including odd offsets and native discontinuities.");
        Console.WriteLine("Projectile quadratic mirror: all 759 word windows and 570 real Polyp integrations match, including scratch writes and subpixel carry/wrap, without a loaded bus.");
        Console.WriteLine($"Quadratic production paths: {initialIndexCases} hop inputs per Beetom/Hopper, 48640 KiHunter angle inputs, and 2271 Yapping Maw additions match cartridge bytes without a loaded bus.");
    }
}
