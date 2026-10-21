using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.KiHunters;

/// <summary>Checks Ki Hunter motion constants, detached wing orbit math, and gravity carry.</summary>
internal static class KiHunterMotionDefinitionsVerification
{
    /// <summary>Compares retail constants and every 16-bit orbit position without gameplay bus reads.</summary>
    internal static void VerifyKiHunterMotionDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => SnesLittleEndian.Word(KiHunterMotionDefinitionsRomByte(rom, address), KiHunterMotionDefinitionsRomByte(rom, address + 1));
        AssertEqual(Word(0xa8f180), KiHunterMotionDefinitions.SwoopTriggerDistance, "Native KiHunter proximity");
        ushort low = Word(0xa8f182), high = Word(0xa8f184);
        byte radius = KiHunterMotionDefinitionsRomByte(rom, 0xa8f186);
        AssertEqual(low, KiHunterMotionDefinitions.GravityFraction, "Native KiHunter fractional acceleration");
        AssertEqual(high, KiHunterMotionDefinitions.GravityWhole, "Native KiHunter whole acceleration");
        AssertEqual(radius, KiHunterMotionDefinitions.DetachedWingRadius, "Native KiHunter low-byte wing radius");
        var orbit = new Action<RoomEnemySlot, KiHunterEnemyState>(KiHunterBehavior.RunDetachedKiHunterWingOrbit);
        var delta = new Func<int, QuadraticSpeedHalf, ushort>(KiHunterBehavior.ReadKiHunterQuadraticAngleDelta);
        var cosine = new Func<ushort, ushort, int>((angle, radius) => EnemyMath.ReadEightBitCosineProduct(new SnesByteAngle(unchecked((byte)angle)), radius));
        var sine = new Func<ushort, ushort, int>((angle, radius) => EnemyMath.ReadEightBitNegativeSineProduct(new SnesByteAngle(unchecked((byte)angle)), radius));
        var gravity = KiHunterBehavior.KiHunterGravity;
        var slot = new RoomEnemySystem(new EnemySession()).SlotTable.Slots[0];
        var state = new KiHunterEnemyState(slot);
        // Any reset distinguishes an arc change, which restores it, from a slowing step, which lands on 256.
        const int speedReset = 10752;
        state.DetachedSpeedReset = speedReset;
        int arcChanges = 0;
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            // The native decrement clamps to this minimum speed; varying arbitrary
            // high-byte indexes would leave the authored quadratic-speed records.
            const ushort speed = 256;
            ushort angle = unchecked((ushort)(raw + delta(speed, QuadraticSpeedHalf.Negative)));
            state.WingArcAngle = SnesAngle.FromRaw((ushort)raw);
            state.WingFunction = KiHunterWingFunction.Orbit;
            state.DetachedSpeedIndex = speed;
            state.OrbitCenter = new Position16<RoomPixels>((ushort)raw, (ushort)(ushort.MaxValue - raw));
            state.OrbitOffset = new Offset16<RoomPixels>(13, 17);
            ushort x = unchecked((ushort)(state.OrbitCenter.X + cosine((ushort)(angle >> 8), radius) - 13));
            ushort y = unchecked((ushort)(state.OrbitCenter.Y + sine((ushort)(angle >> 8), radius) - 17));
            orbit(slot, state);
            AssertEqual(x, slot.Position.X.Whole, "Actual detached orbit uses native radius and wrapped X");
            AssertEqual(y, slot.Position.Y.Whole, "Actual detached orbit uses native radius and wrapped Y");
            // The orbit hands over to the falling arc exactly while the advanced angle lies from a quarter turn up
            // to three quarters (the cartridge's sign test of the angle plus a quarter turn, across its wrap).
            bool changesArc = angle >= SnesAngle.QuarterTurn.RawValue && angle < SnesAngle.ThreeQuarterTurn.RawValue;
            arcChanges += changesArc ? 1 : 0;
            AssertEqual(changesArc ? KiHunterWingFunction.FallingCollisionArc : KiHunterWingFunction.Orbit, state.WingFunction,
                $"Detached orbit at advanced angle {angle} changes arc only within the middle half turn");
            AssertEqual(changesArc ? speedReset : 256, state.DetachedSpeedIndex,
                $"Detached orbit at advanced angle {angle} restores the reset on an arc change or lands a slowing step on 256");
            SnesSignedSixteenSixteen actual = SnesSignedSixteenSixteen.FromParts(
                unchecked((short)(ushort.MaxValue - raw)), (ushort)raw) + gravity;
            uint expected = unchecked(((uint)(ushort.MaxValue - raw) << 16 | (uint)raw) + (SnesSixteenSixteen.Compose(high, low)));
            AssertEqual(SnesFixedPosition.WholeOf(unchecked((int)expected)), actual.WholeWord, "Native gravity whole carry");
            AssertEqual((ushort)expected, actual.Fraction, "Native gravity fraction wrap");
        }
        AssertEqual(32768, arcChanges, "Detached orbit changes arc for exactly half of all advanced angles");
        foreach ((int before, int after) in new[] { new VerifyKiHunterMotionDefinitionsCase(1000, 616), new VerifyKiHunterMotionDefinitionsCase(384, 0), new VerifyKiHunterMotionDefinitionsCase(383, 256), new VerifyKiHunterMotionDefinitionsCase(0, 256) })
        {
            state.WingArcAngle = SnesAngle.Zero;
            state.WingFunction = KiHunterWingFunction.Orbit;
            state.DetachedSpeedIndex = before;
            orbit(slot, state);
            AssertEqual(KiHunterWingFunction.Orbit, state.WingFunction, $"Detached orbit from speed index {before} keeps orbiting");
            AssertEqual(after, state.DetachedSpeedIndex, $"Detached orbit slows speed index {before} by one step or lands on 256");
        }
        Console.WriteLine("KiHunter motion: native constants, 65536 bus-free orbit placements, arc changes and speed steps, and all gravity fractional carries pass.");
    }

    /// <summary>Reads the retail motion record as the oracle for the compiled constants.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ki Hunter motion definitions")]
    private static byte KiHunterMotionDefinitionsRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);
}
