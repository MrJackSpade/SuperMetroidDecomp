using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;

/// <summary>Shared controller-only navigation for private-ROM frontend regressions.</summary>
internal static class FrontendAuditDriver
{
    /// <summary>
    /// Drives a fresh outer dispatcher into its currently selected, existing save slot.
    /// Every transition remains controller-authored so stale menu flags and edge mistakes
    /// cannot be hidden by calling a room initializer directly.
    /// </summary>
    public static FrontendFrame EnterSelectedSlot(SuperMetroidGame game)
    {
        FrontendFrame frame = game.Step(0);
        frame = game.Step((ushort)SnesButton.Start);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.Phase == nameof(TitleSequencePhase.TitleScreen),
            maximumFrames: 120,
            "restarted title montage skip did not reach the title screen");

        frame = game.Step((ushort)SnesButton.Start);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.FileSelectMenus,
            maximumFrames: 120,
            "restarted title screen did not reach file select");
        for (int frameIndex = 0; frameIndex < 16; frameIndex++)
            frame = game.Step(0);

        frame = game.Step((ushort)SnesButton.A);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.GameOptionsMenu,
            maximumFrames: 180,
            "restarted existing slot did not reach game options");
        for (int frameIndex = 0; frameIndex < 16; frameIndex++)
            frame = game.Step(0);

        frame = game.Step((ushort)SnesButton.A);
        frame = StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.SetUpNewGame,
            maximumFrames: 180,
            "restarted options did not request saved-game setup");
        return game.Step(0);
    }

    public static FrontendFrame StepUntil(
        SuperMetroidGame game,
        FrontendFrame initialFrame,
        Func<FrontendFrame, bool> finished,
        int maximumFrames,
        string failure)
    {
        FrontendFrame frame = initialFrame;
        for (int frameIndex = 0; frameIndex < maximumFrames && !finished(frame); frameIndex++)
            frame = game.Step(0);
        if (!finished(frame))
            throw new InvalidOperationException($"{failure} within {maximumFrames} frames.");
        return frame;
    }
}
