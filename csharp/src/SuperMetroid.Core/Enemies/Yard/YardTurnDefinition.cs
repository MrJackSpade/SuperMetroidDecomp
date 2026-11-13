namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One physical lookahead and behaviour-selector record for a Yard surface turn.</summary>
/// <param name="Lookahead">The temporary room-pixel probe offset along the direction of motion.</param>
/// <param name="OutsideTurnAnimation">The animation for an outside turn.</param>
/// <param name="InsideTurnAnimation">The animation for an inside turn.</param>
internal readonly record struct YardTurnDefinition(
    Offset16<RoomPixels> Lookahead,
    YardSegment OutsideTurnAnimation,
    YardSegment InsideTurnAnimation);
