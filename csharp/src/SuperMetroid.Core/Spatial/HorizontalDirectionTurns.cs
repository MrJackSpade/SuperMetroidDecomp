namespace SuperMetroid.Core.Spatial;

/// <summary>Turning a horizontal direction around (#2277).</summary>
public static class HorizontalDirectionTurns
{
    /// <summary>The direction opposite <paramref name="direction"/>: left for right and right for left.</summary>
    public static HorizontalDirection Reversed(this HorizontalDirection direction) =>
        direction == HorizontalDirection.Left ? HorizontalDirection.Right : HorizontalDirection.Left;
}
