using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// The room effect's page as a gameplay frame shows it on the effect overlay (#1403, #1514): its picture as the room
/// effects' animations select it, as its part behind and its part in front (#1634), or none where the overlay shows no
/// page. The HUD is its own layer (#1466).
/// </summary>
/// <param name="Picture">The page's picture, or null where the overlay shows nothing.</param>
public sealed record FxOverlayPage(LayeredPicture? Picture);
