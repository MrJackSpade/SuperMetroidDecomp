using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Represents ki hunter motion definitions verification.</summary>
internal static class KiHunterMotionDefinitionsVerification
{
    /// <summary>Verifies ki hunter motion definitions.</summary>
    internal static void VerifyKiHunterMotionDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => (ushort)(SnesLittleEndian.Word(KiHunterMotionDefinitionsRomByte(rom, address), KiHunterMotionDefinitionsRomByte(rom, address + 1)));
        AssertEqual(Word(0xa8f180), KiHunterMotionDefinitions.SwoopTriggerDistance, "Native KiHunter proximity");
        ushort low = Word(0xa8f182), high = Word(0xa8f184);
        byte radius = KiHunterMotionDefinitionsRomByte(rom, 0xa8f186);
        AssertEqual(low, KiHunterMotionDefinitions.GravityFraction, "Native KiHunter fractional acceleration");
        AssertEqual(high, KiHunterMotionDefinitions.GravityWhole, "Native KiHunter whole acceleration");
        AssertEqual(radius, KiHunterMotionDefinitions.DetachedWingRadius, "Native KiHunter low-byte wing radius");
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
        var orbit = new Action<RoomEnemySlot, KiHunterEnemyState>(KiHunterBehavior.RunDetachedKiHunterWingOrbit);
        var delta = new Func<ushort, bool, ushort>(KiHunterBehavior.ReadKiHunterQuadraticAngleDelta);
        var cosine = new Func<ushort, ushort, int>(EnemyMath.ReadEightBitCosineProduct);
        var sine = new Func<ushort, ushort, int>(EnemyMath.ReadEightBitNegativeSineProduct);
        var gravity = KiHunterBehavior.KiHunterGravity;
        var slot = new RoomEnemySystem().SlotTable.Slots[0];
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
            uint expected = unchecked(((uint)(ushort.MaxValue - raw) << 16 | (uint)raw) + (SnesSixteenSixteen.Compose(high, low)));
            AssertEqual(SnesFixedPosition.WholeOf(unchecked((int)expected)), actual.WholeWord, "Native gravity whole carry");
            AssertEqual((ushort)expected, actual.Fraction, "Native gravity fraction wrap");
        }
        Console.WriteLine("KiHunter motion: native constants, 65536 bus-free orbit placements and all gravity fractional carries pass.");
    }

    /// <summary>Ki hunter motion definitions rom byte.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ki Hunter motion definitions")]
    private static byte KiHunterMotionDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
