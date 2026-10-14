namespace SuperMetroid.Core.Enemies.Yard;

/// <summary>One physical lookahead and callback-selector record for a Yard surface turn.</summary>
/// <param name="Lookahead">The temporary room-pixel probe offset along the direction of motion.</param>
/// <param name="OutsideTurnInstructionList">The instruction list for an outside turn.</param>
/// <param name="InsideTurnInstructionList">The instruction list for an inside turn.</param>
internal readonly record struct YardTurnDefinition(
    Offset16<RoomPixels> Lookahead,
    ushort OutsideTurnInstructionList,
    ushort InsideTurnInstructionList);
