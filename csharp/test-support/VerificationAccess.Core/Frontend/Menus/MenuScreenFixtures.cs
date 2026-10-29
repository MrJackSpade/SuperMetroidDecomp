
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Frontend.Menus;

/// <summary>A menu screen as cartridge-transfer checks read it (#1358): its composed image as PPU memory.</summary>
internal static class MenuScreenFixtures
{
    /// <summary>A VRAM holding the image <paramref name="screen"/> shows.</summary>
    [WholeImageFixture("the menu's 64 KiB VRAM image")]
    internal static SnesVram Vram(MenuScreen screen)
    {
        var vram = new SnesVram();
        vram.LoadBytes(0, MenuScreenMemory.Compose(screen));
        return vram;
    }
}
