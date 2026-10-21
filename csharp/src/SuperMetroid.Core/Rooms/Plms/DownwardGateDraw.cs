namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The downward gate's draw lists (#1296).</summary>
internal enum DownwardGateDraw
{
    /// <summary><c>DrawInst_DownwardsGateOpen</c>.</summary>
    Open,
    /// <summary><c>DrawInst_DownwardsGateOpenClosed_0</c>: the gate a quarter down.</summary>
    Lowered1,
    /// <summary><c>DrawInst_DownwardsGateOpenClosed_1</c>: the gate halfway down.</summary>
    Lowered2,
    /// <summary><c>DrawInst_DownwardsGateOpenClosed_2</c>: the gate three quarters down.</summary>
    Lowered3,
    /// <summary><c>DrawInst_DownwardsGateOpenClosed_3</c>: the gate all the way down.</summary>
    Lowered4,
    /// <summary><c>DrawInst_DownwardsGateClosed</c>.</summary>
    Closed,
    /// <summary><c>DrawInst_GateShotblockDownwardsBlueLeft</c>.</summary>
    TriggerBlueLeft,
    /// <summary><c>DrawInst_GateShotblockDownwardsBlueRight</c>.</summary>
    TriggerBlueRight,
    /// <summary><c>DrawInst_GateShotblockDownwardsRedLeft</c>.</summary>
    TriggerRedLeft,
    /// <summary><c>DrawInst_GateShotblockDownwardsRedRight</c>.</summary>
    TriggerRedRight,
    /// <summary><c>DrawInst_GateShotblockDownwardsGreenLeft</c>.</summary>
    TriggerGreenLeft,
    /// <summary><c>DrawInst_GateShotblockDownwardsGreenRight</c>.</summary>
    TriggerGreenRight,
    /// <summary><c>DrawInst_GateShotblockDownwardsYellowLeft</c>.</summary>
    TriggerYellowLeft,
    /// <summary><c>DrawInst_GateShotblockDownwardsYellowRight</c>.</summary>
    TriggerYellowRight,
}
