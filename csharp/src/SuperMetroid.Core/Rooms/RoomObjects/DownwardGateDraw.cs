namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The downward gate's draws (#1296, #1994): its column open, lowering and closed, and its coloured triggers.</summary>
internal enum DownwardGateDraw
{
    /// <summary>The gate open: its top block alone.</summary>
    Open,
    /// <summary>The gate a quarter down.</summary>
    QuarterLowered,
    /// <summary>The gate halfway down.</summary>
    HalfLowered,
    /// <summary>The gate three quarters down.</summary>
    ThreeQuartersLowered,
    /// <summary>The gate all the way down.</summary>
    FullyLowered,
    /// <summary>The gate closed.</summary>
    Closed,
    /// <summary>The blue trigger on the gate's left.</summary>
    TriggerBlueLeft,
    /// <summary>The blue trigger on the gate's right.</summary>
    TriggerBlueRight,
    /// <summary>The red trigger on the gate's left.</summary>
    TriggerRedLeft,
    /// <summary>The red trigger on the gate's right.</summary>
    TriggerRedRight,
    /// <summary>The green trigger on the gate's left.</summary>
    TriggerGreenLeft,
    /// <summary>The green trigger on the gate's right.</summary>
    TriggerGreenRight,
    /// <summary>The yellow trigger on the gate's left.</summary>
    TriggerYellowLeft,
    /// <summary>The yellow trigger on the gate's right.</summary>
    TriggerYellowRight,
}
