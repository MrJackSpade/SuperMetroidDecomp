using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;

/// <summary>Checks which enemy the Baby Metroid feeds on (#1542).</summary>
internal static class BabyMetroidVictimVerification
{
    /// <summary>
    /// Confirms <see cref="BabyMetroidVictim"/> gives the enemy placed directly after the Baby Metroid, wherever the Baby
    /// is placed, and refuses a Baby in the final place, after which no enemy is placed.
    /// </summary>
    internal static void VerifyBabyMetroidVictim()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        IReadOnlyList<RoomEnemyPlace> places = enemies.Places.InUpdateOrder;
        AssertTrue(ReferenceEquals(places[1], BabyMetroidVictim.Of(places, places[0])), "the retail Baby's victim is the enemy placed after it");
        AssertTrue(ReferenceEquals(places[6], BabyMetroidVictim.Of(places, places[5])), "a Baby placed later feeds on the enemy placed after it");
        int last = places.Count - 1;
        AssertTrue(ReferenceEquals(places[last], BabyMetroidVictim.Of(places, places[last - 1])), "the victim may be the final enemy");
        AssertThrows<ArgumentOutOfRangeException>(() => BabyMetroidVictim.Of(places, places[last]), "a Baby in the final place has no victim");
        AssertThrows<ArgumentOutOfRangeException>(() => BabyMetroidVictim.Of(places, StrayPlace()), "a Baby in no place is refused");
        AssertTrue(BabyMetroidVictim.FeederOf(places, places[0]) is null, "the enemy placed first has no enemy feeding on it");
        AssertTrue(ReferenceEquals(places[5], BabyMetroidVictim.FeederOf(places, places[6])), "a victim's feeder is the enemy placed before it");
        AssertThrows<ArgumentOutOfRangeException>(() => BabyMetroidVictim.FeederOf(places, StrayPlace()), "a victim in no place is refused");
        Console.WriteLine("Baby Metroid victim: the enemy placed directly after the Baby, and none past the last.");
    }

    /// <summary>A place of another room's enemies, which is none of the checked room's places.</summary>
    private static RoomEnemyPlace StrayPlace() =>
        EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance).Places.InUpdateOrder[0];

    /// <summary>
    /// Confirms the victim's question about its Baby Metroid's feeding (#2030): each Baby Metroid owns its state, and a victim asks
    /// the Baby Metroid placed directly before it, which has fed exactly when it started dormant because Samus entered from the
    /// left. A victim with no Baby Metroid before it has not been fed on.
    /// </summary>
    internal static void VerifyBabyMetroidFeeding()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        IReadOnlyList<RoomEnemyPlace> places = enemies.Places.InUpdateOrder;
        var feeding = new BabyMetroidFeeding(enemies.Places);
        // Entering from the left door, the camera still lies left of the room's edge as the room's enemies are placed.
        int leftDoorCameraX = CoordinateAxis.Wrap(-16);
        BabyMetroidBehavior.InitializeBabyMetroid(places[0], 0);
        BabyMetroidBehavior.InitializeBabyMetroid(places[5], leftDoorCameraX);
        AssertEqual(BabyMetroidAction.WaitForCamera, places[0].RequireSpeciesState<BabyMetroidEnemyState>().Action, "entering from the right leaves the first Shitroid waiting");
        AssertEqual(BabyMetroidAction.Dormant, places[5].RequireSpeciesState<BabyMetroidEnemyState>().Action, "entering from the left leaves the second Shitroid dormant");
        AssertTrue(!feeding.HasAlreadyFedOn(places[1]), "the victim after a waiting Shitroid is still there");
        AssertTrue(feeding.HasAlreadyFedOn(places[6]), "the victim after a dormant Shitroid is gone");
        AssertTrue(!feeding.HasAlreadyFedOn(places[0]), "the enemy placed first has no Shitroid feeding on it");
        AssertTrue(!feeding.HasAlreadyFedOn(places[3]), "an enemy placed after no Shitroid has not been fed on");
        Console.WriteLine("Shitroid feeding: each victim asks the Shitroid placed before it, which has fed when it started dormant.");
    }

    /// <summary>Runs the focused Baby Metroid feeding check.</summary>
    [VerificationCommand("--shitroid-feeding")]
    internal static int BabyMetroidFeedingCommand()
    {
        VerifyBabyMetroidFeeding();
        return 0;
    }

    /// <summary>Runs the focused Baby Metroid victim check.</summary>
    [VerificationCommand("--baby-metroid-victim")]
    internal static int BabyMetroidVictimCommand()
    {
        VerifyBabyMetroidVictim();
        return 0;
    }
}
