namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>The wall cannon a cannon room object sets up.</summary>
/// <param name="Orientation">Which wall the cannon faces out from, selecting its draw family and the cannon it controls.</param>
/// <param name="Start">Whether the cannon starts whole or already destroyed.</param>
internal readonly record struct DraygonCannonSetup(DraygonCannonOrientation Orientation, DraygonCannonStart Start);
