using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Hud;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Harness;

/// <summary>Provides installed presentation catalogs for message and pause verification fixtures.</summary>
internal static class RetailPresentationFixtureVerification
{
    /// <summary>A new message box state; what it shows is laid out from <see cref="RetailMessageFixture"/>.</summary>
    internal static GameplayMessageBoxState CreateGameplayMessageFixture() => new(new SuperMetroid.Core.Compatibility.SaveChoiceInputCadence(), new SuperMetroid.Core.Compatibility.MessageBoxPauses());

    /// <summary>The installed title, panel and notice artwork the message box images are composed from.</summary>
    internal static GameplayMessageArtwork RetailMessageFixture() => RetailImportPresentationFixture().Messages;

    /// <summary>Creates a pause menu using the retail address space and installed map presentation.</summary>
    internal static PauseMenuState CreateRetailPauseFixture(PauseMenuEntry entry) => new(entry);

    /// <summary>
    /// The shared installation's map presentation, the same extracted content contract the
    /// playable host uses. Independent game instances still own their mutable state.
    /// </summary>
    internal static InstalledScreenContent RetailPresentationFixture() =>
        InstalledFixtures.Maps;

    /// <summary>
    /// The shared installation's map presentation as the import reads it: <see cref="RetailPresentationFixture"/> beside the
    /// source artwork and colours the import composes pictures from, which the game never receives (#1616).
    /// </summary>
    internal static SuperMetroid.AssetExtraction.Frontend.Map.ImportAreaMapPresentation RetailImportPresentationFixture() =>
        InstalledFixtures.ImportMaps;
}
