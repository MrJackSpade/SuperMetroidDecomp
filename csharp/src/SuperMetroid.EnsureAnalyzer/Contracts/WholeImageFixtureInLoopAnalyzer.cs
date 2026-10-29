using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// A whole-image verification helper is not rebuilt per read (SME6452, #1463): a method marked
/// <c>[WholeImageFixture]</c> builds an entire memory image on every call, so calling one inside a loop body or condition,
/// or inside a lambda a LINQ operator runs per element, rebuilds the image for every value the check compares. The one
/// shape allowed inside a loop is the initializer of a local variable, which states a deliberate rebuild per state (the
/// loop changes what the image shows, then reads it). The file-select map checks rebuilt a 64 KiB image per tilemap word
/// and took over a thousand seconds; this rule makes that shape a build error.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class WholeImageFixtureInLoopAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A whole-image helper is called per loop iteration or per element.</summary>
    public const string FixtureInLoopId = "SME6452";

    /// <summary>The marking attribute's type name.</summary>
    private const string AttributeName = "WholeImageFixtureAttribute";

    private static readonly DiagnosticDescriptor Rule = new(
        FixtureInLoopId,
        "Whole-image fixture rebuilt per read",
        "'{0}' builds {1} on every call and is called {2}; build it once into a local outside the reads and read from that",
        "Performance",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A verification helper marked [WholeImageFixture] rebuilds a whole memory image per call; build it once per state, never per compared value.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(Analyze, OperationKind.Invocation);
    }

    /// <summary>Reports a marked helper called per iteration or per element.</summary>
    private static void Analyze(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod.ReducedFrom ?? invocation.TargetMethod;
        AttributeData? marking = method.GetAttributes().FirstOrDefault(attribute => attribute.AttributeClass?.Name == AttributeName);
        if (marking is null)
            return;
        if (Repetition(invocation.Syntax, invocation.SemanticModel) is not { } where)
            return;
        string builds = marking.ConstructorArguments.Length == 1 && marking.ConstructorArguments[0].Value is string text
            ? text : "a whole image";
        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), method.Name, builds, where));
    }

    /// <summary>
    /// How <paramref name="call"/> repeats: "inside a loop" when it sits in a loop's body or condition, "per element of
    /// a LINQ operator" when it sits in a lambda a LINQ operator runs, or null when it runs once per call of its member or
    /// is a local's initializer inside the loop.
    /// </summary>
    private static string? Repetition(SyntaxNode call, SemanticModel? model)
    {
        SyntaxNode child = call;
        bool initializesLocal = IsLocalInitializer(call);
        for (SyntaxNode? node = call.Parent; node is not null; child = node, node = node.Parent)
        {
            switch (node)
            {
                case MemberDeclarationSyntax or LocalFunctionStatementSyntax:
                    return null;
                case ForStatementSyntax loop when child == loop.Statement || child == loop.Condition || loop.Incrementors.Contains(child):
                case ForEachStatementSyntax each when child == each.Statement:
                case ForEachVariableStatementSyntax eachVariable when child == eachVariable.Statement:
                case WhileStatementSyntax:
                case DoStatementSyntax:
                    return initializesLocal ? null : "inside a loop";
                case AnonymousFunctionExpressionSyntax lambda when IsLinqArgument(lambda, model):
                    return "per element of a LINQ operator";
                default:
                    break;
            }
        }
        return null;
    }

    /// <summary>Whether <paramref name="call"/> is the whole initializer of a local variable declaration.</summary>
    private static bool IsLocalInitializer(SyntaxNode call) =>
        call.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Parent: LocalDeclarationStatementSyntax } } };

    /// <summary>Whether <paramref name="lambda"/> is an argument of a <c>System.Linq.Enumerable</c> operator.</summary>
    private static bool IsLinqArgument(AnonymousFunctionExpressionSyntax lambda, SemanticModel? model) =>
        lambda.Parent is ArgumentSyntax { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax operatorCall } } &&
        model?.GetSymbolInfo(operatorCall).Symbol is IMethodSymbol linq &&
        linq.ContainingType?.ToDisplayString() == "System.Linq.Enumerable";
}
