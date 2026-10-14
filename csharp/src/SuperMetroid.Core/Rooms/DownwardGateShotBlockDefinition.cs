namespace SuperMetroid.Core.Rooms;

/// <summary>
/// One calculated downward-gate shot-block setup action for bank $84.
/// </summary>
internal readonly record struct DownwardGateShotBlockDefinition(
    ushort InstructionList,
    ushort LeftBlockWord,
    ushort RightBlockWord);
