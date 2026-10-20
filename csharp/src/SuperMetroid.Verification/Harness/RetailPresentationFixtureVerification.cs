using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Harness;

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

    /// <summary>Creates a pause menu using the retail address space and installed map presentation.</summary>
    internal static PauseMenuState CreateRetailPauseFixture(PauseMenuEntry entry) =>
        InstalledFixtures.CreatePause(entry);

    /// <summary>
    /// The shared installation's map presentation, the same extracted content contract the
    /// playable host uses. Independent game instances still own their mutable state.
    /// </summary>
    internal static AreaMapPresentationCatalog RetailPresentationFixture() =>
        InstalledFixtures.Maps;
}
