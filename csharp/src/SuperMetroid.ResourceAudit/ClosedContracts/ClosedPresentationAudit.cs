using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.ResourceAudit.ClosedContracts.Bosses;
using SuperMetroid.ResourceAudit.ClosedContracts.Enemies;
using SuperMetroid.ResourceAudit.ClosedContracts.Frontend;
using SuperMetroid.ResourceAudit.ClosedContracts.Samus;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.AssetExtraction.World.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Bosses.Artwork;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

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
            string[] missing = [.. contract.Sources.Where(source => !sources.ContainsKey(source.Path)).Select(source => source.Path)];
            bool valid = missing.Length == 0;
            // Why a proof is stale, so its report says what to re-inspect.
            string? staleReason = valid ? null : "its sources are missing: " + string.Join(", ", missing);
            // A new partial implementation could access the reviewed private state.
            // Every source declaration of the provider must remain in the proof.
            if (valid && compilation.GetTypeByMetadataName(contract.Type) is { } provider)
            {
                string[] undeclared = [.. provider.DeclaringSyntaxReferences.Select(declaration => declaration.SyntaxTree.FilePath)
                    .Where(path => !contract.Sources.Any(source => source.Path == path))];
                valid = undeclared.Length == 0;
                if (!valid)
                    staleReason = "the provider is also declared in " + string.Join(", ", undeclared);
            }
            if (valid) valid = InterfacePresentationDomainAudit.HasReviewedImplementations(compilation, contract);
            if (!valid) staleReason ??= "an implementation of its interface is not reviewed";
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
            if (valid && contract.Type == typeof(SuperMetroid.AssetExtraction.Enemies.EnemyTileSourceArtwork).FullName)
            {
                // Additional partial metadata declarations could mutate the private
                // native arrays used to derive installation keys and DMA lengths.
                valid = new[] { typeof(SuperMetroid.AssetExtraction.Enemies.RoomEnemyGraphicsSetDefinitions).FullName,
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
                        typeof(EnemyProjectilePresentationFrameDefinitions).FullName,
                        typeof(EnemyProjectilePresentationFrameSets).FullName }
                        .All(type => compilation.GetTypeByMetadataName((type ?? throw new InvalidOperationException("Verification requires type."))) is { } dependency &&
                            dependency.DeclaringSyntaxReferences.All(declaration => contract.Sources.Any(source =>
                                source.Path == declaration.SyntaxTree.FilePath)));
            }
            if (valid && contract.Methods.Contains("TryGetDisplay", StringComparer.Ordinal) &&
                contract.Type is "SuperMetroid.AssetExtraction.Enemies.EnemySpritemapCatalog" or "SuperMetroid.Core.Enemies.Artwork.EnemyFrameDisplays")
            {
                string[] definitions = contract.Type == typeof(EnemySpritemapCatalog).FullName
                    ? [ContractTypeNames.MetadataName(typeof(EnemySpritemapDefinitions)), ContractTypeNames.MetadataName(typeof(EnemySpritemapDocumentOrder)),
                        ContractTypeNames.MetadataName(typeof(EnemyFrameNameDefinitions))]
                    : [ContractTypeNames.MetadataName(typeof(EnemySpritemapDefinitions)), ContractTypeNames.MetadataName(typeof(EnemySpritemapDocumentOrder)),
                        ContractTypeNames.MetadataName(typeof(EnemyExtendedFrameDefinitions)), ContractTypeNames.MetadataName(typeof(EnemyExtendedFrameSequence)),
                        ContractTypeNames.MetadataName(typeof(BossSpriteFrameDefinitions)), ContractTypeNames.MetadataName(typeof(EnemyExtendedFrameDocumentOrder)),
                        ContractTypeNames.MetadataName(typeof(EnemyFrameNameDefinitions)),
                        ContractTypeNames.MetadataName(typeof(SuperMetroid.Core.Enemies.Drawing.CommonEnemyEmptyFrameDefinitions))];
                valid = definitions.All(type => compilation.GetTypeByMetadataName(type) is { } dependency &&
                    dependency.DeclaringSyntaxReferences.All(declaration => contract.Sources.Any(source =>
                        source.Path == declaration.SyntaxTree.FilePath)));
            }
            if (valid && contract.Type == typeof(InstalledProjectileSprites).FullName)
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "LoadFrames")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IMethodSymbol factory &&
                            factory.ContainingType.ToDisplayString() == typeof(InstalledProjectileSprites).FullName));
            if (valid && contract.Type == typeof(InstalledProjectileFrameBindings).FullName)
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "TimedRecordAddresses")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is IPropertySymbol property &&
                            property.ContainingType.ToDisplayString() == typeof(SuperMetroid.Core.Samus.Projectiles.SamusProjectileFrameDefinitions).FullName));
            if (valid && contract.Type == typeof(SamusArmCannonSourceArtwork).FullName)
                valid = !compilation.SyntaxTrees.Where(tree => !contract.Sources.Any(source => source.Path == tree.FilePath))
                    .Any(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                        .Where(name => name.Identifier.ValueText == "FromPlacement")
                        .Any(name => compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol is
                            IMethodSymbol { Name: "FromPlacement" } factory &&
                                factory.ContainingType.ToDisplayString() == typeof(SamusArmCannonSourceArtwork).FullName));
            // The installed room artwork and a spawned-object sheet accept whatever content they are given: their proofs are
            // the checks of the reviewed loaders that build them (#1536), so a construction anywhere else revokes the proof.
            if (valid && LoaderBuiltProviders.Contains(contract.Type) && ConstructedOutsideSources(compilation, contract) is { } constructor)
            {
                valid = false;
                staleReason = "the provider is constructed outside its reviewed loader, in " + constructor;
            }
            foreach (string method in contract.Methods)
                contracts.Add(new PresentationMethod(contract.Type, method), new ContractValidity(contract, valid, valid ? null : staleReason ?? "a reviewed escape condition no longer holds"));
            if (!valid)
                invalidContracts.Add(contract.Type);
            if (valid)
                foreach (string method in contract.Methods)
                    exports.Add("closed-presentation-contract", contract.Type + "." + method);
        }
    }

    /// <summary>The providers whose complete key domain only their reviewed loader's checks establish (#1536).</summary>
    private static readonly HashSet<string> LoaderBuiltProviders =
    [
        ContractTypeNames.MetadataName(typeof(SuperMetroid.Core.Rooms.Artwork.InstalledRoomArtwork)),
        ContractTypeNames.MetadataName(typeof(SpawnedObjectSheet)),
    ];

    /// <summary>
    /// The source that constructs <paramref name="contract"/>'s provider outside the contract's reviewed sources, or null when
    /// none does. A source that constructs it names it, so only sources whose text names the provider are bound.
    /// </summary>
    private static string? ConstructedOutsideSources(Compilation compilation, ClosedPresentationContract contract)
    {
        string name = contract.Type[(contract.Type.LastIndexOf('.') + 1)..];
        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            if (contract.Sources.Any(source => source.Path == tree.FilePath) || !tree.GetText().ToString().Contains(name, StringComparison.Ordinal))
                continue;
            SemanticModel semantic = compilation.GetSemanticModel(tree);
            BaseObjectCreationExpressionSyntax? creation = tree.GetRoot().DescendantNodes().OfType<BaseObjectCreationExpressionSyntax>()
                .FirstOrDefault(candidate => semantic.GetSymbolInfo(candidate).Symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } constructor &&
                    constructor.ContainingType.ToDisplayString() == contract.Type);
            if (creation is not null)
                return tree.FilePath + ":" + (creation.GetLocation().GetLineSpan().StartLinePosition.Line + 1);
        }
        return null;
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
            Gap($"Reviewed provider proof {contract.Rule} is stale ({reviewed.StaleReason}); re-inspect the loader and selector contract.");
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
        string? invalid = InvalidConstantIndex(operation, provider.Name)
            ?? SpecializedColorDomainAudit.InvalidConstants(operation) ?? PausePresentationDomainAudit.InvalidConstants(operation)
            ?? EnemyDisplayArtworkDomainAudit.InvalidConstants(operation)
            ?? SamusBodyTransferDomainAudit.InvalidConstants(operation)
            ?? ProjectileSpriteDomainAudit.InvalidConstants(operation)
            ?? InterfacePresentationDomainAudit.InvalidConstants(operation);
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
            IParameterSymbol parameter = argument.Parameter ?? throw new InvalidOperationException("Verification requires argument.Parameter.");
            IOperation value = argument.Value is IConversionOperation conversion ? conversion.Operand : argument.Value;
            if (argument.Value.ConstantValue is { HasValue: true, Value: not null } constant)
                yield return new ConstantSelection(parameter.Name, constant.Value, parameter.Type);
            else if (value is IObjectCreationOperation creation)
                foreach (IArgumentOperation component in creation.Arguments)
                    if (component.Value.ConstantValue is { HasValue: true, Value: not null } part)
                    {
                        IParameterSymbol componentParameter = component.Parameter ?? throw new InvalidOperationException(
                            "Verification requires component.Parameter.");
                        yield return new ConstantSelection(parameter.Name + "." + ComponentName(componentParameter.Name), part.Value,
                            componentParameter.Type);
                    }
        }
    }

    /// <summary>
    /// A constructor argument named as the component property it sets: a positional record's parameter already is (X),
    /// and an ordinary constructor's parameter (x) names the same component (#1480).
    /// </summary>
    private static string ComponentName(string parameter) =>
        parameter.Length == 0 ? parameter : char.ToUpperInvariant(parameter[0]) + parameter[1..];

    /// <summary>Rejects literal arguments outside the reviewed finite identities or numeric index ranges.</summary>
    private static string? InvalidConstantIndex(IInvocationOperation operation, string provider)
    {
        foreach ((string parameter, object constantValue, ITypeSymbol type) in ConstantSelections(operation))
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
            if (ConstantDomainNumbers.Of(type, constantValue) is not int number)
                return $"Constant {owner.Parameter}={constantValue} names no {type.Name}.";
            if (identities is not null && !identities.Contains(number))
                return $"Constant {owner.Parameter}={number} is not owned by the reviewed {owner.Type}.{owner.Method} resource domain.";
            if (range is null) continue;
            // White-frame Draygon hurt does not select a health band at all.
            if (owner is ("DraygonColorCatalog", "ApplyHurt", "healthBand") &&
                operation.Arguments.Single(arg => arg.Parameter?.Name == "whiteFrame").Value.ConstantValue
                    is { HasValue: true, Value: true }) continue;
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
