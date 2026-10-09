using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ResourceAudit;

/// <summary>Revokes the reviewed common-interpreter routing proof when its code changes.</summary>
internal static class EnemyVisualProgramRoutingContracts
{
    /// <summary>Expected source fingerprints for shared enemy visual instruction routing methods.</summary>
    private static readonly Dictionary<string, string> Methods = new()
    {
        ["ReadEnemyInstructionMechanicsWord"] = "D9A4F9CB3B767BF4A16555EE799AAEA8E069CD5FC1221F57718526E04C8CCAC4",
        ["ReadEnemyProjectileInstructionMechanicsWord"] = "F334DE86B660B79F3AE5D9A073534C75BE4D6550182A30627B79E3703E318BC7",
        ["ReadEnemyVisualSelector"] = "C868EC05247BA3AFCFE623B8069E961E73C7344F6224149F8922706B56BB6167",
        ["SetEnemyProjectileVisualOperand"] = "EE891D65133B250AE6404621D0F69587B2B0F4246BBFE0962D66C44C9F71C4D1",
        ["DrawEnemySpritemap"] = "48A6E01227A86D44E60F98951E91F536A27E8C76ECCCAE5832CDC2D0073FBB75",
        ["ProcessInstructions"] = "65C805A5A6863AC871B5330FE103A57B1493E68EAB3A8F52A27512115B72D03B",
        ["ProcessEnemyProjectileInstructions"] = "6169FEBA0A8D8CB8550BCA059295013FBB46EA7EDA37383A2D20224976A92F50",
    };

    /// <summary>Compares common interpreter routing methods with their reviewed fingerprints.</summary>
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
