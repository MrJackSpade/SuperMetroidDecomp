using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using SuperMetroid.Core;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The objects a host bound into a game session, and every object reachable from them, each addressed by its path from
/// the root it was first reached from (#1507). A captured state names such an object by its path instead of holding it,
/// and a restore resolves the path against the objects the host binds now. Each object's children are what its type's
/// generated members hand a visitor (#2602): a field's value, an element, a dictionary entry's key or value, a loaded
/// part's value; a value type is walked through to the references it holds. Content loads parts of itself on first use
/// (<see cref="Lazy{T}"/>) and fills some collections on demand: <see cref="Refresh"/> indexes what it loaded since, and a
/// capture that must name it refreshes first. An object once indexed keeps its path.
/// </summary>
internal sealed class BoundObjectGraph
{
    /// <summary>Separates the steps of a path.</summary>
    private const char StepSeparator = '/';

    /// <summary>The step from a loaded <see cref="Lazy{T}"/> to its value.</summary>
    private const string LazyValueStep = "=value";

    /// <summary>Each field's step, built once.</summary>
    private static readonly ConcurrentDictionary<StateGraphField, string> fieldSteps = new(ReferenceEqualityComparer.Instance);

    /// <summary>The registry naming every type the content holds.</summary>
    private readonly StateGraphTypes types;

    /// <summary>The bound roots, by the name a path's first step gives each.</summary>
    private readonly Dictionary<string, object> roots = new(StringComparer.Ordinal);

    /// <summary>Each reachable object's parent and the step from it; a root's parent is null and its step is its name.</summary>
    private readonly Dictionary<object, Origin> origins = new(ReferenceEqualityComparer.Instance);

    /// <summary>Each object's path, built the first time a capture names it.</summary>
    private readonly Dictionary<object, string> paths = new(ReferenceEqualityComparer.Instance);

    /// <summary>The lazily loaded parts not loaded when they were indexed, whose values a refresh indexes once loaded.</summary>
    private readonly List<object> unloaded = [];

    /// <summary>The content's collections, which content may fill on demand, with the version each was last walked at.</summary>
    private readonly List<GrowingCollection> collections = [];

    /// <summary>The collections already tracked.</summary>
    private readonly HashSet<object> trackedCollections = new(ReferenceEqualityComparer.Instance);

    /// <summary>Each path a restore has resolved, and each path leading to one, with the object it names.</summary>
    private readonly Dictionary<string, object> resolved = new(StringComparer.Ordinal);

    /// <summary>Indexes every object reachable from <paramref name="roots"/>, each root named by its number in the list.</summary>
    /// <param name="roots">The objects the host bound into the session.</param>
    internal BoundObjectGraph(IReadOnlyList<object> roots)
        : this(StateGraphTypes.Shared, Ensure.NotNull(roots).Select((root, index) =>
            (index.ToString(CultureInfo.InvariantCulture), Ensure.NotNull(root))).ToArray())
    {
    }

    /// <summary>
    /// Indexes every object reachable from <paramref name="roots"/>, breadth first in the list's order, each root named by
    /// its name, which a path's first step gives.
    /// </summary>
    /// <param name="types">The registry naming every type the content holds.</param>
    /// <param name="roots">The bound objects, each with a distinct name that holds no step separator.</param>
    /// <exception cref="ArgumentException">Two roots share a name, or a name holds a step separator.</exception>
    internal BoundObjectGraph(StateGraphTypes types, IReadOnlyList<(string Name, object Root)> roots)
    {
        this.types = Ensure.NotNull(types);
        Ensure.NotNull(roots);
        var pending = new Queue<object>();
        foreach ((string name, object root) in roots)
        {
            if (name.Contains(StepSeparator, StringComparison.Ordinal) || !this.roots.TryAdd(name, Ensure.NotNull(root)))
                throw new ArgumentException($"Bound root name '{name}' is repeated or holds a step separator.", nameof(roots));
            if (origins.TryAdd(root, new Origin(null, name)))
                pending.Enqueue(root);
        }
        Index(pending);
    }

