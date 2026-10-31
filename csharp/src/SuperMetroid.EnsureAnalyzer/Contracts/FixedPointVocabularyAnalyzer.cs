using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Keeps fixed-point values, angles and carry flags in their named types (#1326). Outside
/// <c>[EncodingOwner]</c> codec types, Core carries no whole/fraction pair as primitives (SME6351), no angle as a raw
/// byte or word (SME6352), and no processor-carry protocol (SME6353).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FixedPointVocabularyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A whole/fraction pair carried as two primitives or a tuple.</summary>
    public const string SplitFixedPointId = "SME6351";

    /// <summary>An angle, heading or bearing stored or passed as a raw byte or word.</summary>
    public const string RawAngleId = "SME6352";

    /// <summary>A processor-flag protocol: a carry passed by reference, or a carry/overflow member.</summary>
    public const string CarryProtocolId = "SME6353";

    /// <summary>The attribute that marks a type as a codec boundary allowed to hold raw forms.</summary>
    private const string EncodingOwnerAttribute = "EncodingOwnerAttribute";

    /// <summary>Name endings that mark a difference or bit pattern of angles rather than an angle.</summary>
    private static readonly string[] NonAngleSuffixes = ["Step", "Delta", "Mask", "Offset"];

    private static readonly DiagnosticDescriptor SplitFixedPointRule = new(
        SplitFixedPointId,
        "Fixed-point value split into primitives",
        "'{0}' and '{1}' carry one fixed-point value as two primitives; use SignedSixteenSixteen, UnsignedSixteenSixteen or FixedCoordinate",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A whole word and its fraction are one value; Core keeps them in a fixed-point type (#1326).");

    private static readonly DiagnosticDescriptor RawAngleRule = new(
        RawAngleId,
        "Angle held as a raw byte or word",
        "'{0}' holds an angle as {1}; use RotationAngle, ByteAngle or Bearing",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Angles carry their convention in their type: RotationAngle (8.8), ByteAngle (table) or Bearing (#1326).");

    private static readonly DiagnosticDescriptor CarryProtocolRule = new(
        CarryProtocolId,
        "Processor-flag protocol",
        "'{0}' carries a processor flag; state the rule the carry serves in the value's own terms",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A carry is stated as the domain rule it serves, such as a fixed-point addition whose fraction overflow moves the whole part; results say what happened rather than which flag was set (#1326, #1479).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [SplitFixedPointRule, RawAngleRule, CarryProtocolRule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            start.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
            start.RegisterSymbolAction(AnalyzeMember, SymbolKind.Field, SymbolKind.Property);
            start.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
        });
    }

    /// <summary>Reports sibling fields and properties of one type that split a fixed-point value.</summary>
    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (IsExempt(type))
            return;
        ISymbol[] members = type.GetMembers()
            .Where(member => !member.IsImplicitlyDeclared && member is IFieldSymbol or IPropertySymbol && IsPrimitiveWord(TypeOf(member)))
            .ToArray();
        ReportPairs(context, members.Select(member => new NamedLocation(member.Name, member.Locations[0])).ToArray());
    }

    /// <summary>Reports a raw angle field or property, a tuple-typed member pair, and a carry/overflow member.</summary>
    private static void AnalyzeMember(SymbolAnalysisContext context)
    {
        ISymbol member = context.Symbol;
        if (member.IsImplicitlyDeclared || IsExempt(member.ContainingType) || IsBackingField(member))
            return;
        ITypeSymbol? type = TypeOf(member);
        ReportRawAngle(context, member.Name, type, member.Locations[0]);
        ReportTuple(context, type, member.Locations[0]);
        if (member.Name is "Carry" or "Overflow" or "NativeCarry")
            context.ReportDiagnostic(Diagnostic.Create(CarryProtocolRule, member.Locations[0], member.Name));
    }

    /// <summary>Reports raw angle parameters, split parameter pairs, tuple signatures and carry-by-reference parameters.</summary>
    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var method = (IMethodSymbol)context.Symbol;
        if (method.IsImplicitlyDeclared || IsExempt(method.ContainingType) ||
            method.MethodKind is MethodKind.PropertyGet or MethodKind.PropertySet)
        {
            return;
        }
        Location location = method.Locations[0];
        ReportTuple(context, method.ReturnType, location);
        foreach (IParameterSymbol parameter in method.Parameters)
        {
            Location at = parameter.Locations.FirstOrDefault() ?? location;
            ReportRawAngle(context, parameter.Name, parameter.Type, at);
            ReportTuple(context, parameter.Type, at);
            if (parameter.RefKind == RefKind.Ref && parameter.Name.IndexOf("carry", StringComparison.OrdinalIgnoreCase) >= 0 &&
                parameter.Type.SpecialType is SpecialType.System_Boolean or SpecialType.System_Int32)
            {
                context.ReportDiagnostic(Diagnostic.Create(CarryProtocolRule, at, parameter.Name));
            }
        }
        if (method.MethodKind != MethodKind.PropertyGet)
        {
            ReportPairs(context, method.Parameters
                .Where(parameter => IsPrimitiveWord(parameter.Type))
                .Select(parameter => new NamedLocation(parameter.Name, parameter.Locations.FirstOrDefault() ?? location)).ToArray());
        }
    }

    /// <summary>Reports a byte or word whose name says it is an angle.</summary>
    private static void ReportRawAngle(SymbolAnalysisContext context, string name, ITypeSymbol? type, Location location)
    {
        if (type?.SpecialType is not (SpecialType.System_Byte or SpecialType.System_UInt16 or SpecialType.System_SByte or SpecialType.System_Int16))
            return;
        // "Heading" and "Bearing" name an angle only as the whole name or its last word; "HeadingY" is a text position.
        if (name.IndexOf("angle", StringComparison.OrdinalIgnoreCase) < 0 &&
            !name.EndsWith("heading", StringComparison.OrdinalIgnoreCase) &&
            !name.EndsWith("bearing", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        // A step, delta, mask or offset is a difference or bit pattern in table units, not an angle in a convention.
        if (NonAngleSuffixes.Any(suffix => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
            return;
        context.ReportDiagnostic(Diagnostic.Create(RawAngleRule, location, name, type.ToDisplayString()));
    }

    /// <summary>Reports a tuple whose element names split a fixed-point value or carry processor flags.</summary>
    private static void ReportTuple(SymbolAnalysisContext context, ITypeSymbol? type, Location location)
    {
        if (type is not INamedTypeSymbol { IsTupleType: true } tuple)
            return;
        NamedLocation[] elements = tuple.TupleElements
            .Where(element => IsPrimitiveWord(element.Type) && !element.Name.StartsWith("Item", StringComparison.Ordinal))
            .Select(element => new NamedLocation(element.Name, location)).ToArray();
        ReportPairs(context, elements);
        foreach (IFieldSymbol element in tuple.TupleElements)
        {
            if (element.Name is "Carry" or "Overflow" or "NativeCarry")
                context.ReportDiagnostic(Diagnostic.Create(CarryProtocolRule, location, element.Name));
        }
    }

    /// <summary>Reports each whole/fraction pair among the named primitives.</summary>
    private static void ReportPairs(SymbolAnalysisContext context, NamedLocation[] items)
    {
        foreach ((string whole, _) in items)
        {
            foreach ((string fraction, Location location) in items)
            {
                if (!ReferenceEquals(whole, fraction) && whole != fraction && IsFractionOf(fraction, whole))
                    context.ReportDiagnostic(Diagnostic.Create(SplitFixedPointRule, location, whole, fraction));
            }
        }
    }

    /// <summary>
    /// Whether <paramref name="fraction"/> names the fraction of <paramref name="whole"/>: <c>XFraction</c>,
    /// <c>Whole</c>/<c>Fraction</c>, or <c>X</c> with <c>Sub</c> inserted before its last word (<c>Speed</c>/<c>Subspeed</c>,
    /// <c>YSpeed</c>/<c>YSubspeed</c>, <c>Position</c>/<c>Subposition</c>).
    /// </summary>
    internal static bool IsFractionOf(string fraction, string whole)
    {
        if (string.Equals(fraction, whole + "Fraction", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(whole, "Whole", StringComparison.OrdinalIgnoreCase) && string.Equals(fraction, "Fraction", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        for (int split = 0; split < whole.Length; split++)
        {
            if (split > 0 && !char.IsUpper(whole[split]))
                continue;
            string candidate = whole.Substring(0, split) + (split == 0 ? "sub" : "Sub") + char.ToLowerInvariant(whole[split]) + whole.Substring(split + 1);
            if (string.Equals(fraction, candidate, StringComparison.Ordinal) ||
                split == 0 && string.Equals(fraction, "sub" + whole.ToLowerInvariant(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>Whether a type is an integral word or byte.</summary>
    private static bool IsPrimitiveWord(ITypeSymbol? type) => type?.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte or
        SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32;

    /// <summary>The declared type of a field or property.</summary>
    private static ITypeSymbol? TypeOf(ISymbol member) => member switch
    {
        IFieldSymbol field => field.Type,
        IPropertySymbol property => property.Type,
        _ => null,
    };

    /// <summary>Whether a field is an auto-property's compiler-generated backing store.</summary>
    private static bool IsBackingField(ISymbol member) => member is IFieldSymbol { AssociatedSymbol: not null };

    /// <summary>Whether a type or a type containing it is a codec boundary.</summary>
    private static bool IsExempt(INamedTypeSymbol? type)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.ContainingType)
        {
            if (current.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == EncodingOwnerAttribute))
                return true;
        }
        return false;
    }
}
