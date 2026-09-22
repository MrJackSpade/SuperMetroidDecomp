using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>Finds reusable argument guards that should use the shared Ensure API.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnsureUsageAnalyzer : DiagnosticAnalyzer
{
    public const string GuardId = "SME6201";
    public const string EnumCallId = "SME6202";

    private static readonly DiagnosticDescriptor GuardRule = new(
        GuardId,
        "Use Ensure for reusable validation",
        "Use Ensure.{0} for this reusable argument validation",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Keep generic argument validation in the shared Ensure API.");

    private static readonly DiagnosticDescriptor EnumCallRule = new(
        EnumCallId,
        "Let Ensure capture the enum expression",
        "Call Ensure.IsDefined without a redundant cast or manually supplied parameter name",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Ensure.IsDefined captures the complete caller expression and returns the original enum type.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [GuardRule, EnumCallRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            // A few standalone research tools link only selected Core source files.
            // They cannot act on an Ensure suggestion until the shared API is available.
            if (start.Compilation.GetTypeByMetadataName("SuperMetroid.Core.Ensure") is null)
                return;
            start.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
            start.RegisterSyntaxNodeAction(AnalyzeIf, SyntaxKind.IfStatement);
            start.RegisterSyntaxNodeAction(AnalyzeCoalesce, SyntaxKind.CoalesceExpression);
            start.RegisterSyntaxNodeAction(AnalyzeConditional, SyntaxKind.ConditionalExpression);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (InsideEnsure(context))
            return;
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
            return;

        string owner = method.ContainingType.ToDisplayString();
        string? operation = owner switch
        {
            "System.ArgumentNullException" when method.Name == "ThrowIfNull" => "NotNull",
            "System.ArgumentException" when method.Name == "ThrowIfNullOrEmpty" => "NotNullOrEmpty",
            "System.ArgumentException" when method.Name == "ThrowIfNullOrWhiteSpace" => "NotNullOrWhiteSpace",
            "System.ArgumentOutOfRangeException" => method.Name switch
            {
                "ThrowIfNegativeOrZero" => "GreaterThanZero",
                "ThrowIfNegative" => "AtLeastZero",
                "ThrowIfLessThanOrEqual" => "GreaterThan",
                "ThrowIfLessThan" => "AtLeast",
                "ThrowIfGreaterThan" => "AtMost",
                "ThrowIfGreaterThanOrEqual" => "LessThan",
                "ThrowIfEqual" => "NotEqual",
                "ThrowIfNotEqual" => "Equal",
                _ => null
            },
            _ => null
        };
        if (operation is not null)
            Report(context, invocation.GetLocation(), operation);

        if (owner != "SuperMetroid.Core.Ensure" || method.Name != "IsDefined")
            return;
        if (invocation.ArgumentList.Arguments.Count > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                EnumCallRule, invocation.ArgumentList.Arguments[1].GetLocation()));
            return;
        }
        ExpressionSyntax argument = invocation.ArgumentList.Arguments[0].Expression;
        if (argument is CastExpressionSyntax cast &&
            SymbolEqualityComparer.Default.Equals(
                context.SemanticModel.GetTypeInfo(cast).Type,
                context.SemanticModel.GetTypeInfo(cast.Expression).Type))
        {
            context.ReportDiagnostic(Diagnostic.Create(EnumCallRule, cast.GetLocation()));
        }
    }

    private static void AnalyzeIf(SyntaxNodeAnalysisContext context)
    {
        if (InsideEnsure(context))
            return;
        var statement = (IfStatementSyntax)context.Node;
        if (statement.Else is not null || !TryGetThrownArgumentException(statement.Statement, context.SemanticModel,
                out string exceptionType, out ArgumentListSyntax arguments))
            return;

        if (TryUndefinedEnumGuard(statement.Condition, context.SemanticModel, out ExpressionSyntax enumValue))
        {
            if (exceptionType == "System.ArgumentOutOfRangeException")
                Report(context, statement.Condition.GetLocation(), "IsDefined");
            return;
        }

        if (!HasParameterName(arguments, exceptionType))
            return;

        string? operation = GuardOperation(statement.Condition, context.SemanticModel, out ExpressionSyntax? value);
        if (operation is null || value is null ||
            (exceptionType == "System.ArgumentException" && operation is not ("LengthEqual" or "CountEqual")) ||
            !IsApiArgument(value, context) ||
            !ParameterNameMatches(arguments, value, exceptionType))
            return;
        Report(context, statement.Condition.GetLocation(), operation);
    }

    private static void AnalyzeCoalesce(SyntaxNodeAnalysisContext context)
    {
        if (InsideEnsure(context))
            return;
        var expression = (BinaryExpressionSyntax)context.Node;
        if (expression.Right is not ThrowExpressionSyntax thrown ||
            thrown.Expression is not ObjectCreationExpressionSyntax creation ||
            context.SemanticModel.GetTypeInfo(creation).Type?.ToDisplayString() != "System.ArgumentNullException" ||
            creation.ArgumentList is null ||
            !HasParameterName(creation.ArgumentList, "System.ArgumentNullException") ||
            !ParameterNameMatches(creation.ArgumentList, expression.Left, "System.ArgumentNullException"))
            return;
        Report(context, expression.GetLocation(), "NotNull");
    }

    private static void AnalyzeConditional(SyntaxNodeAnalysisContext context)
    {
        if (InsideEnsure(context))
            return;
        var conditional = (ConditionalExpressionSyntax)context.Node;
        if (conditional.WhenFalse is not ThrowExpressionSyntax thrown ||
            thrown.Expression is not ObjectCreationExpressionSyntax creation ||
            context.SemanticModel.GetTypeInfo(creation).Type?.ToDisplayString() != "System.ArgumentOutOfRangeException" ||
            !TryEnumInvocation(conditional.Condition, context.SemanticModel, out ExpressionSyntax value) ||
            conditional.WhenTrue.ToString() != value.ToString())
            return;
        Report(context, conditional.GetLocation(), "IsDefined");
    }

    private static string? GuardOperation(
        ExpressionSyntax condition,
        SemanticModel model,
        out ExpressionSyntax? value)
    {
        value = null;
        if (condition is ParenthesizedExpressionSyntax parenthesized)
            return GuardOperation(parenthesized.Expression, model, out value);

        if (condition is IsPatternExpressionSyntax pattern &&
            pattern.Pattern is UnaryPatternSyntax unary && unary.IsKind(SyntaxKind.NotPattern) &&
            unary.Pattern is ParenthesizedPatternSyntax { Pattern: BinaryPatternSyntax binaryPattern } &&
            binaryPattern.IsKind(SyntaxKind.OrPattern) &&
            binaryPattern.Left is ConstantPatternSyntax &&
            binaryPattern.Right is ConstantPatternSyntax)
        {
            value = pattern.Expression;
            return "OneOf";
        }

        if (condition is IsPatternExpressionSyntax nullPattern &&
            nullPattern.Pattern is ConstantPatternSyntax { Expression: LiteralExpressionSyntax literal } &&
            literal.IsKind(SyntaxKind.NullLiteralExpression))
        {
            value = nullPattern.Expression;
            return "NotNull";
        }

        if (condition is not BinaryExpressionSyntax binary)
            return null;

        if (binary.IsKind(SyntaxKind.LogicalOrExpression) &&
            TryOutsideInclusiveRange(binary, out value))
            return "BetweenInclusive";

        if (binary.IsKind(SyntaxKind.EqualsExpression) ||
            binary.IsKind(SyntaxKind.NotEqualsExpression))
        {
            if (IsNull(binary.Right))
            {
                value = binary.Left;
                return binary.IsKind(SyntaxKind.EqualsExpression) ? "NotNull" : null;
            }
            if (IsNull(binary.Left))
            {
                value = binary.Right;
                return binary.IsKind(SyntaxKind.EqualsExpression) ? "NotNull" : null;
            }
            if (binary.IsKind(SyntaxKind.NotEqualsExpression) &&
                binary.Left is MemberAccessExpressionSyntax member &&
                member.Name.Identifier.ValueText is "Length" or "Count")
            {
                value = member.Expression;
                return member.Name.Identifier.ValueText == "Length" ? "LengthEqual" : "CountEqual";
            }
            value = binary.Left;
            return binary.IsKind(SyntaxKind.NotEqualsExpression) ? "Equal" : "NotEqual";
        }

        if (!IsNumeric(model.GetTypeInfo(binary.Left).Type))
            return null;
        value = binary.Left;
        if (IsZero(binary.Right))
        {
            return binary.Kind() switch
            {
                SyntaxKind.LessThanOrEqualExpression => "GreaterThanZero",
                SyntaxKind.LessThanExpression => "AtLeastZero",
                SyntaxKind.GreaterThanOrEqualExpression => "LessThanZero",
                SyntaxKind.GreaterThanExpression => "AtMostZero",
                _ => null
            };
        }
        return binary.Kind() switch
        {
            SyntaxKind.LessThanExpression => "AtLeast",
            SyntaxKind.LessThanOrEqualExpression => "GreaterThan",
            SyntaxKind.GreaterThanExpression => "AtMost",
            SyntaxKind.GreaterThanOrEqualExpression => "LessThan",
            _ => null
        };
    }

    private static bool TryOutsideInclusiveRange(
        BinaryExpressionSyntax expression,
        out ExpressionSyntax? value)
    {
        value = null;
        if (expression.Left is not BinaryExpressionSyntax low ||
            expression.Right is not BinaryExpressionSyntax high ||
            !low.IsKind(SyntaxKind.LessThanExpression) ||
            !high.IsKind(SyntaxKind.GreaterThanExpression) ||
            low.Left.ToString() != high.Left.ToString())
            return false;
        value = low.Left;
        return true;
    }

    private static bool TryUndefinedEnumGuard(
        ExpressionSyntax condition, SemanticModel model, out ExpressionSyntax value)
    {
        value = null!;
        if (condition is not PrefixUnaryExpressionSyntax negated ||
            !negated.IsKind(SyntaxKind.LogicalNotExpression))
            return false;
        return TryEnumInvocation(negated.Operand, model, out value);
    }

    private static bool TryEnumInvocation(
        ExpressionSyntax expression, SemanticModel model, out ExpressionSyntax value)
    {
        value = null!;
        if (expression is not InvocationExpressionSyntax invocation ||
            model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method ||
            method.ContainingType.ToDisplayString() != "System.Enum" ||
            method.Name != "IsDefined")
            return false;
        var args = invocation.ArgumentList.Arguments;
        if (args.Count is not (1 or 2))
            return false;
        value = args[args.Count - 1].Expression;
        if (model.GetTypeInfo(value).Type is not INamedTypeSymbol enumType ||
            enumType.TypeKind != TypeKind.Enum ||
            enumType.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == "System.FlagsAttribute"))
            return false;
        if (args.Count == 2 && args[0].Expression is not TypeOfExpressionSyntax typeOf)
            return false;
        if (args.Count == 2 &&
            !SymbolEqualityComparer.Default.Equals(model.GetTypeInfo(((TypeOfExpressionSyntax)args[0].Expression).Type).Type, enumType))
            return false;
        return true;
    }

    private static bool TryGetThrownArgumentException(
        StatementSyntax statement, SemanticModel model,
        out string exceptionType, out ArgumentListSyntax arguments)
    {
        exceptionType = "";
        arguments = null!;
        if (statement is BlockSyntax block)
        {
            if (block.Statements.Count != 1)
                return false;
            statement = block.Statements[0];
        }
        if (statement is not ThrowStatementSyntax { Expression: ObjectCreationExpressionSyntax creation } ||
            creation.ArgumentList is null)
            return false;
        exceptionType = model.GetTypeInfo(creation).Type?.ToDisplayString() ?? "";
        if (exceptionType is not ("System.ArgumentException" or
            "System.ArgumentNullException" or "System.ArgumentOutOfRangeException"))
            return false;
        arguments = creation.ArgumentList;
        return true;
    }

    private static bool HasParameterName(ArgumentListSyntax arguments, string exceptionType) =>
        exceptionType switch
        {
            "System.ArgumentNullException" or "System.ArgumentOutOfRangeException" =>
                arguments.Arguments.Count == 1 &&
                arguments.Arguments[0].Expression is InvocationExpressionSyntax,
            "System.ArgumentException" =>
                arguments.Arguments.Count == 2 &&
                arguments.Arguments[1].Expression is InvocationExpressionSyntax,
            _ => false
        };

    private static bool ParameterNameMatches(
        ArgumentListSyntax arguments, ExpressionSyntax value, string exceptionType)
    {
        int index = exceptionType == "System.ArgumentException" ? 1 : 0;
        if (arguments.Arguments.Count <= index ||
            arguments.Arguments[index].Expression is not InvocationExpressionSyntax nameOf ||
            nameOf.Expression.ToString() != "nameof" ||
            nameOf.ArgumentList.Arguments.Count != 1)
            return false;
        return nameOf.ArgumentList.Arguments[0].Expression.ToString() == value.ToString();
    }

    private static bool IsNumeric(ITypeSymbol? type) => type?.SpecialType is
        SpecialType.System_SByte or SpecialType.System_Byte or
        SpecialType.System_Int16 or SpecialType.System_UInt16 or
        SpecialType.System_Int32 or SpecialType.System_UInt32 or
        SpecialType.System_Int64 or SpecialType.System_UInt64 or
        SpecialType.System_Single or SpecialType.System_Double or
        SpecialType.System_Decimal ||
        type?.ToDisplayString() is "System.Half" or "System.IntPtr" or "System.UIntPtr" or
            "System.Int128" or "System.UInt128" or "System.Numerics.BigInteger";

    private static bool IsApiArgument(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
    {
        // A private cartridge routine often validates its own transient state with the
        // same comparison syntax. Only suggest a generic API for a visible contract.
        if (context.ContainingSymbol is not IMethodSymbol method ||
            method.DeclaredAccessibility == Accessibility.Private)
            return false;
        while (expression is MemberAccessExpressionSyntax member)
            expression = member.Expression;
        if (expression is not IdentifierNameSyntax identifier)
            return false;
        return context.SemanticModel.GetSymbolInfo(identifier).Symbol is IParameterSymbol;
    }

    private static bool IsZero(ExpressionSyntax expression) =>
        expression is LiteralExpressionSyntax literal &&
        literal.Token.ValueText is "0" or "0.0";

    private static bool IsNull(ExpressionSyntax expression) =>
        expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NullLiteralExpression);

    private static bool InsideEnsure(SyntaxNodeAnalysisContext context) =>
        context.ContainingSymbol?.ContainingType?.ToDisplayString() == "SuperMetroid.Core.Ensure";

    private static void Report(SyntaxNodeAnalysisContext context, Location location, string operation) =>
        context.ReportDiagnostic(Diagnostic.Create(GuardRule, location, operation));
}
