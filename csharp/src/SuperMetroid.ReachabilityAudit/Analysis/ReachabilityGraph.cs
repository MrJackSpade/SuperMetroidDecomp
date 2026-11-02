namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>
/// Symbol keys and the references between them. An edge means "if the owner is reachable, so is
/// the target". A type-implied member becomes reachable whenever its containing type does: the
/// runtime or framework calls it without a source reference (overrides of framework members,
/// static constructors, interop layout fields).
/// </summary>
internal sealed class ReachabilityGraph
{
    /// <summary>Direct source-reference edges between symbol keys.</summary>
    private readonly Dictionary<string, HashSet<string>> edges = [];
    /// <summary>Framework-driven members reached whenever their containing type is reached.</summary>
    private readonly Dictionary<string, HashSet<string>> typeImplied = [];
    /// <summary>Per declaration, the <see cref="FrameworkDispatch"/> keys of the framework interface members it calls.</summary>
    private readonly Dictionary<string, HashSet<string>> dispatchCalls = [];
    /// <summary>Per <see cref="FrameworkDispatch"/> key, the repository members implementing that contract member.</summary>
    private readonly Dictionary<string, List<DispatchImplementation>> dispatchImplementations = [];
    /// <summary>Per type, its instance constructors, compiler-declared ones included.</summary>
    private readonly Dictionary<string, List<string>> constructors = [];

    /// <summary>Adds a direct reference, ignoring missing or self-referential symbol keys.</summary>
    public void Edge(string? from, string? to)
    {
        if (from is null || to is null || from == to)
            return;
        if (!edges.TryGetValue(from, out var targets))
            edges[from] = targets = [];
        targets.Add(to);
    }

    /// <summary>Records a member whose runtime use follows reachability of its containing type.</summary>
    public void ImpliedByType(string? typeKey, string? memberKey)
    {
        if (typeKey is null || memberKey is null)
            return;
        if (!typeImplied.TryGetValue(typeKey, out var members))
            typeImplied[typeKey] = members = [];
        members.Add(memberKey);
    }

    /// <summary>Records that <paramref name="owner"/> calls a framework interface member through <paramref name="dispatchKey"/>.</summary>
    public void DispatchCall(string? owner, string dispatchKey)
    {
        if (owner is null)
            return;
        if (!dispatchCalls.TryGetValue(owner, out var keys))
            dispatchCalls[owner] = keys = [];
        keys.Add(dispatchKey);
    }

    /// <summary>
    /// Records a repository member implementing the framework contract member named by <paramref name="dispatchKey"/>,
    /// with the keys whose reachability means its type is instantiated: its constructors, or the type itself for a value
    /// type, whose default value needs no constructor.
    /// </summary>
    public void DispatchImplementation(string dispatchKey, string? implementationKey, IReadOnlyList<string> instantiationKeys)
    {
        if (implementationKey is null)
            return;
        if (!dispatchImplementations.TryGetValue(dispatchKey, out var implementations))
            dispatchImplementations[dispatchKey] = implementations = [];
        implementations.Add(new DispatchImplementation(implementationKey, instantiationKeys));
    }

    /// <summary>Records the instance constructors of <paramref name="typeKey"/>.</summary>
    public void Constructors(string? typeKey, IEnumerable<string> constructorKeys)
    {
        if (typeKey is null)
            return;
        if (!constructors.TryGetValue(typeKey, out var keys))
            constructors[typeKey] = keys = [];
        keys.AddRange(constructorKeys);
    }

    /// <summary>
    /// Traverses direct and type-implied edges to a fixed point from the supplied roots. A reached type's implementations
    /// of framework interface members are already reached through its type-implied edges, so calls through framework
    /// interfaces add nothing to this walk.
    /// </summary>
    public HashSet<string> Reach(IEnumerable<string> roots) => Walk(roots, _ => true, null);

    /// <summary>
    /// Traverses direct and type-implied edges from the supplied roots, entering only keys <paramref name="admits"/>
    /// accepts: a reference into a refused key is not followed, so nothing is reached through it. A refused key can still
    /// have built the objects the admitted walk reads through a framework interface (the import builds a colour band the
    /// game indexes as an <c>IReadOnlyList&lt;T&gt;</c>), so a reached call through a framework interface member also
    /// reaches each repository implementation of that member whose type <paramref name="instantiated"/> shows constructed.
    /// For the same reason a reached type's instance constructors are reached: the refused import builds the objects the
    /// admitted walk reads, so their constructors are part of what the admitted code relies on.
    /// </summary>
    public HashSet<string> Reach(IEnumerable<string> roots, Func<string, bool> admits, IReadOnlySet<string> instantiated) =>
        Walk(roots, admits, instantiated);

    /// <summary>
    /// Walks edges from <paramref name="roots"/> through admitted keys; with <paramref name="instantiated"/>, also follows
    /// framework-interface calls to the implementations of instantiated types and reaches each reached type's constructors.
    /// </summary>
    private HashSet<string> Walk(IEnumerable<string> roots, Func<string, bool> admits, IReadOnlySet<string>? instantiated)
    {
        var reached = new HashSet<string>();
        var pending = new Stack<string>(roots);
        while (pending.Count > 0)
        {
            string current = pending.Pop();
            if (!admits(current) || !reached.Add(current))
                continue;
            if (edges.TryGetValue(current, out var targets))
                foreach (string target in targets)
                    pending.Push(target);
            if (typeImplied.TryGetValue(current, out var members))
                foreach (string member in members)
                    pending.Push(member);
            if (instantiated is not null && constructors.TryGetValue(current, out var typeConstructors))
                foreach (string constructor in typeConstructors)
                    pending.Push(constructor);
            if (instantiated is not null && dispatchCalls.TryGetValue(current, out var calls))
                foreach (string call in calls)
                    if (dispatchImplementations.TryGetValue(call, out var implementations))
                        foreach (var implementation in implementations)
                            if (implementation.InstantiationKeys.Any(instantiated.Contains))
                                pending.Push(implementation.ImplementationKey);
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
