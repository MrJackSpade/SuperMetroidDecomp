using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// An identity is named, never spelled by its backing value (SME6458, #2187): in <c>SuperMetroid.Core</c>, no constant
/// zero converts implicitly to an enum, as C# lets the literal <c>0</c> do for every enum. An argument, assignment,
/// initializer, return, coalescing default, case label or pattern written <c>0</c> where an enum is expected names the
/// member whose value happens to be zero (<c>HudItem.Beam</c>, <c>ControllerButton.None</c>) without saying which, and
/// silently changes meaning when that member does. A comparison or arithmetic with the literal is SME6450's read.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LiteralZeroEnumAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Core passes the literal zero where an enum identity is expected.</summary>
    public const string LiteralZeroEnumId = "SME6458";

    private static readonly DiagnosticDescriptor Rule = new(
        LiteralZeroEnumId,
        "Core names an enum identity by the literal zero",
        "'{0}' converts the literal zero to '{1}'; name the member it means",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "C# converts a constant zero to any enum implicitly, so a 0 passed, assigned, returned or matched where " +
            "an enum is expected names whichever member is backed by zero without saying which. Core names the identity " +
            "(HudItem.Beam, ReserveTankMode.None, ControllerButton.None) so the site reads as the game's meaning and " +
            "follows the member if its backing value changes (#2187).");

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
            start.RegisterOperationAction(AnalyzeConversion, OperationKind.Conversion);
        });
    }

    /// <summary>Reports an implicit conversion of an integral constant zero to an enum or a nullable enum.</summary>
    private static void AnalyzeConversion(OperationAnalysisContext context)
    {
        var conversion = (IConversionOperation)context.Operation;
        if (!conversion.Conversion.IsImplicit || conversion.Type is not { } to || EnumOf(to) is not { } enumType)
            return;
        IOperation operand = conversion.Operand;
        // An argument the call leaves to its parameter's default is the compiler's constant, not source: the parameter's
        // own default is read where it is declared.
        if (operand.IsImplicit || conversion.Parent is IArgumentOperation { ArgumentKind: ArgumentKind.DefaultValue })
            return;
        if (operand.Type is not { } from || !IsIntegral(from) || !IsZero(operand.ConstantValue))
            return;
        // A literal combined with an enum in arithmetic or a comparison is SME6450's read of the enum's value.
        if (conversion.Parent is IBinaryOperation binary &&
            Unconverted(ReferenceEquals(binary.LeftOperand, conversion) ? binary.RightOperand : binary.LeftOperand).Type is
                { TypeKind: TypeKind.Enum })
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, operand.Syntax.GetLocation(), ContainingSite(conversion), enumType.Name));
    }

    /// <summary>The source the conversion stands in: its argument, assignment or initializer, else the literal itself.</summary>
    private static string ContainingSite(IConversionOperation conversion) => conversion.Parent switch
    {
        IArgumentOperation argument => argument.Syntax.ToString(),
        IAssignmentOperation assignment => assignment.Syntax.ToString(),
        ICoalesceOperation coalesce => coalesce.Syntax.ToString(),
        _ => conversion.Operand.Syntax.ToString(),
    };

    /// <summary>The operand beneath any implicit conversions.</summary>
    private static IOperation Unconverted(IOperation operand)
    {
        while (operand is IConversionOperation { IsImplicit: true } conversion)
            operand = conversion.Operand;
        return operand;
    }

    /// <summary>The enum a type is, or the enum a nullable type wraps, or null.</summary>
    private static ITypeSymbol? EnumOf(ITypeSymbol type) =>
        type.TypeKind == TypeKind.Enum ? type :
        type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable &&
            nullable.TypeArguments[0] is { TypeKind: TypeKind.Enum } wrapped ? wrapped :
        null;

    /// <summary>Whether a constant is an integral zero.</summary>
    private static bool IsZero(Optional<object?> constant) => constant.HasValue && constant.Value switch
    {
        int value => value == 0,
        uint value => value == 0,
        long value => value == 0,
        ulong value => value == 0,
        short value => value == 0,
        ushort value => value == 0,
        byte value => value == 0,
        sbyte value => value == 0,
        _ => false,
    };

    /// <summary>Whether a type is a built-in integral type.</summary>
    private static bool IsIntegral(ITypeSymbol type) => type.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte
        or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32
        or SpecialType.System_Int64 or SpecialType.System_UInt64;
}
