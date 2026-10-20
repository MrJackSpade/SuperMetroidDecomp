using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// #1374: a shared delegate or interface shape that is wider than its implementations makes every implementation take
/// parameters it ignores and, usually, forward the few it reads to the real method. In SuperMetroid.Core a method whose
/// body never reads two or more of its parameters is reported (SME6426), as is a method whose whole body is one call to
/// another method of its own type passing a strict subset of its parameters unchanged (SME6427). The fix is to narrow the
/// contract and register the real method; an implementation of an external contract that cannot be narrowed says so with
/// <c>[ExternalContract(reason)]</c>.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ForwardingContractAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A method never reads two or more of its parameters.</summary>
    public const string DiscardedParametersId = "SME6426";

    /// <summary>A method only forwards some of its parameters to another method of its type.</summary>
    public const string PureForwarderId = "SME6427";

    /// <summary>The attribute that marks an implementation of an external contract.</summary>
    public const string ExternalContractAttributeName = "ExternalContractAttribute";

    /// <summary>The assembly whose methods are checked.</summary>
    public const string CheckedAssembly = "SuperMetroid.Core";

    /// <summary>Reports a method that ignores two or more parameters.</summary>
    private static readonly DiagnosticDescriptor DiscardedParametersRule = new(
        DiscardedParametersId,
        "Method discards its parameters",
        "'{0}' never reads its parameters {1}; narrow the delegate or interface that forces them, or mark an external contract [ExternalContract(reason)]",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A SuperMetroid.Core method whose body never reads two or more of its parameters exists to fit a shape wider " +
            "than it needs (#1374).");

    /// <summary>Reports a method that only forwards part of its parameter list.</summary>
    private static readonly DiagnosticDescriptor PureForwarderRule = new(
        PureForwarderId,
        "Method only forwards to another",
        "'{0}' only passes {1} of its {2} parameters unchanged to '{3}'; register '{3}' directly and narrow the contract",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A SuperMetroid.Core method whose whole body is one call to another method of its own type, passing a strict " +
            "subset of its parameters unchanged, adds nothing but a wider signature (#1374).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [DiscardedParametersRule, PureForwarderRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName == CheckedAssembly)
                start.RegisterOperationBlockAction(AnalyzeBody);
        });
    }

    /// <summary>Checks one method body for discarded parameters and for a pure forward.</summary>
    private static void AnalyzeBody(OperationBlockAnalysisContext context)
    {
        if (context.OwningSymbol is not IMethodSymbol
            {
                MethodKind: MethodKind.Ordinary or MethodKind.ExplicitInterfaceImplementation,
                IsImplicitlyDeclared: false,
            } method ||
            method.Parameters.Length == 0 || Justified(method))
        {
            return;
        }
        // Parameter default values are operation blocks of their own; only the body says what the method reads.
        ImmutableArray<IOperation> bodies = [.. context.OperationBlocks.Where(block => block is IBlockOperation)];
        if (bodies.IsEmpty)
            return;
        IOperation[] operations = [.. bodies.SelectMany(block => block.DescendantsAndSelf())];
        var read = new HashSet<IParameterSymbol>(operations.OfType<IParameterReferenceOperation>().Select(reference => reference.Parameter),
            SymbolEqualityComparer.Default);
        // An out parameter is the method's result: it is written, never read.
        IParameterSymbol[] discarded = [.. method.Parameters.Where(parameter => parameter.RefKind != RefKind.Out && !read.Contains(parameter))];
        if (discarded.Length >= 2)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiscardedParametersRule, method.Locations[0], Describe(method),
                string.Join(", ", discarded.Select(parameter => parameter.Name))));
        }
        if (ForwardedCall(bodies) is { } call && Forwards(method, call) is { } passed)
        {
            context.ReportDiagnostic(Diagnostic.Create(PureForwarderRule, method.Locations[0], Describe(method), passed,
                method.Parameters.Length, call.TargetMethod.Name));
        }
    }

    /// <summary>The one call a body consists of, or null when it does anything else.</summary>
    private static IInvocationOperation? ForwardedCall(ImmutableArray<IOperation> blocks)
    {
        if (blocks.Length != 1 || blocks[0] is not IBlockOperation { Operations.Length: 1 } block)
            return null;
        return block.Operations[0] switch
        {
            IExpressionStatementOperation { Operation: IInvocationOperation invocation } => invocation,
            IReturnOperation { ReturnedValue: IInvocationOperation invocation } => invocation,
            _ => null,
        };
    }

    /// <summary>
    /// How many parameters a call forwards when it targets another method of the same type and passes only the caller's
    /// own parameters unchanged, each at most once, fewer than the caller takes; null when it is not such a forward.
    /// </summary>
    private static int? Forwards(IMethodSymbol method, IInvocationOperation call)
    {
        IMethodSymbol target = call.TargetMethod.OriginalDefinition;
        if (SymbolEqualityComparer.Default.Equals(target, method.OriginalDefinition) ||
            !SymbolEqualityComparer.Default.Equals(target.ContainingType, method.ContainingType.OriginalDefinition) ||
            call.Instance is not (null or IInstanceReferenceOperation))
        {
            return null;
        }
        var passed = new HashSet<IParameterSymbol>(SymbolEqualityComparer.Default);
        foreach (IArgumentOperation argument in call.Arguments)
        {
            // An omitted optional argument is the target's own default, not a value the caller supplies.
            if (argument.ArgumentKind == ArgumentKind.DefaultValue)
                continue;
            IOperation value = argument.Value;
            while (value is IConversionOperation { IsImplicit: true } conversion)
                value = conversion.Operand;
            if (value is not IParameterReferenceOperation reference ||
                !SymbolEqualityComparer.Default.Equals(reference.Parameter.ContainingSymbol, method) ||
                !passed.Add(reference.Parameter))
            {
                return null;
            }
        }
        return passed.Count < method.Parameters.Length ? passed.Count : null;
    }

    /// <summary>Whether a method carries <c>[ExternalContract(reason)]</c>.</summary>
    private static bool Justified(IMethodSymbol method) =>
        method.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == ExternalContractAttributeName);

    /// <summary>The method's name as a reader finds it.</summary>
    private static string Describe(IMethodSymbol method) => $"{method.ContainingType.Name}.{method.Name}";
}
