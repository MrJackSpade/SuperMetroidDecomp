using Engine.Actors;
using Engine.Shared;
using Engine.World;

namespace Game;

/// <summary>
/// One of the two echoes of Samus thrown off as a shinespark ends. While she is held where she crashed they circle
/// her, out and round and back in, doing no harm; then they are let go, and fly straight out from her, hurting what
/// they pass through, until they have left the screen.
/// </summary>
public partial class ShinesparkEcho : Projectile
{
    private int _angle;
    private int _radius;
    private bool _flying;

    /// <summary>Puts the echo at an angle and distance from her, while it circles.</summary>
    public void Circle(Player samus, int angle, int radius)
    {
        ArgumentNullException.ThrowIfNull(samus);
        (_angle, _radius, _flying) = (angle, radius, false);
        Place(samus);
    }

    /// <summary>Lets the echo go, straight out from her at an angle.</summary>
    public void Fly(int angle) => (_angle, _radius, _flying) = (angle, 0, true);

    /// <inheritdoc/>
    protected override bool DeliversHitNow => _flying && base.DeliversHitNow;

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        Velocity = PixelVelocity.Zero;
        if (!_flying || Launcher is not Player samus)
            return;

        // It is always placed from where she is now: it is an echo of her, not a shot with a course of its own.
        _radius += 8;
        Place(samus);
        int x = Position.X.Whole - room.Session.Camera.Left, y = Position.Y.Whole - room.Session.Camera.Top;
        if (x < 0 || x >= 256 || y < 0 || y >= 256)
            Leave(room);
    }

    /// <summary>Takes the echo away, for a shinespark that ends some other way.</summary>
    public void Dismiss(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);
        if (!HasEnded)
            Leave(room);
    }

    private void Place(Player samus)
    {
        Pixels reach = Pixels.FromWhole(_radius);
        Position = PixelPosition.FromWhole(
            samus.Position.X.Whole + Turns.Sine(_angle, reach).Whole, samus.Position.Y.Whole - Turns.Cosine(_angle, reach).Whole);
    }
}
