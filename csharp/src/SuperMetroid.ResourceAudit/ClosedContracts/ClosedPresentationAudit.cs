using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Areas.Escape.Artwork;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.ResourceAudit.ClosedContracts.Bosses;
using SuperMetroid.ResourceAudit.ClosedContracts.Enemies;
using SuperMetroid.ResourceAudit.ClosedContracts.Frontend;
using SuperMetroid.ResourceAudit.ClosedContracts.Samus;
using SuperMetroid.ResourceAudit.PlmPrograms;
using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>
/// Accounts for the identified closed-provider boundaries using reviewed loader
/// invariants, not possible gameplay paths. Unknown operations, changed source,
/// invalid constant indices and unresolved string names remain failures.
/// </summary>
internal sealed class ClosedPresentationAudit
{
    /// <summary>Reviewed provider methods and whether their source and loader invariants still hold.</summary>
    private readonly Dictionary<(string Type, string Method), (ClosedPresentationContract Contract, bool Valid)> contracts = [];
    /// <summary>Consumer calls classified against a valid closed presentation contract.</summary>
    private int references;
    /// <summary>Contracts whose reviewed proof no longer holds, named in failure diagnostics.</summary>
    private readonly List<string> invalidContracts = [];

    /// <summary>The provider types whose closed contract is currently revoked.</summary>
    internal IReadOnlyList<string> InvalidContracts => invalidContracts;

