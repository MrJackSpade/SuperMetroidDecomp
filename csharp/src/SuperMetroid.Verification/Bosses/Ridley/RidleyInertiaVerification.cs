using static SuperMetroid.Verification.Bosses.Ridley.RidleyDeathAccelerationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Compares Ridley's Ceres and Norfair inertia tables with production acceleration.</summary>
internal static class RidleyInertiaVerification
{
    /// <summary>Compares Ceres and Norfair inertia bytes to the cartridge and probes production acceleration across signed distances.</summary>
    internal static void VerifyCompiledRidleyInertia(SuperMetroidAddressSpace rom)
    {
        Action<RoomEnemySlot, RidleyEnemyState, Position16<RoomPixels>, int> ceres = CeresRidleyFlight.AccelerateRidleyToward;
        Action<RoomEnemySlot, RidleyEnemyState, Position16<RoomPixels>, int, int> norfair = NorfairRidleyMovement.MoveNorfairRidleyToward;
        var slot = new RoomEnemySystem(new EnemySession()).SlotTable.Slots[0];
        var state = new RidleyEnemyState();
        for (int index = 0; index < 16; index++)
        {
            byte divisor = rom.ReadCartridgeByte(0xa6d61f + index);
            byte ceresDivisor = rom.ReadCartridgeByte(0xa6d712 + index);
            AssertEqual(rom.ReadCartridgeByte(0xa6d712 + index), RidleyInertiaDefinitions.Divisor(index), "Ceres native inertia byte");
            AssertEqual(divisor, RidleyInertiaDefinitions.Divisor(index), "Norfair native inertia byte");
            // Every signed distance, with both reversal directions and zero/saturation boundaries.
            // Call the production two-axis entry points without an address space attached.
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                short distance = unchecked((short)raw);
                ushort velocity = unchecked((ushort)((raw % 7) switch { 0 => -1280, 1 => -1, 2 => 0, 3 => 1, 4 => 1280, 5 => short.MinValue, _ => short.MaxValue }));
                slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(0xff00 + distance)), unchecked((ushort)(0x100 - distance))));
                state.Velocity = new EightEightVector<RoomPixels>(new SignedEightEight(velocity), new SignedEightEight(velocity));
                ceres(slot, state, new(0xff00, 0x100), index);
                AssertEqual(ExpectedCeresNativeAcceleration(velocity, slot.Position.X.Whole, 0xff00, ceresDivisor), state.Velocity.X.RawValue, "Ceres actual X acceleration");
                AssertEqual(ExpectedCeresNativeAcceleration(velocity, slot.Position.Y.Whole, 0x100, ceresDivisor), state.Velocity.Y.RawValue, "Ceres actual wrapped Y acceleration");
                state.Velocity = new EightEightVector<RoomPixels>(new SignedEightEight(velocity), new SignedEightEight(velocity));
                ushort boost = (ushort)(raw % 3 * 16);
                norfair(slot, state, new Position16<RoomPixels>(0xff00, 0x100), index, boost);
                AssertEqual(ExpectedRidleyNativeAcceleration(velocity, slot.Position.X.Whole, 0xff00, divisor, boost), state.Velocity.X.RawValue, "Norfair actual X acceleration");
                AssertEqual(ExpectedRidleyNativeAcceleration(velocity, slot.Position.Y.Whole, 0x100, divisor, boost), state.Velocity.Y.RawValue, "Norfair actual wrapped Y acceleration");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => RidleyInertiaDefinitions.Divisor(-1), "Negative inertia index");
        AssertThrows<ArgumentOutOfRangeException>(() => RidleyInertiaDefinitions.Divisor(16), "Inertia table boundary");
        Console.WriteLine("Ridley inertia: 32 native bytes and 2,097,152 real two-axis calls match without a bus.");
    }

    /// <summary>
    /// Computes Ceres Ridley's expected velocity from $A6:D62F/$D6A6. The carry left by
    /// position minus target feeds the first ADC/SBC, and reversal steps chain their carry.
    /// </summary>
    private static ushort ExpectedCeresNativeAcceleration(ushort initial, ushort position, ushort target, int divisor)
    {
        short distance = unchecked((short)(position - target));
        if (distance == 0) return initial;
        int q = Math.Max(1, Math.Abs((int)distance) / divisor);
        int value;
        if (distance > 0)
        {
            int borrow = position < target ? 1 : 0;
            if ((short)initial >= 0)
            {
                int a = initial - q - borrow;
                int b = unchecked((ushort)a) - q - (a < 0 ? 1 : 0);
                value = unchecked((ushort)b) - q - (b < 0 ? 1 : 0);
            }
            else value = initial - q - borrow;
            ushort word = unchecked((ushort)value);
            return unchecked((short)(word - 0xfb00)) < 0 ? (ushort)0xfb00 : word;
        }
        int carry = position >= target ? 1 : 0;
        if ((short)initial < 0)
        {
            int a = initial + q + carry;
            int b = unchecked((ushort)a) + q + WordCarry.OutOfWord(a);
            value = unchecked((ushort)b) + q + WordCarry.OutOfWord(b);
        }
        else value = initial + q + carry;
        ushort result = unchecked((ushort)value);
        return unchecked((short)(result - 0x0500)) >= 0 ? (ushort)0x0500 : result;
    }
    /// <summary>
    /// Models Norfair Ridley's native word arithmetic with wide intermediates and explicit
    /// carry terms. The movie regression supplies expected motion; this covers wrapped inputs.
    /// </summary>
    internal static ushort ExpectedRidleyNativeAcceleration(ushort initial, ushort position, ushort target, int divisor, int boost)
    {
        short distance = unchecked((short)(position - target));
        if (distance == 0) return initial;
        int q = Math.Max(1, Math.Abs((int)distance) / divisor);
        int value;
        if (distance > 0)
        {
            if ((short)initial >= 0)
            {
                int a = unchecked((ushort)(initial - boost)) - 8;
                int b = unchecked((ushort)a) - q - (a < 0 ? 1 : 0);
                value = unchecked((ushort)b) - q - (b < 0 ? 1 : 0);
            }
            else value = initial - q - (position < target ? 1 : 0);
            ushort word = unchecked((ushort)value);
            return unchecked((short)(word - 0xfb00)) < 0 ? (ushort)0xfb00 : word;
        }
        if ((short)initial < 0)
        {
            int a = unchecked((ushort)(initial + boost)) + 8;
            int b = unchecked((ushort)a) + q + WordCarry.OutOfWord(a);
            value = unchecked((ushort)b) + q + WordCarry.OutOfWord(b);
        }
        else value = initial + q + (position >= target ? 1 : 0);
        ushort result = unchecked((ushort)value);
        return unchecked((short)(result - 0x0500)) >= 0 ? (ushort)0x0500 : result;
    }

    /// <summary>Runs the inertia carry and Ridley death acceleration suites against retail data.</summary>
    [VerificationCommand("--ridley-acceleration-carry")]
    internal static int RidleyAccelerationCarryCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCompiledRidleyInertia), () => VerifyCompiledRidleyInertia(rom));
        Suite(nameof(VerifyRidleyDeathAcceleration), () => VerifyRidleyDeathAcceleration(rom));
        return 0;
    }
}
