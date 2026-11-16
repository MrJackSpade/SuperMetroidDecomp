using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.FallingSparks;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.FallingSparks;

/// <summary>#2064: a falling spark's floor probe creeps it by its whole fall speed read as 256ths of a pixel.</summary>
internal static class FallingSparkFloorProbeVerification
{
    /// <summary>
    /// The probe creeps a spark a 256th of a pixel for each whole pixel of its fall speed, whatever its fraction, and has
    /// no creep for a rising spark. In an open room the production fall moves a spark by that creep and then by its own
    /// speed after gravity: from every whole speed zero through four, with and without a fraction, the spark's new height
    /// is its old one plus both.
    /// </summary>
    internal static void VerifyFallingSparkFloorProbe()
    {
        var probe = new FallingSparkFloorProbe();
        AssertThrows<ArgumentOutOfRangeException>(() => probe.CreepFor(SignedSixteenSixteen.FromParts(-1, 32768)), "a rising spark has no creep");

        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().NextRandom = () => 0;
        RoomEnemyPlace source = enemies.Places.InUpdateOrder[0];
        RoomLevelData level = RoomEnemyFrameFixtures.OpenRoom(new(64, 64));
        int cases = 0;
        for (int whole = 0; whole <= 4; whole++)
        foreach (int fraction in new[] { 0, 49152 })
        {
            SignedSixteenSixteen speed = SignedSixteenSixteen.FromParts(whole, fraction);
            AssertEqual(whole, probe.CreepFor(speed).InTwoHundredFiftySixthsOfPixel, $"a spark falling at {whole} pixels and {fraction}/65536 creeps {whole} 256ths");

            foreach (EnemyProjectile old in enemies.Projectiles.EnemyProjectiles)
                old.Delete();
            source.Position = source.Position.WithWhole(new(256, 96));
            enemies.FallingSparksBehavior.SpawnFallingSpark(source);
            EnemyProjectile spark = enemies.Projectiles.EnemyProjectiles.Single(candidate => candidate.Kind == RoomEnemyProjectileKind.FallingSpark);
            FallingSparkState state = spark.StateOf<FallingSparkState>();
            state.VerticalSpeed = speed;
            FixedCoordinate before = spark.Position.Y;

            enemies.FallingSparksBehavior.FallFallingSpark(spark, level, frameCycle: 1);

            FixedCoordinate expected = before
                .MovedBy(SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(whole))
                .MovedBy(state.VerticalSpeed);
            AssertEqual(expected, spark.Position.Y, $"a spark falling at {whole} pixels and {fraction}/65536 creeps, then falls at its new speed");
            cases++;
        }
        Console.WriteLine($"  Falling spark floor probe: {cases} falls creep by their whole speed in 256ths before falling.");
    }
}
