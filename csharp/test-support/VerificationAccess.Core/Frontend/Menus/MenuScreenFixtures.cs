using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>A menu screen as software-render checks read it (#1358): its composed image and palette as PPU memory.</summary>
internal static class MenuScreenFixtures
{
    /// <summary>A VRAM holding the image <paramref name="screen"/> shows.</summary>
    internal static SnesVram Vram(MenuScreen screen)
    {
        var vram = new SnesVram();
        vram.LoadBytes(0, MenuScreenImage.Compose(screen));
        return vram;
    }

    /// <summary>A CGRAM holding <paramref name="screen"/>'s palette.</summary>
    internal static SnesCgram Cgram(MenuScreen screen)
    {
        var cgram = new SnesCgram();
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            cgram.SetColor(color, screen.Palette.Colors[color]);
        return cgram;
    }
}
