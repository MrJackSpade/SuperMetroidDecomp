
namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>One slot-field patch's installed cells.</summary>
internal sealed class FileSelectCompiledPatch
{
    /// <summary>The installed cells in placement order.</summary>
    private readonly FileSelectCompiledPatchCell[] cells;

    /// <summary>Retains the patch's installed cells.</summary>
    internal FileSelectCompiledPatch(FileSelectCompiledPatchCell[] cells) => this.cells = cells;

    /// <summary>The number of installed cells.</summary>
    internal int Count => cells.Length;
    /// <summary>The installed cell at <paramref name="index"/>.</summary>
    internal FileSelectCompiledPatchCell Cell(int index) => cells[index];
}
