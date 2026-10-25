using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.ResourceAudit.ClosedContracts.Bosses;
using SuperMetroid.ResourceAudit.ClosedContracts.Enemies;
using SuperMetroid.ResourceAudit.ClosedContracts.Frontend;
using SuperMetroid.ResourceAudit.ClosedContracts.Samus;
using SuperMetroid.ResourceAudit.PlmPrograms;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.Core.World.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>
/// Accounts for the identified closed-provider boundaries using reviewed loader
/// invariants, not possible gameplay paths. Unknown operations, changed source,
/// invalid constant indices and unresolved string names remain failures.
/// </summary>
internal sealed class ClosedPresentationAudit
{
    /// <summary>Reviewed provider methods and whether their source and loader invariants still hold.</summary>
    private readonly Dictionary<PresentationMethod, ContractValidity> contracts = [];
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
            // Partial fixture factories for credits, room layouts, X-ray overlays and enemy tile artwork live in
            // test-support, which Core and Tooling cannot reference, so no production caller can reach them.
            if (valid && contract.Type == typeof(EnemyTileArtworkCatalog).FullName)
            {
                // Additional partial metadata declarations could mutate the private
                // native arrays used to derive installation keys and DMA lengths.
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
                        .Where(name => name.Identifier.ValueText == nameof(EnemyProjectileSpritemapDefinitions.FrameNames))
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IFieldSymbol field &&
                            field.ContainingType.ToDisplayString() == typeof(EnemyProjectileSpritemapDefinitions).FullName));
                if (valid)
                    valid = new[] { typeof(EnemyProjectileSpritemapDefinitions).FullName,
                        typeof(SuperMetroid.Core.Enemies.Projectiles.EnemyProjectilePresentationFrameDefinitions).FullName,
                        typeof(SuperMetroid.Core.Enemies.Projectiles.EnemyProjectilePresentationFrameSets).FullName }
                        .All(type => compilation.GetTypeByMetadataName((type ?? throw new InvalidOperationException("Verification requires type."))) is { } dependency &&
                            dependency.DeclaringSyntaxReferences.All(declaration => contract.Sources.Any(source =>
                                source.Path == declaration.SyntaxTree.FilePath)));
            }
            if (valid && contract.Methods.Contains("TryGetDisplay", StringComparer.Ordinal) &&
                contract.Type is "SuperMetroid.Core.Enemies.Artwork.EnemySpritemapCatalog" or "SuperMetroid.Core.Enemies.Artwork.EnemyExtendedFrameCatalog")
            {
                string[] definitions = contract.Type == typeof(EnemySpritemapCatalog).FullName
                    ? [ContractTypeNames.MetadataName(typeof(EnemySpritemapDefinitions)), ContractTypeNames.MetadataName(typeof(EnemySpritemapCompositions))]
                    : [ContractTypeNames.MetadataName(typeof(EnemyExtendedFrameDefinitions)), ContractTypeNames.MetadataName(typeof(EnemyExtendedFrameSequence)),
                        ContractTypeNames.MetadataName(typeof(BossSpriteFrameDefinitions)), ContractTypeNames.MetadataName(typeof(EnemyExtendedFrameCompositions)),
                        ContractTypeNames.MetadataName(typeof(SuperMetroid.Core.Enemies.Drawing.CommonEnemyEmptyExtendedFrameDefinitions))];
                valid = definitions.All(type => compilation.GetTypeByMetadataName(type) is { } dependency &&
                    dependency.DeclaringSyntaxReferences.All(declaration => contract.Sources.Any(source =>
                        source.Path == declaration.SyntaxTree.FilePath)));
            }
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
                        .Where(name => name.Identifier.ValueText == "FromPlacement")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is
                            IMethodSymbol { Name: "FromPlacement" } factory &&
                                factory.ContainingType.ToDisplayString() == typeof(SamusArmCannonArtworkCatalog).FullName));
            foreach (string method in contract.Methods)
                contracts.Add(new PresentationMethod(contract.Type, method), new ContractValidity(contract, valid));
            if (!valid)
                invalidContracts.Add(contract.Type);
            if (valid)
                foreach (string method in contract.Methods)
                    exports.Add("closed-presentation-contract", contract.Type + "." + method);
        }
    }

    /// <summary>Classifies a provider call only when its reviewed proof and index domain remain valid.</summary>
    internal bool TryInspect(InvocationExpressionSyntax call, SemanticModel semantic, IMethodSymbol method,
        ResourceIndex exports, AuditReport report, string location)
    {
        // A call through a reviewed narrow interface (for example IGameplayPresentationContent) is classified by
        // that receiver's contract, even when the invoked member is inherited from a broader base interface.
        INamedTypeSymbol provider = Receiver(call, semantic) is { } receiver &&
            contracts.ContainsKey(new PresentationMethod(receiver.ToDisplayString(), method.Name)) ? receiver : method.ContainingType;
        if (!contracts.TryGetValue(new PresentationMethod(provider.ToDisplayString(), method.Name), out var reviewed)) return false;
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
        if (SuperMetroid.ResourceAudit.Resources.ConsumerAudit.UnboundArguments(call, semantic) is { } unbound)
        {
            Gap(unbound);
            return true;
        }
        string? invalid = InvalidConstantIndex(operation, provider.Name) ?? PlmVisualDomainAudit.InvalidConstants(operation)
            ?? SpecializedColorDomainAudit.InvalidConstants(operation) ?? PausePresentationDomainAudit.InvalidConstants(operation)
            ?? EnemyDisplayArtworkDomainAudit.InvalidConstants(operation)
            ?? SamusBodyTransferDomainAudit.InvalidConstants(operation)
            ?? ProjectileSpriteDomainAudit.InvalidConstants(operation)
            ?? InterfacePresentationDomainAudit.InvalidConstants(operation)
            ?? ClosedTransferDomainAudit.InvalidConstants(operation);
        if (invalid is not null)
        {
            Gap(invalid);
            return true;
        }
        report.Require("closed-presentation-contract", owner, contract.Type + "." + method.Name, location, exports);
        references++;
        report.Consumers.Add(new(owner, location, "closed-provider"));
        report.Classifications.Add(new(owner, location));
        return true;
    }

    /// <summary>
    /// Literal selections in an invocation: each constant argument under its parameter name, and each constant
    /// constructor argument of a constructed value (a coordinate) as <c>parameter.Component</c>.
    /// </summary>
    private static IEnumerable<ConstantSelection> ConstantSelections(IInvocationOperation operation)
    {
        foreach (IArgumentOperation argument in operation.Arguments)
        {
            string parameter = (argument.Parameter ?? throw new InvalidOperationException("Verification requires argument.Parameter.")).Name;
            IOperation value = argument.Value is IConversionOperation conversion ? conversion.Operand : argument.Value;
            if (argument.Value.ConstantValue is { HasValue: true, Value: not null } constant)
                yield return new ConstantSelection(parameter, constant.Value);
            else if (value is IObjectCreationOperation creation)
                foreach (IArgumentOperation component in creation.Arguments)
                    if (component.Value.ConstantValue is { HasValue: true, Value: not null } part)
                        yield return new ConstantSelection(parameter + "." + (component.Parameter ?? throw new InvalidOperationException(
                            "Verification requires component.Parameter.")).Name, part.Value);
        }
    }

    /// <summary>Rejects literal arguments outside the reviewed finite identities or numeric index ranges.</summary>
    private static string? InvalidConstantIndex(IInvocationOperation operation, string provider)
    {
        foreach ((string parameter, object constantValue) in ConstantSelections(operation))
        {
            var owner = new PresentationParameter(Type:provider, Method:operation.TargetMethod.Name, Parameter:parameter);
            if (constantValue is string name &&
                ClosedPresentationIdentityDefinitions.GetNames(owner.Type, owner.Method, owner.Parameter) is { } names)
            {
                if (!names.Contains(name, StringComparer.Ordinal))
                    return $"Constant {owner.Parameter}=\"{name}\" is not owned by the reviewed {owner.Type}.{owner.Method} resource domain.";
                continue;
            }
            int[]? identities = ClosedPresentationIdentityDefinitions.Get(owner.Type, owner.Method, owner.Parameter);
            ProviderIndexDomain? range = ClosedPresentationIndexDefinitions.Get(owner.Type, owner.Method, owner.Parameter);
            if (identities is null && range is null) continue;
            int number = Convert.ToInt32(constantValue);
            if (identities is not null && !identities.Contains(number))
                return $"Constant {owner.Parameter}={number} is not owned by the reviewed {owner.Type}.{owner.Method} resource domain.";
            if (range is null) continue;
            // White-frame Draygon hurt does not select a health band at all.
            if (owner is ("DraygonColorCatalog", "ApplyHurt", "healthBand") &&
                operation.Arguments.Single(arg => arg.Parameter?.Name == "whiteFrame").Value.ConstantValue
                    is { HasValue: true, Value: true }) continue;
            if (owner is ("SporeSpawnColorCatalog", "ResolveDeath", "frame") &&
                operation.Arguments.Single(arg => arg.Parameter?.Name == "layer").Value.ConstantValue
                    is { HasValue: true, Value: not null } layer &&
                (SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnDeathPaletteLayer)Convert.ToInt32(layer.Value) is
                    SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnDeathPaletteLayer.Level or
                    SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnDeathPaletteLayer.Background)
                range = new(0, SuperMetroid.Core.Bosses.SporeSpawn.SporeSpawnColorDefinitions.DeathSceneFrameCount);
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
