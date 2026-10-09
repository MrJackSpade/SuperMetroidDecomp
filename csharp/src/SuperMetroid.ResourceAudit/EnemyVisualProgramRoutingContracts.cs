using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ResourceAudit;

/// <summary>Revokes the reviewed common-interpreter routing proof when its code changes.</summary>
internal static class EnemyVisualProgramRoutingContracts
{
    private static readonly Dictionary<string, string> Methods = new()
    {
        // #142 made the readers static and dropped unused parameters; routing is unchanged.
        ["ReadEnemyInstructionMechanicsWord"] = "DBCC5561CF0A363BAE2DCFF7D9D2F4AA81A56005543EEDD98C4AC1B800F35279",
        ["ReadEnemyProjectileInstructionMechanicsWord"] = "F334DE86B660B79F3AE5D9A073534C75BE4D6550182A30627B79E3703E318BC7",
        ["ReadEnemyVisualSelector"] = "1E86C23CB2F1E4DD2E4EFFD4375DD239899CF4E32F92EC71E095378CF662FCDE",
        ["SetEnemyProjectileVisualOperand"] = "EE891D65133B250AE6404621D0F69587B2B0F4246BBFE0962D66C44C9F71C4D1",
        ["DrawEnemySpritemap"] = "48A6E01227A86D44E60F98951E91F536A27E8C76ECCCAE5832CDC2D0073FBB75",
        ["ProcessInstructions"] = "E6E0BE09E2B3C390750413407B10F027EDFCEAA2DC6773A1A9146C4E8B60BE3D",
        ["ProcessEnemyProjectileInstructions"] = "DA3F876F76D717B2CA6A58D5CD4D3D02EF84631FCAC92B070FBAD89A2133A86E",
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
