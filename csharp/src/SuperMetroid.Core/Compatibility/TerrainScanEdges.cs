namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Where a terrain scan's edges, span and alignment lie when the mover reaches past the room's top or left edge
/// (#2568): Samus's block collision, her beams' and Wave shots' block reactions, the enemies' terrain movers and the
/// enemy projectiles' block scans each form an edge as the mover's centre less or plus its radius, and an edge past the
/// room's top or left continues from the far end of the room's coordinates. Its block then lies beyond the room's last
/// row or column, where the room's grid reads solid terrain at its edge or the scan's cycles (<see cref="BlockScanCycle"/>)
/// name a block from the room's top; a span counted from an edge before the room to one inside it is counted within the
/// coordinates; and the place a mover is aligned to against a block met there, and the distance it still travels to
/// touch that block, are taken the same way. A room's first column and row are open in places, and a mover's body can
/// reach past them before its centre does.
/// </summary>
/// <remarks>
/// Consulted by every terrain scan as it forms its edges and span, before it looks up its first block, and by its
/// alignment once a block stops it, in the update of the mover that scans. Evidence: Samus's block collision
/// (<c>$94:9543</c>, <c>$94:959E</c>, <c>$94:95F5</c>), the beam moves (<c>$94:A23B</c>, <c>$94:A2CA</c>,
/// <c>$94:A352</c>, <c>$94:A3E4</c>), the enemy movers (<c>$A0:C6AD</c>, <c>$A0:C788</c>) and the enemy projectile block
/// reactions (<c>$86:8886</c>, <c>$86:889E</c>) add and subtract the radius to the centre as sixteen-bit words and take
/// the block from the word's high bits with no test of its sign; the Ridley full-movie and 100% recorded movies replay
/// those scans on every update.
/// </remarks>
[CompatibilityService("a terrain scan's edges, span and alignment past the room's top or left continue from the far end of the room's coordinates, so the scan reads the blocks there as beyond the room")]
public static partial class TerrainScanEdges
{
    /// <summary>
    /// The room pixel a scan's edge, span end or alignment lands on: one computed past the room's top or left continues
    /// from the far end of the room's coordinates, 0 through 65535.
    /// </summary>
    /// <param name="pixel">The pixel computed, which may lie before the room or past the coordinates' far end.</param>
    public static int Edge(int pixel) => CoordinateAxis.Wrap(pixel);

    /// <summary>
    /// The block row or column a scan steps to: one stepped before the room's first continues from the far end of the
    /// room's block coordinates, 0 through 65535, a block beyond the room's grid.
    /// </summary>
    /// <param name="block">The block row or column computed, which may lie before the room.</param>
    public static int Block(int block) => CoordinateAxis.Wrap(block);

    /// <summary>
    /// How far a mover at <paramref name="position"/> still travels to touch a block face it is aligned against at
    /// <paramref name="face"/>, -32768 through 32767 pixels, negative toward the room's top or left, taken the short way
    /// across the ends of the room's coordinates.
    /// </summary>
    /// <param name="face">The mover's place once aligned against the block, which may be computed past either end of the coordinates.</param>
    /// <param name="position">The mover's place before the alignment.</param>
    public static int Approach(int face, int position) => CoordinateAxis.Difference(face, position);
}
