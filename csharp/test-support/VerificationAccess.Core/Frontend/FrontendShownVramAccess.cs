using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Verification access to the memory image each frontend scene composes when it captures a frame (#1403): what its
/// uploads left, which the published frame no longer carries.
/// </summary>
internal static class FrontendShownVramAccess
{
    /// <summary>
    /// The intro's composed image, or the Ceres flight's while the intro shows the flight, over the flight artwork the intro
    /// was bound with and binds into its flight.
    /// </summary>
    extension(IntroCinematicState self)
    {
        [WholeImageFixture("the scene's composed memory image")]
        internal byte[] ShownVram() => self.CurrentCeresFlight is { } flight && self.Phase == IntroCinematicPhase.CeresFlight
            ? CeresFlightScreenMemory.Compose(flight, self.CharacterArtwork?.CeresFlight
                ?? throw new InvalidOperationException("The intro's Ceres flight is composed from the flight artwork the intro was bound with."))
            : IntroScreenMemory.Compose(self);
    }

    /// <summary>The Ceres flight's composed image, over <paramref name="artwork"/>, the flight artwork its fixture bound.</summary>
    extension(IntroCeresFlightState self) { [WholeImageFixture("the scene's composed memory image")] internal byte[] ShownVram(SuperMetroid.Core.Frontend.CeresCinematics.Artwork.CeresFlightArtwork artwork) => CeresFlightScreenMemory.Compose(self, artwork); }

    /// <summary>The Ceres destruction's composed image.</summary>
    extension(CeresDestructionCinematicState self) { [WholeImageFixture("the scene's composed memory image")] internal byte[] ShownVram() => CeresDestructionScreenMemory.Compose(self); }

    /// <summary>
    /// The ending's composed image, over the installation's flight and ending object artwork, which the ending's installed
    /// pictures are composed from and the game binds none of (#1616).
    /// </summary>
    extension(EndingCreditsState self) { [WholeImageFixture("the scene's composed memory image")] internal byte[] ShownVram() => EndingScreenMemory.Compose(
        EndingScreenLoads.Of(self.Stage.Phase), SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EndingFloors,
        SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.IntroArtwork.FlightCharacters,
        SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EndingObjects,
        // The ending's installed pictures are composed from the installation's ending font (#1583).
        SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportMaps.EndingFont,
        self.Stage.EndingReward); }

    /// <summary>The title screen's composed image.</summary>
    extension(TitleSequenceState self) { [WholeImageFixture("the scene's composed memory image")] internal byte[] ShownVram() => TitleScreenMemory.Compose(self); }

    /// <summary>The pause menu's composed image, over <paramref name="artwork"/>, the map presentation its fixture bound.</summary>
    extension(PauseMenuState self) { [WholeImageFixture("the scene's composed memory image")] internal byte[] ShownVram(SuperMetroid.Core.Frontend.Map.Artwork.AreaMapPresentation artwork) => PauseScreenMemory.Compose(self, artwork); }
}
