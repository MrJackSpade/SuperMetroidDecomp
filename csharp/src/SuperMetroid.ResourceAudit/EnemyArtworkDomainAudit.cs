using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Correlates known enemy DMA sources/definition IDs with their required native lengths.</summary>
internal static class EnemyArtworkDomainAudit
{
    internal static string? InvalidConstants(IInvocationOperation operation)
    {
        if (operation.TargetMethod.ContainingType.ToDisplayString() != typeof(EnemyTileArtworkCatalog).FullName) return null;
        int? Constant(string name) => operation.Arguments.Single(argument => argument.Parameter?.Name == name)
            .Value.ConstantValue is { HasValue: true, Value: not null } value ? Convert.ToInt32(value.Value) : null;
        if (operation.TargetMethod.Name == "LoadTo")
        {
            if (Constant("definitionPointer") is not int pointer || Constant("byteCount") is not int count) return null;
            EnemyTileSourceDefinition? owned = EnemyTileSourceDefinitions.All.Where(definition => definition.DefinitionPointer == pointer)
                .Select(definition => (EnemyTileSourceDefinition?)definition).SingleOrDefault();
            return owned is { } definition && count != definition.ByteCount
                ? $"Enemy ${pointer:X4} requires {definition.ByteCount} tile bytes, not {count}." : null;
        }
        if (operation.TargetMethod.Name != "TryResolve" || Constant("sourceAddress") is not int source ||
            Constant("byteCount") is not int byteCount) return null;
        bool ordinary = EnemyTileSourceDefinitions.All.Any(definition => definition.SourceAddress == source);
        bool ceres = CeresEscapeTileArtworkDefinitions.All.ToArray().Any(page =>
            source >= page.SourceAddress && source < page.SourceAddress + page.ByteCount);
        bool overlay = CeresEscapeOverlayTilemapDefinitions.All.ToArray().Any(page => page.SourceAddress == source);
        bool torizo = TorizoInstructionVramArtworkDefinitions.All.ToArray().Any(page =>
            source >= page.SourceAddress && source < page.SourceAddress + page.ByteCount);
        if (!ordinary && !ceres && !overlay && !torizo) return null; // Valid unowned-source false query.
        bool complete = EnemyTileSourceDefinitions.All.Any(definition =>
            definition.SourceAddress == source && definition.ByteCount == byteCount) ||
            CeresEscapeTileArtworkDefinitions.Contains(source, byteCount) ||
            CeresEscapeOverlayTilemapDefinitionsTooling.IsSource(source, byteCount) ||
            TorizoInstructionVramArtworkDefinitions.All.ToArray().Any(page => byteCount > 0 &&
                source >= page.SourceAddress && source - page.SourceAddress <= page.ByteCount - byteCount);
        return complete ? null : "Owned enemy DMA source/length is not a complete ordinary sheet, bounded Ceres/Torizo tile slice, or exact overlay page.";
    }
}
