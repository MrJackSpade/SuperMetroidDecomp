using SuperMetroid.Core;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace SuperMetroid.Diagnostics.SaveStates;

/// <summary>
/// The objects a host bound into a game session, and every object reachable from them, each addressed by its path from
/// the root it was first reached from (#1507). A captured state names such an object by its path instead of holding it,
/// and a restore resolves the path against the objects the host binds now. The host decides what of the session is game
/// state and what is the content it supplies; Core marks only what is no state at all, such as the host's subscriptions.
/// Content loads parts of itself on first use (<see cref="Lazy{T}"/>) and fills some collections on demand:
/// <see cref="Refresh"/> indexes what it loaded since, and a capture that must name it refreshes first. A path steps
/// through a lazily loaded part by its value, which a restore loads again. An object once indexed keeps its path.
/// A graph of roots that extend another graph's roots (<see cref="BoundObjectGraph(IReadOnlyList{object}, BoundObjectGraph)"/>)
/// indexes only what its own roots reach first and names the rest by the other graph's paths, renumbered, so a list
/// that extends indexed content does not walk that content again.
/// </summary>
internal sealed class BoundObjectGraph
{
    /// <summary>Separates the steps of a path.</summary>
    private const char StepSeparator = '/';

    /// <summary>Begins an array step, which is followed by the element's indices.</summary>
    private const char ElementStep = '#';

    /// <summary>The step from a loaded <see cref="Lazy{T}"/> to its value.</summary>
    private const string LazyValueStep = "=value";

    /// <summary>Separates a field step's declaring type from its field name.</summary>
    private const string FieldSeparator = "::";

    /// <summary>The bound roots, in the order their paths number them.</summary>
    private readonly object[] roots;

    /// <summary>The graph whose roots follow this graph's own, or null for a graph that indexes all its roots itself.</summary>
    private readonly BoundObjectGraph? extended;

    /// <summary>How many roots precede the extended graph's, which its root numbers are offset by.</summary>
    private readonly int extendedOffset;

    /// <summary>Each reachable object's parent and the step from it; a root's parent is null and its step is its number.</summary>
    private readonly Dictionary<object, Origin> origins = new(ReferenceEqualityComparer.Instance);

    /// <summary>Each object's path, built the first time a capture names it.</summary>
    private readonly Dictionary<object, string> paths = new(ReferenceEqualityComparer.Instance);

    /// <summary>The lazily loaded parts not loaded when they were indexed, whose values a refresh indexes once loaded.</summary>
    private readonly List<object> unloaded = [];

    /// <summary>The content's collections, which content may fill on demand, with the version each was last walked at.</summary>
    private readonly List<GrowingCollection> collections = [];

    /// <summary>The collections already tracked.</summary>
    private readonly HashSet<object> trackedCollections = new(ReferenceEqualityComparer.Instance);

    /// <summary>Each collection type's compiled version reader, or null for a type that keeps no version.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Func<object, int>?> versionReaders = new();

    /// <summary>Whether a type's values can hold references, by type; one that cannot holds nothing to index.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, bool> holdsReferences = new();

    /// <summary>Each type's instance fields that can hold a reference, with the step naming each.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, (FieldInfo Field, string Step)[]> referenceFields = new();

    /// <summary>Each type's instance fields in a stable order.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, FieldInfo[]> allFields = new();

    /// <summary>Indexes every object reachable from <paramref name="roots"/>.</summary>
    /// <param name="roots">The objects the host bound into the session.</param>
    internal BoundObjectGraph(IReadOnlyList<object> roots)
    {
        Ensure.NotNull(roots);
        this.roots = [.. roots];
        var pending = new Queue<object>();
        for (int index = 0; index < this.roots.Length; index++)
        {
            object root = Ensure.NotNull(this.roots[index]);
            if (origins.TryAdd(root, new Origin(null, index.ToString(System.Globalization.CultureInfo.InvariantCulture), 0)))
                pending.Enqueue(root);
        }
        Index(pending, refreshing: false);
    }

