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

    /// <summary>Describes reusable guards that can delegate to the shared Ensure API.</summary>
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

    /// <summary>Finds direct framework guard calls that have shared Ensure equivalents.</summary>
    /// <param name="context">The invocation analysis context.</param>
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
    /// <summary>Finds simple throwing conditionals that express reusable argument guards.</summary>
    /// <param name="context">The if-statement analysis context.</param>
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

    /// <summary>Finds null-coalescing throws that can use the shared null guard.</summary>
    /// <param name="context">The coalesce-expression analysis context.</param>
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

    /// <summary>Maps a supported guard condition to its Ensure operation and guarded value.</summary>
    /// <param name="condition">The condition to classify.</param>
    /// <param name="model">The semantic model used to identify numeric operands.</param>
    /// <param name="value">Receives the expression whose value is guarded.</param>
    /// <returns>The matching Ensure operation, or <see langword="null"/> when unsupported.</returns>
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

    /// <summary>Recognizes a value below a lower bound or above an upper bound.</summary>
    /// <param name="expression">The disjunction to inspect.</param>
    /// <param name="value">Receives the repeated bounded expression.</param>
    /// <returns><see langword="true"/> when the disjunction is an inclusive-range rejection.</returns>
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

    /// <summary>Extracts a single directly thrown argument exception from a statement.</summary>
    /// <param name="statement">The statement or one-statement block to inspect.</param>
    /// <param name="model">The semantic model used to classify the exception.</param>
    /// <param name="exceptionType">Receives the fully qualified exception type.</param>
    /// <param name="arguments">Receives the exception constructor arguments.</param>
    /// <returns><see langword="true"/> when the statement throws a supported argument exception.</returns>
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

    /// <summary>Checks that a supported exception call supplies a <c>nameof</c> argument.</summary>
    /// <param name="arguments">The constructor arguments to inspect.</param>
    /// <param name="exceptionType">The fully qualified exception type.</param>
    /// <returns><see langword="true"/> when the expected argument position contains an invocation.</returns>
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

    /// <summary>Checks that an exception's <c>nameof</c> argument identifies the guarded expression.</summary>
    /// <param name="arguments">The exception constructor arguments.</param>
    /// <param name="value">The expression guarded by the condition.</param>
    /// <param name="exceptionType">The fully qualified exception type.</param>
    /// <returns><see langword="true"/> when both expressions name the same value.</returns>
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

    /// <summary>Reports whether a type participates in supported numeric lower-bound checks.</summary>
    /// <param name="type">The operand type.</param>
    /// <returns><see langword="true"/> for a supported numeric type.</returns>
    private static bool IsNumeric(ITypeSymbol? type) => type?.SpecialType is
        SpecialType.System_SByte or SpecialType.System_Byte or
        SpecialType.System_Int16 or SpecialType.System_UInt16 or
        SpecialType.System_Int32 or SpecialType.System_UInt32 or
        SpecialType.System_Int64 or SpecialType.System_UInt64 or
        SpecialType.System_Single or SpecialType.System_Double or
        SpecialType.System_Decimal ||
        type?.ToDisplayString() is "System.Half" or "System.IntPtr" or "System.UIntPtr" or
            "System.Int128" or "System.UInt128" or "System.Numerics.BigInteger";

    /// <summary>Limits suggestions to parameters on non-private API methods.</summary>
    /// <param name="expression">The candidate guarded expression.</param>
    /// <param name="context">The syntax analysis context.</param>
    /// <returns><see langword="true"/> when the expression resolves to a method parameter.</returns>
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

    /// <summary>Checks whether an expression is a supported literal zero.</summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <returns><see langword="true"/> for integer or decimal zero literals.</returns>
    private static bool IsZero(ExpressionSyntax expression) =>
        expression is LiteralExpressionSyntax literal &&
        literal.Token.ValueText is "0" or "0.0";

    /// <summary>Checks whether an expression is the null literal.</summary>
    /// <param name="expression">The expression to inspect.</param>
    /// <returns><see langword="true"/> for a null literal.</returns>
    private static bool IsNull(ExpressionSyntax expression) =>
        expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NullLiteralExpression);

    /// <summary>Checks whether analysis is currently inside the shared Ensure implementation.</summary>
    /// <param name="context">The syntax analysis context.</param>
    /// <returns><see langword="true"/> when the containing type is the Ensure API.</returns>
    private static bool InsideEnsure(SyntaxNodeAnalysisContext context) =>
        context.ContainingSymbol?.ContainingType?.ToDisplayString() == "SuperMetroid.Core.Ensure";

    /// <summary>Reports a suggestion to use the named shared Ensure operation.</summary>
    /// <param name="context">The syntax analysis context.</param>
    /// <param name="location">The source location to highlight.</param>
    /// <param name="operation">The Ensure operation name.</param>
    private static void Report(SyntaxNodeAnalysisContext context, Location location, string operation) =>
        context.ReportDiagnostic(Diagnostic.Create(GuardRule, location, operation));
}
