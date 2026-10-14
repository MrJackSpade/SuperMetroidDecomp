using SuperMetroid.Core.Enemies.Hibashi.Artwork;
using SuperMetroid.Core.Enemies.Hibashi;
using SuperMetroid.Core.Enemies.Animals.Artwork;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Beetom.Artwork;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Rios.Artwork;
using SuperMetroid.Core.Enemies.Rios;
using SuperMetroid.Core.Enemies.YappingMaws.Artwork;
using SuperMetroid.Core.Enemies.YappingMaws;
using SuperMetroid.Core.Enemies.Evir.Artwork;
using SuperMetroid.Core.Enemies.Evir;
using SuperMetroid.Core.Enemies.Platforms.Artwork;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Puyo.Artwork;
using SuperMetroid.Core.Enemies.Puyo;
using SuperMetroid.Core.Enemies.MaridiaSnails.Artwork;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.Sparks.Artwork;
using SuperMetroid.Core.Enemies.Sparks;
using SuperMetroid.Core.Enemies.Powamp.Artwork;
using SuperMetroid.Core.Enemies.Powamp;
using SuperMetroid.Core.Enemies.Boyon.Artwork;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Skultera.Artwork;
using SuperMetroid.Core.Enemies.Skultera;
using SuperMetroid.Core.Enemies.Metroids.Artwork;
using SuperMetroid.Core.Enemies.Metroids;
using SuperMetroid.Core.Enemies.Skree.Artwork;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Enemies.MorphBallEye.Artwork;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.Dragon.Artwork;
using SuperMetroid.Core.Enemies.Dragon;
using SuperMetroid.Core.Enemies.FaceBlocks.Artwork;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.Cacatac.Artwork;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Stoke.Artwork;
using SuperMetroid.Core.Enemies.Stoke;
using SuperMetroid.Core.Enemies.Owtch.Artwork;
using SuperMetroid.Core.Enemies.Owtch;
using SuperMetroid.Core.Enemies.PipeBugs.Artwork;
using SuperMetroid.Core.Enemies.PipeBugs;
using SuperMetroid.Core.Enemies.Crawlers.Artwork;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Hoppers.Artwork;
using SuperMetroid.Core.Enemies.Hoppers;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.LavaJumpers.Artwork;
using SuperMetroid.Core.Enemies.LavaJumpers;
using SuperMetroid.Core.Enemies.Waver.Artwork;
using SuperMetroid.Core.Enemies.Waver;
using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Shutters.Artwork;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Choot.Artwork;
using SuperMetroid.Core.Enemies.Choot;
using SuperMetroid.Core.Enemies.Boulder.Artwork;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Bull.Artwork;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Atomic.Artwork;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Fireflea.Artwork;
using SuperMetroid.Core.Enemies.Fireflea;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Enemies.Sbug.Artwork;
using SuperMetroid.Core.Enemies.Sbug;
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
        RequireTypedFrames(SbugAnimations.Programs, SbugFrameDefinitions.Spritemap, SbugFrameDefinitions.Bank,
            nameof(SbugAnimations), "csharp/src/SuperMetroid.Core/Enemies/Sbug/SbugAnimations.cs");
        RequireTypedFrames(WreckedShipGhostAnimations.Programs, WreckedShipGhostFrameDefinitions.Spritemap, WreckedShipGhostFrameDefinitions.Bank,
            nameof(WreckedShipGhostAnimations), "csharp/src/SuperMetroid.Core/Enemies/WreckedShip/WreckedShipGhostAnimations.cs");
        RequireTypedFrames(FirefleaAnimations.Programs, FirefleaFrameDefinitions.Spritemap, FirefleaFrameDefinitions.Bank,
            nameof(FirefleaAnimations), "csharp/src/SuperMetroid.Core/Enemies/Fireflea/FirefleaAnimations.cs");
        RequireTypedFrames(AtomicAnimations.Programs, AtomicFrameDefinitions.Spritemap, AtomicFrameDefinitions.Bank,
            nameof(AtomicAnimations), "csharp/src/SuperMetroid.Core/Enemies/Atomic/AtomicAnimations.cs");
        RequireTypedFrames(BullAnimations.Programs, BullFrameDefinitions.Spritemap, BullFrameDefinitions.Bank,
            nameof(BullAnimations), "csharp/src/SuperMetroid.Core/Enemies/Bull/BullAnimations.cs");
        RequireTypedFrames(BoulderAnimations.Programs, BoulderFrameDefinitions.Spritemap, BoulderFrameDefinitions.Bank,
            nameof(BoulderAnimations), "csharp/src/SuperMetroid.Core/Enemies/Boulder/BoulderAnimations.cs");
        RequireTypedFrames(ChootAnimations.Programs, ChootFrameDefinitions.Spritemap, ChootFrameDefinitions.Bank,
            nameof(ChootAnimations), "csharp/src/SuperMetroid.Core/Enemies/Choot/ChootAnimations.cs");
        RequireTypedFrames(ShutterAnimations.Programs, ShutterFrameDefinitions.Spritemap, ShutterFrameDefinitions.Bank,
            nameof(ShutterAnimations), "csharp/src/SuperMetroid.Core/Enemies/Shutters/ShutterAnimations.cs");
        RequireTypedFrames(ElevatorAnimations.Programs, ElevatorFrameDefinitions.Spritemap, ElevatorFrameDefinitions.Bank,
            nameof(ElevatorAnimations), "csharp/src/SuperMetroid.Core/Enemies/Elevator/ElevatorAnimations.cs");
        RequireTypedFrames(WaverAnimations.Programs, WaverFrameDefinitions.Spritemap, WaverFrameDefinitions.Bank,
            nameof(WaverAnimations), "csharp/src/SuperMetroid.Core/Enemies/Waver/WaverAnimations.cs");
        RequireTypedFrames(NorfairLavaJumperAnimations.Programs, NorfairLavaJumperFrameDefinitions.Spritemap, NorfairLavaJumperFrameDefinitions.Bank,
            nameof(NorfairLavaJumperAnimations), "csharp/src/SuperMetroid.Core/Enemies/LavaJumpers/NorfairLavaJumperAnimations.cs");
        RequireTypedFrames(DeadTourianAnimations.Programs, DeadTourianFrameDefinitions.Spritemap, DeadTourianFrameDefinitions.Bank,
            nameof(DeadTourianAnimations), "csharp/src/SuperMetroid.Core/Enemies/Corpses/DeadTourianAnimations.cs");
        RequireTypedFrames(DeadTorizoAnimations.Programs, DeadTorizoFrameDefinitions.Spritemap, DeadTorizoFrameDefinitions.Bank,
            nameof(DeadTorizoAnimations), "csharp/src/SuperMetroid.Core/Enemies/Corpses/DeadTorizoAnimations.cs");
        RequireTypedFrames(HopperAnimations.Programs, HopperFrameDefinitions.Spritemap, HopperFrameDefinitions.Bank,
            nameof(HopperAnimations), "csharp/src/SuperMetroid.Core/Enemies/Hoppers/HopperAnimations.cs");
        RequireTypedFrames(MultiviolaAnimations.Programs, MultiviolaFrameDefinitions.Spritemap, MultiviolaFrameDefinitions.Bank,
            nameof(MultiviolaAnimations), "csharp/src/SuperMetroid.Core/Enemies/Crawlers/MultiviolaAnimations.cs");
        RequireTypedFrames(PipeBugAnimations.Programs, PipeBugFrameDefinitions.Spritemap, PipeBugFrameDefinitions.Bank,
            nameof(PipeBugAnimations), "csharp/src/SuperMetroid.Core/Enemies/PipeBugs/PipeBugAnimations.cs");
        RequireTypedFrames(OwtchAnimations.Programs, OwtchFrameDefinitions.Spritemap, OwtchFrameDefinitions.Bank,
            nameof(OwtchAnimations), "csharp/src/SuperMetroid.Core/Enemies/Owtch/OwtchAnimations.cs");
        RequireTypedFrames(StokeAnimations.Programs, StokeFrameDefinitions.Spritemap, StokeFrameDefinitions.Bank,
            nameof(StokeAnimations), "csharp/src/SuperMetroid.Core/Enemies/Stoke/StokeAnimations.cs");
        RequireTypedFrames(CacatacAnimations.Programs, CacatacFrameDefinitions.Spritemap, CacatacFrameDefinitions.Bank,
            nameof(CacatacAnimations), "csharp/src/SuperMetroid.Core/Enemies/Cacatac/CacatacAnimations.cs");
        RequireTypedFrames(BlueBrinstarFaceBlockAnimations.Programs, BlueBrinstarFaceBlockFrameDefinitions.Spritemap, BlueBrinstarFaceBlockFrameDefinitions.Bank,
            nameof(BlueBrinstarFaceBlockAnimations), "csharp/src/SuperMetroid.Core/Enemies/FaceBlocks/BlueBrinstarFaceBlockAnimations.cs");
        RequireTypedFrames(DragonAnimations.Programs, DragonFrameDefinitions.Spritemap, DragonFrameDefinitions.Bank,
            nameof(DragonAnimations), "csharp/src/SuperMetroid.Core/Enemies/Dragon/DragonAnimations.cs");
        RequireTypedFrames(CrawlerAnimations.Programs, CrawlerFrameDefinitions.Spritemap, CrawlerFrameDefinitions.Bank,
            nameof(CrawlerAnimations), "csharp/src/SuperMetroid.Core/Enemies/Crawlers/CrawlerAnimations.cs");
        RequireTypedFrames(MorphBallEyeAnimations.Programs, MorphBallEyeFrameDefinitions.Spritemap, MorphBallEyeFrameDefinitions.Bank,
            nameof(MorphBallEyeAnimations), "csharp/src/SuperMetroid.Core/Enemies/MorphBallEye/MorphBallEyeAnimations.cs");
        RequireTypedFrames(SkreeMetareeAnimations.Programs, SkreeMetareeFrameDefinitions.Spritemap, SkreeMetareeFrameDefinitions.Bank,
            nameof(SkreeMetareeAnimations), "csharp/src/SuperMetroid.Core/Enemies/Skree/SkreeMetareeAnimations.cs");
        RequireTypedFrames(MochtroidAnimations.Programs, MochtroidFrameDefinitions.Spritemap, MochtroidFrameDefinitions.Bank,
            nameof(MochtroidAnimations), "csharp/src/SuperMetroid.Core/Enemies/Metroids/MochtroidAnimations.cs");
        RequireTypedFrames(SkulteraAnimations.Programs, SkulteraFrameDefinitions.Spritemap, SkulteraFrameDefinitions.Bank,
            nameof(SkulteraAnimations), "csharp/src/SuperMetroid.Core/Enemies/Skultera/SkulteraAnimations.cs");
        RequireTypedFrames(BoyonAnimations.Programs, BoyonFrameDefinitions.Spritemap, BoyonFrameDefinitions.Bank,
            nameof(BoyonAnimations), "csharp/src/SuperMetroid.Core/Enemies/Boyon/BoyonAnimations.cs");
        RequireTypedFrames(PowampAnimations.Programs, PowampFrameDefinitions.Spritemap, PowampFrameDefinitions.Bank,
            nameof(PowampAnimations), "csharp/src/SuperMetroid.Core/Enemies/Powamp/PowampAnimations.cs");
        RequireTypedFrames(SparkAnimations.Programs, SparkFrameDefinitions.Spritemap, SparkFrameDefinitions.Bank,
            nameof(SparkAnimations), "csharp/src/SuperMetroid.Core/Enemies/Sparks/SparkAnimations.cs");
        RequireTypedFrames(MetroidAnimations.Programs, MetroidFrameDefinitions.Spritemap, MetroidFrameDefinitions.Bank,
            nameof(MetroidAnimations), "csharp/src/SuperMetroid.Core/Enemies/Metroids/MetroidAnimations.cs");
        RequireTypedFrames(MaridiaLargeSnailAnimations.Programs, MaridiaLargeSnailFrameDefinitions.Spritemap, MaridiaLargeSnailFrameDefinitions.Bank,
            nameof(MaridiaLargeSnailAnimations), "csharp/src/SuperMetroid.Core/Enemies/MaridiaSnails/MaridiaLargeSnailAnimations.cs");
        RequireTypedFrames(PuyoAnimations.Programs, PuyoFrameDefinitions.Spritemap, PuyoFrameDefinitions.Bank,
            nameof(PuyoAnimations), "csharp/src/SuperMetroid.Core/Enemies/Puyo/PuyoAnimations.cs");
        RequireTypedFrames(RinkaAnimations.Programs, RinkaFrameDefinitions.Spritemap, RinkaFrameDefinitions.Bank,
            nameof(RinkaAnimations), "csharp/src/SuperMetroid.Core/Bosses/MotherBrain/RinkaAnimations.cs");
        RequireTypedFrames(PlatformAnimations.Programs, PlatformFrameDefinitions.Spritemap, PlatformFrameDefinitions.Bank,
            nameof(PlatformAnimations), "csharp/src/SuperMetroid.Core/Enemies/Platforms/PlatformAnimations.cs");
        RequireTypedFrames(EvirAnimations.Programs, EvirFrameDefinitions.Spritemap, EvirFrameDefinitions.Bank,
            nameof(EvirAnimations), "csharp/src/SuperMetroid.Core/Enemies/Evir/EvirAnimations.cs");
        RequireTypedFrames(YappingMawAnimations.Programs, YappingMawFrameDefinitions.Spritemap, YappingMawFrameDefinitions.Bank,
            nameof(YappingMawAnimations), "csharp/src/SuperMetroid.Core/Enemies/YappingMaws/YappingMawAnimations.cs");
        RequireTypedFrames(RioAnimations.Programs, RioFrameDefinitions.Spritemap, RioFrameDefinitions.Bank,
            nameof(RioAnimations), "csharp/src/SuperMetroid.Core/Enemies/Rios/RioAnimations.cs");
        RequireTypedFrames(NorfairRioAnimations.Programs, NorfairRioFrameDefinitions.Spritemap, NorfairRioFrameDefinitions.Bank,
            nameof(NorfairRioAnimations), "csharp/src/SuperMetroid.Core/Enemies/Rios/NorfairRioAnimations.cs");
        RequireTypedFrames(LowerNorfairRioAnimations.Programs, LowerNorfairRioFrameDefinitions.Spritemap, LowerNorfairRioFrameDefinitions.Bank,
            nameof(LowerNorfairRioAnimations), "csharp/src/SuperMetroid.Core/Enemies/Rios/LowerNorfairRioAnimations.cs");
        RequireTypedFrames(BeetomAnimations.Programs, BeetomFrameDefinitions.Spritemap, BeetomFrameDefinitions.Bank,
            nameof(BeetomAnimations), "csharp/src/SuperMetroid.Core/Enemies/Beetom/BeetomAnimations.cs");
        RequireTypedFrames(AlcoonAnimations.Programs, AlcoonFrameDefinitions.Spritemap, AlcoonFrameDefinitions.Bank,
            nameof(AlcoonAnimations), "csharp/src/SuperMetroid.Core/Enemies/Alcoon/AlcoonAnimations.cs");
        RequireTypedFrames(FuneNamiheAnimations.Programs, FuneNamiheFrameDefinitions.Spritemap, FuneNamiheFrameDefinitions.Bank,
            nameof(FuneNamiheAnimations), "csharp/src/SuperMetroid.Core/Enemies/FuneNamihe/FuneNamiheAnimations.cs");
        RequireTypedFrames(DachoraAnimations.Programs, DachoraFrameDefinitions.Spritemap, DachoraFrameDefinitions.Bank,
            nameof(DachoraAnimations), "csharp/src/SuperMetroid.Core/Enemies/Animals/DachoraAnimations.cs");
        RequireTypedFrames(EtecoonAnimations.Programs, EtecoonFrameDefinitions.Spritemap, EtecoonFrameDefinitions.Bank,
            nameof(EtecoonAnimations), "csharp/src/SuperMetroid.Core/Enemies/Animals/EtecoonAnimations.cs");
        RequireTypedFrames(HibashiAnimations.Programs, HibashiFrameDefinitions.Spritemap, HibashiFrameDefinitions.Bank,
            nameof(HibashiAnimations), "csharp/src/SuperMetroid.Core/Enemies/Hibashi/HibashiAnimations.cs");
        var heads = KraidHeadInstructionDefinitions.All.ToArray();
        foreach (var frame in heads.Where(frame => frame.Kind == KraidHeadInstructionKind.Frame))
            report.Consumers.Add(new(nameof(KraidHeadInstructionDefinitions),
                "csharp/src/SuperMetroid.Core/Bosses/Kraid/KraidHeadInstructionDefinitions.cs",
                "typed frame exported and required by installation admission"));
        rows.Add(new(heads.Count(frame => frame.Kind == KraidHeadInstructionKind.Frame)));
    }
}
