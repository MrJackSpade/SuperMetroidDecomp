using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Keeps Core's compatibility layer recognisable (#1401, SME6439). Hardware behaviour the game needs survives only as a
/// compatibility service: a type in the <c>Compatibility</c> module, marked <c>[CompatibilityService]</c> with the
/// gameplay behaviour it reproduces, owning typed state. The rule reports a type in the module or implementing one of its
/// contracts without the mark, the mark on a type outside the module, and a memory-image field (an array, span or memory
/// of bytes or words) on a marked type. See <c>docs/compatibility-layer.md</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CompatibilityServiceAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A compatibility type without its mark, a mark outside the layer, or a memory image on a service.</summary>
    public const string CompatibilityServiceId = "SME6439";

    /// <summary>The namespace of Core's compatibility module.</summary>
    internal const string ModuleNamespace = CoreLayerAnalyzer.CoreAssembly + ".Compatibility";

    /// <summary>The attribute that marks a compatibility service and names the behaviour it reproduces.</summary>
    private const string AttributeName = "CompatibilityServiceAttribute";

    private static readonly DiagnosticDescriptor Rule = new(
        CompatibilityServiceId,
        "Compatibility service shape",
        "{0}",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A compatibility service lives in the Compatibility module, is marked [CompatibilityService] with the " +
            "gameplay behaviour it reproduces, and owns typed state rather than a memory image (#1401).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            start.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        });
    }

    /// <summary>Checks one top-level type's place, mark and fields.</summary>
    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.ContainingType is not null || type.IsImplicitlyDeclared || type.Locations.FirstOrDefault(l => l.IsInSource) is not { } location)
            return;
        bool marked = type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == AttributeName);
        bool inModule = InModule(type);
        if (inModule && !marked && !IsAttribute(type))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location,
                $"'{type.Name}' is in the compatibility layer; mark it [CompatibilityService] with the gameplay behaviour it reproduces"));
        }
        if (!inModule && marked)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location,
                $"'{type.Name}' is marked [CompatibilityService] outside the compatibility layer; move it to {ModuleNamespace}"));
        }
        if (!inModule && type.AllInterfaces.FirstOrDefault(InModule) is { } contract)
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location,
                $"'{type.Name}' implements the compatibility contract '{contract.Name}'; it belongs in {ModuleNamespace}, marked [CompatibilityService]"));
        }
        if (!marked)
            return;
        foreach (IFieldSymbol field in type.GetMembers().OfType<IFieldSymbol>())
        {
            if (!field.IsImplicitlyDeclared && IsMemoryImage(field.Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, field.Locations.FirstOrDefault() ?? location,
                    $"'{field.Name}' holds a memory image ({field.Type.ToDisplayString()}); a compatibility service owns typed state"));
            }
        }
    }

    /// <summary>Whether a type is declared in the compatibility module's namespace.</summary>
    private static bool InModule(INamedTypeSymbol type)
    {
        string space = type.ContainingNamespace?.ToDisplayString() ?? "";
        return space == ModuleNamespace || space.StartsWith(ModuleNamespace + ".", StringComparison.Ordinal);
    }

    /// <summary>Whether a type is an attribute class.</summary>
    private static bool IsAttribute(INamedTypeSymbol type)
    {
        for (INamedTypeSymbol? current = type.BaseType; current is not null; current = current.BaseType)
        {
            if (current.ToDisplayString() == "System.Attribute")
                return true;
        }
        return false;
    }

    /// <summary>Whether a type is an array, span or memory of bytes or words.</summary>
    private static bool IsMemoryImage(ITypeSymbol type)
    {
        static bool Word(ITypeSymbol element) => element.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte or
            SpecialType.System_UInt16 or SpecialType.System_Int16;
        return type switch
        {
            IArrayTypeSymbol array => Word(array.ElementType),
            INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } generic =>
                generic.Name is "Span" or "ReadOnlySpan" or "Memory" or "ReadOnlyMemory" && Word(generic.TypeArguments[0]),
            _ => false,
        };
    }
}
