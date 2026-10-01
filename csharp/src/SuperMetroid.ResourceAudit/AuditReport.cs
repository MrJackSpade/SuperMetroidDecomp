using System.Text.Json;

namespace SuperMetroid.ResourceAudit;

internal sealed record AuditFinding(string Code, string Domain, string Owner, string Resource,
    string Source, string Message);
internal sealed record AuditCoverage(string Domain, int References, int Exports);
internal sealed record AuditConsumer(string Domain, string Owner, string Source, string Arguments,
    string Resolution);
internal sealed record AuditCompiledDefinition(string Domain, string Resource, string Owner,
    string Source, string Reason);
internal sealed record AuditClassification(string Rule, string Owner, string Source, string Reason,
    string[] ProviderSources);

/// <summary>Separate concrete missing exports from unresolved analysis boundaries.</summary>
internal sealed class AuditReport
{
    public const string Missing = "SMRA001";
    public const string Unresolved = "SMRA002";
    public int Version { get; } = 2;
    public string Scope { get; } = "Compiled definition dependencies and Core resource consumer inventory; no gameplay execution.";
    public int ReferenceCount { get; set; }
    public int MissingCount => Findings.Count(item => item.Code == Missing);
    public int MissingResourceCount => Findings.Where(item => item.Code == Missing)
        .Select(item => (item.Domain, item.Resource)).Distinct().Count();
    public int UnresolvedCount => Findings.Count(item => item.Code != Missing);
    public List<AuditCoverage> Coverage { get; } = [];
    public List<AuditFinding> Findings { get; } = [];
    public List<AuditConsumer> Consumers { get; } = [];
    public List<AuditCompiledDefinition> CompiledDefinitions { get; } = [];
    public List<AuditClassification> Classifications { get; } = [];
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
    };

    public void Require(string domain, string owner, string key, string source, ResourceIndex exports)
    {
        ReferenceCount++;
        if (!exports.Contains(domain, key))
            Findings.Add(new(Missing, domain, owner, key, source,
                $"Referenced resource {key} has no definition in the audited production provider catalog."));
    }

    public void Gap(string domain, string owner, string source, string message) =>
        Findings.Add(new(Unresolved, domain, owner, string.Empty, source, message));

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

internal sealed class ResourceIndex
{
    private readonly Dictionary<string, HashSet<string>> domains = new(StringComparer.Ordinal);
    public void Add(string domain, string key)
    {
        if (!domains.TryGetValue(domain, out HashSet<string>? entries))
            domains.Add(domain, entries = new(StringComparer.Ordinal));
        entries.Add(key);
    }
    public bool Contains(string domain, string key) =>
        domains.TryGetValue(domain, out HashSet<string>? entries) && entries.Contains(key);
    public int Count(string domain) => domains.TryGetValue(domain, out var entries) ? entries.Count : 0;
    public static string Address(int bank, int pointer) => $"{bank:X2}:{pointer:X4}";
}
