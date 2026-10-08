using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Typed custom programs whose frame fields bypass the common operand interpreter.</summary>
internal static class EnemyVisualTypedProgramAudit
{
    internal static void Inspect(string root, CSharpCompilation compilation, ResourceIndex exports,
        AuditReport report, List<EnemyVisualProgramAudit.ProgramRow> rows)
    {
        const string ceresSource = "csharp/src/SuperMetroid.Core/Game/CeresElevatorArrivalDefinitions.cs";
        EnemyVisualProgramSpecializations.GuardSource(root, ceresSource,
            "34EB4625AAA6B2EDBEEDF28C6C63C1607A02885707758F4D7CE3E705BD5892D0");
        SyntaxTree tree = compilation.SyntaxTrees.Single(tree => tree.FilePath == ceresSource);
        SemanticModel model = compilation.GetSemanticModel(tree);
        MethodDeclarationSyntax reader = tree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Single(method => method.Identifier.ValueText == "ReadInstruction");
        int ceresFrames = 0, records = 0;
        foreach (SwitchExpressionArmSyntax arm in reader.DescendantNodes().OfType<SwitchExpressionArmSyntax>())
        {
            if (arm.Pattern is DiscardPatternSyntax && arm.Expression is ThrowExpressionSyntax) continue;
            if (arm.Pattern is not ConstantPatternSyntax pattern || model.GetConstantValue(pattern.Expression) is not { HasValue: true, Value: not null } constant)
                throw new InvalidDataException("Ceres elevator static instruction domain changed.");
            var instruction = CeresElevatorArrivalDefinitions.ReadInstruction(Convert.ToUInt16(constant.Value));
            records++;
            if (instruction.Operation != CeresElevatorProjectileOperation.Frame) continue;
            report.Require(ResourceDomains.EnemyProjectileSprite, nameof(CeresElevatorArrivalDefinitions),
                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, instruction.SpritemapPointer), ceresSource, exports);
            ceresFrames++;
        }
        rows.Add(new(ceresFrames));

        // Import and installation admission both derive the exact head-map key set
        // from these typed frame records. Guard the reviewed producer/loader source;
        // this is a closed provider proof, not an independently duplicated pointer list.
        EnemyVisualProgramSpecializations.GuardSource(root,
            "csharp/src/SuperMetroid.AssetExtraction/EnemyTileArtworkFiles.cs",
            "684564292FA2DEAE104D897E1B7210F65CC55B6799F1970C5A654E2E41968E9B");
        var heads = KraidHeadInstructionDefinitions.All.ToArray();
        foreach (var frame in heads.Where(frame => frame.Kind == KraidHeadInstructionKind.Frame))
            report.Consumers.Add(new(nameof(KraidHeadInstructionDefinitions),
                "csharp/src/SuperMetroid.Core/Game/KraidHeadInstructionDefinitions.cs",
                "typed frame exported and required by installation admission"));
        rows.Add(new(heads.Count(frame => frame.Kind == KraidHeadInstructionKind.Frame)));
    }
}
