using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>Tints the composed screen by adding a colour to it, each channel held at full (#1544).</summary>
/// <param name="Colour">The colour the tint adds.</param>
public sealed record ScreenTintLayer(RgbColor Colour) : DisplayLayer;
