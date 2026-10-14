namespace SuperMetroid.Core.Spatial;

/// <summary>
/// Background-plane pixels of any BG layer: the tilemap-plane coordinate a BGnHOFS/BGnVOFS
/// register pair selects at the screen origin, as generic render layers consume it. Gameplay
/// state keeps its own spaces (room, BG2, BG3) and converts by name at capture.
/// </summary>
public sealed class BgPlanePixels : ISpatialSpace;
