using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>Tints the composed screen by adding a colour to it, each channel held at full (#1544).</summary>
/// <param name="Colour">The colour the tint adds, which the backend shows, blending any fade (#2563).</param>
public sealed record ScreenTintLayer(ShownColour Colour) : DisplayLayer;
