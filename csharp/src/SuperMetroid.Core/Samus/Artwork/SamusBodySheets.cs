using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Which installed sheet each body picture draws from.</summary>
public static class SamusBodySheets
{
    /// <summary>The sheet that holds a frame: the bottom sheet for frames showing only the bottom half, the top sheet otherwise.</summary>
    public static SpriteSheetId Sheet(SamusBodyFrameKey key) =>
        key.TopPicture is null && key.BottomPicture is not null ? SpriteSheetId.SamusBodyBottom : SpriteSheetId.SamusBodyTop;
}