    /// <summary>
    /// Indexes the roots <paramref name="prefix"/> followed by <paramref name="extended"/>'s roots, walking only what the
    /// prefix reaches first. The index is the one a graph of all those roots makes: breadth first from every root at once,
    /// earlier roots first, an object takes the path it is first reached by. An object the prefix reaches no later than the
    /// extended graph does is reached first through the prefix, and so is everything reached first through it; every other
    /// object the extended graph holds is reached first through the extended graph's roots, by the path it holds, so it keeps
    /// that path with its root renumbered past the prefix. The walk from the prefix therefore stops at an object the extended
    /// graph reaches sooner.
    /// </summary>
    /// <param name="prefix">The roots numbered before the extended graph's.</param>
    /// <param name="extended">The indexed graph whose roots follow the prefix.</param>
    internal BoundObjectGraph(IReadOnlyList<object> prefix, BoundObjectGraph extended)
    {
        Ensure.NotNull(prefix);
        this.extended = Ensure.NotNull(extended);
        extendedOffset = prefix.Count;
        roots = [.. prefix, .. extended.roots];
        var pending = new Queue<object>();
        for (int index = 0; index < prefix.Count; index++)
        {
            object root = Ensure.NotNull(prefix[index]);
            if (origins.TryAdd(root, new Origin(null, index.ToString(System.Globalization.CultureInfo.InvariantCulture), 0)))
                pending.Enqueue(root);
        }
        Index(pending, refreshing: false);
    }

    /// <summary>
    /// Indexes what the content loaded since the last refresh: the values of lazily loaded parts loaded since. An object
    /// keeps the path it was first indexed under, so a path never changes once a capture has used it. A graph that extends
    /// another refreshes that graph first, and indexes from its own roots only what that graph does not already name.
    /// </summary>
    internal void Refresh()
    {
        extended?.Refresh();
        var pending = new Queue<object>();
        foreach (GrowingCollection collection in collections)
        {
            int version = collection.Version();
            if (version == collection.Walked)
                continue;
            collection.Walked = version;
            pending.Enqueue(collection.Collection);
        }
        for (int index = unloaded.Count - 1; index >= 0; index--)
        {
            object lazy = unloaded[index];
            if (LazyValue(lazy, force: false) is not { } loaded)
                continue;
            unloaded.RemoveAt(index);
            if (!loaded.GetType().IsValueType)
                Reach(loaded, new Origin(lazy, LazyValueStep, origins[lazy].Depth + 1), refreshing: true, pending);
        }
        Index(pending, refreshing: true);
    }

    /// <summary>
    /// Indexes everything reachable from <paramref name="pending"/> that is not indexed yet, breadth first, so each object's
    /// path is its shortest from a root.
    /// </summary>
    /// <param name="pending">The indexed objects whose children are walked.</param>
    /// <param name="refreshing">Whether the walk indexes content loaded since the graph was first indexed.</param>
    private void Index(Queue<object> pending, bool refreshing)
    {
        while (pending.Count > 0)
        {
            object current = pending.Dequeue();
            int depth = origins[current].Depth + 1;
            if (IsLazy(current.GetType()) && LazyValue(current, force: false) is null)
                unloaded.Add(current);
            if (VersionReader(current.GetType()) is { } versionReader && trackedCollections.Add(current))
                collections.Add(new GrowingCollection(current, versionReader));
            IndexChildren(current, depth, refreshing, pending);
        }
    }

