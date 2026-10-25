namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// One calculated downward-gate shot-block setup action for bank $84.
/// </summary>
/// <param name="Program">The gate program the trigger wakes.</param>
/// <param name="LeftBlockWord">The level word written left of the trigger, or zero for none.</param>
/// <param name="RightBlockWord">The level word written right of the trigger, or zero for none.</param>
internal readonly record struct DownwardGateShotBlockDefinition(
    DownwardGateProgram Program,
    int LeftBlockWord,
    int RightBlockWord);
