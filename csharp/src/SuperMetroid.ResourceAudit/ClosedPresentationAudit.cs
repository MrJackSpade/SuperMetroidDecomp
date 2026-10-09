using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Accounts for the identified closed-provider boundaries using reviewed loader
/// invariants, not possible gameplay paths. Unknown operations, changed source,
/// invalid constant indices and unresolved string names remain failures.
/// </summary>
internal sealed class ClosedPresentationAudit
{
    private readonly Dictionary<(string Type, string Method), (ClosedPresentationContract Contract, bool Valid)> contracts = [];
    private int references;

    internal ClosedPresentationAudit(Compilation compilation, ResourceIndex exports)
    {
        var sources = compilation.SyntaxTrees.ToDictionary(tree => tree.FilePath, tree => tree.GetText().ToString(),
            StringComparer.Ordinal);
        foreach (ClosedPresentationContract contract in ClosedPresentationContractDefinitions.All)
        {
            bool valid = contract.Sources.All(source => sources.TryGetValue(source.Path, out string? text) &&
                MatchesReviewedSource(text, source));
            // A new partial implementation could access the reviewed private state.
            // Every source declaration of the provider must remain in the proof.
            if (valid && compilation.GetTypeByMetadataName(contract.Type) is { } provider)
                valid = provider.DeclaringSyntaxReferences.All(declaration =>
                    contract.Sources.Any(source => source.Path == declaration.SyntaxTree.FilePath));
            if (valid) valid = InterfacePresentationDomainAudit.HasReviewedImplementations(compilation, contract);
            // This existing definition uses a readonly array reference, not an
            // immutable array. Its reviewed reads are wholly inside these files;
            // a new external use could mutate its keys and must revoke the proof.
            if (valid && contract.Type == typeof(GameplayHudPresentation).FullName)
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "IconNames")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IFieldSymbol field &&
                            field.ContainingType.ToDisplayString() == typeof(GameplayHudDefinitions).FullName));
            // A verification credits instance may have any positive row count.
            // Its use in Core invalidates the production loader's fixed row proof;
            // do not assume retail dimensions merely from the provider's type.
            if (valid && contract.Type == typeof(CreditsPresentation).FullName)
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "FromCompiledRowsForVerification")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IMethodSymbol factory &&
                            factory.ContainingType.ToDisplayString() == typeof(CreditsPresentation).FullName));
            // Partial room geometry is useful in focused fixtures, but a Core
            // caller of that factory invalidates complete production membership.
            if (valid && contract.Type == typeof(SuperMetroid.Core.Rooms.RoomVisualLayoutCatalog).FullName)
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "FromLayoutsForVerification")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IMethodSymbol factory &&
                            factory.ContainingType.ToDisplayString() == typeof(SuperMetroid.Core.Rooms.RoomVisualLayoutCatalog).FullName));
            if (valid && contract.Type == typeof(SuperMetroid.Core.Rooms.XrayOverlayVisualCatalog).FullName)
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "FromOverlaysForVerification")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IMethodSymbol factory &&
                            factory.ContainingType.ToDisplayString() == typeof(SuperMetroid.Core.Rooms.XrayOverlayVisualCatalog).FullName));
            if (valid && contract.Type == typeof(EnemyTileArtworkCatalog).FullName)
            {
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "FromArtworkForVerification")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IMethodSymbol factory &&
                            factory.ContainingType.ToDisplayString() == typeof(EnemyTileArtworkCatalog).FullName));
                // Additional partial metadata declarations could mutate the private
                // native arrays used to derive installation keys and DMA lengths.
                if (valid)
                    valid = new[] { typeof(SuperMetroid.Core.Game.RoomEnemyGraphicsSetDefinitions).FullName!,
                        typeof(SuperMetroid.Core.Game.RoomEnemyDefinitionCatalog).FullName!, typeof(EnemyTileSourceDefinitions).FullName! }
                        .All(type => compilation.GetTypeByMetadataName(type) is { } dependency &&
                            dependency.DeclaringSyntaxReferences.All(declaration => contract.Sources.Any(source =>
                                source.Path == declaration.SyntaxTree.FilePath)));
            }
            if (valid && contract.Type == typeof(EnemyProjectileSpritemapCatalog).FullName)
            {
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "Frames")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IFieldSymbol field &&
                            field.ContainingType.ToDisplayString() == typeof(EnemyProjectileSpritemapDefinitions).FullName));
                if (valid)
                    valid = new[] { typeof(EnemyProjectileSpritemapDefinitions).FullName!,
                        typeof(SuperMetroid.Core.Game.EnemyProjectilePresentationFrameDefinitions).FullName!,
                        typeof(SuperMetroid.Core.Game.EnemyProjectileInstructionMechanicsDefinitions).FullName! }
                        .All(type => compilation.GetTypeByMetadataName(type) is { } dependency &&
                            dependency.DeclaringSyntaxReferences.All(declaration => contract.Sources.Any(source =>
                                source.Path == declaration.SyntaxTree.FilePath)));
            }
            if (valid && contract.Methods.Contains("TryGetDisplay", StringComparer.Ordinal) &&
                contract.Type is "SuperMetroid.Core.Assets.EnemySpritemapCatalog" or "SuperMetroid.Core.Assets.EnemyExtendedFrameCatalog")
            {
                string[] definitions = contract.Type == typeof(EnemySpritemapCatalog).FullName
                    ? [typeof(EnemySpritemapDefinitions).FullName!]
                    : [typeof(EnemyExtendedFrameDefinitions).FullName!, typeof(EnemyExtendedFrameSequence).FullName!,
                        typeof(BossOamFrameDefinitions).FullName!, typeof(PirateArtworkNameDefinitions).FullName!,
                        typeof(SuperMetroid.Core.Game.CommonEnemyEmptyExtendedFrameDefinitions).FullName!];
                valid = definitions.All(type => compilation.GetTypeByMetadataName(type) is { } dependency &&
                    dependency.DeclaringSyntaxReferences.All(declaration => contract.Sources.Any(source =>
                        source.Path == declaration.SyntaxTree.FilePath)));
            }
            if (valid && contract.Type == typeof(EscapeTimerPresentation).FullName)
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "AnchorNames")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IFieldSymbol field &&
                            field.ContainingType.ToDisplayString() == typeof(EscapeTimerPresentationDefinitions).FullName));
            if (valid && contract.Type == typeof(ProjectileSpriteCatalog).FullName)
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "LoadFrames")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IMethodSymbol factory &&
                            factory.ContainingType.ToDisplayString() == typeof(ProjectileSpriteCatalog).FullName));
            if (valid && contract.Type == typeof(ProjectileFrameBindingCatalog).FullName)
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "TimedRecordPointers")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IPropertySymbol property &&
                            property.ContainingType.ToDisplayString() == typeof(SuperMetroid.Core.Game.SamusProjectileRadiusDefinitions).FullName));
            if (valid && contract.Type == typeof(SamusArmCannonArtworkCatalog).FullName)
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText is "FromPlacement" or "TileSourcePointers")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is
                            IMethodSymbol { Name: "FromPlacement" } factory &&
                                factory.ContainingType.ToDisplayString() == typeof(SamusArmCannonArtworkCatalog).FullName ||
                            compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is
                            IFieldSymbol { Name: "TileSourcePointers" } field &&
                                field.ContainingType.ToDisplayString() == typeof(SamusArmCannonArtworkFormat).FullName));
            foreach (string method in contract.Methods)
                contracts.Add((contract.Type, method), (contract, valid));
            if (valid)
                foreach (string method in contract.Methods)
                    exports.Add("closed-presentation-contract", contract.Type + "." + method);
        }
        NamedPresentationAudit.Install(exports);
    }

    internal static bool MatchesReviewedSource(string text, ReviewedSource source) =>
        SourceFingerprint.Of(text) == source.Fingerprint;

    internal bool TryInspect(InvocationExpressionSyntax call, SemanticModel semantic, IMethodSymbol method,
        ResourceIndex exports, AuditReport report, string location, string arguments)
    {
        if (!contracts.TryGetValue((method.ContainingType.ToDisplayString(), method.Name), out var reviewed)) return false;
        ClosedPresentationContract contract = reviewed.Contract;
        string owner = method.ContainingType.Name + "." + method.Name;
        void Gap(string reason)
        {
            report.Gap(method.ContainingType.Name, owner, location, reason);
            report.Consumers.Add(new(owner, location, "unresolved"));
        }
        if (!reviewed.Valid)
        {
            Gap($"Reviewed provider proof {contract.Rule} is stale or its source is unavailable; re-inspect the loader and selector contract.");
            return true;
        }
        if (semantic.GetOperation(call) is not IInvocationOperation operation)
        {
            Gap("Closed-provider operation could not be bound.");
            return true;
        }
        string? invalid = InvalidConstantIndex(operation) ?? PlmVisualDomainAudit.InvalidConstants(operation)
            ?? SpecializedColorDomainAudit.InvalidConstants(operation) ?? PausePresentationDomainAudit.InvalidConstants(operation)
            ?? EnemyArtworkDomainAudit.InvalidConstants(operation)
            ?? EnemyDisplayArtworkDomainAudit.InvalidConstants(operation)
            ?? SamusBodyTransferDomainAudit.InvalidConstants(operation)
            ?? InterfacePresentationDomainAudit.InvalidConstants(operation)
            ?? ClosedTransferDomainAudit.InvalidConstants(operation);
        if (invalid is not null)
        {
            Gap(invalid);
            return true;
        }
        NamedSelectionResult? named = NamedPresentationAudit.Inspect(operation, semantic, owner, location, exports, report);
        if (named == NamedSelectionResult.Unresolved)
        {
            Gap("Named selection has no compiler-resolved finite constant set; complete array coverage does not prove arbitrary strings.");
            return true;
        }
        if (named == NamedSelectionResult.Missing)
        {
            report.Consumers.Add(new(owner, location, "missing"));
            return true;
        }
        report.Require("closed-presentation-contract", owner, contract.Type + "." + method.Name, location, exports);
        references++;
        report.Consumers.Add(new(owner, location, "closed-provider"));
        report.Classifications.Add(new(owner, location));
        return true;
    }

    private static string? InvalidConstantIndex(IInvocationOperation operation)
    {
        foreach (IArgumentOperation argument in operation.Arguments)
        {
            var owner = (Type: operation.TargetMethod.ContainingType.Name,
                Method: operation.TargetMethod.Name, Parameter: argument.Parameter!.Name);
            if (argument.Value.ConstantValue is not { HasValue: true, Value: not null } value) continue;
            int[]? identities = ClosedPresentationIdentityDefinitions.Get(owner.Type, owner.Method, owner.Parameter);
            ProviderIndexDomain? range = ClosedPresentationIndexDefinitions.Get(owner.Type, owner.Method, owner.Parameter);
            if (identities is null && range is null) continue;
            int number = Convert.ToInt32(value.Value);
            if (identities is not null && !identities.Contains(number))
                return $"Constant {owner.Parameter}={number} is not owned by the reviewed {owner.Type}.{owner.Method} resource domain.";
            if (range is null) continue;
            // White-frame Draygon hurt does not select a health band at all.
            if (owner is ("DraygonColorCatalog", "ApplyHurt", "healthTableByteIndex") &&
                operation.Arguments.Single(arg => arg.Parameter?.Name == "whiteFrame").Value.ConstantValue
                    is { HasValue: true, Value: true }) continue;
            if (owner is ("SporeSpawnColorCatalog", "ResolveDeath", "frame") &&
                operation.Arguments.Single(arg => arg.Parameter?.Name == "layer").Value.ConstantValue
                    is { HasValue: true, Value: not null } layer &&
                (SuperMetroid.Core.Game.SporeSpawnDeathPaletteLayer)Convert.ToInt32(layer.Value) is
                    SuperMetroid.Core.Game.SporeSpawnDeathPaletteLayer.Level or
                    SuperMetroid.Core.Game.SporeSpawnDeathPaletteLayer.Background)
                range = new(0, SuperMetroid.Core.Game.SporeSpawnColorRomData.DeathSceneFrameCount);
            if (!range.Value.Contains(number))
                return $"Constant {owner.Parameter}={number} is outside the reviewed valid domain {range.Value.Description}.";
        }
        return null;
    }

    internal void Complete(ResourceIndex exports, AuditReport report) => report.Coverage.Add(new(
        "closed-presentation-contract", references, exports.Count("closed-presentation-contract")));
}
