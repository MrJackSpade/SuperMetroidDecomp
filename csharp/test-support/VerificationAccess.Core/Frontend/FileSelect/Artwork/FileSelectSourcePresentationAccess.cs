using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>Verification access to <see cref="FileSelectSourcePresentation"/> members production does not use.</summary>
internal static class FileSelectSourcePresentationAccess
{
    /// <summary>Exposes <see cref="FileSelectSourcePresentation"/> state that only verification reads.</summary>
    extension(FileSelectSourcePresentation self)
    {
        /// <summary>Copies one named full page into a 1024-cell destination tilemap.</summary>
        internal void CopyPage(FileSelectPage page, Span<BackgroundTile> destination)
        {
            Ensure.LengthEqual(destination, FileSelectPresentationFormat.CellCount);
            self.pages[page].CopyTo(destination);
        }
        /// <summary>Places an ENERGY, NO DATA, or time-colon patch relative to its tile anchor.</summary>
        internal void ApplyPatch(Span<BackgroundTile> tilemap, FileSelectPatch name, Position16<CharacterCells> anchor)
        {
            FileSelectCompiledPatch patch = self.patches[name];
            for (int index = 0; index < patch.Count; index++)
            {
                FileSelectCompiledPatchCell cell = patch.Cell(index);
                tilemap[FileSelectPresentationFormat.PageSize.RowMajorIndex(anchor + cell.Offset)] = cell.Tile;
            }
        }
        /// <summary>Writes one decimal digit using edited glyph art when supplied.</summary>
        internal void WriteDigit(Span<BackgroundTile> tilemap, Position16<CharacterCells> anchor, int offset, int digit) =>
            tilemap[anchor.Y * FileSelectPresentationFormat.PageColumns + anchor.X + offset] = self.Digit(digit);
        /// <summary>Writes the selected slot's letter at its dynamic tile anchor.</summary>
        internal void WriteSlotLetter(Span<BackgroundTile> tilemap, Position16<CharacterCells> anchor, int slot) =>
            tilemap[anchor.Y * FileSelectPresentationFormat.PageColumns + anchor.X] = self.SlotLetter(slot);
        /// <summary>
        /// The tiles of <paramref name="shown"/>'s page, row by row, as the cartridge writes them: the page, each slot's field
        /// and the slot letters its prompt names, so verification compares the menus' pictures' sources with the cartridge's.
        /// </summary>
        internal BackgroundTile[] LayOut(ShownFileSelectPage shown)
        {
            Ensure.NotNull(shown);
            var tiles = new BackgroundTile[FileSelectPresentationFormat.CellCount];
            self.CopyPage(shown.Page, tiles);
            foreach (PlacedFrame placed in shown.Pictures(self).Skip(1))
            {
                ScreenPictureSubject glyph = self.FieldSubjects().First(subject => subject.Name == placed.Frame);
                Position16<CharacterCells> cell = CellOf(placed);
                for (int index = 0; index < glyph.Tiles.Length; index++)
                {
                    if (glyph.Tiles[index] is not { } tile)
                        continue;
                    int x = cell.X + glyph.Offset.X + index % glyph.Size.X, y = cell.Y + glyph.Offset.Y + index / glyph.Size.X;
                    tiles[y * FileSelectPresentationFormat.PageColumns + x] = tile;
                }
            }
            return tiles;
        }
    }

    /// <summary>The page cell <paramref name="placed"/>'s origin lies on, as the cartridge writes its field there.</summary>
    /// <exception cref="InvalidDataException">The picture's origin lies between the page's cells.</exception>
    private static Position16<CharacterCells> CellOf(PlacedFrame placed)
    {
        const int pixels = CharacterCellConversions.CellPixels;
        if (placed.Origin.X % pixels != 0 || placed.Origin.Y % pixels != 0)
            throw new InvalidDataException($"File-select picture {placed.Frame} is placed at {placed.Origin}, between the page's cells.");
        return new(placed.Origin.X / pixels, placed.Origin.Y / pixels);
    }
}