    /// <summary>
    /// Indexes every object <paramref name="value"/> holds that is not indexed yet, as <see cref="Children"/> names them
    /// (#1532): a reference held by a field or an array element is reached directly, its step built only when a capture
    /// names it, and only a value type walked through goes through <see cref="Reached"/>. Each child is reached in
    /// <paramref name="depth"/> steps from a root.
    /// </summary>
    private void IndexChildren(object value, int depth, bool refreshing, Queue<object> pending)
    {
        Type type = value.GetType();
        if (type == typeof(string) || value is Delegate || value is Type || value is MemberInfo || value is Pointer)
            return;
        if (IsLazy(type))
        {
            if (LazyValue(value, force: false) is { } loaded && !loaded.GetType().IsValueType)
                Reach(loaded, new Origin(value, LazyValueStep, depth), refreshing, pending);
            return;
        }
        if (value is Array array)
        {
            Type elementType = ReflectionBoundary.ElementType(type);
            if (RawArrayElements.IsRaw(elementType) || elementType.IsPointer || !HoldsReferences(elementType))
                return;
            if (!elementType.IsValueType && array is object?[] items)
            {
                // A vector of references is an array of objects, read without boxing; an element that is itself a boxed
                // value is walked through like any other value.
                for (int index = 0; index < items.Length; index++)
                {
                    if (items[index] is not { } item)
                        continue;
                    if (item.GetType().IsValueType)
                        ReachThrough(item, value, ElementStep + index.ToString(System.Globalization.CultureInfo.InvariantCulture), depth, refreshing, pending);
                    else
                        Reach(item, new Origin(value, Step: null, depth, index), refreshing, pending);
                }
                return;
            }
            foreach ((object child, string step) in Children(value))
                Reach(child, new Origin(value, step, depth), refreshing, pending);
            return;
        }
        foreach ((FieldInfo field, string step) in ReferenceFields(type))
        {
            if (field.GetValue(value) is not { } fieldValue)
                continue;
            if (fieldValue.GetType().IsValueType)
                ReachThrough(fieldValue, value, step, depth, refreshing, pending);
            else
                Reach(fieldValue, new Origin(value, step, depth), refreshing, pending);
        }
    }

    /// <summary>Indexes the references the value <paramref name="held"/> holds, each named through its step from <paramref name="parent"/>.</summary>
    private void ReachThrough(object held, object parent, string step, int depth, bool refreshing, Queue<object> pending)
    {
        foreach ((object child, string childStep) in Reached(held, step))
            Reach(child, new Origin(parent, childStep, depth), refreshing, pending);
    }

    /// <summary>
    /// Indexes <paramref name="child"/> at <paramref name="origin"/> unless it is indexed already, or the graph this one
    /// extends names it as <see cref="Admits"/> decides.
    /// </summary>
    private void Reach(object child, Origin origin, bool refreshing, Queue<object> pending)
    {
        if (Admits(child, origin.Depth, refreshing) && origins.TryAdd(child, origin))
            pending.Enqueue(child);
    }

    /// <summary>
    /// Whether this graph indexes <paramref name="child"/>, reached from its own roots at <paramref name="depth"/> steps:
    /// always for a graph that extends none. A graph that extends another leaves it to that graph when that graph reaches it
    /// in fewer steps, as a walk from every root at once reaches it first from there, and, once first indexed, leaves it every
    /// object that graph names, so a path it named never changes.
    /// </summary>
    private bool Admits(object child, int depth, bool refreshing) =>
        extended is null || !extended.Holds(child, out int heldDepth) || (!refreshing && heldDepth >= depth);

    /// <summary>Whether this graph names <paramref name="value"/>, and the steps from its roots it was reached in when it does.</summary>
    private bool Holds(object value, out int depth)
    {
        if (origins.TryGetValue(value, out Origin origin))
        {
            depth = origin.Depth;
            return true;
        }
        if (extended is not null)
            return extended.Holds(value, out depth);
        depth = 0;
        return false;
    }

    /// <summary>Whether <paramref name="value"/> is a bound object, and its path when it is.</summary>
    internal bool TryGetPath(object value, [NotNullWhen(true)] out string? path)
    {
        if (paths.TryGetValue(value, out path))
            return true;
        if (origins.TryGetValue(value, out Origin origin))
        {
            path = origin.Parent is { } parent && TryGetPath(parent, out string? parentPath)
                ? parentPath + StepSeparator + origin.Named
                : origin.Named;
            paths.Add(value, path);
            return true;
        }
        if (extended is not null && extended.TryGetPath(value, out string? extendedPath))
        {
            path = Renumbered(extendedPath);
            paths.Add(value, path);
            return true;
        }
        path = null;
        return false;
    }

