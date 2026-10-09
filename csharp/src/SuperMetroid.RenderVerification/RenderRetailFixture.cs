using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Render fixtures bound to the repository installation shared with every other suite.</summary>
internal static class RenderRetailFixture
{
    /// <summary>Shared area-map and frontend presentation catalog installed for render checks.</summary>
    internal static AreaMapPresentationCatalog Maps => RepositoryInstallation.Maps;

    /// <summary>Creates a game with repository installation bindings for retail render comparisons.</summary>
    /// <param name="bus">Cartridge address space used by the game.</param>
    /// <param name="gameOptions">Optional gameplay configuration forwarded to the game constructor.</param>
    /// <param name="renderGameplayFrames">Whether gameplay frame rendering is enabled for the fixture.</param>
    /// <returns>A game with the shared installation's runtime presentation catalogs bound.</returns>
    internal static SuperMetroidGame CreateGame(ISnesAddressSpace bus,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true) =>
        RepositoryInstallation.CreateGame(bus, gameOptions, renderGameplayFrames);

    /// <summary>Creates the title sequence with the shared installation's title presentation assets.</summary>
    /// <param name="bus">Cartridge address space used by the sequence.</param>
    /// <returns>A title state configured with the retail installation catalogs.</returns>
    internal static TitleSequenceState CreateTitle(ISnesAddressSpace bus) => RepositoryInstallation.CreateTitle(bus);

    /// <summary>Creates the Ceres destruction sequence with installed palette, artwork, and room effects data.</summary>
    /// <param name="bus">Cartridge address space used by the cinematic.</param>
    /// <returns>A destruction state configured with the retail installation catalogs.</returns>
    internal static CeresDestructionCinematicState CreateDestruction(ISnesAddressSpace bus) =>
        RepositoryInstallation.CreateDestruction(bus);

    /// <summary>Creates the intro sequence with shared character, beam, Samus, projectile, and narration assets.</summary>
    /// <param name="bus">Cartridge address space used by the sequence.</param>
    /// <returns>An intro state configured with the retail installation catalogs.</returns>
    internal static IntroCinematicState CreateIntro(ISnesAddressSpace bus) => RepositoryInstallation.CreateIntro(bus);
}
