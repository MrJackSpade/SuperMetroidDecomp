using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ResourceAudit.Reports;

namespace SuperMetroid.ResourceAudit.EnemyVisualPrograms;

/// <summary>The common enemy-interpreter methods that route instruction words to visual selectors.</summary>
internal static class EnemyVisualProgramRoutingContracts
{
    /// <summary>Required interpreter and presentation entry points that connect compiled words to visual selectors.</summary>
    /// <remarks>Each routing method and the type that owns it.</remarks>
    private static readonly (string Method, string Owner)[] Methods =
    [
        ("PublishEnemySpritemap", "EnemyPresentation"),
        ("ProcessInstructions", "EnemyInstructionInterpreter"),
        ("ProcessEnemyProjectileInstructions", "EnemyProjectileEngine"),
    ];

    /// <summary>Reports an unresolved audit boundary if any expected routing method disappears from its owner type.</summary>
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
