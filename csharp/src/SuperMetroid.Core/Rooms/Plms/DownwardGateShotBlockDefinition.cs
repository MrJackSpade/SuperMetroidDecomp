namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// One calculated downward-gate shot-block setup action for bank $84.
/// </summary>
internal readonly record struct DownwardGateShotBlockDefinition(
    DownwardGateProgram Program,
    ushort LeftBlockWord,
    ushort RightBlockWord);
