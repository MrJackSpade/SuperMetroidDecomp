using Engine.Actors;
using Engine.Collision;
using Engine.Shared;
using Game.Compatibility;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Multiviola: a ball of fire that travels in a straight line and bounces off whatever it meets. It keeps a heading
/// and takes its speed afresh from it on every update; a wall or a floor turns the heading, and meeting a wall it does
/// not move up or down in that update.
/// </summary>
/// <remarks>
/// Its timeline is "flying". As on the cartridge, its heading counts from straight left: none heads left, a quarter
/// turn (64) down, a half turn right and three quarters up. Each axis takes the cartridge's half wave of the heading
/// as its share of the speed. Heading left or up along an axis, the share's whole pixels are complemented and its
/// part of a pixel reversed by itself, which is not the share's plain reverse: it is a 65536th short of it, and an
/// axis with no share at all still moves a pixel an update left or up. A bounce changes which quarter of the turn the
/// heading lies in and keeps how far into the quarter it is, one change for a wall and another for a floor or a
/// ceiling, so only a heading half way through a quarter bounces as a mirror would.
/// </remarks>
public partial class Multiviola : Enemy
{
    private const int HalfTurn = 128, QuarterTurn = 64;

    /// <summary>The way it sets off, in 256ths of a turn from straight left, turning toward down.</summary>
    [Export]
    public int Heading { get; set; }

    /// <summary>How far it travels each update, in pixels.</summary>
    [Export]
    public int Speed { get; set; } = 1;

    /// <summary>
    /// How its moves are tried, as the cartridge moves an enemy: only where each would end, and sloped ground that holds
    /// things up by their middle met under its middle alone.
    /// </summary>
    private static readonly MoveOptions Moves =
        MoveOptions.AmongBlocks with { Tries = WayTried.WhereItEnds, Slopes = SlopeTreatment.ClimbUnderMiddle, RestsAgainstSlopes = true };

    /// <summary>How a move across is tried: as its other moves, and such sloped ground is no part of it.</summary>
    private static readonly MoveOptions MovesAcross = Moves with { Slopes = SlopeTreatment.IgnoreMiddleHeld };

    private int _heading;

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _heading = Turns.Wrap(Heading);
        Play("flying");
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        Velocity = new PixelVelocity(AlongAxis(_heading + QuarterTurn), AlongAxis(_heading + HalfTurn));
        int quarter = _heading / QuarterTurn;
        if (MoveAcross(MovesAcross).Blocked)
        {
            // A wall swaps the heading's quarter with its neighbour.
            _heading = InQuarter(quarter % 2 == 0 ? quarter + 1 : quarter - 1);
            return;
        }

        // A floor or a ceiling takes the heading to the quarter opposite across the turn.
        if (MoveDown(Moves).Blocked)
            _heading = InQuarter(3 - quarter);
    }

    /// <summary>The heading as far into the given quarter of the turn as it is into its own.</summary>
    private int InQuarter(int quarter) => quarter * QuarterTurn + _heading % QuarterTurn;

    /// <summary>Its speed along the axis whose angle is given: right or down in the turn's second half, left or up in its first.</summary>
    private Pixels AlongAxis(int axisAngle)
    {
        int wrapped = Turns.Wrap(axisAngle);
        long share = (long)Speed * CartridgeAngles.UnsignedHalfWave(wrapped % HalfTurn);
        int whole = (int)(share / Pixels.FractionsPerPixel), fraction = (int)(share % Pixels.FractionsPerPixel);
        return wrapped >= HalfTurn
            ? Pixels.FromParts(whole, fraction)
            : Pixels.FromParts(~whole, (Pixels.FractionsPerPixel - fraction) % Pixels.FractionsPerPixel);
    }
}
