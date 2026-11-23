using Engine.Actors;
using Engine.Actors.Components;
using Engine.Collision;
using Engine.Input;
using Engine.Shared;
using Game.Data;

namespace Game;

/// <summary>
/// Swinging on the grapple beam: she hangs from where the line holds and swings like a weight on a string. Steering
/// pushes the swing; up and down draw the line in and let it out. Letting go of the attack lets go of the line, and
/// she flies off with the speed the swing had.
/// </summary>
public sealed class GrapplingState : PlayerState, ISamusLook
{
    /// <summary>How many parts one angle step of 256 to the turn is divided into, for a smooth swing.</summary>
    private const int Parts = 256;

    /// <summary>The name, in each medium's speeds, of the pull that swings her back toward hanging straight down.</summary>
    public const string SwingPull = "grapple_swing_pull";

    /// <summary>The name, in each medium's speeds, of the push steering gives her swing.</summary>
    public const string SwingPush = "grapple_swing_push";

    /// <summary>The name, in each medium's speeds, of how fast the line is drawn in and let out.</summary>
    public const string ReelSpeed = "grapple_reel_speed";

    /// <summary>The name, in each medium's speeds, of the shortest the line is drawn in to.</summary>
    public const string ShortestLine = "grapple_shortest_line";

    /// <summary>The name, in each medium's speeds, of the longest the line is let out to.</summary>
    public const string LongestLine = "grapple_longest_line";

    /// <summary>Where on its arc she is: the angle from hanging straight down, clockwise seen from the front, in parts of a step.</summary>
    private long _angle;

    /// <summary>How fast the angle is changing each update.</summary>
    private long _turning;

    /// <summary>How long the line is.</summary>
    private Pixels _length;

    /// <inheritdoc/>
    public string Look => "grappling";

    /// <inheritdoc/>
    public override bool CanAttack => false;

    /// <inheritdoc/>
    public override void Enter(Player owner)
    {
        var samus = (Samus)owner;
        SamusGrapple grapple = samus.Grapple ?? throw new InvalidOperationException("Samus is swinging with no grapple beam.");
        owner.LoseMomentum();
        owner.TrySetForm(owner.FormOr(Player.Jumping), aroundMiddle: true);
        PixelOffset hanging = owner.Position - grapple.Anchor;
        long across = hanging.X.Whole, down = hanging.Y.Whole;
        _length = Pixels.FromWhole(WholeRoot(across * across + down * down));
        _angle = AngleOf(across, down);
        // The speed she had along the arc carries into the swing.
        _turning = _length.Whole == 0 ? 0 : owner.Velocity.X.Whole * Parts * 40 / Math.Max(1, _length.Whole);
        owner.Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    public override State<Player> Step(Player owner)
    {
        var samus = (Samus)owner;
        SamusGrapple? grapple = samus.Grapple;
        if (grapple is null || !grapple.IsLatched || !owner.Input.IsHeld(GameAction.Attack) || samus.Selected != Equipment.GrappleBeam)
            return LetGo(owner, grapple);

        PlayerMedium medium = owner.Medium;
        int reel = (owner.Input.IsHeld(GameAction.Down) ? 1 : 0) - (owner.Input.IsHeld(GameAction.Up) ? 1 : 0);
        if (reel != 0)
        {
            Pixels length = _length + medium.Speed(ReelSpeed) * reel;
            Pixels shortest = medium.Speed(ShortestLine), longest = medium.Speed(LongestLine);
            _length = length < shortest ? shortest : length > longest ? longest : length;
        }

        // The pull is strongest with the line level and nothing hanging straight down, as a weight on a string.
        long pull = Sine(_angle, medium.Speed(SwingPull));
        long push = FractionsOf(medium.Speed(SwingPush)) * owner.Intent;
        _turning += (-pull - push) * Parts / Pixels.FractionsPerPixel;
        _angle += _turning;

        PixelPosition anchor = grapple.Anchor;
        var target = new PixelPosition(
            anchor.X + FromFractions(Sine(_angle, _length)),
            anchor.Y + FromFractions(Sine(_angle + 64 * Parts, _length)));
        PixelOffset move = target - owner.Position;
        MoveOptions swing = MoveOptions.Plain with { Slopes = SlopeTreatment.Wall };
        bool stopped = owner.Room.Mover.MoveAcross(owner.Body, move.X, swing).Blocked;
        stopped |= owner.Room.Mover.MoveDown(owner.Body, move.Y, swing).Blocked;
        if (stopped)
        {
            // Meeting something solid turns the swing back with half its speed, from where she actually is.
            PixelOffset hanging = owner.Position - anchor;
            _angle = AngleOf(hanging.X.Whole, hanging.Y.Whole);
            _turning = -_turning / 2;
        }

        if (!move.X.IsZero)
            owner.Facing = move.X.IsNegative ? Facing.Left : Facing.Right;
        owner.Velocity = new PixelVelocity(move.X, move.Y);
        return this;
    }

    /// <summary>Lets go of the line: she flies off with the speed the swing had.</summary>
    private static State<Player> LetGo(Player owner, SamusGrapple? grapple)
    {
        grapple?.Release();
        return owner.IsOnGround ? new StandingState() : owner.Velocity.Y.IsNegative ? new JumpingState(keepsSpeed: true) : new FallingState();
    }

    /// <summary>The angle, in parts of a step from hanging straight down, of a point hanging <paramref name="across"/> and <paramref name="down"/> from the anchor.</summary>
    private static long AngleOf(long across, long down)
    {
        // The step whose direction lies closest to the point's: whole numbers only, so every machine agrees.
        Pixels size = Pixels.FromWhole(4096);
        long best = 0, bestAlong = long.MinValue;
        for (int step = 0; step < Turns.PerTurn; step++)
        {
            long along = across * Turns.Sine(step, size).Whole + down * Turns.Cosine(step, size).Whole;
            if (along > bestAlong)
            {
                bestAlong = along;
                best = step;
            }
        }

        return best * Parts;
    }

    /// <summary>The whole number nearest below the square root of <paramref name="value"/>.</summary>
    private static int WholeRoot(long value)
    {
        long root = 0;
        for (long bit = 1L << 30; bit > 0; bit >>= 1)
        {
            if ((root + bit) * (root + bit) <= value)
                root += bit;
        }

        return (int)root;
    }

    /// <summary><paramref name="size"/> times the sine of an angle given in parts of a step, in fractions of a pixel, smoothed between whole steps.</summary>
    private static long Sine(long angle, Pixels size)
    {
        long step = Math.DivRem(angle, Parts, out long part);
        if (part < 0)
        {
            part += Parts;
            step--;
        }

        long here = FractionsOf(Turns.Sine((int)(step % Turns.PerTurn), size));
        long next = FractionsOf(Turns.Sine((int)((step + 1) % Turns.PerTurn), size));
        return here + (next - here) * part / Parts;
    }

    /// <summary>A distance as a count of fractions of a pixel.</summary>
    private static long FractionsOf(Pixels distance) => (long)distance.Whole * Pixels.FractionsPerPixel + distance.Fraction;

    /// <summary>A distance from a count of fractions of a pixel.</summary>
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
