namespace SuperMetroid.Core.Hud;

/// <summary>The stage a <see cref="GameplayMessageBoxState"/> is in.</summary>
public enum GameplayMessageBoxPhase
{
    /// <summary>No message owns gameplay or the message window; after restoration, any completed save-confirmation result remains available for consumption by the suspended caller.</summary>
    Inactive,
    /// <summary>The window opens: its radius grows from zero through 24 pixels in two-pixel steps while gameplay remains suspended.</summary>
    Opening,
    /// <summary>Fully opened ordinary message's mandatory authored wait: 360 updates for item descriptions or ten for station completion notices, without controller inspection.</summary>
    MinimumDisplay,
    /// <summary>Fully opened input wait: ordinary messages dismiss on any held button, while save confirmation polls newly pressed A/B and left/right/select at its read cadence (<see cref="SaveChoiceInputCadence"/>).</summary>
    AwaitingInput,
    /// <summary>The window closes through radius zero before the screen is restored, or before the accepted gunship save's sound and completion notice.</summary>
    Closing,
    /// <summary>The gunship's saving-sound wait before SAVE COMPLETED.</summary>
    GunshipSavingSound,
    /// <summary>Gameplay pauses while the screen, the box and its window are prepared, before the window opens.</summary>
    Preparing,
    /// <summary>Gameplay stays paused while the closed box is cleared and the screen restored.</summary>
    Restoring,
}
