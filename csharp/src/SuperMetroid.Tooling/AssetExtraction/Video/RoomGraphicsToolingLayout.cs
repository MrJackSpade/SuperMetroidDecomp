using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Video;

/// <summary>Development-tool members of <see cref="RoomGraphicsLayout"/>; never linked by player hosts.</summary>
internal static class RoomGraphicsToolingLayout
{
    /// <summary>VMADD $2800-$3FFF: the common-room-element (CRE) BG characters uploaded before the area tileset.</summary>
    public static readonly PpuLayoutCell CommonRoomElementCharacters = new(nameof(CommonRoomElementCharacters), PpuLayoutRegion.Vram,
        0x2800, 0x1800, "4-bpp BG characters", GameplayVramLayout.BackgroundCharacters);
}
