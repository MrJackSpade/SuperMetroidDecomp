using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

/// <summary>One host-visible 256x224 frame emitted by the front-end dispatcher.</summary>
public readonly record struct FrontendFrame(
    SuperMetroidGameState GameState,
    string Phase,
    ushort FrameNumber,
    Rgba32[] Pixels)
{
    public const int Width = 256;
    public const int Height = 224;
}
