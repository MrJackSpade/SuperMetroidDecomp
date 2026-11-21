using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>One station frame the import pictures (<see cref="StationFrameDefinitions"/>).</summary>
/// <param name="DrawnBy">The station kinds whose stations draw the frame.</param>
/// <param name="Runs">The frame's block shape written as rows, the object's block first.</param>
internal sealed partial record StationFrameDefinition(IReadOnlyList<RoomObjectKind> DrawnBy, IReadOnlyList<RoomObjectShapeRun> Runs);
