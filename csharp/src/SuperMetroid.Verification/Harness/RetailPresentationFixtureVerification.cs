using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Harness;

/// <summary>Provides installed presentation catalogs for message and pause verification fixtures.</summary>
internal static class RetailPresentationFixtureVerification
{
    /// <summary>A new message box state; what it shows is laid out from <see cref="RetailMessageFixture"/>.</summary>
    internal static GameplayMessageBoxState CreateGameplayMessageFixture() => new();

    /// <summary>The installed title, panel and notice artwork the message box images are composed from.</summary>
    internal static GameplayMessageArtwork RetailMessageFixture() => GameplayMessageArtwork.Of(RetailPresentationFixture());

    /// <summary>Creates a pause menu using the retail address space and installed map presentation.</summary>
    internal static PauseMenuState CreateRetailPauseFixture(PauseMenuEntry entry) =>
        InstalledFixtures.CreatePause(entry);

    /// <summary>
    /// The shared installation's map presentation, the same extracted content contract the
    /// playable host uses. Independent game instances still own their mutable state.
    /// </summary>
    internal static AreaMapPresentation RetailPresentationFixture() =>
        InstalledFixtures.Maps;
}
