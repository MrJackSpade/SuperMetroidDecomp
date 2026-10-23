using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.Options;
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
        internal byte[] ShownVram() => self.CurrentCeresFlight is { } flight && self.Phase == IntroCinematicPhase.CeresFlight
            ? CeresFlightScreenMemory.Compose(flight) : IntroScreenMemory.Compose(self);
    }

    /// <summary>The Ceres flight's composed image.</summary>
    extension(IntroCeresFlightState self) { internal byte[] ShownVram() => CeresFlightScreenMemory.Compose(self); }

    /// <summary>The Ceres destruction's composed image.</summary>
    extension(CeresDestructionCinematicState self) { internal byte[] ShownVram() => CeresDestructionScreenMemory.Compose(self); }

    /// <summary>The ending's composed image.</summary>
    extension(EndingCreditsState self) { internal byte[] ShownVram() => self.Image(); }

    /// <summary>The title screen's composed image.</summary>
    extension(TitleSequenceState self) { internal byte[] ShownVram() => TitleScreenMemory.Compose(self); }

    /// <summary>The game over menu's composed image.</summary>
    extension(GameOverMenuState self) { internal byte[] ShownVram() => MenuScreenMemory.Compose(self.screen); }

    /// <summary>The options menu's composed image.</summary>
    extension(GameOptionsMenuState self) { internal byte[] ShownVram() => MenuScreenMemory.Compose(self.screen); }

    /// <summary>The file select menu's composed image.</summary>
    extension(FileSelectMenuState self) { internal byte[] ShownVram() => MenuScreenMemory.Compose(self.screen); }

    /// <summary>The file select area map's composed image.</summary>
    extension(FileSelectAreaMapGraphics self) { internal byte[] ShownVram() => MenuScreenMemory.Compose(self.screen); }

    /// <summary>The file select room map's composed image.</summary>
    extension(FileSelectRoomMapGraphics self) { internal byte[] ShownVram() => MenuScreenMemory.Compose(self.screen); }

    /// <summary>The pause menu's composed image.</summary>
    extension(PauseMenuState self) { internal byte[] ShownVram() => PauseScreenMemory.Compose(self.screen); }
}
