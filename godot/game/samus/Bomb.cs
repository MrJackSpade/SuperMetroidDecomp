using Engine.Actors;
using Engine.Collision;
using Engine.Shared;
using Engine.World;

namespace Game;

/// <summary>
/// One of Samus's bombs: it lies where it was laid until its time runs out, and then bursts. Its settings do all of
/// that. What is its own is the bomb jump, which throws the morph ball lying against it a little before it bursts;
/// what its burst does to the room's blocks, which is to strike the block it lies in and the four beside that one;
/// and being thrown, as the five of a spread are: a thrown bomb falls by the gravity Samus has as it flies, bounces
/// off the floor as high as it was thrown, turns back from walls and stops rising at a ceiling. It meets the room by
/// its middle point alone, as the cartridge's does: the room's edge, a solid tile or block, sloped ground that holds
/// by the middle at or under its surface, and any other slope's whole tile.
/// </summary>
public partial class Bomb : Projectile
{
    /// <summary>The blocks a burst strikes, from the block the bomb lies in, in the order it strikes them.</summary>
    private static readonly (int Across, int Down)[] Cross = [(0, 0), (0, -1), (1, 0), (-1, 0), (0, 1)];

    /// <summary>A bomb strikes no shot of the enemies'.</summary>
    protected override bool MeetsShots => false;

    private bool _thrown;
    private PixelVelocity _flight;
    private Pixels _throwDown;

    /// <summary>Throws the bomb at a speed, to fall faster each update by the gravity of whoever threw it.</summary>
    public void Throw(PixelVelocity speed) => (_thrown, _flight, _throwDown) = (true, speed, speed.Y);

    /// <summary>Whether the room stops a thrown bomb where it lies: what its middle point is in.</summary>
    private bool IsStopped(Room room) =>
        room.Terrain.IsSolidIn(new PixelRect(Position.X.Whole, Position.Y.Whole, 1, 1), SlopeTreatment.WholeUnlessMiddleHeld);

    /// <summary>
    /// Throws the morph ball of the Samus who laid <paramref name="bomb"/> if this is the update for it: the bomb has
    /// <paramref name="fuseLeft"/> updates of its fuse left, and that is the count at which her bombs throw her, and
    /// she lies against it.
    /// </summary>
    internal static void ThrowBall(Projectile bomb, int fuseLeft)
    {
        if (bomb.Launcher is Samus samus && fuseLeft == samus.BombJumpFuseLeft && bomb.Area.Overlaps(samus.BodyRect))
            samus.BombJump(bomb.Position);
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (!_thrown)
        {
            // A bomb of a spread never throws her.
            ThrowBall(this, Settings.LastsUpdates - Age);
            return;
        }

        // It falls by the gravity Samus has now, wherever she has got to since she threw it.
        Samus samus = Launcher as Samus ?? throw new InvalidOperationException("A thrown bomb is no bomb of Samus's.");
        Velocity = PixelVelocity.Zero;
        _flight = _flight.WithY(_flight.Y + Pixels.FromNumber(samus.Medium.Gravity));
        PixelPosition before = Position;
        Position = new PixelPosition(Position.X, Position.Y + _flight.Y);
        if (IsStopped(room))
        {
            // The floor throws it back up as hard as it was first thrown. A ceiling stops its rise, and what its
            // speed held of a part of a pixel is left it as a slow fall. Either way it goes no further this update.
            Position = before;
            _flight = _flight.WithY(_flight.Y.IsNegative ? Pixels.FromParts(0, _flight.Y.Fraction) : _throwDown);
            return;
        }

        before = Position;
        Position = new PixelPosition(Position.X + _flight.X, Position.Y);
        if (!IsStopped(room))
            return;
        Position = before;
        _flight = _flight.WithX(-_flight.X);
    }

    /// <inheritdoc/>
    protected override void OnEnded(Room room)
    {
        // The burst strikes whole blocks: the one the bomb lies in, then those above it, right, left and below.
        int size = room.Terrain.TileSize;
        int column = Math.DivRem(Position.X.Whole, size, out int across), row = Math.DivRem(Position.Y.Whole, size, out int down);
        if (across < 0)
            column--;
        if (down < 0)
            row--;
        foreach ((int byColumn, int byRow) in Cross)
            room.Terrain.TakeHit(Hit, new PixelRect((column + byColumn) * size, (row + byRow) * size, size, size));
    }
}
