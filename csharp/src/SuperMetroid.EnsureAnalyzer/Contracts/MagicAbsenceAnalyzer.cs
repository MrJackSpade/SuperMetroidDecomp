using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Magic values never stand for a role or absence (SME6476, #2558). In <c>SuperMetroid.Core</c> and
/// <c>SuperMetroid.AssetExtraction</c> the rule reports a sentinel number, minus one or <c>int.MaxValue</c>, standing for
/// "none" or "not yet": an array or span filled with one (<c>Fill</c>, <c>Enumerable.Repeat</c>); a choice that gives one
/// beside a computed value (<c>found ? index : -1</c>, <c>index ?? -1</c>); a method that returns one; a field or
/// property seeded with one; and a local seeded or set to one that the same body then tests against zero or minus one
/// (<c>if (selected &lt; 0)</c>). It also reports a placeholder member of an identity: a member named <c>None</c>,
/// <c>Empty</c>, <c>Blank</c> or <c>Nothing</c> of an enum named <c>…Id</c> that is not <c>[Flags]</c>, and a static member of
/// that name and of its own type on a type named <c>…Id</c> or <c>…Type</c>.
/// </summary>
/// <remarks>
/// Absence is a typed optional (<c>int?</c>, a nullable reference, a <c>TryGet</c>) or a case of a closed type, and a role
/// is a named member or type. A named constant (<c>const int Step = -1</c>) is a definition, not a sentinel, and a sign or
/// step beside another constant (<c>left ? -1 : 1</c>) is a sign; neither is reported. A comparison's result
/// (<c>CompareTo</c>, <c>Compare</c>) is not reported. A test of a library's own result against zero (<c>IndexOf</c>,
/// <c>ReadByte</c>) is the library's encoding read at its boundary; only a value the code itself seeds is reported.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MagicAbsenceAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A magic value standing for a role or absence.</summary>
    public const string MagicAbsenceId = "SME6476";

    /// <summary>The import pipeline's assembly, which the rule reads beside Core.</summary>
    private const string ExtractionAssembly = "SuperMetroid.AssetExtraction";

    /// <summary>The names a placeholder member of an identity takes.</summary>
    private static readonly ImmutableHashSet<string> PlaceholderNames = ImmutableHashSet.Create("None", "Empty", "Blank", "Nothing");

    /// <summary>The methods whose sentinel result is a comparison's order rather than absence.</summary>
    private static readonly ImmutableHashSet<string> ComparisonMethods = ImmutableHashSet.Create("CompareTo", "Compare");

    private static readonly DiagnosticDescriptor Rule = new(
        MagicAbsenceId,
        "Magic value standing for a role or absence",
        "{0}; hold absence as a typed optional or a closed type's case, and a role as a named member",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Absence and special roles are named members or types, never -1, int.MaxValue or a None placeholder (#2558): " +
            "a sentinel number makes every reader remember the convention and lets an absent value flow into arithmetic, and a " +
            "None member of an identity makes the identity stand for no thing.");

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
            start.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
            start.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
            start.RegisterOperationAction(AnalyzeConditional, OperationKind.Conditional);
            start.RegisterOperationAction(AnalyzeCoalesce, OperationKind.Coalesce);
            start.RegisterOperationAction(AnalyzeReturn, OperationKind.Return);
            start.RegisterOperationAction(AnalyzeMemberInitializer, OperationKind.FieldInitializer, OperationKind.PropertyInitializer);
            start.RegisterOperationBlockStartAction(AnalyzeBody);
        });
    }

    /// <summary>Reports a placeholder member of an identity enum, or a placeholder static of an identity or type value.</summary>
    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (type.TypeKind == TypeKind.Enum)
        {
            if (!type.Name.EndsWith("Id", StringComparison.Ordinal) ||
                type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == "FlagsAttribute"))
                return;
            foreach (IFieldSymbol member in type.GetMembers().OfType<IFieldSymbol>().Where(member => PlaceholderNames.Contains(member.Name)))
                Report(context, member, $"'{type.Name}.{member.Name}' is an identity member standing for no {type.Name}");
            return;
        }
        if (!(type.Name.EndsWith("Id", StringComparison.Ordinal) || type.Name.EndsWith("Type", StringComparison.Ordinal)))
            return;
        foreach (ISymbol member in type.GetMembers().Where(member => member.IsStatic && PlaceholderNames.Contains(member.Name)))
        {
            ITypeSymbol? memberType = member switch
            {
                IPropertySymbol property => property.Type,
                IFieldSymbol field => field.Type,
                _ => null,
            };
            if (SymbolEqualityComparer.Default.Equals(memberType, type))
                Report(context, member, $"'{type.Name}.{member.Name}' is a value of the type standing for no {type.Name}");
        }
    }

    /// <summary>Reports a sentinel an array, span or sequence is filled with.</summary>
    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        IMethodSymbol method = invocation.TargetMethod;
        bool fills = method.Name == "Fill" || (method.Name == "Repeat" && method.ContainingType?.Name == "Enumerable");
        if (!fills)
            return;
        foreach (IArgumentOperation argument in invocation.Arguments.Where(argument => IsSentinel(argument.Value)))
            context.ReportDiagnostic(Diagnostic.Create(Rule, argument.Syntax.GetLocation(),
                $"'{argument.Syntax}' fills {method.Name} with a sentinel standing for no value"));
    }

    /// <summary>Reports a sentinel a choice gives beside a computed value.</summary>
    private static void AnalyzeConditional(OperationAnalysisContext context)
    {
        var conditional = (IConditionalOperation)context.Operation;
        if (conditional.WhenFalse is not { } whenFalse || conditional.IsRef)
            return;
        if (IsSentinel(conditional.WhenTrue) && !Unwrapped(whenFalse).ConstantValue.HasValue)
            ReportSentinel(context, conditional.WhenTrue, "a choice gives");
        else if (IsSentinel(whenFalse) && !Unwrapped(conditional.WhenTrue).ConstantValue.HasValue)
            ReportSentinel(context, whenFalse, "a choice gives");
    }

    /// <summary>Reports a sentinel a missing value is replaced with.</summary>
    private static void AnalyzeCoalesce(OperationAnalysisContext context)
    {
        var coalesce = (ICoalesceOperation)context.Operation;
        if (IsSentinel(coalesce.WhenNull))
            ReportSentinel(context, coalesce.WhenNull, "a missing value is replaced with");
    }

    /// <summary>Reports a sentinel a method returns, other than a comparison's order.</summary>
    private static void AnalyzeReturn(OperationAnalysisContext context)
    {
        var returned = (IReturnOperation)context.Operation;
        if (returned.ReturnedValue is not { } value || !IsSentinel(value))
            return;
        if (context.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.UserDefinedOperator } ||
            context.ContainingSymbol is IMethodSymbol method && ComparisonMethods.Contains(method.Name))
            return;
        ReportSentinel(context, value, "a method returns");
    }

    /// <summary>Reports a field or property seeded with a sentinel; a constant is a definition.</summary>
    private static void AnalyzeMemberInitializer(OperationAnalysisContext context)
    {
        var initializer = (ISymbolInitializerOperation)context.Operation;
        bool constant = initializer is IFieldInitializerOperation fields && fields.InitializedFields.All(field => field.IsConst);
        if (!constant && IsSentinel(initializer.Value))
            ReportSentinel(context, initializer.Value, "a member is seeded with");
    }

    /// <summary>Reports a local seeded or set to a sentinel that the same body tests against zero or minus one.</summary>
    private static void AnalyzeBody(OperationBlockStartAnalysisContext start)
    {
        var seeded = new ConcurrentBag<(ILocalSymbol Local, IOperation Value)>();
        var tested = new ConcurrentDictionary<ILocalSymbol, bool>(SymbolEqualityComparer.Default);
        start.RegisterOperationAction(context =>
        {
            var declarator = (IVariableDeclaratorOperation)context.Operation;
            if (declarator.GetVariableInitializer()?.Value is { } value && IsSentinel(value) && !declarator.Symbol.IsConst)
                seeded.Add((declarator.Symbol, value));
        }, OperationKind.VariableDeclarator);
        start.RegisterOperationAction(context =>
        {
            var assignment = (ISimpleAssignmentOperation)context.Operation;
            if (assignment.Target is ILocalReferenceOperation local && IsSentinel(assignment.Value))
                seeded.Add((local.Local, assignment.Value));
        }, OperationKind.SimpleAssignment);
        start.RegisterOperationAction(context =>
        {
            var binary = (IBinaryOperation)context.Operation;
            if (TestedLocal(binary) is { } local)
                tested[local] = true;
        }, OperationKind.Binary);
        start.RegisterOperationBlockEndAction(end =>
        {
            foreach ((ILocalSymbol local, IOperation value) in seeded)
            {
                if (tested.ContainsKey(local))
                    end.ReportDiagnostic(Diagnostic.Create(Rule, value.Syntax.GetLocation(),
                        $"'{local.Name}' is seeded with '{value.Syntax}', a sentinel its body tests for"));
            }
        });
    }

    /// <summary>The local a comparison tests for a sentinel: against zero by order, or against minus one by equality.</summary>
    private static ILocalSymbol? TestedLocal(IBinaryOperation binary)
    {
        bool ordered = binary.OperatorKind is BinaryOperatorKind.LessThan or BinaryOperatorKind.LessThanOrEqual or
            BinaryOperatorKind.GreaterThan or BinaryOperatorKind.GreaterThanOrEqual;
        bool equality = binary.OperatorKind is BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals;
        if (!ordered && !equality)
            return null;
        foreach ((IOperation side, IOperation other) in new[] { (binary.LeftOperand, binary.RightOperand), (binary.RightOperand, binary.LeftOperand) })
        {
            if (Unwrapped(side) is not ILocalReferenceOperation local)
                continue;
            if ((ordered && IsConstant(other, 0)) || (equality && IsSentinel(other)))
                return local.Local;
        }
        return null;
    }

    /// <summary>Reports <paramref name="value"/>, a sentinel <paramref name="use"/>.</summary>
    private static void ReportSentinel(OperationAnalysisContext context, IOperation value, string use) =>
        context.ReportDiagnostic(Diagnostic.Create(Rule, value.Syntax.GetLocation(), $"'{value.Syntax}' is a sentinel {use} for no value"));

    /// <summary>Reports <paramref name="member"/> at its declaration.</summary>
    private static void Report(SymbolAnalysisContext context, ISymbol member, string message)
    {
        foreach (Location location in member.Locations.Where(location => location.IsInSource))
            context.ReportDiagnostic(Diagnostic.Create(Rule, location, message));
    }

    /// <summary>
    /// Whether <paramref name="operation"/> is minus one or the largest <c>int</c>, through any conversion. A constant the
    /// code itself names is not one: its name is what makes the value not magic.
    /// </summary>
    private static bool IsSentinel(IOperation operation)
    {
        IOperation unwrapped = Unwrapped(operation);
        if (unwrapped is IFieldReferenceOperation { Field: { IsConst: true } field } && field.Locations.Any(location => location.IsInSource))
            return false;
        if (unwrapped is ILocalReferenceOperation { Local.IsConst: true })
            return false;
        Optional<object?> value = unwrapped.ConstantValue;
        return value.HasValue && value.Value switch
        {
            int number => number is -1 or int.MaxValue,
            long number => number == -1,
            short number => number == -1,
            sbyte number => number == -1,
            _ => false,
        };
    }

    /// <summary>Whether <paramref name="operation"/> is the integer constant <paramref name="expected"/>, through any conversion.</summary>
    private static bool IsConstant(IOperation operation, int expected)
    {
        Optional<object?> value = Unwrapped(operation).ConstantValue;
        return value.HasValue && value.Value is int number && number == expected;
    }

    /// <summary><paramref name="operation"/> with its conversions removed.</summary>
    private static IOperation Unwrapped(IOperation operation)
    {
        while (operation is IConversionOperation conversion)
            operation = conversion.Operand;
        return operation;
    }
}
