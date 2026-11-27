using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Rinka: a ring of fire that swells out of its source, hangs a moment, and then flies straight at where Samus was,
/// until it has left the view. Its source makes another.
/// </summary>
/// <remarks>
/// It plays "life", whose cue "hide" keeps it unseen and harmless while it forms and "fire" brings it out to take aim.
/// </remarks>
public partial class Rinka : Enemy
{
    private const int QuarterTurn = 64, HalfTurn = 128;

    /// <summary>Whether it goes out with only a puff when killed, leaving nothing, as those in Mother Brain's room do.</summary>
    [Export]
    public bool Quiet { get; set; }

    /// <summary>How many updates it hangs before it flies.</summary>
    [Export, DataValue("aim_updates")]
    public int AimUpdates { get; set; } = 27;

    /// <summary>How fast it flies, as a number of pixels an update.</summary>
    [Export, DataValue("speed")]
    public double Speed { get; set; } = 1.125;

    /// <summary>How far past the edge of the view it may go before it is gone, in pixels.</summary>
    [Export, DataValue("view_margin")]
    public int ViewMargin { get; set; } = 16;

    /// <summary>The effect of a quiet one going out, by the data file of its timeline.</summary>
    [Export, DataValue("puff_effect")]
    public string PuffEffect { get; set; } = "";

    private enum Doing
    {
        Forming,
        Aiming,
        Flying,
    }

    private Doing _doing;
    private int _aim;
    private bool _hidden = true;

    /// <inheritdoc/>
    public override bool IsHitBy(HitSide side) => !_hidden && base.IsHitBy(side);

    /// <summary>Puts it out at once, with a puff.</summary>
    public void GoOut()
    {
        if (IsDead)
            return;
        // The puff is one of the enemies' shots on the cartridge: it holds one of the room's places while it plays, and
        // is not made with none free.
        if (!_hidden && PuffEffect.Length > 0)
            Room.Spawn(Effect.Of(FrameTimeline.Read(Room.Content.ReadData(PuffEffect)), Position, holdsPlace: true));
        Vanish();
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _doing = Doing.Forming;
        Hide(true);
        Play("life", restart: true);
    }

    /// <inheritdoc/>
    protected override bool DiesAtOnce(Hit hit)
    {
        if (!Quiet)
            return true;
        GoOut();
        return false;
    }

    /// <inheritdoc/>
    protected override void Touch(Collider part)
    {
        if (!_hidden)
            base.Touch(part);
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        switch (cue)
        {
            case "hide":
                Hide(true);
                break;
            case "fire":
                Hide(false);
                (_doing, _aim) = (Doing.Aiming, AimUpdates);
                break;
            default:
                throw new InvalidOperationException($"A Rinka's timeline gives it the cue \"{cue}\", which it does not know.");
        }
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        switch (_doing)
        {
            case Doing.Forming:
                break;

            case Doing.Aiming:
                if (--_aim > 0)
                    return;
                // As the cartridge aims: between whole pixels, the angle counting up from right, and the speed in
                // 256ths of a pixel split along it with what is finer dropped.
                int aim = CartridgeAngles.AimAngle(
                    Room.Player.Position.X.Whole - Position.X.Whole, Room.Player.Position.Y.Whole - Position.Y.Whole);
                Pixels speed = Pixels.FromNumber(Speed);
                int speedIn256ths = speed.Whole * 256 + speed.Fraction / 256;
                Velocity = new PixelVelocity(
                    FromTwoFiftySixths(CartridgeAngles.SpeedAlong(speedIn256ths, aim + QuarterTurn)),
                    FromTwoFiftySixths(CartridgeAngles.SpeedAlong(speedIn256ths, aim + HalfTurn)));
                _doing = Doing.Flying;
                return;

            case Doing.Flying:
                Position += Velocity.OverOneUpdate;
                break;

            default:
                throw new InvalidOperationException($"A Rinka has nothing to do called {_doing}.");
        }

        if (!IsInFlightWindow())
            Vanish();
    }

    /// <summary>
    /// Whether it is inside the window it flies in: the screen widened by its margin on every side, the far edges
    /// outside, and nothing above or left of the room.
    /// </summary>
    private bool IsInFlightWindow()
    {
        PixelRect screen = Room.Session.Camera.View;
        int x = Position.X.Whole, y = Position.Y.Whole;
        return x >= 0 && y >= 0 &&
            x >= screen.Left - ViewMargin && x < screen.Right + ViewMargin &&
            y >= screen.Top - ViewMargin && y < screen.Bottom + ViewMargin;
    }

    private void Hide(bool hidden)
    {
        _hidden = hidden;
        Visual?.SetShown(!hidden);
    }

    /// <summary>A distance given in 256ths of a pixel.</summary>
    private static Pixels FromTwoFiftySixths(int twoFiftySixths) => Pixels.FromParts(twoFiftySixths >> 8, (twoFiftySixths & 0xFF) << 8);
}
