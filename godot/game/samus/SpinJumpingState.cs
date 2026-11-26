using Engine.Actors;
using Engine.Collision;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// A spin jump: the somersault a jump from a run is. It keeps its speed across only while its top speed caps it,
/// while she is reversing, or while the way she faces is held; a dash still carries her whatever is held. It can jump
/// off walls, jumps again in mid-air with the space jump, and hurts what it touches with the screw attack, or with a
/// fully charged beam. Aiming or firing straightens her out of it into a jump; steering the other way turns the spin.
/// </summary>
/// <remarks>
/// What the input asks of a spin is the first of these that holds, as the cartridge has it: the attack pressed afresh
/// straightens her out with her arm held out level, whatever else is held; the attack held with up, with down, with
/// the angle-up action or with the angle-down action straightens her out aiming up, down, diagonally up or diagonally
/// down; the jump and the way she faces held keep the spin; up, the angle-up action, the angle-down action or down
/// straightens her out aiming so; the way she faces keeps the spin; the other way turns it.
/// </remarks>
public sealed class SpinJumpingState : PlayerState, ISamusLook
{
    /// <summary>The name of the move values a spin jump is steered by.</summary>
    public const string Moves = "spin_jumping";

    /// <summary>The name, in each medium's speeds, of the least falling speed at which the space jump works.</summary>
    public const string SpaceJumpLeastFall = "space_jump_least_fall";

    /// <summary>The name, in each medium's speeds, of the falling speed at and above which the space jump no longer works.</summary>
    public const string SpaceJumpMostFall = "space_jump_most_fall";

    /// <summary>How a spin begins.</summary>
    public enum Start
    {
        /// <summary>From the ground: she is launched.</summary>
        FromGround,

        /// <summary>Out of a wall jump whose jump was let go: she keeps rising as she was.</summary>
        AfterWallJump,

        /// <summary>Out of a wall jump by steering the other way: she turns as the spin begins.</summary>
        Turning,

        /// <summary>
        /// With no jump begun: she is left adrift in the spin's smaller body, as a moonwalk's turn made with the jump
        /// held leaves her when it runs out.
        /// </summary>
        Adrift,
    }

    private readonly Start _start;

    /// <summary>Whether this is the spin a wall jump goes into: it began out of one.</summary>
    public bool IsOfWallJump => _start is Start.AfterWallJump or Start.Turning;

    /// <summary>Whether it shows the pictures in which it is ready to jump off a wall.</summary>
    public bool IsReadyForWall => _ready.IsRunning;

    /// <summary>Whether the spin has settled enough to look for a wall to jump from: see <see cref="Unasked"/>.</summary>
    private bool _looksForWalls;

    /// <summary>The updates left in which the spin is ready to jump off a wall.</summary>
    private Countdown _ready;

    /// <summary>Whether this update's input asked the spin to turn.</summary>
    private bool _turnAsked;

    /// <summary>A spin jump from the ground.</summary>
    public SpinJumpingState()
    {
    }

    /// <summary>A spin that begins as <paramref name="start"/> says.</summary>
    public SpinJumpingState(Start start) => _start = start;

    /// <summary>The kind of spin she is in, by its look: the screw attack's, the space jump's or the plain one.</summary>
    private string _spin = "spin_jumping";

    /// <summary>Whether this update is one on which the spin is ready to jump off a wall it has found.</summary>
    private bool _showsReady;

    /// <summary>
    /// The look of the spin she is in, or of that spin braced against a wall on the updates it is ready to jump off
    /// one: the picture a wall jump is waited for in.
    /// </summary>
    public string Look => _showsReady ? _spin + "_wall_ready" : _spin;

    /// <summary>
    /// The sound of the spin she shows, asked for again as a door's passage ends: chosen by her look alone, never the
    /// underwater one, as the cartridge picks it from her pose.
    /// </summary>
    public string ShownSound => _spin switch
    {
        "screw_attacking" => Sounds.ScrewAttack,
        "space_jumping" => Sounds.SpaceJump,
        _ => Sounds.SpinJump,
    };

