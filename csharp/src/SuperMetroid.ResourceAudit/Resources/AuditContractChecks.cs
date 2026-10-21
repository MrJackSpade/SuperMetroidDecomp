using SuperMetroid.Core.Enemies.Artwork;
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
using SuperMetroid.AssetExtraction.Fx;

namespace SuperMetroid.ResourceAudit.Resources;

/// <summary>Small synthetic confirmations of the audit itself; never searches gameplay.</summary>
internal static class AuditContractChecks
{
    /// <summary>
    /// Runs every synthetic check of resource identity resolution, provider routing and deterministic audit output, then
    /// fails with all of their failures together, so one run reports every broken contract.
    /// </summary>
    /// <exception cref="AggregateException">One or more contract checks failed; each inner exception names its check.</exception>
    public static void Run()
    {
        var failures = new List<Exception>();
        foreach (AuditContractCheck check in Checks)
        {
            try
            {
                check.Run();
            }
            catch (Exception failure)
            {
                failures.Add(new InvalidOperationException($"Audit contract check {check.Name} failed.", failure));
            }
        }
        if (failures.Count > 0)
            throw new AggregateException($"{failures.Count} of {Checks.Length} audit contract checks failed.", failures);
        Console.WriteLine("Audit contracts: constants, named/cast arguments, dynamic/unknown domains, " +
            "direct/bound projectile routing, palette aliases/title provider, empty-frame boundary, " +
            "Mother Brain bank/color-row metadata, guarded closed providers and deterministic output confirmed.");
    }

    /// <summary>The contract checks, in the order they run.</summary>
    private static readonly AuditContractCheck[] Checks =
    [
        new(nameof(ConfirmConsumerClassification), ConfirmConsumerClassification),
        new(nameof(ConfirmProjectileRouting), ConfirmProjectileRouting),
        new(nameof(ConfirmPaletteProviders), ConfirmPaletteProviders),
        new(nameof(ConfirmEmptyFrameBoundary), ConfirmEmptyFrameBoundary),
        new(nameof(ConfirmMotherBrainMetadata), ConfirmMotherBrainMetadata),
        new(nameof(ClosedPresentationContractChecks), ClosedPresentationContractChecks.Run),
        new(nameof(MessagePresentationContractChecks), MessagePresentationContractChecks.Run),
        new(nameof(PlmPresentationContractChecks), PlmPresentationContractChecks.Run),
        new(nameof(PlmDoorPresentationContractChecks), PlmDoorPresentationContractChecks.Run),
        new(nameof(SamusColorPresentationContractChecks), SamusColorPresentationContractChecks.Run),
        new(nameof(EnemyColorPresentationContractChecks), EnemyColorPresentationContractChecks.Run),
        new(nameof(TextAndMapPresentationContractChecks), TextAndMapPresentationContractChecks.Run),
        new(nameof(PausePresentationContractChecks), PausePresentationContractChecks.Run),
        new(nameof(CinematicPresentationContractChecks), CinematicPresentationContractChecks.Run),
        new(nameof(ProjectilePresentationContractChecks), ProjectilePresentationContractChecks.Run),
        new(nameof(SamusArtworkPresentationContractChecks), SamusArtworkPresentationContractChecks.Run),
        new(nameof(BackgroundTransferPresentationContractChecks), () => BackgroundTransferPresentationContractChecks.Run()),
        new(nameof(RoomArtworkPresentationContractChecks), RoomArtworkPresentationContractChecks.Run),
        new(nameof(AreaMapPresentationContractChecks), AreaMapPresentationContractChecks.Run),
        new(nameof(RoomRevealPresentationContractChecks), RoomRevealPresentationContractChecks.Run),
        new(nameof(LibraryBackgroundPresentationContractChecks), LibraryBackgroundPresentationContractChecks.Run),
        new(nameof(RoomLayoutPresentationContractChecks), RoomLayoutPresentationContractChecks.Run),
        new(nameof(MotherBrainSheetsPresentationContractChecks), MotherBrainSheetsPresentationContractChecks.Run),
        new(nameof(XrayRoomOverlayPresentationContractChecks), XrayRoomOverlayPresentationContractChecks.Run),
        new(nameof(NativeDisplaySelectorContractChecks), NativeDisplaySelectorContractChecks.Run),
        new(nameof(EnemyTileArtworkPresentationContractChecks), EnemyTileArtworkPresentationContractChecks.Run),
        new(nameof(EnemyProjectileArtworkContractChecks), EnemyProjectileArtworkContractChecks.Run),
        new(nameof(EnemyDisplayArtworkContractChecks), EnemyDisplayArtworkContractChecks.Run),
        new(nameof(SamusBodyTransferContractChecks), SamusBodyTransferContractChecks.Run),
        new(nameof(InterfacePresentationContractChecks), InterfacePresentationContractChecks.Run),
    ];

