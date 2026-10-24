using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The VRAM word each animated-tile object's frames are copied to (#1406): the cartridge's layout, which extraction alone
/// needs, to picture each frame over the characters its animation paints and to declare those characters.
/// </summary>
internal static class AnimatedTileVramDestinations
{
    /// <summary>The VRAM word <paramref name="animatedObject"/>'s object header copies every frame to.</summary>
    /// <exception cref="InvalidOperationException">The object copies no frames the port pictures.</exception>
    internal static ushort WordOf(AnimatedTileObject animatedObject) => animatedObject switch
    {
        // The horizontal spikes.
        AnimatedTileObject.HorizontalSpikes => 0x3d60,
        // Maridia's sand ceiling and falling sand.
        AnimatedTileObject.MaridiaSandCeiling => 0x1000,
        AnimatedTileObject.MaridiaSandFalling => 0x1020,
        // Lava and acid (objects $82AB/$82C9) and rain ($87:82E7), over BG3's characters.
        AnimatedTileObject.Lava or AnimatedTileObject.Acid or AnimatedTileObject.Rain => 0x4280,
        // The spores, over BG3's characters.
        AnimatedTileObject.Spores => 0x4280,
        // $87:8275 and $87:827B: the Wrecked Ship entrance treadmills.
        AnimatedTileObject.WreckedShipTreadmillRightwards or AnimatedTileObject.WreckedShipTreadmillLeftwards => 0x00e0,
        // The four Tourian statues' animated-tile objects.
        AnimatedTileObject.TourianStatuePhantoon => 0x7800,
        AnimatedTileObject.TourianStatueRidley => 0x7220,
        AnimatedTileObject.TourianStatueKraid => 0x0b40,
        AnimatedTileObject.TourianStatueDraygon => 0x0ca0,
        AnimatedTileObject.None or AnimatedTileObject.Empty or AnimatedTileObject.VerticalSpikes or AnimatedTileObject.CrateriaLake or
            AnimatedTileObject.UnusedCrateriaLava or AnimatedTileObject.BrinstarPlant or AnimatedTileObject.WreckedShipScreen =>
            throw new InvalidOperationException($"Animated-tile object {animatedObject} copies no frames the port pictures."),
        _ => throw new InvalidOperationException($"Undefined {nameof(AnimatedTileObject)} {animatedObject}."),
    };
}
