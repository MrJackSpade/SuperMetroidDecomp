using SuperMetroid.Core.Compatibility;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Checks the shared enemy arithmetic's own bounds (#1627): a sine velocity is formed from whole speeds up to 127 pixels per
/// update and no faster, and a projectile scatter's centre lies among its random field's values, the farthest of them
/// accepted and one past it rejected before any random draw.
/// </summary>
internal static class EnemyMathBoundsVerification
{
    /// <summary>Runs the sine-speed and scatter-centre bound checks.</summary>
    internal static void VerifyEnemyMathBounds()
    {
        for (int angle = 0; angle < 256; angle++)
            _ = EnemyMath.SineVelocity(new TrigonometryAngle(angle), 127);
        AssertThrows<ArgumentOutOfRangeException>(() => EnemyMath.SineVelocity(new TrigonometryAngle(0), 128),
            "a sine velocity faster than the original's signed whole speed is rejected");

        var services = new VerificationRoomEnemyServices(null) { NextRandom = () => ushort.MaxValue, ReadRandom = () => ushort.MaxValue };
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), services: services);
        var projectile = new EnemyProjectile(0, default)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        };
        var field = new RandomSampleField(0, 4);
        // Every draw reads the field's last value, so a centre there scatters by nothing and the redraw ends at once.
        enemies.Projectiles.Scatter(projectile, new EnemyProjectileScatter(field, field, new(field.Outcomes - 1, field.Outcomes - 1)));
        AssertEqual(new Position16<RoomPixels>(128, 128), projectile.Position.Whole, "a scatter centred on its field's last value moves nothing");
        AssertThrows<ArgumentOutOfRangeException>(
            () => enemies.Projectiles.Scatter(projectile, new EnemyProjectileScatter(field, field, new(field.Outcomes, 0))),
            "a scatter centred past its field's values, whose redraw could never end, is rejected");
        AssertThrows<ArgumentOutOfRangeException>(
            () => enemies.Projectiles.Scatter(projectile, new EnemyProjectileScatter(field, field, new(0, field.Outcomes))),
            "a scatter centred past its vertical field's values is rejected");
        Console.WriteLine("Enemy math bounds: sine speeds up to 127 pixels, and scatter centres within their fields, pass.");
    }
}
