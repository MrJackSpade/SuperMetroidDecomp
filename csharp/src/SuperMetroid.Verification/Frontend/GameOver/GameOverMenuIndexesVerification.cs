using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.GameOver;

namespace SuperMetroid.Verification.Frontend.GameOver;

/// <summary>Checks game-over menu index timing, audio cancellation, and continue dispatch.</summary>
internal static class GameOverMenuIndexesVerification
{
    /// <summary>Updates one menu is held in its music wait beyond the other: more than any look of the Baby or the missile lasts.</summary>
    private const int HeldMusicWaitUpdates = 30;

    /// <summary>Updates the two menus are compared after both leave the music wait: past several looks and cries of the Baby.</summary>
    private const int ComparedUpdatesAfterMusicWait = 400;

    /// <summary>Plays <paramref name="audio"/>'s queued music until none is left, as the menu's music wait waits for.</summary>
    private static void DrainMusic(AudioQueues audio)
    {
        for (int frame = 0; audio.HasQueuedMusic; frame++)
        {
            AssertTrue(frame < 1000, "the game-over menu's queued music plays out");
            _ = audio.AdvanceFrame(default);
        }
    }

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
        AssertEqual<SoundEffectId>(RoomSoundStop.Any, library3[0],
            "game-over index zero queues library-three cancel");
        // A menu released from the music wait at once shows the Baby and the missile exactly as one held in it for a
        // while (#2488): the wait neither animates the Baby nor draws the missile, and leaves both their holds as they were.
        var promptAudio = AudioQueues.Standalone();
        var prompt = new GameOverMenuState(promptAudio, presentation, MenuSheetFixtures.For(presentation), continueLoadsCeresArrival: false);
        for (int update = 0; update < expected.Length; update++)
            prompt.Step(0);
        GameOverBabyLook babyLook = menu.BabyLook;
        MenuMissileFrame missileFrame = menu.missile.Frame;
        for (int update = 0; update < HeldMusicWaitUpdates; update++)
        {
            menu.Step(0);
            AssertEqual(GameOverMenuPhase.WaitForInitialMusic, menu.Phase, "music wait holds while the queue is occupied");
            AssertEqual(babyLook, menu.BabyLook, $"music wait does not animate the Baby, update {update}");
            AssertEqual(missileFrame, menu.missile.Frame, $"music wait does not draw the missile, update {update}");
        }
        AssertEqual(1, audio.WaitingSounds(SoundEffectLibrary.Room).Count, "music wait queues no cry");
        DrainMusic(audio);
        DrainMusic(promptAudio);
        for (int update = 0; update < ComparedUpdatesAfterMusicWait; update++)
        {
            menu.Step(0);
            prompt.Step(0);
            AssertEqual(prompt.Phase, menu.Phase, $"held and prompt menus share their step {update} updates after the wait");
            AssertEqual(prompt.BabyLook, menu.BabyLook, $"the held music wait keeps the Baby's hold, update {update} after it");
            AssertEqual(prompt.missile.Frame, menu.missile.Frame, $"the held music wait keeps the missile's hold, update {update} after it");
        }

        foreach (bool ceres in new[] { true, false })
        {
            AudioQueues answeredAudio = AudioQueues.Standalone();
            var answered = new GameOverMenuState(answeredAudio, presentation, MenuSheetFixtures.For(presentation), continueLoadsCeresArrival: ceres);
            answered.RunOpening(answeredAudio);
            answered.Step(0);
            answered.Step(ControllerButton.A);
            AssertEqual(ceres, (answered.Outcome == GameOverMenuOutcome.CeresArrival), $"Yes on loading state $1F={ceres} requests the Ceres loader");
            AssertEqual(ceres ? GameOverMenuPhase.Main : GameOverMenuPhase.FadeOutToContinue, answered.Phase,
                "only an area-map continue fades out");
        }
    }
}
