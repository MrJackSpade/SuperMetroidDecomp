using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.CartridgeEncoding;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Catalog files hold data only (SME6451, #1452): in <c>SuperMetroid.Core</c>, a file the encoding scanner names as a
/// catalog (<see cref="CartridgeEncodingScanner.IsCatalogFile"/>) declares no decision and no procedure. Every
/// <c>if</c>, <c>switch</c> statement or expression, conditional <c>?:</c>, null-coalescing or null-conditional
/// operator, loop, <c>goto</c>, pattern or type test, non-constant comparison, query <c>where</c> clause and LINQ
/// predicate is reported, and so is every body (a method, constructor, accessor, operator, local function or lambda)
/// with statements beyond a single <c>return</c>. Data stays: initializers of literals, record and collection
/// construction, dictionary initializers, <c>nameof</c> and constant arithmetic. A catalog-named file generated from
/// data is authored data too and is analyzed; only build output is skipped.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CatalogDecisionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A Core catalog file holds a decision or a procedure.</summary>
    public const string CatalogDecisionId = "SME6451";

    /// <summary>Methods that take a predicate and decide which elements of a sequence count.</summary>
    private static readonly ImmutableHashSet<string> PredicateMethods =
    [
        "Where", "Any", "All", "First", "FirstOrDefault", "Single", "SingleOrDefault", "Last", "LastOrDefault",
        "Count", "LongCount", "TakeWhile", "SkipWhile", "Find", "FindIndex", "FindLast", "FindLastIndex", "FindAll",
        "Exists", "TrueForAll", "RemoveAll",
    ];

    /// <summary>The decision syntax a catalog may not hold, each with how the diagnostic names it.</summary>
    private static readonly ImmutableDictionary<SyntaxKind, string> Decisions = new Dictionary<SyntaxKind, string>
    {
        [SyntaxKind.IfStatement] = "an 'if'",
        [SyntaxKind.SwitchStatement] = "a 'switch' statement",
        [SyntaxKind.SwitchExpression] = "a 'switch' expression",
        [SyntaxKind.ConditionalExpression] = "a conditional '?:'",
        [SyntaxKind.CoalesceExpression] = "a null-coalescing '??'",
        [SyntaxKind.CoalesceAssignmentExpression] = "a null-coalescing '??'",
        [SyntaxKind.ConditionalAccessExpression] = "a null-conditional access",
        [SyntaxKind.ForStatement] = "a loop",
        [SyntaxKind.ForEachStatement] = "a loop",
        [SyntaxKind.ForEachVariableStatement] = "a loop",
        [SyntaxKind.WhileStatement] = "a loop",
        [SyntaxKind.DoStatement] = "a loop",
        [SyntaxKind.GotoStatement] = "a 'goto'",
        [SyntaxKind.GotoCaseStatement] = "a 'goto'",
        [SyntaxKind.GotoDefaultStatement] = "a 'goto'",
        [SyntaxKind.IsPatternExpression] = "a pattern test",
        [SyntaxKind.IsExpression] = "a pattern test",
        [SyntaxKind.WhereClause] = "a query predicate",
    }.ToImmutableDictionary();

    private static readonly DiagnosticDescriptor Rule = new(
        CatalogDecisionId,
        "Core catalog holds a decision",
        "The catalog '{0}' holds {1}; a catalog holds data only: move the decision to the domain type that owns the behaviour, or to the import pipeline when it decodes cartridge bytes",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Definition data lives in domain-named catalog types, and catalog files hold data only: no if, switch, " +
            "loops or decisions (AGENTS.md). A decision in a catalog hides behaviour behind a data name and exempts it from " +
            "the rules that check functional code. The data stays in the catalog as records, tables and dictionaries; the " +
            "decision moves to the domain type that owns the behaviour, named for the game, or into the import pipeline " +
            "when it is an encoding of cartridge bytes (#1452).");

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
            start.RegisterSyntaxNodeAction(AnalyzeDecision, [.. Decisions.Keys]);
            start.RegisterSyntaxNodeAction(AnalyzeComparison,
                SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression, SyntaxKind.LessThanExpression,
                SyntaxKind.LessThanOrEqualExpression, SyntaxKind.GreaterThanExpression,
                SyntaxKind.GreaterThanOrEqualExpression, SyntaxKind.LogicalAndExpression, SyntaxKind.LogicalOrExpression);
            start.RegisterSyntaxNodeAction(AnalyzePredicate, SyntaxKind.InvocationExpression);
            start.RegisterSyntaxNodeAction(AnalyzeBody, SyntaxKind.Block);
        });
    }

    /// <summary>Whether <paramref name="tree"/> is an authored catalog file.</summary>
    internal static bool IsCatalog(SyntaxTree tree) =>
        CartridgeEncodingScanner.IsCatalogFile(tree.FilePath) && !CartridgeEncodingScanner.IsBuildGenerated(tree.FilePath);

    /// <summary>Reports a branch, loop, jump, test or query predicate.</summary>
    private static void AnalyzeDecision(SyntaxNodeAnalysisContext context)
    {
        if (!IsCatalog(context.Node.SyntaxTree))
            return;
        Report(context, context.Node.GetFirstToken().GetLocation(), Decisions[context.Node.Kind()]);
    }

    /// <summary>Reports a comparison or logical test whose value is not a compile-time constant.</summary>
    private static void AnalyzeComparison(SyntaxNodeAnalysisContext context)
    {
        if (!IsCatalog(context.Node.SyntaxTree) ||
            context.SemanticModel.GetConstantValue(context.Node, context.CancellationToken).HasValue)
        {
            return;
        }
        var binary = (BinaryExpressionSyntax)context.Node;
        Report(context, binary.OperatorToken.GetLocation(), $"a comparison '{binary.OperatorToken.Text}'");
    }

    /// <summary>Reports a call that filters or searches a sequence with a predicate.</summary>
    private static void AnalyzePredicate(SyntaxNodeAnalysisContext context)
    {
        if (!IsCatalog(context.Node.SyntaxTree))
            return;
        var invocation = (InvocationExpressionSyntax)context.Node;
        string? name = invocation.Expression switch
        {
            MemberAccessExpressionSyntax member => member.Name.Identifier.Text,
            MemberBindingExpressionSyntax binding => binding.Name.Identifier.Text,
            _ => null,
        };
        if (name is null || !PredicateMethods.Contains(name) ||
            !invocation.ArgumentList.Arguments.Any(argument => IsPredicate(context, argument)))
        {
            return;
        }
        Report(context, invocation.ArgumentList.GetLocation(), $"a predicate passed to '{name}'");
    }

    /// <summary>
    /// Whether <paramref name="argument"/> is a predicate: it converts to a delegate that answers yes or no. A lookup
    /// that shares a predicate method's name but takes a key, such as a name, decides nothing.
    /// </summary>
    private static bool IsPredicate(SyntaxNodeAnalysisContext context, ArgumentSyntax argument) =>
        context.SemanticModel.GetTypeInfo(argument.Expression, context.CancellationToken).ConvertedType is
            INamedTypeSymbol { TypeKind: TypeKind.Delegate, DelegateInvokeMethod.ReturnType.SpecialType: SpecialType.System_Boolean };

    /// <summary>
    /// Reports a body with statements beyond a single <c>return</c>: a method, constructor, accessor, operator,
    /// local function or lambda that runs a procedure rather than producing a value.
    /// </summary>
    private static void AnalyzeBody(SyntaxNodeAnalysisContext context)
    {
        var block = (BlockSyntax)context.Node;
        if (!IsCatalog(block.SyntaxTree) || !IsBody(block))
            return;
        if (block.Statements.Count == 0 || (block.Statements.Count == 1 && block.Statements[0] is ReturnStatementSyntax))
            return;
        Report(context, block.OpenBraceToken.GetLocation(), "a body with statements");
    }

    /// <summary>Whether <paramref name="block"/> is the body of a member, accessor, local function or lambda.</summary>
    private static bool IsBody(BlockSyntax block) => block.Parent is BaseMethodDeclarationSyntax or AccessorDeclarationSyntax or
        LocalFunctionStatementSyntax or AnonymousFunctionExpressionSyntax;

    /// <summary>Reports one finding against the type that declares it, or against the file outside any type.</summary>
    private static void Report(SyntaxNodeAnalysisContext context, Location location, string what)
    {
        string owner = context.Node.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>()?.Identifier.Text ??
            Path.GetFileName(context.Node.SyntaxTree.FilePath);
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, owner, what));
    }
}
