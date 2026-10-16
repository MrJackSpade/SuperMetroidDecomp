using SuperMetroid.Tooling;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.Json;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.ResourceAudit.ClosedContracts;
using SuperMetroid.ResourceAudit.ClosedContracts.Bosses;
using SuperMetroid.ResourceAudit.ClosedContracts.Enemies;
using SuperMetroid.ResourceAudit.ClosedContracts.Frontend;
using SuperMetroid.ResourceAudit.ClosedContracts.Rooms;
using SuperMetroid.ResourceAudit.ClosedContracts.Samus;
using SuperMetroid.ResourceAudit.Reports;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.ResourceAudit.Resources;

/// <summary>Small synthetic confirmations of the audit itself; never searches gameplay.</summary>
internal static class AuditContractChecks
{
    /// <summary>Runs synthetic checks of resource identity resolution, provider routing, and deterministic audit output.</summary>
    public static void Run()
    {
        const string text = """
            namespace SuperMetroid.Core.Enemies.Artwork {
                public class EnemySpritemapCatalog {
                    public bool TryGetDisplay(byte bank, ushort pointer, out object parts) { parts = null!; return false; }
                }
                public class EnemyExtendedFrameCatalog {
                    internal bool TryGetDisplay(byte bank, ushort pointer, out object parts) { parts = null!; return false; }
                }
            }
            namespace SuperMetroid.Core.Assets {
                public class FutureArtworkCatalog { public object Get(int id) => null!; }
            }
            public class Consumer {
                const byte ShipBank = 0xA2;
                public void Draw(SuperMetroid.Core.Enemies.Artwork.EnemySpritemapCatalog art, ushort dynamicId,
                    SuperMetroid.Core.Assets.FutureArtworkCatalog future,
                    SuperMetroid.Core.Enemies.Artwork.EnemyExtendedFrameCatalog extended) {
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
            "public/internal constant lookups must be missing; dynamic ID and new catalog must be unresolved" + " (" + report.Describe() + ")");
        Require(report.Findings.Single(item => item.Domain == ResourceDomains.EnemySimple && item.Code == AuditReport.Missing).Resource == "A2:AD81",
            "compiler-resolved named arguments must retain the actual bank/pointer identity");
        exports.Add(ResourceDomains.EnemySimple, "A2:AD81");
        exports.Add(ResourceDomains.EnemyExtended, "A2:AD82");
        var fixedReport = new AuditReport();
        foreach (var call in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            ConsumerAudit.Inspect(call, semantic, exports, fixedReport);
        Require(fixedReport.MissingCount == 0 && fixedReport.UnresolvedCount == 2,
            "adding an export must not conceal unresolved consumers" + " (" + fixedReport.Describe() + ")");
        fixedReport.Sort();
        string first = JsonSerializer.Serialize(fixedReport, AuditReport.JsonOptions);
        fixedReport.Sort();
        Require(first == JsonSerializer.Serialize(fixedReport, AuditReport.JsonOptions), "report ordering must be deterministic");
        ConfirmProjectileRouting();
        ConfirmPaletteProviders();
        ConfirmEmptyFrameBoundary();
        ConfirmMotherBrainMetadata();
        ClosedPresentationContractChecks.Run();
        MessagePresentationContractChecks.Run();
        PlmPresentationContractChecks.Run();
        PlmDoorPresentationContractChecks.Run();
        SamusColorPresentationContractChecks.Run();
        EnemyColorPresentationContractChecks.Run();
        TextAndMapPresentationContractChecks.Run();
        PausePresentationContractChecks.Run();
        CinematicPresentationContractChecks.Run();
        ProjectilePresentationContractChecks.Run();
        SamusArtworkPresentationContractChecks.Run();
        BackgroundTransferPresentationContractChecks.Run();
        FinitePresentationNameFlowChecks.Run();
        RoomArtworkPresentationContractChecks.Run();
        AreaMapPresentationContractChecks.Run();
        RoomRevealPresentationContractChecks.Run();
        LibraryBackgroundPresentationContractChecks.Run();
        RoomLayoutPresentationContractChecks.Run();
        MotherBrainSheetsPresentationContractChecks.Run();
        XrayRoomOverlayPresentationContractChecks.Run();
        NativeDisplaySelectorContractChecks.Run();
        EnemyTileArtworkPresentationContractChecks.Run();
        EnemyProjectileArtworkContractChecks.Run();
        EnemyDisplayArtworkContractChecks.Run();
        SamusBodyTransferContractChecks.Run();
        InterfacePresentationContractChecks.Run();
        Console.WriteLine("Audit contracts: constants, named/cast arguments, dynamic/unknown domains, " +
            "direct/bound projectile routing, palette aliases/title provider, empty-frame boundary, " +
            "Mother Brain bank/color-row metadata, guarded closed providers and deterministic output confirmed.");
    }

    /// <summary>Separates direct projectile OAM resources from optional operand-bound frame resources.</summary>
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

    /// <summary>Checks shared Spore Spawn aliases and independent title ambient palette identities.</summary>
    private static void ConfirmPaletteProviders()
    {
        // Confirmation of the two identified audit false positives: shared Spore
        // Spawn colors and title ambient colors have separate installed identities.
        var exports = new ResourceIndex();
        PaletteDefinitionAudit.InstallColorIdentities(exports);
        var report = new AuditReport();
        foreach (BrinstarBlueSporePaletteFxColorDefinition definition in BrinstarBlueSporePaletteFxColorDefinitions.All)
            report.Require(ResourceDomains.PaletteFx, "alias fixture",
                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, definition.ColorPointer(0, 0)), "fixture.cs", exports);
        foreach (TitleScreenAmbientPaletteFxColorDefinition definition in TitleScreenAmbientPaletteFxColorDefinitions.All)
            report.Require(ResourceDomains.PaletteFx, "title fixture",
                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, definition.FramePointer(0) + sizeof(ushort)), "fixture.cs", exports);
        Require(report.Findings.Count == 0, "shared aliases and the independent title provider must be recognized");
    }

    /// <summary>Reports a synthetic audit contract failure with its specific unmet assertion.</summary>
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Resource audit contract failed: " + message);
    }

    /// <summary>Recognizes only reviewed A8/B3 no-op frames without inventing installed artwork entries.</summary>
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
        const string renderer = "class Fixture { void DrawEnemySpritemap(EnemySpritemapDraw draw) { " +
            "if (CommonEnemyEmptyExtendedFrameDefinitions.HasEmptySpritemap(draw.Bank, draw.Pointer)) return; } }";
        MethodDeclarationSyntax ParseMethod(string text) => CSharpSyntaxTree.ParseText(text)
            .GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        Require(CompiledEnemyDisplayAudit.HasNoOpBranch(ParseMethod(renderer)),
            "the precise reviewed no-op branch must be recognized");
        Require(!CompiledEnemyDisplayAudit.HasNoOpBranch(ParseMethod(renderer.Replace("return;", "DrawArt();", StringComparison.Ordinal))) &&
            !CompiledEnemyDisplayAudit.HasNoOpBranch(ParseMethod(renderer.Replace("draw.Bank, draw.Pointer", "draw.Bank, otherPointer", StringComparison.Ordinal))),
            "changed behavior or identity arguments must invalidate the reviewed classification");
    }

    /// <summary>Checks the Baby Metroid pulse poses and the installed Mother Brain flash-row boundary.</summary>
    private static void ConfirmMotherBrainMetadata()
    {
        // Confirms only the two identified metadata gaps. No AI, room, frame,
        // palette effect or gameplay callback is executed.
        // The typed Baby programs (#641) name their three pulse poses directly; each must be installed artwork.
        for (int pose = 0; pose < 3; pose++)
        {
            ushort spritemap = SuperMetroid.Core.Bosses.MotherBrain.Artwork.BabyMetroidVisualDefinitions.Spritemap(pose);
            Require(SuperMetroid.Core.Enemies.Artwork.EnemySpritemapDefinitions.Frames.Any(frame =>
                frame.Bank == SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainVisualDefinitions.Bank && frame.Pointer == spritemap),
                "each Baby Metroid pulse pose must be installed, not just named");
        }

        var exports = new ResourceIndex();
        var missing = new AuditReport();
        MotherBrainRoomFlashAudit.Inspect("fixture.cs", exports, missing);
        Require(missing.MissingCount == MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount &&
            missing.UnresolvedCount == 0, "absent flash rows must be concrete omissions, not an ignored program kind" + " (" + missing.Describe() + ")");
        MotherBrainRoomFlashAudit.Install(exports);
        var present = new AuditReport();
        MotherBrainRoomFlashAudit.Inspect("fixture.cs", exports, present);
        Require(present.Findings.Count == 0 && present.ReferenceCount == missing.ReferenceCount,
            "the actual importer/loader must install every declared timed color row");
        ushort neighboringOperand = checked((ushort)(MotherBrainRoomPaletteProgramDefinitions
            .PresentationWordAddress(MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount - 1) + 1));
        var changed = new AuditReport();
        MotherBrainRoomFlashAudit.RequireOperands([neighboringOperand], "fixture.cs", exports, changed);
        Require(changed.MissingCount == 1, "a changed/unaligned color identity must remain a concrete omission" + " (" + changed.Describe() + ")");

        var document = RequiredJson.Deserialize<SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainRoomColorDocument>(
            MotherBrainRoomFlashAudit.ExtractDeclarationDocument(), ArtworkJson.Options);
        bool rejected = false;
        try
        {
            _ = SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainRoomColorPresentation.Load(new MemoryStream(
                JsonSerializer.SerializeToUtf8Bytes(document with { Flash = document.Flash[..^1] },
                    ArtworkJson.Options)));
        }
        catch (InvalidDataException) { rejected = true; }
        Require(rejected, "a missing installed flash row must fail at the production load boundary");
    }
}
