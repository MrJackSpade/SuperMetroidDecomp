using Engine.Actors;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A drop of the sweat of an eye door's eye. It is thrown out and falls, gaining speed downward each update. A wall
/// across only stops its way across and a ceiling its rise; it ends where it comes down onto terrain, leaving its
/// splash there. Ending on Samus it is simply gone.
/// </summary>
/// <remarks>
/// Its data names its "gravity", the speed it gains downward each update, and its "splash_effect", the data file of
/// the timeline its splash plays.
/// </remarks>
public partial class EyeDoorSweat : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>How much speed downward it gains each update, in pixels an update.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; }

    /// <summary>The effect it leaves where it comes down onto terrain, by the data file of its timeline.</summary>
    [Export, DataValue("splash_effect")]
    public string SplashEffect { get; set; } = "";

    private PixelVelocity _speed;

    /// <inheritdoc/>
    public void ReadValues(DataNode data)
    {
    }

    /// <inheritdoc/>
    public override void _ValidateProperty(Godot.Collections.Dictionary property) => DataValues.Validate(this, property);

    /// <summary>It moves itself, across and then down, and meets terrain its own way: the speed it was thrown with is its own to keep.</summary>
    protected override void OnLaunched(Room room)
    {
        DataValues.Fill(this, room.Content);
        _speed = Velocity;
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        MoveAcrossMeetingTerrain(room, _speed.X);
        bool stopped = MoveDownMeetingTerrain(room, _speed.Y);
        if (stopped && !_speed.Y.IsNegative)
        {
            if (SplashEffect.Length > 0)
                room.Spawn(Effect.Of(FrameTimeline.Read(room.Content.ReadData(SplashEffect)), Position));
            Leave(room);
            return;
        }

        _speed = _speed.WithY(_speed.Y + Pixels.FromNumber(Gravity));
    }
}
