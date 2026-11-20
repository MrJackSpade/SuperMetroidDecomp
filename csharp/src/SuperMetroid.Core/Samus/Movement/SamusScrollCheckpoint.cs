namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// What the camera holds of Samus's point at the end of its last scrolling pass (#2558): the point, or, for a camera
/// placed since its last scroll (a room's entry), that no scrolling pass has run since. Each reader names what it uses
/// for a camera placed since its last scroll.
/// </summary>
public abstract record SamusScrollCheckpoint
{
    /// <summary>Only the checkpoints below exist.</summary>
    private protected SamusScrollCheckpoint()
    {
    }

    /// <summary>The camera has been placed and no scrolling pass has run since.</summary>
    public static SamusScrollCheckpoint SincePlacement { get; } = new SamusCameraPlacedCheckpoint();

    /// <summary>The checkpoint a scrolling pass that ends with Samus at <paramref name="point"/> leaves.</summary>
    /// <param name="point">Samus's point as the pass ends.</param>
    public static SamusScrollCheckpoint At(FixedPosition<RoomPixels> point) => new SamusScrolledCheckpoint(point);
}
