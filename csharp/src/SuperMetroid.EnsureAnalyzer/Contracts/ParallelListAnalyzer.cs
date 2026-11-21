using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// State and data are paired by identity, never by list position (SME6487, #2565). In <c>SuperMetroid.Core</c> and
/// <c>SuperMetroid.AssetExtraction</c>, outside an <c>[EncodingOwner]</c> type and a <c>[CompatibilityService]</c>, the rule
/// reports one collection indexed by another's index: a <c>Zip</c> of two collections that are not one collection
/// (<c>frames.Zip(durations)</c>); an element read whose index is found in another collection
/// (<c>colours[names.IndexOf(name)]</c>); a projection's index reading another collection
/// (<c>looks.Select((look, place) =&gt; uploads[place])</c>); and a loop that reads two collections at its own index, one of them a
/// static definition table (<c>for (int i = 0; ...; i++) Use(Shades[i], colours[i])</c>).
/// </summary>
/// <remarks>
/// A pairing is one record holding both, a keyed lookup or a reference, so nothing has to keep two lists in step. Positions
/// within one authored sequence are not pairings and are left alone: a run's frames, a cycle's steps, a path's progress,
/// neighbouring entries of one list (<c>list.Zip(list.Skip(1))</c>), and a list written at the index it is read from.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ParallelListAnalyzer : DiagnosticAnalyzer
{
    /// <summary>One collection indexed by another's index.</summary>
    public const string ParallelListId = "SME6487";

    /// <summary>The import pipeline's assembly, which the rule reads beside Core.</summary>
    private const string ExtractionAssembly = "SuperMetroid.AssetExtraction";

    /// <summary>The attribute that marks the single owner of an encoding, which may decode a layout's positions.</summary>
    private const string EncodingOwnerAttribute = "EncodingOwnerAttribute";

    /// <summary>The attribute that marks a compatibility service, which may reproduce a pairing by placement.</summary>
    private const string CompatibilityServiceAttribute = "CompatibilityServiceAttribute";

    /// <summary>The methods that find a position in a collection.</summary>
    private static readonly ImmutableHashSet<string> PositionFinders =
        ImmutableHashSet.Create(StringComparer.Ordinal, "IndexOf", "LastIndexOf", "FindIndex", "FindLastIndex");

    private static readonly DiagnosticDescriptor Rule = new(
        ParallelListId,
        "One collection is indexed by another's index",
        "{0} pairs '{1}' with '{2}' by list position; hold each pairing in one record, a keyed lookup or a reference",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "State and its data are paired by identity, never by position (#2565): two lists read at the same " +
            "index must be kept in step by hand and say nothing of which entry belongs to which. One record holds both, a " +
            "keyed lookup finds one by the other, or one refers to the other.");

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
            start.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
            start.RegisterOperationAction(AnalyzeElementRead, OperationKind.ArrayElementReference, OperationKind.PropertyReference);
            start.RegisterOperationAction(AnalyzeLoop, OperationKind.Loop);
        });
    }

    /// <summary>Reports a <c>Zip</c> of two collections, and a projection whose index reads another collection.</summary>
    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocation = (IInvocationOperation)context.Operation;
        if (IsInOwner(context.ContainingSymbol))
            return;
        IMethodSymbol method = invocation.TargetMethod;
        IReadOnlyList<IOperation> sources = Sources(invocation);
        if (method.Name == "Zip" && sources.Count >= 2)
        {
            ISymbol? first = Root(sources[0]);
            for (int other = 1; other < sources.Count; other++)
            {
                ISymbol? root = Root(sources[other]);
                if (first is null || root is null || SymbolEqualityComparer.Default.Equals(first, root))
                    continue;
                Report(context, invocation.Syntax, "The Zip", sources[0], sources[other]);
                return;
            }
            return;
        }
        if (method.Name is "Select" or "SelectMany" or "Where" && sources.Count == 1 &&
            Lambda(invocation) is { } lambda && lambda.Symbol.Parameters.Length == 2 &&
            lambda.Symbol.Parameters[1].Type.SpecialType == SpecialType.System_Int32)
        {
            IParameterSymbol index = lambda.Symbol.Parameters[1];
            ISymbol? source = Root(sources[0]);
            foreach (IOperation read in lambda.Body.Descendants())
            {
                if (IndexedCollection(read, index) is { } collection && Root(collection) is { } root &&
                    !SymbolEqualityComparer.Default.Equals(root, source))
                {
                    Report(context, read.Syntax, "The projection's index", sources[0], collection);
                    return;
                }
            }
        }
    }

    /// <summary>Reports an element read whose index is a position found in another collection.</summary>
    private static void AnalyzeElementRead(OperationAnalysisContext context)
    {
        if (IsInOwner(context.ContainingSymbol))
            return;
        (IOperation? collection, IOperation? index) = Element(context.Operation);
        if (collection is null || index is null || IsWritten(context.Operation))
            return;
        IOperation found = Unwrapped(index);
        if (found is ILocalReferenceOperation { Local: { } local } && SingleInitializer(local, context.Operation) is { } initializer)
            found = Unwrapped(initializer);
        if (found is not IInvocationOperation finder || !PositionFinders.Contains(finder.TargetMethod.Name))
            return;
        // An instance finder searches its receiver; a static or extension finder (Array.IndexOf) its first argument.
        IOperation? searched = finder.Instance ?? (finder.Arguments.Length > 0 ? finder.Arguments[0].Value : null);
        if (searched is null || Root(searched) is not { } searchedRoot || Root(collection) is not { } readRoot ||
            SymbolEqualityComparer.Default.Equals(searchedRoot, readRoot))
            return;
        Report(context, context.Operation.Syntax, "The read", searched, collection);
    }

    /// <summary>Reports a loop that reads two collections at its own index, one of them a static definition table.</summary>
    private static void AnalyzeLoop(OperationAnalysisContext context)
    {
        if (context.Operation is not IForLoopOperation loop || IsInOwner(context.ContainingSymbol))
            return;
        if (loop.Locals.Length != 1 || loop.Locals[0].Type.SpecialType != SpecialType.System_Int32)
            return;
        ILocalSymbol index = loop.Locals[0];
        var read = new List<IOperation>();
        foreach (IOperation operation in loop.Body.Descendants())
        {
            if (IndexedCollection(operation, index) is { } collection && !IsWritten(operation))
                read.Add(collection);
        }
        var roots = new List<(ISymbol Root, IOperation Collection)>();
        foreach (IOperation collection in read)
        {
            if (Root(collection) is { } root && !roots.Any(known => SymbolEqualityComparer.Default.Equals(known.Root, root)))
                roots.Add((root, collection));
        }
        if (roots.Count < 2 || !roots.Any(entry => entry.Root.IsStatic && entry.Root is IFieldSymbol or IPropertySymbol))
            return;
        (ISymbol _, IOperation table) = roots.First(entry => entry.Root.IsStatic && entry.Root is IFieldSymbol or IPropertySymbol);
        (ISymbol _, IOperation partner) = roots.First(entry => !ReferenceEquals(entry.Collection, table));
        Report(context, loop.Syntax, "The loop", table, partner);
    }

    /// <summary>Reports <paramref name="syntax"/> as pairing <paramref name="first"/> with <paramref name="second"/>.</summary>
    private static void Report(OperationAnalysisContext context, SyntaxNode syntax, string what, IOperation first, IOperation second) =>
        context.ReportDiagnostic(Diagnostic.Create(Rule, syntax.GetLocation(), what, Unwrapped(first).Syntax.ToString(), Unwrapped(second).Syntax.ToString()));

    /// <summary>The sequences a LINQ-style call reads: its receiver or extension receiver, then any sequence arguments.</summary>
    private static IReadOnlyList<IOperation> Sources(IInvocationOperation invocation)
    {
        var sources = new List<IOperation>();
        if (invocation.Instance is { } instance)
            sources.Add(instance);
        foreach (IArgumentOperation argument in invocation.Arguments)
        {
            if (argument.Value is IDelegateCreationOperation or IAnonymousFunctionOperation)
                continue;
            if (IsSequence(argument.Value.Type) || IsSequence(Unwrapped(argument.Value).Type))
                sources.Add(argument.Value);
        }
        return sources;
    }

    /// <summary>Whether <paramref name="type"/> is a collection other than a string.</summary>
    private static bool IsSequence(ITypeSymbol? type) =>
        type is not null && type.SpecialType != SpecialType.System_String &&
        (type is IArrayTypeSymbol || type.AllInterfaces.Any(face => face.Name == "IEnumerable") ||
            type is INamedTypeSymbol { Name: "IEnumerable" or "ReadOnlySpan" or "Span" });

    /// <summary>The lambda a call passes, if any.</summary>
    private static IAnonymousFunctionOperation? Lambda(IInvocationOperation invocation)
    {
        foreach (IArgumentOperation argument in invocation.Arguments)
        {
            IOperation value = argument.Value is IDelegateCreationOperation creation ? creation.Target : argument.Value;
            if (value is IAnonymousFunctionOperation lambda)
                return lambda;
        }
        return null;
    }

    /// <summary>The collection <paramref name="operation"/> reads at exactly <paramref name="index"/>, if it is such a read.</summary>
    private static IOperation? IndexedCollection(IOperation operation, ISymbol index)
    {
        (IOperation? collection, IOperation? at) = Element(operation);
        return collection is not null && at is not null && Unwrapped(at) switch
        {
            ILocalReferenceOperation local => SymbolEqualityComparer.Default.Equals(local.Local, index),
            IParameterReferenceOperation parameter => SymbolEqualityComparer.Default.Equals(parameter.Parameter, index),
            _ => false,
        } ? collection : null;
    }

    /// <summary>The collection and index of an element read: an array element or a one-argument integer indexer.</summary>
    private static (IOperation? Collection, IOperation? Index) Element(IOperation operation) => operation switch
    {
        IArrayElementReferenceOperation { Indices.Length: 1 } array => (array.ArrayReference, array.Indices[0]),
        IPropertyReferenceOperation { Property.IsIndexer: true, Arguments.Length: 1, Instance: { } instance } indexer
            when indexer.Arguments[0].Value.Type?.SpecialType == SpecialType.System_Int32 => (instance, indexer.Arguments[0].Value),
        _ => (null, null),
    };

    /// <summary>Whether the element is written rather than read: an assignment's target, or a ref handed on.</summary>
    private static bool IsWritten(IOperation element) =>
        element.Parent is IAssignmentOperation assignment && assignment.Target == element ||
        element.Parent is IIncrementOrDecrementOperation;

    /// <summary>The initializer of <paramref name="local"/> when its declaration in the same body is its only assignment.</summary>
    private static IOperation? SingleInitializer(ILocalSymbol local, IOperation site)
    {
        IOperation root = site;
        while (root.Parent is not null)
            root = root.Parent;
        IOperation? initializer = null;
        foreach (IOperation operation in root.Descendants())
        {
            if (operation is IVariableDeclaratorOperation declarator && SymbolEqualityComparer.Default.Equals(declarator.Symbol, local))
                initializer = declarator.Initializer?.Value;
            else if (operation is ISimpleAssignmentOperation { Target: ILocalReferenceOperation target } &&
                SymbolEqualityComparer.Default.Equals(target.Local, local))
                return null;
        }
        return initializer;
    }

    /// <summary>
    /// The collection a collection expression reads: the local, parameter, field or property it names, through the calls
    /// that derive another sequence from it (<c>list.Skip(1)</c> reads <c>list</c>), or null when it names none.
    /// </summary>
    private static ISymbol? Root(IOperation operation)
    {
        operation = Unwrapped(operation);
        while (true)
        {
            switch (operation)
            {
                case ILocalReferenceOperation local:
                    return local.Local;
                case IParameterReferenceOperation parameter:
                    return parameter.Parameter;
                case IFieldReferenceOperation field:
                    return field.Field;
                case IPropertyReferenceOperation property when !property.Property.IsIndexer:
                    return property.Property;
                case IInvocationOperation call when call.Instance is { } receiver:
                    operation = Unwrapped(receiver);
                    continue;
                case IInvocationOperation call when call.TargetMethod.IsExtensionMethod && call.Arguments.Length > 0:
                    operation = Unwrapped(call.Arguments[0].Value);
                    continue;
                default:
                    return null;
            }
        }
    }

    /// <summary><paramref name="operation"/> with its conversions and parentheses removed.</summary>
    private static IOperation Unwrapped(IOperation operation)
    {
        while (operation is IConversionOperation or IParenthesizedOperation)
            operation = operation is IConversionOperation conversion ? conversion.Operand : ((IParenthesizedOperation)operation).Operand;
        return operation;
    }

    /// <summary>Whether the site lies in a type that owns an encoding or reproduces the original's behaviour.</summary>
    private static bool IsInOwner(ISymbol site)
    {
        for (INamedTypeSymbol? type = site as INamedTypeSymbol ?? site.ContainingType; type is not null; type = type.ContainingType)
        {
            if (type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name is EncodingOwnerAttribute or CompatibilityServiceAttribute))
                return true;
        }
        return false;
    }
}
