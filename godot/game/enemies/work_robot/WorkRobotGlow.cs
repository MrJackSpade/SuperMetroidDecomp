using Engine.Content;
using Engine.Ui;
using Engine.World;

namespace Game.Enemies;

/// <summary>
/// The glow the powered Work Robots of a room share: the room has one, not one a robot. The first robot set up in a
/// room makes it. Every powered robot's setting up starts it again from its beginning, where it rests a step on its
/// last colours, and shows it on that robot from then on; an unpowered robot's setting up stops it where it stands,
/// so the order the robots are set up in decides whether it runs. Each step is a change of the colours of the robots'
/// pictures. It takes its turn with the room's objects, after the enemies have had theirs.
/// </summary>
public partial class WorkRobotGlow : RoomObject
{
    /// <summary>The name its colours are asked under on what shows them.</summary>
    private const string Source = "work_robot_glow";

    private ColourSteps? _steps;
    private bool _runs;

    private ColourSteps Steps => _steps ?? throw new InvalidOperationException("The Work Robots' glow was made with no steps.");

    /// <summary>The glow of <paramref name="room"/>: the one already there, or one made now from a robot's own steps.</summary>
    public static WorkRobotGlow In(Room room, DataNode steps)
    {
        ArgumentNullException.ThrowIfNull(room);
        ArgumentNullException.ThrowIfNull(steps);
        if (room.Objects.OfType<WorkRobotGlow>().FirstOrDefault() is { } shared)
            return shared;
        var glow = new WorkRobotGlow { Name = "WorkRobotGlow", _steps = new ColourSteps(steps, Source) };
        room.Spawn(glow);
        return glow;
    }

    /// <summary>Starts the glow again from its beginning, shown on everything that shows it and on <paramref name="visual"/> from now on.</summary>
    public void StartOn(IRecoloured visual)
    {
        ArgumentNullException.ThrowIfNull(visual);
        Steps.ShowOn(visual);
        Steps.Restart();
        _runs = true;
    }

    /// <summary>Stops the glow where it stands: what shows it keeps the colours it shows now.</summary>
    public void Stop() => _runs = false;

    /// <summary>While it runs it counts its step down, and as that runs out everything that shows it shows the next.</summary>
    protected override void OnStep()
    {
        if (_runs)
            Steps.Step();
    }
}
