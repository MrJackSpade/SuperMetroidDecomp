using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Finds spatial values carried as parallel scalar components (#628): paired X/Y,
/// width/height or horizontal/vertical members of one type, and adjacent paired parameters.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SpatialClumpAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A type exposes the components of one spatial value as separate members.</summary>
    public const string MemberClumpId = "SME6280";

    /// <summary>A signature passes one spatial value as adjacent scalar parameters.</summary>
    public const string ParameterClumpId = "SME6281";

    /// <summary>A type keeps the components of one spatial value in parallel collections.</summary>
    public const string CollectionClumpId = "SME6282";

    /// <summary>A spatial value is rebuilt component by component where a whole-value operation exists.</summary>
    public const string ComponentArithmeticId = "SME6283";

    /// <summary>Warns when matched axes are stored in parallel index-aligned collections.</summary>
    private static readonly DiagnosticDescriptor CollectionClumpRule = new(
        CollectionClumpId,
        "Spatial values stored as parallel collections",
        "'{0}' and '{1}' of {2} hold the {3} components of one spatial value per element; store a collection of typed spatial values",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Parallel X/Y (or width/height, horizontal/vertical) arrays or lists indexed by the same element must be one " +
            "collection of spatial values unless a measured data-oriented layout is encapsulated behind [SpatialComponents].");

    /// <summary>Warns when a same-type spatial operator is rebuilt from separate axis arithmetic.</summary>
    private static readonly DiagnosticDescriptor ComponentArithmeticRule = new(
        ComponentArithmeticId,
        "Spatial value rebuilt component by component",
        "{0} is built from per-axis '{1}' of '{2}' and '{3}'; use the whole-value operator",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A spatial value constructed from the per-axis sum or difference of two spatial values duplicates the " +
            "type's whole-value operator and its wrap rules.");

    /// <summary>Marks an approved spatial value type or a narrow storage/hardware adapter.</summary>
    public const string SpatialComponentsAttributeName = "SpatialComponentsAttribute";

    /// <summary>Warns when one type declares separate scalar members for a single spatial value.</summary>
    private static readonly DiagnosticDescriptor MemberClumpRule = new(
        MemberClumpId,
        "Spatial value stored as parallel components",
        "'{0}' and '{1}' of {2} are the {3} components of one spatial value; store a typed spatial value",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Paired X/Y, width/height or horizontal/vertical members form one location, vector or size and must be one typed value. " +
            "Spatial value types and exact storage/hardware adapters declare [SpatialComponents].");

    /// <summary>Warns when adjacent parameters carry matching scalar axes.</summary>
    private static readonly DiagnosticDescriptor ParameterClumpRule = new(
        ParameterClumpId,
        "Spatial value passed as parallel parameters",
        "Parameters '{0}' and '{1}' of {2} are the {3} components of one spatial value; accept a typed spatial value",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A signature that conceptually accepts one location, vector or size must take one typed value. " +
            "Spatial value types and exact storage/hardware adapters declare [SpatialComponents].");

    /// <summary>The axis-name pairs recognized at identifier word boundaries.</summary>
    private static readonly (string First, string Second, string Axis)[] AxisPairs =
    [
        ("X", "Y", "X/Y"),
        ("Width", "Height", "width/height"),
        ("Horizontal", "Vertical", "horizontal/vertical"),
    ];

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [MemberClumpRule, ParameterClumpRule, CollectionClumpRule, ComponentArithmeticRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        context.RegisterOperationAction(AnalyzeConstruction, OperationKind.ObjectCreation);
    }

    /// <summary>Detects a two-axis constructor that duplicates addition or subtraction of whole spatial values.</summary>
    private static void AnalyzeConstruction(OperationAnalysisContext context)
    {
        var creation = (IObjectCreationOperation)context.Operation;
        if (creation.Type is not INamedTypeSymbol type || !IsSpatialValueType(type) || creation.Arguments.Length != 2 ||
            IsExempt(context.ContainingSymbol))
            return;
        if (Unwrap(creation.Arguments[0].Value) is not IBinaryOperation first ||
            Unwrap(creation.Arguments[1].Value) is not IBinaryOperation second ||
            first.OperatorKind != second.OperatorKind ||
            first.OperatorKind is not (BinaryOperatorKind.Add or BinaryOperatorKind.Subtract))
            return;
        // Both arguments must combine the X then Y members of the same two spatial receivers.
        if (AxisOf(first.LeftOperand, "X") is not { } leftX || AxisOf(first.RightOperand, "X") is not { } rightX ||
            AxisOf(second.LeftOperand, "Y") is not { } leftY || AxisOf(second.RightOperand, "Y") is not { } rightY ||
            leftX != leftY || rightX != rightY)
            return;
        // Only a same-type combination duplicates an operator; building one space from another
        // (room minus camera as a screen position) is a named conversion.
        if (!SymbolEqualityComparer.Default.Equals(ReceiverType(first.LeftOperand), type) ||
            !SymbolEqualityComparer.Default.Equals(ReceiverType(first.RightOperand), type))
            return;
        context.ReportDiagnostic(Diagnostic.Create(ComponentArithmeticRule, creation.Syntax.GetLocation(),
            type.Name, first.OperatorKind == BinaryOperatorKind.Add ? "+" : "-", leftX, rightX));
    }

    /// <summary>The receiver text when <paramref name="operand"/> reads axis <paramref name="axis"/> of a spatial value.</summary>
    private static string? AxisOf(IOperation operand, string axis) =>
        Unwrap(operand) is IPropertyReferenceOperation { Instance: { } instance } property &&
        property.Property.Name == axis && instance.Type is INamedTypeSymbol owner && IsSpatialValueType(owner)
            ? instance.Syntax.ToString()
            : null;

    /// <summary>Identifies the owner type of an axis property read after removing implicit conversions.</summary>
    private static ITypeSymbol? ReceiverType(IOperation operand) =>
        Unwrap(operand) is IPropertyReferenceOperation { Instance: { } instance } ? instance.Type : null;

    /// <summary>Peels conversion operations so axis matching sees the underlying property read.</summary>
    private static IOperation Unwrap(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;
        return operation;
    }

    /// <summary>Recognizes approved spatial value structs with both X and Y members.</summary>
    private static bool IsSpatialValueType(INamedTypeSymbol type) =>
        type.IsValueType && type.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.Name == SpatialComponentsAttributeName) &&
        type.GetMembers("X").Any() && type.GetMembers("Y").Any();

    /// <summary>Checks source-defined members for matching scalar axes and parallel collections.</summary>
    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind is not (TypeKind.Class or TypeKind.Struct) || IsExempt(type))
            return;
        var members = type.GetMembers()
            .Where(member => !member.IsImplicitlyDeclared && member.Locations.Any(location => location.IsInSource))
            .Select(member => (Member: member, Type: ScalarType(member)))
            .Where(entry => entry.Type is not null)
            .ToArray();
        ReportPairs(context, type, members, MemberClumpRule);
        var collections = type.GetMembers()
            .Where(member => !member.IsImplicitlyDeclared && member.Locations.Any(location => location.IsInSource))
            .Select(member => (Member: member, Type: CollectionType(member)))
            .Where(entry => entry.Type is not null)
            .ToArray();
        ReportPairs(context, type, collections, CollectionClumpRule);
    }

    /// <summary>Reports each matching pair once when axis names, types, and static ownership agree.</summary>
    private static void ReportPairs(SymbolAnalysisContext context, INamedTypeSymbol type,
        (ISymbol Member, ITypeSymbol? Type)[] members, DiagnosticDescriptor rule)
    {
        var reported = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach ((ISymbol first, ITypeSymbol? firstType) in members)
        {
            if (reported.Contains(first))
                continue;
            foreach ((string a, string b, string axis) in AxisPairs)
            {
                string? partner = Partner(first.Name, a, b);
                if (partner is null)
                    continue;
                (ISymbol second, ITypeSymbol? _) = members.FirstOrDefault(entry =>
                    entry.Member.Name == partner && entry.Member.IsStatic == first.IsStatic &&
                    SymbolEqualityComparer.Default.Equals(entry.Type, firstType));
                if (second is null || reported.Contains(second))
                    continue;
                reported.Add(first);
                reported.Add(second);
                context.ReportDiagnostic(Diagnostic.Create(rule, first.Locations[0],
                    first.Name, second.Name, type.Name, axis));
                break;
            }
        }
    }

    /// <summary>Checks eligible signatures for adjacent parameters that carry one spatial value.</summary>
    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;
        if (method.IsImplicitlyDeclared || method.IsOverride || method.ExplicitInterfaceImplementations.Length != 0 ||
            method.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet ||
            IsExempt(method) || IsExempt(method.ContainingType))
            return;
        ImmutableArray<IParameterSymbol> parameters = method.Parameters;
        for (int index = 0; index + 1 < parameters.Length; index++)
        {
            IParameterSymbol first = parameters[index], second = parameters[index + 1];
            if (!IsScalar(first.Type) || !SymbolEqualityComparer.Default.Equals(first.Type, second.Type))
                continue;
            foreach ((string a, string b, string axis) in AxisPairs)
            {
                if (!string.Equals(Partner(first.Name, a, b), second.Name, StringComparison.Ordinal))
                    continue;
                context.ReportDiagnostic(Diagnostic.Create(ParameterClumpRule, first.Locations[0],
                    first.Name, second.Name, method.ContainingType.Name + "." + method.Name, axis));
                index++;
                break;
            }
        }
    }

    /// <summary>
    /// The partner name for an axis token at a word boundary: CameraX → CameraY, xPosition → yPosition,
    /// ExtraXDisplacement → ExtraYDisplacement, Width → Height.
    /// </summary>
    private static string? Partner(string name, string first, string second)
    {
        foreach ((string from, string to, bool lower) in new[]
            { (first, second, false), (first.ToLowerInvariant(), second.ToLowerInvariant(), true) })
        {
            int at = 0;
            while ((at = name.IndexOf(from, at, StringComparison.Ordinal)) >= 0)
            {
                // A capitalized token starts a PascalCase word; a lowercase token can only begin a
                // camelCase name (or follow an underscore), so "maxValue" holds no x axis.
                bool startsWord = at == 0 || name[at - 1] == '_' ||
                    (!lower && (char.IsLower(name[at - 1]) || char.IsDigit(name[at - 1])));
                int after = at + from.Length;
                bool endsWord = after == name.Length || char.IsUpper(name[after]) || char.IsDigit(name[after]) || name[after] == '_';
                if (startsWord && endsWord)
                    return name[..at] + to + name[after..];
                at = after;
            }
        }
        return null;
    }

    /// <summary>The collection type of an array or list member whose elements are scalars.</summary>
    private static ITypeSymbol? CollectionType(ISymbol member)
    {
        ITypeSymbol? type = member switch
        {
            IFieldSymbol { IsConst: false } field => field.Type,
            IPropertySymbol { IsIndexer: false } property => property.Type,
            _ => null,
        };
        ITypeSymbol? element = type switch
        {
            IArrayTypeSymbol array => array.ElementType,
            INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } generic when
                generic.Name is "List" or "IList" or "IReadOnlyList" or "ImmutableArray" or "ReadOnlyCollection" or
                    "Span" or "ReadOnlySpan" or "Memory" =>
                generic.TypeArguments[0],
            _ => null,
        };
        return element is not null && IsScalar(element) ? type : null;
    }

    /// <summary>Extracts primitive numeric field or property types eligible for axis pairing.</summary>
    private static ITypeSymbol? ScalarType(ISymbol member) => member switch
    {
        IFieldSymbol { IsConst: false } field when IsScalar(field.Type) => field.Type,
        IPropertySymbol { IsIndexer: false } property when IsScalar(property.Type) => property.Type,
        _ => null,
    };

    /// <summary>Limits spatial clump detection to primitive numeric components.</summary>
    private static bool IsScalar(ITypeSymbol type) => type.SpecialType is
        SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16 or
        SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_Int64 or SpecialType.System_UInt64 or
        SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal;

    /// <summary>Honors a spatial-components annotation on the symbol or an enclosing type.</summary>
    private static bool IsExempt(ISymbol symbol)
    {
        for (ISymbol? current = symbol; current is not null; current = current.ContainingType)
        {
            if (current.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == SpatialComponentsAttributeName))
                return true;
        }
        return false;
    }
}
