using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Render fixtures bound to the repository installation shared with every other suite.</summary>
internal static class RenderRetailFixture
{
    internal static AreaMapPresentationCatalog Maps => RepositoryInstallation.Maps;

    internal static SuperMetroidGame CreateGame(ISnesAddressSpace bus,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true) =>
        RepositoryInstallation.CreateGame(bus, gameOptions, renderGameplayFrames);

    internal static TitleSequenceState CreateTitle(ISnesAddressSpace bus) => RepositoryInstallation.CreateTitle(bus);

    internal static CeresDestructionCinematicState CreateDestruction(ISnesAddressSpace bus) =>
        RepositoryInstallation.CreateDestruction(bus);

    internal static IntroCinematicState CreateIntro(ISnesAddressSpace bus) => RepositoryInstallation.CreateIntro(bus);
}
