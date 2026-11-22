using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Polyp;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Projectiles;

/// <summary>Checks the Polyp state the species owns: its initial behaviour.</summary>
internal static class PolypOwnedStateVerification
{
    /// <summary>
    /// Initializes a Polyp and confirms it starts waiting for Samus with no cooldown.
    /// </summary>
    internal static void VerifyPolypOwnedState()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        enemies.VerificationServices().SeedRandom = _ => { };
        // The rock's launch draws the cooldown that follows it.
        enemies.VerificationServices().NextRandom = () => 0;
        RoomEnemyPlace place = enemies.Places.InUpdateOrder[0];
        // The Polyp is placed and set up as its room's population places it, so its entity runs its main AI.
        _ = enemies.PlaceAs<PolypEntity>(EnemyDefinitionId.Polyp, EnemySpawnSettings.None, RoomEnemyFrameFixtures.StandaloneSamus(), new(128, 128));
        // A new Polyp waits for Samus with no cooldown: Samus beside it sets it shooting, which it signals for one update
        // and then launches its rock.
        SamusState samus = RoomEnemyFrameFixtures.StandaloneSamus();
        samus.Position = place.Position;
        int Rocks() => enemies.Projectiles.EnemyProjectiles.Count(rock => rock.IsActive && rock.Kind == RoomEnemyProjectileKind.PolypRock);
        place.RunMainAi(samus);
        AssertEqual(0, Rocks(), "a new Polyp waits for Samus, and signals for one update before it shoots");
        place.RunMainAi(samus);
        AssertEqual(1, Rocks(), "a new Polyp has no cooldown, so it launches its rock on the next update");
        Console.WriteLine("Polyp owned state: the initial behaviour matches.");
    }

    /// <summary>Runs the focused Polyp owned-state check.</summary>
    [VerificationCommand("--polyp-owned-state")]
    internal static int PolypOwnedStateCommand()
    {
        VerifyPolypOwnedState();
        return 0;
    }
}
