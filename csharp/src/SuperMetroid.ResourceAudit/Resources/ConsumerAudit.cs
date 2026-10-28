using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ResourceAudit.ClosedContracts;
using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.Resources;

/// <summary>
/// Semantic source inventory of installed-resource lookups. A dynamic argument or
/// unregistered resource domain is an explicit coverage gap, never a clean result.
/// This is not a points-to proof of arbitrary gameplay state.
/// </summary>
internal static class ConsumerAudit
{
    /// <summary>Builds a semantic Core/tooling inventory and records resolvable consumers and explicit coverage gaps.</summary>
    public static void Run(string root, ResourceIndex exports, AuditReport report)
    {
        CSharpCompilation compilation = CreateCompilation(root);
        var operands = new ProgramOperandAudit(exports, report);
        var closedProviders = new ClosedPresentationAudit(compilation, exports);
        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            // Catalog implementation is the provider, not a gameplay consumer: the shared Assets module and each
            // domain's Artwork module (#626).
            if (IsArtworkLayer(tree.FilePath) || tree.FilePath.StartsWith('<')) continue;
            SemanticModel semantic = compilation.GetSemanticModel(tree);
            foreach (ClassDeclarationSyntax declaration in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                operands.Inspect(declaration, semantic);
            // Development tooling never runs in the game, so its calls are not resource consumers.
            if (IsTooling(tree)) continue;
            foreach (InvocationExpressionSyntax call in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
                Inspect(call, semantic, exports, report, closedProviders);
        }
        operands.Complete();
        closedProviders.Complete(exports, report);
    }

    /// <summary>Whether a compiled source belongs to the development tool library rather than shipped Core.</summary>
    internal static bool IsTooling(SyntaxTree tree) =>
        tree.FilePath.StartsWith("csharp/src/SuperMetroid.Tooling/", StringComparison.Ordinal);

    /// <summary>A Core program owner's runtime type, or the development type it moved to whole.</summary>
    internal static Type? ProgramOwnerType(string fullName) =>
        typeof(SuperMetroid.Core.World.RoomEnemySystem).Assembly.GetType(fullName) ?? typeof(ToolingTypes).Assembly.GetType(fullName);

    /// <summary>Parses Core and its tooling mirror with SDK implicit usings for semantic source inspection.</summary>
    internal static CSharpCompilation CreateCompilation(string root)
    {
        string directory = Path.Combine(root, "csharp/src/SuperMetroid.Core");
        if (!Directory.Exists(directory)) throw new DirectoryNotFoundException(directory);
        // Catalog members only tools read live in the development library's Core mirror; they
        // belong to the same owners, so ownership guards and program discovery see them too.
        string tooling = Path.Combine(root, "csharp/src/SuperMetroid.Tooling");
        if (!Directory.Exists(tooling)) throw new DirectoryNotFoundException(tooling);
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var trees = Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(tooling, "*.cs", SearchOption.AllDirectories))
            .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar) &&
                           !path.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), options,
                Path.GetRelativePath(root, path).Replace('\\', '/'))).ToList();
        // SDK implicit usings are build inputs, not handwritten Core source.
        trees.Add(CSharpSyntaxTree.ParseText(
            "global using System; global using System.Collections.Generic; global using System.IO; " +
            "global using System.Linq; global using System.Net.Http; global using System.Threading; " +
            "global using System.Threading.Tasks;", options, "<sdk-global-usings>"));
        string[] platforms = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")
            ?? throw new InvalidOperationException("The SDK platform reference inventory is unavailable."))
            .Split(Path.PathSeparator);
        CSharpCompilation compilation = CSharpCompilation.Create("SuperMetroid.Core.ResourceSourceAudit", trees,
            platforms.Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        return compilation;
    }

    /// <summary>Classifies an instance resource lookup by semantic owner and constant source identity.</summary>
    internal static void Inspect(InvocationExpressionSyntax call, SemanticModel semantic,
        ResourceIndex exports, AuditReport report, ClosedPresentationAudit? closedProviders = null)
    {
        IMethodSymbol? method = semantic.GetSymbolInfo(call).Symbol as IMethodSymbol;
        string source = call.SyntaxTree.FilePath + ":" + (call.GetLocation().GetLineSpan().StartLinePosition.Line + 1);
        if (method is null)
        {
            // Do not hide lookups whose receiver binds but overload resolution
            // fails. Syntax alone cannot certify which catalog owns the operation.
            if (call.Expression is MemberAccessExpressionSyntax access &&
                semantic.GetTypeInfo(access.Expression).Type is { } receiver && IsResourceType(receiver))
                report.Gap(ResourceDomains.Consumer, receiver.ToDisplayString(), source,
                    "Resource lookup could not be bound semantically: " + call.Expression + " (" + string.Join(" | ",
                        semantic.GetDiagnostics(call.Span).Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                            .Select(diagnostic => diagnostic.GetMessage())) + ")");
            return;
        }
        // Draw(), Segment(), Fragment(), LoadTo() and Color() also consume resources.
        // Core also consumes internal draw contracts. Accessibility is not a
        // coverage boundary; inventory instance operations, not just public Get().
        // A static operation is a lookup when a reviewed contract names it: a draw from a pre-coloured sheet maps an
        // identity to its frame without reading the presentation's own state (#1392).
        if (method.IsStatic && !ReviewedStaticOperation(method)) return;
        // A reviewed closed contract classifies the call first: it may be reached through a narrow interface that
        // lives outside the artwork layer (gameplay reads the frontend's map install through IGameplayPresentationContent).
        if (closedProviders?.TryInspect(call, semantic, method, exports, report, source) == true) return;
        if (!IsResourceType(method.ContainingType)) return;
        string owner = method.ContainingType.Name + "." + method.Name;
        string arguments = string.Join(", ", call.ArgumentList.Arguments.Select(arg => arg.Expression.ToString()));
        if (UnboundArguments(call, semantic) is { } unbound)
        {
            report.Consumers.Add(new(owner, source, "unresolved"));
            report.Gap(Domain(method.ContainingType.Name, method.Name) ?? method.ContainingType.Name, owner, source, unbound);
            return;
        }
        string? domain = Domain(method.ContainingType.Name, method.Name);
        string? key = domain is null ? null : ConstantKey(domain, call, semantic);
        if (domain is null || key is null)
        {
            string actualDomain = domain ?? method.ContainingType.Name;
            report.Consumers.Add(new(owner, source, "unresolved"));
            report.Gap(actualDomain, owner, source, domain is null
                ? "No definition-coverage adapter for this resource domain. Arguments: " + arguments
                : "Lookup arguments are not statically constant; adapter coverage does not prove this call's possible IDs. Arguments: " + arguments);
            return;
        }
        report.Require(domain, owner, key, source, exports);
        report.Consumers.Add(new(owner, source,             exports.Contains(domain, key) ? "resolved" : "missing"));
    }

    /// <summary>
    /// The compile errors inside a resource call's arguments, as a gap reason, or null when they bind. An argument that
    /// names an inaccessible constant has no constant value, which would otherwise widen its selection to the whole domain.
    /// </summary>
    internal static string? UnboundArguments(InvocationExpressionSyntax call, SemanticModel semantic)
    {
        Diagnostic[] errors = [.. semantic.GetDiagnostics(call.ArgumentList.Span)
            .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)];
        return errors.Length == 0 ? null
            : "Resource lookup arguments could not be bound: " + string.Join(" | ", errors.Select(error => error.GetMessage()));
    }

    /// <summary>Whether a source file belongs to the artwork layer: the shared Assets module or a domain's Artwork module.</summary>
    internal static bool IsArtworkLayer(string path) =>
        path.Contains("/Assets/", StringComparison.Ordinal) || path.Contains("/Artwork/", StringComparison.Ordinal);

    /// <summary>Recognizes the artwork-layer and room presentation types that may consume installed resources.</summary>
    private static bool IsResourceType(ITypeSymbol type)
    {
        // Arrays, pointers and type parameters have no namespace and are not resource types.
        return type.ContainingNamespace?.ToDisplayString() is { } space &&
            (space == "SuperMetroid.Core.Assets" || space == "SuperMetroid.Core.Rooms" || space.StartsWith("SuperMetroid.Core.Rooms.", StringComparison.Ordinal) ||
             space.EndsWith(".Artwork", StringComparison.Ordinal)) &&
            (type.Name.EndsWith("Catalog", StringComparison.Ordinal) ||
             type.Name.EndsWith("Presentation", StringComparison.Ordinal) ||
             type.Name.EndsWith("ColorSource", StringComparison.Ordinal) ||
             type.Name.EndsWith("Visuals", StringComparison.Ordinal) ||
             // A family's drawer over its pre-coloured sheet selects a frame by identity (#1392).
             (type.Name.EndsWith("Sheet", StringComparison.Ordinal) && space.EndsWith(".Artwork", StringComparison.Ordinal)));
    }

    /// <summary>Whether a reviewed closed contract names <paramref name="method"/>'s static operation.</summary>
    private static bool ReviewedStaticOperation(IMethodSymbol method) =>
        ClosedContracts.ClosedPresentationContractDefinitions.All.Any(contract =>
            contract.Type == method.ContainingType.ToDisplayString() && contract.Methods.Contains(method.Name, StringComparer.Ordinal));

    /// <summary>Maps a known presentation operation to the resource domain it addresses.</summary>
    private static string? Domain(string type, string method) => new PresentationMethod(type, method) switch
    {
        ("EnemySpritemapCatalog", "TryGetDisplay" or "TryGet") => ResourceDomains.EnemySimple,
        ("EnemyExtendedFrameCatalog", "TryGetDisplay" or "GetDisplayFrame" or "TryGet") => ResourceDomains.EnemyExtended,
        ("EnemyProjectileSpritemapCatalog", "GetProgramFrame") => ResourceDomains.EnemyProjectileProgram,
        ("EnemyProjectileSpritemapCatalog", "Get") => ResourceDomains.EnemyProjectileSprite,
        ("IPaletteFxColorSource" or "RoomPaletteFxPresentation", "TryReadColor") => ResourceDomains.PaletteFx,
        ("InstalledProjectileSprites", "Frame") => ResourceDomains.SamusProjectile,
        _ => null,
    };

    /// <summary>Builds a resource key from compiler-bound constant arguments, or leaves dynamic calls unresolved.</summary>
    /// <summary>The name of the enemy frame a display lookup passes as a constant, or null when the frame is computed.</summary>
    internal static string? EnemyFrameKey(Microsoft.CodeAnalysis.Operations.IInvocationOperation invocation)
    {
        IOperation? frame = invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Ordinal == 0)?.Value;
        while (frame is Microsoft.CodeAnalysis.Operations.IConversionOperation conversion)
            frame = conversion.Operand;
        return frame is Microsoft.CodeAnalysis.Operations.IFieldReferenceOperation { Field: { IsConst: true, ContainingType.Name: "EnemyFrameId" } field }
            ? field.Name
            : null;
    }

    /// <summary>The name of the projectile composition a lookup constructs from constants, or null when it is computed.</summary>
    internal static string? ProjectileSpriteKey(Microsoft.CodeAnalysis.Operations.IInvocationOperation invocation)
    {
        IOperation? sprite = invocation.Arguments.FirstOrDefault(argument => argument.Parameter?.Ordinal == 0)?.Value;
        while (sprite is Microsoft.CodeAnalysis.Operations.IConversionOperation conversion)
            sprite = conversion.Operand;
        if (sprite is not Microsoft.CodeAnalysis.Operations.IObjectCreationOperation { Type.Name: "ProjectileSpriteId" } creation ||
            creation.Arguments is not [{ Value.ConstantValue: { HasValue: true, Value: { } group } }, { Value.ConstantValue: { HasValue: true, Value: { } index } }])
            return null;
        return $"{(SuperMetroid.Core.Samus.Artwork.ProjectileSpriteGroup)Convert.ToInt32(group)}.{Convert.ToInt32(index)}";
    }

    private static string? ConstantKey(string domain, InvocationExpressionSyntax call, SemanticModel semantic)
    {
        // Match parameter identities, not textual argument order; named arguments
        // and constant casts remain compiler-resolved instead of regex guesses.
        if (semantic.GetOperation(call) is not Microsoft.CodeAnalysis.Operations.IInvocationOperation invocation) return null;
        if (domain is ResourceDomains.EnemySimple or ResourceDomains.EnemyExtended)
            return EnemyFrameKey(invocation);
        if (domain is ResourceDomains.SamusProjectile)
            return ProjectileSpriteKey(invocation);
        var numbers = new Dictionary<int, int>();
        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter?.RefKind is not RefKind.None) continue;
            Optional<object?> value = argument.Value.ConstantValue;
            if (!value.HasValue || value.Value is null) continue;
            try { numbers[argument.Parameter.Ordinal] = Convert.ToInt32(value.Value); }
            catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException) { }
        }
        if (!numbers.TryGetValue(0, out int first)) return null;
        int bank = domain switch
        {
            ResourceDomains.PaletteFx or ResourceDomains.EnemyProjectileSprite => ResourceBanks.ProjectileOamAndPaletteFx,
            ResourceDomains.EnemyProjectileProgram => ResourceBanks.EnemyProjectilePrograms,
            _ => throw new InvalidDataException("Unregistered resource address domain: " + domain),
        };
        return ResourceIndex.Address(bank, first);
    }
}
