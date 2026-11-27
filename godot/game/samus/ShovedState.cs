using Engine.Actors;
using Engine.Shared;

namespace Game;

/// <summary>
/// Samus thrown aside by Ridley as he leaves Ceres, in the knockback's look: she is carried toward the nearer side of
/// the screen as a knockback carries her, falling fast, until a wall stops her, and is then set down as a knockback
/// sets her down. Only her movement is taken: she is still in control, and chooses what she has selected as ever.
/// </summary>
public sealed class ShovedState : PlayerState, ISamusLook
{
    /// <summary>How fast she falls, in pixels each update.</summary>
    private const int FallSpeed = 5;

    private enum Stage
    {
        Starting,
        Pushing,
        SettingDown,
    }

    private Stage _stage = Stage.Starting;
    private Facing _toward;

    /// <summary>How far up taking the knockback's form moved her middle, to keep her feet where they were.</summary>
    private int _lift;

    /// <summary>Whether this update's movement was the first that carried her.</summary>
    private bool _firstPush;

    /// <summary>Whether this update's movement was the shove's first, which only sets it up.</summary>
    private bool _started;

    /// <inheritdoc/>
    public string Look => "knocked_back";

    /// <summary>She cannot attack, as the cartridge's knockback pose she is shoved in does not shoot.</summary>
    public override bool CanAttack => false;

    /// <summary>A knockback waits through the shove, as the cartridge's knockback pose she is shoved in refuses one.</summary>
    public override bool HoldsOffInterruptions => true;

    /// <summary>
    /// The knockback's form, the upright one, taken with no feeling for room, as the cartridge sets the knockback pose
    /// outright; her middle is then moved so her feet stay where they were.
    /// </summary>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(Player.Standing, Unfitted: true);

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        // Her body keeps the shape it had until her next update, so it still says how tall she was.
        _lift = owner.HalfHeightOf(Player.Standing) - owner.Body.Rect.Height / 2;
        owner.Position += PixelOffset.FromWhole(0, -_lift);
    }

    /// <summary>Her input asks nothing of the shove: the knockback pose's one way out, the damage boost, is refused while it lasts.</summary>
    public override PlayerState? Asked(Player owner) => null;

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        _started = _stage == Stage.Starting;
        switch (_stage)
        {
            case Stage.Starting:
            {
                // The first update only chooses the side, by where she is on the screen, and starts her falling.
                int onScreen = owner.Position.X.Whole - owner.Room.Session.Camera.Left;
                _toward = onScreen < owner.Room.Session.Camera.View.Width / 2 ? Facing.Left : Facing.Right;
                owner.Velocity = new PixelVelocity(owner.Velocity.X, Pixels.FromWhole(FallSpeed));
                (_stage, _firstPush) = (Stage.Pushing, true);
                return null;
            }

            case Stage.Pushing:
            {
                // Carried across as a knockback carries her, toward the shove's side and not the way she faces: her
                // speed across goes on changing by the knockback's move values, and a dash she kept carries her on top.
                bool first = _firstPush;
                _firstPush = false;
                owner.Steer(owner.Moves(KnockedBackMoves));
                owner.Velocity = owner.Velocity.WithX(_toward.Toward(owner.Velocity.X.Abs()));
                if (owner.MoveAcross().Blocked)
                {
                    // Met on the first update that carries her, the wall lines her feet up with the form she had
                    // before the shove once more, as the cartridge's does by her pose before the knockback's.
                    if (first && _lift != 0)
                    {
                        owner.Position += PixelOffset.FromWhole(0, -_lift);
                        owner.Room.Session.Camera.ShiftWherePlayerStoodDown(-_lift);
                    }

                    owner.Velocity = owner.Velocity.WithY(Pixels.Zero);
                    _stage = Stage.SettingDown;
                    return null;
                }

                owner.Velocity = owner.Velocity.WithY(Pixels.FromWhole(FallSpeed));
                owner.MoveDown();
                return null;
            }

            case Stage.SettingDown:
                // Stopped, she is still in the knockback's pose, which sets her down as a knockback that has run out
                // does: pressed down once, and standing if that met ground, or falling.
                if (!owner.MoveDownBy(owner.SpeedAcross.Abs() + Pixels.FromWhole(1)).Blocked)
                    return new FallingState(offFloor: true);
                owner.Land(Pixels.Zero);
                return new StandingState();

            default:
                throw new InvalidOperationException($"The shove has no stage {_stage}.");
        }
    }

    /// <summary>
    /// After each update but the first she is steered again and her momentum ends, a dash's speed left, as the
    /// cartridge's knockback pose does when nothing is asked of it.
    /// </summary>
    public override PlayerState? Unasked(Player owner)
    {
        if (!_started)
        {
            owner.Steering = Steering.Steered;
            owner.EndMomentum();
        }

        return null;
    }
}
