using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.ResourceAudit;

/// <summary>One queued transfer that is missing ownership or could not be classified.</summary>
/// <param name="Code">Stable category for the transfer finding.</param>
/// <param name="Owner">Producer method or source operation responsible for the transfer.</param>
/// <param name="Source">Source file and line where the queue call occurs.</param>
/// <param name="Detail">Explanation of the invalid transfer or incomplete producer boundary.</param>
internal sealed record VramDmaFinding(string Code, string Owner, string Source, string Detail);
/// <summary>Queue producer site retained in the report inventory.</summary>
internal sealed record VramDmaSite();
/// <summary>Native-address or typed-asset transfer descriptor emitted by a producer.</summary>
/// <param name="Owner">Method whose body owns the descriptor.</param>
/// <param name="SourceAddress">Native bus address when the descriptor is not typed.</param>
/// <param name="ByteCount">Transfer byte count.</param>
/// <param name="Asset">Typed VRAM asset identifier, or <see cref="VramAssetId.None"/> for a native source.</param>
internal sealed record VramDmaTransfer(string Owner, int SourceAddress, int ByteCount,
    VramAssetId Asset = VramAssetId.None);
/// <summary>Producer sites, transfer descriptors and ownership findings from one DMA analysis.</summary>
internal sealed class VramDmaReport
{
    /// <summary>Source locations that enqueue VRAM transfers.</summary>
    public List<VramDmaSite> Sites { get; } = [];
    /// <summary>Resolved native and typed transfer descriptors.</summary>
    public List<VramDmaTransfer> Transfers { get; } = [];
    /// <summary>Unresolved producers and transfers without an admitted owner.</summary>
    public List<VramDmaFinding> Findings { get; } = [];
}

/// <summary>
/// Inventories the real queue API semantically, including presentation helpers.
/// Constants are compiler-evaluated; state-selected families require reviewed
/// method bodies and correlated immutable descriptor adapters. Never steps a game.
/// </summary>
internal static class VramDmaAudit
{
    /// <summary>Runs the queued-transfer source audit and optionally writes its JSON report.</summary>
    internal static int Run(string root, string? jsonPath)
    {
        VramDmaReport report = Analyze(CreateCompilation(root), root);
        Console.WriteLine($"Static queued VRAM DMA audit: {report.Sites.Count} producer sites; " +
            $"{report.Transfers.Count} source/count descriptors; {report.Findings.Count} findings.");
        Console.WriteLine("No ROM, saves, inputs, gameplay or replay execution was used.");
        foreach (var finding in report.Findings.Take(40))
            Console.Error.WriteLine($"{finding.Code} {finding.Source}: {finding.Owner}: {finding.Detail}");
        if (jsonPath is not null)
        {
            string absolute = Path.GetFullPath(jsonPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);
            File.WriteAllText(absolute, JsonSerializer.Serialize(report, AuditReport.JsonOptions) + Environment.NewLine);
            Console.WriteLine("Report: " + absolute);
        }
        return report.Findings.Count == 0 ? 0 : 1;
    }

