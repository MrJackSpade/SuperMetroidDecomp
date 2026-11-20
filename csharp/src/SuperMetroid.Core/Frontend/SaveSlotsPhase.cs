using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>The save slots of choosing a game (<see cref="SuperMetroidGameState.FileSelect"/>), with copying and clearing.</summary>
/// <param name="session">The frontend session the step runs against.</param>
/// <param name="menu">The save slots the step shows.</param>
internal sealed class SaveSlotsPhase(FrontendSession session, FileSelectMenuState menu) : IFrontendPhase, IScreenPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.FileSelect;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => menu.Phase.ToString();

    /// <inheritdoc/>
    /// <remarks>The save slots wait out a frame of the display on some of their steps.</remarks>
    public bool IsWaitingOutDisplayFrame => menu.IsWaitingOutDisplayFrame;

    /// <inheritdoc/>
    public IFrontendScreen Screen => menu;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        FileSelectSaveChange saveChange = menu.Step(controllerInput);
        if (saveChange != FileSelectSaveChange.None)
            session.RaiseSavesChanged();
        if (saveChange == FileSelectSaveChange.Cleared)
        {
            // Clearing a file starts a new one, which clears every stored explored map, then loads the live map from a
            // stored map that is now empty.
            StoredExploredMapLoads.ClearAll(session.storedExploredMaps);
        }
        session.display.PublishMenu(menu);
        if (menu.Outcome == FileSelectMenuOutcome.Title)
        {
            // The game soft-resets when the menu returns to the title. Starting the title again
            // makes all frontend-owned state disposable and deterministic.
            session.Enter(new TitleResetPhase(session));
        }
        else if (menu.Outcome == FileSelectMenuOutcome.NewGame)
        {
            session.SelectSaveSlot(menu.SelectedSaveSlot, menu.SelectedSlotContainsSave);
            session.RaiseSavesChanged();
            // Existing saves restore the same seven controller bindings and two
            // special settings that saving copied from live state. A new
            // slot begins with a new save file's defaults.
            SuperMetroidSaveSlot? optionSlot = session.LoadingExistingSave
                ? session.saves.ReadSlot(session.SelectedSaveSlot)
                : null;
            var options = new GameOptionsMenuState(
                session.audio,
                session.Artwork.ScreenContent,
                session.Artwork.SpriteSheets,
                optionSlot?.ControllerBindings ?? ControllerBindings.Default,
                optionSlot?.IconCancelEnabled ?? false,
                optionSlot?.MoonwalkEnabled ?? false,
                japaneseText: optionSlot?.JapaneseText ?? false)
            {
                StartupRoute = optionSlot?.StartupRoute ?? SaveStartupRoute.OpeningCinematic,
            };
            session.Enter(new GameOptionsMenuPhase(session, options));
            session.display.PublishMenu(options);
        }
    }
}
