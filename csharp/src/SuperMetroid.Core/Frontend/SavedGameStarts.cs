using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.CeresCinematics;

namespace SuperMetroid.Core.Frontend;

/// <summary>How choosing a game leaves for the game it chose: the intro, the saved destruction cinematic or the area map's own exits.</summary>
internal static class SavedGameStarts
{
    /// <summary>Shows the opening cinematic in the text language the options menu selected.</summary>
    /// <param name="session">The session that starts the intro.</param>
    /// <param name="japaneseText">Whether the cinematic's narration is in Japanese.</param>
    internal static void StartIntro(FrontendSession session, bool japaneseText)
    {
        Ensure.NotNull(session);
        var cinematic = new IntroCinematicState(new IntroCinematicOverreads(
            session.Artwork.SamusOverreads, session.Artwork.BossAndIntroOverreads.ShellFragmentFallPastCurve),
            session.audio, session.Artwork.FlashbackSamusArtwork(),
            session.Artwork.IntroCinematicArt, japaneseText)
        {
            Random = session.Random,
            UnauthoredBoostCadence = session.unauthoredBoostCadence,
            NarrationPresentation = session.Artwork.ScreenContent.IntroNarration,
        };
        session.Enter(new IntroCinematicGamePhase(session, cinematic));
        session.display.PublishIntro(cinematic);
    }

    /// <summary>
    /// Loading sends the saved escape-ending mode to the initial destruction cinematic.
    /// Restore the saved inventory, but do not initialize any Ceres room or actors.
    /// The existing cinematic handoff later loads the Zebes arrival.
    /// </summary>
    /// <param name="session">The session that starts the cinematic.</param>
    /// <param name="slot">The saved game resumed at the destruction.</param>
    internal static void StartSavedCeresDestruction(FrontendSession session, SuperMetroidSaveSlot slot)
    {
        Ensure.NotNull(session);
        Ensure.NotNull(slot);
        SuperMetroidRuntime created = session.CreateGameplayRuntime(GameplayRuntimePurpose.SelectedGame);
        created.RoomLoader.RestoreSavedPlayerState(slot);
        // Unlike an ongoing escape, a file-select resume has a fresh gameplay
        // owner. Construct its ordinary HUD now so the cinematic's later room
        // handoff retains initialized HUD state and standard graphics.
        FrontendSession.InitializeSelectedGameHud(slot, created);
        created.FrameBoundary.PublishWhileLoading();
        session.ApplySelectedGameOptions(created);
        session.BeginCeresDestruction(CeresDestructionOutcome.SamusEscaped, created);
        session.display.PublishBlack();
    }

    /// <summary>
    /// Leaves the saved game's area map for the options menu, set from <paramref name="slot"/>, as the map's back button
    /// asks.
    /// </summary>
    /// <param name="session">The session that shows the options.</param>
    /// <param name="slot">The saved game whose map was showing.</param>
    internal static void ReturnToOptions(FrontendSession session, SuperMetroidSaveSlot slot)
    {
        Ensure.NotNull(session);
        Ensure.NotNull(slot);
        var options = new GameOptionsMenuState(session.audio, session.Artwork.ScreenContent,
            session.Artwork.SpriteSheets, slot.ControllerBindings,
            slot.IconCancelEnabled, slot.MoonwalkEnabled,
            japaneseText: slot.JapaneseText);
        session.Enter(new GameOptionsPhase(session, options));
        session.display.PublishMenu(options);
    }

    /// <summary>Shows one update of the saved game's area map <paramref name="map"/>, leaving for the options when it asks to.</summary>
    /// <param name="session">The session that shows the map.</param>
    /// <param name="map">The map the update ran.</param>
    /// <param name="slot">The saved game whose map it is.</param>
    internal static void PublishAreaMap(FrontendSession session, FileSelectMapMenuState map, SuperMetroidSaveSlot slot)
    {
        Ensure.NotNull(session);
        Ensure.NotNull(map);
        session.display.PublishFileMap(map);
        // A latched load request waits out the next frame before the game load (see AreaMapPhase).
        if (map.Outcome == FileSelectMapOutcome.Options)
            ReturnToOptions(session, slot);
    }
}
