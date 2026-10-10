using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

/// <summary>
/// Field-by-field comparison of two production object graphs. The snapshot-import audit uses
/// it to find state a native snapshot import leaves different from the same moment reached by
/// replaying the movie, so missing import mappings are named rather than discovered as later
/// desynchronizations.
/// </summary>
internal sealed class PortStateDiff
{
    private const BindingFlags InstanceFields =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private readonly HashSet<(object, object)> visited = new(PairComparer.Instance);
    private readonly List<string> differences = [];
    private readonly Func<string, bool> excludePath;

    private PortStateDiff(Func<string, bool> excludePath) => this.excludePath = excludePath;

    /// <summary>Lists every differing leaf between <paramref name="expected"/> and <paramref name="actual"/>.</summary>
    /// <param name="expected">The reference graph, reached by replaying the movie.</param>
    /// <param name="actual">The graph under audit, built by the snapshot import.</param>
    /// <param name="rootName">The path prefix naming both roots in the report.</param>
    /// <param name="excludePath">Returns true for member paths whose subtrees are not compared.</param>
    public static IReadOnlyList<string> Compare(object expected, object actual, string rootName, Func<string, bool> excludePath)
    {
        var diff = new PortStateDiff(excludePath);
        diff.Visit(rootName, expected, actual);
        return diff.differences;
    }

    private void Visit(string path, object? expected, object? actual)
    {
        if (excludePath(path))
            return;
        if (expected is null || actual is null)
        {
            if (expected is not null || actual is not null)
                differences.Add($"{path}: expected {Describe(expected)}, actual {Describe(actual)}");
            return;
        }
        Type type = expected.GetType();
        if (type != actual.GetType())
        {
            differences.Add($"{path}: expected type {type.Name}, actual {actual.GetType().Name}");
            return;
        }
        if (IsLeaf(type))
        {
            if (!expected.Equals(actual))
                differences.Add($"{path}: expected {Describe(expected)}, actual {Describe(actual)}");
            return;
        }
        if (expected is Delegate expectedDelegate)
        {
            var actualDelegate = (Delegate)actual;
            if (expectedDelegate.Method != actualDelegate.Method)
                differences.Add($"{path}: expected delegate {expectedDelegate.Method.Name}, actual {actualDelegate.Method.Name}");
            return;
        }
        // Shared immutable catalogs are the same instance in both graphs.
        if (!type.IsValueType && ReferenceEquals(expected, actual))
            return;
        if (!type.IsValueType && !visited.Add((expected, actual)))
            return;
        if (expected is Array expectedArray)
        {
            var actualArray = (Array)actual;
            if (expectedArray.Length != actualArray.Length)
            {
                differences.Add($"{path}: expected length {expectedArray.Length}, actual {actualArray.Length}");
                return;
            }
            Type element = type.GetElementType()!;
            if (element.IsPrimitive)
            {
                // Large primitive buffers (WRAM-like images) are summarized, not listed.
                int count = 0, first = -1;
                for (int index = 0; index < expectedArray.Length; index++)
                {
                    if (Equals(expectedArray.GetValue(index), actualArray.GetValue(index))) continue;
                    if (first < 0) first = index;
                    count++;
                }
                if (count != 0)
                    differences.Add($"{path}: {count} of {expectedArray.Length} elements differ, first [{first}] " +
                        $"expected {Describe(expectedArray.GetValue(first))}, actual {Describe(actualArray.GetValue(first))}");
                return;
            }
            for (int index = 0; index < expectedArray.Length; index++)
                Visit($"{path}[{index}]", expectedArray.GetValue(index), actualArray.GetValue(index));
            return;
        }
        if (expected is IList expectedList && type.IsGenericType)
        {
            var actualList = (IList)actual;
            if (expectedList.Count != actualList.Count)
                differences.Add($"{path}: expected count {expectedList.Count}, actual {actualList.Count}");
            for (int index = 0; index < Math.Min(expectedList.Count, actualList.Count); index++)
                Visit($"{path}[{index}]", expectedList[index], actualList[index]);
            return;
        }
        for (Type? owner = type; owner is not null && owner != typeof(object); owner = owner.BaseType)
            foreach (FieldInfo field in owner.GetFields(InstanceFields))
            {
                if (field.FieldType.IsPointer || field.FieldType.IsByRefLike) continue;
                Visit($"{path}.{MemberName(field)}", field.GetValue(expected), field.GetValue(actual));
            }
    }

    private static bool IsLeaf(Type type) =>
        type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal);

    private static string MemberName(FieldInfo field) =>
        field.Name.StartsWith('<') && field.Name.EndsWith(">k__BackingField", StringComparison.Ordinal)
            ? field.Name[1..field.Name.IndexOf('>')]
            : field.Name;

    private static string Describe(object? value) => value switch
    {
        null => "null",
        ushort word => $"${word:X4}",
        byte octet => $"${octet:X2}",
        _ => value.ToString() ?? value.GetType().Name,
    };

    private sealed class PairComparer : IEqualityComparer<(object, object)>
    {
        public static readonly PairComparer Instance = new();

        public bool Equals((object, object) x, (object, object) y) =>
            ReferenceEquals(x.Item1, y.Item1) && ReferenceEquals(x.Item2, y.Item2);

        public int GetHashCode((object, object) pair) =>
            HashCode.Combine(RuntimeHelpers.GetHashCode(pair.Item1), RuntimeHelpers.GetHashCode(pair.Item2));
    }
}
