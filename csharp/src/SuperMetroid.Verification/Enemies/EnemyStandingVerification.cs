using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies;

/// <summary>Checks the one test of Samus standing on an enemy that every carrying enemy makes (#2155).</summary>
internal static class EnemyStandingVerification
{
    /// <summary>
    /// The shared standing test against the routine the families each kept a copy of, written as the cartridge writes
    /// it. Every enemy that asks stays in the room, and Samus lies above the room only on a departing up-elevator, where
    /// none of them is placed (#2449), so the enemy centres lie in the room: near its top-left corner, in its middle and
    /// far along it. Samus is placed on every pixel from 40 left to 40 right and from 60 above to 10 below, with ordinary
    /// radii and a radius past every distance in the room, so the horizontal and vertical tangencies and the three-pixel
    /// bias are all crossed; the Kzan's five-pixel bias is compared the same way with the routine its own copy wrote
    /// (#2342). Named cases then pin each boundary.
    /// </summary>
    internal static void VerifySamusStandingOnEnemy()
    {
        RoomEnemySystem enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom());
        RoomEnemyPlace enemy = enemies.Places.InUpdateOrder[0];
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        Extent16<RoomPixels>[] samusRadii = [new(5, 19), new(8, 10)];
        Extent16<RoomPixels>[] enemyRadii = [new(8, 8), new(16, 4), Extent16<RoomPixels>.Zero, new(65530, 65530)];
        int[] centres = [60, 300, 4000];
        int[] biases = [EnemyCollisionDefinitions.SamusStandingBias, SuperMetroid.Core.Enemies.Kzan.KzanMotionDefinitions.SamusStandingBias];
        int cases = 0;
        int standing = 0;
        foreach (int bias in biases)
        foreach (Extent16<RoomPixels> samusRadius in samusRadii)
        foreach (Extent16<RoomPixels> enemyRadius in enemyRadii)
        foreach (int enemyX in centres)
        foreach (int enemyY in centres)
        for (int dx = -40; dx <= 40; dx++)
        for (int dy = -60; dy <= 10; dy++)
        {
            int samusX = enemyX + dx;
            int samusY = enemyY + dy;
            bool expected = CartridgeStandingTest(samusX, samusY, samusRadius, enemyX, enemyY, enemyRadius, bias);
            AssertEqual(expected, Standing(enemy, samus, new(enemyX, enemyY), enemyRadius, new(samusX, samusY), samusRadius, bias),
                $"Samus at ({samusX}, {samusY}) radius {samusRadius} on an enemy at ({enemyX}, {enemyY}) radius {enemyRadius} with bias {bias}");
            cases++;
            if (expected)
                standing++;
        }
        AssertEqual(2 * 2 * 4 * 3 * 3 * 81 * 71, cases, "every placement was compared");
        AssertTrue(standing > 0 && standing < cases, "the placements cover both standing and not standing");

        Extent16<RoomPixels> body = new(5, 19);
        Extent16<RoomPixels> block = new(8, 8);
        Position16<RoomPixels> centre = new(100, 100);
        AssertTrue(Standing(enemy, samus, centre, block, new(112, 80), body), "Samus overlapping the enemy's top stands on it");
        AssertTrue(!Standing(enemy, samus, centre, block, new(113, 80), body), "bodies that only meet side by side do not count");
        AssertTrue(Standing(enemy, samus, centre, block, new(88, 80), body), "the strict overlap holds on the left as on the right");
        AssertTrue(!Standing(enemy, samus, centre, block, new(87, 80), body), "the left tangency does not count either");
        AssertTrue(Standing(enemy, samus, centre, block, new(100, 70), body), "feet meeting the enemy's top, after the bias, count");
        AssertTrue(!Standing(enemy, samus, centre, block, new(100, 69), body), "a pixel higher she is off the enemy");
        AssertTrue(Standing(enemy, samus, centre, block, new(100, 96), body), "a centre four pixels above the enemy's stands");
        AssertTrue(!Standing(enemy, samus, centre, block, new(100, 97), body), "a centre three pixels above the enemy's, the bias, does not");
        int kzan = SuperMetroid.Core.Enemies.Kzan.KzanMotionDefinitions.SamusStandingBias;
        AssertTrue(Standing(enemy, samus, centre, block, new(100, 94), body, kzan), "a centre six pixels above a Kzan's stands on it");
        AssertTrue(!Standing(enemy, samus, centre, block, new(100, 95), body, kzan), "a centre five pixels above a Kzan's, its bias, does not");
        Console.WriteLine($"  Samus standing on an enemy: {cases} placements match the cartridge's routine; tangencies and bias hold.");
    }

    /// <summary>The shared standing test for the given centres and radii.</summary>
    private static bool Standing(RoomEnemyPlace enemy, SamusState samus, Position16<RoomPixels> enemyCentre, Extent16<RoomPixels> enemyRadius,
        Position16<RoomPixels> samusCentre, Extent16<RoomPixels> samusRadius, int bias = EnemyCollisionDefinitions.SamusStandingBias)
    {
        enemy.Position = enemy.Position.WithWhole(enemyCentre);
        enemy.Radius = enemyRadius;
        samus.Position = samus.Position.WithWhole(samusCentre);
        samus.Kinematics.Radius = samusRadius;
        return bias == EnemyCollisionDefinitions.SamusStandingBias
            ? EnemyCollision.IsSamusStandingOn(enemy, samus)
            : EnemyCollision.IsSamusStandingOn(enemy, samus, bias);
    }

    /// <summary>
    /// The cartridge's routine, as each family's copy wrote it before #2155 and the Kzan's before #2342: the wrapped
    /// horizontal distance less Samus's radius compared strictly with the enemy's, then Samus's centre
    /// <paramref name="bias"/> pixels lower, the wrapped difference read as signed, its magnitude less Samus's radius
    /// compared inclusively with the enemy's.
    /// </summary>
    private static bool CartridgeStandingTest(int samusX, int samusY, Extent16<RoomPixels> samusRadius, int enemyX, int enemyY, Extent16<RoomPixels> enemyRadius, int bias)
    {
        int xDistance = Math.Abs((int)unchecked((short)(samusX - enemyX)));
        if (xDistance >= samusRadius.X && unchecked((ushort)(xDistance - samusRadius.X)) >= enemyRadius.X)
            return false;
        int biased = unchecked((ushort)(samusY + bias - enemyY));
        if (unchecked((short)biased) >= 0)
            return false;
        int yDistance = unchecked((ushort)-biased);
        return yDistance < samusRadius.Y || unchecked((ushort)(yDistance - samusRadius.Y)) <= enemyRadius.Y;
    }
}
