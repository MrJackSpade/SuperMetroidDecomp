namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>How the pause equipment page shows the reserve-tank arrow.</summary>
internal enum ReserveArrowState
{
    /// <summary>The arrow's tiles use the hidden palette.</summary>
    Hidden,

    /// <summary>The arrow is shown in its solid colours.</summary>
    Solid,

    /// <summary>The arrow is shown pulsing, for automatic reserve mode.</summary>
    Animated,
}
