using static SuperMetroid.Verification.Bosses.Ridley.RidleyDeathAccelerationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.Ridley;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Compares Ridley's Ceres and Norfair inertia tables with production acceleration.</summary>
internal static class RidleyInertiaVerification
{
    /// <summary>Compares Ceres and Norfair inertia bytes to the cartridge and probes production acceleration across signed distances.</summary>
    internal static void VerifyCompiledRidleyInertia(CartridgeImportAddressSpace rom)
    {
        Action<RoomEnemySlot, RidleyEnemyState, Position16<RoomPixels>, int> ceres = CeresRidleyFlight.AccelerateRidleyToward;
        Action<RoomEnemySlot, RidleyEnemyState, Position16<RoomPixels>, int, int> norfair = NorfairRidleyMovement.MoveNorfairRidleyToward;
        var slot = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom()).SlotTable.Slots[0];
        var state = new RidleyEnemyState();
        for (int index = 0; index < 16; index++)
        {
            byte divisor = rom.ReadCartridgeByte(0xa6d61f + index);
            byte ceresDivisor = rom.ReadCartridgeByte(0xa6d712 + index);
            // The cartridge selects each divisor by its index into a table counting the divisors down from the largest.
            AssertEqual(RidleyInertiaDefinitions.LargestDivisor - index, ceresDivisor, "Ceres native inertia byte");
            AssertEqual(RidleyInertiaDefinitions.LargestDivisor - index, divisor, "Norfair native inertia byte");
            // Every signed distance a room can hold, either way and across the coordinate wrap, with both reversal
            // directions and the clamp boundaries. Velocities lie within the five-pixel clamp every seek applies, the
            // range the seek's signed clamps cover; a velocity near the word's ends is unreachable.
            // Call the production two-axis entry points without an address space attached.
            for (int raw = 0; raw <= 2 * MaximumRoomDistance; raw++)
            {
                short distance = (short)(raw - MaximumRoomDistance);
                ushort velocity = unchecked((ushort)((raw % 7) switch { 0 => -1280, 1 => -1, 2 => 0, 3 => 1, 4 => 1280, 5 => -640, _ => 640 }));
                slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(0xff00 + distance)), unchecked((ushort)(0x100 - distance))));
                state.Velocity = new EightEightVector<RoomPixels>(new SignedEightEight(velocity), new SignedEightEight(velocity));
                ceres(slot, state, new(0xff00, 0x100), ceresDivisor);
                AssertEqual(ExpectedCeresNativeAcceleration(velocity, slot.Position.X.Whole, 0xff00, ceresDivisor), state.Velocity.X.RawValue, "Ceres actual X acceleration");
                AssertEqual(ExpectedCeresNativeAcceleration(velocity, slot.Position.Y.Whole, 0x100, ceresDivisor), state.Velocity.Y.RawValue, "Ceres actual wrapped Y acceleration");
                state.Velocity = new EightEightVector<RoomPixels>(new SignedEightEight(velocity), new SignedEightEight(velocity));
                ushort boost = (ushort)(raw % 3 * 16);
                norfair(slot, state, new Position16<RoomPixels>(0xff00, 0x100), divisor, boost);
                AssertEqual(ExpectedRidleyNativeAcceleration(velocity, slot.Position.X.Whole, 0xff00, divisor, boost), state.Velocity.X.RawValue, "Norfair actual X acceleration");
                AssertEqual(ExpectedRidleyNativeAcceleration(velocity, slot.Position.Y.Whole, 0x100, divisor, boost), state.Velocity.Y.RawValue, "Norfair actual wrapped Y acceleration");
            }
        }
        AssertThrows<ArgumentOutOfRangeException>(() => RidleyInertia.Divisor(RidleyInertiaDefinitions.SmallestDivisor - 1), "Divisor below the smallest");
        AssertThrows<ArgumentOutOfRangeException>(() => RidleyInertia.Divisor(RidleyInertiaDefinitions.LargestDivisor + 1), "Divisor above the largest");
        Console.WriteLine($"Ridley inertia: 32 native bytes and {16 * (2 * MaximumRoomDistance + 1) * 2:N0} real two-axis calls match without a bus.");
    }

    /// <summary>
    /// The largest distance between two points of one room: sixteen screens of 256 pixels. Ridley and every target
    /// he seeks lie within his room, so no seek measures a longer distance.
    /// </summary>
    private const int MaximumRoomDistance = 16 * 256;

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
            int b = unchecked((ushort)a) + q + (a > ushort.MaxValue ? 1 : 0);
            value = unchecked((ushort)b) + q + (b > ushort.MaxValue ? 1 : 0);
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
            int b = unchecked((ushort)a) + q + (a > ushort.MaxValue ? 1 : 0);
            value = unchecked((ushort)b) + q + (b > ushort.MaxValue ? 1 : 0);
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
