using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>A map scroll's state, compared between two scrolls framed from the same map.</summary>
/// <param name="Scroll">The scroll position.</param>
/// <param name="Scrollable">The directions its arrows let it scroll, which its revealed map's reach decides.</param>
/// <param name="Direction">The step under way.</param>
internal readonly record struct ScrollStateResult(Offset16<MapPixels> Scroll, MapScrollDirection[] Scrollable, MapScrollDirection? Direction)
{
    /// <summary>Whether both states stand at one scroll, may scroll the same ways and take the same step.</summary>
    public bool Equals(ScrollStateResult other) =>
        Scroll == other.Scroll && Scrollable.SequenceEqual(other.Scrollable) && Direction == other.Direction;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Scroll, Scrollable.Length, Direction);
}
