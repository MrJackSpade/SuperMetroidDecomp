using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Tooling;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>Confirms the two identified interface-adapter gaps without running a cinematic or palette interpreter.</summary>
internal static class InterfacePresentationContractChecks
{
    /// <summary>Checks real interface loaders and verifies audit closure responds to new owners or changed routing.</summary>
    internal static void Run()
    {
        var document = new IntroDiscoveryActorSpriteDocument {
            Version = IntroDiscoveryActorSpriteFormat.Version,
            Frames = IntroDiscoveryActorSpriteDefinitions.Frames.ToDictionary(frame => frame.Name, _ => Array.Empty<SpriteVisualPart>()) };
        Stream Json() => new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options));
        IntroDiscoveryActorSpritePresentation actor = IntroDiscoveryActorSpritePresentation.Load(Json());
        actor.Draw(IntroDiscoveryActorSpriteDefinitions.Frames[0].Pose,
            new ScreenSprites(new SpriteArtwork(new SpriteArtworkId(SpriteArtworkKind.IntroObjects), new byte[SpriteArtwork.CharacterByteCount]),
                ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection)), new(0, 0), 0, true);
        document.Frames.Remove(IntroDiscoveryActorSpriteDefinitions.Frames[0].Name);
        bool rejected = false;
        try { _ = IntroDiscoveryActorSpritePresentation.Load(Json()); }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "the real cinematic loader rejects a missing required owned frame");

        var trees = InterfacePresentationContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source.Path),
                path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        SyntaxTree calls = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Frontend.Intro.Artwork;
            using SuperMetroid.Core.Fx.Artwork;
            using SuperMetroid.Core.Fx.PaletteFx;
            using SuperMetroid.Core.Hardware;
            class InterfaceConsumer {
                void Inspect(IIntroCinematicSpritePresentation sprites, IPaletteFxColorSource colors, SuperMetroid.Core.Frontend.Intro.CinematicSpritePose pose, PaletteFxColorKey key, SuperMetroid.Core.Assets.ScreenSprites oam) {
                    sprites.Draw(pose, oam, default, 0, true);
                    sprites.Draw(new SuperMetroid.Core.Frontend.Intro.CinematicSpritePose(26), oam, default, 0, true);
                    colors.TryReadColor(key, out _);
                }
            }
            """, path: "fixture-interface-presentation-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("InterfacePresentationFixture", sources,
            platforms.Append(typeof(IIntroCinematicSpritePresentation).Assembly.Location).Distinct().Select(path => MetadataReference.CreateFromFile(path)),
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
        Require(Inspect(compilation, false).UnresolvedCount == 3, "original interface adapter gaps must be visible first");
        AuditReport after = Inspect(compilation, true);
        Require(after.Classifications.Count == 2 && after.UnresolvedCount == 1 && after.Consumers.Count == 3,
            "reviewed interface operations qualify but an unowned cinematic frame remains a finding" + " (" + after.Describe() + ")");
        foreach (string extra in new[] {
            "class OtherSprite : SuperMetroid.Core.Frontend.Intro.Artwork.IIntroCinematicSpritePresentation { public void Draw(SuperMetroid.Core.Frontend.Intro.CinematicSpritePose pose, SuperMetroid.Core.Assets.ScreenSprites objects, SuperMetroid.Core.Spatial.Position16<SuperMetroid.Core.Spatial.OamPixels> origin, ushort paletteBits, bool originIsOnScreen) {} }",
            "class OtherColors : SuperMetroid.Core.Fx.Artwork.IPaletteFxColorSource { public bool TryReadColor(SuperMetroid.Core.Fx.PaletteFx.PaletteFxColorKey key, out SuperMetroid.Core.Hardware.Bgr555 color) { color = default; return false; } }" })
        {
            AuditReport revoked = Inspect(Compile(trees.Append(CSharpSyntaxTree.ParseText(extra, path: "fixture-unreviewed-interface-owner.cs"))), true);
            Require(revoked.Classifications.Count == 1 && revoked.UnresolvedCount == 2 && revoked.Consumers.Count == 3,
                "a new implementation revokes only its interface proof" + " (" + revoked.Describe() + ")");
        }
    }

    /// <summary>Fails the contract check with context for an unreviewed presentation adapter.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Interface presentation confirmation failed: " + reason);
    }
}
