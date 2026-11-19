using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// A step returns what happened; nothing stores a request for another phase to consume and clear (SME6470, #2562). In
/// <c>SuperMetroid.Core</c>, the rule reports a mutable field, or a property with a setter, typed <c>bool</c> or nullable,
/// which the code both sets (to <c>true</c> or a value) and clears (to <c>false</c>, <c>null</c> or <c>default</c>), when
/// its name says it holds a request kept for a later reader (a camel-case word <c>Request</c>, <c>Requested</c>,
/// <c>Requests</c>, <c>Pending</c>, <c>Signal</c> or <c>Queued</c>, or the words <c>ThisFrame</c>, <c>ThisPass</c>,
/// <c>ThisUpdate</c>, <c>NextFrame</c>, <c>NextPass</c> or <c>NextUpdate</c>) or its type is a step's result (a nullable
/// of a type named <c>…Result</c>): a flag one owner sets for another to act on and clear, or a step's result kept on
/// longer-lived state, cleared each update and refilled for a later phase to read. It also reports a method named
/// <c>Consume…</c>, <c>Take…</c> or <c>HandOver…</c> that reads a <c>bool</c> or nullable member of its own type and
/// clears it: the read-and-clear hand-over itself.
/// </summary>
/// <remarks>
/// What replaces such a flag is the step's returned result, an event the step publishes, a direct call to the owner of
/// the effect, a value of the one update's own record that the phases are handed, or a state of the owner's own course
/// that names where it stands. Members of a
/// <c>[CompatibilityService]</c> type are not reported: such a service is the named place a reproduced one-update
/// hand-over of the original lives, with its evidence. Only Core is read.
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class OneUpdateRequestAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A request flag stored for another step to consume and clear.</summary>
    public const string OneUpdateRequestId = "SME6470";

    /// <summary>The camel-case words that name a request kept for a later reader.</summary>
    private static readonly ImmutableHashSet<string> RequestWords =
        ImmutableHashSet.Create("Request", "Requested", "Requests", "Pending", "Signal", "Queued");

    /// <summary>The phrases that name a value kept for the rest of one update.</summary>
    private static readonly ImmutableArray<string> UpdatePhrases =
        ["ThisFrame", "ThisPass", "ThisUpdate", "NextFrame", "NextPass", "NextUpdate"];

    /// <summary>The name prefixes of a method that hands a kept value over to its reader.</summary>
    private static readonly ImmutableArray<string> HandOverPrefixes = ["Consume", "Take", "HandOver"];

    /// <summary>The ending of the name of a type a step returns as its result.</summary>
    private const string ResultTypeEnding = "Result";

    /// <summary>The attribute that marks a compatibility service.</summary>
    private const string CompatibilityServiceAttribute = "CompatibilityServiceAttribute";

    private static readonly DiagnosticDescriptor Rule = new(
        OneUpdateRequestId,
        "Request flag kept for another step",
        "'{0}' is a request kept for another step to consume and clear; return what happened from the step, publish it as an event, call the owner of the effect, or make it a state of the owner's own course",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A step returns what happened (#2562): a bool or nullable flag one owner sets and another consumes and " +
            "clears, named as a request or as a value of one update, or a step's result kept on longer-lived state and cleared " +
            "each update, hides the hand-over in shared state. The step's result, a published event, a direct call, the " +
            "update's own record or a named state of the owner's course says what happened instead.",
        customTags: [WellKnownDiagnosticTags.CompilationEnd]);

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
            var cleared = new ConcurrentDictionary<ISymbol, bool>(SymbolEqualityComparer.Default);
            var set = new ConcurrentDictionary<ISymbol, bool>(SymbolEqualityComparer.Default);
            start.RegisterOperationAction(operation =>
            {
                var assignment = (ISimpleAssignmentOperation)operation.Operation;
                if (Member(assignment.Target) is not { } member || !IsCandidate(member))
                    return;
                if (IsClearingValue(assignment.Value))
                    cleared[member.OriginalDefinition] = true;
                else
                    set[member.OriginalDefinition] = true;
            }, OperationKind.SimpleAssignment);
            start.RegisterOperationAction(AnalyzeConsumer, OperationKind.MethodBody);
            start.RegisterCompilationEndAction(end =>
            {
                foreach (ISymbol member in cleared.Keys)
                {
                    if (!set.ContainsKey(member) || !(NamesRequest(member.Name) || HoldsStepResult(member)) ||
                        InCompatibilityService(member))
                        continue;
                    foreach (Location location in member.Locations.Where(location => location.IsInSource))
                        end.ReportDiagnostic(Diagnostic.Create(Rule, location, member.Name));
                }
            });
        });
    }

    /// <summary>Reports a <c>Consume…</c>, <c>Take…</c> or <c>HandOver…</c> method that reads a flag of its own type and clears it.</summary>
    private static void AnalyzeConsumer(OperationAnalysisContext context)
    {
        if (context.ContainingSymbol is not IMethodSymbol { ContainingType: { } type } method ||
            !HandOverPrefixes.Any(prefix => method.Name.StartsWith(prefix, StringComparison.Ordinal)) || InCompatibilityService(method))
            return;
        var operations = context.Operation.Descendants().ToArray();
        var clearedHere = operations.OfType<ISimpleAssignmentOperation>()
            .Where(assignment => IsClearingValue(assignment.Value))
            .Select(assignment => Member(assignment.Target))
            .Where(member => member is not null && IsCandidate(member) &&
                SymbolEqualityComparer.Default.Equals(member.ContainingType, type))
            .ToImmutableHashSet(SymbolEqualityComparer.Default);
        bool readsCleared = operations.Any(operation =>
            Member(operation) is { } member && clearedHere.Contains(member) && !IsAssignedTo(operation));
        if (clearedHere.Count != 0 && readsCleared)
            context.ReportDiagnostic(Diagnostic.Create(Rule, method.Locations[0], method.Name));
    }

    /// <summary>Whether <paramref name="operation"/> is the target an assignment writes, rather than a read.</summary>
    private static bool IsAssignedTo(IOperation operation) =>
        operation.Parent is IAssignmentOperation assignment && assignment.Target == operation;

    /// <summary>The field or property <paramref name="operation"/> refers to, or null.</summary>
    private static ISymbol? Member(IOperation? operation) => operation switch
    {
        IFieldReferenceOperation field => field.Field,
        IPropertyReferenceOperation property => property.Property,
        _ => null,
    };

    /// <summary>Whether <paramref name="member"/> is a mutable bool or nullable field, or a settable one of those properties.</summary>
    private static bool IsCandidate(ISymbol member)
    {
        ITypeSymbol? type = member switch
        {
            IFieldSymbol { IsConst: false, IsReadOnly: false } field => field.Type,
            IPropertySymbol { SetMethod: not null } property => property.Type,
            _ => null,
        };
        return type is not null && (type.SpecialType == SpecialType.System_Boolean ||
            type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T ||
            (type.IsReferenceType && type.NullableAnnotation == NullableAnnotation.Annotated));
    }

    /// <summary>Whether <paramref name="member"/> holds a step's result: a nullable of a type named <c>…Result</c>.</summary>
    private static bool HoldsStepResult(ISymbol member)
    {
        ITypeSymbol? type = member switch
        {
            IFieldSymbol field => field.Type,
            IPropertySymbol property => property.Type,
            _ => null,
        };
        ITypeSymbol? held = type switch
        {
            INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable => nullable.TypeArguments[0],
            { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated } reference => reference,
            _ => null,
        };
        return held is not null && held.Name.EndsWith(ResultTypeEnding, StringComparison.Ordinal);
    }

    /// <summary>Whether <paramref name="value"/> clears a flag: <c>false</c>, <c>null</c> or <c>default</c>.</summary>
    private static bool IsClearingValue(IOperation value)
    {
        IOperation unwrapped = value is IConversionOperation { IsImplicit: true } conversion ? conversion.Operand : value;
        if (unwrapped is IDefaultValueOperation)
            return true;
        return unwrapped.ConstantValue is { HasValue: true } constant && (constant.Value is null || constant.Value is false);
    }

    /// <summary>Whether <paramref name="name"/> names a request kept for a later reader.</summary>
    internal static bool NamesRequest(string name)
    {
        string trimmed = name.TrimStart('_');
        if (UpdatePhrases.Any(phrase => trimmed.IndexOf(phrase, StringComparison.Ordinal) >= 0))
            return true;
        int start = 0;
        for (int index = 1; index <= trimmed.Length; index++)
        {
            if (index < trimmed.Length && !char.IsUpper(trimmed[index]))
                continue;
            string word = trimmed.Substring(start, index - start);
            if (RequestWords.Contains(char.ToUpperInvariant(word[0]) + word.Substring(1)))
                return true;
            start = index;
        }
        return false;
    }

    /// <summary>Whether <paramref name="symbol"/> is declared in a compatibility service, or a type nested in one.</summary>
    private static bool InCompatibilityService(ISymbol symbol)
    {
        for (INamedTypeSymbol? type = symbol.ContainingType; type is not null; type = type.ContainingType)
            if (type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == CompatibilityServiceAttribute))
                return true;
        return false;
    }
}
