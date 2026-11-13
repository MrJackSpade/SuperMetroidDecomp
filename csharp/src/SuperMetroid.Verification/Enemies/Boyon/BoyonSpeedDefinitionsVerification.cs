using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Enemies.Boyon;

/// <summary>Checks Boyon jump height, speed multiplier, and saturation curve
/// against native data through production initialization and multiplication.</summary>
internal static class BoyonSpeedDefinitionsVerification
{
    /// <summary>
    /// Compares every authored initializer row a Boyon bounces with (heights up to 128 pixels; the three higher
    /// selectors, which no retail Boyon uses, are refused) and effective byte-width multipliers, including out-of-range
    /// inputs and saturated curve entries.
    /// </summary>
    internal static void VerifyCompiledBoyonSpeeds(CartridgeImportAddressSpace rom)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        Action<RoomEnemySlot> initialize = BoyonBehavior.InitializeBoyon;
        RoomEnemySlot actor = enemies.SlotTable.Slots[0];
        actor.Bind(EnemyDefinitionId.Boyon, actor.Definition);
        int initialized = 0;
        for (int height = 0; height < 9; height++)
        {
            for (int multiplier = 0; multiplier < 8; multiplier++)
            {
                actor.SetSpawnParameter1(EnemyDefinitionId.Boyon, (ushort)((height << 8) | multiplier));
                int authoredHeight = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa286ef + height * 2));
                if (authoredHeight > BoyonSpeedDefinitions.LargestJumpHeight * 256)
                {
                    AssertThrows<ArgumentOutOfRangeException>(() => initialize(actor), $"Boyon height selector {height}, above 128 pixels, is refused");
                    continue;
                }
                initialize(actor);
                initialized++;
                BoyonEnemyState state = (enemies.BoyonStates[0] ?? throw new InvalidOperationException("Verification requires enemies.BoyonStates[0]."));
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa286df + multiplier * 2)), state.SpeedMultiplier, "Boyon real initialized multiplier without bus");
                AssertEqual(authoredHeight, state.JumpHeight.InTwoFiftySixths, "Boyon real initialized height without bus");
            }
        }
        AssertEqual(48, initialized, "the six selectors up to 128 pixels initialize with each of the eight multipliers");
        AssertThrows<InvalidDataException>(() => actor.SetSpawnParameter1(EnemyDefinitionId.Boyon, 8), "Boyon import rejects the multiplier past its table");
        AssertThrows<InvalidDataException>(() => actor.SetSpawnParameter1(EnemyDefinitionId.Boyon, 0x900), "Boyon import rejects the height past its table");
        Func<BoyonEnemyState, int> multiply = BoyonBehavior.MultiplyBoyonCurveEntry;
        var curve = new BoyonEnemyState();
        for (int index = 0; index <= ushort.MaxValue; index++)
        {
            byte sample = index < 23 ? rom.ReadCartridgeByte(0xa28701 + index) : (byte)255;
            AssertEqual(sample, BoyonBounceCurve.Sample(BoyonCurvePlace.AtEntry(index)), "Boyon native curve/saturation");
            curve.CurvePlace = BoyonCurvePlace.AtEntry(index);
            for (int multiplier = 0; multiplier < 256; multiplier++)
            {
                // Exercise every multiplier the multiply takes.
                curve.SpeedMultiplier = multiplier;
                AssertEqual((ushort)(sample * multiplier), multiply(curve), "Boyon real byte-width multiply without bus");
            }
        }
        AssertThrows<InvalidOperationException>(() => BoyonBounceCurve.Sample(BoyonCurvePlace.PastTheTop), "no sample is read past the top of the arc");
        Console.WriteLine("Boyon compiled definitions: 48 real initializers, 3 refused heights, 23 native curve bytes, 65536 entry saturation cases and 16777216 real multiplication inputs match without a bus.");
    }
}
