namespace SuperMetroid.Core.Rendering;

/// <summary>Adds fixed five-bit RGB to the composed screen, saturating each component.</summary>
public sealed record FixedColorAddRenderLayer(byte Red, byte Green, byte Blue) : RenderLayer;
