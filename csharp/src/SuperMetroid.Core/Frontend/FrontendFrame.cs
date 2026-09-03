using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Frontend;

/// <summary>One host-visible 256x224 frame emitted by the front-end dispatcher.</summary>
public readonly record struct FrontendFrame(
    SuperMetroidGameState GameState,
    string Phase,
    ushort FrameNumber,
    Rgba32[] Pixels,
    IReadOnlyList<CartridgeAudioCommand> AudioCommands)
{
    public const int Width = SnesPpuLayout.ScreenWidthPixels;
    public const int Height = SnesPpuLayout.ScreenHeightPixels;
}
