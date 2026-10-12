using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>
/// The reviewed per-file counts of ratcheted cartridge encoding (#641). Each line is
/// <c>repository-relative path &lt;TAB&gt; rule id &lt;TAB&gt; count</c>; a file may only lose occurrences.
/// </summary>
internal sealed class CartridgeEncodingBaseline
{
    private readonly Dictionary<(string Path, string Rule), int> counts;

    private CartridgeEncodingBaseline(string repositoryRoot, Dictionary<(string Path, string Rule), int> counts)
    {
        RepositoryRoot = repositoryRoot;
        this.counts = counts;
    }

    /// <summary>The repository root: the baseline lives at <c>csharp/cartridge-encoding-baseline.txt</c>.</summary>
    internal string RepositoryRoot { get; }

    /// <summary>Every (path, rule) entry and its allowed count.</summary>
    internal IReadOnlyDictionary<(string Path, string Rule), int> Counts => counts;

    /// <summary>The allowed count of <paramref name="rule"/> in <paramref name="path"/>; zero when unlisted.</summary>
    internal int Allowed(string path, string rule) => counts.TryGetValue((path, rule), out int count) ? count : 0;

    /// <summary>Parses the baseline at <paramref name="baselinePath"/>, failing on any malformed or duplicate line.</summary>
    internal static CartridgeEncodingBaseline Parse(string baselinePath, string content)
    {
        string csharpDirectory = Path.GetDirectoryName(Path.GetFullPath(baselinePath))
            ?? throw new InvalidDataException($"{baselinePath}: the baseline has no directory.");
        string repositoryRoot = Path.GetDirectoryName(csharpDirectory)
            ?? throw new InvalidDataException($"{baselinePath}: the baseline must live under the repository's csharp directory.");
        var counts = new Dictionary<(string, string), int>();
        int lineNumber = 0;
        foreach (string rawLine in content.Split('\n'))
        {
            lineNumber++;
            string line = rawLine.TrimEnd('\r');
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                continue;
            string[] fields = line.Split('\t');
            if (fields.Length != 3 || !int.TryParse(fields[2], NumberStyles.None, CultureInfo.InvariantCulture, out int count) ||
                count <= 0)
            {
                throw new InvalidDataException($"{baselinePath}({lineNumber}): expected 'path<TAB>rule<TAB>positive count'.");
            }
            if (!counts.ContainsKey((fields[0], fields[1])))
                counts.Add((fields[0], fields[1]), count);
            else
                throw new InvalidDataException($"{baselinePath}({lineNumber}): duplicate entry for {fields[0]} {fields[1]}.");
        }
        return new CartridgeEncodingBaseline(repositoryRoot, counts);
    }

    /// <summary>Formats a baseline from measured counts, sorted for stable diffs.</summary>
    internal static string Format(IEnumerable<KeyValuePair<(string Path, string Rule), int>> measured)
    {
        var builder = new StringBuilder();
        builder.Append("# Reviewed per-file counts for the cartridge-encoding ratchet (#641, docs/cartridge-encoding.md).\n");
        builder.Append("# A file may only lose occurrences. Regenerate after a migration with\n");
        builder.Append("#   dotnet run --project csharp/src/SuperMetroid.CartridgeEncodingAudit -c Release -- --solution csharp/SuperMetroid.Full.slnx --write\n");
        builder.Append("# path<TAB>rule<TAB>count\n");
        foreach (var entry in measured.Where(entry => entry.Value > 0)
                     .OrderBy(entry => entry.Key.Path, StringComparer.Ordinal)
                     .ThenBy(entry => entry.Key.Rule, StringComparer.Ordinal))
        {
            builder.Append(entry.Key.Path).Append('\t').Append(entry.Key.Rule).Append('\t')
                .Append(entry.Value.ToString(CultureInfo.InvariantCulture)).Append('\n');
        }
        return builder.ToString();
    }
}
