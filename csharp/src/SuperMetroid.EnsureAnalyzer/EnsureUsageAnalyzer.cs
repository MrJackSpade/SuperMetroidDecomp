using System.Collections.Immutable;
using System.Linq;
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

    /// <summary>Stores guard rule.</summary>
    private static readonly DiagnosticDescriptor GuardRule = new(
        GuardId,
        "Use Ensure for reusable validation",
        "Use Ensure.{0} for this reusable argument validation",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Keep generic argument validation in the shared Ensure API.");

    /// <summary>Diagnostic identifier for an Ensure call that defeats its caller-expression capture.</summary>
    public const string RedundantArgumentId = "SME6202";

    /// <summary>Stores redundant argument rule.</summary>
    private static readonly DiagnosticDescriptor RedundantArgumentRule = new(
        RedundantArgumentId,
        "Let Ensure capture the value expression",
        "{0}",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Ensure reports the caller's value expression itself. A manually written parameter name " +
            "duplicates or contradicts it, and a numeric cast to fit an overload changes the validated value's type.");

    /// <summary>Gets the diagnostics reported for argument guards and Ensure calls.</summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [GuardRule, RedundantArgumentRule];

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

    /// <summary>analyze invocation.</summary>
    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (InsideEnsure(context))
            return;
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
            return;

        string owner = method.ContainingType.ToDisplayString();
        if (owner == "SuperMetroid.Core.Ensure")
        {
            AnalyzeEnsureCall(context, invocation, method);
            return;
        }
        string? operation = owner switch
        {
            "System.ArgumentNullException" when method.Name == "ThrowIfNull" => "NotNull",
            "System.ArgumentOutOfRangeException" when method.Name == "ThrowIfNegative" => "AtLeastZero",
            _ => null
        };
        if (operation is not null)
            Report(context, invocation.GetLocation(), operation);
    }
    /// <summary>analyze ensure call.</summary>
    private static void AnalyzeEnsureCall(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocation, IMethodSymbol method)
    {
        IMethodSymbol definition = method.ReducedFrom ?? method.OriginalDefinition;
        foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
        {
            IParameterSymbol? parameter = ParameterOf(argument, invocation, definition);
            if (parameter is null)
                continue;
            if (parameter.GetAttributes().Any(attribute =>
                    attribute.AttributeClass?.ToDisplayString() == "System.Runtime.CompilerServices.CallerArgumentExpressionAttribute"))
            {
                context.ReportDiagnostic(Diagnostic.Create(RedundantArgumentRule, argument.GetLocation(),
                    $"Remove the manual parameter name; Ensure.{method.Name} captures the value expression"));
            }
            else if (argument.Expression is CastExpressionSyntax cast &&
                     IsCapturedValue(parameter, definition) &&
                     IsNumeric(context.SemanticModel.GetTypeInfo(cast.Expression).Type))
            {
                context.ReportDiagnostic(Diagnostic.Create(RedundantArgumentRule, cast.GetLocation(),
                    $"Do not cast the value to fit Ensure.{method.Name}; validate it in its own type"));
            }
        }
    }

    /// <summary>parameter of.</summary>
    private static IParameterSymbol? ParameterOf(ArgumentSyntax argument, InvocationExpressionSyntax invocation, IMethodSymbol method)
    {
        if (argument.NameColon is { } nameColon)
            return method.Parameters.FirstOrDefault(parameter => parameter.Name == nameColon.Name.Identifier.ValueText);
        int index = invocation.ArgumentList.Arguments.IndexOf(argument);
        return index < method.Parameters.Length ? method.Parameters[index] : null;
    }

    // The value Ensure validates is the parameter its caller-expression capture names.
    /// <summary>is captured value.</summary>
    private static bool IsCapturedValue(IParameterSymbol parameter, IMethodSymbol method) =>
        method.Parameters.Any(candidate => candidate.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.ToDisplayString() == "System.Runtime.CompilerServices.CallerArgumentExpressionAttribute" &&
            attribute.ConstructorArguments.Length == 1 &&
            attribute.ConstructorArguments[0].Value as string == parameter.Name));

    /// <summary>analyze if.</summary>
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

    /// <summary>analyze coalesce.</summary>
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

    /// <summary>guard operation.</summary>
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

    /// <summary>try outside inclusive range.</summary>
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

    /// <summary>try get thrown argument exception.</summary>
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

    /// <summary>has parameter name.</summary>
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

    /// <summary>parameter name matches.</summary>
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

    /// <summary>is numeric.</summary>
    private static bool IsNumeric(ITypeSymbol? type) => type?.SpecialType is
        SpecialType.System_SByte or SpecialType.System_Byte or
        SpecialType.System_Int16 or SpecialType.System_UInt16 or
        SpecialType.System_Int32 or SpecialType.System_UInt32 or
        SpecialType.System_Int64 or SpecialType.System_UInt64 or
        SpecialType.System_Single or SpecialType.System_Double or
        SpecialType.System_Decimal ||
        type?.ToDisplayString() is "System.Half" or "System.IntPtr" or "System.UIntPtr" or
            "System.Int128" or "System.UInt128" or "System.Numerics.BigInteger";

    /// <summary>is api argument.</summary>
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

    /// <summary>is zero.</summary>
    private static bool IsZero(ExpressionSyntax expression) =>
        expression is LiteralExpressionSyntax literal &&
        literal.Token.ValueText is "0" or "0.0";

    /// <summary>is null.</summary>
    private static bool IsNull(ExpressionSyntax expression) =>
        expression is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.NullLiteralExpression);

    /// <summary>inside ensure.</summary>
    private static bool InsideEnsure(SyntaxNodeAnalysisContext context) =>
        context.ContainingSymbol?.ContainingType?.ToDisplayString() == "SuperMetroid.Core.Ensure";

    /// <summary>report.</summary>
    private static void Report(SyntaxNodeAnalysisContext context, Location location, string operation) =>
        context.ReportDiagnostic(Diagnostic.Create(GuardRule, location, operation));
}
