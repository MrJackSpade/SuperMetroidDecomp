using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Verification access to <see cref="DrawnColouring"/> at the native boundary (#1464): fixture draws in a numbered sprite
/// set, and the sprite palette rows a drawn colouring selects, for checks that compare with the cartridge's attribute words.
/// </summary>
internal static class DrawnColouringAccess
{
    /// <summary>Draws a fixture in a numbered sprite set.</summary>
    extension(DrawnColouring)
    {
        /// <summary>A draw in the colours of sprite set <paramref name="set"/>, whose owner a fixture does not name.</summary>
        /// <exception cref="ArgumentOutOfRangeException">The set is not a sprite set.</exception>
        internal static DrawnColouring InFixtureSet(int set) =>
            new(SpriteColouring.InFixtureSet(set), FixtureSpriteColourPlacement.Instance);
    }

    /// <summary>The native rows a drawn colouring selects.</summary>
    extension(DrawnColouring self)
    {
        /// <summary>The sprite palette row the draw's placement holds its colours in: the palette bits the cartridge ORs in.</summary>
        internal int HoldingRow => self.Placement.StoreOf(self.Shows).Set;

        /// <summary>
        /// The sprite palette row a pixel of part row <paramref name="partRow"/> shows under the draw, as the cartridge
        /// forms it: the part's own row when the part keeps it, otherwise the part's row with the draw's palette bits ORed in.
        /// </summary>
        internal int RowOverPart(int partRow) => (partRow & SpriteImage.FixedPartRow) != 0
            ? partRow & SpriteImage.MaximumPaletteRow
            : partRow | self.HoldingRow;
    }
}
