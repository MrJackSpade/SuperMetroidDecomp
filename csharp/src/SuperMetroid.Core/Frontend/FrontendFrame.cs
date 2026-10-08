using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Frontend;

/// <summary>One host-visible 256x224 frame emitted by the front-end dispatcher.</summary>
/// <remarks>
/// <see cref="Pixels"/> is the producing scene's or runtime's own frame buffer: it stays valid
/// only until that producer renders again. Hosts copy it on presentation; anything that keeps
/// a frame across another step uses <see cref="WithCopiedPixels"/>.
/// </remarks>
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
