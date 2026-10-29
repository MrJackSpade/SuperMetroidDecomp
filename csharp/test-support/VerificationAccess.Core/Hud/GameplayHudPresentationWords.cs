using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Hud.Artwork;

/// <summary>
/// Runs the HUD presentation's writers over the cartridge's packed tilemap words: each word is placed as its tile, the
/// production writer changes the tiles, and every tile is packed back, so verification can compare the HUD rows with the
/// words the cartridge's writers produce.
/// </summary>
internal static class GameplayHudPresentationWords
{
    /// <summary>The HUD writers over packed words.</summary>
    extension(GameplayHudPresentation presentation)
    {
        /// <summary>Writes the complete HUD template into packed words.</summary>
        internal void ApplyTemplate(Span<ushort> words) => Edit(words, tiles => presentation.ApplyTemplate(tiles));

        /// <summary>Draws an item icon into packed words when its anchor cell is blank.</summary>
        internal void TryApplyIcon(Span<ushort> words, int itemIndex) => Edit(words, tiles => presentation.TryApplyIcon(tiles, itemIndex));

        /// <summary>Draws energy tanks and the energy remainder into packed words, as the HUD draws a health update.</summary>
        internal void ApplyEnergy(Span<ushort> words, int health, int maxHealth) => Edit(words, tiles =>
        {
            int tanks = Math.Min(maxHealth / 100, GameplayHudDefinitions.EnergyTankCount);
            for (int tank = 0; tank < tanks; tank++)
                presentation.DrawEnergyTank(tiles, tank, tank < health / 100 ? HudEnergyTank.Full : HudEnergyTank.Empty);
            presentation.DrawEnergyCount(tiles, health % 100);
        });

        /// <summary>Draws an ammunition count into packed words.</summary>
        internal void ApplyAmmo(Span<ushort> words, int itemIndex, int value) => Edit(words, tiles => presentation.ApplyAmmo(tiles, itemIndex, value));

        /// <summary>Draws the AUTO reserve indicator into packed words.</summary>
        internal void ApplyAutoReserve(Span<ushort> words, bool containsEnergy) =>
            Edit(words, tiles => presentation.ApplyAutoReserve(tiles, containsEnergy ? HudReserveIndicator.Filled : HudReserveIndicator.Empty));

        /// <summary>Clears the AUTO reserve indicator in packed words.</summary>
        internal void ClearAutoReserve(Span<ushort> words) => Edit(words, tiles => presentation.ClearAutoReserve(tiles));

        /// <summary>Applies an icon highlight colour variant in packed words.</summary>
        internal void ToggleItemHighlight(Span<ushort> words, HudItem selectedItem, int palette) =>
            Edit(words, tiles => presentation.ToggleItemHighlight(tiles, selectedItem, palette));
    }

    /// <summary>Places <paramref name="words"/> as tiles, runs <paramref name="write"/> over them, and packs them back.</summary>
    private static void Edit(Span<ushort> words, Action<BackgroundTile[]> write)
    {
        BackgroundTile[] tiles = BackgroundTileWordBytes.Tiles(words);
        write(tiles);
        BackgroundTileWordBytes.Words(tiles).CopyTo(words);
    }
}
