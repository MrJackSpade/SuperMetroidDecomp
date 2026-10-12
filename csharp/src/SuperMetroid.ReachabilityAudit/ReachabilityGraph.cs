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

    /// <summary>The shortest chain of keys from a root to <paramref name="target"/>, or null when unreachable.</summary>
    public IReadOnlyList<string>? PathTo(IEnumerable<string> roots, string target)
    {
        var parent = new Dictionary<string, string?>();
        var pending = new Queue<string>();
        foreach (string root in roots)
            if (parent.TryAdd(root, null))
                pending.Enqueue(root);
        while (pending.Count > 0)
        {
            string current = pending.Dequeue();
            if (current == target)
            {
                var path = new List<string>();
                for (string? step = current; step is not null; step = parent[step])
                    path.Add(step);
                path.Reverse();
                return path;
            }
            IEnumerable<string> next = (edges.TryGetValue(current, out var targets) ? targets : [])
                .Concat(typeImplied.TryGetValue(current, out var members) ? members : []);
            foreach (string step in next)
                if (parent.TryAdd(step, current))
                    pending.Enqueue(step);
        }
        return null;
    }
}
