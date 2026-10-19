using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.ResourceAudit.PlmPrograms;

/// <summary>
/// #1162: installed-art ownership for immutable PLM DMA descriptors. This is a
/// finite definition comparison, not an import, frame probe or renderer execution.
/// Reviewed sources bind the ranges to production admission, loading and NMI routing.
/// </summary>
internal static class PlmVramArtworkAudit
{
    /// <summary>Recognizes a positive PLM DMA span covered by an installed
    /// enemy page, Torizo fragment, Ceres escape tile, or overlay resource.</summary>
    internal static bool OwnsInstalledTransfer(int source, int count) => count > 0 &&
        (EnemyTileSourceDefinitions.All.Any(page => page.SourceAddress == source && page.ByteCount == count) ||
         TorizoInstructionVramArtworkDefinitions.All.Any(page =>
             source >= page.SourceAddress && source - page.SourceAddress <= page.ByteCount - count) ||
         CeresEscapeTileArtworkDefinitions.Contains(source, count) ||
         CeresEscapeOverlayTilemapDefinitionsTooling.IsSource(source, count));

}
