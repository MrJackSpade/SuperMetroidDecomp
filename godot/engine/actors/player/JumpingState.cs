using Engine.Input;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// A jump, from its start until the player lands: it keeps the jump's own move values through its rise and its fall.
/// A jump from the ground may first gather itself, moving nothing. Letting go of the jump while rising cuts the rise
/// short; steering against the way the player faces turns them in the air.
/// </summary>
/// <remarks>
/// A jump holds a stance from its start. A jump from the ground starts in the stance of the aim the player has as it
/// begins, their arm at rest, and holds it for as long as it gathers itself, the input asked nothing; as the gathering
/// ends the game's player is told (<see cref="Player.JumpGathered"/>). A throw that carries the player meanwhile moves
/// them in the jump's place, and the gathering goes on counting and ends as it would. A jump that carries on from something else
/// starts in the stance it is given, or failing that the one the input asks for as it begins. A change of stance takes
/// effect when the body can take the form of the new one, and the jump then says so for that update
/// (<see cref="TookUpStance"/>).
/// </remarks>
public sealed class JumpingState : PlayerState
{
    /// <summary>Whether the jump keeps the upward speed the player already has, as one carried on from a wall jump does.</summary>
    private readonly bool _keepsSpeed;

    /// <summary>The updates left in which a jump from the ground gathers itself, and whether the gathering ended on this update.</summary>
    private PictureCount _gathering;
    private bool _gathered;

    /// <summary>Whether the player had any speed of their own across as this update's input was asked.</summary>
    private bool _hadSpeed;

    /// <summary>The stance the player holds in this jump, once it has one, and the one this update's input asks for.</summary>
    private AirStance? _stance;
    private AirStance _stanceAsked;

    /// <summary>A jump from the ground.</summary>
    public JumpingState()
    {
    }

    /// <summary>A jump that carries on with the upward speed the player already has, in the stance the input asks for as it begins.</summary>
    public JumpingState(bool keepsSpeed) => _keepsSpeed = keepsSpeed;

    /// <summary>A jump that carries on with the upward speed the player already has, in <paramref name="stance"/>.</summary>
    public JumpingState(AirStance stance)
    {
        _keepsSpeed = true;
        _stance = stance;
    }

    /// <summary>Whether this is a jump from the ground.</summary>
    public bool FromGround => !_keepsSpeed;

    /// <summary>The stance the player holds in this jump.</summary>
    /// <exception cref="InvalidOperationException">The jump was given no stance and has not begun.</exception>
    public AirStance Stance => _stance ?? throw new InvalidOperationException("The jump has no stance before it begins.");

    /// <summary>How the arm is held in the stance the jump is in: at rest before it has one.</summary>
    public AirArm Arm => _stance?.Arm ?? AirArm.AtRest;

    /// <summary>Whether the player took up another stance within the jump on this update.</summary>
    public bool TookUpStance { get; private set; }

    /// <summary>
    /// Whether the jump is on an update of its gathering before the last: what is held on those can end the
    /// gathering, and a game whose player listens for more there says so as it is asked (<see cref="Player.ChooseState"/>).
    /// </summary>
    public bool GatheringListens => _gathering.IsRunning && !_gathering.IsOnLastUpdate;

    /// <summary>
    /// A jump from the ground is entered in the jump's full form; one that carries on, in the form of the aim its
    /// stance has, or the player's own aim when it was given no stance.
    /// </summary>
    public override FormAsked? EntersIn(Player owner)
    {
        if (!_keepsSpeed)
            return new FormAsked(owner.FormOr(Player.Jumping), AroundMiddle: true);
        Aim aim = _stance is { } given ? given.Aim : owner.Aim;
        string form = aim == Aim.Down && owner.Forms.ContainsKey(Player.AimingDown) ? Player.AimingDown : owner.FormOr(Player.Jumping);
        return new FormAsked(form, AroundMiddle: true);
    }

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        if (_keepsSpeed)
        {
            if (_stance is { } given)
                owner.Aim = given.Aim;
            else
                _stance = AirStance.Of(owner, forwardCounts: true);
            _stanceAsked = Stance;
            owner.SteerAfresh();
            return;
        }

