using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World.Objects;
using Godot;

namespace Game.Objects;

/// <summary>
/// One of the three Etecoons that show Samus the wall jump. It sleeps until she comes level with it, flexes, hops,
/// and when she is near runs to the shaft and climbs it, kicking off one wall and then the other. At the top it
/// hops in place to show how it is done; by its route, it may hop on for ever, run off on a long leap across the
/// gap when Samus comes close, or come back down and climb again.
/// </summary>
/// <remarks>
/// Its timelines are "starting_crouch", "flexing", "hopping_left", "hopping_right", "continue_hopping_left",
/// "continue_hopping_right", "hop_from_crest", "look_right_then_run_left", "look_left_then_run_right",
/// "begin_running_left", "begin_running_right", "running_left", "running_right", "jumping_right",
/// "clinging_to_right_wall", "clinging_to_left_wall", "wall_jump_left" and "wall_jump_right".
/// </remarks>
public partial class Etecoon : Creature
{
    /// <summary>What an Etecoon does once it has climbed the shaft.</summary>
    public enum Routes
    {
        /// <summary>It runs left to where the lesson is given, hops four times, and goes back down to climb again.</summary>
        RunLeftToTeachingStart,

        /// <summary>It runs right to where the lesson is given, hops four times, and goes back down to climb again.</summary>
        RunRightToTeachingStart,

        /// <summary>It hops where it lands for as long as it is left to, and takes the long leap when Samus comes close.</summary>
        TeachAtOnce,
    }

    /// <summary>What it does once it has climbed the shaft.</summary>
    [Export]
    public Routes Route { get; set; }

    /// <summary>Whether it cries as it wakes.</summary>
    [Export]
    public bool CriesOnWaking { get; set; }

    /// <summary>How far across the room, in pixels, the first lesson is given.</summary>
    [Export]
    public int FirstTeachingX { get; set; } = 537;

    /// <summary>Where its runs to the right start their leaps.</summary>
    [Export]
    public int RunStartX { get; set; } = 600;

    /// <summary>The middle of its long leap.</summary>
    [Export]
    public int LongLeapMiddleX { get; set; } = 680;

    /// <summary>The end of its long leap.</summary>
    [Export]
    public int LongLeapEndX { get; set; } = 840;

    /// <summary>The furthest across it takes the long leap from.</summary>
    [Export]
    public int LongRunFromBefore { get; set; } = 832;

    /// <summary>Samus hears it only when she is at least this far across the room.</summary>
    [Export]
    public int HeardFromSamusX { get; set; } = 256;

    /// <summary>How fast things fall here, in pixels an update each update.</summary>
    [Export]
    public double Gravity { get; set; } = 0.109375;

    /// <summary>Its cry as it wakes, by name.</summary>
    [Export, DataValue("wake_sound")]
    public string WakeSound { get; set; } = "";

    /// <summary>The sound of it hopping.</summary>
    [Export, DataValue("jump_sound")]
    public string JumpSound { get; set; } = "";

    /// <summary>The sound of it kicking off a wall.</summary>
    [Export, DataValue("wall_sound")]
    public string WallSound { get; set; } = "";

    private enum Doing
    {
        Dormant,
        Flexing,
        PreJumpCountdown,
        InitialJump,
        FaceSamusPause,
        RunLeftToWall,
        RunRightToWall,
        Airborne,
        WallPause,
        RouteAfterLanding,
        MoveLeftToTeachingStart,
        MoveRightToTeachingStart,
        RunRightToLongJump,
        LongJump,
        RunRightAfterLongJump,
        LongJumpLanding,
        TeachingJump,
        IdleBetweenTeachingJumps,
        WaitForSamusBeforeLongRun,
        RunRightToReturnJump,
        ReturnJump,
    }

    private static readonly Pixels Hop = Pixels.FromWhole(-3);
    private static readonly Pixels RunRight = Pixels.FromWhole(2);
    private static readonly Pixels RunLeft = Pixels.FromWhole(-2);
    private static readonly Pixels FastestFall = Pixels.FromWhole(5);

    /// <summary>How long it crouches before a hop, and how near counts as near, in updates and pixels.</summary>
    private const int HopCrouch = 11, Near = 64;

