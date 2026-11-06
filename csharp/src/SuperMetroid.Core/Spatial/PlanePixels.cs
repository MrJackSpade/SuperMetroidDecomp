namespace SuperMetroid.Core.Spatial;

/// <summary>
/// Pixels of a pictured plane: a layer of placed pictures or picture cells that repeats at its edges, measured from the
/// plane's top-left corner, as the plane pixel shown at the screen's top-left selects. Gameplay state keeps its own
/// spaces (room, background, effect overlay) and converts by name when it publishes a plane.
/// </summary>
public sealed class PlanePixels : ISpatialSpace;
