using System.Text.Json;

namespace SuperMetroid.ResourceAudit;

/// <summary>Represents audit finding.</summary>
internal sealed record AuditFinding(string Code, string Domain, string Owner, string Resource,
    string Source, string Message);
/// <summary>Represents audit coverage.</summary>
internal sealed record AuditCoverage(string Domain, int References, int Exports);
/// <summary>Represents audit consumer.</summary>
internal sealed record AuditConsumer(string Owner, string Source,     string Resolution);
/// <summary>Represents audit compiled definition.</summary>
internal sealed record AuditCompiledDefinition(string Domain, string Resource);
/// <summary>Represents audit classification.</summary>
internal sealed record AuditClassification(string Owner, string Source);

/// <summary>Separate concrete missing exports from unresolved analysis boundaries.</summary>
internal sealed class AuditReport
{
    /// <summary>Stores missing.</summary>
    public const string Missing = "SMRA001";
    /// <summary>Stores unresolved.</summary>
    public const string Unresolved = "SMRA002";
    /// <summary>Gets or sets reference count.</summary>
    public int ReferenceCount { get; set; }
    /// <summary>Gets missing count.</summary>
    public int MissingCount => Findings.Count(item => item.Code == Missing);
    /// <summary>Gets missing resource count.</summary>
    public int MissingResourceCount => Findings.Where(item => item.Code == Missing)
        .Select(item => (item.Domain, item.Resource)).Distinct().Count();
    /// <summary>Gets unresolved count.</summary>
    public int UnresolvedCount => Findings.Count(item => item.Code != Missing);
    /// <summary>Gets coverage.</summary>
    public List<AuditCoverage> Coverage { get; } = [];
    /// <summary>Gets findings.</summary>
    public List<AuditFinding> Findings { get; } = [];
    /// <summary>Gets consumers.</summary>
    public List<AuditConsumer> Consumers { get; } = [];
    /// <summary>Gets compiled definitions.</summary>
    public List<AuditCompiledDefinition> CompiledDefinitions { get; } = [];
    /// <summary>Gets classifications.</summary>
    public List<AuditClassification> Classifications { get; } = [];
    /// <summary>Gets json options.</summary>
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    /// <summary>Counts a resource reference and records a finding if no matching export exists.</summary>
    public void Require(string domain, string owner, string key, string source, ResourceIndex exports)
    {
        ReferenceCount++;
        if (!exports.Contains(domain, key))
            Findings.Add(new(Missing, domain, owner, key, source,
                $"Referenced resource {key} has no definition in the audited production provider catalog."));
    }

    /// <summary>Records an unresolved analysis gap for a resource owner.</summary>
    public void Gap(string domain, string owner, string source, string message) =>
        Findings.Add(new(Unresolved, domain, owner, string.Empty, source, message));

    /// <summary>Sorts report entries into a deterministic order for output.</summary>
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

/// <summary>Represents resource index.</summary>
internal sealed class ResourceIndex
{
    /// <summary>Stores domains.</summary>
    private readonly Dictionary<string, HashSet<string>> domains = new(StringComparer.Ordinal);
    /// <summary>Adds.</summary>
    public void Add(string domain, string key)
    {
        if (!domains.TryGetValue(domain, out HashSet<string>? entries))
            domains.Add(domain, entries = new(StringComparer.Ordinal));
        entries.Add(key);
    }
    /// <summary>Contains.</summary>
    public bool Contains(string domain, string key) =>
        domains.TryGetValue(domain, out HashSet<string>? entries) && entries.Contains(key);
    /// <summary>Count.</summary>
    public int Count(string domain) => domains.TryGetValue(domain, out var entries) ? entries.Count : 0;
    /// <summary>Address.</summary>
    public static string Address(int bank, int pointer) => $"{bank:X2}:{pointer:X4}";
}
