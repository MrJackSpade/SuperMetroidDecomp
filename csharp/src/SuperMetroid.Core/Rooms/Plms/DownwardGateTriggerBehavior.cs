namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>
/// The downward-gate trigger a shootable block's behaviour selects (decoded by <c>RoomBlockBehaviorCodec</c>): the
/// trigger's colour, and whether it sits left or right of its gate.
/// </summary>
public enum DownwardGateTriggerBehavior
{
    /// <summary>The blue trigger left of its gate.</summary>
    BlueLeft,
    /// <summary>The blue trigger right of its gate.</summary>
    BlueRight,
    /// <summary>The red trigger left of its gate.</summary>
    RedLeft,
    /// <summary>The red trigger right of its gate.</summary>
    RedRight,
    /// <summary>The green trigger left of its gate.</summary>
    GreenLeft,
    /// <summary>The green trigger right of its gate.</summary>
    GreenRight,
    /// <summary>The yellow trigger left of its gate.</summary>
    YellowLeft,
    /// <summary>The yellow trigger right of its gate.</summary>
    YellowRight,
}
