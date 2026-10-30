using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Pause;
using SuperMetroid.Core.Frontend.Title;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Verification access to the memory image each frontend scene composes when it captures a frame (#1403): what its
/// uploads left, which the published frame no longer carries.
/// </summary>
internal static class FrontendShownVramAccess
{
    /// <summary>The intro's composed image, or the Ceres flight's while the intro shows the flight.</summary>
    extension(IntroCinematicState self)
    {
        [WholeImageFixture("the scene's composed memory image")]
        internal byte[] ShownVram() => self.CurrentCeresFlight is { } flight && self.Phase == IntroCinematicPhase.CeresFlight
            ? CeresFlightScreenMemory.Compose(flight) : IntroScreenMemory.Compose(self);
    }

    /// <summary>The Ceres flight's composed image.</summary>
    extension(IntroCeresFlightState self) { [WholeImageFixture("the scene's composed memory image")] internal byte[] ShownVram() => CeresFlightScreenMemory.Compose(self); }

    /// <summary>The Ceres destruction's composed image.</summary>
    extension(CeresDestructionCinematicState self) { [WholeImageFixture("the scene's composed memory image")] internal byte[] ShownVram() => CeresDestructionScreenMemory.Compose(self); }

    /// <summary>The ending's composed image.</summary>
    extension(EndingCreditsState self) { [WholeImageFixture("the scene's composed memory image")] internal byte[] ShownVram() => EndingScreenMemory.Compose(self.screen,
        self.Stage.FloorArtwork, self.Flyaway.FlightArtwork, self.Stage.ObjectArtwork, self.Stage.EndingFont, self.Stage.EndingReward); }

    /// <summary>The title screen's composed image.</summary>
    extension(TitleSequenceState self) { [WholeImageFixture("the scene's composed memory image")] internal byte[] ShownVram() => TitleScreenMemory.Compose(self); }

    /// <summary>The pause menu's composed image.</summary>
    extension(PauseMenuState self) { [WholeImageFixture("the scene's composed memory image")] internal byte[] ShownVram() => PauseScreenMemory.Compose(self.screen); }
}
