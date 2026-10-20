using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Map.Artwork;

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
    internal static void VerifyGameOverMenuNativeIndexes(AreaMapPresentationCatalog presentation)
    {
        var audio = new CartridgeAudioState();
        var menu = new GameOverMenuState(audio, presentation);
        (GameOverMenuPhase Phase, bool Resumes)[] expected =
        [
            (GameOverMenuPhase.ConfigureGraphics, true),
            (GameOverMenuPhase.Initialize, false),
            (GameOverMenuPhase.Initialize, true),
            (GameOverMenuPhase.WaitForInitialMusic, false),
        ];
        AssertEqual(GameOverMenuPhase.ConfigureGraphics, menu.Phase, "game-over menu starts at index zero");
        for (int update = 0; update < expected.Length; update++)
        {
            menu.Step(0);
            AssertEqual(expected[update].Phase, menu.Phase, $"game-over index after update {update}");
            AssertEqual(expected[update].Resumes, menu.ResumesAfterNmiWait, $"game-over NMI wait after update {update}");
        }
        int library3 = (int)SoundEffectLibrary3Sounds.CancelAll.Library - 1;
        byte read = audio.SoundReadPositions[library3];
        AssertEqual(1, audio.SoundWritePositions[library3] - read,
            "game-over index zero queues one library-three request");
        AssertEqual(SoundEffectLibrary3Sounds.CancelAll.Value, audio.SoundQueues[library3, read],
            "game-over index zero queues library-three cancel");
        int? babyStep = menu.BabyStep;
        int babyTimer = menu.BabyInstructionTimer.RawValue;
        int missileTimer = menu.missile.Timer;
        menu.Step(0);
        AssertEqual(GameOverMenuPhase.WaitForInitialMusic, menu.Phase, "music wait holds while the queue is occupied");
        AssertEqual(babyStep, menu.BabyStep, "music wait does not animate the Baby");
        AssertEqual(babyTimer, (int)menu.BabyInstructionTimer.RawValue, "music wait keeps the Baby timer");
        AssertEqual(missileTimer, menu.missile.Timer, "music wait does not draw the missile");

        foreach (bool ceres in new[] { true, false })
        {
            var answered = new GameOverMenuState(new CartridgeAudioState(), presentation, continueLoadsCeresArrival: ceres);
            answered.Phase = GameOverMenuPhase.Main;
            answered.Step(0);
            answered.Step(SnesButton.A);
            AssertEqual(ceres, answered.CeresArrivalRequested, $"Yes on loading state $1F={ceres} requests the Ceres loader");
            AssertEqual(ceres ? GameOverMenuPhase.Main : GameOverMenuPhase.FadeOutToContinue, answered.Phase,
                "only an area-map continue fades out");
        }
    }
}
