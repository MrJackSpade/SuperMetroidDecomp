using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>VRAM layout and palette fields used by the bank-$82 pause-screen routines.</summary>
internal static class PauseMenuLayout
{
    /// <summary>Pause character/palette word used to blank the retained FX plane.</summary>
    public static BackgroundTile Bg3FxClearTile { get; } = new(0x4e, 6, highPriority: false);
    /// <summary>The first tile of the two mutable button rows in the button tilemap.</summary>
    public const int ButtonRowsSourceTile = 0x0120;
    /// <summary>The tiles in the two mutable button rows.</summary>
    public const int ButtonRowsTileCount = 0x0040;
    /// <summary>$7E:3400 button source expressed relative to the $7E:3000 native word indexes.</summary>
    public const int ButtonSourceWordOrigin = 0x0200;
}