    private Doing _doing;
    private int _timer;
    private int _quakePause;
    private int _teachingHops;
    private bool _headsRight;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        (_doing, _timer, _quakePause, _teachingHops, _headsRight) = (Doing.Dormant, 0, 0, 0, false);
        Velocity = PixelVelocity.Zero;
        Play("starting_crouch");
    }

    /// <inheritdoc/>
    protected override void OnStep()
    {
        // Through a quake's pause it does nothing, and the picture it was showing as the pause began stays.
        if (_quakePause > 0)
        {
            _quakePause--;
            return;
        }

        base.OnStep();
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Player samus = Room.Player;
        switch (_doing)
        {
            case Doing.Dormant:
                if (!WithinDown(samus, 128))
                    break;
                if (CriesOnWaking)
                    Sound(WakeSound);
                Play("flexing", restart: true);
                (_doing, _timer) = (Doing.Flexing, 256);
                break;

            case Doing.Flexing:
                if (--_timer > 0)
                    break;
                Play("hopping_left", restart: true);
                (_doing, _timer) = (Doing.PreJumpCountdown, HopCrouch);
                break;

            case Doing.PreJumpCountdown:
                if (--_timer > 0)
                    break;
                Velocity = Velocity.WithY(Hop);
                Play("continue_hopping_left", restart: true);
                _doing = Doing.InitialJump;
                JumpCry(samus);
                break;

            case Doing.InitialJump:
                if (!MoveDown())
                    break;
                if (Velocity.Y.IsNegative)
                {
                    Crest();
                    break;
                }

                if (WithinDown(samus, Near) && WithinAcross(samus, Near))
                {
                    // It looks at her, and then runs the other way.
                    bool samusLeft = SamusIsToItsLeft(samus);
                    Play(samusLeft ? "look_left_then_run_right" : "look_right_then_run_left", restart: true);
                    Head(right: samusLeft);
                    (_doing, _timer) = (Doing.FaceSamusPause, 32);
                }
                else
                {
                    (_doing, _timer) = (Doing.PreJumpCountdown, HopCrouch);
                    Play("hopping_left", restart: true);
                }

                break;

            case Doing.FaceSamusPause:
                if (--_timer > 0)
                    break;
                Play(_headsRight ? "begin_running_right" : "begin_running_left", restart: true);
                Velocity = new PixelVelocity(_headsRight ? RunRight : RunLeft, Hop);
                _doing = _headsRight ? Doing.RunRightToWall : Doing.RunLeftToWall;
                break;

            case Doing.RunLeftToWall:
                if (!MoveAcross())
                    break;
                Velocity = Velocity.WithX(RunRight);
                _doing = Doing.RunRightToWall;
                Play("running_right", restart: true);
                Head(right: true);
                break;

            case Doing.RunRightToWall:
                // With the shaft's wall just ahead it leaps at it.
                if (Room.Terrain.IsSolidIn(MovingBody.RoomRect.MovedBy(32, 0), SlopeTreatment.Wall))
                {
                    Play("jumping_right", restart: true);
                    _doing = Doing.Airborne;
                    break;
                }

                MoveAcross();
                break;

            case Doing.Airborne:
                PauseForQuake();
                if (MoveAcross())
                {
                    // It clings to the wall a moment before kicking off it.
                    Play(_headsRight ? "clinging_to_right_wall" : "clinging_to_left_wall", restart: true);
                    Head(right: !_headsRight);
                    (_doing, _timer) = (Doing.WallPause, 8);
                    if (samus.Position.X.Whole >= HeardFromSamusX)
                        Sound(WallSound);
                    break;
                }

                if (!MoveDown())
                    break;
                Play(_headsRight ? "hopping_left" : "hopping_right", restart: true);
                (_doing, _timer) = (Doing.RouteAfterLanding, HopCrouch);
                Velocity = Velocity.WithY(Hop);
                break;

            case Doing.WallPause:
                PauseForQuake();
                if (--_timer > 0)
                    break;
                Play(_headsRight ? "wall_jump_right" : "wall_jump_left", restart: true);
                Velocity = new PixelVelocity(_headsRight ? RunRight : RunLeft, Hop);
                _doing = Doing.Airborne;
                break;

            case Doing.RouteAfterLanding:
                if (--_timer > 0)
                    break;
                Velocity = Velocity.WithY(Hop);
                TakeRoute();
                break;

            case Doing.MoveLeftToTeachingStart:
                MoveAcross();
                if (Position.X.Whole < FirstTeachingX)
                    BeginLessonPause();
                break;

            case Doing.MoveRightToTeachingStart:
                MoveAcross();
                if (Position.X.Whole >= RunStartX)
                    BeginLessonPause();
                break;

            case Doing.RunRightToLongJump:
                MoveAcross();
                if (Position.X.Whole < RunStartX)
                    break;
                _doing = Doing.LongJump;
                Velocity = Velocity.WithY(Pixels.FromWhole(-4));
                Play("jumping_right", restart: true);
                break;

            case Doing.LongJump:
                MoveAcross();
                MoveDown();
                if (Position.X.Whole < LongLeapMiddleX)
                    break;
                Play("running_right", restart: true);
                _doing = Doing.RunRightAfterLongJump;
                break;

            case Doing.RunRightAfterLongJump:
                MoveAcross();
                if (Position.X.Whole < LongLeapEndX)
                    break;
                Play("jumping_right", restart: true);
                _doing = Doing.LongJumpLanding;
                Velocity = Velocity.WithY(Pixels.FromWhole(-1));
                break;

            case Doing.LongJumpLanding:
                MoveAcross();
                if (MoveDown())
                    BeginLessonPause();
                break;

            case Doing.TeachingJump:
                PauseForQuake();
                if (!MoveDown())
                    break;
                if (Velocity.Y.IsNegative)
                {
                    Crest();
                    break;
                }

                _timer = HopCrouch;
                Play("hopping_left", restart: true);
                _doing = Route == Routes.TeachAtOnce && Position.X.Whole < LongRunFromBefore
                    ? Doing.WaitForSamusBeforeLongRun
                    : Doing.IdleBetweenTeachingJumps;
                break;

            case Doing.IdleBetweenTeachingJumps:
                PauseForQuake();
                if (--_timer > 0)
                    break;
                _teachingHops++;
                bool lessonGoesOn = _teachingHops < 4;
                if (!lessonGoesOn)
                    _teachingHops = 0;
                if (lessonGoesOn || Route == Routes.TeachAtOnce)
                {
                    _doing = Doing.TeachingJump;
                    Play("continue_hopping_left", restart: true);
                }
                else
                {
                    _doing = Doing.RunRightToReturnJump;
                    Play("running_right", restart: true);
                    Velocity = Velocity.WithX(RunRight);
                }

                Velocity = Velocity.WithY(Hop);
                JumpCry(samus);
                break;

            case Doing.WaitForSamusBeforeLongRun:
                PauseForQuake();
                if (--_timer > 0)
                    break;
                if (WithinDown(samus, Near) && WithinAcross(samus, 48))
                {
                    Play("running_right", restart: true);
                    _doing = Doing.RunRightToLongJump;
                    break;
                }

                Velocity = Velocity.WithY(Hop);
                Play("continue_hopping_left", restart: true);
                _doing = Doing.TeachingJump;
                JumpCry(samus);
                break;

            case Doing.RunRightToReturnJump:
                MoveAcross();
                if (Position.X.Whole < RunStartX)
                    break;
                _doing = Doing.ReturnJump;
                Play("jumping_right", restart: true);
                break;

            case Doing.ReturnJump:
                MoveAcross();
                if (!MoveDown())
                    break;
                Velocity = new PixelVelocity(RunLeft, Hop);
                _doing = Doing.RunLeftToWall;
                Play("running_left", restart: true);
                break;

            default:
                throw new InvalidOperationException($"The Etecoon has no step {_doing}.");
        }
    }

    /// <summary>Having climbed the shaft and landed, it does what its route says.</summary>
    private void TakeRoute()
    {
        switch (Route)
        {
            case Routes.RunLeftToTeachingStart:
                _doing = Doing.MoveLeftToTeachingStart;
                Play("running_left", restart: true);
                Velocity = Velocity.WithX(RunLeft);
                break;
            case Routes.RunRightToTeachingStart:
                _doing = Doing.MoveRightToTeachingStart;
                Play("running_right", restart: true);
                Velocity = Velocity.WithX(RunRight);
                break;
            case Routes.TeachAtOnce:
                _doing = Doing.TeachingJump;
                Play(_headsRight ? "continue_hopping_left" : "continue_hopping_right", restart: true);
                Velocity = Velocity.WithX(RunRight);
                break;
            default:
                throw new InvalidOperationException($"The Etecoon has no route {Route}.");
        }
    }

    private void BeginLessonPause()
    {
        (_doing, _timer) = (Doing.IdleBetweenTeachingJumps, HopCrouch);
        Play("hopping_left", restart: true);
    }

    /// <summary>It has struck its head on the way up: it hangs a moment and comes down.</summary>
    private void Crest()
    {
        Velocity = Velocity.WithY(Pixels.Zero);
        Play("hop_from_crest", restart: true);
    }

    private void Head(bool right)
    {
        _headsRight = right;
        _teachingHops = 0;
    }

    private void JumpCry(Player samus)
    {
        if (samus.Position.X.Whole >= HeardFromSamusX)
            Sound(JumpSound);
    }

    /// <summary>While the room shakes it stops what it is doing for a while, holding the picture it is on.</summary>
    private void PauseForQuake()
    {
        if (Room.ShakeStrength <= 0)
            return;
        _quakePause = 128;
    }

    /// <summary>Moves it across at its speed. Returns whether something stopped it.</summary>
    private bool MoveAcross() => MoveAcrossBy(Velocity.X);

    /// <summary>Moves it up or down at its speed, which gravity then adds to. Returns whether something stopped it.</summary>
    private bool MoveDown()
    {
        Pixels by = Velocity.Y;
        if (Velocity.Y < FastestFall)
            Velocity = Velocity.WithY(Velocity.Y + Pixels.FromNumber(Gravity));
        return MoveDownBy(by);
    }

    private bool WithinAcross(Player samus, int distance) => Math.Abs(Position.X.Whole - samus.Position.X.Whole) < distance;

    private bool WithinDown(Player samus, int distance) => Math.Abs(Position.Y.Whole - samus.Position.Y.Whole) < distance;

    /// <summary>Whether Samus is to its left; one straight above or below it counts as to its right.</summary>
    private bool SamusIsToItsLeft(Player samus)
    {
        PixelOffset to = samus.Position - Position;
        bool straightAboveOrBelow = Math.Abs(to.Y.Whole) >= 32 && Math.Abs(to.X.Whole) < 32;
        return to.X.Whole < 0 && !straightAboveOrBelow;
    }
}
