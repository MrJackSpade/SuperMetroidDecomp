using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// The programs a projectile program family's construction can start it in when its program argument is not a constant
/// (#1784), as a spawner starting the program its callers hand it does: a program member named outright; for a parameter of
/// the method holding it, or a positional record's property, what the calls or constructions passing that parameter pass,
/// traced on in turn; for a call of a Core method, what each of its returns can be (#1881); and for an element of a
/// read-only table, what each of the table's entries can be (#1881), as Bomb Torizo's statue pieces take their programs
/// from the <c>Animation</c> of their entries in <c>BombTorizoStatueFragmentDefinitions.Fragments</c> (#2029). Since the
/// projectile behaviours took over from the kind-keyed start tables (#1760), this is how a kind's spawner names the program
/// it starts.
/// </summary>
/// <param name="model">The Core facts.</param>
/// <param name="reaches">
/// Whether a walk reaches a unit: the calls a tracer follows are those whose code the walk reaches, or every one in Core when
/// the walk reaches none; null to follow every call.
/// </param>
internal sealed class ProgramTracer(CoreModel model, Func<Unit, bool>? reaches)
{
    /// <summary>The programs of <paramref name="programType"/> <paramref name="expression"/> can be; null when one of them may be any.</summary>
    /// <param name="programType">The family's program type.</param>
    /// <param name="expression">The program expression.</param>
    /// <param name="semantic">The semantic model it binds in.</param>
    public HashSet<string>? Programs(INamedTypeSymbol programType, ExpressionSyntax expression, SemanticModel semantic) =>
        Traced(programType, expression, semantic, new HashSet<ISymbol>(SymbolEqualityComparer.Default));

    /// <summary>
    /// The value expressions the tracing of <paramref name="expression"/> read the programs from in the methods and tables it
    /// went through (each method's returns, each table's entries); empty when it went through none or traced no program.
    /// </summary>
    public IReadOnlyList<ExpressionSyntax> ValuesReadFor(INamedTypeSymbol programType, ExpressionSyntax expression, SemanticModel semantic)
    {
        _read = [];
        HashSet<string>? traced = Traced(programType, expression, semantic, new HashSet<ISymbol>(SymbolEqualityComparer.Default));
        List<ExpressionSyntax> read = _read;
        _read = null;
        return traced is null ? [] : read;
    }

    /// <summary>While <see cref="ValuesReadFor"/> traces, the value expressions read from methods and tables.</summary>
    private List<ExpressionSyntax>? _read;

    /// <summary>
    /// The programs <paramref name="expression"/> can be, with <paramref name="visiting"/> the parameters, methods and tables
    /// being traced.
    /// </summary>
    private HashSet<string>? Traced(INamedTypeSymbol programType, ExpressionSyntax expression, SemanticModel semantic, HashSet<ISymbol> visiting)
    {
        var programs = new HashSet<string>(StringComparer.Ordinal);
        foreach (ExpressionSyntax choice in CoreModel.Choices(expression))
        {
            ISymbol? symbol = semantic.GetSymbolInfo(choice).Symbol;
            if (symbol is IFieldSymbol member && SymbolEqualityComparer.Default.Equals(member.ContainingType, programType))
            {
                programs.Add(member.Name);
                continue;
            }
            // An animation an owner starts by its own name is that program (#2061), as the Tourian statue's descent dust.
            if (IsNamedAnimation(symbol, programType))
            {
                programs.Add(symbol!.Name);
                continue;
            }
            // The animation a family's own table holds at a program, or a lookup of that table at its argument, is the program
            // it is looked up at (#2061): StartingAt[segment], StartingWith(segment).
            ExpressionSyntax? lookedUpAt = choice switch
            {
                ElementAccessExpressionSyntax { ArgumentList.Arguments: [var at] } access when IsProgramTable(semantic.GetSymbolInfo(access.Expression).Symbol, programType) => at.Expression,
                InvocationExpressionSyntax { ArgumentList.Arguments: var arguments } when symbol is IMethodSymbol lookup && LookupParameter(lookup, programType) is { } index
                    && index < arguments.Count => arguments[index].Expression,
                _ => null,
            };
            if (lookedUpAt is not null)
            {
                if (Traced(programType, lookedUpAt, semantic, visiting) is not { } atPrograms)
                    return null;
                programs.UnionWith(atPrograms);
                continue;
            }
            (ISymbol Source, IReadOnlyList<ExpressionSyntax> Values, SemanticModel Semantic)? values = choice switch
            {
                InvocationExpressionSyntax when symbol is IMethodSymbol method && Returned(method) is { } returned => (method.OriginalDefinition, returned.Values, returned.Semantic),
                ElementAccessExpressionSyntax access => Entries(semantic.GetSymbolInfo(access.Expression).Symbol),
                IdentifierNameSyntax when symbol is ILocalSymbol local => Entries(LookedUpIn(semantic, local)),
                _ => null,
            };
            HashSet<string>? traced;
            if (values is { } read)
            {
                // A method or table reached again through its own values adds nothing of its own.
                if (!visiting.Add(read.Source))
                    continue;
                _read?.AddRange(read.Values);
                traced = TracedAll(programType, read.Values, read.Semantic, visiting);
                visiting.Remove(read.Source);
            }
            else
            {
                if (model.HandedParameter(symbol) is not { ContainingSymbol: IMethodSymbol callee } handed)
                    return null;
                // A cycle of calls handing the program round adds nothing of its own.
                if (!visiting.Add(handed))
                    continue;
                traced = Passed(programType, callee, handed, visiting);
                visiting.Remove(handed);
            }
            if (traced is null)
                return null;
            programs.UnionWith(traced);
        }
        return programs;
    }

