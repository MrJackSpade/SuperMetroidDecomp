namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Crocomire's saturating count of mouth hits.</summary>
public static class CrocomireMouthHits
{
    /// <summary>The highest count the hits reach.</summary>
    public const int MaximumCount = 15;

    /// <summary>The count plus one, saturating at <see cref="MaximumCount"/>: the value both mouth-hit routines compute.</summary>
    public static int Next(int hitCount) => Math.Min(hitCount + 1, MaximumCount);
}
