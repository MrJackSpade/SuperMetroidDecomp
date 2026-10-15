using SuperMetroid.Tooling;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

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
        ((IIntroCinematicSpritePresentation)actor).Draw(IntroDiscoveryActorSpriteDefinitions.Frames[0].Pointer, new OamBuffer(), new(0, 0), 0, true);
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
            using SuperMetroid.Core.Hardware;
            class InterfaceConsumer {
                void Inspect(IIntroCinematicSpritePresentation sprites, IPaletteFxColorSource colors, ushort pointer, OamBuffer oam) {
                    sprites.Draw(pointer, oam, 0, 0, 0, true);
                    sprites.Draw(0, oam, 0, 0, 0, true);
                    colors.TryReadColor(pointer, out _);
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
            "reviewed interface operations qualify but an unowned cinematic frame remains a finding");
        foreach (string extra in new[] {
            "class OtherSprite : SuperMetroid.Core.Frontend.Intro.Artwork.IIntroCinematicSpritePresentation { public void Draw(ushort pointer, SuperMetroid.Core.Hardware.OamBuffer oam, ushort x, ushort y, ushort paletteBits, bool originIsOnScreen) {} }",
            "class OtherColors : SuperMetroid.Core.Fx.Artwork.IPaletteFxColorSource { public bool TryReadColor(ushort pointer, out ushort color) { color = 0; return false; } }" })
        {
            AuditReport revoked = Inspect(Compile(trees.Append(CSharpSyntaxTree.ParseText(extra, path: "fixture-unreviewed-interface-owner.cs"))), true);
            Require(revoked.Classifications.Count == 1 && revoked.UnresolvedCount == 2 && revoked.Consumers.Count == 3,
                "a new implementation revokes only its interface proof");
        }
        const string router = "csharp/src/SuperMetroid.Core/Frontend/CeresCinematics/Artwork/CeresDestructionSpritePresentation.cs";
        var stale = trees.Select(tree => tree.FilePath == router ? CSharpSyntaxTree.ParseText(tree.GetText().ToString()
            .Replace("? destruction : flight", "? flight : destruction", StringComparison.Ordinal), path: tree.FilePath) : tree);
        Require(Inspect(Compile(stale), true).Classifications.Count == 1, "changed combined-scene routing revokes cinematic closure");
        const string palette = "csharp/src/SuperMetroid.Core/Fx/Artwork/RoomPaletteFxPresentation.cs";
        stale = trees.Select(tree => tree.FilePath == palette ? CSharpSyntaxTree.ParseText(tree.GetText().ToString()
            .Replace("colors.TryGetValue(pointer, out color)", "throw new InvalidDataException()", StringComparison.Ordinal), path: tree.FilePath) : tree);
        Require(Inspect(Compile(stale), true).Classifications.Count == 1, "changed palette query semantics revoke its proof");
    }

    /// <summary>Fails the contract check with context for an unreviewed presentation adapter.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Interface presentation confirmation failed: " + reason);
    }
}