    /// <summary>Classifies constant, named, dynamic and unknown-catalog lookups and keeps report output deterministic.</summary>
    private static void ConfirmConsumerClassification()
    {
        const string text = """
            namespace SuperMetroid.Core.Enemies.Artwork {
                public enum EnemyFrameId { Ship, ShipExtended }
                public class EnemySpritemapCatalog {
                    public bool TryGetDisplay(EnemyFrameId frame, out object parts) { parts = null!; return false; }
                }
                public class EnemyExtendedFrameCatalog {
                    internal bool TryGetDisplay(EnemyFrameId frame, out object parts) { parts = null!; return false; }
                }
            }
            namespace SuperMetroid.Core.Assets {
                public class FutureArtworkCatalog { public object Get(int id) => null!; }
            }
            public class Consumer {
                const SuperMetroid.Core.Enemies.Artwork.EnemyFrameId ShipFrame = SuperMetroid.Core.Enemies.Artwork.EnemyFrameId.Ship;
                public void Draw(SuperMetroid.Core.Enemies.Artwork.EnemySpritemapCatalog art, SuperMetroid.Core.Enemies.Artwork.EnemyFrameId dynamicFrame,
                    SuperMetroid.Core.Assets.FutureArtworkCatalog future,
                    SuperMetroid.Core.Enemies.Artwork.EnemyExtendedFrameCatalog extended) {
                    art.TryGetDisplay(parts: out _, frame: SuperMetroid.Core.Enemies.Artwork.EnemyFrameId.Ship);
                    extended.TryGetDisplay(SuperMetroid.Core.Enemies.Artwork.EnemyFrameId.ShipExtended, out _);
                    art.TryGetDisplay(dynamicFrame, out _);
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
        Require(report.Findings.Single(item => item.Domain == ResourceDomains.EnemySimple && item.Code == AuditReport.Missing).Resource == "Ship",
            "compiler-resolved named arguments must retain the actual frame identity");
        exports.Add(ResourceDomains.EnemySimple, "Ship");
        exports.Add(ResourceDomains.EnemyExtended, "ShipExtended");
        var fixedReport = new AuditReport();
        foreach (var call in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            ConsumerAudit.Inspect(call, semantic, exports, fixedReport);
        Require(fixedReport.MissingCount == 0 && fixedReport.UnresolvedCount == 2,
            "adding an export must not conceal unresolved consumers" + " (" + fixedReport.Describe() + ")");
        fixedReport.Sort();
        string first = JsonSerializer.Serialize(fixedReport, AuditReport.JsonOptions);
        fixedReport.Sort();
        Require(first == JsonSerializer.Serialize(fixedReport, AuditReport.JsonOptions), "report ordering must be deterministic");
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
                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, PaletteFxRecordSources.ColorAddress(definition.Color(0, 0))), "fixture.cs", exports);
        foreach (TitleScreenAmbientPaletteFxColorDefinition definition in TitleScreenAmbientPaletteFxColorDefinitions.All)
            report.Require(ResourceDomains.PaletteFx, "title fixture",
                ResourceIndex.Address(ResourceBanks.ProjectileOamAndPaletteFx, PaletteFxRecordSources.ColorAddress(definition.Color(0, 0))), "fixture.cs", exports);
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
        ushort empty = CommonEnemyEmptyExtendedFrameToolingDefinitions.EmptySpritemap;
        Require(CommonEnemyEmptyExtendedFrameToolingDefinitions.HasEmptySpritemap(0xa8, empty) &&
            CommonEnemyEmptyExtendedFrameToolingDefinitions.HasEmptySpritemap(0xb3, empty),
            "the two reported identities must be compiled no-op frames");
        Require(!CommonEnemyEmptyExtendedFrameToolingDefinitions.HasEmptySpritemap(0xb5, empty) &&
            !CommonEnemyEmptyExtendedFrameToolingDefinitions.HasEmptySpritemap(0xa8, unchecked((ushort)(empty + 1))),
            "unsupported banks and neighboring pointers must not be classified as empty artwork");
        var exports = new ResourceIndex();
        exports.Add(ResourceDomains.EnemyDisplay, ResourceIndex.Address(0xa8, empty));
        Require(!exports.Contains(ResourceDomains.EnemySimple, ResourceIndex.Address(0xa8, empty)),
            "a compiled renderer no-op must not imply an installed catalog entry");
        const string renderer = "class Fixture { void PublishEnemySpritemap(EnemySpritemapDraw draw) { " +
            "if (draw.Frame == EnemyFrameId.Empty) return; } }";
        MethodDeclarationSyntax ParseMethod(string text) => CSharpSyntaxTree.ParseText(text)
            .GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
        Require(CompiledEnemyDisplayAudit.HasNoOpBranch(ParseMethod(renderer)),
            "the precise reviewed no-op branch must be recognized");
        Require(!CompiledEnemyDisplayAudit.HasNoOpBranch(ParseMethod(renderer.Replace("return;", "DrawArt();", StringComparison.Ordinal))) &&
            !CompiledEnemyDisplayAudit.HasNoOpBranch(ParseMethod(renderer.Replace("EnemyFrameId.Empty", "EnemyFrameId.EmptyExtended", StringComparison.Ordinal))),
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
            ushort spritemap = SuperMetroid.Core.Bosses.MotherBrain.Artwork.BabyMetroidVisualToolingDefinitions.Spritemap(pose);
            Require(SuperMetroid.Core.World.Artwork.EnemySpritemapDefinitions.Frames.Any(frame =>
                frame.Bank == SuperMetroid.Core.Bosses.MotherBrain.Artwork.MotherBrainVisualToolingDefinitions.Bank && frame.Pointer == spritemap),
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
        ushort neighboringOperand = checked((ushort)(SuperMetroid.AssetExtraction.Bosses.MotherBrain.MotherBrainRoomColorExtractor
            .FlashPaletteOperandAddress(MotherBrainRoomPaletteProgramDefinitions.PresentationWordCount - 1) + 1));
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
