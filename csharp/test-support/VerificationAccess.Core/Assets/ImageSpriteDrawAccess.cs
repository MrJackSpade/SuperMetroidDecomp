namespace SuperMetroid.Core.Assets;

/// <summary>Verification access to <see cref="ImageSpriteDraw"/> members production does not use.</summary>
internal static class ImageSpriteDrawAccess
{
    /// <summary>Exposes <see cref="ImageSpriteDraw"/> state that only verification reads.</summary>
    extension(ImageSpriteDraw self)
    {
        /// <summary>
        /// The colour group's place times sixteen plus colour of the image pixel shown at <paramref name="cell"/>; zero,
        /// the transparent colour, where a rotting corpse's rot has cleared the row (#1531).
        /// </summary>
        public int PaletteIndexAt(Position16<ScreenPixels> cell) =>
            self.SheetIndex(cell) is int index ? self.Image.Sheet.Inks[index].PaletteIndex : 0;
        /// <summary>The priority against the backgrounds 0..3 of the image pixel shown at <paramref name="cell"/>.</summary>
        /// <exception cref="InvalidOperationException">A rotting corpse's rot has cleared the row, which shows no pixel.</exception>
        public int PriorityAt(Position16<ScreenPixels> cell) => self.SheetIndex(cell) is int index ? self.Image.Sheet.Inks[index].Priority
            : throw new InvalidOperationException($"{self} shows no pixel at ({cell.X},{cell.Y}): its corpse's rot has cleared that row.");
        /// <summary>
        /// Where on the sheet the pixel shown at <paramref name="cell"/> lies, as the renderer samples it: through a rotting
        /// corpse's rows, by the pixel's character column, if the draw shows any (#1531); none where the rot has cleared it.
        /// </summary>
        internal int? SheetIndex(Position16<ScreenPixels> cell)
        {
            int sourceX = (self.Mirror & SpriteMirror.Horizontal) != 0 ? self.Image.Size.X - 1 - cell.X : cell.X;
            int sourceY = (self.Mirror & SpriteMirror.Vertical) != 0 ? self.Image.Size.Y - 1 - cell.Y : cell.Y;
            if (self.CorpseRot is { } rot)
            {
                if (rot.Shown(rot.TableOf(sourceX / CorpseRotPicture.ColumnPixels), sourceY) is not int shown)
                    return null;
                sourceY = shown;
            }
            return (self.Image.Corner.Y + sourceY) * self.Image.Sheet.Size.X + self.Image.Corner.X + sourceX;
        }
    }
}
