namespace SuperMetroid.Core.Game;

/// <summary>Coroutine phase for <see cref="GameplayMessageBoxState"/>.</summary>
public enum GameplayMessageBoxPhase : byte
{
    /// <summary>No message owns gameplay or the BG3 window; after restoration, any completed save-confirmation result remains available for consumption by the suspended caller.</summary>
    Inactive,
    /// <summary>$85:844C Open_MessageBox: publish scanline-window radii from zero through 24 pixels in two-pixel steps while gameplay remains suspended.</summary>
    Opening,
    /// <summary>Fully opened ordinary message's mandatory authored wait: 360 updates for item descriptions or ten for station completion notices, without controller inspection.</summary>
    MinimumDisplay,
    /// <summary>Fully opened input wait: ordinary messages dismiss on any held button, while save confirmation polls newly pressed A/B and left/right/select at its native read cadence.</summary>
    AwaitingInput,
    /// <summary>$85:8589 Close_MessageBox: contract the window through radius zero before PPU restoration, or before the accepted gunship-save sound/completion sequence.</summary>
    Closing,
    /// <summary>The gunship's $85:8119 sound wait before SAVE COMPLETED.</summary>
    GunshipSavingSound,
    /// <summary>Lag frames before Open_MessageBox: PPU, tilemap and HDMA setup.</summary>
    Preparing,
    /// <summary>Lag frames after Close_MessageBox: tilemap clear and PPU restore.</summary>
    Restoring,
}