    /// <inheritdoc/>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(owner.FormOr(Samus.Spinning), AroundMiddle: true);

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        bool swamped = owner.IsWhollyInLiquid;
        bool screw = !swamped && owner.Inventory.IsOn(Equipment.ScrewAttack), space = !swamped && owner.Inventory.IsOn(Equipment.SpaceJump);
        _spin = screw ? "screw_attacking" : space ? "space_jumping" : "spin_jumping";
        switch (_start)
        {
            case Start.FromGround:
                owner.Rise(owner.LaunchSpeed(offWall: false, owner.FormOr(Samus.Spinning)));
                owner.Room.Session.Sounds.Request(SpinSound(owner), channel: "samus");
                break;

            case Start.AfterWallJump:
                // Only the space jump's and the screw attack's spins keep what the wall jump gave her across.
                if (!screw && !space)
                    owner.StopAcross();
                else if (!screw)
                    owner.Room.Session.Sounds.Request(Sounds.SpaceJump, channel: "samus");
                _looksForWalls = true;
                break;

            case Start.Turning:
                Turn(owner);
                break;

            case Start.Adrift:
                // The cartridge takes up the spin's pose here without starting a jump: she has no way up or down.
                owner.Drift();
                owner.Room.Session.Sounds.Request(SpinSound(owner), channel: "samus");
                break;

            default:
                throw new InvalidOperationException($"Undefined start {_start} of a spin jump.");
        }
    }

    /// <summary>
    /// Straightens her out into a jump in <paramref name="stance"/>. The jump names the form of its stance, which is
    /// fitted before the jump is entered: with room for it she straightens out; stopped one way with no room the
    /// other she spins on as she was, as the cartridge leaves a spin that cannot grow into the pose asked for; and
    /// with no room above or below she is put in the crouch, as the cartridge puts her.
    /// </summary>
    private static PlayerState Straightened(AirStance stance) => new JumpingState(stance);

    /// <inheritdoc/>
    public override PlayerState? Asked(Player owner)
    {
        GameInput input = owner.Input;
        bool attack = input.IsHeld(GameAction.Attack), up = input.IsHeld(GameAction.Up), down = input.IsHeld(GameAction.Down);
        bool angleUp = input.IsHeld(Actions.AngleUp), angleDown = input.IsHeld(Actions.AngleDown);
        bool forward = input.IsHeld(owner.Facing == Facing.Right ? GameAction.Right : GameAction.Left);
        bool backward = input.IsHeld(owner.Facing == Facing.Right ? GameAction.Left : GameAction.Right);
        _turnAsked = false;

        // Straightened out, she is in a jump once this update is done.
        if (input.WasPressed(GameAction.Attack))
            return Straightened(new AirStance(Aim.Forward, AirArm.Out));
        if (attack && up)
            return Straightened(new AirStance(Aim.Up));
        if (attack && down)
            return Straightened(new AirStance(Aim.Down));
        if (attack && angleUp)
            return Straightened(new AirStance(Aim.DiagonalUp));
        if (attack && angleDown)
            return Straightened(new AirStance(Aim.DiagonalDown));
        if (forward && input.IsHeld(GameAction.Jump))
            return Spinning(owner);
        if (up)
            return Straightened(new AirStance(Aim.Up));
        if (angleUp)
            return Straightened(new AirStance(Aim.DiagonalUp));
        if (angleDown)
            return Straightened(new AirStance(Aim.DiagonalDown));
        if (down)
            return Straightened(new AirStance(Aim.Down));
        if (forward)
            return Spinning(owner);
        if (!backward)
        {
            owner.Aim = Aim.Forward;
            return null;
        }

        _turnAsked = true;
        return Spinning(owner);
    }

    /// <summary>She spins on, her arm at rest.</summary>
    private SpinJumpingState Spinning(Player owner)
    {
        owner.Aim = Aim.Forward;
        return this;
    }

    /// <summary>The spin turns once she has moved.</summary>
    public override void CarriedOn(Player owner)
    {
        if (_turnAsked)
            Turn(owner);
    }

    /// <summary>
    /// With nothing asked, the spin goes on as it is. A spin looks for a wall to jump from only once it has done so,
    /// or turned, or come out of a wall jump: one held steadily from the ground does not.
    /// </summary>
    public override PlayerState? Unasked(Player owner)
    {
        owner.Aim = Aim.Forward;
        _looksForWalls = true;
        return null;
    }

    /// <summary>Turns the spin: the dash is added into her speed, her momentum ends, and she reverses.</summary>
    private void Turn(Player owner)
    {
        owner.TurnAround();
        owner.EndMomentum();
        _looksForWalls = true;
        if (_spin == "space_jumping")
            owner.Room.Session.Sounds.Request(Sounds.SpaceJump, channel: "samus");
    }

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;

        // Wholly under a liquid that hinders her, the spin neither jumps again nor hurts anything.
        bool swamped = owner.IsWhollyInLiquid;
        if (!swamped)
        {
            TrySpaceJump(owner);
            if (owner.Inventory.IsOn(Equipment.ScrewAttack) && samus.Power(HitKinds.ScrewAttack) is { } screw)
                screw.IsActive = true;
            else if (owner.Weapons.ChargedFor >= samus.ChargedSpinUpdates && samus.Power(HitKinds.ChargedSpin) is { } charged)
                charged.IsActive = true;
        }

        owner.StepDash(building: false);
        JumpingState.CutShort(owner);

        // The spin keeps her own speed across only while its top speed capped it, while she reverses, or while the way
        // she faces is held; kept, it no longer needs steering.
        bool capped = owner.SteerInAir(owner.Moves(Moves));
        bool forward = owner.Input.IsHeld(owner.Facing == Facing.Right ? GameAction.Right : GameAction.Left);
        if (!capped && owner.Steering != Steering.Reversing && !forward)
            owner.ComeToRest();
        else if (owner.Steering == Steering.Steered)
            owner.Steering = Steering.Unsteered;
        owner.MoveAcross();

        if (JumpsOffWall(owner))
        {
            // The wall jump takes the place of this update's rise or fall.
            owner.Room.Session.Sounds.Request(Sounds.Land, channel: "samus");
            return new WallJumpingState();
        }

        return AirStance.FallAndLand(owner, this);
    }

    /// <summary>
    /// Whether she jumps off a wall this update. A wall within reach on the side away from the direction held makes
    /// the spin ready from the next update, for as long as its ready picture lasts; ready, a fresh press of the jump
    /// with the wall still there jumps off it.
    /// </summary>
    private bool JumpsOffWall(Player owner)
    {
        var samus = (Samus)owner;
        bool ready = _ready.IsRunning;
        _showsReady = ready;
        _ready = _ready.Stepped();
        if (!_looksForWalls || !owner.MayWallJump || FeelForWall(owner) is not { } gap)
            return false;

        // Ready, a fresh press of the jump launches her if the wall is nearer than her reach; something solid found
        // at her reach's very end is felt but not jumped from.
        if (ready)
            return owner.JumpPressed && gap < owner.WallJumpReach;
        _ready = Countdown.Of(samus.InMedium("wall_jump_ready_updates", samus.WallJumpReadyUpdates));
        return false;
    }

    /// <summary>
    /// Feels for a wall as the cartridge does: by trying a move of her reach to the side away from the direction
    /// held, to the right when left is held and otherwise to the left when right is. Returns the whole pixels
    /// between her and what would stop that move, or null when nothing would or no direction is held. The trying
    /// leaves her where she is but for her part of a pixel: terrain that would stop her leaves it across as a stop
    /// against it would, and a solid she lies exactly against takes it away up and down.
    /// </summary>
    private static int? FeelForWall(Player owner)
    {
        bool left = owner.Input.IsHeld(GameAction.Left);
        if (!left && !owner.Input.IsHeld(GameAction.Right))
            return null;
        Facing side = left ? Facing.Right : Facing.Left;
        MoveResult tried = owner.Room.Mover.TryAcross(
            owner.Body, side.Toward(Pixels.FromWhole(owner.WallJumpReach)), owner.BodyMoves, out PixelPosition rest);
        if (!tried.Blocked)
            return null;
        if (tried.BlockedByTerrain)
            owner.Position = owner.Position with { X = owner.Position.X.WithFraction(rest.X.Fraction) };
        else
            owner.Position = owner.Position with { Y = owner.Position.Y.WithFraction(rest.Y.Fraction) };
        return Math.Abs(tried.Moved.Whole);
    }

    /// <summary>
    /// Jumps again in mid-air when the space jump is on: on a fresh press of jump while she is falling, not rising and
    /// not adrift, and only while her falling speed is within the window of the medium she was last in.
    /// </summary>
    private static void TrySpaceJump(Player owner)
    {
        if (!owner.Inventory.IsOn(Equipment.SpaceJump) || !owner.JumpPressed || owner.IsRising || owner.IsAdrift)
            return;
        Pixels falling = owner.Velocity.Y;
        // The least speed is the medium's she was in as her last update ended, as the cartridge remembers it.
        if (falling < ((Samus)owner).MediumLastEntered.Speed(SpaceJumpLeastFall) || falling >= owner.Medium.Speed(SpaceJumpMostFall))
            return;
        owner.Rise(owner.LaunchSpeed(offWall: false, owner.Form));
        owner.Room.Session.Sounds.Request(SpinSound(owner), channel: "samus");
    }

    /// <summary>The sound of the spin she is in.</summary>
    private static string SpinSound(Player owner) =>
        owner.IsWhollyInLiquid ? Sounds.UnderwaterSpinJump
        : owner.Inventory.IsOn(Equipment.ScrewAttack) ? Sounds.ScrewAttack
        : owner.Inventory.IsOn(Equipment.SpaceJump) ? Sounds.SpaceJump
        : Sounds.SpinJump;
}
