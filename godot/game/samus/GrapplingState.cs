using Engine.Actors;
using Engine.Collision;
using Engine.Input;
using Engine.Shared;
using Game.Compatibility;
using Game.Data;

namespace Game;

/// <summary>
/// Swinging on the grapple beam, as the cartridge swings her. She hangs from the end of a line of a set length that
/// turns about what holds it, and is put where the line's end is: nothing moves her body through the room. The
/// line's angle is counted clockwise from straight up in 65536ths of a turn, 256 to an angle step, and its turning
/// in 256ths of a step an update.
/// <list type="bullet">
/// <item>A fresh press of up draws the line in and one of down lets it out, a few pixels an update from then on,
/// tried a pixel at a time against the ground at her hand's end or her feet's; the line draws itself in from the
/// moment it takes hold.</item>
/// <item>In the lower half of the turn a held direction pushes the swing, and kicks a still one hanging straight
/// down into motion.</item>
/// <item>Gravity is by quarter of the turn, not by the sine: whole in the lower two, a share in the upper two, and
/// none hanging straight down, where a slow swing stops. A small correction is added when the swing turns against
/// the side she hangs on.</item>
/// <item>The turn of an update is tried an angle step at a time against the ground at points along her body. A
/// strike leaves her in the middle of the last step that was clear and turns the swing back at half its speed, or
/// stops it when the line is at its shortest and the strike is by her hand; for a while after a bounce a fresh press
/// of the jump adds a kick the way the swing turns.</item>
/// <item>Under a liquid without the gravity suit the swing takes a share of gravity, of the push and of the kick,
/// and turns through a share of its speed. Whether she is under one is looked at after each update, for the next.</item>
/// </list>
/// Letting go of the attack, or losing what the line held, lets go: hanging still and straight down she drops, standing,
/// on the update after, and
/// otherwise she flies off at twice the swing's speed along the line's own direction, facing the way it turned.
/// </summary>
public sealed class GrapplingState : PlayerState, ISamusLook
{
    /// <summary>How many parts an angle step is divided into.</summary>
    private const int Parts = 256;

    /// <summary>The angle steps of a quarter, a half and three quarters of a turn, in parts.</summary>
    private const int QuarterTurn = Turns.PerTurn / 4 * Parts, HalfTurn = Turns.PerTurn / 2 * Parts, ThreeQuarterTurn = 3 * QuarterTurn;

    /// <summary>The parts in a whole turn.</summary>
    private const int WholeTurn = Turns.PerTurn * Parts;

    /// <summary>
    /// While the swing turns fast, either way, the view follows her three pixels an update toward keeping her between
    /// 96 and 160 across it and 112 and 144 down it, as the cartridge's does.
    /// </summary>
    private static readonly SlowFollowing FastSwingView = new(3, new PixelRect(96, 112, 64, 32));

    private GrappleSwingRules? _rules;

    /// <summary>The line's angle, clockwise from straight up, in parts of a step: none through a whole turn less one.</summary>
    private int _angle;

    /// <summary>How fast the angle changes each update, in parts of a step.</summary>
    private int _turning;

    /// <summary>What a held direction, gravity and the correction add to the turning on this update, and the jump's kick.</summary>
    private int _pushing;
    private int _pulling;
    private int _correcting;
    private int _kick;

    /// <summary>How long the line is, and how its length changes each update.</summary>
    private int _length;
    private int _lengthChange;

    /// <summary>The updates left of a bounce off the ground, in which the jump adds its kick.</summary>
    private int _bounceLeft;

    /// <summary>Whether the swing was last stopped by her hand on the shortest line, and has not turned freely since.</summary>
    private bool _restsByHand;

    /// <summary>Whether she was under a liquid, without the gravity suit, when last looked at.</summary>
    private bool _submerged;

    /// <summary>What holds the line, put within its block where the line leaves it.</summary>
    private PixelPosition _anchor;

    /// <summary>Whether she let go hanging still and straight down, to be dropped on the next update.</summary>
    private bool _dropping;

    /// <inheritdoc/>
    public override SlowFollowing? ViewFollowsSlowly(Player owner) =>
        _rules is { } rules && Math.Abs(_turning) >= rules.FastTurning ? FastSwingView : null;

    /// <inheritdoc/>
    public string Look
    {
        get
        {
            // She is shown one of so many ways by the way the line runs: the way nearest the whole step its angle lies
            // in, counted round from straight up, the same number facing either way.
            int ways = _rules?.BodyLeft.Count ?? 32;
            return $"grappling_{Way(ways)}";
        }
    }

