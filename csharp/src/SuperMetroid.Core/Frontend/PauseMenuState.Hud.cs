using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

internal sealed partial class PauseMenuState
{
    /// <summary>Retains only HUD DMA accepted by NMI, not the pending WRAM tilemap.</summary>
    public void SynchronizeAcceptedHud(SnesVram gameplayVram)
    {
        // Pause owns an isolated PPU image. Mirror the native mutable HUD range,
        // leaving its static first row and deliberately cleared FX rows untouched.
        int offset = HudState.VramDestination * 2;
        vram.LoadBytes(offset, gameplayVram.Bytes.Slice(offset, HudState.MutableByteCount));
    }
}
