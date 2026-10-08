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
    /// <summary>Diagnostic identifier for reusable argument validation that should use the shared Ensure API.</summary>
    public const string GuardId = "SME6201";

    private static readonly DiagnosticDescriptor GuardRule = new(
        GuardId,
        "Use Ensure for reusable validation",
        "Use Ensure.{0} for this reusable argument validation",
        "Usage",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Keep generic argument validation in the shared Ensure API.");

    /// <summary>Gets the diagnostic reported for argument guards that can use the shared Ensure API.</summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [GuardRule];

    /// <summary>Registers argument-guard analysis when the compilation provides the shared Ensure API.</summary>
    /// <param name="context">The context used to configure analysis and register syntax callbacks.</param>
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
            "System.ArgumentOutOfRangeException" when method.Name == "ThrowIfNegative" => "AtLeastZero",
            _ => null
        };
        if (operation is not null)
            Report(context, invocation.GetLocation(), operation);
    }
    private static void AnalyzeIf(SyntaxNodeAnalysisContext context)
    {
        if (InsideEnsure(context))
            return;
        var statement = (IfStatementSyntax)context.Node;
        if (statement.Else is not null || !TryGetThrownArgumentException(statement.Statement, context.SemanticModel,
                out string exceptionType, out ArgumentListSyntax arguments))
            return;

        if (!HasParameterName(arguments, exceptionType))
            return;

        string? operation = GuardOperation(statement.Condition, context.SemanticModel, out ExpressionSyntax? value);
        if (operation is null || value is null ||
            (exceptionType == "System.ArgumentException" && operation != "LengthEqual") ||
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

    private static string? GuardOperation(
        ExpressionSyntax condition,
        SemanticModel model,
        out ExpressionSyntax? value)
    {
        value = null;
        if (condition is ParenthesizedExpressionSyntax parenthesized)
            return GuardOperation(parenthesized.Expression, model, out value);

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
                binary.Left is MemberAccessExpressionSyntax { Name.Identifier.ValueText: "Length" } member)
            {
                value = member.Expression;
                return "LengthEqual";
            }
            return null;
        }

        if (!IsNumeric(model.GetTypeInfo(binary.Left).Type) || !IsZero(binary.Right) ||
            !binary.IsKind(SyntaxKind.LessThanExpression))
            return null;
        value = binary.Left;
        return "AtLeastZero";
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
