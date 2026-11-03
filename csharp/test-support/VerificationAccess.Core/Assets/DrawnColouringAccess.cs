namespace SuperMetroid.Core.Assets;

/// <summary>
/// Verification access to <see cref="DrawnColouring"/> at the native boundary (#1464): the sprite palette rows a drawn
/// colouring selects, for checks of the screen draws still coloured that way that compare with the cartridge's attribute
/// words. Enemy draws name their shading instead (#1512); <c>SpriteColouringAccess</c> gives their rows.
/// </summary>
internal static class DrawnColouringAccess
{
    /// <summary>The native rows a drawn colouring selects.</summary>
    extension(DrawnColouring self)
    {
        /// <summary>The sprite palette row the draw's placement holds its colours in: the palette bits the cartridge ORs in.</summary>
        internal int HoldingRow => self.Placement.StoreOf(self.Shows).Set;

        /// <summary>
        /// The sprite palette row a pixel of part group <paramref name="part"/> shows under the draw, as the cartridge
        /// forms it: the part's own row when the part keeps it, otherwise the part's row with the draw's palette bits ORed in.
        /// </summary>
        internal int RowOverPart(PartColourGroup part) => part.KeepsOwnColours
            ? part.Group.Number
            : part.Group.Number | self.HoldingRow;
    }
}
