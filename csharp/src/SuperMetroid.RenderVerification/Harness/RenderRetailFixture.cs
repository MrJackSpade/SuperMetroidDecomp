using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics;
namespace SuperMetroid.RenderVerification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Render fixtures bound to the repository installation shared with every other suite.</summary>
internal static class RenderRetailFixture
{
    /// <summary>Uses the shared installed map presentation in retail render captures.</summary>
    internal static AreaMapPresentationCatalog Maps => InstalledFixtures.Maps;

    /// <summary>Creates a retail-backed game with the requested gameplay-frame capture setting.</summary>
    internal static SuperMetroidGame CreateGame(ISnesAddressSpace bus,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true) =>
        InstalledFixtures.CreateGame(bus, gameOptions, renderGameplayFrames);

    /// <summary>Creates the title sequence from the shared retail fixture installation.</summary>
    internal static TitleSequenceState CreateTitle(ISnesAddressSpace bus) => InstalledFixtures.CreateTitle(bus);

    /// <summary>Creates the Ceres destruction sequence from the shared retail fixture installation.</summary>
    internal static CeresDestructionCinematicState CreateDestruction(ISnesAddressSpace bus) =>
        InstalledFixtures.CreateDestruction(bus);

    /// <summary>Creates the intro cinematic from the shared retail fixture installation.</summary>
    internal static IntroCinematicState CreateIntro(ISnesAddressSpace bus) => InstalledFixtures.CreateIntro(bus);
}
