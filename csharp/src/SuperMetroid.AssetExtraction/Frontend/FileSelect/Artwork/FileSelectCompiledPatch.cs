using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>Retains edited patch cells while deriving unchanged cells from compiled stock definitions.</summary>
internal sealed class FileSelectCompiledPatch
{
    /// <summary>Schema identity used to reconstruct a stock patch.</summary>
    private readonly FileSelectPatch name;
    /// <summary>Edited cells, or null when every cell equals the compiled stock patch.</summary>
    internal readonly FileSelectCompiledPatchCell[]? suppliedCells;

    /// <summary>Compares authored cells with stock definitions to avoid storing unchanged artwork.</summary>
    internal FileSelectCompiledPatch(FileSelectPatch name, FileSelectCompiledPatchCell[] cells)
    {
        this.name = name;
        if (cells.Length != FileSelectFieldTiles.PatchCellCount(name))
        {
            suppliedCells = cells;
            return;
        }
        for (int index = 0; index < cells.Length; index++)
            if (cells[index] != FileSelectFieldTiles.PatchCell(name, index))
            {
                suppliedCells = cells;
                return;
            }
    }

    /// <summary>Cell count from edited art or the stock patch catalog.</summary>
    internal int Count => suppliedCells?.Length ?? FileSelectFieldTiles.PatchCellCount(name);
    /// <summary>Reads an edited cell or falls back to the corresponding stock cell.</summary>
    internal FileSelectCompiledPatchCell Cell(int index) => suppliedCells is null
        ? FileSelectFieldTiles.PatchCell(name, index) : suppliedCells[index];
}
