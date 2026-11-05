using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.FrameDefinitionsGenerator;

/// <summary>
/// A spawn whose object's frame behaviour replaces the spawner's graphics it was spawned with before the object first
/// draws (#1513), so nothing it spawns draws from its spawner's own sheet.
/// </summary>
/// <param name="Type">The type declaring the spawn.</param>
/// <param name="Method">The method spawning the object with its spawner's graphics.</param>
/// <param name="FrameBehaviour">The frame behaviour, in the same type, whose first statement replaces the object's graphics.</param>
internal sealed record RedrawnSpawn(string Type, string Method, string FrameBehaviour)
{
    /// <summary>
    /// The configured spawns: a Work Robot's laser installs the robot's graphics with its definition, and its flight's
    /// frame behaviour clears them on the laser's first pass, before it is drawn.
    /// </summary>
    public static IReadOnlyList<RedrawnSpawn> All { get; } =
    [
        new("WorkRobotBehavior", "SpawnWorkRobotLaser", "RunWorkRobotLaserFrameBehavior"),
    ];
}

/// <summary>Recognizes the <see cref="RedrawnSpawn"/> spawns and checks each against Core's source.</summary>
internal static class RedrawnSpawns
{
    /// <summary>Whether <paramref name="unit"/> is a configured spawn, whose spawner graphics no draw shows.</summary>
    public static bool IsRedrawn(CoreModel model, Unit unit) =>
        unit is { Kind: UnitKind.Member, Symbol: IMethodSymbol method }
        && RedrawnSpawn.All.Any(spawn => method.Name == spawn.Method && method.ContainingType.Name == spawn.Type && model.IsSource(method));

    /// <summary>
    /// The self-check: each configured spawn's method exists, and its frame behaviour's first statement sets the object's
    /// graphics, so the spawner's graphics are gone before any draw.
    /// </summary>
    /// <exception cref="InvalidDataException">A configured method is missing, or the frame behaviour does not start by replacing the graphics.</exception>
    public static void Require(CoreModel model)
    {
        foreach (RedrawnSpawn spawn in RedrawnSpawn.All)
        {
            INamedTypeSymbol[] types = [.. model.Types.Where(type => type.Name == spawn.Type)];
            if (types is not [var declaring])
                throw new InvalidDataException($"Expected one Core type {spawn.Type} for a redrawn spawn; found {types.Length}.");
            if (declaring.GetMembers(spawn.Method) is not [IMethodSymbol])
                throw new InvalidDataException($"{spawn.Type} declares no single {spawn.Method}, the redrawn spawn the generator names.");
            if (declaring.GetMembers(spawn.FrameBehaviour) is not [IMethodSymbol behaviour]
                || behaviour.DeclaringSyntaxReferences is not [var reference]
                || reference.GetSyntax() is not MethodDeclarationSyntax { Body.Statements: [ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax first }, ..] } declaration
                || first is not { RawKind: (int)SyntaxKind.SimpleAssignmentExpression, Left: MemberAccessExpressionSyntax { Name.Identifier.Text: "Graphics" } }
                || model.ModelOf(declaration.SyntaxTree).GetTypeInfo(first.Left).Type is not { } assigned
                || !SymbolEqualityComparer.Default.Equals(assigned.OriginalDefinition, model.SpriteGraphics))
            {
                throw new InvalidDataException(
                    $"{spawn.Type}.{spawn.FrameBehaviour} does not start by replacing its object's graphics, so {spawn.Method}'s spawns may draw with their spawner's.");
            }
        }
    }
}
