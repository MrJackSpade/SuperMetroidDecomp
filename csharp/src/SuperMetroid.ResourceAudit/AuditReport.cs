using System.Text.Json;

namespace SuperMetroid.ResourceAudit;

/// <summary>One missing resource export or unresolved audit boundary.</summary>
/// <param name="Code">Stable audit finding code.</param>
/// <param name="Domain">Resource domain in which the finding occurred.</param>
/// <param name="Owner">Source owner responsible for the resource reference.</param>
/// <param name="Resource">Referenced resource identity.</param>
/// <param name="Source">Source location or operation that produced the finding.</param>
/// <param name="Message">Human-readable explanation of the missing export or boundary.</param>
internal sealed record AuditFinding(string Code, string Domain, string Owner, string Resource,
    string Source, string Message);
/// <summary>Reference and export totals for one audited resource domain.</summary>
/// <param name="Domain">Audited resource domain.</param>
/// <param name="References">Number of resource references discovered in the domain.</param>
/// <param name="Exports">Number of matching compiled resource exports.</param>
internal sealed record AuditCoverage(string Domain, int References, int Exports);
/// <summary>One source operation that consumes an audited resource and its resolution status.</summary>
/// <param name="Owner">Source owner containing the resource operation.</param>
/// <param name="Source">Source operation or location being classified.</param>
/// <param name="Resolution">Static resolution assigned to the consumer.</param>
internal sealed record AuditConsumer(string Owner, string Source,     string Resolution);
/// <summary>One resource definition found in compiled source.</summary>
/// <param name="Domain">Resource domain containing the compiled definition.</param>
/// <param name="Resource">Compiled resource identity.</param>
internal sealed record AuditCompiledDefinition(string Domain, string Resource);
/// <summary>One source site classified as a resource consumer.</summary>
/// <param name="Owner">Source owner containing the classified site.</param>
/// <param name="Source">Classified source operation or location.</param>
internal sealed record AuditClassification(string Owner, string Source);

/// <summary>Separate concrete missing exports from unresolved analysis boundaries.</summary>
internal sealed class AuditReport
{
    /// <summary>Finding code for a statically identified resource without a provider definition.</summary>
    public const string Missing = "SMRA001";
    /// <summary>Finding code for a source boundary the audit cannot resolve statically.</summary>
    public const string Unresolved = "SMRA002";
    /// <summary>Number of concrete provider lookups evaluated.</summary>
    public int ReferenceCount { get; set; }
    /// <summary>Number of distinct missing findings.</summary>
    public int MissingCount => Findings.Count(item => item.Code == Missing);
    /// <summary>Number of distinct domain/resource pairs reported missing.</summary>
    public int MissingResourceCount => Findings.Where(item => item.Code == Missing)
        .Select(item => (item.Domain, item.Resource)).Distinct().Count();
    /// <summary>Number of findings that describe an unresolved boundary.</summary>
    public int UnresolvedCount => Findings.Count(item => item.Code != Missing);
    /// <summary>Coverage totals grouped by resource domain.</summary>
    public List<AuditCoverage> Coverage { get; } = [];
    /// <summary>Missing and unresolved findings emitted by the audit.</summary>
    public List<AuditFinding> Findings { get; } = [];
    /// <summary>Consumer source sites and their resolution states.</summary>
    public List<AuditConsumer> Consumers { get; } = [];
    /// <summary>Resource definitions compiled from source catalogs.</summary>
    public List<AuditCompiledDefinition> CompiledDefinitions { get; } = [];
    /// <summary>Consumer-site classifications included in the JSON report.</summary>
    public List<AuditClassification> Classifications { get; } = [];
    /// <summary>Stable JSON formatting options shared by audit reports.</summary>
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>Counts a resource reference and records it when its domain/key is absent from the exports.</summary>
    public void Require(string domain, string owner, string key, string source, ResourceIndex exports)
    {
        ReferenceCount++;
        if (!exports.Contains(domain, key))
            Findings.Add(new(Missing, domain, owner, key, source,
                $"Referenced resource {key} has no definition in the audited production provider catalog."));
    }

    /// <summary>Records a lookup whose target could not be established by the audit.</summary>
    public void Gap(string domain, string owner, string source, string message) =>
        Findings.Add(new(Unresolved, domain, owner, string.Empty, source, message));

    /// <summary>Orders report collections deterministically for stable text and JSON output.</summary>
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

/// <summary>Set of resource keys known to be exported by each audited domain.</summary>
internal sealed class ResourceIndex
{
    /// <summary>Domain names mapped to their unique exported resource keys.</summary>
    private readonly Dictionary<string, HashSet<string>> domains = new(StringComparer.Ordinal);
    /// <summary>Adds a resource key to its export domain.</summary>
    public void Add(string domain, string key)
    {
        if (!domains.TryGetValue(domain, out HashSet<string>? entries))
            domains.Add(domain, entries = new(StringComparer.Ordinal));
        entries.Add(key);
    }
    /// <summary>Checks whether the specified resource key has an export in the given domain.</summary>
    public bool Contains(string domain, string key) =>
        domains.TryGetValue(domain, out HashSet<string>? entries) && entries.Contains(key);
    /// <summary>Returns the number of unique exported keys in a domain.</summary>
    public int Count(string domain) => domains.TryGetValue(domain, out var entries) ? entries.Count : 0;
    /// <summary>Formats a bank and pointer as the canonical hexadecimal audit key.</summary>
    public static string Address(int bank, int pointer) => $"{bank:X2}:{pointer:X4}";
}
