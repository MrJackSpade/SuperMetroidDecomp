using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Fx;

/// <summary>Cartridge values and helpers of <see cref="RoomFxRomData.Layer3"/> that only verification reads.</summary>
internal static class RoomFxRomDataLayer3Constants
{
        /// <summary>
        /// Blank tilemap word copied through the gameplay portion of the first BG3 page by
        /// <c>Load_StandardBG3Tiles_SpriteTiles_ClearTilemaps</c> at $82:82E2.
        /// </summary>
        public const ushort PaddingTilemapWord = 0x006f;
        /// <summary>Words from BG3 row four through the end of its first 32-row page.</summary>
        public const int PaddingWordCount = 0x0380;
        /// <summary>
        /// BG3SC base used by liquid effects. Its vertical-size bit exposes the HUD/padding
        /// page followed by the room-effect page as one 32-by-64 tilemap.
        /// </summary>
        public const ushort LiquidTilemapBaseWord = 0x5800;
        /// <summary>
        /// BG3SC base selected directly by the rain and fog initializers at $88:D950 and
        /// $88:DB08. These effects use only the second 32-by-32 page.
        /// </summary>
        public const ushort FullScreenAtmosphereTilemapBaseWord = 0x5c00;
        /// <summary>The liquid BG3 tilemap: one screen wide, two screens tall (32 by 64 characters).</summary>
        public static Extent16<TilemapCells> LiquidTilemapSize =>
            new(SnesPpuLayout.TilemapPageWidthInTiles, 2 * SnesPpuLayout.TilemapPageWidthInTiles);
        /// <summary>Rain and fog's BG3 tilemap: one 32-by-32 screen.</summary>
        public static Extent16<TilemapCells> FullScreenAtmosphereTilemapSize =>
            new(SnesPpuLayout.TilemapPageWidthInTiles, SnesPpuLayout.TilemapPageWidthInTiles);
        /// <summary>
        /// First word cleared by <c>Clear_FX_Tilemap</c> at $82:E566 and
        /// <c>ClearFXTilemap</c> at $80:A29C before a room's effect tilemap is uploaded.
        /// </summary>
        public const ushort ClearDestinationWord = 0x5880;
        /// <summary>VRAM word destination of a room effect's layer-three tilemap.</summary>
        public const ushort TilemapDestinationWord = 0x5be0;
}
