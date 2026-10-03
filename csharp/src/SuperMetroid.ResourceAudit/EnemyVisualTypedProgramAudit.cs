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
            "8EE8D83506305DD6372F18722C89D54E3E92CD1ED9A7AA98FEB874FC77402C88");
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
        rows.Add(new(nameof(CeresElevatorArrivalDefinitions), ResourceBanks.EnemyProjectilePrograms,
            "typed-projectile-records", ceresFrames, records));

        // Import and installation admission both derive the exact head-map key set
        // from these typed frame records. Guard the reviewed producer/loader source;
        // this is a closed provider proof, not an independently duplicated pointer list.
        EnemyVisualProgramSpecializations.GuardSource(root,
            "csharp/src/SuperMetroid.AssetExtraction/EnemyTileArtworkFiles.cs",
            "089FFD24D5F854A30042F9B7E68D8C97C346745B186C1A1EE018D6BAB176962D");
        var heads = KraidHeadInstructionDefinitions.All.ToArray();
        foreach (var frame in heads.Where(frame => frame.Kind == KraidHeadInstructionKind.Frame))
            report.Consumers.Add(new("kraid-head-bg2", nameof(KraidHeadInstructionDefinitions),
                "csharp/src/SuperMetroid.Core/Game/KraidHeadInstructionDefinitions.cs",
                ResourceIndex.Address(KraidBackgroundRomData.NativeBank >> 16, frame.Tilemap),
                "typed frame exported and required by installation admission"));
        rows.Add(new(nameof(KraidHeadInstructionDefinitions), KraidBackgroundRomData.NativeBank >> 16,
            "typed-bg2-records", heads.Count(frame => frame.Kind == KraidHeadInstructionKind.Frame), heads.Length));
    }
}
