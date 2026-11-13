using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Checks the hold a damage-reaction mouth shot adds to Crocomire's animation frame (#1632): eight frames in every fight
/// function, the value the game kept for the projectile attack and for every other function alike, and nothing while a
/// reaction is already pending.
/// </summary>
internal static class CrocomireMouthShotHoldVerification
{
    /// <summary>The frames the body's hold starts at before each shot.</summary>
    private const int StartingHold = 5;

    /// <summary>Shoots the mouth from off screen and with an on-screen Super Missile in every fight function.</summary>
    internal static void VerifyCrocomireMouthShotHold()
    {
        int expected = WrappingCountdown.Start(StartingHold).AddFrames(8).RemainingFrames;
        int cases = 0;
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(),
            definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        body.Bind(EnemyDefinitionId.Crocomire, body.Definition);
        body.Position = body.Position.WithWhole(new(576, 144));
        foreach (CrocomireFightFunction function in Enum.GetValues<CrocomireFightFunction>())
        {
            foreach ((string place, int cameraX) in new[] { ("off screen", 0), ("on screen", 400) })
            {
                var state = new CrocomireEnemyState(body) { FightFunction = function, Camera = new(cameraX, 0) };
                enemies.CrocomireBehavior.Access.Crocomire = state;
                body.AnimationClock.HoldFor(StartingHold);
                body.FlashTimer = 0;

                SamusProjectileType superMissile = SamusProjectileType.Of(SamusProjectileFamily.SuperMissile);
                enemies.CrocomireBehavior.Combat.ResolveCrocomireMouthShot(state, body, superMissile, body.Position.Whole);
                AssertEqual(expected, body.AnimationClock.Hold.RemainingFrames, $"a {place} mouth shot during {function} holds the frame eight more");

                enemies.CrocomireBehavior.Combat.ResolveCrocomireMouthShot(state, body, superMissile, body.Position.Whole);
                AssertEqual(expected, body.AnimationClock.Hold.RemainingFrames, $"a {place} mouth shot during {function}'s pending reaction adds no hold");
                cases++;
            }
        }
        Console.WriteLine($"Crocomire mouth shot hold: {cases} shots across every fight function add eight frames once per reaction.");
    }
}
