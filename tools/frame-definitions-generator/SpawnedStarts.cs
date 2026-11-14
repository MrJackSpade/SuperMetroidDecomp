using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>The object one start (a family's construction or a typed-start kind's name) starts, as its enclosing member treats it.</summary>
/// <param name="Receiver">The local or parameter holding the object, when the start is handed to its <c>StartAnimation</c>.</param>
/// <param name="Spawn">
/// When the member spawns the object (hands it, or the started kind, to a spawn call), every sheet that call's graphics can
/// make it draw from; null when the member does not spawn it.
/// </param>
/// <param name="Kind">The kind the spawn call is handed, when it spawns the object and names one.</param>
/// <param name="KindSemantic">The semantic model <paramref name="Kind"/> binds in.</param>
/// <param name="Switched">Whether the member switches the object's graphics (other than reshading them).</param>
internal sealed record StartedObject(ISymbol? Receiver, IReadOnlyList<SpawnTarget>? Spawn, ExpressionSyntax? Kind, SemanticModel? KindSemantic, bool Switched)
{
    /// <summary>Whether the member spawns the object with graphics drawing from the room's shared sheet alone.</summary>
    public bool IsRoomSpawn => Spawn is { Count: > 0 } targets && targets.All(target => target.Source == GraphicsSource.Shared);
}

/// <summary>
/// Reads which object a start starts (#1854): one the starting code spawns itself, with the graphics its spawn call names,
/// or one it was handed (the object a frame-behaviour or cue handler moves), and whether the code switches that object's
/// graphics. A handler's start on an object it spawns (Spore Spawn's spawner cue spawning a spore with <c>SharedIn</c> of
/// his colours) is that spawn's, never a program the handler's own object plays.
/// </summary>
/// <param name="model">The Core facts.</param>
internal sealed class SpawnedStarts(CoreModel model)
{
    /// <summary>What <paramref name="start"/>, a family's construction or a typed-start kind's name, starts.</summary>
    public StartedObject Of(SemanticModel semantic, ExpressionSyntax start)
    {
        if (ArgumentOf(start) is not { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation } } argument)
            return new StartedObject(null, null, null, null, Switched: false);
        // A start handed to the spawn call itself (a typed-start kind) starts the object that call spawns.
        if (SpawnGraphics(semantic, invocation) is { } direct)
            return new StartedObject(null, direct, KindArgument(semantic, invocation) ?? argument.Expression, semantic, Switched: false);
        if (invocation.Expression is not MemberAccessExpressionSyntax { Expression: var receiver }
            || semantic.GetSymbolInfo(receiver).Symbol is not { } started || started is not (ILocalSymbol or IParameterSymbol)
            || start.Ancestors().OfType<MemberDeclarationSyntax>().FirstOrDefault() is not { } member)
        {
            return new StartedObject(null, null, null, null, Switched: false);
        }
        bool switched = member.DescendantNodes().OfType<AssignmentExpressionSyntax>().Any(assignment => SwitchesGraphics(semantic, assignment, started));
        foreach (InvocationExpressionSyntax spawn in member.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (spawn.ArgumentList.Arguments.Any(handed => SymbolEqualityComparer.Default.Equals(semantic.GetSymbolInfo(handed.Expression).Symbol, started))
                && SpawnGraphics(semantic, spawn) is { } graphics)
                return new StartedObject(started, graphics, KindArgument(semantic, spawn), semantic, switched);
        }
        return new StartedObject(started, null, null, null, switched);
    }

    /// <summary>The argument <paramref name="expression"/> is, through a member access naming it; null when it is none.</summary>
    private static ArgumentSyntax? ArgumentOf(ExpressionSyntax expression)
    {
        SyntaxNode node = expression.Parent is MemberAccessExpressionSyntax access && access.Name == expression ? access : expression;
        return node.Parent as ArgumentSyntax;
    }

    /// <summary>Whether <paramref name="assignment"/> sets <paramref name="target"/>'s graphics to other than a reshading of them.</summary>
    private bool SwitchesGraphics(SemanticModel semantic, AssignmentExpressionSyntax assignment, ISymbol target) =>
        assignment is { RawKind: (int)SyntaxKind.SimpleAssignmentExpression, Left: MemberAccessExpressionSyntax { Expression: var owner } }
        && SymbolEqualityComparer.Default.Equals(semantic.GetSymbolInfo(owner).Symbol, target)
        && semantic.GetTypeInfo(assignment.Left).Type is { } assigned
        && SymbolEqualityComparer.Default.Equals(assigned.OriginalDefinition, model.SpriteGraphics)
        && !new GraphicsClassifier(model).IsReshading(semantic, assignment.Right);

    /// <summary>The arguments of <paramref name="invocation"/>, each with the parameter it is passed for (by name or position).</summary>
    private static IEnumerable<(ArgumentSyntax Argument, IParameterSymbol Parameter)> Parameters(SemanticModel semantic, InvocationExpressionSyntax invocation)
    {
        if (semantic.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
            yield break;
        SeparatedSyntaxList<ArgumentSyntax> arguments = invocation.ArgumentList.Arguments;
        for (int index = 0; index < arguments.Count; index++)
        {
            ArgumentSyntax argument = arguments[index];
            IParameterSymbol? parameter = argument.NameColon is { } name
                ? method.Parameters.FirstOrDefault(p => p.Name == name.Name.Identifier.Text)
                : index < method.Parameters.Length ? method.Parameters[index] : null;
            if (parameter is not null)
                yield return (argument, parameter);
        }
    }

    /// <summary>
    /// Every sheet the graphics <paramref name="invocation"/> hands a spawned object can make it draw from, when it is a
    /// spawn call (a call handed a spawned object's graphics); null otherwise.
    /// </summary>
    private IReadOnlyList<SpawnTarget>? SpawnGraphics(SemanticModel semantic, InvocationExpressionSyntax invocation)
    {
        foreach ((ArgumentSyntax argument, IParameterSymbol parameter) in Parameters(semantic, invocation))
        {
            if (SymbolEqualityComparer.Default.Equals(parameter.Type.OriginalDefinition, model.SpriteGraphics))
                return new GraphicsClassifier(model).Classify(semantic, argument.Expression);
        }
        return null;
    }

    /// <summary>The kind <paramref name="invocation"/> is handed, when it is handed one.</summary>
    private ExpressionSyntax? KindArgument(SemanticModel semantic, InvocationExpressionSyntax invocation) =>
        Parameters(semantic, invocation)
            .FirstOrDefault(pair => SymbolEqualityComparer.Default.Equals(pair.Parameter.Type, model.RoomEnemyProjectileKind)).Argument?.Expression;
}
