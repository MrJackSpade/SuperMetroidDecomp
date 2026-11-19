using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// The name helpers of one compilation (SME6460, SME6462, #2544, #2548): the names each unmarked method, local function or
/// property makes its result, the keys its callers hand that result to, and the helpers whose results it returns; and the
/// names each parameter of the compilation is handed, the keys a reference of that parameter reaches, the parameters it
/// is handed on to and the helpers that return it. A record's positional parameter reaches whatever its property reaches.
/// At the compilation's end each name a helper returns, or a parameter is handed, is reported against the key it reaches,
/// through any chain of helpers and parameters.
/// </summary>
internal sealed class NameHelperFlows
{
    /// <summary>The names each helper makes its result.</summary>
    private readonly ConcurrentDictionary<ISymbol, ConcurrentBag<EnumNameWrite>> returned = new(SymbolEqualityComparer.Default);

    /// <summary>The key a call of each helper, or a reference of each parameter, hands its value to, the first one found.</summary>
    private readonly ConcurrentDictionary<ISymbol, KeyDestination> fed = new(SymbolEqualityComparer.Default);

    /// <summary>The helpers and parameters whose values each helper or parameter hands on as its own.</summary>
    private readonly ConcurrentDictionary<ISymbol, ConcurrentBag<ISymbol>> inner = new(SymbolEqualityComparer.Default);

    /// <summary>The names each parameter is handed by the calls of its method.</summary>
    private readonly ConcurrentDictionary<ISymbol, ConcurrentBag<EnumNameWrite>> passed = new(SymbolEqualityComparer.Default);

    /// <summary>Records that <paramref name="helper"/> makes <paramref name="write"/>'s name its result.</summary>
    internal void Returns(ISymbol helper, EnumNameWrite write) => returned.GetOrAdd(helper, _ => []).Add(write);

    /// <summary>Records that a call of <paramref name="helper"/>, or a reference of a parameter, hands its value to <paramref name="key"/>.</summary>
    internal void Feeds(ISymbol helper, KeyDestination key) => fed.TryAdd(helper, key);

    /// <summary>Records that <paramref name="outer"/> returns the result of a call of <paramref name="helper"/> as its own.</summary>
    internal void ReturnsResultOf(ISymbol outer, ISymbol helper) => inner.GetOrAdd(outer, _ => []).Add(helper);

    /// <summary>Records that a call hands <paramref name="write"/>'s name to <paramref name="parameter"/>.</summary>
    internal void PassesTo(IParameterSymbol parameter, EnumNameWrite write) => passed.GetOrAdd(parameter.OriginalDefinition, _ => []).Add(write);

    /// <summary>
    /// Records that the value of <paramref name="value"/>, a parameter or a helper's result, reaches whatever
    /// <paramref name="onward"/> reaches: a parameter it is handed on to, or the helper that returns it.
    /// </summary>
    internal void HandsOn(ISymbol value, ISymbol onward) => inner.GetOrAdd(onward.OriginalDefinition, _ => []).Add(value.OriginalDefinition);

    /// <summary>
    /// Hands <paramref name="report"/> each name a helper returns, or a parameter is handed, whose value reaches a key, with
    /// that key, once every block of the compilation has been read.
    /// </summary>
    internal void Report(Action<(EnumNameWrite Write, KeyDestination Key)> report)
    {
        var reached = new Dictionary<ISymbol, KeyDestination>(fed, SymbolEqualityComparer.Default);
        // A record's positional parameter is its property: what the property's reads reach, the parameter's value reaches.
        foreach (KeyValuePair<ISymbol, KeyDestination> entry in fed)
        {
            if (PositionalParameterOf(entry.Key) is { } parameter && !reached.ContainsKey(parameter))
                reached.Add(parameter, entry.Value);
        }
        var pending = new Queue<ISymbol>(reached.Keys);
        while (pending.Count > 0)
        {
            ISymbol outer = pending.Dequeue();
            if (!inner.TryGetValue(outer, out ConcurrentBag<ISymbol>? helpers))
                continue;
            foreach (ISymbol helper in helpers)
            {
                if (reached.ContainsKey(helper))
                    continue;
                reached.Add(helper, reached[outer]);
                pending.Enqueue(helper);
            }
        }
        foreach (KeyValuePair<ISymbol, ConcurrentBag<EnumNameWrite>> entry in returned.Concat(passed))
        {
            if (!reached.TryGetValue(entry.Key, out KeyDestination key))
                continue;
            foreach (EnumNameWrite write in entry.Value.OrderBy(write => write.Text.Syntax.SpanStart))
                report((write, key));
        }
    }

    /// <summary>
    /// The primary constructor parameter a record's property is declared by, or null for any other member: the parameter
    /// of the record's own declaration that has the property's name.
    /// </summary>
    private static IParameterSymbol? PositionalParameterOf(ISymbol member)
    {
        if (member is not IPropertySymbol { ContainingType: { IsRecord: true } record } property)
            return null;
        foreach (IMethodSymbol constructor in record.InstanceConstructors)
        {
            if (!constructor.DeclaringSyntaxReferences.Any(reference => reference.GetSyntax() is TypeDeclarationSyntax))
                continue;
            if (constructor.Parameters.FirstOrDefault(parameter => parameter.Name == property.Name) is { } positional)
                return positional.OriginalDefinition;
        }
        return null;
    }
}
