using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.ResourceAudit.Resources;

namespace SuperMetroid.ResourceAudit.ClosedContracts.Samus;

/// <summary>Source-only confirmation of the identified projectile provider and partial-factory boundaries.</summary>
internal static class ProjectilePresentationContractChecks
{
    /// <summary>Compiles projectile consumers and checks valid provider coverage and stale partial-factory proofs.</summary>
    internal static void Run()
    {
        var trees = ProjectileClosedContractDefinitions.All.SelectMany(contract => contract.Sources)
            .DistinctBy(source => source.Path).Select(source => CSharpSyntaxTree.ParseText(
                File.ReadAllText(source.Path), path: source.Path)).ToList<SyntaxTree>();
        trees.Add(CoreGlobalUsings.Tree());
        string consumer = """
            using SuperMetroid.Core.Hardware;
            using SuperMetroid.Core.Samus.Artwork;
            using SuperMetroid.Core.Samus.Projectiles;
            class ProjectileConsumer {
                void Inspect(BeamTileCatalog beams, ChargeFlarePlacementCatalog placement,
                    ChargeFlareSpriteCatalog flare, ProjectileTrailCatalog trails,
                    ProjectileFrameBindingCatalog bindings, GrappleSpriteCatalog grapple,
                    GrappleSwingFrameCatalog swing, ProjectileSpriteCatalog sprites, OamBuffer oam) {
                    beams.Resolve(VramAssetId.BeamPowerTiles);
                    beams.Resolve(VramAssetId.BeamPlasmaIceWaveTiles);
                    beams.Resolve(VramAssetId.StandardHudTiles);
                    placement.Resolve(false, 15);
                    placement.Resolve(true, 16);
                    flare.Frame(53);
                    flare.Frame(54);
                    trails.Resolve(ProjectileTrailDefinitions.LeftIce);
                    trails.Resolve(ProjectileTrailDefinitions.LeftIce + 1);
                    trails.ResolveCurrent(ProjectileTrailDefinitions.LeftIce, 0);
                    trails.ResolveCurrent(ProjectileTrailDefinitions.LeftIce + 4, 0);
                    trails.ResolveCurrent(ProjectileTrailDefinitions.LeftIce + 5, 0);
                    bindings.Resolve(FIRST_TIMED);
                    bindings.Resolve(0);
                    grapple.Segment(3);
                    grapple.Segment(4);
                    swing.Resolve(byte.MaxValue);
                    sprites.Frame(ProjectileSpriteDefinitions.NativePointers[0]);
                    sprites.Frame(0);
                }
            }
            """.Replace("FIRST_TIMED", SamusProjectileRecordDefinitions.TimedRecordAddresses[0].ToString(
                System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        SyntaxTree calls = CSharpSyntaxTree.ParseText(consumer, path: "fixture-projectile-consumer.cs");
        trees.Add(calls);
        string[] platforms = RequiredReflection.TrustedPlatformAssemblies();
        CSharpCompilation Compile(IEnumerable<SyntaxTree> sources) => CSharpCompilation.Create("ProjectileProviderFixture", sources,
            platforms.Append(typeof(BeamTileCatalog).Assembly.Location).Distinct()
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
        Require(before.UnresolvedCount == 18 && before.MissingCount == 1,
            "nineteen constructed calls must first expose absent adapters and the known missing zero sprite (" + before.Describe() + ")");
        Require(after.Classifications.Count == 11 && after.UnresolvedCount == 8 && after.MissingCount == 0 &&
            after.Consumers.Count == 19, "complete sparse/ranged domains qualify, not neighboring frames or other asset types" + " (" + after.Describe() + ")");
        Require(after.Classifications.Count(item => item.Owner == "ProjectileTrailCatalog.ResolveCurrent") == 2 &&
            after.Findings.Count(item => item.Owner == "ProjectileTrailCatalog.ResolveCurrent") == 1,
            "retained stream starts and next-instruction frame offsets have distinct exact domains");

        SyntaxTree partialFactory = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Samus.Artwork;
            class ExternalPartialProjectile {
                object Make() => ProjectileSpriteCatalog.LoadFrames(null!, new ushort[] { 0 });
            }
            """, path: "fixture-external-projectile-factory.cs");
        AuditReport partial = Inspect(Compile(trees.Append(partialFactory)), true);
        Require(partial.Classifications.Count == 10 && partial.UnresolvedCount == 9 &&
            partial.Findings.Count(item => item.Owner == "ProjectileSpriteCatalog.Frame" &&
                item.Message.Contains("stale", StringComparison.Ordinal)) == 2,
            "external partial construction revokes ordinary coverage without revoking the private flare wrapper" + " (" + partial.Describe() + ")");

        SyntaxTree mutablePointers = CSharpSyntaxTree.ParseText("""
            class ExternalTimedPointers {
                void Change() { ((ushort[])SuperMetroid.Core.Samus.Projectiles.SamusProjectileRecordDefinitions.TimedRecordAddresses)[0] = 0; }
            }
            """, path: "fixture-external-projectile-pointers.cs");
        AuditReport mutable = Inspect(Compile(trees.Append(mutablePointers)), true);
        Require(mutable.Classifications.Count == 10 && mutable.UnresolvedCount == 9 &&
            mutable.Findings.Count(item => item.Owner == "ProjectileFrameBindingCatalog.Resolve" &&
                item.Message.Contains("stale", StringComparison.Ordinal)) == 2,
            "external reference to the array-backed timed pointer list revokes binding ownership" + " (" + mutable.Describe() + ")");
    }

    /// <summary>Reports which source-only projectile ownership expectation failed.</summary>
    private static void Require(bool valid, string reason)
    {
        if (!valid) throw new InvalidOperationException("Projectile provider confirmation failed: " + reason);
    }
}
