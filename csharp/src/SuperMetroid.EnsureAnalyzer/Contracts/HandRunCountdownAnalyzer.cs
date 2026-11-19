using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// A wait is the shared countdown, never an integer stepped by hand (SME6463, #2561): in <c>SuperMetroid.Core</c>, an
/// integral field, property or local whose name says it times a wait (a timer, countdown, delay, cooldown, wait, hold or
/// lifetime, the updates, frames or ticks counted, or what is left of one) is not stepped by one in place:
/// <c>x++</c>, <c>++x</c>, <c>x--</c>, <c>--x</c>, <c>x += 1</c>, <c>x -= 1</c>, or an assignment to <c>x</c> whose value
/// takes one from or adds one to <c>x</c> itself (<c>x = x - 1</c>, <c>x = Math.Max(0, x - 1)</c>,
/// <c>x = Math.Min(limit, x + 1)</c>). Such a wait is an <c>UpdateCountdown</c> (its <c>Count</c>, <c>Step</c> and
/// <c>StepEnds</c> say how it ends), a count that has no length of its own is <c>ElapsedUpdates</c>, and those types,
/// the value-typed <c>IntervalClock</c> and the two animation players that own their frames' holds are the one place
/// that steps a count by one.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HandRunCountdownAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Core steps a wait's integer by hand.</summary>
    public const string HandRunCountdownId = "SME6463";

    /// <summary>
    /// The shared types that own stepping a count by one: the countdown and the elapsed count, the value-typed repeating
    /// countdown records carry, and the two animation players, which own their frames' holds.
    /// </summary>
    private static readonly ImmutableHashSet<string> Owners = ImmutableHashSet.Create(
        "SuperMetroid.Core.UpdateCountdown", "SuperMetroid.Core.ElapsedUpdates", "SuperMetroid.Core.Fx.IntervalClock",
        "SuperMetroid.Core.Animation.AnimationPlayer", "SuperMetroid.Core.Enemies.EnemyAnimationClock");

    /// <summary>The camel-case words that mark a name as timing a wait.</summary>
    private static readonly ImmutableHashSet<string> WaitWords = ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase,
        "timer", "timers", "countdown", "delay", "cooldown", "wait", "hold", "left", "lifetime", "frames", "updates",
        "ticks", "tick");

    private static readonly DiagnosticDescriptor Rule = new(
        HandRunCountdownId,
        "Wait stepped by hand",
        "'{0}' steps the wait '{1}' by hand; hold it as an UpdateCountdown (Count, Step or StepEnds say how it ends), or as ElapsedUpdates for a count with no length",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A wait, a cooldown or a hold kept as an integer that its owner increments or decrements by hand restates " +
            "the countdown each time, with its own off-by-one and its own rule for what happens once it runs out. The " +
            "shared UpdateCountdown holds the updates left and names how each owner ends its wait; ElapsedUpdates counts " +
            "a phase with no length of its own (#2561).");

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
            start.RegisterSyntaxNodeAction(AnalyzeUnary, SyntaxKind.PreIncrementExpression, SyntaxKind.PreDecrementExpression,
                SyntaxKind.PostIncrementExpression, SyntaxKind.PostDecrementExpression);
            start.RegisterSyntaxNodeAction(AnalyzeAssignment, SyntaxKind.AddAssignmentExpression,
                SyntaxKind.SubtractAssignmentExpression, SyntaxKind.SimpleAssignmentExpression);
        });
    }

    /// <summary>Reports <c>x++</c>, <c>++x</c>, <c>x--</c> and <c>--x</c> on a wait.</summary>
    private static void AnalyzeUnary(SyntaxNodeAnalysisContext context)
    {
        ExpressionSyntax operand = context.Node switch
        {
            PrefixUnaryExpressionSyntax prefix => prefix.Operand,
            PostfixUnaryExpressionSyntax postfix => postfix.Operand,
            _ => throw new InvalidOperationException($"Unexpected node {context.Node.Kind()}."),
        };
        if (WaitSymbol(context, operand) is { } wait)
            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Node.GetLocation(), context.Node.ToString(), wait.Name));
    }

    /// <summary>Reports <c>x += 1</c>, <c>x -= 1</c> and an assignment whose value steps the target itself by one.</summary>
    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        var assignment = (AssignmentExpressionSyntax)context.Node;
        if (WaitSymbol(context, assignment.Left) is not { } wait)
            return;
        bool compound = assignment.IsKind(SyntaxKind.AddAssignmentExpression) || assignment.IsKind(SyntaxKind.SubtractAssignmentExpression);
        bool stepped = compound
            ? IsOne(context, assignment.Right)
            : assignment.Right.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>().Any(binary =>
                (binary.IsKind(SyntaxKind.AddExpression) || binary.IsKind(SyntaxKind.SubtractExpression)) &&
                (IsOne(context, binary.Right) && SameSymbol(context, binary.Left, wait) ||
                    binary.IsKind(SyntaxKind.AddExpression) && IsOne(context, binary.Left) && SameSymbol(context, binary.Right, wait)));
        if (stepped)
            context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.GetLocation(), assignment.ToString(), wait.Name));
    }

    /// <summary>
    /// The integral field, property or local <paramref name="target"/> names when its name marks a wait and it is not one of
    /// the owners' own members; null otherwise.
    /// </summary>
    private static ISymbol? WaitSymbol(SyntaxNodeAnalysisContext context, ExpressionSyntax target)
    {
        ISymbol? symbol = context.SemanticModel.GetSymbolInfo(target, context.CancellationToken).Symbol;
        ITypeSymbol? type = symbol switch
        {
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            ILocalSymbol local => local.Type,
            _ => null,
        };
        if (symbol is null || type is null || !IsIntegral(type) || !NamesWait(symbol.Name))
            return null;
        INamedTypeSymbol? containing = context.ContainingSymbol?.ContainingType ?? context.ContainingSymbol as INamedTypeSymbol;
        return containing is not null && Owners.Contains($"{containing.ContainingNamespace.ToDisplayString()}.{containing.Name}") ? null : symbol;
    }

    /// <summary>Whether <paramref name="type"/> is an integral number, not an enum.</summary>
    private static bool IsIntegral(ITypeSymbol type) =>
        type.TypeKind != TypeKind.Enum && type.SpecialType is SpecialType.System_Int32 or SpecialType.System_Int16 or
            SpecialType.System_Int64 or SpecialType.System_UInt16 or SpecialType.System_UInt32 or SpecialType.System_Byte or
            SpecialType.System_SByte;

    /// <summary>Whether one of <paramref name="name"/>'s camel-case words marks a wait.</summary>
    internal static bool NamesWait(string name)
    {
        string trimmed = name.TrimStart('_');
        int start = 0;
        for (int index = 1; index <= trimmed.Length; index++)
        {
            if (index < trimmed.Length && !char.IsUpper(trimmed[index]))
                continue;
            if (WaitWords.Contains(trimmed.Substring(start, index - start)))
                return true;
            start = index;
        }
        return false;
    }

    /// <summary>Whether <paramref name="expression"/> is the constant one.</summary>
    private static bool IsOne(SyntaxNodeAnalysisContext context, ExpressionSyntax expression) =>
        context.SemanticModel.GetConstantValue(expression, context.CancellationToken) is { HasValue: true, Value: int one } && one == 1;

    /// <summary>Whether <paramref name="expression"/> reads <paramref name="symbol"/>.</summary>
    private static bool SameSymbol(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, ISymbol symbol) =>
        SymbolEqualityComparer.Default.Equals(context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken).Symbol, symbol);
}
