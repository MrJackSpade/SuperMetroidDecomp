using SuperMetroid.EnsureAnalyzer.SourceLayout;
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
/// and a dictionary keyed by identities (an enum, or a generic catalog's type parameter), or its <c>Keys</c> or
/// <c>Values</c>, enumerated as a sequence: it enumerates in the order its entries were inserted, the order a definition
/// wrote its identities in. A dictionary keyed by names or numbers is a lookup and is not followed. The sequence is followed through the
/// LINQ operators that keep its order (<c>Select</c>, <c>Where</c>, <c>SelectMany</c> and its selector's result,
/// <c>Concat</c>, <c>Zip</c>, <c>Except</c> and the like), collection expressions and the locals it is stored in.
/// <para>
/// It is order-free where it lands in a set or a dictionary (<c>ToDictionary</c>, <c>ToHashSet</c>, their frozen and
/// immutable forms, a set or dictionary constructed or typed from it), where only a count or a membership is asked
/// (<c>Length</c>, <c>Count</c>, <c>Any</c>, <c>All</c>, <c>Contains</c>, <c>Sum</c>, <c>Min</c>, <c>Max</c>), where it
/// is sorted again by another key, where a <c>Single</c> picks its one match, where it only spells a thrown exception's
/// message, where an <c>Aggregate</c> combines it with a bitwise operator, and where a
/// <c>foreach</c> over it has no ordered effect. A loop's ordered effects are a <c>yield return</c>, a <c>break</c> or a
/// <c>return</c> out of the loop (the first match wins), an <c>Add</c>, <c>Insert</c>, <c>Push</c>, <c>Enqueue</c>,
/// <c>Append</c> or <c>Write</c> on a collection or writer that is not a set or a dictionary and outlives the iteration,
/// such a collection or writer handed to a call that takes it as one, and, of a variable that outlives the iteration, an assignment of a
/// computed value or a compound assignment or increment the loop's body also reads (a running offset; a count only
/// accumulated is order-free). A dictionary handed whole to a callee as a dictionary, a serializer among them, is the
/// callee's to enumerate and is not followed.
/// </para>
/// <para>
/// A sequence handed to a parameter, stored in a field or property, or returned from a method or property of the same
/// assembly is followed into every read of that symbol across the compilation, through the interface members it
/// implements and the members it overrides: entries filed only into a lookup by the function that receives them, writers
/// a caller only unions into sorted sets, and a registry's list read only to file each entry by key are order-free. A
/// call that only checks its argument (<c>Ensure.NotNull</c>) hands it on, and a value whose result is discarded feeds
/// nothing. A parameter of a function the compilation does not read, and a member or method the assembly never reads,
/// are taken to feed an order, as another assembly may read them.
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
        "ToLookup", "All", "Any", "Count", "LongCount", "Contains", "Single", "SingleOrDefault", "Sum", "Min", "Max", "Average", "SetEquals",
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
            "as an explicit list; elsewhere the sequence is used only as a set, a lookup or a count (#2567).",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

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
            IAssemblySymbol assembly = start.Compilation.Assembly;
            var flows = new OrderFlows();
            start.RegisterOperationBlockAction(block => AnalyzeBlock(block, enumType, assembly, flows));
            start.RegisterCompilationEndAction(end => flows.Report(end.ReportDiagnostic));
        });
    }

    /// <summary>
    /// Records every source in a block with what its sequence feeds, and how each read of a parameter, member or method
    /// result of <paramref name="assembly"/> in the block uses the sequence it holds.
    /// </summary>
    private static void AnalyzeBlock(OperationBlockAnalysisContext context, INamedTypeSymbol? enumType, IAssemblySymbol assembly, OrderFlows flows)
    {
        // Code a source generator emits, such as a debugger state's writer of every field, orders nothing of the game (#2602).
        if (context.OperationBlocks.FirstOrDefault()?.Syntax.SyntaxTree is { } tree && GeneratedSource.IsSourceGeneratorOutput(tree, context.CancellationToken))
            return;
        IOperation[] operations = [.. context.OperationBlocks.SelectMany(block => block.DescendantsAndSelf())];
        foreach (IOperation operation in operations)
        {
            if (SourceOf(operation, enumType) is not { } source)
                continue;
            // A list sorted in place by an enum's values is sorted only for its order.
            OrderVerdict verdict = operation is IInvocationOperation { TargetMethod.Name: "Sort" }
                ? OrderVerdict.Feeds
                : FeedsOrder(operation, operations, isSource: true, depth: 0, assembly);
            if (verdict.FeedsAtOnce || verdict.Conditions.Count != 0)
                flows.Pending(Diagnostic.Create(Rule, operation.Syntax.GetLocation(), Shown(operation.Syntax), source), verdict);
        }
        IEnumerable<IParameterSymbol> parameters = context.OwningSymbol is IMethodSymbol owner ? owner.Parameters : [];
        parameters = parameters.Concat(operations.OfType<ILocalFunctionOperation>().SelectMany(function => function.Symbol.Parameters));
        foreach (IParameterSymbol parameter in parameters)
        {
            if (IsTracked(parameter.Type))
                flows.Body(parameter.OriginalDefinition);
        }
        foreach (IOperation operation in operations)
        {
            ISymbol? read = operation switch
            {
                IInvocationOperation invocation when IsTracked(invocation.Type) => Normalized(invocation.TargetMethod),
                IPropertyReferenceOperation { Property.IsIndexer: false } property when IsTracked(property.Type) => Normalized(property.Property),
                IFieldReferenceOperation field when IsTracked(field.Type) => field.Field.OriginalDefinition,
                IParameterReferenceOperation parameter when IsTracked(parameter.Type) &&
                    parameter.Parameter.ContainingSymbol is not IMethodSymbol { MethodKind: MethodKind.AnonymousFunction } => parameter.Parameter.OriginalDefinition,
                _ => null,
            };
            if (read is null || !InAssembly(read, assembly) || operation.Parent is ISimpleAssignmentOperation assignment && assignment.Target == operation)
                continue;
            flows.Use(read, FeedsOrder(operation, operations, isSource: true, depth: 0, assembly));
        }
    }

    /// <summary>Whether a symbol is declared in source of <paramref name="assembly"/>.</summary>
    private static bool InAssembly(ISymbol symbol, IAssemblySymbol assembly) =>
        SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, assembly) && symbol.Locations.Any(location => location.IsInSource);

    /// <summary>The symbol a read is recorded under: a property's accessor as its property, a generic as its definition.</summary>
    private static ISymbol Normalized(ISymbol symbol) =>
        symbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol property } ? property.OriginalDefinition : symbol.OriginalDefinition;

    /// <summary>Whether a value of a type carries an order the rule follows: a sequence that is not a set, a dictionary or text.</summary>
    private static bool IsTracked(ITypeSymbol? type) =>
        type is not null && type.SpecialType != SpecialType.System_String && !IsSetOrDictionary(type) &&
        (type is IArrayTypeSymbol || Implements(type, "System.Collections.Generic.IEnumerable<T>") ||
            type.AllInterfaces.Any(face => face.SpecialType == SpecialType.System_Collections_IEnumerable));

    /// <summary>The source an operation spans as the message shows it: its first line, at most eighty characters.</summary>
    private static string Shown(SyntaxNode syntax)
    {
        string text = syntax.ToString();
        int end = text.IndexOfAny(['\r', '\n']);
        if (end >= 0)
            text = text.Substring(0, end).TrimEnd() + " ...";
        return text.Length <= 80 ? text : text.Substring(0, 77) + "...";
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
        bool dictionary = IsIdentityDictionary(operation.Type) ||
            operation is IPropertyReferenceOperation { Property.Name: "Keys" or "Values", Instance.Type: var owner } && IsIdentityDictionary(owner);
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
    private static OrderVerdict FeedsOrder(IOperation operation, IOperation[] block, bool isSource, int depth, IAssemblySymbol assembly)
    {
        if (depth > 16)
            return OrderVerdict.Feeds;
        if (InsideThrow(operation))
            return OrderVerdict.Free;
        IOperation current = operation;
        while (true)
        {
            if (!isSource && IsSetOrDictionary(current.Type))
                return OrderVerdict.Free;
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
                    if (Consumes(invocation, argument, assembly) is { } feeds)
                        return feeds;
                    current = invocation;
                    continue;
                case IArgumentOperation { Parent: IObjectCreationOperation creation } argument:
                    if (IsSetOrDictionary(creation.Type))
                        return OrderVerdict.Free;
                    return creation.Constructor is { } constructor && InAssembly(constructor.OriginalDefinition, assembly) && argument.Parameter is { } parameter
                        ? OrderVerdict.DependsOn(parameter.OriginalDefinition)
                        : OrderVerdict.Feeds;
                case IInvocationOperation { Instance: var instance } instanceCall when instance == current:
                    return !OrderFree.Contains(instanceCall.TargetMethod.Name) && instanceCall.TargetMethod.Name is not
                        ("ContainsKey" or "ContainsValue" or "TryGetValue" or "Remove") ? OrderVerdict.Feeds : OrderVerdict.Free;
                case IPropertyReferenceOperation { Property: var property, Instance: var instance } when instance == current:
                    return Sizes.Contains(property.Name) ? OrderVerdict.Free : OrderVerdict.Feeds;
                case ISpreadOperation spread when spread.Parent is ICollectionExpressionOperation collection:
                    current = collection;
                    continue;
                case IForEachLoopOperation loop when loop.Collection == current:
                    return HasOrderedEffect(loop) ? OrderVerdict.Feeds : OrderVerdict.Free;
                case IVariableInitializerOperation { Parent: IVariableDeclaratorOperation { Symbol: var local } }:
                    return LocalFeedsOrder(local, block, depth, assembly);
                case ISimpleAssignmentOperation { Target: ILocalReferenceOperation { Local: var local } } assignment
                    when assignment.Value == current:
                    return LocalFeedsOrder(local, block, depth, assembly);
                case ISimpleAssignmentOperation { Target: var target } assignment when assignment.Value == current:
                    return Stored(Referenced(target), target.Type, assembly);
                case IFieldInitializerOperation fieldInitializer when fieldInitializer.InitializedFields.Length != 0:
                    return Stored(fieldInitializer.InitializedFields[0], fieldInitializer.InitializedFields[0].Type, assembly);
                case IPropertyInitializerOperation propertyInitializer when propertyInitializer.InitializedProperties.Length != 0:
                    return Stored(propertyInitializer.InitializedProperties[0], propertyInitializer.InitializedProperties[0].Type, assembly);
                case IExpressionStatementOperation:
                    return OrderVerdict.Free;
                case IReturnOperation returned when IsSelectManySelectorResult(returned) is { } selectMany:
                    current = selectMany;
                    continue;
                case IReturnOperation returned:
                    return Returned(returned, assembly);
                default:
                    return OrderVerdict.Feeds;
            }
        }
    }

    /// <summary>
    /// What a sequence stored in <paramref name="member"/> feeds: nothing in a set or a dictionary, what the member's reads
    /// do with it for a field or property of the assembly, and otherwise an order.
    /// </summary>
    private static OrderVerdict Stored(ISymbol? member, ITypeSymbol? type, IAssemblySymbol assembly)
    {
        if (IsSetOrDictionary(type))
            return OrderVerdict.Free;
        return member is IFieldSymbol or IPropertySymbol && InAssembly(member.OriginalDefinition, assembly)
            ? OrderVerdict.DependsOn(Normalized(member))
            : OrderVerdict.Feeds;
    }

    /// <summary>
    /// What a sequence a <c>return</c> hands back feeds: nothing as a set or a dictionary, an order out of a lambda, and
    /// otherwise what the calls of its local function, method or property do with it.
    /// </summary>
    private static OrderVerdict Returned(IReturnOperation returned, IAssemblySymbol assembly)
    {
        if (IsSetOrDictionary(ReturnedType(returned)))
            return OrderVerdict.Free;
        for (IOperation? scope = returned.Parent; scope is not null; scope = scope.Parent)
        {
            if (scope is IAnonymousFunctionOperation)
                return OrderVerdict.Feeds;
            if (scope is ILocalFunctionOperation local)
                return OrderVerdict.DependsOn(local.Symbol.OriginalDefinition);
        }
        return returned.SemanticModel?.GetEnclosingSymbol(returned.Syntax.SpanStart) is { } enclosing &&
            enclosing is IMethodSymbol or IPropertySymbol && InAssembly(enclosing, assembly)
            ? OrderVerdict.DependsOn(Normalized(enclosing))
            : OrderVerdict.Feeds;
    }

    /// <summary>
    /// Whether an operation lies in a thrown exception: an order that only spells a failure's message feeds no installed
    /// or behavioural order.
    /// </summary>
    private static bool InsideThrow(IOperation operation)
    {
        for (IOperation? up = operation.Parent; up is not null; up = up.Parent)
        {
            if (up is IThrowOperation)
                return true;
        }
        return false;
    }

    /// <summary>
    /// What a call does with the sequence one of its arguments carries: null when its result keeps the order, or is the
    /// argument handed back by a check, and is followed further; order-free for a count, a membership, a set, a lookup or a
    /// resort; what the parameter's reads do for a function of the assembly; and otherwise an order.
    /// </summary>
    private static OrderVerdict? Consumes(IInvocationOperation invocation, IArgumentOperation argument, IAssemblySymbol assembly)
    {
        IMethodSymbol method = invocation.TargetMethod;
        string name = method.Name;
        if (name == "NotNull" && method.ContainingType?.Name == "Ensure")
            return null;
        if (IsLinq(method) && OrderKeeping.Contains(name))
            return null;
        if (IsLinq(method) || IsSequenceInterface(argument.Parameter?.Type))
        {
            if (OrderFree.Contains(name) || Sorts.Contains(name))
                return OrderVerdict.Free;
            if (name == "Aggregate" && IsBitwiseCombination(invocation))
                return OrderVerdict.Free;
        }
        return InAssembly(method.OriginalDefinition, assembly) && argument.Parameter is { } parameter
            ? OrderVerdict.DependsOn(parameter.OriginalDefinition)
            : OrderVerdict.Feeds;
    }

    /// <summary>Whether an <c>Aggregate</c>'s combining lambda is a bitwise or, and or exclusive or of its two parameters.</summary>
    private static bool IsBitwiseCombination(IInvocationOperation invocation) =>
        invocation.Arguments.LastOrDefault()?.Value is { } last &&
        Unwrapped(last) is IDelegateCreationOperation { Target: IAnonymousFunctionOperation lambda } &&
        lambda.Body.Operations.Length == 1 && lambda.Body.Operations[0] is IReturnOperation { ReturnedValue: { } value } &&
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

    /// <summary>What the reads of a local holding the sequence feed, together.</summary>
    private static OrderVerdict LocalFeedsOrder(ILocalSymbol local, IOperation[] block, int depth, IAssemblySymbol assembly)
    {
        if (IsSetOrDictionary(local.Type))
            return OrderVerdict.Free;
        OrderVerdict verdict = OrderVerdict.Free;
        foreach (IOperation operation in block)
        {
            if (operation is ILocalReferenceOperation { IsDeclaration: false } read &&
                SymbolEqualityComparer.Default.Equals(read.Local, local) &&
                !(read.Parent is ISimpleAssignmentOperation assignment && assignment.Target == read))
            {
                verdict = verdict.With(FeedsOrder(read, block, isSource: true, depth + 1, assembly));
                if (verdict.FeedsAtOnce)
                    return verdict;
            }
        }
        return verdict;
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
                case IReturnOperation { Kind: OperationKind.YieldReturn }:
                    return true;
                case IReturnOperation returned when !InsideFunction(returned, loop):
                    return true;
                case IBranchOperation { BranchKind: BranchKind.Break } branch when BreaksOut(branch, loop):
                    return true;
                case IInvocationOperation call when Accumulating.Contains(call.TargetMethod.Name) || call.TargetMethod.Name.StartsWith("Write", StringComparison.Ordinal):
                    if (call.Instance is { } receiver && !IsSetOrDictionary(receiver.Type) && Outlives(Referenced(receiver)))
                        return true;
                    break;
                case IArgumentOperation { Value: var value, Parameter.Type: var parameter } when IsAccumulator(parameter) &&
                    IsAccumulator(Unwrapped(value).Type) && Outlives(Referenced(Unwrapped(value))):
                    return true;
                case ICompoundAssignmentOperation compound when Outlives(Referenced(compound.Target)) &&
                    (compound.Type?.SpecialType == SpecialType.System_String || ReadElsewhere(loop, compound)):
                    return true;
                case IIncrementOrDecrementOperation step when Outlives(Referenced(step.Target)) && ReadElsewhere(loop, step):
                    return true;
                case ISimpleAssignmentOperation { Target: ILocalReferenceOperation or IFieldReferenceOperation or IParameterReferenceOperation } assignment
                    when Outlives(Referenced(assignment.Target)) && !assignment.Value.ConstantValue.HasValue:
                    return true;
                default:
                    break;
            }
        }
        return false;
    }

    /// <summary>
    /// Whether the variable <paramref name="write"/> accumulates into is read anywhere else in the loop's body: a running
    /// count or offset an iteration reads depends on the iterations before it, while one only accumulated does not.
    /// </summary>
    private static bool ReadElsewhere(IForEachLoopOperation loop, IOperation write)
    {
        ISymbol? written = write switch
        {
            ICompoundAssignmentOperation compound => Referenced(compound.Target),
            IIncrementOrDecrementOperation step => Referenced(step.Target),
            _ => null,
        };
        if (written is null)
            return false;
        var own = new HashSet<IOperation>(write.DescendantsAndSelf());
        return loop.Body.DescendantsAndSelf().Any(other => !own.Contains(other) &&
            other is ILocalReferenceOperation or IParameterReferenceOperation or IFieldReferenceOperation or IPropertyReferenceOperation &&
            SymbolEqualityComparer.Default.Equals(Referenced(other), written));
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

    /// <summary>
    /// Whether a type is a dictionary keyed by identities: its key an enum, or a type parameter a generic catalog keys by
    /// identities. Such a dictionary enumerates in the order its entries were inserted, the order a definition wrote its
    /// identities in or a declaration listed them; a dictionary keyed by names or numbers is a lookup read by key.
    /// </summary>
    private static bool IsIdentityDictionary(ITypeSymbol? type)
    {
        if (type is null)
            return false;
        IEnumerable<INamedTypeSymbol> faces = type is INamedTypeSymbol { TypeKind: TypeKind.Interface } self
            ? type.AllInterfaces.Prepend(self)
            : type.AllInterfaces;
        return faces.Any(face => face.OriginalDefinition.ToDisplayString() is "System.Collections.Generic.IDictionary<TKey, TValue>"
                or "System.Collections.Generic.IReadOnlyDictionary<TKey, TValue>" &&
            face.TypeArguments[0].TypeKind is TypeKind.Enum or TypeKind.TypeParameter);
    }

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
