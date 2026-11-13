using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>What decides the sheet a spawned object draws from.</summary>
internal enum GraphicsSource
{
    /// <summary>Its spawner's graphics (<c>SpawnedBy</c>): the spawner's own spawned-object sheet.</summary>
    Spawner,
    /// <summary>Shared graphics: the room's shared sheet.</summary>
    Shared,
    /// <summary>A sheet-specific owner (Mother Brain's tank glass): that owner's sheet alone.</summary>
    Sheet,
    /// <summary>Graphics copied from another object (its <c>Graphics</c>): wherever the objects this code's type manages draw.</summary>
    Inherited,
    /// <summary>Graphics the enclosing method was handed: wherever its callers' graphics say.</summary>
    Parameter,
    /// <summary>Graphics this generator cannot read; the self-check fails on them.</summary>
    Unknown,
}

/// <summary>Where one spawn's graphics make it draw from.</summary>
/// <param name="Source">What decides the sheet.</param>
/// <param name="Scope">The behaviour whose member supplies the graphics, when one does; otherwise the code's own scope.</param>
/// <param name="Sheet">For <see cref="GraphicsSource.Sheet"/>, the SpawnedObjectOwner property naming the sheet.</param>
/// <param name="Origin">For <see cref="GraphicsSource.Parameter"/>, the method whose parameter the graphics are.</param>
internal readonly record struct SpawnTarget(GraphicsSource Source, INamedTypeSymbol? Scope = null, string? Sheet = null, IMethodSymbol? Origin = null)
{
    /// <summary>A spawn with its spawner's graphics in the code's own scope.</summary>
    public static SpawnTarget Spawner => new(GraphicsSource.Spawner);

    /// <summary>A spawn drawing from the room's shared sheet.</summary>
    public static SpawnTarget SharedSheet => new(GraphicsSource.Shared);

    /// <inheritdoc/>
    public bool Equals(SpawnTarget other) =>
        Source == other.Source && SymbolEqualityComparer.Default.Equals(Scope, other.Scope) && Sheet == other.Sheet
        && SymbolEqualityComparer.Default.Equals(Origin, other.Origin);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Source, Scope is null ? 0 : SymbolEqualityComparer.Default.GetHashCode(Scope), Sheet,
        Origin is null ? 0 : SymbolEqualityComparer.Default.GetHashCode(Origin));
}

/// <summary>
/// Reads which sheets the graphics handed to a spawn can make it draw from. The shared standard graphics (<c>default</c>,
/// <c>SpriteGraphics.SharedStandard</c>), <c>SpriteGraphics.SharedIn</c> and the shared owner draw from the room's shared
/// sheet; a sheet-specific owner draws from that owner's sheet alone; <c>SpawnedBy</c> draws from the spawner's sheet.
/// A conditional or switch can yield any of its branches. A property, field, local or method yielding graphics draws
/// where its initializer, body or returns say, and when it is a behaviour's member, the spawn is that behaviour's. An
/// instance member with no value of its own holds graphics copied at run time (inherited); a parameter holds its
/// callers'. Anything else is unknown, which fails the generator's self-check.
/// </summary>
/// <param name="model">The Core facts.</param>
internal sealed class GraphicsClassifier(CoreModel model)
{
    private readonly HashSet<ISymbol> _visiting = new(SymbolEqualityComparer.Default);

