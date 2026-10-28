using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Boyon;

/// <summary>Checks Boyon jump height, speed multiplier, and saturation curve
/// against native data through production initialization and multiplication.</summary>
internal static class BoyonSpeedDefinitionsVerification
{
    /// <summary>Compares all authored initializer rows and effective byte-width
    /// multipliers, including out-of-range inputs and saturated curve indexes.</summary>
    internal static void VerifyCompiledBoyonSpeeds(SuperMetroidAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(new EnemySession());
        Action<RoomEnemySlot> initialize = BoyonBehavior.InitializeBoyon;
        RoomEnemySlot actor = enemies.SlotTable.Slots[0];
        actor.Bind(EnemyDefinitionId.Boyon, actor.Definition);
        for (int height = 0; height < 9; height++)
        {
            for (int multiplier = 0; multiplier < 8; multiplier++)
            {
                actor.SetSpawnParameter1((ushort)((height << 8) | multiplier));
                initialize(actor);
                BoyonEnemyState initialized = (enemies.BoyonStates[0] ?? throw new InvalidOperationException("Verification requires enemies.BoyonStates[0]."));
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa286df + multiplier * 2)), initialized.SpeedMultiplier, "Boyon real initialized multiplier without bus");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa286ef + height * 2)), initialized.JumpHeight, "Boyon real initialized height without bus");
            }
        }
        actor.SetSpawnParameter1(8);
        AssertThrows<InvalidDataException>(() => initialize(actor), "Boyon rejected multiplier unchanged");
        actor.SetSpawnParameter1(0x900);
        AssertThrows<InvalidDataException>(() => initialize(actor), "Boyon rejected height unchanged");
        Func<BoyonEnemyState, int> multiply = BoyonBehavior.MultiplyBoyonCurveEntry;
        var state = new BoyonEnemyState();
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
