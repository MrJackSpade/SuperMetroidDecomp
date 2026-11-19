using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Velocities are signed values, never a magnitude plus a sign (SME6471, #2569): in <c>SuperMetroid.Core</c>, outside an
/// <c>[EncodingOwner]</c> type, which implements its own signed arithmetic, and outside a <c>[CompatibilityService]</c>
/// type, which reproduces the original's unsigned-magnitude arithmetic where it is observable, a condition does not choose
/// between a value and its reversal (<c>left ? -speed : speed</c>, <c>v.IsNegative ? -offset : offset</c>,
/// <c>facing == Right ? speed : speed.Negated()</c>), nor between adding and subtracting the same change
/// (<c>v.IsNegative ? v - a : v + a</c>), and no enum names a sign (members <c>Positive</c> and <c>Negative</c>). A
/// quantity is taken toward a direction by the one direction-aware operation (<c>speed.Toward(direction)</c>), a velocity
/// carries its own direction in its sign, and the speed types own the operations that act on a speed away from or toward
/// rest.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MagnitudeAndSignAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Core assembles a velocity from a magnitude and a sign.</summary>
    public const string MagnitudeAndSignId = "SME6471";

    /// <summary>The attribute that marks the single owner of an encoding, which implements its own signed arithmetic.</summary>
    private const string EncodingOwnerAttribute = "EncodingOwnerAttribute";

    /// <summary>The attribute that marks a compatibility service, which may reproduce the original's unsigned arithmetic.</summary>
    private const string CompatibilityServiceAttribute = "CompatibilityServiceAttribute";

    /// <summary>The parameterless methods that reverse a value, as the unary minus does.</summary>
    private static readonly ImmutableHashSet<string> ReversingMethods = ["Negated", "Reversed"];

    private static readonly DiagnosticDescriptor Rule = new(
        MagnitudeAndSignId,
        "Velocity told as a magnitude and a sign",
        "'{0}' {1}",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A speed, distance or turn taken toward a direction is one signed value built by the direction-aware " +
            "operation (Toward), and a velocity's own sign is its direction. A condition that picks a value or its reversal, " +
            "or that picks between adding and subtracting a change, restates a direction as a sign test beside a magnitude; " +
            "an enum of signs does the same. The speed types own the operations that speed a velocity up away from rest or " +
            "drag it toward rest, and a compatibility service owns any unsigned arithmetic the original made observable " +
            "(#2569).");

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
            start.RegisterSyntaxNodeAction(AnalyzeConditional, SyntaxKind.ConditionalExpression);
            start.RegisterSymbolAction(AnalyzeEnum, SymbolKind.NamedType);
        });
    }

    /// <summary>Reports a condition choosing a value or its reversal, or a sum or a difference of the same operands.</summary>
    private static void AnalyzeConditional(SyntaxNodeAnalysisContext context)
    {
        var conditional = (ConditionalExpressionSyntax)context.Node;
        ExpressionSyntax whenTrue = Unwrapped(conditional.WhenTrue);
        ExpressionSyntax whenFalse = Unwrapped(conditional.WhenFalse);
        string? finding = IsReversalOf(whenTrue, whenFalse) || IsReversalOf(whenFalse, whenTrue)
            ? "chooses a value or its reversal by a condition; take the value toward its direction (Toward), or let a velocity's sign carry it"
            : IsSumAndDifference(whenTrue, whenFalse)
                ? "chooses between adding and subtracting the same change by a condition; use the speed type's operation that speeds a velocity up or drags it toward rest, or pass a signed change"
                : null;
        if (finding is null || IsExempt(context.ContainingSymbol))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, conditional.GetLocation(), conditional.ToString(), finding));
    }

    /// <summary>Reports an enum whose members name a sign.</summary>
    private static void AnalyzeEnum(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind != TypeKind.Enum)
            return;
        ImmutableHashSet<string> members = type.GetMembers().OfType<IFieldSymbol>().Select(field => field.Name).ToImmutableHashSet();
        if (!members.Contains("Positive") || !members.Contains("Negative"))
            return;
        foreach (Location location in type.Locations.Where(location => location.IsInSource))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, type.Name,
                "names a sign; hold a direction (HorizontalDirection, VerticalDirection) or a signed value"));
        }
    }

    /// <summary>Whether <paramref name="reversed"/> is <paramref name="value"/> negated or reversed by its own method.</summary>
    private static bool IsReversalOf(ExpressionSyntax reversed, ExpressionSyntax value) => reversed switch
    {
        PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.UnaryMinusExpression } negation =>
            SyntaxFactory.AreEquivalent(Unwrapped(negation.Operand), value),
        InvocationExpressionSyntax { ArgumentList.Arguments.Count: 0, Expression: MemberAccessExpressionSyntax access }
            when ReversingMethods.Contains(access.Name.Identifier.ValueText) =>
            SyntaxFactory.AreEquivalent(Unwrapped(access.Expression), value),
        _ => false,
    };

    /// <summary>Whether the two arms add and subtract the same right operand to and from the same left operand.</summary>
    private static bool IsSumAndDifference(ExpressionSyntax first, ExpressionSyntax second) =>
        first is BinaryExpressionSyntax left && second is BinaryExpressionSyntax right &&
        ((left.IsKind(SyntaxKind.AddExpression) && right.IsKind(SyntaxKind.SubtractExpression)) ||
         (left.IsKind(SyntaxKind.SubtractExpression) && right.IsKind(SyntaxKind.AddExpression))) &&
        SyntaxFactory.AreEquivalent(Unwrapped(left.Left), Unwrapped(right.Left)) &&
        SyntaxFactory.AreEquivalent(Unwrapped(left.Right), Unwrapped(right.Right));

    /// <summary>The expression beneath any parentheses.</summary>
    private static ExpressionSyntax Unwrapped(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax parenthesized)
            expression = parenthesized.Expression;
        return expression;
    }

    /// <summary>Whether code in <paramref name="symbol"/> lies in an encoding owner or a compatibility service.</summary>
    private static bool IsExempt(ISymbol symbol)
    {
        for (INamedTypeSymbol? type = symbol as INamedTypeSymbol ?? symbol.ContainingType; type is not null; type = type.ContainingType)
        {
            if (type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name is EncodingOwnerAttribute or CompatibilityServiceAttribute))
                return true;
        }
        return false;
    }
}
