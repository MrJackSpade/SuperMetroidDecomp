namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One physical lookahead and callback-selector record for a Yard surface turn.</summary>
/// <param name="Lookahead">The temporary room-pixel probe offset along the direction of motion.</param>
/// <param name="OutsideTurnProgram">The animation for an outside turn.</param>
/// <param name="InsideTurnProgram">The animation for an inside turn.</param>
internal readonly record struct YardTurnDefinition(
    Offset16<RoomPixels> Lookahead,
    YardProgram OutsideTurnProgram,
    YardProgram InsideTurnProgram);
