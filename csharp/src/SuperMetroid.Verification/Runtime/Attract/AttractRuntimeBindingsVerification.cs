using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using static SuperMetroid.Verification.Enemies.EnemyContentIdentityFixtureVerification;
using SuperMetroid.Core;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Runtime.Attract;
using SuperMetroid.Verification.Harness;

/// <summary>Checks installed artwork, random state, and cheat-option ownership in attract and selected play.</summary>
internal static class AttractRuntimeBindingsVerification
{
    /// <summary>
    /// Verifies cartridge-free attract runtime bindings. The runtime binds no base palette (#1512): its pictures are
    /// pre-coloured by the import, so the #1171 missing-palette construction failure no longer has a binding to omit.
    /// </summary>
    internal static void VerifyAttractRuntimeBindings()
    {
        var enemyArtwork = new EnemyIdentityFixture().Build();
        var game = new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), new GameplayAssists
            { SamusCannotDie = true, AmmoNeverRunsOut = true, EscapeTimerHoldsAtOneSecond = true });
        game.BindArtwork(game.Artwork with
        {
            SamusOverreads = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusOverreads,
            BossAndIntroOverreads = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.BossAndIntroOverreads,
            EnemyOverreads = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EnemyOverreads,
            EnemyFrameDisplays = enemyArtwork.Enemies.FrameDisplays ?? throw new InvalidOperationException("The fixture authors enemy frames."),
            EnemySheets = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EnemySheets, KraidEyes = enemyArtwork.Bosses.KraidColors?.Eyes(), 
        });
        int incomingRandom = game.DispatcherRandomNumber;
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.AttractDemo);
        var demo = game.GameplayRuntimeForVerification;
        AssertTrue(ReferenceEquals(enemyArtwork.Enemies.FrameDisplays, demo.EnemySession.FrameDisplays),
            "attract runtime binds the installed extended frame displays");
        AssertEqual(incomingRandom, demo.System.RandomNumber, "attract runtime preserves frontend random state");
        AssertTrue(ReferenceEquals(demo.Assists.Current, GameplayAssists.None),
            "attract runtime plays unassisted");
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        var selected = game.GameplayRuntimeForVerification;
        AssertTrue(!ReferenceEquals(demo, selected), "selected play and attract playback own separate runtimes");
        AssertTrue(selected.Assists.Current is { SamusCannotDie: true, AmmoNeverRunsOut: true, EscapeTimerHoldsAtOneSecond: true },
            "selected runtime plays under the session's assists");

        // #1330: one random-number word per session. Each runtime reads and advances the session's instance, so a
        // release to the menu and a recreation keep the exact word with no copy.
        AssertTrue(ReferenceEquals(game.Session.Random, demo.System.Random) && ReferenceEquals(game.Session.Random, selected.System.Random),
            "every runtime shares the session's random-number word");
        selected.System.NextRandom();
        int beforeRelease = game.Session.Random.Read();
        game.Session.ReleaseRuntime();
        AssertEqual(beforeRelease, game.DispatcherRandomNumber, "release keeps the word gameplay advanced");
        int menuFrame = game.Session.Random.Generate();
        game.Session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        AssertTrue(ReferenceEquals(game.Session.Random, game.GameplayRuntimeForVerification.System.Random),
            "a recreated runtime shares the session's word");
        AssertEqual(menuFrame, game.GameplayRuntimeForVerification.System.RandomNumber, "a recreated runtime starts from the menu's word");
        // #1776: the camera fractions a game leaves are the session's, so the next game's first room starts from them.
        AssertTrue(ReferenceEquals(game.Session.cameraFractions, demo.CameraFractions) &&
            ReferenceEquals(game.Session.cameraFractions, selected.CameraFractions) &&
            ReferenceEquals(game.Session.cameraFractions, game.GameplayRuntimeForVerification.CameraFractions),
            "every runtime shares the session's camera fractions kept across games");
        Console.WriteLine("Attract runtime bindings: shared construction loads installed artwork without ROM, shares the session's RNG word across runtimes, and isolates demo options.");
    }

    /// <summary>Runs the focused attract runtime binding verification command.</summary>
    [VerificationCommand("--attract-runtime-bindings")]
    internal static int AttractRuntimeBindingsCommand()
    {
        Suite(nameof(VerifyAttractRuntimeBindings), () => VerifyAttractRuntimeBindings());
        return 0;
    }
}
