namespace SuperMetroid.ResourceAudit.Reports;

/// <summary>Tracks exported resource identities separately for each audit domain.</summary>
internal sealed class ResourceIndex
{
    /// <summary>Maps each resource domain to its exported identity set.</summary>
    private readonly Dictionary<string, HashSet<string>> domains = new(StringComparer.Ordinal);
    /// <summary>Adds one exported identity under its domain.</summary>
    public void Add(string domain, string key)
    {
        if (!domains.TryGetValue(domain, out HashSet<string>? entries))
            domains.Add(domain, entries = new(StringComparer.Ordinal));
        entries.Add(key);
    }
    /// <summary>Checks whether a domain exports the requested resource identity.</summary>
    public bool Contains(string domain, string key) =>
        domains.TryGetValue(domain, out HashSet<string>? entries) && entries.Contains(key);
    /// <summary>Counts distinct exported identities in one domain.</summary>
    public int Count(string domain) => domains.TryGetValue(domain, out var entries) ? entries.Count : 0;
    /// <summary>Formats a native bank and pointer as an audit resource key.</summary>
    public static string Address(int bank, int pointer) => $"{bank:X2}:{pointer:X4}";
}
