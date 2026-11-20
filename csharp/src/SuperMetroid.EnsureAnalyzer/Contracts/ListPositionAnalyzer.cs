using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Relations are references, never list positions (SME6482, #2556): in <c>SuperMetroid.Core</c>, outside an
/// <c>[EncodingOwner]</c> type and a <c>[CompatibilityService]</c>, no code reads a place's <c>Ordinal</c>, its position in
/// the list it belongs to, other than to spell it in an interpolated message. A partner found by counting places from
/// another (<c>part.Ordinal - 1</c>), a place number handed to a lookup or stored (<c>Inherit(place.Ordinal, ...)</c>) and
/// a loader's progress asked by number all read it. A member of a multipart enemy finds another by its role through the one
/// owner of the population's listing, a rule that reproduces a reach by placement is a compatibility service, and a place
/// is passed as itself.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ListPositionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Core reads a place's position in its list.</summary>
    public const string ListPositionId = "SME6482";

    /// <summary>The member that holds a place's position in its list.</summary>
    private const string OrdinalMember = "Ordinal";

    /// <summary>The attribute that marks the single owner of an encoding, which may decode a list's positions.</summary>
    private const string EncodingOwnerAttribute = "EncodingOwnerAttribute";

    /// <summary>The attribute that marks a compatibility service, which may reproduce a reach by placement.</summary>
    private const string CompatibilityServiceAttribute = "CompatibilityServiceAttribute";

    private static readonly DiagnosticDescriptor Rule = new(
        ListPositionId,
        "Core reads a place's position in its list",
        "'{0}' reads a place's position in its list; hold the partner as a reference found by its role, or let a compatibility service reproduce a reach by placement",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Identities and relations never use list positions or slot numbers. A multipart enemy's members find " +
            "each other by role through the one owner of the population's listing and keep the place they find; a rule the " +
            "game makes depend on placement itself is a compatibility service; a place is handed on as itself, never as its " +
            "number. Spelling the number in an interpolated message is not a read (#2556).");

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
            start.RegisterOperationAction(AnalyzePropertyReference, OperationKind.PropertyReference);
        });
    }

    /// <summary>Reports a read of a place's <c>Ordinal</c> outside an owner and outside an interpolated message.</summary>
    private static void AnalyzePropertyReference(OperationAnalysisContext context)
    {
        var reference = (IPropertyReferenceOperation)context.Operation;
        IPropertySymbol property = reference.Property;
        if (property.Name != OrdinalMember || property.Type.SpecialType != SpecialType.System_Int32 ||
            !SymbolEqualityComparer.Default.Equals(property.ContainingAssembly, context.Compilation.Assembly))
            return;
        if (IsAssigned(reference) || IsSpelledInMessage(reference) || IsInOwner(context.ContainingSymbol))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, reference.Syntax.GetLocation(), reference.Syntax.ToString()));
    }

    /// <summary>Whether the reference is the target of an assignment, which sets the position rather than reading it.</summary>
    private static bool IsAssigned(IOperation reference) =>
        reference.Parent is IAssignmentOperation assignment && assignment.Target == reference;

    /// <summary>Whether the reference only spells the position in an interpolated string.</summary>
    private static bool IsSpelledInMessage(IOperation reference)
    {
        for (IOperation? parent = reference.Parent; parent is not null; parent = parent.Parent)
        {
            if (parent is IInterpolationOperation)
                return true;
            if (parent is not IConversionOperation and not IBinaryOperation and not IParenthesizedOperation)
                return false;
        }
        return false;
    }

    /// <summary>Whether the site lies in a type that owns an encoding or reproduces the original's behaviour.</summary>
    private static bool IsInOwner(ISymbol site)
    {
        for (INamedTypeSymbol? type = site as INamedTypeSymbol ?? site.ContainingType; type is not null; type = type.ContainingType)
        {
            if (type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name is EncodingOwnerAttribute or CompatibilityServiceAttribute))
                return true;
        }
        return false;
    }
}
