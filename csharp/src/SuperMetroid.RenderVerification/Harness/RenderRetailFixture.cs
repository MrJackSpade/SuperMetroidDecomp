using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics;
namespace SuperMetroid.RenderVerification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Audio;

/// <summary>Render fixtures bound to the repository installation shared with every other suite.</summary>
internal static class RenderRetailFixture
{
    /// <summary>Uses the shared installed map presentation in retail render captures.</summary>
    internal static AreaMapPresentationCatalog Maps => InstalledFixtures.Maps;

    /// <summary>Creates a retail-backed game over <paramref name="saves"/> with the requested gameplay-frame capture setting.</summary>
    internal static SuperMetroidGame CreateGame(SuperMetroid.Core.Saves.SaveSlotStore saves,
        SuperMetroidGameOptions? gameOptions = null, bool renderGameplayFrames = true) =>
        InstalledFixtures.CreateGame(saves, gameOptions, renderGameplayFrames);

    /// <summary>Creates the title sequence from the shared retail fixture installation.</summary>
    internal static TitleSequenceState CreateTitle() => InstalledFixtures.CreateTitle();

    /// <summary>Creates the Ceres destruction sequence from the shared retail fixture installation.</summary>
    internal static CeresDestructionCinematicState CreateDestruction() =>
        InstalledFixtures.CreateDestruction();

    /// <summary>Creates the intro cinematic from the shared retail fixture installation, queueing its music on <paramref name="audio"/>.</summary>
    internal static IntroCinematicState CreateIntro(CartridgeAudioState audio) => InstalledFixtures.CreateIntro(audio);
}
