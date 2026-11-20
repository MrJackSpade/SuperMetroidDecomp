using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports a native-sized integer (<c>IntPtr</c>, <c>UIntPtr</c>, <c>nint</c>, <c>nuint</c>) or a raw pointer type outside
/// native interop (SME6456, #1509). Two areas that share state share one strongly typed instance injected into both, scoped
/// to the session (<c>docs/godot-architecture.md</c>, "Shared state and dependency injection"), never an address, a handle
/// or a raw buffer.
/// </summary>
/// <remarks>
/// Native interop is outside the rule by definition (owner decision, 2026-11-02): it is removed whole with the Godot port in
/// v1.0.0, so it takes no per-site justification. Interop is the native host assemblies, which hand the operating system and
/// the GPU handles and pinned buffers, and, in any other assembly, a type that declares a platform invoke together with
/// everything nested in it.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NativeAddressAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A native-sized integer or a raw pointer outside native interop.</summary>
    public const string NativeAddressId = "SME6456";

    /// <summary>The native host assemblies: the desktop host and its Direct3D 11 renderer, whose interop leaves in v1.0.0.</summary>
    internal static readonly ImmutableHashSet<string> NativeHostAssemblies =
        ImmutableHashSet.Create(StringComparer.Ordinal, "SuperMetroid.Desktop", "SuperMetroid.Rendering.Direct3D11");

    /// <summary>The attribute that declares a source-generated platform invoke.</summary>
    private const string LibraryImportAttribute = "System.Runtime.InteropServices.LibraryImportAttribute";

    private static readonly DiagnosticDescriptor Rule = new(
        NativeAddressId,
        "A native address outside native interop",
        "'{0}' is a native address outside native interop; areas that share state share one typed instance injected into both",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Native-sized integers and raw pointers belong only to the native interop the Godot port removes. Two " +
            "areas that share state share one strongly typed, session-scoped instance injected into both (#1509, " +
            "docs/godot-architecture.md, \"Shared state and dependency injection\").");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName is { } assembly && NativeHostAssemblies.Contains(assembly))
                return;
            var interop = new ConcurrentDictionary<INamedTypeSymbol, bool>(SymbolEqualityComparer.Default);
            start.RegisterSyntaxNodeAction(node => Analyze(node, interop), SyntaxKind.IdentifierName, SyntaxKind.PointerType);
        });
    }

    /// <summary>Reports <paramref name="context"/>'s node when it spells a native address outside interop.</summary>
    private static void Analyze(SyntaxNodeAnalysisContext context, ConcurrentDictionary<INamedTypeSymbol, bool> interop)
    {
        SyntaxNode node = context.Node;
        if (node is IdentifierNameSyntax name)
        {
            // 'var' takes its type from the expression the rule already reads; a pointer is reported once, whole.
            if (name.IsVar || node.Ancestors().Any(ancestor => ancestor is PointerTypeSyntax))
                return;
            if (context.SemanticModel.GetSymbolInfo(name, context.CancellationToken).Symbol is not ITypeSymbol
                { SpecialType: SpecialType.System_IntPtr or SpecialType.System_UIntPtr })
                return;
        }
        if (InInterop(node, context.SemanticModel, interop, context.CancellationToken))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), node.ToString()));
    }

    /// <summary>Whether <paramref name="node"/> lies in a type that declares a platform invoke, or in one nested in it.</summary>
    private static bool InInterop(SyntaxNode node, SemanticModel model, ConcurrentDictionary<INamedTypeSymbol, bool> interop,
        CancellationToken cancellation)
    {
        if (node.Ancestors().OfType<BaseTypeDeclarationSyntax>().LastOrDefault() is not { } outermost ||
            model.GetDeclaredSymbol(outermost, cancellation) is not INamedTypeSymbol type)
            return false;
        return interop.GetOrAdd(type, DeclaresPlatformInvoke);
    }

    /// <summary>Whether <paramref name="type"/> or a type nested in it declares a platform invoke.</summary>
    private static bool DeclaresPlatformInvoke(INamedTypeSymbol type) =>
        type.GetMembers().OfType<IMethodSymbol>().Any(method => method.GetDllImportData() is not null ||
            method.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == LibraryImportAttribute)) ||
        type.GetTypeMembers().Any(DeclaresPlatformInvoke);
}
