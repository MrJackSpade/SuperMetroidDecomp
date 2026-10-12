using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Typed custom programs whose frame fields bypass the common operand interpreter.</summary>
internal static class EnemyVisualTypedProgramAudit
{
    /// <summary>Finds frame arms in Ceres elevator's typed instruction reader and audits Botwoon and Kraid typed frame domains against installed resources.</summary>
    internal static void Inspect(CSharpCompilation compilation, ResourceIndex exports,
        AuditReport report, List<EnemyVisualProgramAudit.ProgramRow> rows)
    {
        const string ceresSource = "csharp/src/SuperMetroid.Core/Game/CeresElevatorArrivalDefinitions.cs";
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

        // Botwoon's typed head and projectile animations (#641) draw through its presentation catalog.
        const string botwoonSource = "csharp/src/SuperMetroid.Core/Assets/BotwoonVisualDefinitions.cs";
        // The hidden head draws the shared empty map; Verification proves every timeline frame is one of these.
        EnemySpritemapDefinition[] botwoonHeadFrames = BotwoonVisualDefinitions.Frames();
        foreach (EnemySpritemapDefinition frame in botwoonHeadFrames)
        {
            report.Require(ResourceDomains.EnemyDisplay, nameof(BotwoonVisualDefinitions),
                ResourceIndex.Address(frame.Bank, frame.Pointer), botwoonSource, exports);
        }
        foreach (BotwoonProjectileFrame frame in BotwoonVisualDefinitions.ProjectileFrames)
        {
            ProjectileDefinitionAudit.RequireFrame(BotwoonVisualDefinitions.ProjectilePresentationKey(frame),
                nameof(BotwoonVisualDefinitions), botwoonSource, exports, report);
        }
        rows.Add(new(botwoonHeadFrames.Length + BotwoonVisualDefinitions.ProjectileFrames.Count));

        var heads = KraidHeadInstructionDefinitions.All.ToArray();
        foreach (var frame in heads.Where(frame => frame.Kind == KraidHeadInstructionKind.Frame))
            report.Consumers.Add(new(nameof(KraidHeadInstructionDefinitions),
                "csharp/src/SuperMetroid.Core/Game/KraidHeadInstructionDefinitions.cs",
                "typed frame exported and required by installation admission"));
        rows.Add(new(heads.Count(frame => frame.Kind == KraidHeadInstructionKind.Frame)));
    }
}
