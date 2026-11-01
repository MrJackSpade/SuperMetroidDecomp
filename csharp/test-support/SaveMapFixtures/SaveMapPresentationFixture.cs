using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.TestSupport.SaveMapFixtures;

/// <summary>Explicit startup import for diagnostics that decode cartridge-compatible SRAM.</summary>
internal static class SaveMapPresentationFixture
{
    /// <summary>Loads a fresh typed map catalog from the shared repository installation, which SRAM decoding reads.</summary>
    public static AreaMapPresentation Create() => RepositoryInstallation.RepositoryInstallation.Installation.LoadMaps();
}
