namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// One ordered list of Torizo touch/shot rectangles, shared by every frame component that names it. A
/// left-facing list carries its right-facing counterpart, which is fitted separately to the mirrored
/// drawing rather than reflected; a list without one (the empty list, and lists only right-facing or
/// jump-back frames use directly) is its own counterpart.
/// </summary>
internal sealed class TorizoHitboxList
{
    /// <summary>The right-facing counterpart, or null when this list is its own.</summary>
    private readonly TorizoHitboxList? rightFacing;

    /// <summary>Creates a list of rectangles with an optional separately fitted right-facing counterpart.</summary>
    /// <param name="rectangles">Rectangles in test order; the first one a probe overlaps decides the reaction.</param>
    /// <param name="rightFacing">The list a mirrored, right-facing frame uses in place of this one.</param>
    internal TorizoHitboxList(GoldenTorizoCollisionHitbox[] rectangles, TorizoHitboxList? rightFacing = null)
    {
        Rectangles = Ensure.NotNull(rectangles);
        this.rightFacing = rightFacing;
    }

    /// <summary>Rectangles in test order; empty for a component without collision.</summary>
    internal IReadOnlyList<GoldenTorizoCollisionHitbox> Rectangles { get; }

    /// <summary>The list a mirrored, right-facing copy of a component holding this list uses.</summary>
    internal TorizoHitboxList RightFacing => rightFacing ?? this;
}
