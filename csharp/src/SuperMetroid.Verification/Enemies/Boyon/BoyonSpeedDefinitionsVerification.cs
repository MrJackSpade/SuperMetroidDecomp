using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

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
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        Action<RoomEnemyPlace> initialize = BoyonBehavior.InitializeBoyon;
        RoomEnemyPlace actor = enemies.Places.InUpdateOrder[0];
        actor.Bind(EnemyDefinitionId.Boyon, actor.Definition);
        int initialized = 0;
        for (int height = 0; height < 9; height++)
        {
            for (int multiplier = 0; multiplier < 8; multiplier++)
            {
                actor.SetSpawnParameter1(EnemyDefinitionId.Boyon, (ushort)((height << 8) | multiplier));
                int authoredHeight = CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa286ef + height * 2));
                initialize(actor);
                initialized++;
                BoyonEnemyState state = (enemies.BoyonStates[0] ?? throw new InvalidOperationException("Verification requires enemies.BoyonStates[0]."));
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((0xa286df + multiplier * 2)), state.SpeedMultiplier, "Boyon real initialized multiplier without bus");
                AssertEqual(authoredHeight, state.JumpHeight.InTwoFiftySixths, "Boyon real initialized height without bus");
            }
        }
        AssertEqual(72, initialized, "every height selector initializes with each of the eight multipliers (#2282)");
        AssertThrows<InvalidDataException>(() => actor.SetSpawnParameter1(EnemyDefinitionId.Boyon, 8), "Boyon import rejects the multiplier past its table");
        AssertThrows<InvalidDataException>(() => actor.SetSpawnParameter1(EnemyDefinitionId.Boyon, 0x900), "Boyon import rejects the height past its table");
        Func<BoyonEnemyState, UnsignedSixteenSixteen> multiply = BoyonBehavior.MultiplyBoyonCurveEntry;
        var curve = new BoyonEnemyState();
        for (int index = 0; index <= ushort.MaxValue; index++)
        {
            byte sample = index < 23 ? rom.ReadCartridgeByte(0xa28701 + index) : (byte)255;
            AssertEqual(sample, BoyonBounceCurve.Sample(BoyonCurvePlaceConstants.AtEntry(index)), "Boyon native curve/saturation");
            curve.CurvePlace = BoyonCurvePlaceConstants.AtEntry(index);
            for (int multiplier = 1; multiplier <= 128; multiplier++)
            {
                // Exercise every multiplier a Boyon takes (#2066).
                curve.SpeedMultiplier = multiplier;
                AssertEqual(UnsignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel((ushort)(sample * multiplier)), multiply(curve), "Boyon real multiply without bus");
            }
        }
        AssertThrows<InvalidOperationException>(() => BoyonBounceCurve.Sample(BoyonCurvePlaceConstants.PastTheTop), "no sample is read past the top of the arc");
        Console.WriteLine("Boyon compiled definitions: 48 real initializers, 3 refused heights, 23 native curve bytes, 65536 entry saturation cases and 8388608 real multiplication inputs match without a bus.");
    }
}
