using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.Declarations;
using SuperMetroid.ReachabilityAudit.SolutionLoading;

namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>
/// The instance members that only constructors give a value, each with the constructors that write it. A type is decided
/// consistently (#2611): while code reads such a member, a constructor that sets it stays, even one only checks call,
/// since without it the member is read but never set. UnsupportedGameStateException's only constructor was called only by
/// a check, while the game's error reporter matched the type and read its <c>BehaviorIdentity</c>; the prune removed the
/// constructor, and the property it alone set no longer compiled (CS8618). The rule applies only when no writer is reached
/// (<see cref="ReachabilityGraph.StateWriters"/>): a second constructor only checks call, beside one the game calls, still
/// goes.
/// </summary>
/// <remarks>
/// A member with an initializer has a value without any constructor, and a positional record property is set by its
/// parameter, so neither is recorded. A writer is a constructor whose own body assigns the member; one that chains to such
/// a constructor reaches it through the call.
/// </remarks>
internal sealed class ConstructorStateWriters(SymbolIdentity identity, ReachabilityGraph graph)
{
    /// <summary>Records every source type's constructor-only members of the compilation, with their writers.</summary>
    public void Collect(Compilation compilation)
    {
        foreach (var type in RootCollector.AllTypes(compilation.Assembly.GlobalNamespace))
        {
            if (type.IsStatic || type.TypeKind is not (TypeKind.Class or TypeKind.Struct))
                continue;
            var writers = new Dictionary<ISymbol, HashSet<string>>(SymbolEqualityComparer.Default);
            foreach (var constructor in type.InstanceConstructors)
            {
                if (identity.Key(constructor) is not { } constructorKey)
                    continue;
                foreach (var reference in constructor.DeclaringSyntaxReferences)
                {
                    if (reference.GetSyntax() is not ConstructorDeclarationSyntax declaration || !identity.IsRepositorySource(reference.SyntaxTree))
                        continue;
                    var model = SemanticModels.Of(compilation, reference.SyntaxTree);
                    foreach (var assignment in declaration.DescendantNodes().OfType<AssignmentExpressionSyntax>())
                    {
                        foreach (var target in Targets(assignment.Left))
                        {
                            if (model.GetSymbolInfo(target).Symbol?.OriginalDefinition is not { } written ||
                                !SymbolEqualityComparer.Default.Equals(written.ContainingType, type.OriginalDefinition) ||
                                !ConstructorOnly(written))
                                continue;
                            if (!writers.TryGetValue(written, out var keys))
                                writers[written] = keys = [];
                            keys.Add(constructorKey);
                        }
                    }
                }
            }
            foreach (var (member, keys) in writers)
                graph.StateWriters(identity.Key(member), keys);
        }
    }

    /// <summary>The members an assignment's left side writes on its own instance: a name, <c>this.</c> a name, or each such
    /// element of a deconstructing tuple.</summary>
    private static IEnumerable<ExpressionSyntax> Targets(ExpressionSyntax left) => left switch
    {
        TupleExpressionSyntax tuple => tuple.Arguments.SelectMany(argument => Targets(argument.Expression)),
        IdentifierNameSyntax or MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } => [left],
        _ => [],
    };

    /// <summary>Whether a member is an instance field or automatic property declared in source without an initializer.</summary>
    private static bool ConstructorOnly(ISymbol member) => member switch
    {
        IFieldSymbol { IsStatic: false, IsConst: false, IsImplicitlyDeclared: false } field =>
            field.DeclaringSyntaxReferences.Any() &&
            field.DeclaringSyntaxReferences.All(reference => reference.GetSyntax() is VariableDeclaratorSyntax { Initializer: null }),
        IPropertySymbol { IsStatic: false } property =>
            property.DeclaringSyntaxReferences.Any() &&
            property.DeclaringSyntaxReferences.All(reference => reference.GetSyntax() is PropertyDeclarationSyntax { Initializer: null } declaration &&
                declaration.AccessorList is { } accessors && accessors.Accessors.All(accessor => accessor.Body is null && accessor.ExpressionBody is null)),
        _ => false,
    };
}
