using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// A result nothing reads is not computed, and a parameter nothing needs is not taken (SME6474, #2566). In
/// <c>SuperMetroid.Core</c> and <c>SuperMetroid.AssetExtraction</c>, a call to a method of the assembly does not throw its
/// result away (<c>_ = Require(x);</c>, <c>Find(x, out _)</c>): the caller uses what the call reports, or calls an
/// operation that reports nothing, or states the check it wants by name. A method no caller in its assembly reads the
/// result of returns nothing; a method that returns null on every path returns nothing; and a method takes no parameter
/// it discards (<c>_</c>). The <c>Ensure</c> guards, whose result is the value they checked, are calls made for their
/// check; a method a delegate refers to keeps the shape the delegate's contract gives it, and overrides and interface
/// implementations keep their contract's.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnreadResultAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A computed result, or a taken parameter, that nothing reads.</summary>
    public const string UnreadResultId = "SME6474";

    /// <summary>The assembly of the import pipeline, which the rule covers beside Core.</summary>
    private const string ExtractionAssembly = "SuperMetroid.AssetExtraction";

    /// <summary>The guard class whose methods return the value they checked, so a call to one is made for its check.</summary>
    private const string GuardClass = "Ensure";

    private static readonly DiagnosticDescriptor Rule = new(
        UnreadResultId,
        "Unread result or unneeded parameter",
        "'{0}' {1}",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Every result is used and every parameter needed (#2566). A call kept only for its exception states " +
            "the check by name, a draw kept only to advance the random numbers names the advance, an operation some " +
            "callers need no result from offers one that reports nothing, a method whose result no caller reads returns " +
            "nothing, and a method takes only what it uses.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName is not (CoreLayerAnalyzer.CoreAssembly or ExtractionAssembly))
                return;
            var state = new CompilationState(start.Compilation.Assembly);
            start.RegisterOperationAction(state.AnalyzeInvocation, OperationKind.Invocation);
            start.RegisterOperationAction(state.AnalyzeMethodReference, OperationKind.MethodReference);
            start.RegisterSymbolAction(state.AnalyzeMethod, SymbolKind.Method);
            start.RegisterSyntaxNodeAction(state.AnalyzeLocalFunction, SyntaxKind.LocalFunctionStatement);
            start.RegisterCompilationEndAction(state.Report);
        });
    }

    /// <summary>What one compilation's calls read, and the methods whose shape is checked once every call has been seen.</summary>
    private sealed class CompilationState(IAssemblySymbol assembly)
    {
        /// <summary>The methods some call reads the result of.</summary>
        private readonly ConcurrentDictionary<IMethodSymbol, bool> read = new(SymbolEqualityComparer.Default);

        /// <summary>The methods some call discards the result of.</summary>
        private readonly ConcurrentDictionary<IMethodSymbol, bool> discarded = new(SymbolEqualityComparer.Default);

        /// <summary>The methods a delegate refers to.</summary>
        private readonly ConcurrentDictionary<IMethodSymbol, bool> referenced = new(SymbolEqualityComparer.Default);

        /// <summary>The methods whose shape the rule checks, with what they are found to do.</summary>
        private readonly ConcurrentDictionary<IMethodSymbol, MethodShape> shapes = new(SymbolEqualityComparer.Default);

        /// <summary>Records whether a call reads its result, and reports a source call whose result it discards by name.</summary>
        internal void AnalyzeInvocation(OperationAnalysisContext context)
        {
            var invocation = (IInvocationOperation)context.Operation;
            IMethodSymbol method = invocation.TargetMethod.OriginalDefinition;
            if (!IsOwnMethod(method))
                return;
            foreach (IArgumentOperation argument in invocation.Arguments)
            {
                if (argument.Parameter is { RefKind: RefKind.Out } && argument.Value is IDiscardOperation)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, argument.Syntax.GetLocation(), method.Name,
                        "discards an out result; take only what the caller uses, or use what the call reports"));
                }
            }
            if (method.ReturnsVoid)
                return;
            switch (Consumer(invocation))
            {
                case ResultUse.DiscardedByName:
                    discarded[method] = true;
                    context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.Syntax.GetLocation(), method.Name,
                        "discards the call's result; use what the call reports, call an operation that reports nothing, or state the check by name"));
                    return;
                case ResultUse.Discarded:
                    discarded[method] = true;
                    return;
                case ResultUse.Read:
                    read[method] = true;
                    return;
                default:
                    throw new InvalidOperationException("Undefined result use.");
            }
        }

        /// <summary>Records a method a delegate refers to, whose shape its delegate's contract gives.</summary>
        internal void AnalyzeMethodReference(OperationAnalysisContext context) =>
            referenced[((IMethodReferenceOperation)context.Operation).Method.OriginalDefinition] = true;

        /// <summary>Records an ordinary method's discarded parameters and whether it returns only null.</summary>
        internal void AnalyzeMethod(SymbolAnalysisContext context)
        {
            var method = (IMethodSymbol)context.Symbol;
            if (method.MethodKind is not (MethodKind.Ordinary or MethodKind.Constructor) || !OwnsItsShape(method))
                return;
            Record(method, method.DeclaringSyntaxReferences.Select(reference => reference.GetSyntax(context.CancellationToken)));
        }

        /// <summary>Records a local function, which owns its shape as an ordinary method does.</summary>
        internal void AnalyzeLocalFunction(SyntaxNodeAnalysisContext context)
        {
            if (context.SemanticModel.GetDeclaredSymbol(context.Node, context.CancellationToken) is IMethodSymbol method)
                Record(method, [context.Node]);
        }

        /// <summary>Reports, once every call has been seen, each result no caller reads and each shape a method need not have.</summary>
        internal void Report(CompilationAnalysisContext context)
        {
            foreach (KeyValuePair<IMethodSymbol, MethodShape> entry in shapes)
            {
                IMethodSymbol method = entry.Key;
                MethodShape shape = entry.Value;
                if (referenced.ContainsKey(method))
                    continue;
                if (shape.DiscardedParameter is { } parameter)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, parameter, method.Name,
                        "takes a parameter it discards; take only what it uses"));
                }
                if (shape.ReturnsOnlyNull)
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, shape.Location, method.Name,
                        "returns null on every path; return nothing"));
                }
                else if (!method.ReturnsVoid && discarded.ContainsKey(method) && !read.ContainsKey(method))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Rule, shape.Location, method.Name,
                        "returns a value no caller reads; return nothing"));
                }
            }
        }

        /// <summary>Records what the declarations of <paramref name="method"/> show of its shape.</summary>
        private void Record(IMethodSymbol method, IEnumerable<SyntaxNode> declarations)
        {
            Location? location = method.Locations.FirstOrDefault(candidate => candidate.IsInSource);
            if (location is null || IsGuard(method))
                return;
            Location? discardedParameter = method.Parameters
                .Where(parameter => parameter.Name.Length > 0 && parameter.Name.All(character => character == '_'))
                .SelectMany(parameter => parameter.Locations)
                .FirstOrDefault(candidate => candidate.IsInSource);
            bool nullableResult = method.ReturnType.NullableAnnotation == NullableAnnotation.Annotated ||
                method.ReturnType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };
            bool returnsOnlyNull = nullableResult && declarations.Any() && declarations.All(ReturnsOnlyNull);
            shapes[method] = new MethodShape(location, discardedParameter, returnsOnlyNull);
        }

        /// <summary>Whether <paramref name="method"/> is declared in this assembly's source and the rule reads its calls.</summary>
        private bool IsOwnMethod(IMethodSymbol method) =>
            SymbolEqualityComparer.Default.Equals(method.ContainingAssembly, assembly) &&
            method.MethodKind is MethodKind.Ordinary or MethodKind.LocalFunction &&
            !IsGuard(method);

        /// <summary>
        /// Whether <paramref name="method"/> owns its shape: it overrides nothing, implements no interface member, and is
        /// neither abstract, virtual, external nor partial.
        /// </summary>
        private static bool OwnsItsShape(IMethodSymbol method)
        {
            if (method.IsOverride || method.IsAbstract || method.IsVirtual || method.IsExtern ||
                method.PartialDefinitionPart is not null || method.PartialImplementationPart is not null ||
                !method.ExplicitInterfaceImplementations.IsEmpty || method.IsImplicitlyDeclared)
            {
                return false;
            }
            INamedTypeSymbol type = method.ContainingType;
            return !type.AllInterfaces.SelectMany(face => face.GetMembers().OfType<IMethodSymbol>())
                .Any(member => SymbolEqualityComparer.Default.Equals(type.FindImplementationForInterfaceMember(member), method));
        }

        /// <summary>Whether <paramref name="method"/> is one of the <c>Ensure</c> guards.</summary>
        private static bool IsGuard(IMethodSymbol method) => method.ContainingType?.Name == GuardClass;

        /// <summary>How the value of <paramref name="invocation"/> is used.</summary>
        private static ResultUse Consumer(IInvocationOperation invocation)
        {
            IOperation used = invocation;
            while (used.Parent is IConditionalAccessOperation access && ReferenceEquals(access.WhenNotNull, used))
                used = access;
            return used.Parent switch
            {
                IExpressionStatementOperation => ResultUse.Discarded,
                ISimpleAssignmentOperation { Target: IDiscardOperation } assignment when assignment.Parent is IExpressionStatementOperation &&
                    ReferenceEquals(assignment.Value, used) => ResultUse.DiscardedByName,
                _ => ResultUse.Read,
            };
        }

        /// <summary>Whether every return of the declaration returns the null literal, and it returns at least once.</summary>
        private static bool ReturnsOnlyNull(SyntaxNode declaration)
        {
            (BlockSyntax? body, ArrowExpressionClauseSyntax? arrow) = declaration switch
            {
                MethodDeclarationSyntax method => (method.Body, method.ExpressionBody),
                LocalFunctionStatementSyntax local => (local.Body, local.ExpressionBody),
                _ => (null, null),
            };
            if (arrow is not null)
                return IsNull(arrow.Expression);
            if (body is null)
                return false;
            ReturnStatementSyntax[] returns = [.. body.DescendantNodes(node => node is not (LambdaExpressionSyntax or LocalFunctionStatementSyntax or AnonymousMethodExpressionSyntax))
                .OfType<ReturnStatementSyntax>()];
            return returns.Length != 0 && returns.All(statement => statement.Expression is { } expression && IsNull(expression));
        }

        /// <summary>Whether <paramref name="expression"/> is the null literal, perhaps in parentheses.</summary>
        private static bool IsNull(ExpressionSyntax expression)
        {
            while (expression is ParenthesizedExpressionSyntax parenthesized)
                expression = parenthesized.Expression;
            return expression.IsKind(SyntaxKind.NullLiteralExpression);
        }
    }

    /// <summary>How a call's value is used.</summary>
    private enum ResultUse
    {
        /// <summary>Something reads the value.</summary>
        Read,

        /// <summary>The call is a statement of its own, its value unread.</summary>
        Discarded,

        /// <summary>The value is assigned to the discard.</summary>
        DiscardedByName,
    }

    /// <summary>What a method's declaration shows of its shape.</summary>
    /// <param name="Location">The method's name.</param>
    /// <param name="DiscardedParameter">The first parameter it discards, or null when it discards none.</param>
    /// <param name="ReturnsOnlyNull">Whether it is declared to return a value and returns null on every path.</param>
    private sealed record MethodShape(Location Location, Location? DiscardedParameter, bool ReturnsOnlyNull);
}
