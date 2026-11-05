using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SuperMetroid.Core;

namespace SuperMetroid.TestSupport.FrameRendering;

/// <summary>
/// Whether two published object graphs hold the same values (#1532), walked side by side field by field. Two twin games
/// publish frames that share their installed content by reference and hold equal copies of everything else, so a walk
/// that stops at every shared reference compares them without drawing either and without serializing either graph. The
/// answer is conservative: <see langword="true"/> only when every value the two graphs reach is bit for bit the same,
/// which is what the frame's pixels are a function of; any difference answers <see langword="false"/>, and a caller then
/// draws both frames to compare their pixels.
/// Fields marked <see cref="NonSerializedAttribute"/> hold transient subscriptions, not state, and are not compared, as
/// a debugger capture does not keep them.
/// </summary>
[ReflectionAudit("structural equality: two published graphs are compared field by field, so equal graphs need not be drawn")]
internal static class PublishedGraphs
{
    /// <summary>Whether <paramref name="left"/> and <paramref name="right"/> reach the same values through the same shape.</summary>
    internal static bool Same(object? left, object? right)
    {
        // Each pair of objects already compared, so a cycle closes on its pair. What a renderer shows is a function of the
        // values it reads, so an object shared in one graph and copied in the other compares by its values.
        var compared = new HashSet<(object Left, object Right)>(PairComparer.Instance);
        var pending = new Stack<(object? Left, object? Right)>();
        pending.Push((left, right));
        while (pending.Count > 0)
        {
            (object? mine, object? theirs) = pending.Pop();
            if (ReferenceEquals(mine, theirs))
                continue;
            if (mine is null || theirs is null)
                return false;
            Type type = mine.GetType();
            if (type != theirs.GetType())
                return false;
            if (!type.IsValueType && !compared.Add((mine, theirs)))
                continue;
            if (!Compare(type, mine, theirs, pending))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Compares two values of <paramref name="type"/> that hold no further graph, or queues the pairs of values they hold.
    /// </summary>
    private static bool Compare(Type type, object mine, object theirs, Stack<(object? Left, object? Right)> pending)
    {
        switch (KindOf(type))
        {
            case GraphKind.Bits:
                return BitsComparer(type)(mine, theirs);
            case GraphKind.Text:
                return string.Equals((string)mine, (string)theirs, StringComparison.Ordinal);
            case GraphKind.Identity:
                return mine.Equals(theirs);
            case GraphKind.Delegate:
                Delegate[] myCalls = ((Delegate)mine).GetInvocationList(), theirCalls = ((Delegate)theirs).GetInvocationList();
                if (myCalls.Length != theirCalls.Length)
                    return false;
                for (int index = 0; index < myCalls.Length; index++)
                {
                    if (myCalls[index].Method != theirCalls[index].Method)
                        return false;
                    pending.Push((myCalls[index].Target, theirCalls[index].Target));
                }
                return true;
            case GraphKind.Array:
                var myArray = (Array)mine;
                var theirArray = (Array)theirs;
                if (myArray.Rank != 1 || myArray.Length != theirArray.Length ||
                    myArray.GetLowerBound(0) != 0 || theirArray.GetLowerBound(0) != 0)
                    return false;
                Type element = type.GetElementType() ?? throw new InvalidOperationException($"Array {type} has no element type.");
                if (!HoldsReferences(element))
                    return BitsArrayComparer(element)(myArray, theirArray);
                for (int index = 0; index < myArray.Length; index++)
                    pending.Push((myArray.GetValue(index), theirArray.GetValue(index)));
                return true;
            case GraphKind.Fields:
                foreach (FieldInfo field in FieldsOf(type))
                    pending.Push((field.GetValue(mine), field.GetValue(theirs)));
                return true;
            default:
                throw new InvalidOperationException($"Type {type} has no graph comparison.");
        }
    }

    /// <summary>How values of one type are compared.</summary>
    private enum GraphKind
    {
        /// <summary>A value type holding no reference: compared bit for bit.</summary>
        Bits,
        /// <summary>A string: compared by its characters.</summary>
        Text,
        /// <summary>Reflection metadata or a pointer: compared by its own equality.</summary>
        Identity,
        /// <summary>A delegate: the same methods, called on targets compared as graphs.</summary>
        Delegate,
        /// <summary>An array: its elements, bit for bit when they hold no reference.</summary>
        Array,
        /// <summary>Any other object or value: every instance field.</summary>
        Fields,
    }

    /// <summary>The comparison values of <paramref name="type"/> need.</summary>
    private static GraphKind KindOf(Type type) => kinds.GetOrAdd(type, static candidate =>
        candidate == typeof(string) ? GraphKind.Text
        : candidate.IsArray ? GraphKind.Array
        : typeof(Delegate).IsAssignableFrom(candidate) ? GraphKind.Delegate
        : typeof(MemberInfo).IsAssignableFrom(candidate) || candidate == typeof(Pointer) ? GraphKind.Identity
        : candidate.IsValueType && !HoldsReferences(candidate) ? GraphKind.Bits
        : GraphKind.Fields);

    /// <summary>Whether values of <paramref name="type"/> can hold a reference; a reference type always can.</summary>
    private static bool HoldsReferences(Type type) => holdsReferences.GetOrAdd(type, static candidate =>
        !candidate.IsValueType || Generic(nameof(ContainsReferences), candidate).Invoke(null, null) is true);

    /// <summary>Whether <typeparamref name="T"/> holds a reference.</summary>
    private static bool ContainsReferences<T>() => RuntimeHelpers.IsReferenceOrContainsReferences<T>();

    /// <summary>The bit-for-bit comparison of two boxed values of a value type that holds no reference.</summary>
    private static Func<object, object, bool> BitsComparer(Type type) => bitsComparers.GetOrAdd(type, static candidate =>
        Generic(nameof(SameBits), candidate).CreateDelegate<Func<object, object, bool>>());

    /// <summary>The bit-for-bit comparison of two equal-length arrays of a value type that holds no reference.</summary>
    private static Func<Array, Array, bool> BitsArrayComparer(Type element) => bitsArrayComparers.GetOrAdd(element, static candidate =>
        Generic(nameof(SameArrayBits), candidate).CreateDelegate<Func<Array, Array, bool>>());

    /// <summary>Whether two boxed <typeparamref name="T"/> values hold the same bits.</summary>
    private static bool SameBits<T>(object mine, object theirs) where T : struct
    {
        T left = (T)mine, right = (T)theirs;
        return MemoryMarshal.AsBytes(new ReadOnlySpan<T>(ref left)).SequenceEqual(MemoryMarshal.AsBytes(new ReadOnlySpan<T>(ref right)));
    }

    /// <summary>Whether two arrays of <typeparamref name="T"/> hold the same bits.</summary>
    private static bool SameArrayBits<T>(Array mine, Array theirs) where T : struct =>
        MemoryMarshal.AsBytes(((T[])mine).AsSpan()).SequenceEqual(MemoryMarshal.AsBytes(((T[])theirs).AsSpan()));

    /// <summary>This class's generic method <paramref name="name"/> made for <paramref name="type"/>.</summary>
    private static MethodInfo Generic(string name, Type type) =>
        (typeof(PublishedGraphs).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"{name} is missing.")).MakeGenericMethod(type);

    /// <summary>Every instance field of <paramref name="type"/> and its bases that holds state.</summary>
    private static FieldInfo[] FieldsOf(Type type) => fields.GetOrAdd(type, static owner =>
    {
        var found = new List<FieldInfo>();
        for (Type? current = owner; current is not null; current = current.BaseType)
            found.AddRange(current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(field => !field.IsDefined(typeof(NonSerializedAttribute), false)));
        return [.. found];
    });

    /// <summary>Compares pairs of objects by the identity of both.</summary>
    private sealed class PairComparer : IEqualityComparer<(object Left, object Right)>
    {
        /// <summary>The one comparer.</summary>
        internal static PairComparer Instance { get; } = new();

        /// <summary>Whether both pairs hold the same two objects.</summary>
        public bool Equals((object Left, object Right) x, (object Left, object Right) y) =>
            ReferenceEquals(x.Left, y.Left) && ReferenceEquals(x.Right, y.Right);

        /// <summary>Combines both objects' identity hashes.</summary>
        public int GetHashCode((object Left, object Right) obj) =>
            HashCode.Combine(RuntimeHelpers.GetHashCode(obj.Left), RuntimeHelpers.GetHashCode(obj.Right));
    }

    /// <summary>Each type's comparison kind.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, GraphKind> kinds = new();

    /// <summary>Whether each type's values can hold a reference.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, bool> holdsReferences = new();

    /// <summary>Each value type's bit comparison.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Func<object, object, bool>> bitsComparers = new();

    /// <summary>Each element type's array bit comparison.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Func<Array, Array, bool>> bitsArrayComparers = new();

    /// <summary>Each type's compared fields.</summary>
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, FieldInfo[]> fields = new();
}