    /// <summary>Whether <paramref name="symbol"/> is a static property or field holding one animation over <paramref name="programType"/>.</summary>
    private static bool IsNamedAnimation(ISymbol? symbol, INamedTypeSymbol programType) =>
        (symbol switch
        {
            IPropertySymbol { IsStatic: true } property => property.Type,
            IFieldSymbol { IsStatic: true } field => field.Type,
            _ => null,
        }) is INamedTypeSymbol { Name: "Animation", TypeArguments: [var played] } && SymbolEqualityComparer.Default.Equals(played, programType);

    /// <summary>Whether <paramref name="table"/> is a static dictionary from <paramref name="programType"/> to animations over it.</summary>
    private static bool IsProgramTable(ISymbol? table, INamedTypeSymbol programType) =>
        (table switch
        {
            IPropertySymbol { IsStatic: true } property => property.Type,
            IFieldSymbol { IsStatic: true } field => field.Type,
            _ => null,
        }) is INamedTypeSymbol { TypeArguments: [var key, INamedTypeSymbol { Name: "Animation", TypeArguments: [var played] }] }
        && SymbolEqualityComparer.Default.Equals(key, programType) && SymbolEqualityComparer.Default.Equals(played, programType);

    /// <summary>
    /// The parameter <paramref name="method"/> looks a family's program table up at, when its body is only that lookup
    /// (<c>StartingWith(segment) => StartingAt[segment]</c>); null for any other method.
    /// </summary>
    private int? LookupParameter(IMethodSymbol method, INamedTypeSymbol programType)
    {
        if (!model.IsSource(method) || method.OriginalDefinition.DeclaringSyntaxReferences is not [var reference]
            || reference.GetSyntax() is not MethodDeclarationSyntax { ExpressionBody.Expression: ElementAccessExpressionSyntax { ArgumentList.Arguments: [var at] } access } declaration)
            return null;
        SemanticModel semantic = model.ModelOf(declaration.SyntaxTree);
        return IsProgramTable(semantic.GetSymbolInfo(access.Expression).Symbol, programType)
            && semantic.GetSymbolInfo(at.Expression).Symbol is IParameterSymbol { Ordinal: var ordinal } parameter
            && SymbolEqualityComparer.Default.Equals(parameter.ContainingSymbol, method.OriginalDefinition)
            ? ordinal
            : null;
    }

    /// <summary>The programs any of <paramref name="values"/> can be; null when one of them may be any.</summary>
    private HashSet<string>? TracedAll(INamedTypeSymbol programType, IReadOnlyList<ExpressionSyntax> values, SemanticModel semantic, HashSet<ISymbol> visiting)
    {
        var programs = new HashSet<string>(StringComparer.Ordinal);
        foreach (ExpressionSyntax value in values)
        {
            if (Traced(programType, value, semantic, visiting) is not { } traced)
                return null;
            programs.UnionWith(traced);
        }
        return programs;
    }

    /// <summary>
    /// What a Core method's returns hand back: its expression body, or every <c>return</c> of its block body outside the
    /// lambdas and local functions in it; null for a method declared outside Core, with no body, or with no value returned.
    /// </summary>
    private (IReadOnlyList<ExpressionSyntax> Values, SemanticModel Semantic)? Returned(IMethodSymbol method)
    {
        if (!model.IsSource(method) || method.OriginalDefinition.DeclaringSyntaxReferences is not [var reference]
            || reference.GetSyntax() is not MethodDeclarationSyntax declaration)
            return null;
        List<ExpressionSyntax> values = declaration.ExpressionBody is { Expression: var body }
            ? [body]
            : [.. declaration.Body?.DescendantNodes(node => node is not (AnonymousFunctionExpressionSyntax or LocalFunctionStatementSyntax))
                .OfType<ReturnStatementSyntax>().Select(statement => statement.Expression).OfType<ExpressionSyntax>() ?? []];
        return values.Count == 0 ? null : (values, model.ModelOf(declaration.SyntaxTree));
    }

