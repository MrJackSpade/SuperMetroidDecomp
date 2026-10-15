namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The downward gate's bank-$84 draw lists (#1296); each member's value is its draw list.</summary>
internal enum DownwardGateDraw : ushort
{
    /// <summary><c>DrawInst_DownwardsGateOpen</c>.</summary>
    Open = 0xa517,
    /// <summary><c>DrawInst_DownwardsGateOpenClosed_0</c>: the gate a quarter down.</summary>
    Lowered1 = 0xa525,
    /// <summary><c>DrawInst_DownwardsGateOpenClosed_1</c>: the gate halfway down.</summary>
    Lowered2 = 0xa533,
    /// <summary><c>DrawInst_DownwardsGateOpenClosed_2</c>: the gate three quarters down.</summary>
    Lowered3 = 0xa541,
    /// <summary><c>DrawInst_DownwardsGateOpenClosed_3</c>: the gate all the way down.</summary>
    Lowered4 = 0xa54f,
    /// <summary><c>DrawInst_DownwardsGateClosed</c>.</summary>
    Closed = 0xa55d,
    /// <summary><c>DrawInst_GateShotblockDownwardsBlueLeft</c>.</summary>
    TriggerBlueLeft = 0xa5d7,
    /// <summary><c>DrawInst_GateShotblockDownwardsBlueRight</c>.</summary>
    TriggerBlueRight = 0xa5e3,
    /// <summary><c>DrawInst_GateShotblockDownwardsRedLeft</c>.</summary>
    TriggerRedLeft = 0xa5eb,
    /// <summary><c>DrawInst_GateShotblockDownwardsRedRight</c>.</summary>
    TriggerRedRight = 0xa5f7,
    /// <summary><c>DrawInst_GateShotblockDownwardsGreenLeft</c>.</summary>
    TriggerGreenLeft = 0xa5ff,
    /// <summary><c>DrawInst_GateShotblockDownwardsGreenRight</c>.</summary>
    TriggerGreenRight = 0xa60b,
    /// <summary><c>DrawInst_GateShotblockDownwardsYellowLeft</c>.</summary>
    TriggerYellowLeft = 0xa613,
    /// <summary><c>DrawInst_GateShotblockDownwardsYellowRight</c>.</summary>
    TriggerYellowRight = 0xa61f,
}
