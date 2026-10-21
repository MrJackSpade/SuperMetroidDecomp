
namespace SuperMetroid.Core.Enemies;

/// <summary>VRAM that enemy and boss routines write directly or through the NMI queue (#630).</summary>
[PpuLayoutBoundary("catalog: enemy and boss VRAM")]
public static class EnemyVramLayout
{

    /// <summary>
    /// VMADD $4800-$4FFF: the enemy BG2 image that boss routines stage at <c>$7E:2000</c> and
    /// upload over the room's BG2 tilemap (Phantoon, Draygon, Crocomire, Mother Brain).
    /// </summary>
    public static readonly PpuLayoutCell EnemyBg2Tilemap = new(nameof(EnemyBg2Tilemap), PpuLayoutRegion.Vram,
        SuperMetroid.Core.Enemies.Artwork.EnemyBg2FrameLayout.VramBase, SuperMetroid.Core.Enemies.Artwork.EnemyBg2FrameLayout.TilemapWordCount, "BG tilemap words (32x64)",
        GameplayVramLayout.Bg2Tilemap);

    /// <summary>
    /// VMADD $6C00-$7FFF: the enemy graphics-set character area that room loading fills from
    /// each enemy's tile data (<c>$A0:8C6C</c>, VRAM byte $D800 onward).
    /// </summary>
    public static readonly PpuLayoutCell EnemyTiles = new(nameof(EnemyTiles), PpuLayoutRegion.Vram,
        RoomEnemyRomLayout.VramByteBase / sizeof(ushort), 0x1400, "4-bpp OBJ characters", GameplayVramLayout.ObjectCharacters);

}
