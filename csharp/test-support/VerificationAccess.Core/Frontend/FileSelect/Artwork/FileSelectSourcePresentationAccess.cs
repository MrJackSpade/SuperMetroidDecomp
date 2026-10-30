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
            Ensure.LengthEqual(destination, FileSelectPresentationDefinitions.CellCount);
            self.pages[page].CopyTo(destination);
        }
        /// <summary>Places an ENERGY, NO DATA, or time-colon patch relative to its tile anchor.</summary>
        internal void ApplyPatch(Span<BackgroundTile> tilemap, FileSelectPatch name, Position16<TilemapCells> anchor)
        {
            FileSelectCompiledPatch patch = self.patches[name];
            for (int index = 0; index < patch.Count; index++)
            {
                FileSelectCompiledPatchCell cell = patch.Cell(index);
                tilemap[FileSelectPresentationDefinitions.PageSize.RowMajorIndex(anchor + cell.Offset)] = cell.Tile;
            }
        }
        /// <summary>Writes one decimal digit using edited glyph art when supplied.</summary>
        internal void WriteDigit(Span<BackgroundTile> tilemap, Position16<TilemapCells> anchor, int offset, int digit) =>
            tilemap[anchor.Y * FileSelectPresentationDefinitions.Width + anchor.X + offset] = self.Digit(digit);
        /// <summary>Writes the selected slot's letter at its dynamic tile anchor.</summary>
        internal void WriteSlotLetter(Span<BackgroundTile> tilemap, Position16<TilemapCells> anchor, int slot) =>
            tilemap[anchor.Y * FileSelectPresentationDefinitions.Width + anchor.X] = self.SlotLetter(slot);
        /// <summary>
        /// The tiles of <paramref name="shown"/>'s page, row by row, as the cartridge writes them: the page, each slot's field
        /// and the slot letters its prompt names, so verification compares the menus' pictures' sources with the cartridge's.
        /// </summary>
        internal BackgroundTile[] LayOut(ShownFileSelectPage shown)
        {
            Ensure.NotNull(shown);
            var tiles = new BackgroundTile[FileSelectPresentationDefinitions.CellCount];
            self.CopyPage(shown.Page, tiles);
            foreach (PlacedFrame placed in shown.Pictures(self).Skip(1))
            {
                ScreenPictureSubject glyph = self.FieldSubjects().First(subject => subject.Name == placed.Frame);
                Position16<TilemapCells> cell = placed.Cell;
                for (int index = 0; index < glyph.Tiles.Length; index++)
                {
                    if (glyph.Tiles[index] is not { } tile)
                        continue;
                    int x = cell.X + glyph.Offset.X + index % glyph.Size.X, y = cell.Y + glyph.Offset.Y + index / glyph.Size.X;
                    tiles[y * FileSelectPresentationDefinitions.Width + x] = tile;
                }
            }
            return tiles;
        }
    }
}
