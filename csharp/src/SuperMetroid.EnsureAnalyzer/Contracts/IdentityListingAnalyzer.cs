using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Behaviour reads a property, never tests which listed identity a value is (SME6484, #2559). In
/// <c>SuperMetroid.Core</c> and <c>SuperMetroid.AssetExtraction</c> the rule reports a test that lists two or more members
/// of one identity enum (an enum named <c>…Id</c> that is not <c>[Flags]</c>) to decide something about a value: a pattern
/// test that combines them (<c>pose is SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose</c>,
/// <c>area is not (AreaId.Brinstar or AreaId.Tourian)</c>), and a chain of equality tests of one value against them
/// (<c>enemy == EnemyDefinitionId.Ridley || enemy == EnemyDefinitionId.RidleyCeres</c>).
/// </summary>
/// <remarks>
/// The trait the listed members share is declared once, as a property of each member's definition or a named family the
/// definitions list, and the behaviour reads it (<c>SamusPoseMetadata.IsSpaceJumpPose(pose)</c>), so adding a member never
/// means hunting down the tests that list its kin. A switch that maps each member to its own value is a lookup, not a
/// listing test, and a test of one member, a named constant or a value another member's definition holds is not reported.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IdentityListingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A test listing identity members.</summary>
    public const string IdentityListingId = "SME6484";

    /// <summary>The import pipeline's assembly, which the rule reads beside Core.</summary>
    private const string ExtractionAssembly = "SuperMetroid.AssetExtraction";

    /// <summary>The fewest members of one identity a test names to be a listing.</summary>
    private const int ListingMembers = 2;

    private static readonly DiagnosticDescriptor Rule = new(
        IdentityListingId,
        "Behaviour tests which listed identity a value is",
        "{0} lists {1} members of {2}; read a property the members' definitions declare instead",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A behaviour that depends on a trait reads that trait from the member's definition (#2559): a test " +
            "listing which identities share it must be found and extended whenever a member is added, and says nothing of " +
            "what the members have in common.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName is not (CoreLayerAnalyzer.CoreAssembly or ExtractionAssembly))
                return;
            start.RegisterOperationAction(AnalyzeIsPattern, OperationKind.IsPattern);
            start.RegisterOperationAction(AnalyzeLogical, OperationKind.Binary);
        });
    }

    /// <summary>Reports a pattern test that combines two or more members of one identity.</summary>
    private static void AnalyzeIsPattern(OperationAnalysisContext context)
    {
        var test = (IIsPatternOperation)context.Operation;
        var members = new List<IFieldSymbol>();
        CollectPatternMembers(test.Pattern, members);
        ReportListing(context, test.Syntax, members, "The pattern test");
    }

    /// <summary>Reports the outermost conditional chain whose equality tests of one value name two or more members of one identity.</summary>
    private static void AnalyzeLogical(OperationAnalysisContext context)
    {
        var binary = (IBinaryOperation)context.Operation;
        if (!IsConditionalLogical(binary) || binary.Parent is { } above && Unwrapped(above) is IBinaryOperation parent && IsConditionalLogical(parent))
            return;
        var tested = new Dictionary<string, List<IFieldSymbol>>(StringComparer.Ordinal);
        CollectEqualityMembers(binary, tested);
        foreach (List<IFieldSymbol> members in tested.Values)
            ReportListing(context, binary.Syntax, members, "The chain of equality tests");
    }

    /// <summary>Reports <paramref name="syntax"/> when <paramref name="members"/> holds enough members of one identity.</summary>
    private static void ReportListing(OperationAnalysisContext context, SyntaxNode syntax, List<IFieldSymbol> members, string what)
    {
        foreach (IGrouping<INamedTypeSymbol, IFieldSymbol> identity in members.GroupBy<IFieldSymbol, INamedTypeSymbol>(member => member.ContainingType, SymbolEqualityComparer.Default))
        {
            int distinct = identity.Select(member => member.Name).Distinct(StringComparer.Ordinal).Count();
            if (distinct >= ListingMembers)
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, syntax.GetLocation(), what, distinct, identity.Key.Name));
                return;
            }
        }
    }

    /// <summary>Collects the identity members a pattern's constant patterns name, through its combinators.</summary>
    private static void CollectPatternMembers(IPatternOperation pattern, List<IFieldSymbol> members)
    {
        switch (pattern)
        {
            case IBinaryPatternOperation combined:
                CollectPatternMembers(combined.LeftPattern, members);
                CollectPatternMembers(combined.RightPattern, members);
                break;
            case INegatedPatternOperation negated:
                CollectPatternMembers(negated.Pattern, members);
                break;
            case IConstantPatternOperation constant when IdentityMember(constant.Value) is { } member:
                members.Add(member);
                break;
            default:
                // A type, relational or declaration pattern, or a constant that is no identity member, names none.
                break;
        }
    }

    /// <summary>
    /// Collects, by the tested value's text, the identity members a conditional chain's equality and inequality tests name.
    /// </summary>
    private static void CollectEqualityMembers(IOperation operation, Dictionary<string, List<IFieldSymbol>> tested)
    {
        operation = Unwrapped(operation);
        if (operation is not IBinaryOperation binary)
            return;
        if (IsConditionalLogical(binary))
        {
            CollectEqualityMembers(binary.LeftOperand, tested);
            CollectEqualityMembers(binary.RightOperand, tested);
            return;
        }
        if (binary.OperatorKind is not (BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals))
            return;
        foreach ((IOperation side, IOperation other) in new[] { (binary.LeftOperand, binary.RightOperand), (binary.RightOperand, binary.LeftOperand) })
        {
            if (IdentityMember(side) is not { } member)
                continue;
            string value = Unwrapped(other).Syntax.ToString();
            if (!tested.TryGetValue(value, out List<IFieldSymbol>? members))
                tested[value] = members = [];
            members.Add(member);
            return;
        }
    }

    /// <summary>The member of an identity enum <paramref name="operation"/> names directly, or null.</summary>
    private static IFieldSymbol? IdentityMember(IOperation operation) =>
        Unwrapped(operation) is IFieldReferenceOperation { Field: { } field } &&
        field.ContainingType is { TypeKind: TypeKind.Enum } enumType &&
        enumType.Name.EndsWith("Id", StringComparison.Ordinal) &&
        !enumType.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "FlagsAttribute")
            ? field
            : null;

    /// <summary>Whether <paramref name="binary"/> is a conditional and or or.</summary>
    private static bool IsConditionalLogical(IBinaryOperation binary) =>
        binary.OperatorKind is BinaryOperatorKind.ConditionalOr or BinaryOperatorKind.ConditionalAnd;

    /// <summary><paramref name="operation"/> with its conversions and parentheses removed.</summary>
    private static IOperation Unwrapped(IOperation operation)
    {
        while (operation is IConversionOperation or IParenthesizedOperation)
            operation = operation is IConversionOperation conversion ? conversion.Operand : ((IParenthesizedOperation)operation).Operand;
        return operation;
    }
}
