
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The intro's shown narration tilemap as checks read it (#1358, #1467): at its native words in an otherwise empty VRAM.</summary>
internal static class IntroTilemapsFixtures
{
    /// <summary>A VRAM holding <paramref name="tilemaps"/> at their native words.</summary>
    [WholeImageFixture("the intro tilemaps' VRAM image")]
    internal static SnesVram Vram(IntroTilemaps tilemaps)
    {
        var vram = new SnesVram();
        vram.LoadBytes(0, IntroScreenMemory.Compose(new byte[SnesVram.ByteCount], null, tilemaps));
        return vram;
    }
}