    /// <summary>
    /// The values the read-only Core table <paramref name="table"/> holds: a field it cannot reassign, or a property with no
    /// setter, whose initializer lists every value as an expression: a collection expression without spreads or an array
    /// initializer (its entries), or a dictionary's initializer (each key's value), read through a conversion called on it
    /// with no arguments (<c>ToFrozenDictionary()</c>); null for anything else.
    /// </summary>
    private (ISymbol Table, IReadOnlyList<ExpressionSyntax> Values, SemanticModel Semantic)? Entries(ISymbol? table)
    {
        if (table is not (IFieldSymbol { IsReadOnly: true } or IPropertySymbol { SetMethod: null }) || !model.IsSource(table))
            return null;
        foreach (SyntaxReference reference in table.OriginalDefinition.DeclaringSyntaxReferences)
        {
            ExpressionSyntax? initializer = reference.GetSyntax() switch
            {
                VariableDeclaratorSyntax { Initializer.Value: var value } => value,
                PropertyDeclarationSyntax { Initializer.Value: var value } => value,
                _ => null,
            };
            while (initializer is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Expression: var converted }, ArgumentList.Arguments.Count: 0 })
                initializer = converted;
            IReadOnlyList<ExpressionSyntax>? values = initializer switch
            {
                CollectionExpressionSyntax collection when collection.Elements.All(element => element is ExpressionElementSyntax) =>
                    [.. collection.Elements.Cast<ExpressionElementSyntax>().Select(element => element.Expression)],
                ArrayCreationExpressionSyntax { Initializer: { } entries } => entries.Expressions,
                ImplicitArrayCreationExpressionSyntax { Initializer: var entries } => entries.Expressions,
                InitializerExpressionSyntax entries => entries.Expressions,
                BaseObjectCreationExpressionSyntax { Initializer: { } entries } => DictionaryValues(entries),
                _ => null,
            };
            if (values is { Count: > 0 })
                return (table.OriginalDefinition, values, model.ModelOf(reference.SyntaxTree));
        }
        return null;
    }

    /// <summary>
    /// Each key's value in a dictionary's initializer, written <c>[key] = value</c> or <c>{ key, value }</c>; null when an
    /// entry is written otherwise.
    /// </summary>
    private static List<ExpressionSyntax>? DictionaryValues(InitializerExpressionSyntax entries)
    {
        var values = new List<ExpressionSyntax>();
        foreach (ExpressionSyntax entry in entries.Expressions)
        {
            switch (entry)
            {
                case AssignmentExpressionSyntax { Left: ImplicitElementAccessSyntax, Right: var value }:
                    values.Add(value);
                    break;
                case InitializerExpressionSyntax { Expressions: [_, var value] }:
                    values.Add(value);
                    break;
                default:
                    return null;
            }
        }
        return values;
    }

    /// <summary>
    /// The table <paramref name="local"/> was looked up in: the receiver of the <c>TryGetValue</c> call whose <c>out</c>
    /// argument declares it, when no other code in its method assigns it; null for any other local.
    /// </summary>
    private static ISymbol? LookedUpIn(SemanticModel semantic, ILocalSymbol local)
    {
        if (local.DeclaringSyntaxReferences is not [var reference]
            || reference.GetSyntax() is not SingleVariableDesignationSyntax { Parent: DeclarationExpressionSyntax { Parent: ArgumentSyntax argument } }
            || !argument.RefKindKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OutKeyword)
            || argument.Parent is not ArgumentListSyntax { Parent: InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "TryGetValue", Expression: var table } } }
            || argument.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault() is not { } member
            || member.DescendantNodes().OfType<AssignmentExpressionSyntax>()
                .Any(assignment => SymbolEqualityComparer.Default.Equals(semantic.GetSymbolInfo(assignment.Left).Symbol, local)))
            return null;
        return semantic.GetSymbolInfo(table).Symbol;
    }

    /// <summary>
    /// The programs the calls of <paramref name="callee"/> pass for <paramref name="parameter"/>: those whose code the walk
    /// reaches, or every one when it reaches none; null when none calls it, or one leaves it to its default or passes what
    /// cannot be traced.
    /// </summary>
    private HashSet<string>? Passed(INamedTypeSymbol programType, IMethodSymbol callee, IParameterSymbol parameter, HashSet<ISymbol> visiting)
    {
        IReadOnlyList<CallSite> sites = model.CallSites(callee);
        List<CallSite> reached = reaches is null ? [] : [.. sites.Where(site => site.Caller is { } caller && reaches(caller))];
        if (sites.Count == 0)
            return null;
        var programs = new HashSet<string>(StringComparer.Ordinal);
        foreach (CallSite site in reached.Count > 0 ? reached : sites)
        {
            if (site.ArgumentFor(parameter) is not { } argument || Traced(programType, argument, site.Semantic, visiting) is not { } traced)
                return null;
            programs.UnionWith(traced);
        }
        return programs;
    }
}
