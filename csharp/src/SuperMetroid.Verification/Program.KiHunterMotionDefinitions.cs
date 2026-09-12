using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyKiHunterMotionDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        AssertEqual(Word(0xa8f180), KiHunterMotionDefinitions.SwoopTriggerDistance, "Native KiHunter proximity");
        ushort low = Word(0xa8f182), high = Word(0xa8f184);
        byte radius = rom.ReadByte(0xa8f186);
        AssertEqual(low, KiHunterMotionDefinitions.GravityFraction, "Native KiHunter fractional acceleration");
        AssertEqual(high, KiHunterMotionDefinitions.GravityWhole, "Native KiHunter whole acceleration");
        AssertEqual(radius, KiHunterMotionDefinitions.DetachedWingRadius, "Native KiHunter low-byte wing radius");
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
        T Method<T>(string name) where T : Delegate => typeof(RoomEnemySystem).GetMethod(name, flags)!.CreateDelegate<T>();
        var orbit = Method<Action<RoomEnemySlot, KiHunterEnemyState>>("RunDetachedKiHunterWingOrbit");
        var delta = Method<Func<ushort, bool, ushort>>("ReadKiHunterQuadraticAngleDelta");
        var cosine = Method<Func<ushort, ushort, int>>("ReadEightBitCosineProduct");
        var sine = Method<Func<ushort, ushort, int>>("ReadEightBitNegativeSineProduct");
        var add = Method<Func<ushort, ushort, ushort, ushort, (ushort Whole, ushort Fraction)>>("AddKiHunterFixed");
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
            state.OrbitCenterX = (ushort)raw;
            state.OrbitCenterY = (ushort)(ushort.MaxValue - raw);
            state.OrbitXOffset = 13;
            state.OrbitYOffset = 17;
            ushort x = unchecked((ushort)(state.OrbitCenterX + cosine((ushort)(angle >> 8), radius) - 13));
            ushort y = unchecked((ushort)(state.OrbitCenterY + sine((ushort)(angle >> 8), radius) - 17));
            orbit(slot, state);
            AssertEqual(x, slot.XPosition, "Actual detached orbit uses native radius and wrapped X");
            AssertEqual(y, slot.YPosition, "Actual detached orbit uses native radius and wrapped Y");
            var actual = add((ushort)(ushort.MaxValue - raw), (ushort)raw,
                KiHunterMotionDefinitions.GravityWhole, KiHunterMotionDefinitions.GravityFraction);
            uint expected = unchecked(((uint)(ushort.MaxValue - raw) << 16 | (uint)raw) + ((uint)high << 16 | low));
            AssertEqual((ushort)(expected >> 16), actual.Whole, "Native gravity whole carry");
            AssertEqual((ushort)expected, actual.Fraction, "Native gravity fraction wrap");
        }
        Console.WriteLine("KiHunter motion: native constants, 65536 bus-free orbit placements and all gravity fractional carries pass.");
    }
}
