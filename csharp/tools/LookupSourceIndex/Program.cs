using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Static review navigation for #1165. This does not execute gameplay or infer a
/// disposition. Run from the repository root with the output JSONL path as its argument.
/// Candidates include false positives; a member can contain multiple logical tables.
/// </summary>
internal static partial class Program
{
    private static int Main(string[] args)
    {
        if (OperatingSystem.IsWindows())
            NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);
        try
        {
            if (args.Length != 1)
                throw new ArgumentException("Expected one output JSONL path; run from the repository root.");
            var start = new ProcessStartInfo("git") { RedirectStandardOutput = true, UseShellExecute = false };
            start.ArgumentList.Add("ls-files");
            start.ArgumentList.Add("-z");
            using var git = Process.Start(start) ?? throw new InvalidOperationException("Cannot start git.");
            string tracked = git.StandardOutput.ReadToEnd();
            git.WaitForExit();
            if (git.ExitCode != 0)
                throw new InvalidOperationException($"git ls-files failed: {git.ExitCode}.");

            using var review = JsonDocument.Parse(File.ReadAllText(
                "csharp/test-fixtures/lookup-algorithm-review-1165.json"));
            var links = new Dictionary<string, List<ReviewLink>>(StringComparer.Ordinal);
            var unlinked = new List<ReviewLink>();
            CollectReviewLinks(review.RootElement, null);
            var trackedPaths = tracked.Split('\0', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.Ordinal);

            using var output = new StreamWriter(args[0], append: false);
            Write(new
            {
                kind = "scope", issue = 1165,
                method = "C# syntax only; no gameplay or test execution",
                limitations = "Candidate members are not logical-table dispositions. Split fields, reconcile aliases, inspect consumers and independently verify evidence. No-hit files still need manual review for externally loaded or computed mappings. Non-C# sources require manual review. Expected-output fixtures are not automatically excluded.",
                reviewInventory = "csharp/test-fixtures/lookup-algorithm-review-1165.json",
                reviewLinkMeaning = "Owner-level navigation only. Listed logical-table states come from the review inventory and do not establish whole-file coverage or validate changes since review.",
                reviewsWithoutOwner = unlinked.OrderBy(item => item.Id, StringComparer.Ordinal),
                untrackedReviewOwners = links.Keys.Where(path => !trackedPaths.Contains(path)).Order(StringComparer.Ordinal),
            });
            int files = 0, candidates = 0;
            foreach (string path in tracked.Split('\0', StringSplitOptions.RemoveEmptyEntries).Order(StringComparer.Ordinal))
            {
                string extension = Path.GetExtension(path).ToLowerInvariant();
                // The generated inventory is an index, not an implementation mapping.
                if (path is "csharp/test-fixtures/lookup-source-index-1165.jsonl" or
                    "csharp/test-fixtures/lookup-algorithm-review-1165.json")
                    continue;
                files++;
                if (extension != ".cs")
                {
                    Write(new { kind = "manual-path", path, state = "unreviewed", reviewedLogicalTables = ReviewsFor(path), note = "Classify source, data, bundled support or non-target artifact before exclusion." });
                    continue;
                }
                string source = File.ReadAllText(path);
                string hash = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(source)));
                var root = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)).GetRoot();
                var members = new List<object>();
                foreach (MemberDeclarationSyntax member in root.DescendantNodes().OfType<MemberDeclarationSyntax>())
                {
                    if (member is not (BaseFieldDeclarationSyntax or PropertyDeclarationSyntax or MethodDeclarationSyntax or ConstructorDeclarationSyntax or EnumDeclarationSyntax or IndexerDeclarationSyntax))
                        continue;
                    var reasons = new SortedSet<string>(StringComparer.Ordinal);
                    foreach (SyntaxNode node in member.DescendantNodesAndSelf())
                    {
                        if (node is ArrayTypeSyntax) reasons.Add("array-type");
                        if (node is CollectionExpressionSyntax or InitializerExpressionSyntax) reasons.Add("initializer");
                        if (node is SwitchExpressionSyntax or SwitchStatementSyntax) reasons.Add("switch-mapping");
                        if (node is EnumDeclarationSyntax) reasons.Add("enum-mapping");
                        if (node is GenericNameSyntax name && name.Identifier.ValueText is "Span" or "ReadOnlySpan" or "Dictionary" or "FrozenDictionary" or "ImmutableArray") reasons.Add("indexed-container");
                        if (node is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression) && literal.Token.ValueText.Length >= 64) reasons.Add("long-string-possible-encoded-data");
                    }
                    if (reasons.Count == 0) continue;
                    string nameText = member switch
                    {
                        BaseFieldDeclarationSyntax field => string.Join(",", field.Declaration.Variables.Select(v => v.Identifier.ValueText)),
                        PropertyDeclarationSyntax property => property.Identifier.ValueText,
                        MethodDeclarationSyntax method => method.Identifier.ValueText,
                        ConstructorDeclarationSyntax constructor => constructor.Identifier.ValueText,
                        EnumDeclarationSyntax enumeration => enumeration.Identifier.ValueText,
                        _ => "this[]",
                    };
                    string owner = string.Join(".", member.Ancestors().OfType<TypeDeclarationSyntax>().Reverse().Select(t => t.Identifier.ValueText));
                    members.Add(new { owner, member = nameText, line = member.GetLocation().GetLineSpan().StartLinePosition.Line + 1, reasons = reasons.ToArray() });
                    candidates++;
                }
                bool hasInactiveCode = root.DescendantTrivia(descendIntoTrivia: true).Any(t => t.IsKind(SyntaxKind.DisabledTextTrivia));
                Write(new { kind = "csharp-source", path, sha256 = hash, state = "unreviewed", reviewedLogicalTables = ReviewsFor(path), hasInactiveCode, members });
            }
            Console.WriteLine($"Static index: {files} tracked paths; {candidates} C# candidate members. No dispositions inferred.");
            return 0;

            void Write(object value) => output.WriteLine(JsonSerializer.Serialize(value));

            ReviewLink[] ReviewsFor(string path) => links.TryGetValue(path, out var items)
                ? items.OrderBy(item => item.Id, StringComparer.Ordinal).ToArray() : [];

            void CollectReviewLinks(JsonElement node, string? inheritedOwner)
            {
                if (node.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in node.EnumerateArray()) CollectReviewLinks(item, inheritedOwner);
                    return;
                }
                if (node.ValueKind != JsonValueKind.Object) return;
                string? owner = node.TryGetProperty("owner", out var ownerNode) && ownerNode.ValueKind == JsonValueKind.String
                    ? ownerNode.GetString() : inheritedOwner;
                if (node.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String &&
                    node.TryGetProperty("state", out var state) && state.ValueKind == JsonValueKind.String)
                {
                    var link = new ReviewLink(id.GetString()!, state.GetString()!);
                    if (owner is null) unlinked.Add(link);
                    else
                    {
                        if (!links.TryGetValue(owner, out var items)) links.Add(owner, items = []);
                        items.Add(link);
                    }
                }
                foreach (JsonProperty child in node.EnumerateObject()) CollectReviewLinks(child.Value, owner);
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception.ToString());
            return 1;
        }
    }

    private sealed record ReviewLink(string Id, string State);

    private static partial class NativeConsoleProcess
    {
        [LibraryImport("kernel32.dll")]
        internal static partial uint SetErrorMode(uint mode);
    }
}
