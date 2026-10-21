using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The request each sound library reports back as acknowledged. The game's sound handlers compare it with the request
/// they are waiting on, so it decides when a handler moves on to its next request (<see cref="SoundAcceptanceClock"/>).
/// </summary>
[CompatibilityService("the request each sound library reports back as acknowledged")]
public readonly record struct SoundAcknowledgements
{
    /// <summary>Each library's acknowledged request, 0 through 255, indexed by zero-based library index.</summary>
    private readonly PerLibrary requests;

    /// <summary>Records each library's acknowledged request, validated to the range a request can carry.</summary>
    /// <param name="library1">Library one's acknowledged request.</param>
    /// <param name="library2">Library two's acknowledged request.</param>
    /// <param name="library3">Library three's acknowledged request.</param>
    /// <exception cref="ArgumentOutOfRangeException">A request is outside 0 through 255.</exception>
    public SoundAcknowledgements(int library1, int library2, int library3)
    {
        int[] values = [library1, library2, library3];
        for (int library = 0; library < SoundAcceptanceClock.LibraryCount; library++)
            requests[library] = Ensure.BetweenInclusive(0, byte.MaxValue, values[library]);
    }

    /// <summary>The acknowledged request of the library at zero-based <paramref name="library"/>.</summary>
    /// <exception cref="IndexOutOfRangeException"><paramref name="library"/> is outside 0 through 2.</exception>
    public int this[int library] => requests[library];

    /// <summary>One request per sound library; inline arrays have no built-in value equality, so it compares element by element.</summary>
    [InlineArray(SoundAcceptanceClock.LibraryCount)]
    private struct PerLibrary : IEquatable<PerLibrary>
    {
        /// <summary>Library one's request; the other libraries' follow it.</summary>
        private int first;

        /// <summary>Two request sets are equal when every library's request matches.</summary>
        public readonly bool Equals(PerLibrary other)
        {
            for (int library = 0; library < SoundAcceptanceClock.LibraryCount; library++)
                if (this[library] != other[library]) return false;
            return true;
        }

        /// <summary>Compares with another boxed request set element by element.</summary>
        public override readonly bool Equals(object? obj) => obj is PerLibrary other && Equals(other);

        /// <summary>Combines every library's request in index order.</summary>
        public override readonly int GetHashCode()
        {
            var hash = new HashCode();
            for (int library = 0; library < SoundAcceptanceClock.LibraryCount; library++) hash.Add(this[library]);
            return hash.ToHashCode();
        }
    }
}
