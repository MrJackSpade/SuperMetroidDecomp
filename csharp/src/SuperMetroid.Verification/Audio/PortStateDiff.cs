using SuperMetroid.Core;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Verification.Harness;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Verification.Audio;

/// <summary>
/// Comparison of two production object graphs through the members the compiler writes for every game type (#2602). The
/// snapshot-import audit uses it to find state a native snapshot import leaves different from the same moment reached by
/// replaying the movie, so missing import mappings are named rather than discovered as later desynchronizations.
/// </summary>
/// <remarks>
/// Two values are the same when their debugger-state graphs are (<see cref="GraphDigestVerification.SameGraph(object, object, object[])"/>),
/// both naming the installed content by path. A pair that differs is followed into the children each value hands a state's
/// walk (its fields that reach another object, its elements, its entries), and each differing pair none of whose children
/// differs is listed: its own values differ.
/// </remarks>
internal sealed class PortStateDiff
{
    /// <summary>Reference-paired graph nodes already compared, preventing cycles and repeated diagnostics.</summary>
    private readonly HashSet<PortStateDiffX> visited = new(PairComparer.Instance);
    /// <summary>Differences accumulated with their object-graph paths.</summary>
    private readonly List<string> differences = [];
    /// <summary>Caller-selected subtrees omitted from the snapshot-import audit.</summary>
    private readonly Func<string, bool> excludePath;

    /// <summary>Starts one graph comparison with the caller's subtree filter.</summary>
    private PortStateDiff(Func<string, bool> excludePath) => this.excludePath = excludePath;

    /// <summary>Lists every differing node between <paramref name="expected"/> and <paramref name="actual"/>.</summary>
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

    /// <summary>Compares one pair, following a differing pair into its children; returns whether the pair differs.</summary>
    private bool Visit(string path, object? expected, object? actual)
    {
        if (excludePath(path))
            return false;
        if (expected is null || actual is null)
        {
            if (expected is null && actual is null)
                return false;
            differences.Add($"{path}: expected {Describe(expected)}, actual {Describe(actual)}");
            return true;
        }
        Type type = expected.GetType();
        if (type != actual.GetType())
        {
            differences.Add($"{path}: expected type {type.Name}, actual {actual.GetType().Name}");
            return true;
        }
        // Shared immutable catalogs are the same instance in both graphs.
        if (!type.IsValueType && ReferenceEquals(expected, actual))
            return false;
        if (!type.IsValueType && !visited.Add(new PortStateDiffX(expected, actual)))
            return false;
        if (GraphDigestVerification.SameGraph(expected, actual))
            return false;
        var mine = new Children();
        var theirs = new Children();
        StateGraphTypes.Shared.For(expected).Visit(expected, mine);
        StateGraphTypes.Shared.For(actual).Visit(actual, theirs);
        bool childDiffers = false;
        if (mine.Items.Count != theirs.Items.Count)
        {
            differences.Add($"{path}: expected {mine.Items.Count} held values, actual {theirs.Items.Count}");
            return true;
        }
        for (int index = 0; index < mine.Items.Count; index++)
            childDiffers |= Visit($"{path}{mine.Items[index].Step}", mine.Items[index].Value, theirs.Items[index].Value);
        if (!childDiffers)
            differences.Add($"{path}: own values differ: expected {Describe(expected)}, actual {Describe(actual)}");
        return true;
    }

    /// <summary>Formats byte and word values in cartridge-style hexadecimal for readable differences.</summary>
    private static string Describe(object? value) => value switch
    {
        null => "null",
        ushort word => $"${word:X4}",
        byte octet => $"${octet:X2}",
        _ => value.ToString() ?? value.GetType().Name,
    };

    /// <summary>The values one object hands a state's walk, each with the step that names it.</summary>
    private sealed class Children : IStateGraphChildren
    {
        /// <summary>The values, in the walk's order.</summary>
        internal List<(string Step, object? Value)> Items { get; } = [];

        /// <inheritdoc/>
        public void Child(StateGraphField field, object? value) => Items.Add(($".{field.Name}", value));

        /// <inheritdoc/>
        public void Element(int index, object? value) => Items.Add(($"[{index}]", value));

        /// <inheritdoc/>
        public void Part(string step, object? value) => Items.Add(($".{step}", value));
    }

    /// <summary>Uses reference identity for both nodes of each visited graph pair.</summary>
    private sealed class PairComparer : IEqualityComparer<PortStateDiffX>
    {
        /// <summary>Shared comparer for detecting graph cycles by object identity.</summary>
        public static readonly PairComparer Instance = new();

        /// <summary>Matches two graph pairs only when both object references are the same.</summary>
        public bool Equals(PortStateDiffX x, PortStateDiffX y) =>
            ReferenceEquals(x.First, y.First) && ReferenceEquals(x.Second, y.Second);

        /// <summary>Combines identity hashes without invoking value equality on graph nodes.</summary>
        public int GetHashCode(PortStateDiffX pair) =>
            HashCode.Combine(RuntimeHelpers.GetHashCode(pair.First), RuntimeHelpers.GetHashCode(pair.Second));
    }
}
