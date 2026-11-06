namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The wall cannon a room cannon PLM sets up.</summary>
/// <param name="Orientation">Which wall the cannon faces out from, selecting its draw family and the cannon it controls.</param>
/// <param name="StartsDestroyed">Whether the cannon starts destroyed, its program beginning at the destruction step.</param>
internal readonly record struct DraygonCannonSetup(DraygonCannonOrientation Orientation, bool StartsDestroyed);
