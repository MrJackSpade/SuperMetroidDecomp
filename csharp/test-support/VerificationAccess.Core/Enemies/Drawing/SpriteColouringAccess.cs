using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// Verification access at the native boundary (#1464, #1512): the sprite palette row the cartridge would select for an
/// actor's draw, for checks that compare against its palette words, and the shading a native row denotes for an actor,
/// for checks that import native state. Rows are the import's knowledge (<see cref="GraphicsSetRows"/>); gameplay names
/// shades only.
/// </summary>
internal static class SpriteColouringAccess
{
    /// <summary>The shared row each selection shade selects, which a draw showing it selects on the cartridge.</summary>
    private static readonly Dictionary<PictureShade, int> ShadeRows = new()
    {
        [EnemyShades.HitFlash] = EnemySheetRowDefinitions.Common,
        [EnemyShades.EnemyProjectileColours] = EnemySheetRowDefinitions.EnemyProjectiles,
        [EnemyShades.Frozen] = EnemySheetRowDefinitions.Beams,
        [NinjaSpacePirateShades.GoldWall] = EnemySheetRowDefinitions.NinjaGoldWall,
        [CeresDoorShades.Normal] = EnemySheetRowDefinitions.CeresDoorActive,
        [CeresDoorShades.Escaping] = EnemySheetRowDefinitions.CeresDoorActive,
        [TourianCorpseShades.Drained] = EnemySheetRowDefinitions.DrainedCorpses,
    };

    /// <summary>
    /// The row a draw under <paramref name="shading"/> selects, for a sprite whose sheet is coloured in
    /// <paramref name="standardRow"/>: the row its last selection shade selects, otherwise the sheet's own.
    /// </summary>
    internal static int RowSelectedBy(PictureShading shading, int standardRow) => SelectedRow(shading) ?? standardRow;

    /// <summary>The row the last selection shade of <paramref name="shading"/> selects, or null when it selects none.</summary>
    internal static int? SelectedRow(PictureShading shading)
    {
        Ensure.NotNull(shading);
        int? row = null;
        foreach (PictureShade shade in shading.Shades)
        {
            if (ShadeRows.TryGetValue(shade, out int selected))
                row = selected;
        }
        return row;
    }

    /// <summary>
    /// The shading a native row <paramref name="row"/> denotes for a sprite whose sheet is coloured in
    /// <paramref name="standardRow"/>: its standard colours in its own row, otherwise the shared selection the row is.
    /// </summary>
    /// <exception cref="InvalidDataException">The row is neither the sprite's own nor a shared selection's.</exception>
    internal static PictureShading ShadingInRow(int row, int standardRow)
    {
        if (row == standardRow)
            return PictureShading.Standard;
        return row switch
        {
            EnemySheetRowDefinitions.Common => EnemyShades.HitFlashed,
            EnemySheetRowDefinitions.EnemyProjectiles => EnemyShades.InEnemyProjectileColours,
            EnemySheetRowDefinitions.Beams => EnemyShades.FrozenLook,
            _ => throw new InvalidDataException($"Native palette row {row} is neither the sprite's own row {standardRow} nor a shared selection's."),
        };
    }

    /// <summary>Rows of an enemy's colours.</summary>
    extension(RoomEnemySlot self)
    {
        /// <summary>The row the slot's draws select under <paramref name="enemies"/>' loaded graphics set.</summary>
        internal int PaletteRowIn(RoomEnemySystem enemies) =>
            RowSelectedBy(self.Shading, enemies.GraphicsSet.DrawRow(self.DefinitionId));

        /// <summary>Gives the slot the shading native row <paramref name="row"/> denotes under <paramref name="enemies"/>' loaded set.</summary>
        internal void SetPaletteRow(RoomEnemySystem enemies, int row) =>
            self.Shading = ShadingInRow(row, enemies.GraphicsSet.DrawRow(self.DefinitionId));
    }

    /// <summary>Rows of a sprite's graphics.</summary>
    extension(SpriteGraphics self)
    {
        /// <summary>
        /// The row the graphics' draws select under <paramref name="enemies"/>' loaded graphics set: their sheet's, the row
        /// of the member drawing at their tile base or the common row at character zero, unless a selection shade names another.
        /// </summary>
        internal int PaletteRowIn(RoomEnemySystem enemies) => RowSelectedBy(self.Shading, enemies.GraphicsSet.RowAtTileBase(self.TileBase));

        /// <summary>The row graphics at character zero select: the common row, unless a selection shade names another.</summary>
        internal int FixedPaletteRow => RowSelectedBy(self.Shading, EnemySheetRowDefinitions.Common);
    }

    /// <summary>Rows of a loaded graphics set's members.</summary>
    extension(EnemyGraphicsSet self)
    {
        /// <summary>The row <paramref name="enemy"/>'s sheets are coloured in under the set.</summary>
        internal int DrawRow(EnemyDefinitionId enemy) => new GraphicsSetRows(self.LoadedSet).RowOf(enemy);

        /// <summary>The row of the first member drawing at tile base <paramref name="tileBase"/>, or the common row at character zero.</summary>
        internal int RowAtTileBase(int tileBase)
        {
            if (tileBase == 0)
                return EnemySheetRowDefinitions.Common;
            foreach (RoomEnemyGraphicsSetEntry entry in self.Entries)
            {
                if (self.DrawIndexes(entry.DefinitionId).TileBase == tileBase)
                    return self.DrawRow(entry.DefinitionId);
            }
            throw new InvalidDataException($"No member of graphics set {self.LoadedSet.Name} draws at tile base {tileBase}.");
        }

        /// <summary>The shading native row <paramref name="row"/> denotes for a sprite drawn by <paramref name="owner"/> under the set.</summary>
        internal PictureShading ShadingInRow(int row, EnemyDefinitionId owner) => SpriteColouringAccess.ShadingInRow(row, self.DrawRow(owner));
    }
}
