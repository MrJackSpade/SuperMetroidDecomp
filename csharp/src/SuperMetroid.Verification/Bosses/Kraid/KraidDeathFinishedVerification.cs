using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>
/// Checks Kraid's one finished death phase (#2464): the cartridge ran it as two identical routines, for a Kraid it found
/// alive and one it found already defeated, and both had already recorded him defeated, so whether his body takes a dead
/// enemy's properties depends only on whether the camera has left the room's left edge.
/// </summary>
internal static class KraidDeathFinishedVerification
{
    /// <summary>
    /// Runs the finished phase with the camera at the room's left edge, which leaves his body as it is, and just right of it,
    /// which gives his body a dead enemy's properties; the phase stays finished either way.
    /// </summary>
    internal static void VerifyKraidDeathFinished()
    {
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, services: new VerificationRoomEnemyServices(null));
        RoomEnemyPlace body = enemies.Places.InUpdateOrder[0];
        enemies.Places.BindKraidPartsInRoomOrder();
        // His setup reads whether his room's boss is defeated, which this room's is not, and writes the screen
        // scrolls his fight frames, which this room takes.
        enemies.VerificationServices().IsAreaBossDefeated ??= () => false;
        enemies.VerificationServices().SetRoomScrollState ??= (_, _) => { };
        KraidEnemyState state = enemies.Places.SetUpKraidInRoomOrder();
        state.BodyState.SunkPhase = KraidSunkPhase.Finished;
        body.Presence = body.Presence.Without(EnemyPresence.Deleted | EnemyPresence.Hidden);

        enemies.KraidBehavior.Death.RunKraidSunkPhase(body, state, (KraidStageRun.Sunk)state.BodyState.Run, cameraX: 0);
        AssertTrue(!body.Presence.HasAny(EnemyPresence.Deleted | EnemyPresence.Hidden),
            "A finished Kraid's body stays while the camera is at the room's left edge");
        AssertEqual(KraidSunkPhase.Finished, state.BodyState.SunkPhase, "The finished phase stays finished at the left edge");

        enemies.KraidBehavior.Death.RunKraidSunkPhase(body, state, (KraidStageRun.Sunk)state.BodyState.Run, cameraX: 1);
        AssertTrue(body.Presence.HasAny(EnemyPresence.Deleted) && body.Presence.HasAny(EnemyPresence.Hidden),
            "A finished Kraid's body is deleted and hidden once the camera leaves the room's left edge");
        AssertTrue(body.Contact.HasAny(EnemyContact.IgnoresSamus), "His dead body ignores Samus");
        AssertEqual(KraidSunkPhase.Finished, state.BodyState.SunkPhase, "The finished phase stays finished");
        Console.WriteLine("Kraid's finished death: his body stays at the room's left edge and is given a dead enemy's properties past it.");
    }

    /// <summary>Runs the finished death phase check.</summary>
    [VerificationCommand("--kraid-death-finished")]
    internal static int KraidDeathFinishedCommand()
    {
        Suite(nameof(VerifyKraidDeathFinished), VerifyKraidDeathFinished);
        return 0;
    }
}
