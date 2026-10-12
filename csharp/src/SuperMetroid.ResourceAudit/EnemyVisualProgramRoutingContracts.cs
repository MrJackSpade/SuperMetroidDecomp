using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ResourceAudit;

/// <summary>The common enemy-interpreter methods that route instruction words to visual selectors.</summary>
internal static class EnemyVisualProgramRoutingContracts
{
    // Each routing method and the type that owns it.
    private static readonly (string Method, string Owner)[] Methods =
    [
        ("ReadEnemyInstructionMechanicsWord", "EnemyInstructionInterpreter"),
        ("ReadEnemyProjectileInstructionMechanicsWord", "EnemyProjectileEngine"),
        ("ReadEnemyVisualSelector", "EnemyInstructionInterpreter"),
        ("SetEnemyProjectileVisualOperand", "EnemyProjectileEngine"),
        ("DrawEnemySpritemap", "EnemyPresentation"),
        ("ProcessInstructions", "EnemyInstructionInterpreter"),
        ("ProcessCommonEnemyInstruction", "EnemyInstructionInterpreter"),
        ("TryProcessOwnedEnemyInstruction", "EnemyInstructionInterpreter"),
        ("ProcessEnemyProjectileInstructions", "EnemyProjectileEngine"),
    ];

    internal static void Inspect(CSharpCompilation compilation, AuditReport report)
    {
        var declared = compilation.SyntaxTrees.SelectMany(tree => tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            .SelectMany(type => type.Members.OfType<MethodDeclarationSyntax>()
                .Select(method => (Method: method.Identifier.ValueText, Owner: type.Identifier.ValueText)))
            .ToHashSet();
        foreach (var (name, owner) in Methods)
        {
            if (!declared.Contains((name, owner)))
                report.Gap(ResourceDomains.CompiledSelector, name, owner,
                    "Interpreter routing method is missing; update this audit's routing contract.");
        }
    }
}
