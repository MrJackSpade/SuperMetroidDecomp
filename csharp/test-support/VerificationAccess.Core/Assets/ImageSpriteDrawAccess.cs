namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="ImageSpriteDraw"/> members production does not use.</summary>
internal static class ImageSpriteDrawAccess
{
    /// <summary>Exposes <see cref="ImageSpriteDraw"/> state that only verification reads.</summary>
    extension(ImageSpriteDraw self)
    {
        /// <summary>The colour group's place times sixteen plus colour of the image pixel shown at <paramref name="cell"/>.</summary>
        public int PaletteIndexAt(Position16<ScreenPixels> cell) => self.Image.Sheet.Inks[self.SheetIndex(cell)].PaletteIndex;
        /// <summary>The part colour group of the image pixel shown at <paramref name="cell"/>.</summary>
        public PartColourGroup PartGroupAt(Position16<ScreenPixels> cell) => self.Image.Sheet.Inks[self.SheetIndex(cell)].PartGroup;
        /// <summary>The priority against the backgrounds 0..3 of the image pixel shown at <paramref name="cell"/>.</summary>
        public int PriorityAt(Position16<ScreenPixels> cell) => self.Image.Sheet.Inks[self.SheetIndex(cell)].Priority;
        /// <summary>Where on the sheet the pixel shown at <paramref name="cell"/> lies.</summary>
        internal int SheetIndex(Position16<ScreenPixels> cell)
        {
            int sourceX = (self.Mirror & SpriteMirror.Horizontal) != 0 ? self.Image.Size.X - 1 - cell.X : cell.X;
            int sourceY = (self.Mirror & SpriteMirror.Vertical) != 0 ? self.Image.Size.Y - 1 - cell.Y : cell.Y;
            return (self.Image.Corner.Y + sourceY) * self.Image.Sheet.Size.X + self.Image.Corner.X + sourceX;
        }
    }
}
