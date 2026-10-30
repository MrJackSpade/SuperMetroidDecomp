using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Frontend;

/// <summary>Confirms identified title/opening/Ceres resource boundaries with source-only fixtures.</summary>
internal static class CinematicPresentationContractChecks
{
    /// <summary>Compiles cinematic consumers with and without the closed adapter and checks valid and invalid ownership decisions.</summary>
    internal static void Run()
    {
        var trees = CinematicClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        const string consumer = """
            using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
            using SuperMetroid.Core.Areas.Ceres;
            using SuperMetroid.Core.Areas.Ceres.Artwork;
            using SuperMetroid.Core.Areas.Escape;
            using SuperMetroid.Core.Escape;
            using SuperMetroid.Core.Escape.Artwork;
            using SuperMetroid.Core.Frontend.Intro.Artwork;
            using SuperMetroid.Core.Frontend.Title;
            using SuperMetroid.Core.Frontend.Title.Artwork;
            class CinematicConsumer {
                void Inspect(TitlePalettePresentation palette, TitleGradientPresentation gradient,
                    TitleGraphicsPresentation title, IntroEyeTilemapPresentation eye,
                    CinematicSpriteSheet caret, CinematicSpriteSheet motherBrain,
                    CinematicSpriteSheet explosions, CeresDoorVisuals door,
                    EscapeTimerPresentation timer,
                    EscapeTimer countdown, SuperMetroid.Core.Assets.BackgroundColourSets backgrounds, SuperMetroid.Core.Rendering.ScreenPalette screen, SuperMetroid.Core.Assets.ScreenSprites oam,
                    SuperMetroid.Core.Assets.SpriteFrameBuffer sprites, SuperMetroid.Core.Assets.SpriteSheet timerSheet,
                    SuperMetroid.Core.Assets.ScreenSprites titleSprites, SuperMetroid.Core.Assets.SpriteSheet sheet) {
                    palette.Apply(screen);
                    gradient.Resolve(ushort.MaxValue);
                    TitleGraphicsPresentation.DrawSprite(sheet, TitleSprite.SuperMetroidLogo, titleSprites, default, SuperMetroid.Core.Frontend.ScreenColourGroup.TitleLogo);
                    TitleGraphicsPresentation.DrawSprite(sheet, (TitleSprite)(-1), titleSprites, default, SuperMetroid.Core.Frontend.ScreenColourGroup.TitleLogo);
                    TitleGraphicsPresentation.DrawSprite(sheet, (TitleSprite)31, titleSprites, default, SuperMetroid.Core.Frontend.ScreenColourGroup.TitleLogo);
                    eye.FrameTiles(IntroEyeTilemapFormat.FrameCount - 1);
                    eye.FrameTiles(IntroEyeTilemapFormat.FrameCount);
                    caret.Draw(new SuperMetroid.Core.Frontend.Intro.CinematicSpritePose(25), oam, default, SuperMetroid.Core.Frontend.ScreenColourGroup.IntroCrossFadeObjects);
                    caret.Draw(new SuperMetroid.Core.Frontend.Intro.CinematicSpritePose(999), oam, default, SuperMetroid.Core.Frontend.ScreenColourGroup.IntroCrossFadeObjects);
                    motherBrain.Draw(new SuperMetroid.Core.Frontend.Intro.CinematicSpritePose(0), oam, default, SuperMetroid.Core.Frontend.ScreenColourGroup.IntroMotherBrain);
                    motherBrain.Draw(new SuperMetroid.Core.Frontend.Intro.CinematicSpritePose(998), oam, default, SuperMetroid.Core.Frontend.ScreenColourGroup.IntroMotherBrain);
                    explosions.Draw(new SuperMetroid.Core.Frontend.Intro.CinematicSpritePose(67), oam, default, SuperMetroid.Core.Frontend.ScreenColourGroup.IntroExplosions);
                    explosions.Draw(new SuperMetroid.Core.Frontend.Intro.CinematicSpritePose(997), oam, default, SuperMetroid.Core.Frontend.ScreenColourGroup.IntroExplosions);
                    _ = door.Characters;
                    door.LoadNormalColors(screen.SpriteSets.Set(2));
                    door.LoadEscapeColors(screen.SpriteSets.Set(7));
                    door.LoadAnimationColors(backgrounds, CeresDoorVisualDefinitions.AnimationRowCount - 1);
                    door.LoadAnimationColors(backgrounds, CeresDoorVisualDefinitions.AnimationRowCount);
                    timer.Draw(countdown, sprites, timerSheet);
                }
            }
            """;
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-cinematic-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("CinematicProviderFixture", sources,
            platforms.Append(typeof(TitleGraphicsPresentation).Assembly.Location).Append(typeof(IntroEyeTilemapPresentation).Assembly.Location).Distinct()
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
        FixtureConsumerBinding.Require(compilation, calls, "CinematicPresentationContractChecks");
        AuditReport before = Inspect(compilation, false), after = Inspect(compilation, true);
        Require(before.UnresolvedCount == 18, "eighteen constructed calls must reproduce absent adapters first" + " (" + before.Describe() + ")");
        Require(after.Classifications.Count == 11 && after.UnresolvedCount == 7 && after.MissingCount == 0 &&
            after.Consumers.Count == 18, "supported sets and valid false membership queries qualify; unowned poses and bad array indices fail" + " (" + after.Describe() + ")");
        Require(after.Findings.Count(item => item.Owner == "TitleGraphicsPresentation.DrawSprite") == 2,
            "undefined title compositions must not be accepted as loaded title compositions");
    }

    /// <summary>Raises a contract failure with the specific source-only fixture expectation.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Cinematic provider confirmation failed: " + reason);
    }
}
