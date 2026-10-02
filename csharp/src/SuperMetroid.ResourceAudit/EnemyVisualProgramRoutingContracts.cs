using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SuperMetroid.ResourceAudit;

/// <summary>Revokes the reviewed common-interpreter routing proof when its code changes.</summary>
internal static class EnemyVisualProgramRoutingContracts
{
    private static readonly Dictionary<string, string> Methods = new()
    {
        ["ReadEnemyInstructionMechanicsWord"] = "78C846F138FBD864577FC5B370924917A2AB4C55C287D73100C5637182DD70CE",
        ["ReadEnemyProjectileInstructionMechanicsWord"] = "0DFF0A03BFCE9607A45D28FA81E1B158023D6E1D9CFDCE682C587F0BA45240A9",
        ["ReadEnemyVisualSelector"] = "6FB5ADC8C63B8D832AE601B1B7C4EEEA018661AF7F54DAD526A6C539AA5AFC34",
        ["SetEnemyProjectileVisualOperand"] = "7769ECC83131B76FAEF68F04B0287AE7DB2E148B966162AA430CA4BB4EC0E701",
        ["DrawEnemySpritemap"] = "FF3713D52CC84D611BF9FA437F66F910A8AA8A85EEC5AC623C19246C599F3DBD",
        ["ProcessInstructions"] = "DF37BF0A5EF5A7936D7F8AA3479A4C1BEF5524786309361310F23D0623C94444",
        ["ProcessEnemyProjectileInstructions"] = "158D73DA8E0B49FF52984E8E67FE6470B8D865E5A02FAFAE3D69B895242B9856",
    };

    internal static void Inspect(CSharpCompilation compilation, AuditReport report)
    {
        MethodDeclarationSyntax[] methods = compilation.SyntaxTrees.SelectMany(tree => tree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>().Where(type => type.Identifier.ValueText == "RoomEnemySystem")
            .SelectMany(type => type.Members.OfType<MethodDeclarationSyntax>())).ToArray();
        foreach ((string name, string expected) in Methods)
        {
            MethodDeclarationSyntax method = methods.Single(method => method.Identifier.ValueText == name);
            string tokens = string.Join("\n", method.DescendantTokens().Select(token => token.Text));
            string actual = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tokens)));
            if (actual != expected)
                report.Gap(ResourceDomains.CompiledSelector, name, method.SyntaxTree.FilePath,
                    "Interpreter routing changed; review this audit's source contract. Token SHA256=" + actual);
        }
    }
}
