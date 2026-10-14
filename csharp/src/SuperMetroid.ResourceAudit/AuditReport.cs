using System.Text.Json;

namespace SuperMetroid.ResourceAudit;

/// <summary>Separate concrete missing exports from unresolved analysis boundaries.</summary>
internal sealed class AuditReport
{
    /// <summary>Finding code for a referenced resource absent from its provider catalog.</summary>
    public const string Missing = "SMRA001";
    /// <summary>Finding code for a reference the static audit cannot yet resolve.</summary>
    public const string Unresolved = "SMRA002";
    /// <summary>Number of resource references examined, including those that resolve.</summary>
    public int ReferenceCount { get; set; }
    /// <summary>Number of missing-reference findings, including repeated uses of one resource.</summary>
    public int MissingCount => Findings.Count(item => item.Code == Missing);
    /// <summary>Number of distinct missing resources by domain and identity.</summary>
    public int MissingResourceCount => Findings.Where(item => item.Code == Missing)
        .Select(item => (item.Domain, item.Resource)).Distinct().Count();
    /// <summary>Number of findings left unresolved by the static audit.</summary>
    public int UnresolvedCount => Findings.Count(item => item.Code != Missing);
    /// <summary>Per-domain reference and export totals.</summary>
    public List<AuditCoverage> Coverage { get; } = [];
    /// <summary>Missing resources and unresolved analysis boundaries.</summary>
    public List<AuditFinding> Findings { get; } = [];
    /// <summary>Production sites that consume audited resources.</summary>
    public List<AuditConsumer> Consumers { get; } = [];
    /// <summary>Resource identities supplied by compiled catalogs.</summary>
    public List<AuditCompiledDefinition> CompiledDefinitions { get; } = [];
    /// <summary>Recorded classifications for audit owners.</summary>
    public List<AuditClassification> Classifications { get; } = [];
    /// <summary>Stable, indented JSON formatting for machine-readable audit output.</summary>
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>Counts a production reference and records it if no matching export exists.</summary>
    public void Require(string domain, string owner, string key, string source, ResourceIndex exports)
    {
        ReferenceCount++;
        if (!exports.Contains(domain, key))
            Findings.Add(new(Missing, domain, owner, key, source,
                $"Referenced resource {key} has no definition in the audited production provider catalog."));
    }

    /// <summary>Records a production reference whose identity cannot be resolved statically.</summary>
    public void Gap(string domain, string owner, string source, string message) =>
        Findings.Add(new(Unresolved, domain, owner, string.Empty, source, message));

    /// <summary>Orders every report section deterministically for stable audit output.</summary>
    public void Sort()
    {
        Findings.Sort((left, right) => StringComparer.Ordinal.Compare(
            $"{left.Code}|{left.Domain}|{left.Resource}|{left.Source}|{left.Owner}",
            $"{right.Code}|{right.Domain}|{right.Resource}|{right.Source}|{right.Owner}"));
        Consumers.Sort((left, right) => StringComparer.Ordinal.Compare(left.Source, right.Source));
        Classifications.Sort((left, right) => StringComparer.Ordinal.Compare(
            $"{left.Source}|{left.Owner}", $"{right.Source}|{right.Owner}"));
        Coverage.Sort((left, right) => StringComparer.Ordinal.Compare(left.Domain, right.Domain));
        CompiledDefinitions.Sort((left, right) => StringComparer.Ordinal.Compare(
            $"{left.Domain}|{left.Resource}", $"{right.Domain}|{right.Resource}"));
    }
}
