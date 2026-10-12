using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Render fixtures bound to the repository installation shared with every other suite.</summary>
internal static class RenderRetailFixture
{
    internal static AreaMapPresentationCatalog Maps => InstalledFixtures.Maps;

    internal static SuperMetroidGame CreateGame(ISnesAddressSpace bus,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true) =>
        InstalledFixtures.CreateGame(bus, gameOptions, renderGameplayFrames);

    internal static TitleSequenceState CreateTitle(ISnesAddressSpace bus) => InstalledFixtures.CreateTitle(bus);

    internal static CeresDestructionCinematicState CreateDestruction(ISnesAddressSpace bus) =>
        InstalledFixtures.CreateDestruction(bus);

    internal static IntroCinematicState CreateIntro(ISnesAddressSpace bus) => InstalledFixtures.CreateIntro(bus);
}
