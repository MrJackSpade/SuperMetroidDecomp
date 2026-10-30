using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Hud;

/// <summary>
/// Imports the HUD a native snapshot shows into the HUD's values (#1466). The cartridge rewrites its counters, AUTO
/// indicator and icons from Samus's inventory on every gameplay frame, so those import from her imported inventory and
/// the item the HUD last highlighted; the minimap's cells import from the cartridge's HUD words, which this native
/// boundary decodes.
/// </summary>
internal static class HudNativeState
{
    /// <summary>The HUD's native import.</summary>
    extension(HudState hud)
    {
        /// <summary>
        /// Sets the HUD up as <paramref name="samus"/>'s inventory shows it with <paramref name="highlighted"/> the last
        /// highlighted item, then takes each minimap cell from the native word <paramref name="artwork"/> places it at.
        /// </summary>
        /// <param name="words">The cartridge's three HUD rows, row-major.</param>
        /// <param name="samus">The imported Samus.</param>
        /// <param name="highlighted">The item the HUD last highlighted.</param>
        /// <param name="artwork">The installed HUD artwork, which places the minimap.</param>
        /// <exception cref="InvalidDataException">A minimap word is not one the HUD's minimap writes.</exception>
        internal void ImportNative(ReadOnlySpan<ushort> words, SamusState samus, HudItem highlighted, GameplayHudPresentation artwork)
        {
            Ensure.LengthEqual(words, HudStateConstants.MutableTileCount);
            Ensure.NotNull(samus);
            Ensure.NotNull(artwork);
            hud.Initialize(new HudSnapshot(samus.Health, samus.MaxHealth, samus.Missiles, samus.MaxMissiles, samus.SuperMissiles,
                samus.MaxSuperMissiles, samus.PowerBombs, samus.MaxPowerBombs, samus.EquippedItems, highlighted, samus.ReserveEnergy,
                samus.ReserveTankMode));
            hud.PreviousSelectedItem = highlighted;
            int columns = GameplayHudPresentation.MinimapSize.X;
            for (int index = 0; index < hud.Minimap.Length; index++)
            {
                var output = new Position16<TilemapCells>(checked((ushort)(index % columns)), checked((ushort)(index / columns)));
                hud.Minimap[index] = Decode(new MapTileWord(words[GameplayHudArtwork.Of(artwork).MinimapCellIndex(output)]));
            }
        }
    }

    /// <summary>What the minimap shows at a cell whose native word is <paramref name="word"/>.</summary>
    private static MinimapCell Decode(MapTileWord word)
    {
        if (word == MapTileWords.HudBlank)
            return MinimapHiddenCell.Instance;
        var shown = new MapTileWord((ushort)(word.Raw & (MapTileWord.CharacterMask | MapTileWord.FlipMask)));
        MinimapShade shade = word == shown.ForHud(explored: true) ? MinimapShade.Explored
            : word == shown.ForHud(explored: false) ? MinimapShade.Unexplored
            : word == shown.ForHud(explored: true).WithLocationBlink() ? MinimapShade.SamusLocation
            : throw new InvalidDataException($"HUD minimap word {word.Raw:x4} is not one the minimap writes.");
        return new MinimapMapCell(new MapCellShape(shown.CharacterIndex), shown.Mirroring, shade);
    }
}
