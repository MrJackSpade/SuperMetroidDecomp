using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>The intro's shown tilemaps as checks read them (#1358): at their native words in an otherwise empty VRAM.</summary>
internal static class IntroTilemapsFixtures
{
    /// <summary>A VRAM holding <paramref name="tilemaps"/> at their native words.</summary>
    internal static SnesVram Vram(IntroTilemaps tilemaps)
    {
        var vram = new SnesVram();
        vram.LoadBytes(0, IntroScreenMemory.Compose(new byte[SnesVram.ByteCount], tilemaps));
        return vram;
    }
}
