using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Native atmospheric OBJ identity progression and palette routing, with narrowly retained drawn-layer priority.</summary>
internal static class SamusAtmosphericArtworkDefinitions
{
    /// <summary>$90:8C0F, AtmosphericGraphics_SpriteTileNumberAttributes_1_Footstep: first of four consecutive footstep OBJ tiles.</summary>
    internal const int FootstepFirstTile = 0x02C;
    /// <summary>$90:8C17, AtmosphericGraphics_SpriteTileNumberAttribute_4_6_7_LavaDust: first of four consecutive shared lava/dust OBJ tiles.</summary>
    internal const int LavaDustFirstTile = 0x048;
    /// <summary>$90:8C0F-$8C1E: route the initial enemy-projectile colors at CGRAM208 to their OBJ palette row.</summary>
    /// <remarks>RoomLoadingRomData.InitialEnemyProjectilePaletteCgramIndex uses the same destination.
    /// OBJ colors occupy the upper half of CGRAM, with sixteen colors per row.</remarks>
    internal const int Palette = (GameplayBasePaletteFormat.EnemyProjectileInitialColor - SnesCgram.ColorCount / 2) /
        GameplayBasePaletteFormat.SpriteColorCount;
    /// <summary>$90:8C0F-$8C1E: selected OBJ priority two, retained as the exact atmospheric drawing-layer composition.</summary>
    /// <remarks>This materially determines visibility against background layers. Only the eight
    /// original attribute words are covered; independent tile pixels, RGB colors and timing are not exempt.</remarks>
    internal const int Priority = 2;

    internal static ushort Attributes(bool footstep, int frame)
    {
        if ((uint)frame >= SamusMovementRomData.Environment.DirectAtmosphericFrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return SnesObjAttributeWord.Create((footstep ? FootstepFirstTile : LavaDustFirstTile) + frame,
            Palette, Priority).Raw;
    }
}
