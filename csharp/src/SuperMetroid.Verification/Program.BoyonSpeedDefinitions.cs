using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

/// <summary>Represents boyon speed definitions verification.</summary>
internal static class BoyonSpeedDefinitionsVerification
{
    /// <summary>verify compiled boyon speeds.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled Boyon speeds")]
    internal static void VerifyCompiledBoyonSpeeds(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem();
        Action<RoomEnemySlot> initialize = enemies.BoyonBehavior.InitializeBoyon;
        RoomEnemySlot actor = enemies.SlotTable.Slots[0];
        for (int height = 0; height < 9; height++)
        {
            for (int multiplier = 0; multiplier < 8; multiplier++)
            {
                actor.Parameter1 = (ushort)((height << 8) | multiplier);
                initialize(actor);
                BoyonEnemyState initialized = enemies.BoyonStates[0]!;
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa286df + multiplier * 2)), initialized.SpeedMultiplier, "Boyon real initialized multiplier without bus");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa286ef + height * 2)), initialized.JumpHeight, "Boyon real initialized height without bus");
            }
        }
        actor.Parameter1 = 8;
        AssertThrows<InvalidDataException>(() => initialize(actor), "Boyon rejected multiplier unchanged");
        actor.Parameter1 = 0x900;
        AssertThrows<InvalidDataException>(() => initialize(actor), "Boyon rejected height unchanged");
        Func<BoyonEnemyState, ushort> multiply = BoyonBehavior.MultiplyBoyonCurveEntry;
        var state = new BoyonEnemyState(new RoomEnemySystem().SlotTable.Slots[0]);
        for (int index = 0; index <= ushort.MaxValue; index++)
        {
            byte sample = index < 23 ? rom.ReadCartridgeByte(0xa28701 + index) : (byte)255;
            AssertEqual(sample, BoyonSpeedDefinitions.Sample((ushort)index), "Boyon native curve/saturation");
            state.SpeedTableIndex = (ushort)index;
            for (int multiplier = 0; multiplier < 256; multiplier++)
            {
                // Exercise every effective hardware input, with nonzero discarded high bytes.
                state.SpeedMultiplier = (ushort)(0xa500 | multiplier);
                AssertEqual((ushort)(sample * multiplier), multiply(state), "Boyon real byte-width multiply without bus");
            }
        }
        Console.WriteLine("Boyon compiled definitions: 72 real initializers, 23 native curve bytes, 65536 index saturation cases and 16777216 real multiplication inputs match without a bus.");
    }
}
