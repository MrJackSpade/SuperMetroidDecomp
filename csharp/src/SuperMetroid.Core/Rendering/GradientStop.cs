using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Where one band of a <see cref="ScreenGradient"/> begins (#1468): from <paramref name="Line"/> down to the next stop,
/// or to the gradient's bottom, every line blends <paramref name="Colour"/> in as <paramref name="Blend"/> says.
/// </summary>
/// <param name="Line">The screen line the band begins on, counted from the top of the screen.</param>
/// <param name="Colour">The colour the band blends in.</param>
/// <param name="Blend">The layers the colour blends with and whether it is subtracted from them.</param>
public readonly record struct GradientStop(int Line, RgbColor Colour, ColorBlend Blend);