    /// <summary>
    /// Stops <paramref name="owner"/> across and up and down as the line takes hold, and keeps a rise a rise: the
    /// cartridge clears her speeds there and leaves which way she was going up or down, so the view goes on leading
    /// a Samus caught on her way up for as long as she hangs.
    /// </summary>
    internal static void StopKeepingRise(Player owner)
    {
        bool rising = owner.IsRising;
        owner.Velocity = PixelVelocity.Zero;
        if (rising)
            owner.Rise(Pixels.Zero);
    }

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <summary>The movement is not her pose's own: it goes on moving her through her death's last pass of play, as the cartridge's does.</summary>
    public override bool MovesOnWhileDying => true;

    /// <summary>A knockback waits while she is in this state, as the cartridge refuses it here and tries again each update of its time.</summary>
    public override bool HoldsOffInterruptions => true;

    /// <summary>
    /// The swing's own body, taken with no feeling for room and hers at once, as the cartridge sets the swinging pose
    /// and its size outright as the line takes hold: she is then put at the line's end.
    /// </summary>
    public override FormAsked? EntersIn(Player owner) => new FormAsked(Samus.Swinging, Unfitted: true, AtOnce: true);

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        var samus = (Samus)owner;
        SamusGrapple grapple = samus.Grapple ?? throw new InvalidOperationException("Samus is swinging with no grapple beam.");
        GrappleSwingRules rules = _rules = grapple.Swing;
        owner.LoseMomentum();

        // The angle is that of where she was when the line took hold, a whole step; a long line is shortened; and
        // the line begins to draw itself in. Her speeds are cleared.
        _anchor = grapple.Anchor;
        PixelOffset hanging = owner.Position - _anchor;
        _angle = CartridgeAngles.AngleClockwiseFromUp(hanging.X.Whole, hanging.Y.Whole) * Parts;
        _length = grapple.Length;
        if (_length >= rules.LongLine)
            _length -= rules.LongLineShortening;
        (_turning, _pushing, _pulling, _correcting, _kick, _bounceLeft, _restsByHand) = (0, 0, 0, 0, 0, 0, false);
        _lengthChange = rules.Retraction;
        _submerged = IsSubmerged(owner);
        StopKeepingRise(owner);
        PlaceFromLine(owner, rules);
        grapple.ShowLineAs(WholeStep, _length);

