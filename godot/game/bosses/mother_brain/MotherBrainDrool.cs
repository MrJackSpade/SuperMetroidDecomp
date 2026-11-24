using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// Something that hangs from Mother Brain's head and goes where it goes: a drop of her drool, which lets go, falls
/// and splashes on the floor, or one of the sparks of the beam she is gathering, which stays with her until it ends.
/// </summary>
/// <remarks>
/// Its timeline gives the cue "drop" when it lets go, "fall" when it starts to fall, and "end" to take it away. Its
/// data names its "splash_timeline".
/// </remarks>
public partial class MotherBrainDrool : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>How far down the room the floor it splashes on is.</summary>
    [Export, DataValue("floor")]
    public int Floor { get; set; } = 215;

    /// <summary>How far it drops as it lets go, in pixels.</summary>
    [Export, DataValue("release_drop")]
    public int ReleaseDrop { get; set; } = 12;

    /// <summary>How far above where it lands its splash is shown, in pixels.</summary>
    [Export, DataValue("splash_rise")]
    public int SplashRise { get; set; } = 4;

    /// <summary>The timeline of its splash, by its data file.</summary>
    [Export, DataValue("splash_timeline")]
    public string SplashTimeline { get; set; } = "";

    private static readonly Pixels Gravity = Pixels.FromNumber(3 / 64.0);

    private MotherBrain? _owner;
    private PixelOffset _point;
    private bool _hanging = true;
    private bool _falling;
    private Pixels _fall;

    /// <summary>Hangs it from <paramref name="owner"/>'s head, at <paramref name="point"/> from it.</summary>
    public void HangFrom(MotherBrain owner, PixelOffset point) => (_owner, _point) = (owner, point);

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <inheritdoc/>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void OnCue(Room room, string cue)
    {
        switch (cue)
        {
            case "drop":
                _hanging = false;
                Position += PixelOffset.FromWhole(0, ReleaseDrop);
                break;
            case "fall":
                _hanging = false;
                _falling = true;
                break;
            case "end":
                Leave(room);
                break;
            default:
                throw new InvalidOperationException($"What hangs from Mother Brain does not know the cue \"{cue}\".");
        }
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (_hanging)
        {
            if (_owner is not { } owner || !IsInstanceValid(owner) || owner.IsDead)
            {
                Leave(room);
                return;
            }

            Position = owner.HeadPosition + _point;
            return;
        }

        if (!_falling)
            return;
        _fall += Gravity;
        Position += new PixelOffset(Pixels.Zero, _fall);
        if (Position.Y.Whole < Floor)
            return;
        _falling = false;
        Position -= PixelOffset.FromWhole(0, SplashRise);
        PlayTimeline(SplashTimeline);
    }
}
