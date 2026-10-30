using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// A colour added to the screen inside a window (#1468): each channel is widened to eight bits, added and clamped at
/// 255, and the screen's opacity kept. Beams, the Power Bomb's blast, the suit pickup's light and the Ceres haze are
/// colours added through their windows. Unlike the intro's colour addition, the screen's colour is not first reduced
/// to five-bit channels.
/// </summary>
/// <param name="Window">The pixels the colour is added to.</param>
/// <param name="Colour">The colour added, whole on every line unless <paramref name="Steps"/> strengthens it down the screen.</param>
/// <param name="Steps">How the colour strengthens down the screen, or null for the whole colour on every line.</param>
public sealed record ColourAddWindowLayer(ScreenWindow Window, RgbColor Colour, LineColourSteps? Steps = null) : DisplayLayer
{
    /// <summary>The pixels the colour is added to.</summary>
    public ScreenWindow Window { get; init; } = Ensure.NotNull(Window);
}
