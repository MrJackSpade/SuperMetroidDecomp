using Engine.Actors;
using Engine.Collision;
using Engine.Shared;
using Engine.World;

namespace Game;

/// <summary>
/// One of Samus's bombs: it lies where it was laid until its time runs out, and then bursts. Its settings do all of
/// that. What is its own is the bomb jump, which throws the morph ball lying against it a little before it bursts;
/// what its burst does to the room's blocks, which is to strike the block it lies in and the four beside that one;
/// and being thrown, as the five of a spread are: a thrown bomb falls, bounces off the floor as high as it was
/// thrown, turns back from walls and stops rising at a ceiling.
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
    private Pixels _gravity;

    /// <summary>Throws the bomb at a speed, to fall at <paramref name="gravity"/> faster each update.</summary>
    public void Throw(PixelVelocity speed, Pixels gravity) => (_thrown, _flight, _throwDown, _gravity) = (true, speed, speed.Y, gravity);

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

        Velocity = PixelVelocity.Zero;
        _flight = _flight.WithY(_flight.Y + _gravity);
        PixelPosition before = Position;
        Position = new PixelPosition(Position.X, Position.Y + _flight.Y);
        if (room.Terrain.IsSolidIn(Area, SlopeTreatment.Wall))
        {
            // The floor throws it back up as hard as it was first thrown; a ceiling only stops its rise.
            Position = before;
            _flight = _flight.WithY(_flight.Y.IsNegative ? Pixels.Zero : _throwDown);
            return;
        }

        before = Position;
        Position = new PixelPosition(Position.X + _flight.X, Position.Y);
        if (!room.Terrain.IsSolidIn(Area, SlopeTreatment.Wall))
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
