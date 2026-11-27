using Engine.Actors;
using Engine.Actors.Components;
using Engine.Content;
using Engine.Shared;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Squeept: it leaps straight up out of the lava, trailing flame, flips over at the top, and falls back in, again
/// and again, each leap one of four heights picked by chance. Nothing in the room stops it.
/// </summary>
/// <remarks>
/// Its timelines are "hidden", "jump", whose cue "flipped" marks the end of its flip, and "flame", which its flame
/// plays from its data file: the flame is a thing of the room in its own right, placed after it
/// (<see cref="SqueeptFlame"/>). Its data lists the upward speeds its leaps start with under "launch_speeds".
/// </remarks>
public partial class Squeept : Enemy
{
    /// <summary>The downward speed it gains each update, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.21875;

    /// <summary>The upward speed, in pixels an update, it must have slowed to before it starts to flip.</summary>
    [Export, DataValue("flip_speed")]
    public double FlipSpeed { get; set; } = 4;

    /// <summary>How tall one screen of a room is, in pixels; it is back in the lava in the lowest block of one.</summary>
    [Export, DataValue("screen_height")]
    public int ScreenHeight { get; set; } = 256;

    /// <summary>How tall the lowest block of a screen is, in pixels.</summary>
    [Export, DataValue("block_height")]
    public int BlockHeight { get; set; } = 16;

    /// <summary>The sound of it leaping, by name.</summary>
    [Export, DataValue("jump_sound")]
    public string JumpSound { get; set; } = "";

    private enum Doing
    {
        BeginLeap,
        Rising,
        Flipping,
        Flipped,
        FallingBack,
    }

    private readonly List<Pixels> _launchSpeeds = [];
    private Doing _doing;
    private PixelPosition _placed;

    /// <summary>Whether it is on its way up, as it is while its flame shows.</summary>
    public bool IsRising => Velocity.Y.IsNegative;

    /// <summary>The offset its pictures are drawn at, which its flame's are drawn at too.</summary>
    public Vector2I PictureOffset => Visual?.PictureOffset ?? Vector2I.Zero;

    /// <inheritdoc/>
    public override void ReadValues(DataNode data)
    {
        base.ReadValues(data);
        _launchSpeeds.Clear();
        _launchSpeeds.AddRange(data.List("launch_speeds").Select(speed => speed.AsPixels()));
        if (_launchSpeeds.Count != 4)
            throw data.Problem("must list four launch speeds");
    }

    /// <inheritdoc/>
    protected override void OnEnterRoom()
    {
        _placed = PixelPosition.FromWhole(Position.X.Whole, Position.Y.Whole);
        Play("hidden");
    }

    /// <inheritdoc/>
    protected override void OnCue(string cue)
    {
        if (cue != "flipped")
            return;
        if (_doing != Doing.Flipping)
            throw new InvalidOperationException($"The Squeept \"{Name}\" finished its flip while it was {_doing}.");
        _doing = Doing.Flipped;
    }

    /// <inheritdoc/>
    protected override void Behave()
    {
        // It draws a number every update, whether or not it is used.
        RandomDraw draw = Random.Draw();
        Pixels gravity = Pixels.FromNumber(Gravity);
        switch (_doing)
        {
            case Doing.BeginLeap:
                Velocity = new PixelVelocity(Pixels.Zero, -_launchSpeeds[draw.Part(9, 2)]);
                _doing = Doing.Rising;
                AlwaysActive = true;
                Sound(JumpSound);
                break;

            case Doing.Rising:
                Rise(gravity);
                if (Velocity.Y.IsNegative && Velocity.Y >= -Pixels.FromNumber(FlipSpeed))
                {
                    Play("jump");
                    _doing = Doing.Flipping;
                }

                break;

            case Doing.Flipping:
                Rise(gravity);
                break;

            case Doing.Flipped:
                Rise(gravity);
                _doing = Doing.FallingBack;
                break;

            case Doing.FallingBack:
                Position += new PixelOffset(Pixels.Zero, Velocity.Y);
                if (Position.Y.Whole % ScreenHeight >= ScreenHeight - BlockHeight)
                {
                    // Back where it was placed, keeping where it lay within a pixel for its next leap.
                    Position = new PixelPosition(Pixels.FromParts(_placed.X.Whole, Position.X.Fraction), Pixels.FromParts(_placed.Y.Whole, Position.Y.Fraction));
                    Play("hidden");
                    _doing = Doing.BeginLeap;
                    AlwaysActive = false;
                }
                else
                {
                    Velocity = Velocity.WithY(Velocity.Y + gravity);
                }

                break;

            default:
                throw new InvalidOperationException($"Undefined Squeept stage {_doing}.");
        }
    }

    /// <summary>Moves it by its speed and lets gravity slow or quicken that.</summary>
    private void Rise(Pixels gravity)
    {
        Position += new PixelOffset(Pixels.Zero, Velocity.Y);
        Velocity = Velocity.WithY(Velocity.Y + gravity);
    }
}