        _stance = new AirStance(owner.Aim);
        _stanceAsked = Stance;
        owner.Rise(owner.LaunchSpeed(offWall: false, AirStance.FormFor(owner, Player.Jumping)));
        _gathering = PictureCount.Begin(owner, owner.JumpStartPictures);
    }

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        TookUpStance = false;
        _hadSpeed = !owner.Velocity.X.IsZero;

        // A jump that is gathering itself holds the stance it began in. Where the gathering is longer than an update,
        // as in a liquid, what is held on its updates before the last can end it: the jump with the way behind turns
        // the player in the air; the jump with an aim or the way ahead, or the attack, has them rise at once.
        if (_gathering.IsRunning)
        {
            GameInput input = owner.Input;
            bool right = owner.Facing == Facing.Right;
            bool jump = input.IsHeld(GameAction.Jump);
            bool ahead = input.IsHeld(right ? GameAction.Right : GameAction.Left), behind = input.IsHeld(right ? GameAction.Left : GameAction.Right);
            bool listens = GatheringListens;
            if (listens && jump && behind)
                return new AirTurningState(fromJump: true);
            if (!listens || !((jump && (owner.AimIsHeld || ahead)) || input.IsHeld(GameAction.Attack)))
            {
                _stanceAsked = Stance;
                return this;
            }

            _gathering = PictureCount.Over;
            owner.SteerAfresh();
            _gathered = true;
        }

        // A turn is made aiming as the player aimed before it: what is held with the way behind aims nothing.
        if (AirTurningState.IsAskedFor(owner))
            return new AirTurningState(fromJump: true);
        owner.AimFromInput();
        _stanceAsked = Stance.Kept(AirStance.Of(owner, forwardCounts: true));

        // The jump held asks for the jump to go on in every stance but the arm held out, which only the attack, an
        // aim or a direction keeps asked: with the jump alone held it is asked nothing.
        return AirStance.AsksNothing(owner, jumpCounts: Stance.Arm != AirArm.Out) ? null : this;
    }

    /// <summary>
    /// Whether this update's movement was the jump's gathering. A gathering is not interrupted, its last update
    /// included: what would interrupt the player waits for the jump to have risen once.
    /// </summary>
    private bool _movedGathering;

    /// <inheritdoc/>
    public override bool HoldsOffInterruptions => _movedGathering;

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        owner.StepDash(building: false);
        _movedGathering = _gathering.IsRunning;
        if (_gathering.IsRunning)
        {
            // Gathering, the player moves nothing of their own: a dash still carries them, and nothing else.
            owner.Steering = Steering.Steered;
            owner.MoveAcross(byDashAlone: true);
            CountGathering(owner);
            return null;
        }

        CutShort(owner);
        owner.SteerAndMoveInAir(owner.Moves(JumpingMoves));
        return AirStance.FallAndLand(owner, this);
    }

    /// <summary>A throw carries the player in place of the jump's own movement, and its gathering goes on counting.</summary>
    public override void WhileThrown(Player owner)
    {
        if (_gathering.IsRunning)
            CountGathering(owner);
    }

    /// <summary>Counts one update of the gathering, and has it over once its pictures have been held.</summary>
    private void CountGathering(Player owner)
    {
        _gathering = _gathering.Stepped(owner);
        if (!_gathering.IsOver)
            return;
        owner.SteerAfresh();
        _gathered = true;
    }

    /// <summary>As the gathering ends the game's player is told, and may be in another state instead of rising.</summary>
    public override PlayerState? Ended(Player owner)
    {
        if (!_gathered)
            return null;
        _gathered = false;
        return owner.AfterGathering(this);
    }

    /// <summary>
    /// The body takes the form of the stance asked for, where it fits; a new stance is taken up only when it does, and
    /// sets the steering afresh. Where it does not fit the aim goes back to the stance's own.
    /// </summary>
    public override void CarriedOn(Player owner)
    {
        bool fits = owner.TrySetForm(AirStance.FormFor(owner, Player.Jumping), aroundMiddle: true);
        if (!fits)
        {
            // With no room for the form of the stance asked for, the player keeps the stance they have, and its aim.
            owner.Aim = Stance.Aim;
            return;
        }

        if (_stanceAsked == Stance)
            return;
        owner.SteerAfresh();
        _stance = _stanceAsked;
        TookUpStance = true;
    }

    /// <summary>
    /// With nothing at all held, or with the jump alone held by a jumper whose arm is held out, a jumper who has
    /// speed across becomes unsteered: the dash is added into that speed
    /// and lost, and the momentum ends. One with no speed is steered. A jump aiming up, or diagonally either way,
    /// lets its aim go for the stance of steering the way the player faces, so that the same aim asked again is a
    /// stance taken up anew; one aiming straight down, or with the arm held out, stays as it is.
    /// </summary>
    public override PlayerState? Unasked(Player owner)
    {
        if (_stance is { Aim: Aim.Up or Aim.DiagonalUp or Aim.DiagonalDown })
        {
            owner.Aim = Aim.Forward;
            _stance = new AirStance(Aim.Forward, AirArm.Forward);
            owner.TrySetForm(AirStance.FormFor(owner, Player.Jumping), aroundMiddle: true);
        }

        if (_hadSpeed && !owner.Velocity.X.IsZero)
        {
            owner.FoldDash();
            owner.Steering = Steering.Unsteered;
        }
        else
        {
            owner.Steering = Steering.Steered;
        }

        owner.EndMomentum();
        return null;
    }

    /// <summary>
    /// Ends a rise at its top, the update after gravity has slowed it past rest, and cuts a rising jump short once the
    /// jump action is no longer held: to the cut speed, or, where that is none, to a fall from rest.
    /// </summary>
    public static void CutShort(Player owner)
    {
        if (!owner.IsRising)
            return;
        if (owner.TurnAtTop() || owner.Input.IsHeld(GameAction.Jump))
            return;
        Pixels cut = -Pixels.FromNumber(owner.JumpCutSpeed);
        if (cut.IsZero)
            owner.FallFromRest();
        else if (owner.Velocity.Y < cut)
            owner.Velocity = owner.Velocity.WithY(cut);
    }

    /// <summary>
    /// Left and right held together are read as the way behind, and the player turns in the air; unless they aim as
    /// well, by up or down, or by an aiming action of the game's own with the jump still held, which reads them as the
    /// way ahead: the player aims on a slant the way they face.
    /// </summary>
    public override int ReadsBothAcross(Player owner)
    {
        GameInput input = owner.Input;
        bool upOrDown = input.IsHeld(GameAction.Up) || input.IsHeld(GameAction.Down);
        return upOrDown || (owner.AimIsHeld && input.IsHeld(GameAction.Jump)) ? Forward(owner) : -Forward(owner);
    }
}