    /// <summary>Compiles repository Core source with SDK platform references for semantic DMA inspection.</summary>
    internal static CSharpCompilation CreateCompilation(string root)
    {
        var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        var trees = Directory.EnumerateFiles(Path.Combine(root, "csharp/src/SuperMetroid.Core"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"))
            .Order(StringComparer.Ordinal)
            .Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), options,
                Path.GetRelativePath(root, path).Replace('\\', '/'))).ToList();
        trees.Add(CSharpSyntaxTree.ParseText("global using System; global using System.Collections.Generic; " +
            "global using System.IO; global using System.Linq; global using System.Threading; global using System.Threading.Tasks;",
            options, "<sdk-global-usings>"));
        string[] platforms = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")
            ?? throw new InvalidOperationException("SDK platform references are unavailable.")).Split(Path.PathSeparator);
        return CSharpCompilation.Create("QueuedDmaSourceAudit", trees,
            platforms.Select(path => MetadataReference.CreateFromFile(path)),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
    }

    /// <summary>Finds queued transfer sites, resolves finite descriptors and checks installed ownership.</summary>
    internal static VramDmaReport Analyze(CSharpCompilation compilation, string? root = null,
        Func<int, int, bool>? ownsNative = null, Func<VramAssetId, int, bool>? ownsTyped = null)
    {
        var report = new VramDmaReport();
        if (root is not null) VramDmaSourceContracts.Verify(root, report);
        ownsNative ??= VramDmaOwnership.OwnsNative;
        ownsTyped ??= VramDmaOwnership.OwnsTyped;
        var adapted = new HashSet<string>(StringComparer.Ordinal);
        foreach (SyntaxTree tree in compilation.SyntaxTrees)
        {
            SemanticModel semantic = compilation.GetSemanticModel(tree);
            foreach (var syntax in tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (semantic.GetOperation(syntax) is not IInvocationOperation call ||
                    call.TargetMethod.ContainingType.ToDisplayString() != typeof(VramWriteQueue).FullName ||
                    call.TargetMethod.Name is not (nameof(VramWriteQueue.Enqueue) or nameof(VramWriteQueue.EnqueueAsset)))
                {
                    // A bound queue receiver with a failed overload is not an invisible site.
                    if (semantic.GetSymbolInfo(syntax).Symbol is null &&
                        syntax.Expression is MemberAccessExpressionSyntax member &&
                        semantic.GetTypeInfo(member.Expression).Type?.ToDisplayString() == typeof(VramWriteQueue).FullName)
                        report.Findings.Add(new("unresolved-api", member.ToString(), Location(syntax), "Queue invocation did not bind."));
                    continue;
                }
                var methodSyntax = syntax.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
                var method = methodSyntax is null ? null : semantic.GetDeclaredSymbol(methodSyntax) as IMethodSymbol;
                string owner = method is null ? "<non-method queue producer>" : method.ContainingType.ToDisplayString() + "." + method.Name;
                string hash = methodSyntax is null ? "" : Hash(methodSyntax);
                report.Sites.Add(new());
                bool typed = call.TargetMethod.Name == nameof(VramWriteQueue.EnqueueAsset);
                int? count = Constant(call, "sizeInBytes");
                int? source = Constant(call, typed ? "asset" : "sourceAddress");
                if (source.HasValue && count.HasValue)
                {
                    if (typed && source.Value == (int)VramAssetId.None)
                    {
                        report.Findings.Add(new("invalid-typed-asset", owner, Location(syntax), "EnqueueAsset cannot select None."));
                        continue;
                    }
                    Check(new(owner, typed ? 0 : source.Value, count.Value,
                        typed ? (VramAssetId)source.Value : VramAssetId.None), Location(syntax));
                    continue;
                }
                if (!VramDmaProducerContracts.TryGet(owner, hash, out string? family))
                {
                    report.Findings.Add(new("unresolved-producer", owner, Location(syntax),
                        "Nonconstant source/count needs a reviewed finite-domain adapter. Method SHA256=" + hash));
                    continue;
                }
                if (!adapted.Add(owner)) continue;
                foreach (VramDmaTransfer transfer in VramDmaDomains.ForFamily(family!, owner).Distinct())
                    Check(transfer, Location(syntax));
                if (family == "plm" && root is not null && PlmProgramAudit.Run(root, null) != 0)
                    report.Findings.Add(new("unresolved-plm-program", owner, Location(syntax), "The PLM descriptor/ownership audit failed."));
            }
            // A future delegate can invoke the queue without naming its API at
            // the actual producer. Fail on the method-group escape rather than
            // quietly claiming that the direct-invocation inventory is complete.
            foreach (var name in tree.GetRoot().DescendantNodes().OfType<IdentifierNameSyntax>()
                         .Where(name => name.Identifier.ValueText is nameof(VramWriteQueue.Enqueue) or nameof(VramWriteQueue.EnqueueAsset)))
            {
                if (semantic.GetSymbolInfo(name).Symbol is not IMethodSymbol target ||
                    target.ContainingType.ToDisplayString() != typeof(VramWriteQueue).FullName) continue;
                SyntaxNode expression = name.Parent is MemberAccessExpressionSyntax member ? member : name;
                if (expression.Parent is InvocationExpressionSyntax invocation && invocation.Expression == expression) continue;
                if (name.Ancestors().OfType<InvocationExpressionSyntax>().Any(invocation =>
                    invocation.Expression is IdentifierNameSyntax id && id.Identifier.ValueText == "nameof")) continue;
                report.Findings.Add(new("unresolved-api-reference", target.Name, Location(name),
                    "Queue method reference escapes direct invocation; its delegate/dataflow needs an adapter."));
            }
        }
        report.Findings.Sort((a, b) => StringComparer.Ordinal.Compare(a.Source + a.Detail, b.Source + b.Detail));
        return report;

        void Check(VramDmaTransfer transfer, string location)
        {
            report.Transfers.Add(transfer);
            string? failure = CheckTransfer(transfer, ownsNative, ownsTyped);
            if (failure is not null)
                report.Findings.Add(new("missing-transfer", transfer.Owner, location, failure));
        }
    }

    /// <summary>Checks transfer size and verifies that its typed or native source has an installed owner.</summary>
    internal static string? CheckTransfer(VramDmaTransfer transfer, Func<int, int, bool> ownsNative,
        Func<VramAssetId, int, bool> ownsTyped)
    {
        if (transfer.ByteCount is <= 0 or > ushort.MaxValue) return "Invalid queue byte count: " + transfer.ByteCount;
        if (transfer.Asset != VramAssetId.None)
            return ownsTyped(transfer.Asset, transfer.ByteCount) ? null :
                $"Typed asset {transfer.Asset}+${transfer.ByteCount:X} has no admitted runtime resolver of that length.";
        if ((uint)transfer.SourceAddress > VramDmaDomainGeometry.MaximumBusAddress)
            return "Native DMA source is not a 24-bit address: " + transfer.SourceAddress;
        if (ownsNative(transfer.SourceAddress, transfer.ByteCount)) return null;
        for (int offset = 0; offset < transfer.ByteCount; offset++)
        {
            SnesAddress source = SnesAddress.FromBusAddress(transfer.SourceAddress).AddWithinBank(offset);
            SnesDmaSourceKind kind = SnesDmaSourceMap.Classify(source);
            if (kind is not (SnesDmaSourceKind.WorkRam or SnesDmaSourceKind.SaveRam))
                return $"Native source ${transfer.SourceAddress:X6}+${transfer.ByteCount:X} has no installed resolver; " +
                    $"byte {source} maps to {kind}, not mutable RAM.";
        }
        return null;
    }

    /// <summary>Reads a named invocation argument when its value is a compile-time integer constant.</summary>
    private static int? Constant(IInvocationOperation call, string name)
    {
        var value = call.Arguments.Single(argument => argument.Parameter?.Name == name).Value.ConstantValue;
        return value is { HasValue: true, Value: not null } ? Convert.ToInt32(value.Value) : null;
    }
    /// <summary>Computes the stable source-token fingerprint used by reviewed producer contracts.</summary>
    internal static string Hash(SyntaxNode node) => SourceFingerprint.Of(node);
    /// <summary>Formats a syntax node's repository path and one-based line number.</summary>
    private static string Location(SyntaxNode node) => node.SyntaxTree.FilePath + ":" +
        (node.GetLocation().GetLineSpan().StartLinePosition.Line + 1);
}
