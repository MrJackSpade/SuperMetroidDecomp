namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="ImageSpriteDraw"/> members production does not use.</summary>
internal static class ImageSpriteDrawAccess
{
    /// <summary>Exposes <see cref="ImageSpriteDraw"/> state that only verification reads.</summary>
    extension(ImageSpriteDraw self)
    {
        /// <summary>
        /// The composed palette index of the image pixel shown at <paramref name="cell"/> (#1544): its colour group's place
        /// times sixteen plus its colour; zero, the transparent colour, for a transparent pixel or where a rotting corpse's
        /// rot has cleared the row (#1539).
        /// </summary>
        public int PaletteIndexAt(Position16<ScreenPixels> cell) => self.SheetIndex(cell) is int index
            ? SuperMetroid.AssetExtraction.Artwork.InstalledPictureColoursConstants.ComposedIndexOf(self.Image.Sheet, self.Image.Sheet.Inks[index])
            : 0;
        /// <summary>
        /// Where on the sheet the pixel shown at <paramref name="cell"/> lies, as the renderer samples it: through a rotting
        /// corpse's rows across the whole image, if the draw shows any (#1539); none where the rot has cleared it.
        /// </summary>
        internal int? SheetIndex(Position16<ScreenPixels> cell)
        {
            int sourceX = (self.Mirror & SpriteMirror.Horizontal) != 0 ? self.Image.Size.X - 1 - cell.X : cell.X;
            int sourceY = (self.Mirror & SpriteMirror.Vertical) != 0 ? self.Image.Size.Y - 1 - cell.Y : cell.Y;
            if (self.CorpseRot is { } rot)
            {
                if (rot.Shown(sourceY) is not int shown)
                    return null;
                sourceY = shown;
            }
            return (self.Image.Corner.Y + sourceY) * self.Image.Sheet.Size.X + self.Image.Corner.X + sourceX;
        }
    }
}