    /// <summary>Every sheet <paramref name="graphics"/> can make a spawn draw from.</summary>
    public IReadOnlyList<SpawnTarget> Classify(SemanticModel semantic, ExpressionSyntax graphics)
    {
        switch (graphics)
        {
            case ParenthesizedExpressionSyntax parenthesized:
                return Classify(semantic, parenthesized.Expression);
            case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.DefaultLiteralExpression):
                return [SpawnTarget.SharedSheet];
            case DefaultExpressionSyntax:
                return [SpawnTarget.SharedSheet];
            case ConditionalExpressionSyntax conditional:
                return [.. Classify(semantic, conditional.WhenTrue).Concat(Classify(semantic, conditional.WhenFalse)).Distinct()];
            case SwitchExpressionSyntax switchExpression:
                return [.. switchExpression.Arms.SelectMany(arm => Classify(semantic, arm.Expression)).Distinct()];
            case BaseObjectCreationExpressionSyntax { ArgumentList.Arguments: [var owner, ..] } creation
                when semantic.GetSymbolInfo(creation).Symbol is IMethodSymbol { ContainingType: var created } && IsSpriteGraphics(created):
                return [ClassifyOwner(semantic, owner.Expression)];
            case InvocationExpressionSyntax invocation when semantic.GetSymbolInfo(invocation).Symbol is IMethodSymbol method:
                if (IsSpriteGraphics(method.ContainingType))
                {
                    return method.Name switch
                    {
                        "SharedIn" => [SpawnTarget.SharedSheet],
                        "SpawnedBy" => [SpawnTarget.Spawner],
                        // Another shading keeps the graphics' owner.
                        "In" when invocation.Expression is MemberAccessExpressionSyntax receiver => Classify(semantic, receiver.Expression),
                        _ => [new SpawnTarget(GraphicsSource.Unknown)],
                    };
                }
                return Resolve(method.OriginalDefinition);
        }
        ISymbol? symbol = semantic.GetSymbolInfo(graphics).Symbol;
        switch (symbol)
        {
            case { } member when IsSpriteGraphics(member.ContainingType):
                return [member.Name == "SharedStandard" ? SpawnTarget.SharedSheet : new SpawnTarget(GraphicsSource.Unknown)];
            case IParameterSymbol { ContainingSymbol: IMethodSymbol { MethodKind: not MethodKind.AnonymousFunction } method }:
                return [new SpawnTarget(GraphicsSource.Parameter, Origin: method.OriginalDefinition)];
            case ILocalSymbol local:
                return ResolveLocal(local);
            case IPropertySymbol or IFieldSymbol:
                return Resolve(symbol.OriginalDefinition);
            default:
                return [new SpawnTarget(GraphicsSource.Unknown)];
        }
    }

    /// <summary>Whether <paramref name="expression"/> is a <c>.In(...)</c> reshading of graphics, which keeps their owner.</summary>
    public bool IsReshading(SemanticModel semantic, ExpressionSyntax expression) =>
        expression is InvocationExpressionSyntax invocation
        && semantic.GetSymbolInfo(invocation).Symbol is IMethodSymbol { Name: "In" } method
        && IsSpriteGraphics(method.ContainingType);

    /// <summary>Where a spawned-object owner makes graphics draw from.</summary>
    private SpawnTarget ClassifyOwner(SemanticModel semantic, ExpressionSyntax owner) =>
        semantic.GetSymbolInfo(owner).Symbol switch
        {
            IPropertySymbol { IsStatic: true, Name: "Shared" } shared when IsOwnerType(shared.ContainingType) => SpawnTarget.SharedSheet,
            IPropertySymbol { IsStatic: true } sheet when IsOwnerType(sheet.ContainingType) => new SpawnTarget(GraphicsSource.Sheet, Sheet: sheet.Name),
            IMethodSymbol { Name: "Of" } of when IsOwnerType(of.ContainingType) => SpawnTarget.Spawner,
            _ => new SpawnTarget(GraphicsSource.Unknown),
        };

    /// <summary>Whether <paramref name="type"/> is SpriteGraphics.</summary>
    private bool IsSpriteGraphics(INamedTypeSymbol? type) =>
        SymbolEqualityComparer.Default.Equals(type?.OriginalDefinition, model.SpriteGraphics);

    /// <summary>Whether <paramref name="type"/> is SpawnedObjectOwner.</summary>
    private bool IsOwnerType(INamedTypeSymbol? type) =>
        SymbolEqualityComparer.Default.Equals(type?.OriginalDefinition, model.SpawnedObjectOwner);

    /// <summary>Where a local's graphics draw from, read from its declaration's initializer.</summary>
    private IReadOnlyList<SpawnTarget> ResolveLocal(ILocalSymbol local)
    {
        foreach (SyntaxReference reference in local.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is VariableDeclaratorSyntax { Initializer.Value: var value })
                return Classify(model.ModelOf(value.SyntaxTree), value);
        }
        return [new SpawnTarget(GraphicsSource.Unknown)];
    }

    /// <summary>
    /// Where the graphics a property, field or method yields draw from, read from its initializer, expression body or
    /// returns. An instance member with none holds graphics copied at run time.
    /// </summary>
    private IReadOnlyList<SpawnTarget> Resolve(ISymbol member)
    {
        INamedTypeSymbol? scope = member.ContainingType is { } type ? model.BehaviourOf(type) : null;
        if (!model.IsSource(member))
            return [new SpawnTarget(GraphicsSource.Unknown)];
        // A settable instance member (a spawned object's Graphics) holds whatever the code spawning or switching that object
        // assigns: its initializer is only the free slot's value, so the graphics read from it are copied, not its initializer's.
        if (member is IPropertySymbol { IsStatic: false, SetMethod: not null } or IFieldSymbol { IsStatic: false, IsReadOnly: false, IsConst: false })
            return [new SpawnTarget(GraphicsSource.Inherited)];
        if (!_visiting.Add(member))
            return [];
        try
        {
            var values = new List<ExpressionSyntax>();
            foreach (SyntaxReference reference in member.DeclaringSyntaxReferences)
            {
                switch (reference.GetSyntax())
                {
                    case PropertyDeclarationSyntax property:
                        if ((property.Initializer?.Value ?? property.ExpressionBody?.Expression) is { } propertyValue)
                            values.Add(propertyValue);
                        values.AddRange(property.AccessorList?.Accessors
                            .Where(a => a.IsKind(SyntaxKind.GetAccessorDeclaration))
                            .SelectMany(a => a.ExpressionBody is { } body ? [body.Expression] : Returns(a.Body)) ?? []);
                        break;
                    case VariableDeclaratorSyntax { Initializer.Value: var fieldValue }:
                        values.Add(fieldValue);
                        break;
                    case MethodDeclarationSyntax method:
                        values.AddRange(method.ExpressionBody is { } body ? [body.Expression] : Returns(method.Body));
                        break;
                }
            }
            // A get-only instance member with no value of its own holds what its type's constructors assign it.
            if (values.Count == 0 && member is IPropertySymbol { IsStatic: false } or IFieldSymbol { IsStatic: false } && member.ContainingType is { } owner)
                values.AddRange(ConstructorAssignments(owner, member));
            if (values.Count == 0)
                return [new SpawnTarget(GraphicsSource.Unknown)];
            return [.. values
                .SelectMany(value => Classify(model.ModelOf(value.SyntaxTree), value))
                .Select(target => target with { Scope = target.Scope ?? scope })
                .Distinct()];
        }
        finally
        {
            _visiting.Remove(member);
        }
    }

    /// <summary>The values <paramref name="owner"/>'s constructors assign to <paramref name="member"/>.</summary>
    private IEnumerable<ExpressionSyntax> ConstructorAssignments(INamedTypeSymbol owner, ISymbol member)
    {
        foreach (IMethodSymbol constructor in owner.InstanceConstructors)
        {
            foreach (SyntaxReference reference in constructor.DeclaringSyntaxReferences)
            {
                SyntaxNode declaration = reference.GetSyntax();
                SemanticModel semantic = model.ModelOf(declaration.SyntaxTree);
                foreach (AssignmentExpressionSyntax assignment in declaration.DescendantNodes().OfType<AssignmentExpressionSyntax>())
                {
                    if (SymbolEqualityComparer.Default.Equals(semantic.GetSymbolInfo(assignment.Left).Symbol?.OriginalDefinition, member))
                        yield return assignment.Right;
                }
            }
        }
    }

    /// <summary>The expressions a block body returns.</summary>
    private static IEnumerable<ExpressionSyntax> Returns(BlockSyntax? body) =>
        body?.DescendantNodes().OfType<ReturnStatementSyntax>().Select(r => r.Expression).OfType<ExpressionSyntax>() ?? [];
}
