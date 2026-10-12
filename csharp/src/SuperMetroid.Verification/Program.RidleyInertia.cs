using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static Program;
using static RidleyDeathAccelerationVerification;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static class RidleyInertiaVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ridley inertia")]
    internal static void VerifyCompiledRidleyInertia(SuperMetroidAddressSpace rom)
    {
        var ceres = typeof(RoomEnemySystem).GetMethod("AccelerateRidleyToward", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, Position16<RoomPixels>, int>>();
        var norfair = typeof(RoomEnemySystem).GetMethod("MoveNorfairRidleyToward", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<RoomEnemySlot, RidleyEnemyState, Position16<RoomPixels>, int, ushort>>();
        var slot = new RoomEnemySystem().Slots[0];
        var state = new RidleyEnemyState();
        for (int index = 0; index < 16; index++)
        {
            byte divisor = rom.ReadCartridgeByte(0xa6d61f + index);
            byte ceresDivisor = rom.ReadCartridgeByte(0xa6d712 + index);
            AssertEqual((ushort)rom.ReadCartridgeByte(0xa6d712 + index), RidleyInertiaDefinitions.Divisor(index), "Ceres native inertia byte");
            AssertEqual((ushort)divisor, RidleyInertiaDefinitions.Divisor(index), "Norfair native inertia byte");
            // Every signed distance, with both reversal directions and zero/saturation boundaries.
            // Call the production two-axis entry points without an address space attached.
            for (int raw = 0; raw <= ushort.MaxValue; raw++)
            {
                short distance = unchecked((short)raw);
                ushort velocity = unchecked((ushort)((raw % 7) switch { 0 => -1280, 1 => -1, 2 => 0, 3 => 1, 4 => 1280, 5 => short.MinValue, _ => short.MaxValue }));
                slot.Position = slot.Position.WithWhole(new(unchecked((ushort)(0xff00 + distance)), unchecked((ushort)(0x100 - distance))));
                state.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(velocity), new SnesSignedEightEight(velocity));
                ceres(slot, state, new(0xff00, 0x100), index);
                AssertEqual(ExpectedCeresNativeAcceleration(velocity, slot.Position.X.Whole, 0xff00, ceresDivisor), state.Velocity.X.RawValue, "Ceres actual X acceleration");
                AssertEqual(ExpectedCeresNativeAcceleration(velocity, slot.Position.Y.Whole, 0x100, ceresDivisor), state.Velocity.Y.RawValue, "Ceres actual wrapped Y acceleration");
                state.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(velocity), new SnesSignedEightEight(velocity));
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

    // $A6:D62F/$D6A6: the carry left by `position - target` feeds the first ADC/SBC of
    // the velocity step, and each reversal step chains its own carry into the next.
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
            int b = unchecked((ushort)a) + q + SnesCarry.OutOfWord(a);
            value = unchecked((ushort)b) + q + SnesCarry.OutOfWord(b);
        }
        else value = initial + q + carry;
        ushort result = unchecked((ushort)value);
        return unchecked((short)(result - 0x0500)) >= 0 ? (ushort)0x0500 : result;
    }
    // Model the native word arithmetic independently as wide intermediate values
    // and explicit carry terms. The movie-derived regression supplies independent
    // expected motion; this existing numeric contract covers wrapped inputs too.
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
            int b = unchecked((ushort)a) + q + SnesCarry.OutOfWord(a);
            value = unchecked((ushort)b) + q + SnesCarry.OutOfWord(b);
        }
        else value = initial + q + (position >= target ? 1 : 0);
        ushort result = unchecked((ushort)value);
        return unchecked((short)(result - 0x0500)) >= 0 ? (ushort)0x0500 : result;
    }

    [VerificationCommand("--ridley-acceleration-carry")]
    internal static int RidleyAccelerationCarryCommand()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCompiledRidleyInertia), () => VerifyCompiledRidleyInertia(rom));
        Suite(nameof(VerifyRidleyDeathAcceleration), () => VerifyRidleyDeathAcceleration(rom));
        return 0;
    }
}