    /// <summary>A path of the extended graph with its root numbered among this graph's roots.</summary>
    private string Renumbered(string extendedPath)
    {
        int separator = extendedPath.IndexOf(StepSeparator, StringComparison.Ordinal);
        ReadOnlySpan<char> root = separator < 0 ? extendedPath.AsSpan() : extendedPath.AsSpan(0, separator);
        int number = int.Parse(root, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture) + extendedOffset;
        return separator < 0
            ? number.ToString(System.Globalization.CultureInfo.InvariantCulture)
            : number.ToString(System.Globalization.CultureInfo.InvariantCulture) + extendedPath[separator..];
    }

    /// <summary>The object <paramref name="path"/> names among the bound objects.</summary>
    /// <exception cref="InvalidDataException">The path names no object the host binds now.</exception>
    internal object Resolve(string path)
    {
        if (resolved.TryGetValue(Ensure.NotNull(path), out object? known))
            return known;
        object value = ResolveSteps(path);
        resolved.Add(path, value);
        return value;
    }

    /// <summary>Each path a restore has resolved, with the object it names.</summary>
    private readonly Dictionary<string, object> resolved = new(StringComparer.Ordinal);

    /// <summary>The object <paramref name="path"/> names, found step by step from its root.</summary>
    /// <exception cref="InvalidDataException">The path names no object the host binds now.</exception>
    private object ResolveSteps(string path)
    {
        string[] steps = path.Split(StepSeparator);
        if (!int.TryParse(steps[0], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int root) ||
            (uint)root >= (uint)roots.Length)
            throw new InvalidDataException($"Debugger state names bound root '{steps[0]}', which the host does not bind.");
        object? current = roots[root];
        for (int index = 1; index < steps.Length; index++)
        {
            current = current is null ? null : Step(current, steps[index]);
            if (current is null)
                throw new InvalidDataException($"Debugger state names bound object '{path}', which the content bound now does not hold.");
        }
        return current ?? throw new InvalidDataException($"Debugger state names bound object '{path}', which is null.");
    }

    /// <summary>Every object <paramref name="value"/> holds, with the step that reaches it; value types are walked through.</summary>
    private static IEnumerable<(object Child, string Step)> Children(object value)
    {
        Type type = value.GetType();
        if (type == typeof(string) || value is Delegate || value is Type || value is MemberInfo || value is Pointer)
            yield break;
        if (IsLazy(type))
        {
            if (LazyValue(value, force: false) is { } loaded && !loaded.GetType().IsValueType)
                yield return (loaded, LazyValueStep);
            yield break;
        }
        if (value is Array array)
        {
            Type elementType = ReflectionBoundary.ElementType(type);
            if (RawArrayElements.IsRaw(elementType) || elementType.IsPointer || !HoldsReferences(elementType))
                yield break;
            if (array.Rank == 1)
            {
                for (int index = 0; index < array.Length; index++)
                {
                    if (array.GetValue(index) is not { } item)
                        continue;
                    string step = ElementStep + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    foreach ((object child, string childStep) in Reached(item, step))
                        yield return (child, childStep);
                }
                yield break;
            }
            foreach (int[] indices in ArrayIndices(array))
            {
                object? element = array.GetValue(indices);
                if (element is not null)
                {
                    string step = ElementStep + string.Join(',', indices);
                    foreach ((object child, string childStep) in Reached(element, step))
                        yield return (child, childStep);
                }
            }
            yield break;
        }
        foreach ((FieldInfo field, string step) in ReferenceFields(type))
        {
            object? fieldValue = field.GetValue(value);
            if (fieldValue is null)
                continue;
            foreach ((object child, string childStep) in Reached(fieldValue, step))
                yield return (child, childStep);
        }
    }

