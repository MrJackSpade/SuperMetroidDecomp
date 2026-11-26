using Engine.Actors;
using Engine.Input;
using Engine.Shared;

namespace Engine.Tests.Core;

/// <summary>
/// The stand-in game's player: the plainest answer to each thing the engine's player leaves to a game. Gravity is
/// capped at the medium's fall limit; the aim is up, down in the air, or a diagonal with a direction held; a hurt
/// knocks them back while they are in control; a wall leaves a runner standing; ongoing damage is kept in hundredths
/// until they make a point; a shot fired the way they are moving has their speed across added to its own; a hit
/// takes the energy it says; and the state they are about to be in is the one they are in next.
/// </summary>
public partial class StandInPlayer : Player
{
    private int _ongoingHundredths;

    /// <inheritdoc/>
    protected override Pixels AfterGravity(Pixels speed, PlayerMedium medium)
    {
        Pixels limit = Pixels.FromNumber(medium.FallSpeedLimit);
        Pixels faster = speed + Pixels.FromNumber(medium.Gravity);
        return faster > limit ? limit : faster;
    }

    /// <inheritdoc/>
    public override void AimFromInput()
    {
        bool up = Input.IsHeld(GameAction.Up), down = Input.IsHeld(GameAction.Down) && !IsOnGround;
        Aim = up ? (Intent != 0 ? Aim.DiagonalUp : Aim.Up) : down ? (Intent != 0 ? Aim.DiagonalDown : Aim.Down) : Aim.Forward;
    }

    /// <inheritdoc/>
    protected override void OnHurt(Hit hit, Facing from)
    {
        if (State.InControl)
            KnockBack(from);
    }

    /// <inheritdoc/>
    protected internal override PlayerState StoppedByWall() => new StandingState();

    /// <inheritdoc/>
    protected override PlayerState Entering(PlayerState from, PlayerState to, bool unasked) => to;

    /// <inheritdoc/>
    protected override int DamageFrom(Hit hit) => hit.Damage;

    /// <inheritdoc/>
    public override bool MovedAgainstWall => MovedAs is RunningState;

    /// <inheritdoc/>
    protected override int OngoingPointsTaken(int hundredths)
    {
        _ongoingHundredths += hundredths;
        int points = _ongoingHundredths / 100;
        _ongoingHundredths %= 100;
        return points;
    }

    /// <inheritdoc/>
    public override PixelVelocity ShotVelocity(WeaponStats weapon, PixelVelocity own, int across, int down) =>
        across != 0 && !SpeedAcross.IsZero && SpeedAcross.IsNegative == across < 0 ? own.WithX(own.X + SpeedAcross) : own;
}