    /// <summary>
    /// Indexes what the content loaded since the last refresh: the values of lazily loaded parts loaded since, and what the
    /// collections it fills on demand gained. An object keeps the path it was first indexed under.
    /// </summary>
    internal void Refresh()
    {
        var pending = new Queue<object>();
        foreach (GrowingCollection collection in collections)
        {
            int version = collection.Type.Version(collection.Collection) ?? collection.Walked;
            if (version == collection.Walked)
                continue;
            collection.Walked = version;
            pending.Enqueue(collection.Collection);
        }
        for (int index = unloaded.Count - 1; index >= 0; index--)
        {
            object lazy = unloaded[index];
            if (types.For(lazy).LazyValue(lazy, force: false) is not { } loaded)
                continue;
            unloaded.RemoveAt(index);
            if (!loaded.GetType().IsValueType && origins.TryAdd(loaded, new Origin(lazy, LazyValueStep)))
                pending.Enqueue(loaded);
        }
        Index(pending);
    }

    /// <summary>Indexes everything reachable from <paramref name="pending"/> not indexed yet, breadth first, so each path is its shortest.</summary>
    private void Index(Queue<object> pending)
    {
        var walker = new Walker(this, pending);
        while (pending.Count > 0)
        {
            object current = pending.Dequeue();
            if (current is string or Delegate or Type)
                continue;
            StateGraphType type = TypeOf(current);
            if (type.IsLazy && type.LazyValue(current, force: false) is null)
                unloaded.Add(current);
            if (type.Version(current) is { } version && trackedCollections.Add(current))
                collections.Add(new GrowingCollection(current, type, version));
            if (type.ReachesObjects)
                walker.Visit(current, type, prefix: null);
        }
    }

    /// <summary>The handling of a bound object's type.</summary>
    /// <exception cref="NotSupportedException">No registration names the type, so what the object holds cannot be named.</exception>
    private StateGraphType TypeOf(object value) => types.Find(value) ?? throw new NotSupportedException(
        $"The bound content holds a {value.GetType().FullName}, which no registration names, so a debugger state cannot name what it holds.");

    /// <summary>Indexes <paramref name="child"/> at <paramref name="origin"/> unless it is indexed already.</summary>
    private void Reach(object child, Origin origin, Queue<object> pending)
    {
        if (origins.TryAdd(child, origin))
            pending.Enqueue(child);
    }

    /// <summary>Whether <paramref name="value"/> is a bound object, and its path when it is.</summary>
    internal bool TryGetPath(object value, [NotNullWhen(true)] out string? path)
    {
        if (!origins.TryGetValue(value, out Origin origin))
        {
            path = null;
            return false;
        }
        if (!paths.TryGetValue(value, out path))
        {
            path = origin.Parent is { } parent && TryGetPath(parent, out string? parentPath)
                ? parentPath + StepSeparator + origin.Named
                : origin.Named;
            paths.Add(value, path);
        }
        return true;
    }

    /// <summary>
    /// Whether <paramref name="value"/> is a bound object, with its path and its depth: how many steps from a root the walk
    /// first reached it, a root's being zero. Indexing is breadth first, so no path to it from a root is shorter.
    /// </summary>
    internal bool TryGetPath(object value, [NotNullWhen(true)] out string? path, out int depth)
    {
        depth = 0;
        if (!TryGetPath(value, out path))
            return false;
        for (object? parent = origins[value].Parent; parent is not null; parent = origins[parent].Parent)
            depth++;
        return true;
    }

    /// <summary>The object <paramref name="path"/> names among the bound objects.</summary>
    /// <exception cref="InvalidDataException">The path names no object the host binds now.</exception>
    internal object Resolve(string path) => Resolved(Ensure.NotNull(path), path);

    private object Resolved(string path, string named)
    {
        if (resolved.TryGetValue(path, out object? known))
            return known;
        int last = path.LastIndexOf(StepSeparator);
        object value;
        if (last < 0)
        {
            value = roots.TryGetValue(path, out object? root)
                ? root
                : throw new InvalidDataException($"Debugger state names bound root '{path}', which the host does not bind.");
        }
        else
        {
            value = Step(Resolved(path[..last], named), path[(last + 1)..])
                ?? throw new InvalidDataException($"Debugger state names bound object '{named}', which the content bound now does not hold.");
        }
        resolved.Add(path, value);
        return value;
    }

    /// <summary>The value one step reaches from <paramref name="current"/>, or null when it holds nothing there.</summary>
    private object? Step(object current, string step)
    {
        StateGraphType type = TypeOf(current);
        if (step == LazyValueStep && type.IsLazy)
            return type.LazyValue(current, force: true);
        var finder = new StepFinder(step);
        type.Visit(current, finder);
        return finder.Found;
    }

