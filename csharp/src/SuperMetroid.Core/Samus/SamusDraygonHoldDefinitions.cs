namespace SuperMetroid.Core.Samus;

/// <summary>How Draygon holds Samus and how she escapes (#2111).</summary>
public static class SamusDraygonHoldDefinitions
{
    /// <summary>The counted presses of the directions that free Samus from Draygon's hold.</summary>
    public const int EscapePresses = 60;

    /// <summary>How far in front of Draygon, toward the side Draygon faces, the hold places Samus, in pixels.</summary>
    public const int HeldAhead = 8;

    /// <summary>How far below Draygon the hold places Samus, in pixels.</summary>
    public const int HeldBelow = 40;
}
