using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;

namespace SuperMetroid.Core.Frontend;

/// <summary>One host-visible 256x224 frame emitted by the front-end dispatcher.</summary>
/// <remarks>
/// <see cref="Pixels"/> is the producing scene's or runtime's own frame buffer: it stays valid
/// only until that producer renders again. Hosts copy it on presentation; anything that keeps
/// a frame across another step must retain a copied pixel array.
/// </remarks>
/// <param name="GameState">Front-end dispatcher state reported with this emitted frame.</param>
/// <param name="Phase">Human-readable scene or runtime phase label for host diagnostics.</param>
/// <param name="FrameNumber">Wrapping 16-bit front-end step counter, not elapsed time or a native movie video-frame number.</param>
/// <param name="Pixels">Producer-owned row-major RGBA screen buffer, valid until that producer renders again; copy it before retaining the frame.</param>
/// <param name="AudioCommands">Ordered cartridge audio commands published with this step for host audio dispatch; this record does not execute them.</param>
public readonly record struct FrontendFrame(
    SuperMetroidGameState GameState,
    string Phase,
    ushort FrameNumber,
    Rgba32[] Pixels,
    IReadOnlyList<CartridgeAudioCommand> AudioCommands)
{
    /// <summary>Visible frame width in pixels: 256 columns in row-major screen order.</summary>
    public const int Width = SnesPpuLayout.ScreenWidthPixels;
    /// <summary>Visible non-overscan frame height in pixels: 224 scanlines.</summary>
    public const int Height = SnesPpuLayout.ScreenHeightPixels;
}
