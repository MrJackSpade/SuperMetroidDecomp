using System.Runtime.CompilerServices;
using SuperMetroid.Core.World;

namespace SuperMetroid.Core.Enemies.NoobTube;

/// <summary>
/// The glass tube's pieces as checks reach them (#2554): a piece the tube's room object spawned carries out its cues through
/// the pieces' behaviour it was bound to, which it holds; a piece a check places itself is bound, as the tube binds each of
/// its pieces, to one pieces' behaviour over its enemy system's room, shared by every piece the check places there.
/// </summary>
internal static class NoobTubePieceFixtures
{
    /// <summary>The pieces' behaviour each enemy system's checks bind the pieces they place to.</summary>
    private static readonly ConditionalWeakTable<RoomEnemySystem, NoobTubeBehavior.NoobTubeProjectileBehavior> placedPieces = new();

    /// <summary>The pieces' behaviour the pieces a check places in <paramref name="enemies"/> are bound to.</summary>
    internal static NoobTubeBehavior.NoobTubeProjectileBehavior PlacedPieces(RoomEnemySystem enemies) =>
        placedPieces.GetValue(Ensure.NotNull(enemies), system => new NoobTubeBehavior.NoobTubeProjectileBehavior(system.Room));

    /// <summary>A tube piece's own debris state, held by the pieces' behaviour it is bound to.</summary>
    extension(EnemyProjectile self)
    {
        /// <summary>The piece's column, sway and hiding, as the pieces' behaviour it is bound to holds them.</summary>
        /// <exception cref="InvalidOperationException">The projectile is no piece of the tube.</exception>
        internal NoobTubeDebrisState NoobTubeDebris =>
            (self.Behavior as NoobTubeBehavior.NoobTubeProjectileBehavior
                ?? throw new InvalidOperationException($"Enemy projectile {self.Place} is a {self.Kind}, no piece of the glass tube.")).Pieces.Of(self);
    }
}
