using SuperMetroid.Core.World;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// Verification access: the sprite palette rows actors' colourings occupy (#1464), for checks that compare against the
/// cartridge's palette words, and the colourings a native row denotes, for checks that import native state.
/// </summary>
internal static class SpriteColouringAccess
{
    /// <summary>Rows of an enemy's colours.</summary>
    extension(RoomEnemySlot self)
    {
        /// <summary>The row holding the slot's colours under <paramref name="enemies"/>' loaded graphics set.</summary>
        internal int PaletteRowIn(RoomEnemySystem enemies) => enemies.Presentation.ColouringRows.Row(self.Colouring);

        /// <summary>Gives the slot the colours native row <paramref name="row"/> denotes under <paramref name="enemies"/>' loaded set.</summary>
        internal void SetPaletteRow(RoomEnemySystem enemies, int row) => self.Colouring = enemies.Presentation.ColouringRows.InRow(row);
    }

    /// <summary>Rows of a sprite's graphics.</summary>
    extension(SpriteGraphics self)
    {
        /// <summary>The row holding the graphics' colours under <paramref name="enemies"/>' loaded graphics set.</summary>
        internal int PaletteRowIn(RoomEnemySystem enemies) => enemies.Presentation.ColouringRows.Row(self.Colouring);

        /// <summary>The row holding colours that need no graphics set: a shared set's, an encounter's or an unnamed row.</summary>
        internal int FixedPaletteRow => SpriteColouringRows.FixedRow(self.Colouring);
    }

    /// <summary>Rows of a loaded graphics set's members.</summary>
    extension(EnemyGraphicsSet self)
    {
        /// <summary>The row holding the colours <paramref name="enemy"/> draws in under the set.</summary>
        internal int DrawRow(EnemyDefinitionId enemy) => new SpriteColouringRows(self).Row(self.DrawIndexes(enemy).Colouring);
    }
}
