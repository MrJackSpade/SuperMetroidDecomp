using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>The pause screen as software-render checks read it (#1358): its composed image and palette as PPU memory.</summary>
internal static class PauseScreenFixtures
{
    /// <summary>A VRAM holding the image <paramref name="screen"/> shows.</summary>
    internal static SnesVram Vram(PauseScreen screen)
    {
        var vram = new SnesVram();
        vram.LoadBytes(0, PauseScreenMemory.Compose(screen));
        return vram;
    }

    /// <summary>A CGRAM holding <paramref name="screen"/>'s palette.</summary>
    internal static SnesCgram Cgram(PauseScreen screen)
    {
        var cgram = new SnesCgram();
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            cgram.SetColor(color, screen.Palette.Colors[color]);
        return cgram;
    }
}
