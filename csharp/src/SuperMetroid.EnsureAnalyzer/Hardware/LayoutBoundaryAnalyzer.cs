using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>
/// Confines one hardware layout to declared boundaries. A raw API is marked with the subclass's
/// access attribute (on the member, its declaring type, or an interface it implements); only code
/// inside a declaration carrying the boundary attribute — or inside a raw API type's own
/// implementation — may use it, and only boundaries may implement a raw interface.
/// </summary>
public abstract class LayoutBoundaryAnalyzer(
    string accessAttributeName,
    string boundaryAttributeName,
    DiagnosticDescriptor rawAccessRule,
    DiagnosticDescriptor rawImplementationRule) : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [rawAccessRule, rawImplementationRule];

    /// <inheritdoc />
    public sealed override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeOperation,
            OperationKind.Invocation, OperationKind.PropertyReference, OperationKind.MethodReference,
            OperationKind.ObjectCreation, OperationKind.FieldReference);
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    /// <summary>Reports a raw layout API call or reference when its enclosing declaration lacks the matching boundary.</summary>
    private void AnalyzeOperation(OperationAnalysisContext context)
    {
        ISymbol? target = context.Operation switch
        {
            IInvocationOperation invocation => invocation.TargetMethod,
            IPropertyReferenceOperation property => property.Property,
            IMethodReferenceOperation reference => reference.Method,
            IObjectCreationOperation creation => creation.Constructor,
            IFieldReferenceOperation field => field.Field,
            _ => null,
        };
        // Constructing a backend with no arguments carries no layout knowledge.
        if (target is null || target is IMethodSymbol { MethodKind: MethodKind.Constructor, Parameters.Length: 0 } ||
            !IsRawAccess(target) || InBoundary(context.ContainingSymbol))
            return;
        context.ReportDiagnostic(Diagnostic.Create(rawAccessRule, context.Operation.Syntax.GetLocation(),
            $"{target.ContainingType?.Name}.{(target is IMethodSymbol { MethodKind: MethodKind.Constructor } ? "ctor" : target.Name)}"));
    }

    /// <summary>Rejects implementing a raw layout interface outside an attributed boundary.</summary>
    private void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct) || InBoundary(type))
            return;
        foreach (INamedTypeSymbol contract in type.Interfaces)
        {
            if (!HasAttribute(contract, accessAttributeName))
                continue;
            context.ReportDiagnostic(Diagnostic.Create(rawImplementationRule, type.Locations[0], type.Name, contract.Name));
            return;
        }
    }

    /// <summary>Whether <paramref name="member"/> is a raw API: attributed itself, declared by an attributed
    /// type, or a concrete implementation of an attributed interface's member.</summary>
    private bool IsRawAccess(ISymbol member)
    {
        ISymbol original = member.OriginalDefinition;
        if (HasAttribute(original, accessAttributeName) ||
            original.ContainingType is { } owner && HasAttribute(owner, accessAttributeName))
            return true;
        if (original.ContainingType is not { } declaring || original.IsStatic)
            return false;
        foreach (INamedTypeSymbol contract in declaring.AllInterfaces)
        {
            if (!HasAttribute(contract, accessAttributeName))
                continue;
            foreach (ISymbol contractMember in contract.GetMembers())
            {
                if (SymbolEqualityComparer.Default.Equals(declaring.FindImplementationForInterfaceMember(contractMember), original))
                    return true;
            }
        }
        return false;
    }

    /// <summary>Whether <paramref name="symbol"/> lies inside a boundary or a raw API type's own implementation.</summary>
    private bool InBoundary(ISymbol? symbol)
    {
        for (ISymbol? current = symbol; current is not null; current = current.ContainingSymbol)
        {
            if (current is INamespaceSymbol)
                return false;
            if (HasAttribute(current, boundaryAttributeName) ||
                current is IMethodSymbol { AssociatedSymbol: { } property } && HasAttribute(property, boundaryAttributeName) ||
                current is INamedTypeSymbol && HasAttribute(current, accessAttributeName))
                return true;
        }
        return false;
    }

    /// <summary>Matches an analyzer marker by its short attribute type name.</summary>
    private static bool HasAttribute(ISymbol symbol, string name) =>
        symbol.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == name);
}