    /// <summary>Revalidates each provider's source closure and known factory/array escape conditions before registering exports.</summary>
    internal ClosedPresentationAudit(Compilation compilation, ResourceIndex exports)
    {
        var sources = compilation.SyntaxTrees.ToDictionary(tree => tree.FilePath, tree => tree.GetText().ToString(),
            StringComparer.Ordinal);
        foreach (ClosedPresentationContract contract in ClosedPresentationContractDefinitions.All)
        {
            bool valid = contract.Sources.All(source => sources.ContainsKey(source.Path));
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
            if (valid && contract.Type == typeof(SuperMetroid.Core.Rooms.Xray.XrayOverlayVisualCatalog).FullName)
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "FromOverlaysForVerification")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IMethodSymbol factory &&
                            factory.ContainingType.ToDisplayString() == typeof(SuperMetroid.Core.Rooms.Xray.XrayOverlayVisualCatalog).FullName));
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
                    valid = new[] { typeof(SuperMetroid.Core.Enemies.RoomEnemyGraphicsSetDefinitions).FullName,
                        typeof(SuperMetroid.Core.Enemies.RoomEnemyDefinitionCatalog).FullName, typeof(EnemyTileSourceDefinitions).FullName }
                        .All(type => compilation.GetTypeByMetadataName((type ?? throw new InvalidOperationException("Verification requires type."))) is { } dependency &&
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
                    valid = new[] { typeof(EnemyProjectileSpritemapDefinitions).FullName,
                        typeof(SuperMetroid.Core.Enemies.Projectiles.EnemyProjectilePresentationFrameDefinitions).FullName,
                        typeof(SuperMetroid.Core.Enemies.Projectiles.EnemyProjectileFirstProgramFrameManifest).FullName }
                        .All(type => compilation.GetTypeByMetadataName((type ?? throw new InvalidOperationException("Verification requires type."))) is { } dependency &&
                            dependency.DeclaringSyntaxReferences.All(declaration => contract.Sources.Any(source =>
                                source.Path == declaration.SyntaxTree.FilePath)));
            }
            if (valid && contract.Methods.Contains("TryGetDisplay", StringComparer.Ordinal) &&
                contract.Type is "SuperMetroid.Core.Enemies.Artwork.EnemySpritemapCatalog" or "SuperMetroid.Core.Enemies.Artwork.EnemyExtendedFrameCatalog")
            {
                string[] definitions = contract.Type == typeof(EnemySpritemapCatalog).FullName
                    ? [(typeof(EnemySpritemapDefinitions).FullName ?? throw new InvalidOperationException("Verification requires typeof(EnemySpritemapDefinitions).FullName."))]
                    : [(typeof(EnemyExtendedFrameDefinitions).FullName ?? throw new InvalidOperationException("Verification requires typeof(EnemyExtendedFrameDefinitions).FullName.")), (typeof(EnemyExtendedFrameSequence).FullName ?? throw new InvalidOperationException("Verification requires typeof(EnemyExtendedFrameSequence).FullName.")),
                        (typeof(BossOamFrameDefinitions).FullName ?? throw new InvalidOperationException("Verification requires typeof(BossOamFrameDefinitions).FullName.")), (typeof(PirateArtworkNameDefinitions).FullName ?? throw new InvalidOperationException("Verification requires typeof(PirateArtworkNameDefinitions).FullName.")),
                        (typeof(SuperMetroid.Core.Enemies.Drawing.CommonEnemyEmptyExtendedFrameDefinitions).FullName ?? throw new InvalidOperationException("Verification requires typeof(SuperMetroid.Core.Enemies.Drawing.CommonEnemyEmptyExtendedFrameDefinitions).FullName."))];
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
                        .Where(name => name.Identifier.ValueText == "TimedRecordAddresses")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IPropertySymbol property &&
                            property.ContainingType.ToDisplayString() == typeof(SuperMetroid.Core.Samus.Projectiles.SamusProjectileRecordDefinitions).FullName));
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
            if (!valid)
                invalidContracts.Add(contract.Type);
            if (valid)
                foreach (string method in contract.Methods)
                    exports.Add("closed-presentation-contract", contract.Type + "." + method);
        }
        NamedPresentationAudit.Install(exports);
    }

    /// <summary>Classifies a provider call only when its reviewed proof, index domain, and named selection remain valid.</summary>
    internal bool TryInspect(InvocationExpressionSyntax call, SemanticModel semantic, IMethodSymbol method,
        ResourceIndex exports, AuditReport report, string location)
    {
        // A call through a reviewed narrow interface (for example IGameplayPresentationContent) is classified by
        // that receiver's contract, even when the invoked member is inherited from a broader base interface.
        INamedTypeSymbol provider = Receiver(call, semantic) is { } receiver &&
            contracts.ContainsKey((receiver.ToDisplayString(), method.Name)) ? receiver : method.ContainingType;
        if (!contracts.TryGetValue((provider.ToDisplayString(), method.Name), out var reviewed)) return false;
        ClosedPresentationContract contract = reviewed.Contract;
        string owner = provider.Name + "." + method.Name;
        // Record why a recognized provider call cannot be classified under its closed contract.
        void Gap(string reason)
        {
            report.Gap(provider.Name, owner, location, reason);
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
        string? invalid = InvalidConstantIndex(operation, provider.Name) ?? PlmVisualDomainAudit.InvalidConstants(operation)
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

    /// <summary>Rejects literal arguments outside the reviewed finite identities or numeric index ranges.</summary>
    private static string? InvalidConstantIndex(IInvocationOperation operation, string provider)
    {
        foreach (IArgumentOperation argument in operation.Arguments)
        {
            var owner = (Type: provider,
                Method: operation.TargetMethod.Name, Parameter: (argument.Parameter ?? throw new InvalidOperationException("Verification requires argument.Parameter.")).Name);
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
                (SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnDeathPaletteLayer)Convert.ToInt32(layer.Value) is
                    SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnDeathPaletteLayer.Level or
                    SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnDeathPaletteLayer.Background)
                range = new(0, SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnColorRomData.DeathSceneFrameCount);
            if (!range.Value.Contains(number))
                return $"Constant {owner.Parameter}={number} is outside the reviewed valid domain {range.Value.Description}.";
        }
        return null;
    }

    /// <summary>The static type of an invoked member's receiver, including a null-conditional receiver.</summary>
    private static INamedTypeSymbol? Receiver(InvocationExpressionSyntax call, SemanticModel semantic)
    {
        ExpressionSyntax? receiver = call.Expression switch
        {
            MemberAccessExpressionSyntax access => access.Expression,
            MemberBindingExpressionSyntax => call.FirstAncestorOrSelf<ConditionalAccessExpressionSyntax>()?.Expression,
            _ => null,
        };
        return receiver is null ? null : semantic.GetTypeInfo(receiver).Type as INamedTypeSymbol;
    }

    /// <summary>Publishes the number of classified consumer calls against registered closed-provider exports.</summary>
    internal void Complete(ResourceIndex exports, AuditReport report) => report.Coverage.Add(new(
        "closed-presentation-contract", references, exports.Count("closed-presentation-contract")));
}
