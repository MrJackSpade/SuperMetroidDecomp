using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports a sixteen-bit coordinate wrap taken outside a compatibility service (SME6463, #2568). A wrap exists in Core
/// only where it reproduces the original's arithmetic, through a compatibility service named for the gameplay behaviour
/// the wrap produces: an actor placed past the room's top or left edge, a distance taken the short way across the
/// room's coordinates, the camera carried before the room's origin, a terrain scan's edge past the room. The rule
/// reports every call of, and every method group naming, a member of the coordinate family that wraps: any method of
/// <c>CoordinateAxis</c>, <c>Position16&lt;T&gt;.Wrapped</c> and <c>Offset16&lt;T&gt;.Wrapped</c>,
/// <c>FixedCoordinate.WithWrappedWhole</c>, and <c>FixedPosition&lt;T&gt;.WithWrappedWholeX</c> and
/// <c>WithWrappedWholeY</c>.
/// </summary>
/// <remarks>
/// A use inside a <c>[CompatibilityService]</c> type, read on the use's type and every type containing it, is that
/// service reproducing its behaviour. The spatial namespace declares the coordinate family and its conversions between
/// spaces, which are the representation's own arithmetic, and is exempt; so are the family's translation operators, which
/// wrap as the positions' arithmetic and are not wraps a site chooses. Only Core is read.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CoordinateWrapAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A sixteen-bit coordinate wrap taken outside a compatibility service.</summary>
    public const string CoordinateWrapId = "SME6463";

    /// <summary>The namespace of the spatial types, which declares the coordinate family.</summary>
    private const string SpatialNamespace = "SuperMetroid.Core.Spatial";

    /// <summary>The per-axis form of the wrap, every method of which wraps.</summary>
    private const string AxisType = "CoordinateAxis";

    /// <summary>The attribute whose types may take a wrap: a compatibility service.</summary>
    private const string CompatibilityServiceAttribute = "CompatibilityServiceAttribute";

    private static readonly DiagnosticDescriptor Rule = new(
        CoordinateWrapId,
        "Coordinate wrap outside a compatibility service",
        "'{0}' wraps a sixteen-bit coordinate outside a compatibility service; take it through the compatibility service " +
        "that names the gameplay behaviour the wrap reproduces, or compute it plainly where no gameplay reaches the wrap",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A sixteen-bit coordinate wrap exists in Core only where it reproduces the original's arithmetic, " +
            "through a compatibility service named for the gameplay behaviour it produces (#2568): CoordinateAxis, the " +
            "positions' Wrapped factories and the fixed positions' WithWrappedWhole members are called only by " +
            "[CompatibilityService] types and the spatial types that declare them.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            start.RegisterOperationAction(Analyze, OperationKind.Invocation, OperationKind.MethodReference);
        });
    }

    /// <summary>Reports a wrapping member called or named outside a compatibility service and the spatial namespace.</summary>
    private static void Analyze(OperationAnalysisContext context)
    {
        IMethodSymbol? method = context.Operation switch
        {
            IInvocationOperation invocation => invocation.TargetMethod,
            IMethodReferenceOperation reference => reference.Method,
            _ => null,
        };
        if (method is null || !Wraps(method) || InSpatialNamespace(context.ContainingSymbol.ContainingNamespace))
            return;
        for (INamedTypeSymbol? type = context.ContainingSymbol.ContainingType; type is not null; type = type.ContainingType)
        {
            if (type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == CompatibilityServiceAttribute))
                return;
        }
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(),
            $"{method.ContainingType.Name}.{method.Name}"));
    }

    /// <summary>Whether <paramref name="method"/> is one of the coordinate family's wrapping members.</summary>
    private static bool Wraps(IMethodSymbol method)
    {
        INamedTypeSymbol type = method.ContainingType;
        if (type.ContainingNamespace?.ToDisplayString() != SpatialNamespace)
            return false;
        return (type.Name, method.Name) switch
        {
            (AxisType, _) => true,
            ("Position16" or "Offset16", "Wrapped") => true,
            ("FixedCoordinate", "WithWrappedWhole") => true,
            ("FixedPosition", "WithWrappedWholeX" or "WithWrappedWholeY") => true,
            _ => false,
        };
    }

    /// <summary>Whether <paramref name="space"/> is the spatial namespace or lies inside it.</summary>
    private static bool InSpatialNamespace(INamespaceSymbol? space)
    {
        string name = space?.ToDisplayString() ?? "";
        return name == SpatialNamespace || name.StartsWith(SpatialNamespace + ".", StringComparison.Ordinal);
    }
}
