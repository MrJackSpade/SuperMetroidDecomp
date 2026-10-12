using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.ResourceAudit;

/// <summary>
/// Conservative source-only name flow for the identified menu boundaries. Every
/// initializer/write/call must be finite; unknown writes, ref aliases, escaping
/// helper delegates and recursive flows remain unresolved. No path is executed.
/// </summary>
internal sealed class FinitePresentationNameFlow(Compilation compilation,
    Func<IOperation, string[]?> directConstants)
{
    private readonly HashSet<ISymbol> active = new(SymbolEqualityComparer.Default);
    private readonly Dictionary<ISymbol, string[]?> known = new(SymbolEqualityComparer.Default);

    internal string[]? Resolve(IOperation operation)
    {
        if (directConstants(operation) is { } direct) return direct;
        if (operation is IConversionOperation conversion) return Resolve(conversion.Operand);
        if (operation is IConditionalOperation { WhenFalse: not null } conditional)
        {
            if (conditional.Condition.ConstantValue is { HasValue: true, Value: bool choice })
                return Resolve(choice ? conditional.WhenTrue : conditional.WhenFalse);
            return Union([Resolve(conditional.WhenTrue), Resolve(conditional.WhenFalse)]);
        }
        if (operation is ISwitchExpressionOperation selection)
            return Union(selection.Arms.Where(arm => !Throws(arm.Value))
                .Select(arm => Resolve(arm.Value)));
        if (operation is ILocalReferenceOperation local) return SymbolValues(local.Local);
        if (operation is IFieldReferenceOperation { Field.DeclaredAccessibility: Accessibility.Private } field)
            return SymbolValues(field.Field);
        if (operation is IParameterReferenceOperation parameter &&
            parameter.Parameter.ContainingSymbol is IMethodSymbol method &&
            (method.MethodKind == MethodKind.LocalFunction || method.DeclaredAccessibility == Accessibility.Private))
            return SymbolValues(parameter.Parameter);
        if (operation is IInvocationOperation invocation)
        {
            // A source expression-bodied helper can return only the resolved values.
            // Unknown bodies, ordinary statements and unavailable metadata are not inferred.
            var declarations = invocation.TargetMethod.DeclaringSyntaxReferences;
            SyntaxNode? declaration = declarations.Length == 1 ? declarations[0].GetSyntax() : null;
            ExpressionSyntax? expression = declaration switch
            {
                MethodDeclarationSyntax member => member.ExpressionBody?.Expression,
                LocalFunctionStatementSyntax localFunction => localFunction.ExpressionBody?.Expression,
                _ => null,
            };
            if (expression is null || !active.Add(invocation.TargetMethod)) return null;
            try { return Operation(expression) is { } body ? Resolve(body) : null; }
            finally { active.Remove(invocation.TargetMethod); }
        }
        return null;
    }

    private string[]? SymbolValues(ISymbol symbol)
    {
        if (known.TryGetValue(symbol, out string[]? cached)) return cached;
        if (!active.Add(symbol)) return null;
        try
        {
            var sets = new List<string[]?>();
            if (symbol is IParameterSymbol parameter)
            {
                var owner = (IMethodSymbol)parameter.ContainingSymbol;
                bool called = false;
                foreach (IdentifierNameSyntax reference in References(owner))
                {
                    SyntaxNode access = reference.Parent is MemberAccessExpressionSyntax member && member.Name == reference
                        ? member : reference;
                    if (access.Parent is not InvocationExpressionSyntax call || Operation(call) is not IInvocationOperation invoked ||
                        !SymbolEqualityComparer.Default.Equals(invoked.TargetMethod.OriginalDefinition, owner.OriginalDefinition))
                        return Cache(null); // Delegate/method-group escape destroys the closed call set.
                    IArgumentOperation? argument = invoked.Arguments.SingleOrDefault(arg => arg.Parameter?.Ordinal == parameter.Ordinal);
                    if (argument is null) return Cache(null);
                    sets.Add(Resolve(argument.Value));
                    called = true;
                }
                if (!called) return Cache(null);
            }
            else
            {
                foreach (SyntaxReference declaration in symbol.DeclaringSyntaxReferences)
                {
                    if (declaration.GetSyntax() is not VariableDeclaratorSyntax { Initializer.Value: { } initializer } ||
                        Operation(initializer) is not { } value) return Cache(null);
                    sets.Add(Resolve(value));
                }
                if (sets.Count == 0) return Cache(null);
            }
            foreach (IdentifierNameSyntax reference in References(symbol))
            {
                SyntaxNode access = reference.Parent is MemberAccessExpressionSyntax member && member.Name == reference
                    ? member : reference;
                IOperation? value = Operation(access);
                if (value is null) return Cache(null);
                if (access.Ancestors().OfType<RefExpressionSyntax>().Any()) return Cache(null);
                for (IOperation? parent = value.Parent; parent is not null; parent = parent.Parent)
                {
                    bool Contains(IOperation target) => target.Syntax.SyntaxTree == value.Syntax.SyntaxTree &&
                        target.Syntax.Span.Contains(value.Syntax.Span);
                    if (parent is IAssignmentOperation assignment && Contains(assignment.Target))
                    {
                        if (assignment is not ISimpleAssignmentOperation { IsRef: false } simple ||
                            simple.Target.Syntax.Span != value.Syntax.Span) return Cache(null);
                        sets.Add(Resolve(simple.Value));
                        break;
                    }
                    if (parent is IIncrementOrDecrementOperation increment && Contains(increment.Target) ||
                        parent is IArgumentOperation { Parameter.RefKind: not RefKind.None } argument && Contains(argument.Value))
                        return Cache(null);
                }
            }
            return Cache(Union(sets));
        }
        finally { active.Remove(symbol); }

        string[]? Cache(string[]? values) { known[symbol] = values; return values; }
    }

    private IEnumerable<IdentifierNameSyntax> References(ISymbol symbol) => compilation.SyntaxTrees
        .SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
            .Where(name => name.Identifier.ValueText == symbol.Name)
            .Where(name => SymbolEqualityComparer.Default.Equals(
                compilation.GetSemanticModel(tree).GetSymbolInfo(name).Symbol?.OriginalDefinition, symbol.OriginalDefinition)));

    private IOperation? Operation(SyntaxNode syntax) => compilation.GetSemanticModel(syntax.SyntaxTree).GetOperation(syntax);

    private static bool Throws(IOperation operation) => operation is IThrowOperation ||
        operation is IConversionOperation conversion && Throws(conversion.Operand);

    private static string[]? Union(IEnumerable<string[]?> sets)
    {
        var values = new HashSet<string>(StringComparer.Ordinal);
        foreach (string[]? set in sets)
        {
            if (set is null || set.Length == 0) return null;
            values.UnionWith(set);
        }
        return values.Count == 0 ? null : values.Order(StringComparer.Ordinal).ToArray();
    }
}
