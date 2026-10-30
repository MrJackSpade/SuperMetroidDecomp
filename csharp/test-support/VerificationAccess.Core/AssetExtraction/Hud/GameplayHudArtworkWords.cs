using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Hud;

/// <summary>
/// Runs the HUD's writers over the cartridge's packed tilemap words: each word is placed as its tile, the writer changes
/// the tiles, and every tile is packed back, so verification can compare the HUD rows with the words the cartridge's
/// writers produce.
/// </summary>
internal static class GameplayHudArtworkWords
{
    /// <summary>The HUD writers over packed words.</summary>
    extension(GameplayHudArtwork artwork)
    {
        /// <summary>Writes the complete HUD template into packed words.</summary>
        internal void ApplyTemplate(Span<ushort> words) => Edit(words, tiles => artwork.ApplyTemplate(tiles));

        /// <summary>Draws an item icon into packed words when its anchor cell is blank.</summary>
        internal void TryApplyIcon(Span<ushort> words, int itemIndex) => Edit(words, tiles => artwork.TryApplyIcon(tiles, itemIndex));

        /// <summary>Draws energy tanks and the energy remainder into packed words, as the HUD draws a health update.</summary>
        internal void ApplyEnergy(Span<ushort> words, int health, int maxHealth) => Edit(words, tiles =>
        {
            int tanks = Math.Min(maxHealth / 100, GameplayHudDefinitions.EnergyTankCount);
            for (int tank = 0; tank < tanks; tank++)
                artwork.DrawEnergyTank(tiles, tank, tank < health / 100 ? HudEnergyTank.Full : HudEnergyTank.Empty);
            artwork.DrawEnergyCount(tiles, health % 100);
        });

        /// <summary>Draws an ammunition count into packed words.</summary>
        internal void ApplyAmmo(Span<ushort> words, int itemIndex, int value) => Edit(words, tiles => artwork.ApplyAmmo(tiles, itemIndex, value));

        /// <summary>Draws the AUTO reserve indicator into packed words.</summary>
        internal void ApplyAutoReserve(Span<ushort> words, bool containsEnergy) =>
            Edit(words, tiles => artwork.ApplyAutoReserve(tiles, containsEnergy ? HudReserveIndicator.Filled : HudReserveIndicator.Empty));

        /// <summary>Clears the AUTO reserve indicator in packed words.</summary>
        internal void ClearAutoReserve(Span<ushort> words) => Edit(words, tiles => artwork.ClearAutoReserve(tiles));

        /// <summary>Applies an icon highlight colour variant in packed words.</summary>
        internal void ToggleItemHighlight(Span<ushort> words, HudItem selectedItem, int palette) =>
            Edit(words, tiles => artwork.ToggleItemHighlight(tiles, selectedItem, palette));
    }

    /// <summary>Places <paramref name="words"/> as tiles, runs <paramref name="write"/> over them, and packs them back.</summary>
    private static void Edit(Span<ushort> words, Action<BackgroundTile[]> write)
    {
        BackgroundTile[] tiles = BackgroundTileWordBytes.Tiles(words);
        write(tiles);
        BackgroundTileWordBytes.Words(tiles).CopyTo(words);
    }
}
