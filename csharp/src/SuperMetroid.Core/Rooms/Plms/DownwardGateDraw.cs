namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The downward gate's draw lists (#1296).</summary>
internal enum DownwardGateDraw
{
    /// <summary>Downwards gate open draw.</summary>
    Open,
    /// <summary>The gate a quarter down.</summary>
    Lowered1,
    /// <summary>The gate halfway down.</summary>
    Lowered2,
    /// <summary>The gate three quarters down.</summary>
    Lowered3,
    /// <summary>The gate all the way down.</summary>
    Lowered4,
    /// <summary>Downwards gate closed draw.</summary>
    Closed,
    /// <summary>Gate shotblock downwards blue left draw.</summary>
    TriggerBlueLeft,
    /// <summary>Gate shotblock downwards blue right draw.</summary>
    TriggerBlueRight,
    /// <summary>Gate shotblock downwards red left draw.</summary>
    TriggerRedLeft,
    /// <summary>Gate shotblock downwards red right draw.</summary>
    TriggerRedRight,
    /// <summary>Gate shotblock downwards green left draw.</summary>
    TriggerGreenLeft,
    /// <summary>Gate shotblock downwards green right draw.</summary>
    TriggerGreenRight,
    /// <summary>Gate shotblock downwards yellow left draw.</summary>
    TriggerYellowLeft,
    /// <summary>Gate shotblock downwards yellow right draw.</summary>
    TriggerYellowRight,
}
