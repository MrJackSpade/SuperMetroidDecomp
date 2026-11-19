using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// An order that matters is stated as data (SME6469, #2567): in <c>SuperMetroid.Core</c> and
/// <c>SuperMetroid.AssetExtraction</c>, no sequence whose order is an enum's declaration or a dictionary's enumeration
/// feeds an order. Three sources are followed: <c>Enum.GetValues</c> (either overload, and
/// <c>GetValuesAsUnderlyingType</c>), which lists an enum's members by their backing values; <c>Order</c>,
/// <c>OrderBy</c>, <c>ThenBy</c> and their descending forms, or a <c>Sort</c>, that order by an enum without a comparer;
/// and a dictionary, or its <c>Keys</c> or <c>Values</c>, enumerated as a sequence. The sequence is followed through the
/// LINQ operators that keep its order (<c>Select</c>, <c>Where</c>, <c>SelectMany</c> and its selector's result,
/// <c>Concat</c>, <c>Zip</c>, <c>Except</c> and the like), collection expressions and the locals it is stored in.
/// <para>
/// It is order-free where it lands in a set or a dictionary (<c>ToDictionary</c>, <c>ToHashSet</c>, their frozen and
/// immutable forms, a set or dictionary constructed or typed from it), where only a count or a membership is asked
/// (<c>Length</c>, <c>Count</c>, <c>Any</c>, <c>All</c>, <c>Contains</c>, <c>Sum</c>, <c>Min</c>, <c>Max</c>), where it
/// is sorted again by another key, where an <c>Aggregate</c> combines it with a bitwise operator, and where a
/// <c>foreach</c> over it has no ordered effect. A loop's ordered effects are a <c>yield return</c>, a <c>break</c> or a
/// <c>return</c> out of the loop (the first match wins), an <c>Add</c>, <c>Insert</c>, <c>Push</c>, <c>Enqueue</c>,
/// <c>Append</c> or <c>Write</c> on a collection or writer that is not a set or a dictionary and outlives the iteration,
/// such a collection or writer handed to a call, and an assignment, compound assignment or increment of a variable
/// that outlives the iteration.
/// </para>
/// <para>
/// Everything else feeds an order and is reported: an array or list built from it, a member or method result exposing
/// it, an element read by position, a <c>First</c>, a <c>SequenceEqual</c>, a call handed it. Reordering an enum's
/// members, or the inserts into a dictionary, then silently reorders an installed file, a sheet, a document or a
/// behaviour. The order is written out instead: an explicit list (checked with <c>Ensure.EveryMemberOnce</c> where it
/// must hold every member), as <c>StoryEventKeys.SavedOrder</c> and <c>EndingShadeLayeringDefinitions</c> do (#2543).
/// </para>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class EnumerationOrderAnalyzer : DiagnosticAnalyzer
{
    /// <summary>An enum's declaration order or a dictionary's enumeration order feeds an order.</summary>
    public const string EnumerationOrderId = "SME6469";

    /// <summary>The import pipeline, which writes the installed files whose order the rule protects.</summary>
    private const string ExtractionAssembly = "SuperMetroid.AssetExtraction";

    /// <summary>LINQ operators whose result keeps the order of the sequences handed to them.</summary>
    private static readonly ImmutableHashSet<string> OrderKeeping =
    [
        "Select", "Where", "SelectMany", "Cast", "OfType", "Concat", "Prepend", "Append", "Zip", "Distinct", "DistinctBy",
        "Except", "ExceptBy", "Intersect", "IntersectBy", "Union", "UnionBy", "Skip", "SkipLast", "SkipWhile", "Take",
        "TakeLast", "TakeWhile", "Reverse", "AsEnumerable", "DefaultIfEmpty", "Chunk", "Index", "GroupBy",
    ];

    /// <summary>Calls that ask only a count, a membership or an extreme of a sequence, or file it in a set or a lookup.</summary>
    private static readonly ImmutableHashSet<string> OrderFree =
    [
        "ToDictionary", "ToFrozenDictionary", "ToImmutableDictionary", "ToHashSet", "ToFrozenSet", "ToImmutableHashSet",
        "ToLookup", "All", "Any", "Count", "LongCount", "Contains", "Sum", "Min", "Max", "Average", "SetEquals",
        "IsSubsetOf", "IsSupersetOf", "IsProperSubsetOf", "IsProperSupersetOf", "Overlaps", "UnionWith", "ExceptWith",
        "IntersectWith", "SymmetricExceptWith",
    ];

    /// <summary>Calls that sort a sequence again, so its incoming order no longer shows.</summary>
    private static readonly ImmutableHashSet<string> Sorts = ["Order", "OrderBy", "OrderDescending", "OrderByDescending"];

    /// <summary>The sorts that order by a key, and the stable refinements of a sort.</summary>
    private static readonly ImmutableHashSet<string> KeyedSorts = ["OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending"];

    /// <summary>Calls on a collection or writer that put an element after the ones already in it.</summary>
    private static readonly ImmutableHashSet<string> Accumulating =
    [
        "Add", "AddRange", "Insert", "InsertRange", "Push", "Enqueue", "Append", "AppendLine", "AppendFormat", "AppendJoin",
    ];

    /// <summary>The order-free sizes of a sequence.</summary>
    private static readonly ImmutableHashSet<string> Sizes = ["Length", "LongLength", "Count"];

    private static readonly DiagnosticDescriptor Rule = new(
        EnumerationOrderId,
        "An order is taken from a declaration or a dictionary's enumeration",
        "'{0}' takes its order from {1}; state the order as data (an explicit list), or use it only as a set, a lookup or a count",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "An enum's members are identities whose declaration order, like a dictionary's enumeration order, " +
            "carries nothing. A sequence of an enum's members from Enum.GetValues, a sort by an enum's values, or a " +
            "dictionary enumerated as a sequence that feeds an installed file's, a sheet's, a document's or a behaviour's " +
            "order makes reordering the members or the inserts silently change it. Where order matters it is written out " +
            "as an explicit list; elsewhere the sequence is used only as a set, a lookup or a count (#2567).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName is not (CoreLayerAnalyzer.CoreAssembly or ExtractionAssembly))
                return;
            INamedTypeSymbol? enumType = start.Compilation.GetSpecialType(SpecialType.System_Enum);
            start.RegisterOperationBlockAction(block => AnalyzeBlock(block, enumType));
        });
    }

    /// <summary>Reports every source in a block whose sequence feeds an order.</summary>
    private static void AnalyzeBlock(OperationBlockAnalysisContext context, INamedTypeSymbol? enumType)
    {
        IOperation[] operations = [.. context.OperationBlocks.SelectMany(block => block.DescendantsAndSelf())];
        foreach (IOperation operation in operations)
        {
            if (SourceOf(operation, enumType) is not { } source)
                continue;
            if (FeedsOrder(operation, operations, isSource: true, depth: 0))
                context.ReportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax.ToString(), source));
        }
    }

    /// <summary>What an operation takes its order from, or null when it is not a source.</summary>
    private static string? SourceOf(IOperation operation, INamedTypeSymbol? enumType)
    {
        if (operation is IInvocationOperation invocation)
        {
            IMethodSymbol method = invocation.TargetMethod;
            if (method.ContainingType is { } owner && SymbolEqualityComparer.Default.Equals(owner, enumType) &&
                method.Name is "GetValues" or "GetValuesAsUnderlyingType")
                return method.IsGenericMethod
                    ? $"the declaration of '{method.TypeArguments[0].Name}'"
                    : "an enum's declaration";
            if (IsLinq(method) && method.Name is "Order" or "OrderDescending" && ElementType(invocation.Type) is { TypeKind: TypeKind.Enum } element)
                return $"the values of '{element.Name}'";
            if (IsLinq(method) && KeyedSorts.Contains(method.Name) && method.Parameters.Length == 2 &&
                method.TypeArguments.Length == 2 && method.TypeArguments[1].TypeKind == TypeKind.Enum)
                return $"the values of '{method.TypeArguments[1].Name}'";
            if (method.Name == "Sort" && invocation.Arguments.Length <= 1 && SortedElement(invocation) is { TypeKind: TypeKind.Enum } sorted)
                return $"the values of '{sorted.Name}'";
            return null;
        }
        if (IsDictionaryEnumeration(operation))
            return "a dictionary's enumeration";
        return null;
    }

    /// <summary>The element type a parameterless <c>Sort</c> orders: its list's, or its one array argument's.</summary>
    private static ITypeSymbol? SortedElement(IInvocationOperation invocation)
    {
        if (invocation.Arguments.Length == 0)
            return invocation.Instance?.Type is INamedTypeSymbol { TypeArguments.Length: 1 } list ? list.TypeArguments[0] : null;
        return invocation.TargetMethod.IsStatic && Unwrapped(invocation.Arguments[0].Value).Type is IArrayTypeSymbol array
            ? array.ElementType
            : null;
    }

    /// <summary>
    /// Whether an operation is a dictionary, or a dictionary's <c>Keys</c> or <c>Values</c>, handed on as a sequence: a
    /// <c>foreach</c>'s collection, an argument converted to a sequence (a LINQ receiver among them) or a spread element.
    /// </summary>
    private static bool IsDictionaryEnumeration(IOperation operation)
    {
        bool dictionary = IsDictionary(operation.Type) ||
            operation is IPropertyReferenceOperation { Property.Name: "Keys" or "Values", Instance.Type: var owner } && IsDictionary(owner);
        if (!dictionary || operation.Parent is IPropertyReferenceOperation { Property.Name: "Keys" or "Values" })
            return false;
        IOperation current = operation;
        while (current.Parent is IConversionOperation or IParenthesizedOperation)
            current = current.Parent;
        return current.Parent switch
        {
            IForEachLoopOperation loop => loop.Collection == current,
            ISpreadOperation => true,
            IArgumentOperation { Parameter.Type: var parameter } => IsSequenceInterface(parameter),
            _ => false,
        };
    }

    /// <summary>
    /// Whether the sequence an operation produces feeds an order, following it up through the operations that keep its
    /// order and the locals it is stored in.
    /// </summary>
    private static bool FeedsOrder(IOperation operation, IOperation[] block, bool isSource, int depth)
    {
        if (depth > 16)
            return true;
        IOperation current = operation;
        while (true)
        {
            if (!isSource && IsSetOrDictionary(current.Type))
                return false;
            isSource = false;
            IOperation? parent = current.Parent;
            switch (parent)
            {
                case IConversionOperation conversion:
                    current = conversion;
                    continue;
                case IParenthesizedOperation parenthesized:
                    current = parenthesized;
                    continue;
                case IArgumentOperation { Parent: IInvocationOperation invocation } argument:
                    if (Consumes(invocation, argument) is { } feeds)
                        return feeds;
                    current = invocation;
                    continue;
                case IArgumentOperation { Parent: IObjectCreationOperation creation }:
                    return !IsSetOrDictionary(creation.Type);
                case IInvocationOperation { Instance: var instance } instanceCall when instance == current:
                    return !OrderFree.Contains(instanceCall.TargetMethod.Name) && instanceCall.TargetMethod.Name is not
                        ("ContainsKey" or "ContainsValue" or "TryGetValue" or "Remove");
                case IPropertyReferenceOperation { Property: var property, Instance: var instance } when instance == current:
                    return !Sizes.Contains(property.Name);
                case ISpreadOperation spread when spread.Parent is ICollectionExpressionOperation collection:
                    current = collection;
                    continue;
                case IForEachLoopOperation loop when loop.Collection == current:
                    return HasOrderedEffect(loop);
                case IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Symbol: var local } }:
                    return LocalFeedsOrder(local, block, depth);
                case ISimpleAssignmentOperation { Target: ILocalReferenceOperation { Local: var local } } assignment
                    when assignment.Value == current:
                    return LocalFeedsOrder(local, block, depth);
                case ISimpleAssignmentOperation { Target: var target } assignment when assignment.Value == current:
                    return !IsSetOrDictionary(target.Type);
                case IReturnOperation returned when IsSelectManySelectorResult(returned) is { } selectMany:
                    current = selectMany;
                    continue;
                case IReturnOperation:
                    return !IsSetOrDictionary(ReturnedType(parent));
                default:
                    return true;
            }
        }
    }

    /// <summary>
    /// What a call does with the sequence one of its arguments carries: null when its result keeps the order and is
    /// followed further, false when the call is order-free, true when it feeds an order.
    /// </summary>
    private static bool? Consumes(IInvocationOperation invocation, IArgumentOperation argument)
    {
        IMethodSymbol method = invocation.TargetMethod;
        string name = method.Name;
        if (!IsSequenceInterface(argument.Parameter?.Type) && !IsLinq(method))
            return true;
        if (IsLinq(method) && OrderKeeping.Contains(name))
            return null;
        if (OrderFree.Contains(name) || Sorts.Contains(name))
            return false;
        if (name == "Aggregate" && IsBitwiseCombination(invocation))
            return false;
        return true;
    }

    /// <summary>Whether an <c>Aggregate</c>'s combining lambda is a bitwise or, and or exclusive or of its two parameters.</summary>
    private static bool IsBitwiseCombination(IInvocationOperation invocation) =>
        invocation.Arguments.LastOrDefault()?.Value is { } last &&
        Unwrapped(last) is IDelegateCreationOperation { Target: IAnonymousFunctionOperation lambda } &&
        lambda.Body.Operations is [IReturnOperation { ReturnedValue: { } value }] &&
        Unwrapped(value) is IBinaryOperation
        {
            OperatorKind: BinaryOperatorKind.Or or BinaryOperatorKind.And or BinaryOperatorKind.ExclusiveOr,
        };

    /// <summary>The <c>SelectMany</c> whose selector lambda returns the value of <paramref name="returned"/>, if any.</summary>
    private static IInvocationOperation? IsSelectManySelectorResult(IReturnOperation returned)
    {
        IOperation? scope = returned.Parent;
        while (scope is not null and not IAnonymousFunctionOperation)
            scope = scope is IBlockOperation ? scope.Parent : null;
        if (scope is not IAnonymousFunctionOperation lambda)
            return null;
        IOperation? up = lambda.Parent;
        while (up is IDelegateCreationOperation or IConversionOperation)
            up = up.Parent;
        return up is IArgumentOperation { Parent: IInvocationOperation { TargetMethod.Name: "SelectMany" } selectMany } && IsLinq(selectMany.TargetMethod)
            ? selectMany
            : null;
    }

    /// <summary>The type the member or function a <c>return</c> leaves declares it returns.</summary>
    private static ITypeSymbol? ReturnedType(IOperation returned)
    {
        IOperation? scope = returned.Parent;
        while (scope is not null)
        {
            if (scope is IAnonymousFunctionOperation lambda)
                return lambda.Symbol.ReturnType;
            if (scope is ILocalFunctionOperation local)
                return local.Symbol.ReturnType;
            scope = scope.Parent;
        }
        return returned.SemanticModel?.GetEnclosingSymbol(returned.Syntax.SpanStart) switch
        {
            IMethodSymbol method => method.ReturnType,
            IPropertySymbol property => property.Type,
            _ => null,
        };
    }

    /// <summary>Whether any read of a local holding the sequence feeds an order.</summary>
    private static bool LocalFeedsOrder(ILocalSymbol local, IOperation[] block, int depth)
    {
        if (IsSetOrDictionary(local.Type))
            return false;
        foreach (IOperation operation in block)
        {
            if (operation is ILocalReferenceOperation { IsDeclaration: false } read &&
                SymbolEqualityComparer.Default.Equals(read.Local, local) &&
                !(read.Parent is ISimpleAssignmentOperation assignment && assignment.Target == read) &&
                FeedsOrder(read, block, isSource: true, depth + 1))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Whether a loop's body does something whose result depends on the order it visits the elements: a yield return, a
    /// break or return out of it, an accumulation into a collection or writer that outlives the iteration, or a write of a
    /// variable that does.
    /// </summary>
    private static bool HasOrderedEffect(IForEachLoopOperation loop)
    {
        Microsoft.CodeAnalysis.Text.TextSpan within = loop.Syntax.Span;
        bool Outlives(ISymbol? symbol) => symbol switch
        {
            ILocalSymbol local => local.DeclaringSyntaxReferences.All(reference => !within.Contains(reference.Span)),
            IParameterSymbol parameter => parameter.DeclaringSyntaxReferences.All(reference => !within.Contains(reference.Span)),
            IFieldSymbol or IPropertySymbol => true,
            _ => false,
        };
        foreach (IOperation operation in loop.Body.DescendantsAndSelf())
        {
            switch (operation)
            {
                case IReturnOperation { Kind: ReturnKind.YieldReturn }:
                    return true;
                case IReturnOperation returned when !InsideFunction(returned, loop):
                    return true;
                case IBranchOperation { BranchKind: BranchKind.Break } branch when BreaksOut(branch, loop):
                    return true;
                case IInvocationOperation call when Accumulating.Contains(call.TargetMethod.Name) || call.TargetMethod.Name.StartsWith("Write", StringComparison.Ordinal):
                    if (call.Instance is { } receiver && !IsSetOrDictionary(receiver.Type) && Outlives(Referenced(receiver)))
                        return true;
                    break;
                case IArgumentOperation { Value: var value } when IsAccumulator(Unwrapped(value).Type) && Outlives(Referenced(Unwrapped(value))):
                    return true;
                case ICompoundAssignmentOperation compound when Outlives(Referenced(compound.Target)):
                    return true;
                case IIncrementOrDecrementOperation step when Outlives(Referenced(step.Target)):
                    return true;
                case ISimpleAssignmentOperation { Target: ILocalReferenceOperation or IFieldReferenceOperation or IParameterReferenceOperation } assignment
                    when Outlives(Referenced(assignment.Target)):
                    return true;
                default:
                    break;
            }
        }
        return false;
    }

    /// <summary>Whether a return lies in a lambda or local function nested in the loop, and so does not leave it.</summary>
    private static bool InsideFunction(IOperation operation, IForEachLoopOperation loop)
    {
        for (IOperation? up = operation.Parent; up is not null && up != loop; up = up.Parent)
            if (up is IAnonymousFunctionOperation or ILocalFunctionOperation)
                return true;
        return false;
    }

    /// <summary>Whether a break leaves <paramref name="loop"/> rather than a nested loop or switch.</summary>
    private static bool BreaksOut(IBranchOperation branch, IForEachLoopOperation loop)
    {
        for (IOperation? up = branch.Parent; up is not null; up = up.Parent)
        {
            if (up == loop)
                return true;
            if (up is ILoopOperation or ISwitchOperation or IAnonymousFunctionOperation or ILocalFunctionOperation)
                return false;
        }
        return false;
    }

    /// <summary>The variable, parameter, field or property an operation reads, if it reads one directly.</summary>
    private static ISymbol? Referenced(IOperation operation) => Unwrapped(operation) switch
    {
        ILocalReferenceOperation local => local.Local,
        IParameterReferenceOperation parameter => parameter.Parameter,
        IFieldReferenceOperation field => field.Field,
        IPropertyReferenceOperation { Property.IsIndexer: false } property => property.Property,
        _ => null,
    };

    /// <summary>
    /// Whether a type is a collection or writer elements are put into in order: a list, a builder, a queue or stack, a
    /// string builder, a text writer, a stream or a JSON writer, but not a set, a dictionary or an array.
    /// </summary>
    private static bool IsAccumulator(ITypeSymbol? type)
    {
        if (type is null || IsSetOrDictionary(type) || type.TypeKind == TypeKind.Array)
            return false;
        if (type is INamedTypeSymbol { TypeKind: TypeKind.Interface } face)
            return face.OriginalDefinition.ToDisplayString() is "System.Collections.Generic.IList<T>" or "System.Collections.Generic.ICollection<T>";
        for (ITypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            if (current.OriginalDefinition.ToDisplayString() is "System.Collections.Generic.List<T>" or
                "System.Collections.Generic.LinkedList<T>" or "System.Collections.Generic.Queue<T>" or
                "System.Collections.Generic.Stack<T>" or "System.Collections.ObjectModel.Collection<T>" or
                "System.Collections.Immutable.ImmutableArray<T>.Builder" or "System.Collections.Immutable.ImmutableList<T>.Builder" or
                "System.Text.StringBuilder" or "System.IO.TextWriter" or "System.IO.Stream" or "System.IO.BinaryWriter" or
                "System.Text.Json.Utf8JsonWriter")
                return true;
        }
        return false;
    }

    /// <summary>Whether a method is one of LINQ's <c>Enumerable</c> operators.</summary>
    private static bool IsLinq(IMethodSymbol method) =>
        method.ContainingType?.ToDisplayString() is "System.Linq.Enumerable" or "System.Linq.ImmutableArrayExtensions";

    /// <summary>The element type of a sequence type.</summary>
    private static ITypeSymbol? ElementType(ITypeSymbol? type) => type switch
    {
        IArrayTypeSymbol array => array.ElementType,
        INamedTypeSymbol named => named.AllInterfaces.Concat(named.TypeKind == TypeKind.Interface ? [named] : [])
            .FirstOrDefault(face => face.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)?.TypeArguments[0],
        _ => null,
    };

    /// <summary>Whether a parameter type is a sequence interface a value is enumerated through.</summary>
    private static bool IsSequenceInterface(ITypeSymbol? type) =>
        type is INamedTypeSymbol { TypeKind: TypeKind.Interface } named &&
        (named.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T ||
            named.SpecialType == SpecialType.System_Collections_IEnumerable);

    /// <summary>Whether a type is a dictionary: it implements a generic dictionary interface.</summary>
    private static bool IsDictionary(ITypeSymbol? type) => type is not null && Implements(type,
        "System.Collections.Generic.IDictionary<TKey, TValue>", "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>");

    /// <summary>Whether a type is a set or a dictionary, whose order nothing reads.</summary>
    private static bool IsSetOrDictionary(ITypeSymbol? type) => type is not null && Implements(type,
        "System.Collections.Generic.IDictionary<TKey, TValue>", "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>",
        "System.Collections.Generic.ISet<T>", "System.Collections.Generic.IReadOnlySet<T>", "System.Linq.ILookup<TKey, TElement>");

    /// <summary>Whether a type is, or implements, one of the named generic interfaces.</summary>
    private static bool Implements(ITypeSymbol type, params string[] interfaces)
    {
        IEnumerable<INamedTypeSymbol> faces = type is INamedTypeSymbol { TypeKind: TypeKind.Interface } self
            ? type.AllInterfaces.Prepend(self)
            : type.AllInterfaces;
        return faces.Any(face => interfaces.Contains(face.OriginalDefinition.ToDisplayString()));
    }

    /// <summary>The operation beneath any conversions and parentheses.</summary>
    private static IOperation Unwrapped(IOperation operation)
    {
        while (true)
        {
            if (operation is IConversionOperation conversion)
                operation = conversion.Operand;
            else if (operation is IParenthesizedOperation parenthesized)
                operation = parenthesized.Operand;
            else
                return operation;
        }
    }
}
