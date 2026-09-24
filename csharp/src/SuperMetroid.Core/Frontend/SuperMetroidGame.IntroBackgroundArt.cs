using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

public sealed partial class SuperMetroidGame
{
    [NonSerialized] private IntroBackgroundAtlas? introBackgroundArt;

    /// <summary>
    /// Binds installed opening artwork independently of emulated game state. A loaded
    /// debugger state keeps its timeline but displays the currently selected PNG.
    /// </summary>
    public void BindIntroBackgroundArt(IntroBackgroundAtlas? artwork)
    {
        introBackgroundArt = artwork;
        intro?.BindBackgroundArtwork(artwork);
    }
}
