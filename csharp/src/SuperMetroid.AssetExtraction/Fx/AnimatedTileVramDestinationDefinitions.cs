using SuperMetroid.Core;
using System.Collections.Frozen;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>
/// The VRAM word each animated-tile object's frames are copied to (#1406): the cartridge's layout, which extraction alone
/// needs, to picture each frame over the characters its animation paints and to declare those characters.
/// </summary>
[EncodingOwner("VRAM words of the animated-tile transfers")]
internal static class AnimatedTileVramDestinationDefinitions
{
    /// <summary>
    /// The VRAM word each object header copies every frame to: the horizontal spikes, Maridia's sand ceiling and falling
    /// sand, lava, acid (objects $82AB/$82C9), rain ($87:82E7) and the spores over BG3's characters, the Wrecked Ship
    /// entrance treadmills ($87:8275 and $87:827B), and the four Tourian statues.
    /// </summary>
    internal static readonly FrozenDictionary<BlockAnimation, ushort> Destinations = new Dictionary<BlockAnimation, ushort>
    {
        [BlockAnimation.HorizontalSpikes] = 0x3d60,
        [BlockAnimation.MaridiaSandCeiling] = 0x1000,
        [BlockAnimation.MaridiaSandFalling] = 0x1020,
        [BlockAnimation.Lava] = 0x4280,
        [BlockAnimation.Acid] = 0x4280,
        [BlockAnimation.Rain] = 0x4280,
        [BlockAnimation.Spores] = 0x4280,
        [BlockAnimation.WreckedShipTreadmillRightwards] = 0x00e0,
        [BlockAnimation.WreckedShipTreadmillLeftwards] = 0x00e0,
        [BlockAnimation.TourianStatuePhantoon] = 0x7800,
        [BlockAnimation.TourianStatueRidley] = 0x7220,
        [BlockAnimation.TourianStatueKraid] = 0x0b40,
        [BlockAnimation.TourianStatueDraygon] = 0x0ca0,
    }.ToFrozenDictionary();
}
