using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>The ending's screen as checks read it (#1358): its recorded transfers replayed into a VRAM.</summary>
internal static class EndingScreenFixtures
{
    /// <summary>A VRAM holding the tilemap transfers <paramref name="screen"/> recorded, which read no installed artwork.</summary>
    internal static SnesVram Vram(EndingScreen screen) => Vram(screen, new EndingScreenArtwork(null, null, null, null, EndingReward.Armored));

    /// <summary>A VRAM holding the transfers <paramref name="screen"/> recorded, replayed over <paramref name="artwork"/>.</summary>
    internal static SnesVram Vram(EndingScreen screen, EndingScreenArtwork artwork)
    {
        var vram = new SnesVram();
        vram.LoadBytes(0, EndingScreenImage.Compose(screen.uploads, artwork));
        return vram;
    }
}
