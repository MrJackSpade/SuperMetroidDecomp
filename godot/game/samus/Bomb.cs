using Engine.Actors;
using Engine.Collision;
using Engine.Shared;
using Engine.World;

namespace Game;

/// <summary>
/// One of Samus's bombs: it lies where it was laid until its time runs out, and then bursts. Its settings do all of
/// that. What is its own is the bomb jump, a burst that catches the morph ball throwing it; and being thrown, as the
/// five of a spread are: a thrown bomb falls, bounces off the floor as high as it was thrown, turns back from walls
/// and stops rising at a ceiling.
/// </summary>
public partial class Bomb : Projectile
{
    private bool _thrown;
    private PixelVelocity _flight;
    private Pixels _throwDown;
    private Pixels _gravity;

    /// <summary>Throws the bomb at a speed, to fall at <paramref name="gravity"/> faster each update.</summary>
    public void Throw(PixelVelocity speed, Pixels gravity) => (_thrown, _flight, _throwDown, _gravity) = (true, speed, speed.Y, gravity);

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (!_thrown)
            return;
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
        if (Launcher is not Samus samus || Settings.EndingBurst.Length == 0)
            return;
        ProjectileSettings burst = ShotType.Read(room.Content.ReadData(Settings.EndingBurst)).Settings;
        var blast = PixelRect.Around(Position.X.Whole, Position.Y.Whole, burst.Width / 2, burst.Height / 2);
        if (blast.Overlaps(samus.BodyRect))
            samus.BombJump(Position);
    }
}