        // Taking hold puts her at the line's end, and the view follows that by no more than its cap.
        samus.CapViewAfterPlacing();
    }

    /// <summary>The swing is hers before the enemies act, as the cartridge plays the grapple's whole step in her input's part of the update.</summary>
    public override bool MovesBeforeEnemies => true;

    /// <inheritdoc/>
    public override PlayerState? Move(Player owner)
    {
        var samus = (Samus)owner;
        SamusGrapple? grapple = samus.Grapple;
        GrappleSwingRules rules = _rules ?? throw new InvalidOperationException("Samus swings before her swing was begun.");

        // Dropped on the update before, she is let go on this one: her speeds cleared and standing, whatever she aims
        // and in the air too, as the cartridge drops a swing into its standing pose. Nothing of her moves meanwhile.
        if (_dropping)
        {
            // Here her rise is over with her speeds. (The cartridge's drop keeps which way she was going into its
            // standing pose; what its standing then makes of that was not read, so it is not carried.)
            owner.Velocity = PixelVelocity.Zero;
            owner.FallFromRest();
            return new StandingState();
        }

        if (grapple is null || !grapple.IsLatched || !owner.Input.IsHeld(GameAction.Attack) || samus.Selected != Equipment.GrappleBeam)
            return LetGo(owner, grapple, rules);

        // What holds the line may have moved; the line's end is reckoned from it afresh.
        _anchor = grapple.Anchor;
        TakeInput(owner, rules);
        ChangeLength(owner, rules);
        ReckonGravity(rules);

        // The turning takes the push and gravity, and the correction where it turns against the side she hangs on:
        // turning back with the angle in the first half of the turn, or on with it in the second.
        int turning = _turning + _pushing + _pulling;
        if ((turning < 0) != (_angle >= HalfTurn))
            turning += _correcting;
        _turning = Math.Clamp(turning, -rules.MostTurning, rules.MostTurning);

        // A fresh press of the jump while a bounce lasts kicks the way the swing turns; a still swing takes none.
        if (_bounceLeft > 0 && owner.Input.WasPressed(GameAction.Jump))
            _kick = Math.Sign(_turning) * (_submerged ? rules.JumpKick / rules.SubmergedShare : rules.JumpKick);
        Turn(owner, rules);

        // Stopped by her hand at one of the exact angles the swing's data names, she is left in that angle's pose
        // at its place from what holds the line: held crouched, or against a wall.
        if (_restsByHand && rules.Stops.FirstOrDefault(named => named.Angle == _angle) is { } stop)
        {
            owner.Facing = stop.FacesLeft ? Facing.Left : Facing.Right;
            PixelPosition at = _anchor + stop.Place;
            return stop.GrabsWall ? new GrappleWallGrabState(at) : new GrappleLockedState(crouched: true, at, Aim.DiagonalDown);
        }

        PlaceFromLine(owner, rules);
        grapple.ShowLineAs(WholeStep, _length);
        _submerged = IsSubmerged(owner);
        return null;
    }

    /// <summary>
    /// The line's length changes on a fresh press of up or down, and stays changing; a held direction pushes the
    /// swing only through the lower half of the turn.
    /// </summary>
    private void TakeInput(Player owner, GrappleSwingRules rules)
    {
        if (owner.Input.WasPressed(GameAction.Up))
        {
            if (_length != 0)
                _lengthChange = -rules.LengthStep;
        }
        else if (owner.Input.WasPressed(GameAction.Down))
        {
            if (_length < rules.LengthCap)
                _lengthChange = rules.LengthStep;
            else
                _length = rules.LengthCap;
        }

        _pushing = 0;
        if (_angle < QuarterTurn || _angle >= ThreeQuarterTurn)
            return;
        int push = _submerged ? rules.Push / rules.SubmergedShare : rules.Push;
        bool still = HangsStraightDown && _turning == 0;
        if (owner.Input.IsHeld(GameAction.Left))
        {
            if (still)
                _turning = rules.StillKick;
            _pushing = push;
        }
        else if (owner.Input.IsHeld(GameAction.Right))
        {
            if (still)
                _turning = -rules.StillKick;
            _pushing = -push;
        }
    }

    /// <summary>
    /// Walks the line's length toward what its change asks, a pixel at a time, testing her hand's end when it shortens
    /// and her feet's when it lengthens, and keeps the last length that fitted. A bound reached ends the change; a
    /// strike leaves it, to be tried again on the next update.
    /// </summary>
    private void ChangeLength(Player owner, GrappleSwingRules rules)
    {
        if (_lengthChange == 0)
            return;
        int target = _length + _lengthChange, direction, probe;
        if (_lengthChange < 0)
        {
            (direction, probe) = (-1, rules.ShorteningProbe);
            if (target < rules.Shortest)
                (target, _lengthChange) = (rules.Shortest, 0);
        }
        else
        {
            (direction, probe) = (1, rules.LengtheningProbe);
            if (target >= rules.LongestLetOut)
                (target, _lengthChange) = (rules.LongestLetOut, 0);
        }

        int length = _length;
        while (length != target)
        {
            int candidate = length + direction;
            if (Strikes(owner, PointAt(rules, WholeStep, candidate + probe)))
                break;
            length = candidate;
        }

        _length = length;
    }

    /// <summary>
    /// Gravity and the correction for this update, by the quarter of the turn the angle lies in: whole below the
    /// level of what holds the line and a share above it, each swinging her back toward hanging straight down, where
    /// there is neither and a slow swing stops. Under a liquid gravity is a share of itself first.
    /// </summary>
    private void ReckonGravity(GrappleSwingRules rules)
    {
        int gravity = _submerged ? rules.Gravity / rules.SubmergedShare : rules.Gravity;
        int correction = rules.Correction;
        if (_angle >= ThreeQuarterTurn)
        {
            (_correcting, _pulling) = (-(correction / rules.UpperShare), -(gravity / rules.UpperShare));
        }
        else if (_angle >= HalfTurn)
        {
            if (HangsStraightDown)
            {
                (_correcting, _pulling) = (0, 0);
                if (Math.Abs(_turning) < rules.RestingTurning)
                    _turning = 0;
            }
            else
            {
                (_correcting, _pulling) = (-correction, -gravity);
            }
        }
        else if (_angle >= QuarterTurn)
        {
            (_correcting, _pulling) = (correction, gravity);
        }
        else
        {
            (_correcting, _pulling) = (correction / rules.UpperShare, gravity / rules.UpperShare);
        }
    }

    /// <summary>
    /// Turns the line by this update's turning and kick, an angle step at a time, testing the points along her body
    /// at each step. A strike leaves the angle in the middle of the last step that was clear: on the shortest line, by
    /// her hand, the swing stops; otherwise it turns back at half its speed and the jump's kick may be added for a
    /// while. With nothing struck the whole turn is kept, a bounce ages and the kick dies away.
    /// </summary>
    private void Turn(Player owner, GrappleSwingRules rules)
    {
        int combined = _turning + _kick;
        int turn = _submerged ? combined * rules.SubmergedTurnShare / rules.SubmergedTurnParts : combined;
        if (turn == 0)
            return;
        int target = Wrapped(_angle + turn);
        int targetStep = target / Parts, safe = WholeStep, way = Math.Sign(combined);
        while (safe != targetStep)
        {
            int candidate = Turns.Wrap(safe + way);
            if (StruckPoint(owner, rules, candidate) is { } struck)
            {
                _angle = safe * Parts + Parts / 2;
                if (_length == rules.Shortest && struck < rules.StoppingPoints)
                {
                    (_turning, _kick, _restsByHand) = (0, 0, true);
                }
                else
                {
                    _bounceLeft = rules.BounceUpdates;
                    (_turning, _kick) = (ReversedAtHalf(_turning), ReversedAtHalf(_kick));
                }

                return;
            }

            safe = candidate;
        }

        (_angle, _restsByHand) = (target, false);
        if (_bounceLeft > 0)
            _bounceLeft--;
        _kick = _kick < 0 ? Math.Min(0, _kick + rules.KickDecay) : Math.Max(0, _kick - rules.KickDecay);
    }

    /// <summary>Half a speed the other way, the half rounded toward below nothing before it is turned about.</summary>
    private static int ReversedAtHalf(int speed) => -(int)Math.Floor(speed / 2.0);

    /// <summary>Which of the points along her body, counted from her hand, first strikes the ground with the line at an angle step; none when all are clear.</summary>
    private int? StruckPoint(Player owner, GrappleSwingRules rules, int step)
    {
        for (int point = 0; point < rules.BodyPoints; point++)
        {
            // Each point felt for meets the block it lies in, up to the one that strikes: one that hurts whoever meets
            // it, a spike or spike air, hurts her as the cartridge's swing is hurt at the points it tries.
            PixelPosition felt = PointAt(rules, step, _length + (point + 1) * rules.BodyPointSpacing);
            owner.MeetBlocksAt(felt);
            if (Strikes(owner, felt))
                return point;
        }

        return null;
    }

    /// <summary>
    /// The point a distance along the line's direction at an angle step, from what holds the line. What holds it is
    /// first put within its block where the line leaves it, a pixel nearer the block's start on an axis the line runs
    /// back along, and stays there.
    /// </summary>
    private PixelPosition PointAt(GrappleSwingRules rules, int step, int distance)
    {
        int across = CartridgeAngles.SignedSine(step), down = CartridgeAngles.SignedSine(step - Turns.PerTurn / 4);
        _anchor = PixelPosition.FromWhole(
            BlockStart(_anchor.X.Whole) + (across < 0 ? rules.AnchorTowardNegative : rules.AnchorTowardPositive),
            BlockStart(_anchor.Y.Whole) + (down < 0 ? rules.AnchorTowardNegative : rules.AnchorTowardPositive));
        return PixelPosition.FromWhole(_anchor.X.Whole + across * distance / 256, _anchor.Y.Whole + down * distance / 256);
    }

    /// <summary>
    /// Where the end of a line of a length lies, at an angle step from what holds it, what holds it put within its
    /// block where the line leaves it: for a connection that does not swing.
    /// </summary>
    internal static PixelPosition EndOfLine(GrappleSwingRules rules, PixelPosition anchor, int step, int length)
    {
        int across = CartridgeAngles.SignedSine(step), down = CartridgeAngles.SignedSine(step - Turns.PerTurn / 4);
        return PixelPosition.FromWhole(
            BlockStart(anchor.X.Whole) + (across < 0 ? rules.AnchorTowardNegative : rules.AnchorTowardPositive) + across * length / 256,
            BlockStart(anchor.Y.Whole) + (down < 0 ? rules.AnchorTowardNegative : rules.AnchorTowardPositive) + down * length / 256);
    }

    /// <summary>Where the block a pixel lies in starts.</summary>
    private static int BlockStart(int pixel) => pixel & ~15;

    /// <summary>Whether the block at a point stops the swing: anything solid does, a slope as a wall.</summary>
    private static bool Strikes(Player owner, PixelPosition point) =>
        owner.Room.Terrain.IsSolidIn(new PixelRect(point.X.Whole, point.Y.Whole, 1, 1), SlopeTreatment.Wall);

    /// <summary>Puts her where the line's end is: at the line's length from what holds it, and from there by the way the line runs and the way she faces.</summary>
    private void PlaceFromLine(Player owner, GrappleSwingRules rules)
    {
        PixelPosition end = PointAt(rules, WholeStep, _length);
        IReadOnlyList<PixelOffset> places = owner.Facing == Facing.Left ? rules.BodyLeft : rules.BodyRight;
        owner.Position = end + places[Way(places.Count)];
        RopeStart = end;
    }

    /// <summary>
    /// Where the line ends by her as she swings: at the line's length from what holds it. Her body hangs from there,
    /// so it is where the line's pictures are laid from, not her arm's place for a shot. None before she is first placed.
    /// </summary>
    public PixelPosition? RopeStart { get; private set; }

    /// <summary>
    /// Lets go of the line. Hanging still and exactly straight down she drops on the next update, standing and her
    /// speeds cleared, and stays where she is on this one. Otherwise she
    /// flies off at twice the swing's speed: down by the sine of the line's angle, and across by the sine of the
    /// angle a quarter turn back, less a few steps for each whole step of the swing's speed, facing left when the
    /// swing turned on and right when it turned back.
    /// The flight that follows is its own movement (<see cref="GrappleReleasedState"/>) and moves her on this update.
    /// </summary>
    private PlayerState? LetGo(Player owner, SamusGrapple? grapple, GrappleSwingRules rules)
    {
        // The line itself ends on the update after this one, as the cartridge's teardown does.
        grapple?.EndOnNextUpdate();
        if (_turning == 0 && _angle == HalfTurn)
        {
            // The drop, too, is done on the update after: on this one she stays where the swing last put her.
            _dropping = true;
            return null;
        }

        int twice = _turning * 2;
        long down = (long)CartridgeAngles.SignedSine(WholeStep) * twice;
        int wholeSteps = Math.Abs(_turning) / Parts;
        int lead = rules.ReleaseLead - rules.ReleaseLeadPerStep * wholeSteps;
        long across = Math.Abs((long)CartridgeAngles.SignedSine(Turns.Wrap(WholeStep - lead)) * twice);
        // The way the line ran as she let go is the way she is still shown on this update.
        string look = Look;
        owner.Facing = _turning >= 0 ? Facing.Left : Facing.Right;
        owner.Aim = Aim.Forward;
        owner.Velocity = new PixelVelocity(FromFractions(owner.Facing == Facing.Left ? -across : across), FromFractions(down));

        // Her flight moves her on this same update; one that is over at once leaves her in the jump it ends in.
        var flight = new GrappleReleasedState(look);
        return flight.Move(owner) ?? flight;
    }

    /// <summary>Whether a liquid slows her swing: she is in one without the suit that lets her move freely in it.</summary>
    private static bool IsSubmerged(Player owner) => owner.IsHinderedByLiquid;

    /// <summary>The whole angle step the line's angle lies in.</summary>
    private int WholeStep => _angle / Parts;

    /// <summary>Whether the line's angle lies in the step of hanging straight down.</summary>
    private bool HangsStraightDown => WholeStep == Turns.PerTurn / 2;

    /// <summary>Which of <paramref name="ways"/> evenly spaced ways, counted round from straight up, lies nearest the line's whole step: one halfway between two is the later.</summary>
    private int Way(int ways) => (WholeStep * ways + Turns.PerTurn / 2) / Turns.PerTurn % ways;

    private static int Wrapped(int angle) => ((angle % WholeTurn) + WholeTurn) % WholeTurn;

    /// <summary>A distance from a count of 65536ths of a pixel.</summary>
    private static Pixels FromFractions(long fractions)
    {
        long whole = Math.DivRem(fractions, Pixels.FractionsPerPixel, out long fraction);
        if (fraction < 0)
        {
            fraction += Pixels.FractionsPerPixel;
            whole--;
        }

        return Pixels.FromParts((int)whole, (int)fraction);
    }
}
