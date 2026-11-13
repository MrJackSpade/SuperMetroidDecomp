using SuperMetroid.Core.Fx.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room effect's artwork as a gameplay frame shows it on the effect overlay (#1403, #1514, #1706): a liquid's surface
/// strip and body or an atmosphere's picture, as the room effects' animations select them, or none where the overlay shows
/// nothing. The HUD is its own layer (#1466).
/// </summary>
/// <param name="Shown">The effect's artwork, or null where the overlay shows nothing.</param>
public sealed record FxOverlayArtwork(RoomEffectArtwork? Shown);
