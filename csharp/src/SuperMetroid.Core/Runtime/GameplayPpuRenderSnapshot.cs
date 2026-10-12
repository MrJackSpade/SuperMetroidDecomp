using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// PPU register values that were made visible together by one accepted gameplay NMI.
/// </summary>
/// <param name="Layer1Position">The camera/layer-one position the frame's actors were drawn against.</param>
/// <param name="Bg1Scroll">The latched BG1HOFS/BG1VOFS mirrors.</param>
/// <param name="Bg2Scroll">The latched BG2HOFS/BG2VOFS mirrors.</param>
/// <param name="RoomShake">The room shake displayed with this frame.</param>
/// <param name="Bg2FirstScanline">The first scanline of the BG2 main-screen window.</param>
/// <param name="Bg2EndScanline">The scanline that ends the BG2 main-screen window.</param>
public readonly record struct GameplayPpuRenderSnapshot(
    Position16<RoomPixels> Layer1Position,
    Position16<RoomPixels> Bg1Scroll,
    Position16<Bg2Pixels> Bg2Scroll,
    RoomShakeFrameResult RoomShake,
    int Bg2FirstScanline = 32, int Bg2EndScanline = 224);