    /// <summary>The step a field names.</summary>
    private static string FieldStep(StateGraphField field) =>
        fieldSteps.TryGetValue(field, out string? step) ? step : fieldSteps.GetOrAdd(field, static named => named.DeclaringType + "::" + named.Name);

    /// <summary>The step an element names.</summary>
    private static string ElementStep(int index) => "#" + index.ToString(CultureInfo.InvariantCulture);

    /// <summary>A content collection and the version it was last walked at.</summary>
    private sealed class GrowingCollection(object collection, StateGraphType type, int walked)
    {
        /// <summary>The collection.</summary>
        internal object Collection { get; } = collection;

        /// <summary>Its handling.</summary>
        internal StateGraphType Type { get; } = type;

        /// <summary>The version it was last walked at.</summary>
        internal int Walked { get; set; } = walked;
    }

    /// <summary>
    /// Where an object was first reached: the object that holds it and the step from it, a field, an element's index or a
    /// named part, built into text only when a capture names the object.
    /// </summary>
    /// <param name="Parent">The holding object, or null for a root.</param>
    /// <param name="Step">The field, the part's step or a walked-through value's whole step, or the root's name.</param>
    /// <param name="Element">The element's index, or negative when the step is not an element.</param>
    private readonly record struct Origin(object? Parent, object? Step, int Element = -1)
    {
        /// <summary>The step from the parent, as a path names it.</summary>
        internal string Named => Element >= 0
            ? ElementStep(Element)
            : Step switch
            {
                StateGraphField fieldStep => FieldStep(fieldStep),
                string text => text,
                _ => throw new InvalidOperationException("A bound object's origin names no step."),
            };
    }

    /// <summary>Indexes the children each object hands it, walking value types through to the references they hold.</summary>
    /// <param name="graph">The index.</param>
    /// <param name="pending">The objects still to walk.</param>
    private sealed class Walker(BoundObjectGraph graph, Queue<object> pending) : IStateGraphChildren
    {
        private object parent = null!;
        private string? prefix;

        /// <summary>Indexes what <paramref name="value"/> holds, each step after <paramref name="prefix"/> when it is a value walked through.</summary>
        internal void Visit(object value, StateGraphType type, string? prefix)
        {
            (object outerParent, string? outerPrefix) = (parent, this.prefix);
            if (prefix is null)
                parent = value;
            this.prefix = prefix;
            type.Visit(value, this);
            (parent, this.prefix) = (outerParent, outerPrefix);
        }

        /// <inheritdoc/>
        public void Element(int index, object? value)
        {
            if (value is not null)
                Held(value, prefix is null ? null : prefix + ElementStep(index), null, index);
        }

        /// <inheritdoc/>
        public void Part(string step, object? value)
        {
            if (value is not null)
                Held(value, prefix is null ? step : prefix + step, null, -1);
        }

        /// <summary>Indexes a reference, or walks a value through to the references it holds.</summary>
        private void Held(object value, string? whole, StateGraphField? field, int element)
        {
            Type runtimeType = value.GetType();
            if (runtimeType.IsValueType)
            {
                // A registered scalar, enum or value holding no reference reaches nothing, and neither does any other scalar.
                StateGraphType? known = graph.types.Registered(runtimeType);
                if (known is { ReachesObjects: false } || runtimeType.IsPrimitive || runtimeType.IsEnum)
                    return;
                StateGraphType type = known ?? graph.TypeOf(value);
                if (!type.ReachesObjects)
                    return;
                string step = whole ?? (field is not null ? FieldStep(field) : ElementStep(element));
                Visit(value, type, step + StepSeparator);
                return;
            }
            Origin origin = whole is not null ? new Origin(parent, whole) : field is not null ? new Origin(parent, field) : new Origin(parent, null, element);
            graph.Reach(value, origin, pending);
        }
    }

    /// <summary>Finds the value one step names among what an object hands it.</summary>
    /// <param name="step">The step.</param>
    private sealed class StepFinder(string step) : IStateGraphChildren
    {
        /// <summary>The value the step names, once found.</summary>
        internal object? Found { get; private set; }

        /// <inheritdoc/>
        public void Element(int index, object? value)
        {
            if (Found is null && ElementStep(index) == step)
                Found = value;
        }

        /// <inheritdoc/>
        public void Part(string named, object? value)
        {
            if (Found is null && named == step)
                Found = value;
        }
    }
}
