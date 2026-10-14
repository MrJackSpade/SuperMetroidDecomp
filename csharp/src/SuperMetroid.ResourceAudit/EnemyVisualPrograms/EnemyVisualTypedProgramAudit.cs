using SuperMetroid.Core.Enemies.Polyp.Artwork;
using SuperMetroid.Core.Enemies.Polyp;
using SuperMetroid.Core.Enemies.Kzan.Artwork;
using SuperMetroid.Core.Enemies.Kzan;
using SuperMetroid.Core.Enemies.Kago.Artwork;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.Flies.Artwork;
using SuperMetroid.Core.Enemies.Flies;
using SuperMetroid.Core.Enemies.Nuclear.Artwork;
using SuperMetroid.Core.Enemies.Nuclear;
using SuperMetroid.Core.Enemies.Zoa.Artwork;
using SuperMetroid.Core.Enemies.Zoa;
using SuperMetroid.Core.Enemies.Ripper.Artwork;
using SuperMetroid.Core.Enemies.Ripper;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Kraid.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.EnemyVisualPrograms;

/// <summary>Typed custom programs whose frame fields bypass the common operand interpreter.</summary>
internal static class EnemyVisualTypedProgramAudit
{
    /// <summary>Requires each Ceres elevator arrival pose's spritemap and audits Botwoon and Kraid typed frame domains against installed resources.</summary>
    internal static void Inspect(ResourceIndex exports,
        AuditReport report, List<EnemyVisualProgramAudit.ProgramRow> rows)
    {
        // The Ceres elevator arrival's typed programs (#641) draw these poses.
        const string ceresSource = "csharp/src/SuperMetroid.Core/Areas/Ceres/CeresElevatorArrivalDefinitions.cs";
        int ceresFrames = 0;
        foreach (CeresElevatorArrivalPose pose in Enum.GetValues<CeresElevatorArrivalPose>())
        {
            report.Require(ResourceDomains.EnemyProjectileSprite, nameof(CeresElevatorArrivalDefinitions),
                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, CeresElevatorArrivalDefinitions.Spritemap(pose)), ceresSource, exports);
            ceresFrames++;
        }
        rows.Add(new(ceresFrames));

        // Botwoon's typed head and projectile animations (#641) draw through its presentation catalog.
        const string botwoonSource = "csharp/src/SuperMetroid.Core/Bosses/Botwoon/Artwork/BotwoonVisualDefinitions.cs";
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

        // Typed boss programs (#641) name their frames' installed spritemaps directly; each distinct frame a program
        // shows must be an exported enemy display resource in its bank.
        void RequireTypedFrames<TProgram, TFrame, TCue>(AnimationProgramSet<TProgram, TFrame, TCue> programs,
            Func<TFrame, ushort> spritemap, byte bank, string owner, string source)
            where TProgram : struct, Enum
            where TFrame : struct
            where TCue : notnull
        {
            ushort[] frames = [.. Enum.GetValues<TProgram>().SelectMany(program => programs.Steps(program))
                .Where(step => step.Kind == AnimationProgramStepKind.Show)
                .Select(step => spritemap(step.Frame)).Distinct()];
            foreach (ushort frame in frames)
                report.Require(ResourceDomains.EnemyDisplay, owner, ResourceIndex.Address(bank, frame), source, exports);
            rows.Add(new(frames.Length));
        }

