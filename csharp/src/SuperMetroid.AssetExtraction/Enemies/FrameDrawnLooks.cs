namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>The looks of one encounter a frame is drawn in, and whether its artwork is its look's own (#1538).</summary>
/// <param name="Looks">The looks the frame is drawn in, in the order the encounter takes them.</param>
/// <param name="ArtworkOfLook">
/// Whether the frame's artwork is what its one look copies: a frame those copies never write reads only what the room's load
/// or an earlier look leaves there, another family's artwork, so it shows nothing.
/// </param>
internal sealed record FrameDrawnLooks(IReadOnlyList<ComposedLook> Looks, bool ArtworkOfLook);
