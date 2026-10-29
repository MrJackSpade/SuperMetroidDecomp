using SuperMetroid.Core.World;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>
/// Verification access at the native boundary (#1464): the sprite palette rows actors' colourings occupy, for checks that
/// compare against the cartridge's palette words, and the colourings a native row denotes under a loaded graphics set,
/// for checks that import native state.
/// </summary>
internal static class SpriteColouringAccess
{
    /// <summary>The shared set each role row holds, which a row imported or recorded as a number denotes.</summary>
    private static readonly Dictionary<int, SharedSpriteColouring> RoleRows = new()
    {
        [SpriteColourSetDefinitions.Common.Set] = SharedSpriteColouring.Common,
        [SpriteColourSetDefinitions.EnemyProjectiles.Set] = SharedSpriteColouring.EnemyProjectiles,
        [SpriteColourSetDefinitions.Frozen.Set] = SharedSpriteColouring.Frozen,
    };

    /// <summary>Rows of an enemy's colours.</summary>
    extension(RoomEnemySlot self)
    {
        /// <summary>The row holding the slot's colours under <paramref name="enemies"/>' loaded graphics set.</summary>
        internal int PaletteRowIn(RoomEnemySystem enemies) => enemies.Presentation.Placement.StoreOf(self.Colouring).Set;

        /// <summary>Gives the slot the colours native row <paramref name="row"/> denotes under <paramref name="enemies"/>' loaded set.</summary>
        internal void SetPaletteRow(RoomEnemySystem enemies, int row) => self.Colouring = enemies.GraphicsSet.ColouringInRow(row);
    }

    /// <summary>Rows of a sprite's graphics.</summary>
    extension(SpriteGraphics self)
    {
        /// <summary>The row holding the graphics' colours under <paramref name="enemies"/>' loaded graphics set.</summary>
        internal int PaletteRowIn(RoomEnemySystem enemies) => enemies.Presentation.Placement.StoreOf(self.Colouring).Set;

        /// <summary>The row holding colours that need no graphics set: a shared role's or an encounter's.</summary>
        internal int FixedPaletteRow => GraphicsSetColourPlacement.FixedStore(self.Colouring).Set;
    }

    /// <summary>Rows of a loaded graphics set's members.</summary>
    extension(EnemyGraphicsSet self)
    {
        /// <summary>The row holding the colours <paramref name="enemy"/> draws in under the set.</summary>
        internal int DrawRow(EnemyDefinitionId enemy) => self.Placement.StoreOf(self.DrawIndexes(enemy).Colouring).Set;

        /// <summary>
        /// The colouring native sprite palette row <paramref name="row"/> denotes under the set, for state imported as
        /// rows: the shared set its role places there, the member the set loaded into it last, whose colours it holds,
        /// or, for a row neither names, a fixture colouring held in that row. The set's placement places the result back
        /// in <paramref name="row"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The row is not a sprite palette row.</exception>
        internal SpriteColouring ColouringInRow(int row)
        {
            Ensure.BetweenInclusive(0, ColourSets.SetCount - 1, row);
            if (RoleRows.TryGetValue(row, out SharedSpriteColouring? role))
                return role;
            for (int index = self.Entries.Count - 1; index >= 0; index--)
            {
                if (self.Entries[index].ColourStore.Set == row)
                    return new GraphicsSetMemberColouring(self.Entries[index].DefinitionId);
            }
            return SpriteColouring.InFixtureSet(row);
        }
    }
}
