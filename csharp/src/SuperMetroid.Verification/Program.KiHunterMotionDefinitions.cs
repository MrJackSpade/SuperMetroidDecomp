using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKiHunterMotionDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(KiHunterMotionDefinitionsRomByte(rom, address) | KiHunterMotionDefinitionsRomByte(rom, address + 1) << 8);
        AssertEqual(Word(0xa8f180), KiHunterMotionDefinitions.SwoopTriggerDistance, "Native KiHunter proximity");
        ushort low = Word(0xa8f182), high = Word(0xa8f184);
        byte radius = KiHunterMotionDefinitionsRomByte(rom, 0xa8f186);
        AssertEqual(low, KiHunterMotionDefinitions.GravityFraction, "Native KiHunter fractional acceleration");
        AssertEqual(high, KiHunterMotionDefinitions.GravityWhole, "Native KiHunter whole acceleration");
        AssertEqual(radius, KiHunterMotionDefinitions.DetachedWingRadius, "Native KiHunter low-byte wing radius");
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
        T Method<T>(string name) where T : Delegate => typeof(RoomEnemySystem).GetMethod(name, flags)!.CreateDelegate<T>();
        var orbit = Method<Action<RoomEnemySlot, KiHunterEnemyState>>("RunDetachedKiHunterWingOrbit");
        var delta = Method<Func<ushort, bool, ushort>>("ReadKiHunterQuadraticAngleDelta");
        var cosine = Method<Func<ushort, ushort, int>>("ReadEightBitCosineProduct");
        var sine = Method<Func<ushort, ushort, int>>("ReadEightBitNegativeSineProduct");
        var gravity = (SnesSignedSixteenSixteen)typeof(RoomEnemySystem).GetProperty("KiHunterGravity", flags)!.GetValue(null)!;
        var slot = new RoomEnemySystem().Slots[0];
        var state = new KiHunterEnemyState(slot);
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            // The native decrement clamps to this minimum speed; varying arbitrary
            // high-byte indexes would leave the authored quadratic-speed records.
            const ushort speed = 256;
            ushort angle = unchecked((ushort)(raw + delta(speed, true)));
            state.Angle = (ushort)raw;
            state.TargetXOrSpeedIndex = speed;
            state.OrbitCenter = new Position16<RoomPixels>((ushort)raw, (ushort)(ushort.MaxValue - raw));
            state.OrbitOffset = new Offset16<RoomPixels>(13, 17);
            ushort x = unchecked((ushort)(state.OrbitCenter.X + cosine((ushort)(angle >> 8), radius) - 13));
            ushort y = unchecked((ushort)(state.OrbitCenter.Y + sine((ushort)(angle >> 8), radius) - 17));
            orbit(slot, state);
            AssertEqual(x, slot.Position.X.Whole, "Actual detached orbit uses native radius and wrapped X");
            AssertEqual(y, slot.Position.Y.Whole, "Actual detached orbit uses native radius and wrapped Y");
            SnesSignedSixteenSixteen actual = SnesSignedSixteenSixteen.FromParts(
                unchecked((short)(ushort.MaxValue - raw)), (ushort)raw) + gravity;
            uint expected = unchecked(((uint)(ushort.MaxValue - raw) << 16 | (uint)raw) + ((uint)high << 16 | low));
            AssertEqual((ushort)(expected >> 16), actual.WholeWord, "Native gravity whole carry");
            AssertEqual((ushort)expected, actual.Fraction, "Native gravity fraction wrap");
        }
        Console.WriteLine("KiHunter motion: native constants, 65536 bus-free orbit placements and all gravity fractional carries pass.");
    }

    [CpuBusBoundary("exact bus test: retail ROM oracle for Ki Hunter motion definitions")]
    private static byte KiHunterMotionDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
