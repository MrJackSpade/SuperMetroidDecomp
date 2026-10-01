using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit;

/// <summary>Confirm the requested audit's failure contracts, not look for gameplay defects.</summary>
internal static class VramDmaContractChecks
{
    internal static void Run()
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText("""
            using SuperMetroid.Core.Hardware;
            class Producer {
                const int Artwork = 0xADB200;
                void Write(VramWriteQueue renamedQueue, int unknownSource) {
                    renamedQueue.Enqueue(sourceAddress: Artwork, sizeInBytes: (ushort)1024, encodedVramDestination: 0);
                    renamedQueue.Enqueue(1024, 0x7E2000, 0);
                    renamedQueue.Enqueue(64, 0x801FF0, 0);
                    renamedQueue.EnqueueAsset(VramAssetId.StandardHudTiles, 8192, 0);
                    renamedQueue.EnqueueAsset(VramAssetId.StandardHudTiles, 8191, 0);
                    renamedQueue.Enqueue(32, unknownSource, 0);
                    renamedQueue.EnqueueAsset(VramAssetId.None, 32, 0);
                    System.Action<ushort, int, ushort> indirect = renamedQueue.Enqueue;
                }
            }
            """, path: "dma-contract-fixture.cs");
        string[] platforms = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);
        var compilation = CSharpCompilation.Create("DmaContractFixture", [tree],
            platforms.Append(typeof(VramWriteQueue).Assembly.Location).Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        Require(!compilation.GetDiagnostics().Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error),
            "synthetic producer fixture compiles");
        VramDmaReport missing = VramDmaAudit.Analyze(compilation, ownsNative: (_, _) => false);
        Require(missing.Sites.Count == 7, "named/cast constants and renamed queue receiver are inventoried");
        Require(missing.Findings.Count(finding => finding.Code == "missing-transfer") == 3,
            "missing artwork, wrong typed length and mirror/unmapped boundary crossing fail");
        Require(missing.Findings.Count(finding => finding.Code == "unresolved-producer") == 1,
            "dynamic unknown producer is an explicit failure");
        Require(missing.Findings.Count(finding => finding.Code == "invalid-typed-asset") == 1,
            "typed None is not mislabeled as RAM");
        Require(missing.Findings.Count(finding => finding.Code == "unresolved-api-reference") == 1,
            "delegate producer cannot escape the source inventory");
        VramDmaReport owned = VramDmaAudit.Analyze(compilation,
            ownsNative: (source, count) => source == 0xadb200 && count == 1024);
        Require(owned.Findings.Count == missing.Findings.Count - 1, "installing the exact binding resolves only that descriptor");
        Require(VramDmaAudit.CheckTransfer(new("bank-wrap", 0x7efff0, 64), (_, _) => false, (_, _) => false) is null,
            "WRAM DMA wraps its offset, not its bank");
        Console.WriteLine("Queued DMA audit contracts: semantic inventory, missing binding, exact count, RAM boundary/wrap, typed None, unknown producer and delegate escape passed.");
    }

    internal static void Require(bool result, string property)
    {
        if (!result) throw new InvalidOperationException("Queued DMA audit contract failed: " + property);
    }
}
