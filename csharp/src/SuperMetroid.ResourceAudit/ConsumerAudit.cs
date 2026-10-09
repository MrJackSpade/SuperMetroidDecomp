using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Semantic source inventory of installed-resource lookups. A dynamic argument or
/// unregistered resource domain is an explicit coverage gap, never a clean result.
/// This is not a points-to proof of arbitrary gameplay state.
/// </summary>
internal static class ConsumerAudit
{
    /// <summary>Finds source resource lookups and compares their constant identities with audited exports.</summary>
    public static void Run(string root, ResourceIndex exports, AuditReport report)
    {
        CSharpCompilation compilation = CreateCompilation(root);
        var operands = new ProgramOperandAudit(exports, report);
        var closedProviders = new ClosedPresentationAudit(compilation, exports);
        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            // Catalog implementation is the provider, not a gameplay consumer.
            if (tree.FilePath.Contains("/Assets/", StringComparison.Ordinal) || tree.FilePath.StartsWith('<')) continue;
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
        typeof(SuperMetroid.Core.Game.RoomEnemySystem).Assembly.GetType(fullName) ?? typeof(ToolingTypes).Assembly.GetType(fullName);

    /// <summary>Builds a Roslyn compilation from Core and tooling sources with the installed SDK references.</summary>
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

    /// <summary>Classifies one invocation as a resource consumer and records missing or unresolved coverage.</summary>
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
                    "Resource lookup could not be bound semantically: " + call.Expression);
            return;
        }
        // Draw(), Segment(), Fragment(), LoadTo() and Color() also consume resources.
        // Core also consumes internal draw contracts. Accessibility is not a
        // coverage boundary; inventory instance operations, not just public Get().
        if (method.IsStatic || !IsResourceType(method.ContainingType)) return;
        string owner = method.ContainingType.Name + "." + method.Name;
        string arguments = string.Join(", ", call.ArgumentList.Arguments.Select(arg => arg.Expression.ToString()));
        if (closedProviders?.TryInspect(call, semantic, method, exports, report, source, arguments) == true) return;
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

    /// <summary>Checks whether a type name and namespace identify one of the audited resource providers.</summary>
    private static bool IsResourceType(ITypeSymbol type)
    {
        string space = type.ContainingNamespace.ToDisplayString();
        return (space == "SuperMetroid.Core.Assets" || space == "SuperMetroid.Core.Rooms") &&
            (type.Name.EndsWith("Catalog", StringComparison.Ordinal) ||
             type.Name.EndsWith("Presentation", StringComparison.Ordinal) ||
             type.Name.EndsWith("ColorSource", StringComparison.Ordinal) ||
             type.Name.EndsWith("Visuals", StringComparison.Ordinal));
    }

    /// <summary>Maps a provider operation to the resource identity domain it consumes.</summary>
    private static string? Domain(string type, string method) => (type, method) switch
    {
        ("EnemySpritemapCatalog", "TryGetDisplay" or "TryGet") => ResourceDomains.EnemySimple,
        ("EnemyExtendedFrameCatalog", "TryGetDisplay" or "GetDisplayPointer" or "TryGet") => ResourceDomains.EnemyExtended,
        ("EnemyProjectileSpritemapCatalog", "GetProgramFrame") => ResourceDomains.EnemyProjectileProgram,
        ("EnemyProjectileSpritemapCatalog", "Get") => ResourceDomains.EnemyProjectileSprite,
        ("IPaletteFxColorSource" or "RoomPaletteFxPresentation", "TryReadColor") => ResourceDomains.PaletteFx,
        ("ProjectileSpriteCatalog", "Draw") => ResourceDomains.SamusProjectile,
        _ => null,
    };

    /// <summary>Builds the bank/address key from constant invocation arguments and the provider's domain.</summary>
    private static string? ConstantKey(string domain, InvocationExpressionSyntax call, SemanticModel semantic)
    {
        // Match parameter identities, not textual argument order; named arguments
        // and constant casts remain compiler-resolved instead of regex guesses.
        if (semantic.GetOperation(call) is not Microsoft.CodeAnalysis.Operations.IInvocationOperation invocation) return null;
        var numbers = new Dictionary<int, int>();
        foreach (var argument in invocation.Arguments)
        {
            if (argument.Parameter?.RefKind is not RefKind.None) continue;
            Optional<object?> value = argument.Value.ConstantValue;
            if (!value.HasValue || value.Value is null) continue;
            try { numbers[argument.Parameter!.Ordinal] = Convert.ToInt32(value.Value); }
            catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException) { }
        }
        if (!numbers.TryGetValue(0, out int first)) return null;
        if (domain is ResourceDomains.EnemySimple or ResourceDomains.EnemyExtended)
            return numbers.TryGetValue(1, out int pointer) ? ResourceIndex.Address(first, pointer) : null;
        int bank = domain switch
        {
            ResourceDomains.PaletteFx or ResourceDomains.EnemyProjectileSprite => ResourceBanks.ProjectileOamAndPaletteFx,
            ResourceDomains.EnemyProjectileProgram => ResourceBanks.EnemyProjectilePrograms,
            ResourceDomains.SamusProjectile => ResourceBanks.SamusProjectiles,
            _ => throw new InvalidDataException("Unregistered resource address domain: " + domain),
        };
        return ResourceIndex.Address(bank, first);
    }
}
