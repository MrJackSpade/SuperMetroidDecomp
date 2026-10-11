using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>
/// Confines the SNES CPU A-bus layout to approved boundaries (#629). Raw address-space reads and
/// writes, raw ROM/WRAM/SRAM buffers and address-bound cell primitives are declared
/// <c>[CpuBusAccess]</c>; only declarations inside a <c>[CpuBusBoundary("reason")]</c> may use them.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CpuBusAccessAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Raw CPU-bus access outside an approved boundary.</summary>
    public const string RawAccessId = "SME6290";

    /// <summary>A raw CPU-bus contract implemented outside an approved boundary.</summary>
    public const string RawImplementationId = "SME6291";

    /// <summary>Marks a raw CPU-bus API.</summary>
    public const string AccessAttributeName = "CpuBusAccessAttribute";

    /// <summary>Marks an approved CPU-bus boundary.</summary>
    public const string BoundaryAttributeName = "CpuBusBoundaryAttribute";

    private static readonly DiagnosticDescriptor RawAccessRule = new(
        RawAccessId,
        "Raw CPU-bus access outside a memory boundary",
        "'{0}' is raw CPU-bus access; use a typed, domain-named memory view",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Addresses, banks, widths, byte order and encodings belong to typed domain memory views, the " +
            "address-space implementation, native-layout codecs and exact bus tests, which declare [CpuBusBoundary].");

    private static readonly DiagnosticDescriptor RawImplementationRule = new(
        RawImplementationId,
        "Raw CPU-bus contract implemented outside a memory boundary",
        "{0} implements raw CPU-bus contract '{1}'; declare it an approved [CpuBusBoundary] or use a typed memory view",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Only the address-space implementation and exact bus test doubles may implement raw CPU-bus contracts.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [RawAccessRule, RawImplementationRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeOperation,
            OperationKind.Invocation, OperationKind.PropertyReference, OperationKind.MethodReference,
            OperationKind.ObjectCreation);
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    private static void AnalyzeOperation(OperationAnalysisContext context)
    {
        ISymbol? target = context.Operation switch
        {
            IInvocationOperation invocation => invocation.TargetMethod,
            IPropertyReferenceOperation property => property.Property,
            IMethodReferenceOperation reference => reference.Method,
            IObjectCreationOperation creation => creation.Constructor,
            _ => null,
        };
        if (target is null || !IsRawAccess(target) || InBoundary(context.ContainingSymbol))
            return;
        context.ReportDiagnostic(Diagnostic.Create(RawAccessRule, context.Operation.Syntax.GetLocation(),
            $"{target.ContainingType?.Name}.{(target is IMethodSymbol { MethodKind: MethodKind.Constructor } ? "ctor" : target.Name)}"));
    }

    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct) || InBoundary(type))
            return;
        foreach (INamedTypeSymbol contract in type.Interfaces)
        {
            if (!HasAttribute(contract, AccessAttributeName))
                continue;
            context.ReportDiagnostic(Diagnostic.Create(RawImplementationRule, type.Locations[0], type.Name, contract.Name));
            return;
        }
    }

    /// <summary>Whether <paramref name="member"/> is a raw API: attributed itself, declared by an attributed
    /// type, or a concrete implementation of an attributed interface's member.</summary>
    private static bool IsRawAccess(ISymbol member)
    {
        ISymbol original = member.OriginalDefinition;
        if (HasAttribute(original, AccessAttributeName) ||
            original.ContainingType is { } owner && HasAttribute(owner, AccessAttributeName))
            return true;
        if (original.ContainingType is not { } declaring || original.IsStatic)
            return false;
        foreach (INamedTypeSymbol contract in declaring.AllInterfaces)
        {
            if (!HasAttribute(contract, AccessAttributeName))
                continue;
            foreach (ISymbol contractMember in contract.GetMembers())
            {
                if (SymbolEqualityComparer.Default.Equals(declaring.FindImplementationForInterfaceMember(contractMember), original))
                    return true;
            }
        }
        return false;
    }

    /// <summary>Whether <paramref name="symbol"/> lies inside a boundary or a raw primitive's own implementation.</summary>
    private static bool InBoundary(ISymbol? symbol)
    {
        for (ISymbol? current = symbol; current is not null; current = current.ContainingSymbol)
        {
            if (current is INamespaceSymbol)
                return false;
            if (HasAttribute(current, BoundaryAttributeName) ||
                current is IMethodSymbol { AssociatedSymbol: { } property } && HasAttribute(property, BoundaryAttributeName) ||
                current is INamedTypeSymbol && HasAttribute(current, AccessAttributeName))
                return true;
        }
        return false;
    }

    private static bool HasAttribute(ISymbol symbol, string name) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == name);
}
