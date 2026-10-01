using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirms identified title/opening/Ceres resource boundaries with source-only fixtures.</summary>
internal static class CinematicPresentationContractChecks
{
    internal static void Run()
    {
        var trees = CinematicClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.IO; " +
            "global using System.Linq; global using System.Collections.Generic;", path: "<cinematic-usings>"));
        const string consumer = """
            using SuperMetroid.Core.Assets;
            using SuperMetroid.Core.Game;
            using SuperMetroid.Core.Frontend;
            using SuperMetroid.Core.Hardware;
            class CinematicConsumer {
                void Inspect(TitlePalettePresentation palette, TitleGradientPresentation gradient,
                    TitleGraphicsPresentation title, IntroEyeTilemapPresentation eye,
                    IntroCaretSpritePresentation caret, IntroMotherBrainSpritePresentation motherBrain,
                    IntroMotherBrainExplosionSpritePresentation explosions, CeresDoorVisualCatalog door,
                    EscapeTimerPresentation timer, CeresEscapeOverlayTilemapCatalog overlay,
                    EscapeTimer countdown, SnesVram vram, SnesCgram cgram, OamBuffer oam) {
                    palette.Apply(cgram);
                    gradient.Resolve(ushort.MaxValue);
                    title.DrawSprite(TitleSequenceRomData.Sprites.SuperMetroidLogo, oam, 0, 0, 0);
                    title.DrawSprite(TitleSequenceRomData.Sprites.Blank, oam, 0, 0, 0);
                    title.DrawSprite(ushort.MaxValue, oam, 0, 0, 0);
                    eye.FrameWords(IntroEyeTilemapFormat.FrameCount - 1);
                    eye.FrameWords(IntroEyeTilemapFormat.FrameCount);
                    caret.Draw(IntroCaretSpriteDefinitions.Still, oam, 0, 0, 0);
                    caret.Draw(IntroCaretSpriteDefinitions.Still + 1, oam, 0, 0, 0);
                    motherBrain.Draw(IntroMotherBrainSpriteDefinitions.FrameTwo, oam, 0, 0, 0);
                    motherBrain.Draw(IntroMotherBrainSpriteDefinitions.FrameTwo + 1, oam, 0, 0, 0);
                    explosions.Draw(IntroMotherBrainExplosionSpriteDefinitions.BigStart, oam, 0, 0, 0);
                    explosions.Draw(IntroMotherBrainExplosionSpriteDefinitions.End, oam, 0, 0, 0);
                    door.LoadTiles(vram);
                    door.LoadNormalColors(cgram, CeresDoorVisualRomData.NormalTargetColor);
                    door.LoadEscapeColors(cgram, CeresDoorVisualRomData.ActiveTargetColor);
                    door.LoadAnimationColors(cgram, CeresDoorVisualRomData.AnimationRowCount - 1);
                    door.LoadAnimationColors(cgram, CeresDoorVisualRomData.AnimationRowCount);
                    door.LoadMode7DoorFrame(vram, CeresDoorVisualRomData.Mode7FrameCount - 1);
                    door.LoadMode7DoorFrame(vram, CeresDoorVisualRomData.Mode7FrameCount);
                    timer.Draw(countdown, oam);
                    overlay.TryResolve(CeresEscapeOverlayTilemapDefinitions.Emergency.SourceAddress, 18, out _);
                    overlay.TryResolve(CeresEscapeOverlayTilemapDefinitions.Emergency.SourceAddress, 0, out _);
                    overlay.TryResolve(0, 0, out _);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-cinematic-consumer.cs");
        trees.Add(calls);
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("CinematicProviderFixture", sources,
            platforms.Append(typeof(TitleGraphicsPresentation).Assembly.Location).Distinct()
                .Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        AuditReport Inspect(CSharpCompilation compilation, bool adapted)
        {
            var exports = new ResourceIndex();
            var adapter = adapted ? new ClosedPresentationAudit(compilation, exports) : null;
            var report = new AuditReport();
            foreach (InvocationExpressionSyntax call in calls.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
                ConsumerAudit.Inspect(call, compilation.GetSemanticModel(calls), exports, report, adapter);
            return report;
        }
        CSharpCompilation compilation = Compile(trees);
        AuditReport before = Inspect(compilation, false), after = Inspect(compilation, true);
        Require(before.UnresolvedCount == 24, "twenty-four constructed calls must reproduce absent adapters first");
        Require(after.Classifications.Count == 16 && after.UnresolvedCount == 8 && after.MissingCount == 0 &&
            after.Consumers.Count == 24, "supported sets and valid false membership queries qualify; unowned pointers and bad array indices fail");
        Require(after.Classifications.Count(item => item.Owner == "CeresEscapeOverlayTilemapCatalog.TryResolve") == 3,
            "unsupported overlay source/length queries validly return false, not demand nonexistent artwork");
        Require(after.Findings.Count(item => item.Owner == "TitleGraphicsPresentation.DrawSprite") == 2,
            "blank timed entries and arbitrary pointers must not be accepted as loaded title compositions");

        SyntaxTree externalUse = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Assets;
            class ExternalTimerAnchors {
                void Change() { EscapeTimerPresentationDefinitions.AnchorNames[0] = "Absent.Anchor"; }
            }
            """, path: "fixture-external-timer-anchors.cs");
        AuditReport revoked = Inspect(Compile(trees.Append(externalUse)), true);
        Require(revoked.Classifications.Count == 15 && revoked.UnresolvedCount == 9 &&
            revoked.Findings.Any(item => item.Owner == "EscapeTimerPresentation.Draw" &&
                item.Message.Contains("stale", StringComparison.Ordinal)),
            "external access to mutable AnchorNames must revoke the fixed timer-anchor proof");
    }

    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Cinematic provider confirmation failed: " + reason);
    }
}
