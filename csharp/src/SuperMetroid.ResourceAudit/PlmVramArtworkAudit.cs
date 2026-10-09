using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// #1162: installed-art ownership for immutable PLM DMA descriptors. This is a
/// finite definition comparison, not an import, frame probe or renderer execution.
/// Reviewed sources bind the ranges to production admission, loading and NMI routing.
/// </summary>
internal static class PlmVramArtworkAudit
{
    /// <summary>Determines whether an exact native PLM transfer is backed by installed artwork.</summary>
    internal static bool OwnsInstalledTransfer(int source, int count) => count > 0 &&
        (EnemyTileSourceDefinitions.All.Any(page => page.SourceAddress == source && page.ByteCount == count) ||
         TorizoInstructionVramArtworkDefinitions.All.ToArray().Any(page =>
             source >= page.SourceAddress && source - page.SourceAddress <= page.ByteCount - count) ||
         CeresEscapeTileArtworkDefinitions.Contains(source, count) ||
         CeresEscapeOverlayTilemapDefinitionsTooling.IsSource(source, count));

    /// <summary>Reports reviewed PLM artwork source contracts that no longer match their source.</summary>
    internal static void VerifySource(string root, PlmProgramAuditReport report)
    {
        // Includes the importer: a range merely named in Core must not count as
        // installed if extraction, manifest admission or PNG loading was removed.
        foreach (ReviewedSource source in PlmVramArtworkSourceContract.Sources)
            if (!ClosedPresentationAudit.MatchesReviewedSource(
                File.ReadAllText(Path.Combine(root, source.Path)), source))
                report.Findings.Add(new("unresolved-artwork-source", source.Path, 0,
                    "PLM DMA provider/import/NMI routing changed; review its coverage before updating the contract."));
    }
}
