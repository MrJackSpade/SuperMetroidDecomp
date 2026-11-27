using Engine.Core;
using Engine.World;
using Godot;

namespace Engine.Tests.Core;

/// <summary>The stand-in game's definition: its rooms are built here rather than loaded from scenes.</summary>
public partial class TwoRoomDefinition : StandInDefinition
{
    /// <summary>Makes the first mode.</summary>
    public Func<GameMode> First { get; init; } = () => new PlayingMode();

    /// <summary>Whether the pause screen is one that Cancel quits from, to a stand-in mode.</summary>
    public bool QuitFromPause { get; set; }

    /// <inheritdoc/>
    public override GameMode CreateFirstMode() => First();

    /// <inheritdoc/>
    public override GameMode CreatePauseMode(PlayingMode playing, PauseAsker askedBy) => new StandInPause(playing, this);

    /// <inheritdoc/>
    public override Room CreateRoom(string scenePath)
    {
        var room = new Room { Name = scenePath };
        room.AddChild(new Terrain { Name = "Terrain", LayoutFile = "room.json" });
        switch (scenePath)
        {
            case "a":
                room.AddChild(new Marker2D { Name = "Start", Position = new Vector2(40, 80) });
                room.AddChild(new RoomExit
                {
                    Name = "East", Position = new Vector2(120, 0), Size = new Vector2I(16, 80),
                    LeadsToRoom = "b", LeadsToExit = "West", ArrivalOffset = new Vector2I(-40, 80),
                });
                break;
            case "b":
                room.AddChild(new RoomExit
                {
                    Name = "West", Position = new Vector2(0, 0), Size = new Vector2I(16, 80),
                    LeadsToRoom = "a", LeadsToExit = "East", ArrivalOffset = new Vector2I(40, 80),
                });
                break;
            default:
                throw new InvalidOperationException($"The stand-in game has no room \"{scenePath}\".");
        }

        return room;
    }
}