    /// <summary>A reference value is reached itself; a value type is walked through to the references it holds.</summary>
    private static IEnumerable<(object Child, string Step)> Reached(object value, string step)
    {
        if (!value.GetType().IsValueType)
        {
            yield return (value, step);
            yield break;
        }
        if (value.GetType().IsPrimitive || value.GetType().IsEnum)
            yield break;
        foreach ((object child, string childStep) in Children(value))
            yield return (child, step + StepSeparator + childStep);
    }

    /// <summary>The value one step reaches from <paramref name="current"/>, or null when it holds nothing there.</summary>
    private static object? Step(object current, string step)
    {
        if (step == LazyValueStep)
            return IsLazy(current.GetType()) ? LazyValue(current, force: true) : null;
        if (step.StartsWith(ElementStep))
        {
            int[] indices = [.. step[1..].Split(',').Select(index => int.Parse(index, System.Globalization.CultureInfo.InvariantCulture))];
            return current is Array array && indices.Length == array.Rank ? array.GetValue(indices) : null;
        }
        if (!step.Contains(FieldSeparator, StringComparison.Ordinal))
            throw new InvalidDataException($"Debugger state has a malformed bound step '{step}'.");
        foreach ((FieldInfo field, string fieldStep) in ReferenceFields(current.GetType()))
        {
            if (string.Equals(fieldStep, step, StringComparison.Ordinal))
                return field.GetValue(current);
        }
        return null;
    }

    /// <summary>
    /// A compiled reader of a <c>System.Collections.Generic</c> collection's version, which every change to it advances;
    /// null for a type that keeps none.
    /// </summary>
    [ReflectionAudit("the bound-object index re-walks a content collection only when its version shows it changed")]
    private static Func<object, int>? VersionReader(Type type) => versionReaders.GetOrAdd(type, static candidate =>
    {
        if (candidate.Namespace != "System.Collections.Generic" || !candidate.IsGenericType ||
            candidate.GetField("_version", BindingFlags.Instance | BindingFlags.NonPublic) is not { } field || field.FieldType != typeof(int))
            return null;
        var collection = System.Linq.Expressions.Expression.Parameter(typeof(object), "collection");
        return System.Linq.Expressions.Expression.Lambda<Func<object, int>>(
            System.Linq.Expressions.Expression.Field(System.Linq.Expressions.Expression.Convert(collection, candidate), field),
            collection).Compile();
    });

    /// <summary>A content collection and the version it was last walked at.</summary>
    /// <param name="collection">The collection.</param>
    /// <param name="version">Its compiled version reader.</param>
    private sealed class GrowingCollection(object collection, Func<object, int> version)
    {
        /// <summary>The collection.</summary>
        internal object Collection { get; } = collection;

        /// <summary>The version it was last walked at.</summary>
        internal int Walked { get; set; } = version(collection);

        /// <summary>Its version now.</summary>
        internal int Version() => version(Collection);
    }

    /// <summary>Whether <paramref name="type"/> is <see cref="Lazy{T}"/>.</summary>
    private static bool IsLazy(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Lazy<>);

    /// <summary>A lazy part's value: loaded now when <paramref name="force"/>, else only when already loaded.</summary>
    [ReflectionAudit("bound-object paths step through the installed content's lazily loaded parts by their value")]
    private static object? LazyValue(object lazy, bool force)
    {
        LazyAccess access = lazyAccess.GetOrAdd(lazy.GetType(), static type =>
            (typeof(BoundObjectGraph).GetMethod(nameof(AccessFor), BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new InvalidOperationException("The lazy accessor factory is missing."))
                .MakeGenericMethod(type.GetGenericArguments()).Invoke(null, null) as LazyAccess
                ?? throw new InvalidOperationException($"No accessors were made for {type}."));
        return force || access.IsLoaded(lazy) ? access.Value(lazy) : null;
    }

