namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// Mother Brain's escape-gate block draws (#2034): the block shape each draws, from
/// <see cref="MotherBrainEscapeGateObjectDrawDefinitions"/>, and the installed picture (#1557) each shows.
/// </summary>
internal static class MotherBrainEscapeGateDraws
{
    /// <summary>The gate's three frames, pictured by the installed escape-gate frames.</summary>
    internal static RoomObjectDrawFamily<MotherBrainEscapeGateDraw> Family { get; } =
        new RoomObjectDrawFamily<MotherBrainEscapeGateDraw>("Mother Brain escape gate",
            frame => RoomObjectDrawFamily<MotherBrainEscapeGateDraw>.Defined(MotherBrainEscapeGateObjectDrawDefinitions.Shapes, frame));
}
