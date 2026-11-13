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

    /// <summary>The bound roots, by the name a path's first step gives each.</summary>
    private readonly Dictionary<string, object> roots = new(StringComparer.Ordinal);

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

    /// <summary>Indexes every object reachable from <paramref name="roots"/>, each root named by its number in the list.</summary>
    /// <param name="roots">The objects the host bound into the session.</param>
    internal BoundObjectGraph(IReadOnlyList<object> roots)
        : this(Ensure.NotNull(roots).Select((root, index) =>
            (index.ToString(System.Globalization.CultureInfo.InvariantCulture), Ensure.NotNull(root))).ToArray())
    {
    }

    /// <summary>
    /// Indexes every object reachable from <paramref name="roots"/>, breadth first in the list's order, each root named by
    /// its name, which a path's first step gives.
    /// </summary>
    /// <param name="roots">The bound objects, each with a distinct name that holds no step separator.</param>
    /// <exception cref="ArgumentException">Two roots share a name, or a name holds a step separator.</exception>
    internal BoundObjectGraph(IReadOnlyList<(string Name, object Root)> roots)
    {
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
    /// Indexes what the content loaded since the last refresh: the values of lazily loaded parts loaded since. An object
    /// keeps the path it was first indexed under, so a path never changes once a capture has used it.
    /// </summary>
    internal void Refresh()
    {
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
            if (!loaded.GetType().IsValueType && origins.TryAdd(loaded, new Origin(lazy, LazyValueStep)))
                pending.Enqueue(loaded);
        }
        Index(pending);
    }

    /// <summary>
    /// Indexes everything reachable from <paramref name="pending"/> that is not indexed yet, breadth first, so each object's
    /// path is its shortest from a root.
    /// </summary>
    private void Index(Queue<object> pending)
    {
        while (pending.Count > 0)
        {
            object current = pending.Dequeue();
            if (IsLazy(current.GetType()) && LazyValue(current, force: false) is null)
                unloaded.Add(current);
            if (VersionReader(current.GetType()) is { } versionReader && trackedCollections.Add(current))
                collections.Add(new GrowingCollection(current, versionReader));
            IndexChildren(current, pending);
        }
    }

    /// <summary>
    /// Indexes every object <paramref name="value"/> holds that is not indexed yet, as <see cref="Children"/> names them
    /// (#1532): a reference held by a field or an array element is reached directly, its step built only when a capture
    /// names it, and only a value type walked through goes through <see cref="Reached"/>.
    /// </summary>
    private void IndexChildren(object value, Queue<object> pending)
    {
        Type type = value.GetType();
        if (type == typeof(string) || value is Delegate || value is Type || value is MemberInfo || value is Pointer)
            return;
        if (IsLazy(type))
        {
            if (LazyValue(value, force: false) is { } loaded && !loaded.GetType().IsValueType)
                Reach(loaded, new Origin(value, LazyValueStep), pending);
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
                        ReachThrough(item, value, ElementStep + index.ToString(System.Globalization.CultureInfo.InvariantCulture), pending);
                    else
                        Reach(item, new Origin(value, Step: null, index), pending);
                }
                return;
            }
            if (type.IsSZArray && ElementReferences(elementType) is { } elementHeld)
            {
                // A vector of values is read element by element through the references each element holds, without
                // boxing an element or building a step until a capture names what it reaches (#1532).
                object?[] values = HeldValues(elementHeld.Steps.Length);
                for (int index = 0; index < array.Length; index++)
                {
                    elementHeld.Read(array, index, values);
                    for (int leaf = 0; leaf < elementHeld.Steps.Length; leaf++)
                        ReachHeld(values[leaf], value, elementHeld.Steps[leaf], index, pending);
                }
                Array.Clear(values, 0, elementHeld.Steps.Length);
                return;
            }
            foreach ((object child, string step) in Children(value))
                Reach(child, new Origin(value, step), pending);
            return;
        }
        if (!type.IsValueType && ObjectReferences(type) is { } held)
        {
            // An object holding values that hold references reads those references through the values, without boxing a
            // value, each named by its whole step from the object, built once for its type (#1532).
            object?[] values = HeldValues(held.Steps.Length);
            held.Read(value, values);
            for (int leaf = 0; leaf < held.Steps.Length; leaf++)
                ReachHeld(values[leaf], value, held.Steps[leaf], element: -1, pending);
            Array.Clear(values, 0, held.Steps.Length);
            return;
        }
        foreach ((FieldInfo field, string step) in ReferenceFields(type))
        {
            if (field.GetValue(value) is not { } fieldValue)
                continue;
            if (fieldValue.GetType().IsValueType)
                ReachThrough(fieldValue, value, step, pending);
            else
                Reach(fieldValue, new Origin(value, step), pending);
        }
    }

    /// <summary>
    /// Indexes the value <paramref name="parent"/> holds at <paramref name="step"/>, within its element
    /// <paramref name="element"/> when that is not negative: a reference is reached itself, and a value held where a
    /// reference could be (a boxed value) is walked through to the references it holds.
    /// </summary>
    private void ReachHeld(object? held, object parent, string step, int element, Queue<object> pending)
    {
        if (held is null)
            return;
        if (held.GetType().IsValueType)
            ReachThrough(held, parent, element < 0
                ? step
                : ElementStep + element.ToString(System.Globalization.CultureInfo.InvariantCulture) + StepSeparator + step, pending);
        else
            Reach(held, new Origin(parent, step, element), pending);
    }

    /// <summary>This graph's buffer for the references one object or element holds, with room for at least <paramref name="count"/>.</summary>
    private object?[] HeldValues(int count)
    {
        if (heldValues.Length < count)
            heldValues = new object?[count];
        return heldValues;
    }

    /// <summary>The references the object or element being indexed holds, read into one buffer the walk reuses.</summary>
    private object?[] heldValues = [];

    /// <summary>Indexes the references the value <paramref name="held"/> holds, each named through its step from <paramref name="parent"/>.</summary>
    private void ReachThrough(object held, object parent, string step, Queue<object> pending)
    {
        foreach ((object child, string childStep) in Reached(held, step))
            Reach(child, new Origin(parent, childStep), pending);
    }

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
    /// Whether <paramref name="value"/> is a bound object, with its path and its depth: how many steps from a root the
    /// walk first reached it, a root's being zero. Indexing is breadth first, so no path to it from a root is shorter.
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

    /// <summary>
    /// Each path a restore has resolved, and each path leading to one, with the object it names: a path is resolved from
    /// the object its parent path names, so paths sharing a prefix step through it once (#1532).
    /// </summary>
    private readonly Dictionary<string, object> resolved = new(StringComparer.Ordinal);

    /// <summary>
    /// The object <paramref name="path"/> names, <paramref name="named"/> or a path leading to it: its root, or the step
    /// from the object its parent path names.
    /// </summary>
    /// <param name="path">The path resolved.</param>
    /// <param name="named">The path the restore asked for, which a failure names.</param>
    /// <exception cref="InvalidDataException">The path names no object the host binds now.</exception>
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
        return FieldSteps(current.GetType()).TryGetValue(step, out FieldInfo? field) ? field.GetValue(current) : null;
    }

    /// <summary>Each type's fields that can hold a reference, by the step that names each.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Dictionary<string, FieldInfo>> fieldSteps = new();

    /// <summary>The fields of <paramref name="type"/> that can hold a reference, by the step that names each, looked up once per step.</summary>
    private static Dictionary<string, FieldInfo> FieldSteps(Type type) => fieldSteps.GetOrAdd(type, static owner =>
        ReferenceFields(owner).ToDictionary(entry => entry.Step, entry => entry.Field, StringComparer.Ordinal));

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
    /// Where an object was first reached: the object that holds it and the step from it. An element of an array keeps its
    /// index, whose step is built only when a capture names it (#1532).
    /// </summary>
    /// <param name="Parent">The holding object, or null for a root.</param>
    /// <param name="Step">
    /// The step from the parent, or the root's number; within element <paramref name="Element"/> when that is not
    /// negative, and null for the element itself.
    /// </param>
    /// <param name="Element">The element's index in its array, or negative when the step is not within an element.</param>
    private readonly record struct Origin(object? Parent, string? Step, int Element = -1)
    {
        /// <summary>The step from the parent, as a path names it.</summary>
        internal string Named => Element < 0
            ? Step ?? throw new InvalidOperationException("A bound object's origin names neither a step nor an element.")
            : Step is null
                ? ElementStep + Element.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : ElementStep + Element.ToString(System.Globalization.CultureInfo.InvariantCulture) + StepSeparator + Step;
    }

    /// <summary>
    /// The references values of one type hold, read in the order <see cref="Children"/> walks them: each field that can hold
    /// a reference, a field holding a value that holds references walked through to that value's own (#1532). Each is
    /// named by its whole step from the value; a reference read where a value could be held (a boxed value) is walked
    /// through when indexed.
    /// </summary>
    /// <typeparam name="TRead">The reader's delegate type.</typeparam>
    /// <param name="Steps">Each held reference's step from the value, in walk order.</param>
    /// <param name="Read">Reads a value's held references into a buffer, in walk order.</param>
    private sealed record HeldReferences<TRead>(string[] Steps, TRead Read) where TRead : Delegate;

    /// <summary>Each reference type's held references, or null for a type none of whose fields holds a value holding references.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, HeldReferences<Action<object, object?[]>>?> objectReferences = new();

    /// <summary>Each value element type's held references, or null for a nullable element type.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, HeldReferences<Action<Array, int, object?[]>>?> elementReferences = new();

    /// <summary>
    /// The references an object of <paramref name="type"/> holds through its fields, or null when no field holds a value
    /// that holds references, whose fields the walk reads directly.
    /// </summary>
    private static HeldReferences<Action<object, object?[]>>? ObjectReferences(Type type) => objectReferences.GetOrAdd(type, static owner =>
        ReferenceFields(owner).Any(entry => ThroughValue(entry.Field.FieldType)) ? CompileObjectReferences(owner) : null);

    /// <summary>The references an element of a vector of <paramref name="elementType"/> holds, or null for a nullable element.</summary>
    private static HeldReferences<Action<Array, int, object?[]>>? ElementReferences(Type elementType) => elementReferences.GetOrAdd(elementType,
        static candidate => Nullable.GetUnderlyingType(candidate) is null ? CompileElementReferences(candidate) : null);

    /// <summary>
    /// Whether a field of <paramref name="fieldType"/> holds a value walked through to its own references: a value type
    /// that can hold one. A nullable value is read as the value it boxes to, so it is walked through when indexed.
    /// </summary>
    private static bool ThroughValue(Type fieldType) =>
        fieldType.IsValueType && Nullable.GetUnderlyingType(fieldType) is null && HoldsReferences(fieldType);

    /// <summary>Compiles the reader of the references an object of the reference type <paramref name="type"/> holds.</summary>
    [ReflectionAudit("the bound-object index reads the references an object holds through its values without boxing them")]
    private static HeldReferences<Action<object, object?[]>> CompileObjectReferences(Type type)
    {
        var value = System.Linq.Expressions.Expression.Parameter(typeof(object), "value");
        var buffer = System.Linq.Expressions.Expression.Parameter(typeof(object[]), "buffer");
        var held = System.Linq.Expressions.Expression.Variable(type, "held");
        var steps = new List<string>();
        var body = new List<System.Linq.Expressions.Expression>
        {
            System.Linq.Expressions.Expression.Assign(held, System.Linq.Expressions.Expression.Convert(value, type)),
        };
        FlattenHeld(held, prefix: null, buffer, steps, body);
        return new([.. steps], System.Linq.Expressions.Expression.Lambda<Action<object, object?[]>>(
            System.Linq.Expressions.Expression.Block([held], body), value, buffer).Compile());
    }

    /// <summary>Compiles the reader of the references an element of a vector of the value type <paramref name="type"/> holds.</summary>
    [ReflectionAudit("the bound-object index reads the references an array element holds without boxing the element")]
    private static HeldReferences<Action<Array, int, object?[]>> CompileElementReferences(Type type)
    {
        var array = System.Linq.Expressions.Expression.Parameter(typeof(Array), "array");
        var index = System.Linq.Expressions.Expression.Parameter(typeof(int), "index");
        var buffer = System.Linq.Expressions.Expression.Parameter(typeof(object[]), "buffer");
        var held = System.Linq.Expressions.Expression.Variable(type, "held");
        var steps = new List<string>();
        var body = new List<System.Linq.Expressions.Expression>
        {
            System.Linq.Expressions.Expression.Assign(held, System.Linq.Expressions.Expression.ArrayIndex(
                System.Linq.Expressions.Expression.Convert(array, type.MakeArrayType()), index)),
        };
        FlattenHeld(held, prefix: null, buffer, steps, body);
        return new([.. steps], System.Linq.Expressions.Expression.Lambda<Action<Array, int, object?[]>>(
            System.Linq.Expressions.Expression.Block([held], body), array, index, buffer).Compile());
    }

    /// <summary>
    /// Adds to <paramref name="body"/> a read into <paramref name="buffer"/> of each reference <paramref name="owner"/>
    /// holds, in the order <see cref="Children"/> walks them, and each one's step to <paramref name="steps"/>: a field
    /// holding a value that holds references is walked through to that value's fields.
    /// </summary>
    [ReflectionAudit("the bound-object index reads the references a value holds through its fields without boxing the value")]
    private static void FlattenHeld(System.Linq.Expressions.Expression owner, string? prefix, System.Linq.Expressions.ParameterExpression buffer,
        List<string> steps, List<System.Linq.Expressions.Expression> body)
    {
        foreach ((FieldInfo field, string step) in ReferenceFields(owner.Type))
        {
            string named = prefix is null ? step : prefix + StepSeparator + step;
            System.Linq.Expressions.Expression read = System.Linq.Expressions.Expression.Field(owner, field);
            if (ThroughValue(field.FieldType))
            {
                FlattenHeld(read, named, buffer, steps, body);
                continue;
            }
            body.Add(System.Linq.Expressions.Expression.Assign(
                System.Linq.Expressions.Expression.ArrayAccess(buffer, System.Linq.Expressions.Expression.Constant(steps.Count)),
                System.Linq.Expressions.Expression.Convert(read, typeof(object))));
            steps.Add(named);
        }
    }
}
