using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ResourceAudit;

/// <summary>The common enemy-interpreter methods that route instruction words to visual selectors.</summary>
internal static class EnemyVisualProgramRoutingContracts
{
    private static readonly string[] Methods =
    [
        "ReadEnemyInstructionMechanicsWord",
        "ReadEnemyProjectileInstructionMechanicsWord",
        "ReadEnemyVisualSelector",
        "SetEnemyProjectileVisualOperand",
        "DrawEnemySpritemap",
        "ProcessInstructions",
        "ProcessCommonEnemyInstruction",
        "TryProcessOwnedEnemyInstruction",
        "ProcessEnemyProjectileInstructions",
    ];

    internal static void Inspect(CSharpCompilation compilation, AuditReport report)
    {
        MethodDeclarationSyntax[] methods = compilation.SyntaxTrees.SelectMany(tree => tree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>().Where(type => type.Identifier.ValueText == "RoomEnemySystem")
            .SelectMany(type => type.Members.OfType<MethodDeclarationSyntax>())).ToArray();
        foreach (string name in Methods)
        {
            if (!methods.Any(method => method.Identifier.ValueText == name))
                report.Gap(ResourceDomains.CompiledSelector, name, "RoomEnemySystem",
                    "Interpreter routing method is missing; update this audit's routing contract.");
        }
    }
}