    /// <summary>The accessors of one <see cref="Lazy{T}"/> type, compiled once.</summary>
    /// <param name="IsLoaded">Whether a lazy part has loaded its value.</param>
    /// <param name="Value">A lazy part's value, loading it if it has not loaded.</param>
    private sealed record LazyAccess(Func<object, bool> IsLoaded, Func<object, object?> Value);

    /// <summary>The accessors of <see cref="Lazy{T}"/> of <typeparamref name="T"/>.</summary>
    private static LazyAccess AccessFor<T>() => new(
        static lazy => ((Lazy<T>)lazy).IsValueCreated,
        static lazy => ((Lazy<T>)lazy).Value);

    /// <summary>Each lazy type's accessors.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, LazyAccess> lazyAccess = new();

    /// <summary>Whether values of <paramref name="type"/> can hold a reference; a reference type always can.</summary>
    [ReflectionAudit("the bound-object index skips value types that hold no reference, which cannot reach content")]
    private static bool HoldsReferences(Type type) =>
        holdsReferences.GetOrAdd(type, static candidate => !candidate.IsValueType ||
            (typeof(System.Runtime.CompilerServices.RuntimeHelpers)
                .GetMethod(nameof(System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences))
                ?? throw new InvalidOperationException("The runtime cannot tell which types hold references."))
                .MakeGenericMethod(candidate).Invoke(null, null) is true);

    /// <summary>Every instance field of <paramref name="type"/> and its bases, in a stable order.</summary>
    [ReflectionAudit("bound-object paths name every instance field of the host's bound content by name")]
    private static FieldInfo[] Fields(Type type) => allFields.GetOrAdd(type, static owner =>
    {
        var fields = new List<FieldInfo>();
        for (Type? current = owner; current is not null; current = current.BaseType)
            fields.AddRange(current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .OrderBy(field => field.MetadataToken));
        return [.. fields];
    });

    /// <summary>The fields of <paramref name="type"/> that can hold a reference, each with the step that names it.</summary>
    private static (FieldInfo Field, string Step)[] ReferenceFields(Type type) => referenceFields.GetOrAdd(type, static owner =>
        [.. Fields(owner)
            .Where(field => !field.FieldType.IsPointer && !field.FieldType.IsByRef && HoldsReferences(field.FieldType))
            .Select(field => (field, DebuggerStateTypeIdentity.GetSerializedName(ReflectionBoundary.DeclaringType(field)) + FieldSeparator + field.Name))]);

    /// <summary>Every index tuple of <paramref name="array"/>, in row order.</summary>
    private static IEnumerable<int[]> ArrayIndices(Array array)
    {
        if (array.Length == 0)
            yield break;
        int[] indices = new int[array.Rank];
        for (int dimension = 0; dimension < array.Rank; dimension++)
            indices[dimension] = array.GetLowerBound(dimension);
        while (true)
        {
            yield return [.. indices];
            int dimension = array.Rank - 1;
            while (dimension >= 0)
            {
                indices[dimension]++;
                if (indices[dimension] <= array.GetUpperBound(dimension))
                    break;
                indices[dimension] = array.GetLowerBound(dimension);
                dimension--;
            }
            if (dimension < 0)
                yield break;
        }
    }

    /// <summary>
    /// Where an object was first reached: the object that holds it and the step from it. An element of an array of
    /// references keeps its index, whose step is built only when a capture names it (#1532).
    /// </summary>
    /// <param name="Parent">The holding object, or null for a root.</param>
    /// <param name="Step">The step from the parent, or the root's number; null for an array element named by its index.</param>
    /// <param name="Depth">The steps from a root it was reached in: zero for a root, one more than its parent's otherwise.</param>
    /// <param name="Element">The element's index in its array when <paramref name="Step"/> is null.</param>
    private readonly record struct Origin(object? Parent, string? Step, int Depth, int Element = 0)
    {
        /// <summary>The step from the parent, as a path names it.</summary>
        internal string Named => Step ?? ElementStep + Element.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }
}
