using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.GameOver;

namespace SuperMetroid.Verification.Frontend.GameOver;

/// <summary>Checks game-over menu index timing, audio cancellation, and continue dispatch.</summary>
internal static class GameOverMenuIndexesVerification
{
    /// <summary>
    /// #1275: game-over indexes zero ($81:8D0F) and one ($81:91A4) each end in their own NMI
    /// wait, so each spans two updates and the second resumes without a main-loop dispatch.
    /// Index zero cancels library-three sounds; index two neither animates the Baby nor
    /// draws the selection missile. On a Yes answer, loading state $1F publishes the
    /// Ceres loader on the accepting frame; other saves fade into the area map.
    /// </summary>
    internal static void VerifyGameOverMenuNativeIndexes(InstalledScreenContent presentation)
    {
        var audio = AudioQueues.Standalone();
        var menu = new GameOverMenuState(audio, presentation, MenuSheetFixtures.For(presentation), continueLoadsCeresArrival: false);
        GameOverMenuIndexesExpected[] expected =
        [
            new GameOverMenuIndexesExpected(GameOverMenuPhase.BlackHold, true),
            new GameOverMenuIndexesExpected(GameOverMenuPhase.Initialize, false),
            new GameOverMenuIndexesExpected(GameOverMenuPhase.Initialize, true),
            new GameOverMenuIndexesExpected(GameOverMenuPhase.WaitForInitialMusic, false),
        ];
        AssertEqual(GameOverMenuPhase.BlackHold, menu.Phase, "game-over menu starts at index zero");
        for (int update = 0; update < expected.Length; update++)
        {
            menu.Step(0);
            AssertEqual(expected[update].Phase, menu.Phase, $"game-over index after update {update}");
            AssertEqual(expected[update].Resumes, menu.IsWaitingOutDisplayFrame, $"game-over NMI wait after update {update}");
        }
        IReadOnlyList<SoundEffectId> library3 = audio.WaitingSounds(SoundEffectLibrary.Room);
        AssertEqual(1, library3.Count,
            "game-over index zero queues one library-three request");
        AssertEqual<SoundEffectId>(RoomSound.Silence, library3[0],
            "game-over index zero queues library-three cancel");
        AnimationPlace? babyPlace = menu.BabyPlace;
        int babyTimer = menu.BabyFrameHold;
        MenuMissileFrame missileFrame = menu.missile.Frame;
        int missileUpdatesLeft = menu.missile.UpdatesLeft;
        menu.Step(0);
        AssertEqual(GameOverMenuPhase.WaitForInitialMusic, menu.Phase, "music wait holds while the queue is occupied");
        AssertEqual(babyPlace, menu.BabyPlace, "music wait does not animate the Baby");
        AssertEqual(babyTimer, menu.BabyFrameHold, "music wait keeps the Baby timer");
        AssertEqual(missileFrame, menu.missile.Frame, "music wait does not draw the missile");
        AssertEqual(missileUpdatesLeft, menu.missile.UpdatesLeft, "music wait keeps the missile's hold");

        foreach (bool ceres in new[] { true, false })
        {
            var answered = new GameOverMenuState(AudioQueues.Standalone(), presentation, MenuSheetFixtures.For(presentation), continueLoadsCeresArrival: ceres);
            answered.SkipToPhase(GameOverMenuPhase.Main);
            answered.Step(0);
            answered.Step(ControllerButton.A);
            AssertEqual(ceres, answered.CeresArrivalRequested, $"Yes on loading state $1F={ceres} requests the Ceres loader");
            AssertEqual(ceres ? GameOverMenuPhase.Main : GameOverMenuPhase.FadeOutToContinue, answered.Phase,
                "only an area-map continue fades out");
        }
    }
}
