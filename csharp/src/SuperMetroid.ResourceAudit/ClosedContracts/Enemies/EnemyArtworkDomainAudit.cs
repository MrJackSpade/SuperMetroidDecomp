using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Areas.Ceres.Artwork;
using SuperMetroid.Core.Bosses.Torizo.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Enemies;

/// <summary>Correlates known enemy DMA sources/definition IDs with their required native lengths.</summary>
internal static class EnemyArtworkDomainAudit
{
    /// <summary>
    /// Diagnoses a known enemy DMA source paired with the wrong byte count. Unowned sources
    /// remain valid false queries; owned ordinary sheets, Ceres/Torizo slices, and overlay
    /// pages must match their separately reviewed length contracts.
    /// </summary>
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.ContainingType.ToDisplayString() != typeof(EnemyTileArtworkCatalog).FullName) return null;
        int? Constant(string name) => operation.Arguments.Single(argument => argument.Parameter?.Name == name)
            .Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;
        if (operation.TargetMethod.Name != "TryResolve" || Constant("sourceAddress") is not int source ||
            Constant("byteCount") is not int byteCount) return null;
        bool ordinary = EnemyTileSourceDefinitions.All.Any(definition => definition.SourceAddress == source);
        bool ceres = CeresEscapeTileArtworkDefinitions.All.Any(page =>
            source >= page.SourceAddress && source < page.SourceAddress + page.ByteCount);
        bool overlay = CeresEscapeOverlayTilemapDefinitions.All.Any(page => page.SourceAddress == source);
        bool torizo = TorizoInstructionVramArtworkDefinitions.All.Any(page =>
            source >= page.SourceAddress && source < page.SourceAddress + page.ByteCount);
        if (!ordinary && !ceres && !overlay && !torizo) return null; // Valid unowned-source false query.
        bool complete = EnemyTileSourceDefinitions.All.Any(definition =>
            definition.SourceAddress == source && definition.ByteCount == byteCount) ||
            CeresEscapeTileArtworkDefinitionsTooling.Contains(source, byteCount) ||
            CeresEscapeOverlayTilemapDefinitionsTooling.IsSource(source, byteCount) ||
            TorizoInstructionVramArtworkDefinitions.All.Any(page => byteCount > 0 &&
                source >= page.SourceAddress && source - page.SourceAddress <= page.ByteCount - byteCount);
        return complete ? null : "Owned enemy DMA source/length is not a complete ordinary sheet, bounded Ceres/Torizo tile slice, or exact overlay page.";
    }
}
