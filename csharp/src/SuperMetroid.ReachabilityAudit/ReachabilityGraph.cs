namespace SuperMetroid.ReachabilityAudit;

/// <summary>
/// Symbol keys and the references between them. An edge means "if the owner is reachable, so is
/// the target". A type-implied member becomes reachable whenever its containing type does: the
/// runtime or framework calls it without a source reference (overrides of framework members,
/// static constructors, interop layout fields).
/// </summary>
internal sealed class ReachabilityGraph
{
    private readonly Dictionary<string, HashSet<string>> edges = [];
    private readonly Dictionary<string, HashSet<string>> typeImplied = [];

    public void Edge(string? from, string? to)
    {
        if (from is null || to is null || from == to)
            return;
        if (!edges.TryGetValue(from, out var targets))
            edges[from] = targets = [];
        targets.Add(to);
    }

    public void ImpliedByType(string? typeKey, string? memberKey)
    {
        if (typeKey is null || memberKey is null)
            return;
        if (!typeImplied.TryGetValue(typeKey, out var members))
            typeImplied[typeKey] = members = [];
        members.Add(memberKey);
    }

    public HashSet<string> Reach(IEnumerable<string> roots)
    {
        var reached = new HashSet<string>();
        var pending = new Stack<string>(roots);
        while (pending.Count > 0)
        {
            string current = pending.Pop();
            if (!reached.Add(current))
                continue;
            if (edges.TryGetValue(current, out var targets))
                foreach (string target in targets)
                    pending.Push(target);
            if (typeImplied.TryGetValue(current, out var members))
                foreach (string member in members)
                    pending.Push(member);
        }
        return reached;
    }
}
