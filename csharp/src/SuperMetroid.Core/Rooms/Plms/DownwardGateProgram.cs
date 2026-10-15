namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The downward gate's PLM programs (#1296), in place of its bank-$84 lists at $84:BC13..BCDE.</summary>
internal enum DownwardGateProgram
{
    /// <summary>Draws the open gate and sleeps until shot (<c>InstList_PLM_DownwardsOpenGate</c>).</summary>
    Open,
    /// <summary>The open gate's list after its sleep: lowers the gate, then runs into <see cref="Closed"/>.</summary>
    Close,
    /// <summary>Draws the closed gate and sleeps until shot or Samus is below it (<c>InstList_PLM_DownwardsClosedGate</c>).</summary>
    Closed,
    /// <summary>The closed gate's list after its sleep: raises the gate, then goes to <see cref="Open"/>.</summary>
    Reopen,
    /// <summary>Draws the blue left trigger and deletes (<c>InstList_PLM_DownwardsGateShotblock_BlueLeft</c>).</summary>
    TriggerBlueLeft,
    /// <summary>Draws the blue right trigger and deletes.</summary>
    TriggerBlueRight,
    /// <summary>Draws the red left trigger and deletes.</summary>
    TriggerRedLeft,
    /// <summary>Draws the red right trigger and deletes.</summary>
    TriggerRedRight,
    /// <summary>Draws the green left trigger and deletes.</summary>
    TriggerGreenLeft,
    /// <summary>Draws the green right trigger and deletes.</summary>
    TriggerGreenRight,
    /// <summary>Draws the yellow left trigger and deletes.</summary>
    TriggerYellowLeft,
    /// <summary>Draws the yellow right trigger and deletes.</summary>
    TriggerYellowRight,
}
