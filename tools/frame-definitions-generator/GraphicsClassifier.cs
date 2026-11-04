using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// Reads which sheet the graphics handed to a spawn make it draw from. Emptied graphics (<c>default</c>,
/// <c>SpriteGraphics.Emptied</c>), <c>SpriteGraphics.SharedIn</c> and the shared owner draw from the room's shared
/// sheet; a sheet-specific owner (Mother Brain's tank glass) draws from that owner's sheet alone; <c>SpawnedBy</c> and
/// graphics a slot carries draw from the spawner's sheet. A property, field or method yielding graphics draws where its
/// initializer or expression body says, and when it is a behaviour's member, the spawn is that behaviour's.
/// </summary>
/// <param name="model">The Core facts.</param>
internal sealed class GraphicsClassifier(CoreModel model)
{
    private readonly HashSet<ISymbol> _visiting = new(SymbolEqualityComparer.Default);

    /// <summary>Where <paramref name="graphics"/> makes a spawn draw from.</summary>
    public SpawnTarget Classify(SemanticModel semantic, ExpressionSyntax graphics)
    {
        switch (graphics)
        {
            case ParenthesizedExpressionSyntax parenthesized:
                return Classify(semantic, parenthesized.Expression);
            case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.DefaultLiteralExpression):
                return SpawnTarget.SharedSheet;
            case DefaultExpressionSyntax:
                return SpawnTarget.SharedSheet;
            case BaseObjectCreationExpressionSyntax { ArgumentList.Arguments: [var owner, ..] } creation
                when semantic.GetSymbolInfo(creation).Symbol is IMethodSymbol { ContainingType: var created } && IsSpriteGraphics(created):
                return ClassifyOwner(semantic, owner.Expression);
            case InvocationExpressionSyntax invocation when semantic.GetSymbolInfo(invocation).Symbol is IMethodSymbol method:
                if (IsSpriteGraphics(method.ContainingType))
                {
                    return method.Name switch
                    {
                        "SharedIn" => SpawnTarget.SharedSheet,
                        // Another shading keeps the graphics' owner.
                        "In" when invocation.Expression is MemberAccessExpressionSyntax receiver => Classify(semantic, receiver.Expression),
                        _ => SpawnTarget.Default,
                    };
                }
                return Resolve(method.OriginalDefinition);
        }
        return semantic.GetSymbolInfo(graphics).Symbol switch
        {
            { } member when IsSpriteGraphics(member.ContainingType) => member.Name == "Emptied" ? SpawnTarget.SharedSheet : SpawnTarget.Default,
            IPropertySymbol or IFieldSymbol => Resolve(semantic.GetSymbolInfo(graphics).Symbol!.OriginalDefinition),
            _ => SpawnTarget.Default,
        };
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
            IPropertySymbol { IsStatic: true } sheet when IsOwnerType(sheet.ContainingType) => new SpawnTarget(false, null, sheet.Name),
            _ => SpawnTarget.Default,
        };

    /// <summary>Whether <paramref name="type"/> is SpriteGraphics.</summary>
    private bool IsSpriteGraphics(INamedTypeSymbol? type) =>
        SymbolEqualityComparer.Default.Equals(type?.OriginalDefinition, model.SpriteGraphics);

    /// <summary>Whether <paramref name="type"/> is SpawnedObjectOwner.</summary>
    private bool IsOwnerType(INamedTypeSymbol? type) =>
        SymbolEqualityComparer.Default.Equals(type?.OriginalDefinition, model.SpawnedObjectOwner);

    /// <summary>Where the graphics a property, field or method yields draw from, read from its initializer or expression body.</summary>
    private SpawnTarget Resolve(ISymbol member)
    {
        INamedTypeSymbol? scope = member.ContainingType is { } type ? model.BehaviourOf(type) : null;
        if (!model.IsSource(member) || !_visiting.Add(member))
            return SpawnTarget.Default with { Scope = scope };
        try
        {
            foreach (SyntaxReference reference in member.DeclaringSyntaxReferences)
            {
                ExpressionSyntax? value = reference.GetSyntax() switch
                {
                    PropertyDeclarationSyntax property => property.Initializer?.Value ?? property.ExpressionBody?.Expression,
                    VariableDeclaratorSyntax variable => variable.Initializer?.Value,
                    MethodDeclarationSyntax method => method.ExpressionBody?.Expression,
                    _ => null,
                };
                if (value is not null)
                {
                    SpawnTarget target = Classify(model.ModelOf(value.SyntaxTree), value);
                    return target with { Scope = target.Scope ?? scope };
                }
            }
            return SpawnTarget.Default with { Scope = scope };
        }
        finally
        {
            _visiting.Remove(member);
        }
    }
}
