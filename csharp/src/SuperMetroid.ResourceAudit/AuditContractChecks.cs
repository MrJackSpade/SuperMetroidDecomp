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
        ConfirmEmptyFrameBoundary();
        ConfirmMotherBrainMetadata();
        Console.WriteLine("Audit contracts: constants, named/cast arguments, dynamic/unknown domains, " +
            "direct/bound projectile routing, palette aliases/title provider, empty-frame boundary, " +
            "Mother Brain bank/color-row metadata and deterministic output confirmed.");
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

    private static void ConfirmEmptyFrameBoundary()
    {
        // Confirmation of the identified A8/B3 blank-frame false positives.
        ushort empty = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
        Require(CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap(0xa8, empty) &&
            CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap(0xb3, empty),
            "the two reported identities must be compiled no-op frames");
        Require(!CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap(0xb5, empty) &&
            !CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap(0xa8, unchecked((ushort)(empty + 1))),
            "unsupported banks and neighboring pointers must not be classified as empty artwork");
        var exports = new ResourceIndex();
        exports.Add(ResourceDomains.EnemyDisplay, ResourceIndex.Address(0xa8, empty));
        Require(!exports.Contains(ResourceDomains.EnemySimple, ResourceIndex.Address(0xa8, empty)),
            "a compiled renderer no-op must not imply an installed catalog entry");
        const string renderer = "class Fixture { void DrawEnemySpritemap(byte bank, ushort pointer) { " +
            "if (CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap(bank, pointer)) return; } }";
        MethodDeclarationSyntax ParseMethod(string text) => CSharpSyntaxTree.ParseText(text)
            .GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        Require(CompiledEnemyDisplayAudit.HasNoOpBranch(ParseMethod(renderer)),
            "the precise reviewed no-op branch must be recognized");
        Require(!CompiledEnemyDisplayAudit.HasNoOpBranch(ParseMethod(renderer.Replace("return;", "DrawArt();", StringComparison.Ordinal))) &&
            !CompiledEnemyDisplayAudit.HasNoOpBranch(ParseMethod(renderer.Replace("bank, pointer", "bank, otherPointer", StringComparison.Ordinal))),
            "changed behavior or identity arguments must invalidate the reviewed classification");
    }

    private static void ConfirmMotherBrainMetadata()
    {
        // Confirms only the two identified metadata gaps. No AI, room, frame,
        // palette effect or gameplay callback is executed.
        Require(MotherBrainBabyInstructionProgramDefinitions.Bank ==
            MotherBrainRoomColorRomData.SourceBank >> 16, "cutscene-baby bank ownership must be explicit");
        // Pinned InstList_BabyMetroid initial, drain and fatal-blow sequences.
        ushort[] expected = [0xf9a8, 0xfa40, 0xfad8, 0xfa40,
            0xf9a8, 0xfa40, 0xfad8, 0xfa40, 0xfad8];
        Require(expected.Length == MotherBrainBabyInstructionProgramDefinitions.PresentationWordCount,
            "the reviewed cutscene-baby sequences must not silently grow");
        for (int index = 0; index < expected.Length; index++)
        {
            Require(SuperMetroid.Core.Assets.CompiledEnemyVisualSelectors.TryGet(
                MotherBrainBabyInstructionProgramDefinitions.Bank,
                MotherBrainBabyInstructionProgramDefinitions.PresentationWordAddress(index), out ushort selected)
                && selected == expected[index], "every cutscene-baby operand must select its pinned native frame");
            Require(SuperMetroid.Core.Assets.EnemySpritemapDefinitions.Frames.ToArray().Any(frame =>
                frame.Bank == MotherBrainBabyInstructionProgramDefinitions.Bank && frame.Pointer == selected),
                "each selected cutscene-baby frame must be installed, not just bound");
        }
        Require(!SuperMetroid.Core.Assets.CompiledEnemyVisualSelectors.TryGet(
            MotherBrainBabyInstructionProgramDefinitions.Bank, MotherBrainBabyInstructionProgramDefinitions.Initial, out _),
            "an adjacent mechanics word must not become a visual selector");

        var exports = new ResourceIndex();
        var missing = new AuditReport();
        MotherBrainRoomFlashAudit.Inspect("fixture.cs", exports, missing);
        Require(missing.MissingCount == MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount &&
            missing.UnresolvedCount == 0, "absent flash rows must be concrete omissions, not an ignored program kind");
        MotherBrainRoomFlashAudit.Install(exports);
        var present = new AuditReport();
        MotherBrainRoomFlashAudit.Inspect("fixture.cs", exports, present);
        Require(present.Findings.Count == 0 && present.ReferenceCount == missing.ReferenceCount,
            "the actual importer/loader must install every declared timed color row");
        ushort neighboringOperand = checked((ushort)(MotherBrainRoomPaletteProgramDefinitions
            .PresentationWordAddress(MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount - 1) + 1));
        var changed = new AuditReport();
        MotherBrainRoomFlashAudit.RequireOperands([neighboringOperand], "fixture.cs", exports, changed);
        Require(changed.MissingCount == 1, "a changed/unaligned color identity must remain a concrete omission");

        var document = JsonSerializer.Deserialize<SuperMetroid.Core.Assets.MotherBrainRoomColorDocument>(
            MotherBrainRoomFlashAudit.ExtractDeclarationDocument(), SuperMetroid.Core.Assets.MapPresentationFormat.JsonOptions)!;
        bool rejected = false;
        try
        {
            _ = SuperMetroid.Core.Assets.MotherBrainRoomColorPresentation.Load(new MemoryStream(
                JsonSerializer.SerializeToUtf8Bytes(document with { Flash = document.Flash[..^1] },
                    SuperMetroid.Core.Assets.MapPresentationFormat.JsonOptions)));
        }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "a missing installed flash row must fail at the production load boundary");
    }
}
