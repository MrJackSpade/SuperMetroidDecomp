using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;

using SuperMetroid.Verification.Harness;

/// <summary>Checks which enemy a shot on a Zebetite shares its health and flash with (#1542).</summary>
internal static class ZebetiteShotShareVerification
{
    /// <summary>
    /// Confirms <see cref="ZebetiteShotShare"/> gives a linked Zebetite's other half, and a Zebetite with no linked half
    /// the first enemy placed in its room.
    /// </summary>
    internal static void VerifyZebetiteShotShare()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        IReadOnlyList<RoomEnemyPlace> places = enemies.Places.InUpdateOrder;
        AssertTrue(ReferenceEquals(places[4], ZebetiteShotShare.Recipient(places[0], places[4])), "a linked Zebetite shares with its other half");
        AssertTrue(ReferenceEquals(places[0], ZebetiteShotShare.Recipient<RoomEnemyPlace>(places[0], null)), "a single Zebetite shares with the first enemy placed");
        Console.WriteLine("Zebetite shot share: a linked half, or the first enemy placed for a single Zebetite.");
    }

    /// <summary>Runs the focused Zebetite shot-share check.</summary>
    [VerificationCommand("--zebetite-shot-share")]
    internal static int ZebetiteShotShareCommand()
    {
        VerifyZebetiteShotShare();
        return 0;
    }
}
