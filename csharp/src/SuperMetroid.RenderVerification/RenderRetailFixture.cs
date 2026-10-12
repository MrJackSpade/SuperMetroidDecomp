using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Render fixtures bound to the repository installation shared with every other suite.</summary>
internal static class RenderRetailFixture
{
    /// <summary>Gets maps.</summary>
    internal static AreaMapPresentationCatalog Maps => InstalledFixtures.Maps;

    /// <summary>Creates game.</summary>
    internal static SuperMetroidGame CreateGame(ISnesAddressSpace bus,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true) =>
        InstalledFixtures.CreateGame(bus, gameOptions, renderGameplayFrames);

    /// <summary>Creates title.</summary>
    internal static TitleSequenceState CreateTitle(ISnesAddressSpace bus) => InstalledFixtures.CreateTitle(bus);

    /// <summary>Creates destruction.</summary>
    internal static CeresDestructionCinematicState CreateDestruction(ISnesAddressSpace bus) =>
        InstalledFixtures.CreateDestruction(bus);

    /// <summary>Creates intro.</summary>
    internal static IntroCinematicState CreateIntro(ISnesAddressSpace bus) => InstalledFixtures.CreateIntro(bus);
}
