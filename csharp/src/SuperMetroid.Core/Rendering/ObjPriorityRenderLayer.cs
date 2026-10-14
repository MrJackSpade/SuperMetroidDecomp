namespace SuperMetroid.Core.Rendering;

/// <summary>Inserts only pixels whose winning OAM record has this priority.</summary>
public sealed record ObjPriorityRenderLayer(byte Priority, FixedColorAddRenderLayer? FixedColor = null) : RenderLayer;
