using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Reports a room-grid operation spelled as a machine bit idiom outside the spatial types (SME6457, #1753, #1786): a room
/// pixel coordinate shifted right by four bits (its block) or three (its half block); masked with 15, 7 or 8 (its offset
/// within a block or half block, or which half of its block it lies in); masked with ~15 or ~7 (its block or half block's
/// first pixel); or-ed with 15, 7 or 8 (its block or half block's last pixel, or the block's centre); xor-ed with 15 or 7
/// (its mirrored offset); and a room block coordinate shifted left by four bits (its first pixel).
/// <c>RoomGridConversions</c> names each of these operations.
/// </summary>
/// <remarks>
/// The rule reads the operand's type, so it reports only what is a room grid coordinate by construction: an axis of a
/// <c>Position16&lt;RoomPixels&gt;</c> or <c>Position16&lt;RoomBlocks&gt;</c>, the whole pixel of an axis of a
/// <c>FixedPosition&lt;RoomPixels&gt;</c>, such a room pixel offset by a length that is not itself a coordinate, or the
/// sixteen-bit wrap of one. A room pixel's low four bits are its place in its 16-pixel block and its fourth bit the half
/// of the block it lies in, so each reported idiom is a grid operation and the rule has no false positive; a coordinate
/// already copied into a plain integer is out of its reach. The spatial namespace owns the conversions and is exempt.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RoomGridIdiomAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A room grid operation spelled as a machine bit idiom.</summary>
    public const string RoomGridIdiomId = "SME6457";

    /// <summary>The namespace of the spatial types, which owns the room grid's conversions.</summary>
    private const string SpatialNamespace = "SuperMetroid.Core.Spatial";

    /// <summary>The bits a 16-pixel block's offset takes.</summary>
    private const int BlockPixelBits = 4;

    /// <summary>The bits an eight-pixel half block's offset takes.</summary>
    private const int HalfBlockPixelBits = 3;

    /// <summary>The masks that read a room pixel's place in its block: its offset in the block or half block, and its half.</summary>
    private static readonly ImmutableHashSet<int> WithinBlockMasks = ImmutableHashSet.Create(15, 7, 8);

    /// <summary>The masks that clear a room pixel's place in its block or half block, as signed and as sixteen-bit constants.</summary>
    private static readonly ImmutableHashSet<int> BlockStartMasks = ImmutableHashSet.Create(~15, ~7, 0xFFF0, 0xFFF8);

    /// <summary>The bits that set a room pixel to its block or half block's last pixel, or to its block's centre.</summary>
    private static readonly ImmutableHashSet<int> BlockEndBits = ImmutableHashSet.Create(15, 7, 8);

    /// <summary>The bits that mirror a room pixel's offset within its block or half block.</summary>
    private static readonly ImmutableHashSet<int> MirrorBits = ImmutableHashSet.Create(15, 7);

    private static readonly DiagnosticDescriptor Rule = new(
        RoomGridIdiomId,
        "Room grid operation spelled as a bit idiom",
        "'{0}' spells a room grid operation as a machine bit idiom; use the named operation in RoomGridConversions",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The room grid's operations (pixel to block, block start, end and centre, offset within a block, the " +
            "half of a block, half-block alignment, block to pixel) are named in the spatial types; Core does not spell " +
            "them as the machine's shifts and masks (#1753, #1786).");

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

    /// <summary>Reports a room grid bit idiom outside the spatial namespace.</summary>
    private static void Analyze(OperationAnalysisContext context)
    {
        var operation = (IBinaryOperation)context.Operation;
        if (!IsGridIdiom(operation))
            return;
        if (context.ContainingSymbol.ContainingNamespace?.ToDisplayString() is { } name &&
            (name == SpatialNamespace || name.StartsWith(SpatialNamespace + ".", StringComparison.Ordinal)))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax.ToString()));
    }

    /// <summary>Whether <paramref name="operation"/> is one of the room grid's bit idioms on a room grid coordinate.</summary>
    private static bool IsGridIdiom(IBinaryOperation operation)
    {
        BinaryOperatorKind kind = operation.OperatorKind;
        if (kind is BinaryOperatorKind.RightShift or BinaryOperatorKind.UnsignedRightShift)
            return Constant(operation.RightOperand) is BlockPixelBits or HalfBlockPixelBits && IsRoomPixelValue(operation.LeftOperand);
        if (kind is BinaryOperatorKind.LeftShift)
            return Constant(operation.RightOperand) is BlockPixelBits && IsRoomBlockCoordinate(Unwrap(operation.LeftOperand));
        if (kind is BinaryOperatorKind.And)
            return MasksRoomPixel(operation, WithinBlockMasks) || MasksRoomPixel(operation, BlockStartMasks);
        if (kind is BinaryOperatorKind.Or)
            return MasksRoomPixel(operation, BlockEndBits);
        return kind is BinaryOperatorKind.ExclusiveOr && MasksRoomPixel(operation, MirrorBits);
    }

    /// <summary>Whether one operand of <paramref name="operation"/> is a constant of <paramref name="bits"/> and the other a room pixel.</summary>
    private static bool MasksRoomPixel(IBinaryOperation operation, ImmutableHashSet<int> bits) =>
        (Constant(operation.RightOperand) is { } right && bits.Contains(right) && IsRoomPixelValue(operation.LeftOperand)) ||
        (Constant(operation.LeftOperand) is { } left && bits.Contains(left) && IsRoomPixelValue(operation.RightOperand));

    /// <summary>The integer constant <paramref name="operation"/> evaluates to, or null when it is not one.</summary>
    private static int? Constant(IOperation operation) => operation.ConstantValue switch
    {
        { HasValue: true, Value: int value } => value,
        { HasValue: true, Value: ushort value } => value,
        { HasValue: true, Value: short value } => value,
        _ => null,
    };

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
    /// Whether <paramref name="operation"/> is a room pixel coordinate: an axis of a room pixel position, the whole pixel
    /// of an axis of a fixed-point one, such a coordinate plus or minus a value that is not a coordinate, or the
    /// sixteen-bit wrap of one.
    /// </summary>
    private static bool IsRoomPixelValue(IOperation operation)
    {
        IOperation value = Unwrap(operation);
        return value switch
        {
            IBinaryOperation { OperatorKind: BinaryOperatorKind.Add } sum =>
                IsRoomPixelValue(sum.LeftOperand) != IsRoomPixelValue(sum.RightOperand),
            IBinaryOperation { OperatorKind: BinaryOperatorKind.Subtract } difference =>
                IsRoomPixelValue(difference.LeftOperand) && !IsRoomPixelValue(difference.RightOperand),
            IInvocationOperation { TargetMethod: { Name: "Wrap", ContainingType.Name: "CoordinateAxis" } wrap, Arguments.Length: 1 } invocation =>
                wrap.ContainingType.ContainingNamespace?.ToDisplayString() == SpatialNamespace && IsRoomPixelValue(invocation.Arguments[0].Value),
            _ => IsRoomPixelCoordinate(value),
        };
    }

    /// <summary>
    /// Whether <paramref name="operation"/> reads an axis of a room pixel position, or the whole pixel of an axis of a
    /// fixed-point room pixel position.
    /// </summary>
    private static bool IsRoomPixelCoordinate(IOperation operation) => operation switch
    {
        IPropertyReferenceOperation { Property.Name: "X" or "Y" } axis => IsSpatialType(axis.Property.ContainingType, "Position16", "RoomPixels"),
        IPropertyReferenceOperation { Property.Name: "Whole", Property.ContainingType.Name: "FixedCoordinate", Instance: { } instance } =>
            Unwrap(instance) is IPropertyReferenceOperation { Property.Name: "X" or "Y" } fixedAxis &&
            IsSpatialType(fixedAxis.Property.ContainingType, "FixedPosition", "RoomPixels"),
        _ => false,
    };

    /// <summary>Whether <paramref name="operation"/> reads an axis of a room block position.</summary>
    private static bool IsRoomBlockCoordinate(IOperation operation) =>
        operation is IPropertyReferenceOperation { Property.Name: "X" or "Y" } axis &&
        IsSpatialType(axis.Property.ContainingType, "Position16", "RoomBlocks");

    /// <summary>Whether <paramref name="type"/> is the spatial generic <paramref name="name"/> over the space <paramref name="space"/>.</summary>
    private static bool IsSpatialType(INamedTypeSymbol type, string name, string space) =>
        type.Name == name && type.TypeArguments.Length == 1 &&
        type.TypeArguments[0] is INamedTypeSymbol { } argument && argument.Name == space &&
        type.ContainingNamespace?.ToDisplayString() == SpatialNamespace &&
        argument.ContainingNamespace?.ToDisplayString() == SpatialNamespace;
}
