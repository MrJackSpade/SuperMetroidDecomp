using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Spatial;

/// <summary>Provides installed presentation catalogs for message and pause verification fixtures.</summary>
internal static class RetailPresentationFixtureVerification
{
    /// <summary>Binds the installed title, panel, and notice artwork to a new message box state.</summary>
    internal static GameplayMessageBoxState CreateGameplayMessageFixture()
    {
        var maps = RetailPresentationFixture();
        var state = new GameplayMessageBoxState();
        state.BindPresentation(maps.GameplayMessageTitles, maps.GameplayMessagePanels, maps.GameplayMessageNotices);
        return state;
    }

    /// <summary>
    /// Steps a requested L/R pause-page switch to completion. Native page switches take
    /// a fade-out, a separate load dispatch and a delay-one fade-in (46 updates).
    /// </summary>
    private static void CompletePausePageTransition(PauseMenuState pause)
    {
        for (int update = 0; update < 64 && pause.PageTransitionActive; update++)
            pause.Step(0, 0);
        AssertTrue(!pause.PageTransitionActive, "pause page switch completes");
    }

    /// <summary>Creates a pause menu using the retail address space and installed map presentation.</summary>
    internal static PauseMenuState CreateRetailPauseFixture(ISnesAddressSpace bus, PauseMenuEntry entry) =>
        InstalledFixtures.CreatePause(bus, entry);

    /// <summary>
    /// The shared installation's map presentation, the same extracted content contract the
    /// playable host uses. Independent game instances still own their mutable state.
    /// </summary>
    internal static AreaMapPresentationCatalog RetailPresentationFixture() =>
        InstalledFixtures.Maps;
}
