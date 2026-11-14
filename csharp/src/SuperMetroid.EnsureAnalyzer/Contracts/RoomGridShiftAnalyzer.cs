using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports a room pixel coordinate shifted right by four bits outside the spatial types (SME6457, #1753): the shift is the
/// machine idiom for the room grid's pixel-to-block conversion, which <c>RoomGridConversions.PixelToBlock</c> and
/// <c>ToRoomBlock</c> name.
/// </summary>
/// <remarks>
/// The rule reads the shifted value's type, so it reports only what is a room pixel coordinate by construction: an axis
/// of a <c>Position16&lt;RoomPixels&gt;</c>, or the whole pixel of an axis of a <c>FixedPosition&lt;RoomPixels&gt;</c>.
/// Room pixels shifted four bits are always counted in 16-pixel blocks, so the rule has no false positive; a coordinate
/// already copied into a plain integer is out of its reach. The spatial namespace owns the conversion and is exempt.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RoomGridShiftAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A room pixel coordinate converted to its block by a raw four-bit shift.</summary>
    public const string RoomGridShiftId = "SME6457";

    /// <summary>The namespace of the spatial types, which owns the room grid's conversions.</summary>
    private const string SpatialNamespace = "SuperMetroid.Core.Spatial";

    /// <summary>The bits a 16-pixel block's offset takes.</summary>
    private const int BlockPixelBits = 4;

    private static readonly DiagnosticDescriptor Rule = new(
        RoomGridShiftId,
        "Room pixel shifted to its block",
        "'{0}' converts a room pixel to its block by a raw shift; use RoomGridConversions.PixelToBlock or ToRoomBlock",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The room grid's pixel-to-block conversion is named in the spatial types; Core does not spell it as " +
            "the machine's four-bit shift (#1753).");

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
            start.RegisterOperationAction(Analyze, OperationKind.Binary);
        });
    }

    /// <summary>Reports a four-bit right shift of a room pixel coordinate outside the spatial namespace.</summary>
    private static void Analyze(OperationAnalysisContext context)
    {
        var shift = (IBinaryOperation)context.Operation;
        if (shift.OperatorKind is not (BinaryOperatorKind.RightShift or BinaryOperatorKind.UnsignedRightShift) ||
            shift.RightOperand.ConstantValue is not { HasValue: true, Value: int bits } || bits != BlockPixelBits)
            return;
        if (!IsRoomPixelCoordinate(Unwrap(shift.LeftOperand)))
            return;
        if (context.ContainingSymbol.ContainingNamespace?.ToDisplayString() is { } name &&
            (name == SpatialNamespace || name.StartsWith(SpatialNamespace + ".", StringComparison.Ordinal)))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, shift.Syntax.GetLocation(), shift.Syntax.ToString()));
    }

    /// <summary>The operand beneath its implicit conversions and parentheses.</summary>
    private static IOperation Unwrap(IOperation operation)
    {
        while (true)
        {
            switch (operation)
            {
                case IConversionOperation { IsImplicit: true } conversion:
                    operation = conversion.Operand;
                    continue;
                case IParenthesizedOperation parenthesized:
                    operation = parenthesized.Operand;
                    continue;
                default:
                    return operation;
            }
        }
    }

    /// <summary>
    /// Whether <paramref name="operation"/> reads an axis of a room pixel position, or the whole pixel of an axis of a
    /// fixed-point room pixel position.
    /// </summary>
    private static bool IsRoomPixelCoordinate(IOperation operation) => operation switch
    {
        IPropertyReferenceOperation { Property.Name: "X" or "Y" } axis => IsRoomPixelType(axis.Property.ContainingType, "Position16"),
        IPropertyReferenceOperation { Property.Name: "Whole", Property.ContainingType.Name: "FixedCoordinate", Instance: { } instance } =>
            Unwrap(instance) is IPropertyReferenceOperation { Property.Name: "X" or "Y" } fixedAxis &&
            IsRoomPixelType(fixedAxis.Property.ContainingType, "FixedPosition"),
        _ => false,
    };

    /// <summary>Whether <paramref name="type"/> is the spatial generic <paramref name="name"/> over room pixels.</summary>
    private static bool IsRoomPixelType(INamedTypeSymbol type, string name) =>
        type.Name == name && type.TypeArguments.Length == 1 &&
        type.TypeArguments[0] is INamedTypeSymbol { Name: "RoomPixels" } space &&
        type.ContainingNamespace?.ToDisplayString() == SpatialNamespace &&
        space.ContainingNamespace?.ToDisplayString() == SpatialNamespace;
}