        RequireTypedFrames(CrocomireBodyAnimations.Programs, frame => frame.Spritemap, CrocomireBodyVisualDefinitions.Bank,
            nameof(CrocomireBodyAnimations), "csharp/src/SuperMetroid.Core/Bosses/Crocomire/CrocomireBodyAnimations.cs");
        RequireTypedFrames(DraygonAnimations.Programs, frame => frame.Spritemap, DraygonBg2FrameDefinitions.Bank,
            nameof(DraygonAnimations), "csharp/src/SuperMetroid.Core/Bosses/Draygon/DraygonAnimations.cs");
        RequireTypedFrames(KraidAnimations.Programs, frame => frame.Spritemap, KraidLintVisualDefinitions.Bank,
            nameof(KraidAnimations), "csharp/src/SuperMetroid.Core/Bosses/Kraid/KraidAnimations.cs");
        RequireTypedFrames(MotherBrainAnimations.Body, frame => frame.Spritemap, MotherBrainBodyVisualDefinitions.Bank,
            nameof(MotherBrainAnimations), "csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainAnimations.cs");
        RequireTypedFrames(MotherBrainAnimations.Head, frame => frame.Spritemap, MotherBrainVisualDefinitions.Bank,
            nameof(MotherBrainAnimations), "csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainAnimations.cs");
        RequireTypedFrames(MotherBrainBabyAnimations.Programs, frame => frame.Spritemap, MotherBrainVisualDefinitions.Bank,
            nameof(MotherBrainBabyAnimations), "csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainAnimations.cs");
        RequireTypedFrames(MotherBrainTubeAnimations.Programs, frame => frame.Spritemap, MotherBrainVisualDefinitions.Bank,
            nameof(MotherBrainTubeAnimations), "csharp/src/SuperMetroid.Core/Bosses/MotherBrain/MotherBrainTubeAnimations.cs");
        RequireTypedFrames(ShitroidAnimations.Programs, frame => frame.Spritemap, ShitroidVisualDefinitions.Bank,
            nameof(ShitroidAnimations), "csharp/src/SuperMetroid.Core/Bosses/MotherBrain/ShitroidAnimations.cs");
        RequireTypedFrames(ZebetiteAnimations.Programs, frame => frame.Spritemap, ZebetiteVisualDefinitions.Bank,
            nameof(ZebetiteAnimations), "csharp/src/SuperMetroid.Core/Bosses/MotherBrain/ZebetiteAnimations.cs");

        RequireTypedFrames(RipperAnimations.Programs, RipperFrameDefinitions.Spritemap, RipperFrameDefinitions.Bank,
            nameof(RipperAnimations), "csharp/src/SuperMetroid.Core/Enemies/Ripper/RipperAnimations.cs");
        RequireTypedFrames(ZoaAnimations.Programs, ZoaFrameDefinitions.Spritemap, ZoaFrameDefinitions.Bank,
            nameof(ZoaAnimations), "csharp/src/SuperMetroid.Core/Enemies/Zoa/ZoaAnimations.cs");
        RequireTypedFrames(NuclearWaffleAnimations.Programs, NuclearWaffleFrameDefinitions.Spritemap, NuclearWaffleFrameDefinitions.Bank,
            nameof(NuclearWaffleAnimations), "csharp/src/SuperMetroid.Core/Enemies/Nuclear/NuclearWaffleAnimations.cs");
        RequireTypedFrames(FlyAnimations.Programs, FlyFrameDefinitions.Spritemap, FlyFrameDefinitions.Bank,
            nameof(FlyAnimations), "csharp/src/SuperMetroid.Core/Enemies/Flies/FlyAnimations.cs");
        RequireTypedFrames(KagoAnimations.Programs, KagoFrameDefinitions.Spritemap, KagoFrameDefinitions.Bank,
            nameof(KagoAnimations), "csharp/src/SuperMetroid.Core/Enemies/Kago/KagoAnimations.cs");
        RequireTypedFrames(KzanAnimations.Programs, KzanFrameDefinitions.Spritemap, KzanFrameDefinitions.Bank,
            nameof(KzanAnimations), "csharp/src/SuperMetroid.Core/Enemies/Kzan/KzanAnimations.cs");
        RequireTypedFrames(PolypAnimations.Programs, PolypFrameDefinitions.Spritemap, PolypFrameDefinitions.Bank,
            nameof(PolypAnimations), "csharp/src/SuperMetroid.Core/Enemies/Polyp/PolypAnimations.cs");
        var heads = KraidHeadInstructionDefinitions.All.ToArray();
        foreach (var frame in heads.Where(frame => frame.Kind == KraidHeadInstructionKind.Frame))
            report.Consumers.Add(new(nameof(KraidHeadInstructionDefinitions),
                "csharp/src/SuperMetroid.Core/Bosses/Kraid/KraidHeadInstructionDefinitions.cs",
                "typed frame exported and required by installation admission"));
        rows.Add(new(heads.Count(frame => frame.Kind == KraidHeadInstructionKind.Frame)));
    }
}
