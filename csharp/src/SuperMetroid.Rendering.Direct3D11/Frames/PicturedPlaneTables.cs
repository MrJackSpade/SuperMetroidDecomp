using System.Collections.Immutable;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Rendering.Direct3D11.Frames;

/// <summary>
/// The pictured planes a frame draws (#1467), each once; equal to another holding the same planes in the same order, so an
/// unchanged screen keeps its packing.
/// </summary>
/// <param name="planes">The planes.</param>
internal sealed class PicturedPlaneTables(ImmutableArray<PicturedPlane> planes) : IEquatable<PicturedPlaneTables>
{
    /// <summary>The planes.</summary>
    public ImmutableArray<PicturedPlane> Planes { get; } = planes;

    /// <inheritdoc />
    public bool Equals(PicturedPlaneTables? other) => other is not null && Planes.SequenceEqual(other.Planes);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as PicturedPlaneTables);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (PicturedPlane plane in Planes)
            hash.Add(plane);
        return hash.ToHashCode();
    }
}
