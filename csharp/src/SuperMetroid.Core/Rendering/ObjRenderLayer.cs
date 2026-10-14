namespace SuperMetroid.Core.Rendering;

/// <summary>Inserts the winning OAM pixel, or adds it as an OBJ subscreen using saturated five-bit color arithmetic.</summary>
public sealed record ObjRenderLayer(bool AddToScreen = false, FixedColorAddRenderLayer? FixedColor = null) : RenderLayer;
