using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.CartridgeEncoding;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// An identity's backing value is arbitrary and nothing reads it (SME6450, #1418): in <c>SuperMetroid.Core</c>, no code
/// outside a type marked <c>[SnesEncodingOwner]</c> converts an enum to an integral type or an integral value to an enum
/// (an explicit cast, <c>Enum.ToObject</c> or a <c>Convert</c> call), or combines an enum-typed value with an integral
/// literal in arithmetic or a comparison. Each such read is a translation that belongs in a codec, a dispatch that
/// belongs to a behaviour reference, or a table index that belongs to a typed lookup by identity.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnumValueReadAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Core reads an enum's numeric value.</summary>
    public const string EnumValueReadId = "SME6450";

    private static readonly DiagnosticDescriptor Rule = new(
        EnumValueReadId,
        "Core reads an enum's numeric value",
        "{0} reads the numeric value of '{1}'; translate it in an encoding codec, dispatch through a behaviour reference, or look it up by identity",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Identities are semantic names whose backing values are arbitrary and unread. A cast between an enum " +
            "and an integral type, or an enum value combined with an integral literal, reads that value: a translation " +
            "moves to the codec that owns the encoding (marked [SnesEncodingOwner]), a dispatch becomes a behaviour " +
            "reference or capability, and a table index becomes a typed lookup by identity (#1418).");

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
            start.RegisterOperationAction(AnalyzeConversion, OperationKind.Conversion);
            start.RegisterOperationAction(AnalyzeBinary, OperationKind.Binary);
            start.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
        });
    }

    /// <summary>Reports an explicit cast between an enum and an integral type.</summary>
    private static void AnalyzeConversion(OperationAnalysisContext context)
    {
        var conversion = (IConversionOperation)context.Operation;
        if (conversion.IsImplicit || conversion.Operand.Type is not { } from || conversion.Type is not { } to)
            return;
        ITypeSymbol? enumType = IsEnum(from) && IsIntegral(to) ? from : IsIntegral(from) && IsEnum(to) ? to : null;
        if (enumType is not null)
            Report(context, conversion.Syntax, $"The cast '{conversion.Syntax}'", enumType);
    }

    /// <summary>Reports arithmetic or a comparison between an enum-typed value and an integral literal.</summary>
    private static void AnalyzeBinary(OperationAnalysisContext context)
    {
        var binary = (IBinaryOperation)context.Operation;
        ITypeSymbol? enumType =
            EnumOperand(binary.LeftOperand) is { } left && IsIntegralLiteral(binary.RightOperand) ? left :
            EnumOperand(binary.RightOperand) is { } right && IsIntegralLiteral(binary.LeftOperand) ? right : null;
        if (enumType is not null)
            Report(context, binary.Syntax, $"'{binary.Syntax}'", enumType);
    }

    /// <summary>Reports <c>Enum.ToObject</c> and a <c>Convert</c> call given an enum value.</summary>
    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod;
        string? owner = method.ContainingType?.ToDisplayString();
        if (owner == "System.Enum" && method.Name == "ToObject" && invocation.Arguments.Length > 0 &&
            invocation.Arguments[0].Value is ITypeOfOperation { TypeOperand: { } converted })
        {
            Report(context, invocation.Syntax, $"'{invocation.Syntax}'", converted);
            return;
        }
        if (owner == "System.Convert" && method.Name.StartsWith("To", StringComparison.Ordinal) && invocation.Arguments.Length > 0 &&
            Unconverted(invocation.Arguments[0].Value).Type is { } argument && IsEnum(argument))
        {
            Report(context, invocation.Syntax, $"'{invocation.Syntax}'", argument);
        }
    }

    /// <summary>Reports unless the site lies in a type that owns an encoding.</summary>
    private static void Report(OperationAnalysisContext context, SyntaxNode site, string what, ITypeSymbol enumType)
    {
        for (INamedTypeSymbol? type = context.ContainingSymbol.ContainingType; type is not null; type = type.ContainingType)
        {
            if (type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == CartridgeEncodingScanner.EncodingOwnerAttribute))
                return;
        }
        context.ReportDiagnostic(Diagnostic.Create(Rule, site.GetLocation(), what, enumType.Name));
    }

    /// <summary>The enum type of an operand, looking through implicit conversions, or null.</summary>
    private static ITypeSymbol? EnumOperand(IOperation operand) =>
        Unconverted(operand).Type is { } type && IsEnum(type) ? type :
        operand.Type is { } converted && IsEnum(converted) && !IsIntegralLiteral(operand) ? converted : null;

    /// <summary>Whether an operand is an integral literal, looking through implicit conversions.</summary>
    private static bool IsIntegralLiteral(IOperation operand) =>
        Unconverted(operand) is ILiteralOperation { Type: { } type } && IsIntegral(type);

    /// <summary>The operand beneath any implicit conversions.</summary>
    private static IOperation Unconverted(IOperation operand)
    {
        while (operand is IConversionOperation { IsImplicit: true } conversion)
            operand = conversion.Operand;
        return operand;
    }

    /// <summary>Whether a type is an enum.</summary>
    private static bool IsEnum(ITypeSymbol type) => type.TypeKind == TypeKind.Enum;

    /// <summary>Whether a type is a built-in integral type.</summary>
    private static bool IsIntegral(ITypeSymbol type) => type.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte
        or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32
        or SpecialType.System_Int64 or SpecialType.System_UInt64;
}
