namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The looks of one encounter a frame is drawn in, and whether its artwork is its look's own (#1538).</summary>
/// <param name="Looks">The places among the encounter's looks of those the frame is drawn in, in order.</param>
/// <param name="ArtworkOfLook">
/// Whether the frame's artwork is what its one look copies: a frame those copies never write reads only what the room's load
/// or an earlier look leaves there, another family's artwork, so it shows nothing.
/// </param>
internal sealed record FrameDrawnLooks(IReadOnlyList<int> Looks, bool ArtworkOfLook);
