using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;
using SuperMetroid.Core.Game;

namespace SuperMetroid.ResourceAudit;

/// <summary>Small synthetic confirmations of the audit itself; never searches gameplay.</summary>
internal static class AuditContractChecks
{
    public static void Run()
    {
        const string text = """
            namespace SuperMetroid.Core.Assets {
                public class EnemySpritemapCatalog {
                    public bool TryGetDisplay(byte bank, ushort pointer, out object parts) { parts = null!; return false; }
                }
                public class EnemyExtendedFrameCatalog {
                    internal bool TryGetDisplay(byte bank, ushort pointer, out object parts) { parts = null!; return false; }
                }
                public class FutureArtworkCatalog { public object Get(int id) => null!; }
            }
            public class Consumer {
                const byte ShipBank = 0xA2;
                public void Draw(SuperMetroid.Core.Assets.EnemySpritemapCatalog art, ushort dynamicId,
                    SuperMetroid.Core.Assets.FutureArtworkCatalog future,
                    SuperMetroid.Core.Assets.EnemyExtendedFrameCatalog extended) {
                    art.TryGetDisplay(pointer: (ushort)0xAD81, bank: ShipBank, parts: out _);
                    extended.TryGetDisplay(ShipBank, 0xAD82, out _);
                    art.TryGetDisplay(ShipBank, dynamicId, out _);
                    future.Get(5);
                }
            }
            """;
        SyntaxTree tree = CSharpSyntaxTree.ParseText(text, path: "fixture.cs");
        var compilation = CSharpCompilation.Create("AuditFixture", [tree],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        SemanticModel semantic = compilation.GetSemanticModel(tree);
        var exports = new ResourceIndex();
        var report = new AuditReport();
        foreach (var call in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            ConsumerAudit.Inspect(call, semantic, exports, report);
        Require(report.MissingCount == 2 && report.UnresolvedCount == 2,
            "public/internal constant lookups must be missing; dynamic ID and new catalog must be unresolved");
        Require(report.Findings.Single(item => item.Domain == ResourceDomains.EnemySimple && item.Code == AuditReport.Missing).Resource == "A2:AD81",
            "compiler-resolved named arguments must retain the actual bank/pointer identity");
        exports.Add(ResourceDomains.EnemySimple, "A2:AD81");
        exports.Add(ResourceDomains.EnemyExtended, "A2:AD82");
        var fixedReport = new AuditReport();
        foreach (var call in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            ConsumerAudit.Inspect(call, semantic, exports, fixedReport);
        Require(fixedReport.MissingCount == 0 && fixedReport.UnresolvedCount == 2,
            "adding an export must not conceal unresolved consumers");
        fixedReport.Sort();
        string first = JsonSerializer.Serialize(fixedReport, AuditReport.JsonOptions);
        fixedReport.Sort();
        Require(first == JsonSerializer.Serialize(fixedReport, AuditReport.JsonOptions), "report ordering must be deterministic");
        ConfirmProjectileRouting();
        ConfirmPaletteProviders();
        Console.WriteLine("Audit contracts: constants, named/cast arguments, dynamic/unknown domains, " +
            "direct/bound projectile routing, palette aliases/title provider and deterministic output confirmed.");
    }

    private static void ConfirmProjectileRouting()
    {
        // These synthetic IDs isolate the two provider contracts, not an enemy or frame sequence.
        const ushort operand = 0x9000, pointer = 0x9100;
        var exports = new ResourceIndex();
        var missing = new AuditReport();
        ProjectileDefinitionAudit.RequireFrame(operand, pointer, "fixture", "fixture.cs", exports, missing);
        Require(missing.Findings.Single().Domain == ResourceDomains.EnemyProjectileSprite,
            "an unbound frame must require its direct physical OAM, not an absent optional operand binding");
        exports.Add(ResourceDomains.EnemyProjectileSprite, ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, pointer));
        var direct = new AuditReport();
        ProjectileDefinitionAudit.RequireFrame(operand, pointer, "fixture", "fixture.cs", exports, direct);
        Require(direct.Findings.Count == 0, "installed direct OAM must satisfy the fallback route");
        var boundExports = new ResourceIndex();
        boundExports.Add(ResourceDomains.EnemyProjectileProgram, ResourceIndex.Address(ResourceBanks.EnemyProjectilePrograms, operand));
        var bound = new AuditReport();
        ProjectileDefinitionAudit.RequireFrame(operand, pointer, "fixture", "fixture.cs", boundExports, bound);
        Require(bound.Findings.Count == 0, "an operand-bound frame must not require unused direct OAM");
    }

    private static void ConfirmPaletteProviders()
    {
        // Confirmation of the two identified audit false positives: shared Spore
        // Spawn colors and title ambient colors have separate installed identities.
        var exports = new ResourceIndex();
        PaletteDefinitionAudit.InstallColorIdentities(exports);
        var report = new AuditReport();
        foreach (BrinstarBlueSporePaletteFxProgramDefinition definition in BrinstarBlueSporePaletteFxProgramMechanicsDefinitions.All)
            report.Require(ResourceDomains.PaletteFx, "alias fixture",
                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, definition.ColorPointer(0, 0)), "fixture.cs", exports);
        foreach (TitleScreenAmbientPaletteFxProgramDefinition definition in TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All)
            report.Require(ResourceDomains.PaletteFx, "title fixture",
                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, definition.FramePointer(0) + sizeof(ushort)), "fixture.cs", exports);
        Require(report.Findings.Count == 0, "shared aliases and the independent title provider must be recognized");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Resource audit contract failed: " + message);
    }
}
