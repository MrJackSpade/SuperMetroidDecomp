using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ResourceAudit;

/// <summary>Revokes the reviewed common-interpreter routing proof when its code changes.</summary>
internal static class EnemyVisualProgramRoutingContracts
{
    private static readonly Dictionary<string, string> Methods = new()
    {
        // #142 made the readers static and dropped unused parameters; routing is unchanged.
        ["ReadEnemyInstructionMechanicsWord"] = "9375D363FBE94B5030A9749AEF26D92CFCE3F232C9B869EE6C3161C2960CD9DA",
        ["ReadEnemyProjectileInstructionMechanicsWord"] = "F334DE86B660B79F3AE5D9A073534C75BE4D6550182A30627B79E3703E318BC7",
        ["ReadEnemyVisualSelector"] = "DDCA6A13A4F9F6278F5B19F1790A73E509DB240F1266136A88F5847EDB95D360",
        ["SetEnemyProjectileVisualOperand"] = "EE891D65133B250AE6404621D0F69587B2B0F4246BBFE0962D66C44C9F71C4D1",
        ["DrawEnemySpritemap"] = "48A6E01227A86D44E60F98951E91F536A27E8C76ECCCAE5832CDC2D0073FBB75",
        // #1275 re-pin: $A6:E4D2's low-energy branch also stores the Ridley timer; routing unchanged.
        // #627 re-pin: enemy identity aliases became EnemyDefinitionId members of equal value; routing unchanged.
        // #627 types the Baby Metroid goto opcodes; same cases and guards, typed call replaces the discarded bool.
        ["ProcessInstructions"] = "B083A4CB336D37A17403340EAC76B3DA6B7FBDBD06C298F7A3E57DC7113BCAA9",
        // #627 decodes the word into EnemyProjectileInstruction; same cases, guards and order.
        ["ProcessEnemyProjectileInstructions"] = "5EDA20D0F4E41D11C6AF1C3DC2242A661B60A4F808E141DB45DF2703C14367CC",
    };

    internal static void Inspect(CSharpCompilation compilation, AuditReport report)
    {
        MethodDeclarationSyntax[] methods = compilation.SyntaxTrees.SelectMany(tree => tree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>().Where(type => type.Identifier.ValueText == "RoomEnemySystem")
            .SelectMany(type => type.Members.OfType<MethodDeclarationSyntax>())).ToArray();
        foreach ((string name, string expected) in Methods)
        {
            MethodDeclarationSyntax method = methods.Single(method => method.Identifier.ValueText == name);
            string actual = SourceFingerprint.Of(method);
            if (actual != expected)
                report.Gap(ResourceDomains.CompiledSelector, name, method.SyntaxTree.FilePath,
                    "Interpreter routing changed; review this audit's source contract. Token SHA256=" + actual);
        }
    }
}
