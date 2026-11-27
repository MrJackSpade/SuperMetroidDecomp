using Engine.Actors;
using Engine.Collision;
using Engine.Content;
using Engine.Shared;
using Engine.World;
using Godot;

namespace Game.Enemies;

/// <summary>
/// A Ki-Hunter's spit of acid: it gathers at the mouth, shoots forward, and falls in an arc. A wall stops it going
/// across; the floor splashes it.
/// </summary>
/// <remarks>
/// Its timeline's cue "launch" sets it moving. The timeline of its splash has the cues "stop", which holds it still,
/// and "end", which removes it; its data names the splash's file ("splash"), which lies in the folder its own
/// timeline is in, since each Ki-Hunter's acid is of that Ki-Hunter's colours. Its settings let it through terrain,
/// which it meets itself.
/// </remarks>
public partial class KiHunterAcid : Projectile, IHasValues
{
    /// <inheritdoc/>
    [Export(PropertyHint.File)]
    public string DataFile { get; set; } = "";

    /// <summary>How far forward it jumps as it is launched, in pixels.</summary>
    [Export, DataValue("launch_shift")]
    public int LaunchShift { get; set; } = 19;

    /// <summary>The downward speed it gains each update, as a number of pixels.</summary>
    [Export, DataValue("gravity")]
    public double Gravity { get; set; } = 0.0625;

    /// <summary>The fastest it falls, as a number of pixels an update.</summary>
    [Export, DataValue("fastest_fall")]
    public double FastestFall { get; set; } = 2;

    /// <summary>The file of the timeline of its splash on the floor, by its name within the folder its own timeline is in.</summary>
    [Export, DataValue("splash")]
    public string Splash { get; set; } = "";

    private PixelVelocity _launch;
    private bool _flying;
    private bool _splashed;

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
        // It waits at the mouth until its timeline launches it.
        _launch = Velocity;
        Velocity = PixelVelocity.Zero;
    }

    /// <inheritdoc/>
    protected override void OnCue(Room room, string cue)
    {
        switch (cue)
        {
            case "launch":
                Position += PixelOffset.FromWhole(_launch.X.IsNegative ? -LaunchShift : LaunchShift, 0);
                Velocity = _launch;
                _flying = true;
                break;
            case "stop":
                Velocity = PixelVelocity.Zero;
                break;
            case "end":
                Leave(room);
                break;
        }
    }

    /// <summary>It is moved by its own class, down and then across, each tried where it ends.</summary>
    protected override bool MovesItself => true;

    /// <inheritdoc/>
    protected override void BeforeTravel(Room room)
    {
        if (!_flying || _splashed)
            return;

        // It moves as the cartridge's enemy shots do, down first: stopped there, it splashes where that leaves it.
        if (MoveAgainstTerrain(room, Velocity.Y, across: false))
        {
            _splashed = true;
            Velocity = PixelVelocity.Zero;
            PlayTimeline($"{Settings.Timeline[..Settings.Timeline.LastIndexOf('/')]}/{Splash}");
            return;
        }

        // Then across, where a wall only ends its speed across, and it gains no fall on that update. With no speed
        // across its right side is still tried where it lies.
        if (MoveAgainstTerrain(room, Velocity.X, across: true))
        {
            Velocity = Velocity.WithX(Pixels.Zero);
            return;
        }

        Pixels faster = Velocity.Y + Pixels.FromNumber(Gravity), fastest = Pixels.FromNumber(FastestFall);
        Velocity = Velocity.WithY(faster >= fastest ? fastest : faster);
    }
}
