using System.Runtime.CompilerServices;
using SuperMetroid.Core;

namespace SuperMetroid.TestSupport.FrameRendering;

/// <summary>
/// Whether two published object graphs hold the same values (#1532), walked side by side field by field. Two twin games
/// publish frames that share their installed content by reference and hold equal copies of everything else, so a walk
/// that stops at every shared reference compares them without drawing either and without serializing either graph. The
/// answer is conservative: <see langword="true"/> only when every value the two graphs reach is bit for bit the same,
/// which is what the frame's pixels are a function of; any difference, or a value the game's registration does not
/// describe, answers <see langword="false"/>, and a caller then draws both frames to compare their pixels.
/// Each object's fields are the ones the compiler wrote its debugger-state members for (#2602), read through the game's
/// generated registration (<see cref="GraphComparisons"/>); transient subscriptions are not among them, as a debugger
/// capture does not keep them.
/// </summary>
internal static class PublishedGraphs
{
    /// <summary>Whether <paramref name="left"/> and <paramref name="right"/> reach the same values through the same shape.</summary>
    internal static bool Same(object? left, object? right)
    {
        // Each pair of objects already compared, so a cycle closes on its pair. What a renderer shows is a function of the
        // values it reads, so an object shared in one graph and copied in the other compares by its values.
        var compared = new HashSet<(object Left, object Right)>(PairComparer.Instance);
        var walk = new GraphWalk();
        walk.Push(left, right);
        while (walk.TryPop(out object? mine, out object? theirs))
        {
            if (ReferenceEquals(mine, theirs))
                continue;
            if (mine is null || theirs is null)
                return false;
            Type type = mine.GetType();
            if (type != theirs.GetType())
                return false;
            if (!type.IsValueType && !compared.Add((mine, theirs)))
                continue;
            if (GraphComparisons.Shared.For(mine) is not { } compare || !compare(mine, theirs, walk))
                return false;
        }
        return true;
    }

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
}
